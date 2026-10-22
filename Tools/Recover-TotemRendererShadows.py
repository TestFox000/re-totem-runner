"""Restore two renderer shadow flags from the original 21 Unity 3.5 scenes.

Only MeshRenderer and SkinnedMeshRenderer m_CastShadows/m_ReceiveShadows
scalar values are replaced. Scene structure, active states, references, line
endings, and meta/GUIDs remain byte-for-byte unchanged.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import zipfile
from pathlib import Path

import UnityPy


APK_SHA256 = "108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e"
TYPES = {23: "MeshRenderer", 137: "SkinnedMeshRenderer"}
FIELDS = ("m_CastShadows", "m_ReceiveShadows")
HEADERS = re.compile(rb"(?m)^--- !u!(\d+) &(-?\d+)\r?\n")


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def patch_bytes(original: bytes, replacements: list[tuple[int, int, bytes]]) -> bytes:
    pieces = []
    cursor = 0
    for start, end, value in sorted(replacements):
        if start < cursor:
            raise ValueError("Overlapping scalar replacements")
        pieces.extend((original[cursor:start], value))
        cursor = end
    pieces.append(original[cursor:])
    return b"".join(pieces)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", type=Path, required=True)
    parser.add_argument("--project", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()
    apk_hash = sha256(args.apk.read_bytes())
    if apk_hash != APK_SHA256:
        raise ValueError("The source APK does not match the inspected original.")
    project = args.project.resolve()

    plans = []
    evidence = []
    with zipfile.ZipFile(args.apk) as archive:
        main_data = UnityPy.load(archive.read("assets/bin/Data/mainData"))
        settings = next(obj.read_typetree() for obj in main_data.objects if obj.type.name == "BuildSettings")
        scene_paths = settings["levels"]
        if len(scene_paths) != 21:
            raise ValueError(f"Expected 21 original scenes, found {len(scene_paths)}")
        for index, scene_path in enumerate(scene_paths):
            source_member = "assets/bin/Data/" + ("mainData" if index == 0 else "level" + str(index - 1))
            environment = main_data if index == 0 else UnityPy.load(archive.read(source_member))
            source_flags = {}
            for obj in environment.objects:
                if obj.type.name in TYPES.values():
                    tree = obj.read_typetree()
                    source_flags[obj.path_id] = (obj.type.name, {key: int(bool(tree[key])) for key in FIELDS})

            destination = (project / scene_path).resolve()
            destination.relative_to(project / "Assets" / "Levels")
            original = destination.read_bytes()
            meta = Path(str(destination) + ".meta")
            meta_hash = sha256(meta.read_bytes())
            headers = list(HEADERS.finditer(original))
            replacements = []
            changes = []
            checked = set()
            type_counts = {typ: 0 for typ in TYPES.values()}
            source_true_counts = {field: 0 for field in FIELDS}
            for offset, header in enumerate(headers):
                class_id, path_id = (int(header.group(1)), int(header.group(2)))
                if class_id not in TYPES:
                    continue
                if path_id not in source_flags:
                    raise ValueError(f"Renderer has no original path ID: {scene_path} / {path_id}")
                source_type, flags = source_flags[path_id]
                if TYPES[class_id] != source_type:
                    raise ValueError(f"Renderer class differs: {scene_path} / {path_id}")
                checked.add(path_id)
                type_counts[source_type] += 1
                start = header.end()
                end = headers[offset + 1].start() if offset + 1 < len(headers) else len(original)
                block = original[start:end]
                for field, expected in flags.items():
                    source_true_counts[field] += expected
                    pattern = re.compile(rb"(?m)^  " + field.encode("ascii") + rb":[ \t]*([0-9]+)(?=\r?$)")
                    matches = list(pattern.finditer(block))
                    if len(matches) != 1:
                        raise ValueError(f"Expected one {field}: {scene_path} / {path_id}")
                    match = matches[0]
                    previous = int(match.group(1))
                    if previous != expected:
                        replacements.append((start + match.start(1), start + match.end(1), str(expected).encode("ascii")))
                        changes.append({"path_id": path_id, "type": source_type, "field": field, "before": previous, "after": expected})
            if checked != set(source_flags):
                raise ValueError(f"Original renderers are missing from the export: {scene_path}")
            recovered = patch_bytes(original, replacements)
            # All original flags are bools; modern casting uses Off=0/On=1.
            # No particle-renderer compatibility fields are touched.
            plans.append((destination, sha256(original), recovered, meta, meta_hash))
            evidence.append({"scene": scene_path, "source_member": source_member, "renderer_counts": type_counts, "checked_renderers": len(checked), "source_true_counts": source_true_counts, "changed_fields": len(changes), "changed_renderers": len({change["path_id"] for change in changes}), "sha256_before": sha256(original), "sha256_after": sha256(recovered), "meta_sha256": meta_hash, "changes": changes})

    # Validate every scene before making any change, then atomically replace
    # each affected scene. A concurrent scene edit causes a fail-closed stop.
    for destination, expected_hash, recovered, meta, meta_hash in plans:
        if sha256(destination.read_bytes()) != expected_hash or sha256(meta.read_bytes()) != meta_hash:
            raise ValueError(f"Scene or meta changed while preparing: {destination}")
    if args.apply:
        for destination, expected_hash, recovered, meta, meta_hash in plans:
            if sha256(destination.read_bytes()) != expected_hash:
                raise ValueError(f"Scene changed before write: {destination}")
            if sha256(recovered) != expected_hash:
                temporary = destination.with_name(destination.name + ".shadow-recovery.tmp")
                if temporary.exists():
                    raise FileExistsError(temporary)
                with temporary.open("xb") as output:
                    output.write(recovered)
                    output.flush()
                    os.fsync(output.fileno())
                os.replace(temporary, destination)
            if destination.read_bytes() != recovered or sha256(meta.read_bytes()) != meta_hash:
                raise ValueError(f"Post-write verification failed: {destination}")

    report = {"source_apk": str(args.apk.resolve()), "source_apk_sha256": apk_hash, "applied": args.apply, "scope": "Only MeshRenderer/SkinnedMeshRenderer m_CastShadows and m_ReceiveShadows values in the original 21 scenes; bool casting maps to ShadowCastingMode Off=0/On=1. No Unity execution. All other bytes and meta/GUIDs retained.", "scene_count": len(evidence), "checked_renderers": sum(item["checked_renderers"] for item in evidence), "changed_renderers": sum(item["changed_renderers"] for item in evidence), "changed_fields": sum(item["changed_fields"] for item in evidence), "scenes": evidence}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({key: report[key] for key in ("applied", "scene_count", "checked_renderers", "changed_renderers", "changed_fields")}) + " report=" + str(args.report.resolve()))


if __name__ == "__main__":
    main()
