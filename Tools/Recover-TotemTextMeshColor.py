"""Give migrated Unity 3.5 TextMesh vertices their neutral white color.

The original TextMesh records have no color field: their tint comes from the
renderer material. The exported modern TextMesh records incorrectly contain
rgba=0, which multiplies that material by transparent black. Only this newly
introduced vertex-color value is changed, after checking the original record.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import os
import re
import zipfile
from pathlib import Path

import UnityPy
import yaml


APK_SHA256 = "108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e"
HEADERS = re.compile(rb"(?m)^--- !u!(\d+) &(-?\d+)\r?\n")
COLOR = re.compile(rb"(?m)^  m_Color:\r?\n    serializedVersion: 2\r?\n    rgba: ([0-9]+)(?=\r?$)")
WHITE_RGBA = 4294967295
METRICS = ("m_Text", "m_OffsetZ", "m_CharacterSize", "m_LineSpacing", "m_Anchor", "m_Alignment", "m_TabSize", "m_FontSize", "m_FontStyle")


class UnityLoader(yaml.CSafeLoader):
    def construct_mapping(self, node, deep=False):
        result = super().construct_mapping(node, deep=deep)
        for key, value in node.value:
            # Unity knows m_Text is a string even when its YAML scalar looks
            # numeric (for example a score or version number).
            if key.value == "m_Text" and isinstance(value, yaml.ScalarNode):
                result[key.value] = value.value
        return result


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def check_metrics(source: dict, exported: dict, label: str) -> None:
    if "m_Color" in source:
        raise ValueError(f"The original has an explicit vertex color: {label}")
    for key in METRICS:
        original, current = source[key], exported[key]
        if isinstance(original, (float, int)) and isinstance(current, (float, int)):
            equal = math.isclose(original, current, rel_tol=1e-6, abs_tol=1e-6)
        else:
            equal = original == current
        if not equal:
            raise ValueError(f"Unexpected original/export difference at {label}.{key}")


def text_blocks(data: bytes):
    headers = list(HEADERS.finditer(data))
    for i, header in enumerate(headers):
        if int(header.group(1)) != 102:
            continue
        start = header.end()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(data)
        block = data[start:end]
        text = next(iter(yaml.load(block.decode("utf-8"), Loader=UnityLoader).values()))
        yield int(header.group(2)), start, block, text


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", type=Path, required=True)
    parser.add_argument("--project", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()
    if sha256(args.apk.read_bytes()) != APK_SHA256:
        raise ValueError("The source APK is not the inspected original.")
    project = args.project.resolve()
    plans = []
    records = []

    def prepare(destination: Path, originals: dict, member: str, match_by_text: bool = False) -> None:
        destination = destination.resolve()
        destination.relative_to(project / "Assets")
        before = destination.read_bytes()
        meta = Path(str(destination) + ".meta")
        meta_hash = sha256(meta.read_bytes())
        replacements = []
        matches = []
        checked = set()
        for path_id, start, block, exported in text_blocks(before):
            if match_by_text:
                candidates = [(ident, source) for ident, source in originals.items() if source["m_Text"] == exported["m_Text"]]
                if len(candidates) != 1:
                    raise ValueError(f"Prefab text does not identify one original record: {destination}/{path_id}")
                source_id, original = candidates[0]
            else:
                source_id = path_id
                original = originals[path_id]
            check_metrics(original, exported, f"{destination.name}/{path_id}")
            checked.add(source_id)
            colors = list(COLOR.finditer(block))
            if len(colors) != 1:
                raise ValueError(f"Expected one modern packed vertex color: {destination}/{path_id}")
            color = colors[0]
            previous = int(color.group(1))
            if previous not in (0, WHITE_RGBA):
                raise ValueError(f"An explicit modern tint needs review: {destination}/{path_id} = {previous}")
            if previous != WHITE_RGBA:
                replacements.append((start + color.start(1), start + color.end(1), str(WHITE_RGBA).encode("ascii")))
            matches.append({"exported_path_id": path_id, "source_path_id": source_id, "text": original["m_Text"], "source_has_color_field": False, "rgba_before": previous, "rgba_after": WHITE_RGBA, "text_and_metrics_match": True})
        if checked != set(originals):
            raise ValueError(f"Original/export TextMesh records differ: {destination}")
        if not matches:
            return
        pieces = []
        cursor = 0
        for start, end, value in sorted(replacements):
            pieces.extend((before[cursor:start], value))
            cursor = end
        pieces.append(before[cursor:])
        after = b"".join(pieces)
        plans.append((destination, sha256(before), after, meta, meta_hash))
        records.append({"asset": str(destination.relative_to(project)), "source_member": member, "textmesh_count": len(matches), "changed_colors": len(replacements), "sha256_before": sha256(before), "sha256_after": sha256(after), "meta_sha256": meta_hash, "textmeshes": matches})

    with zipfile.ZipFile(args.apk) as archive:
        main = UnityPy.load(archive.read("assets/bin/Data/mainData"))
        levels = next(obj.read_typetree()["levels"] for obj in main.objects if obj.type.name == "BuildSettings")
        if len(levels) != 21:
            raise ValueError("The original 21-scene set is required.")
        for index, path in enumerate(levels):
            member = "assets/bin/Data/" + ("mainData" if index == 0 else "level" + str(index - 1))
            environment = main if index == 0 else UnityPy.load(archive.read(member))
            originals = {obj.path_id: obj.read_typetree() for obj in environment.objects if obj.type.name == "TextMesh"}
            prepare(project / path, originals, member)

        # AssetRipper assigns new local IDs to the achievement prefab. Its
        # three unique strings and original metrics identify the source records.
        member = "assets/bin/Data/sharedassets1.assets"
        shared = UnityPy.load(archive.read(member))
        prefab_originals = {obj.path_id: obj.read_typetree() for obj in shared.objects if obj.type.name == "TextMesh"}
        if len(prefab_originals) != 3:
            raise ValueError("Expected three original achievement-prefab TextMesh records.")
        prepare(project / "Assets/GameObject/AchLinePrefab.prefab", prefab_originals, member, match_by_text=True)

    for destination, expected_hash, after, meta, meta_hash in plans:
        if sha256(destination.read_bytes()) != expected_hash or sha256(meta.read_bytes()) != meta_hash:
            raise ValueError(f"A scene/prefab changed while preparing: {destination}")
    if args.apply:
        for destination, expected_hash, after, meta, meta_hash in plans:
            if sha256(destination.read_bytes()) != expected_hash:
                raise ValueError(f"Concurrent scene/prefab edit: {destination}")
            if sha256(after) != expected_hash:
                temporary = destination.with_name(destination.name + ".textcolor-recovery.tmp")
                if temporary.exists():
                    raise FileExistsError(temporary)
                with temporary.open("xb") as output:
                    output.write(after)
                    output.flush()
                    os.fsync(output.fileno())
                os.replace(temporary, destination)
            if destination.read_bytes() != after or sha256(meta.read_bytes()) != meta_hash:
                raise ValueError(f"Post-write verification failed: {destination}")

    report = {"source_apk": str(args.apk.resolve()), "source_apk_sha256": APK_SHA256, "applied": args.apply, "scope": "Only modern TextMesh m_Color.rgba: transparent black to neutral white, for original Unity 3.5 records with no vertex-color field. Original material tint/alpha, text, glyph/font references, all metrics, projection and all other asset bytes are retained; no shader changes or Unity execution.", "modern_color_reference": "https://docs.unity3d.com/2021.3/Documentation/ScriptReference/TextMesh-color.html", "original_component_reference": "https://docs.unity3d.com/352/Documentation/Components/class-TextMesh.html", "file_count": len(records), "textmesh_count": sum(r["textmesh_count"] for r in records), "changed_colors": sum(r["changed_colors"] for r in records), "files": records}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({key: report[key] for key in ("applied", "file_count", "textmesh_count", "changed_colors")}) + " report=" + str(args.report.resolve()))


if __name__ == "__main__":
    main()
