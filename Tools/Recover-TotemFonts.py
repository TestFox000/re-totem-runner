"""Restore static-font glyph advances lost during the Unity 3.5 export.

Reads the original APK using the installed UnityPy package. Only the three
existing Font assets' advance scalars are changed; GUIDs, glyph geometry,
texture references, line spacing, and all other settings are retained.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import re
import zipfile
from pathlib import Path

import UnityPy
import yaml


APK_SHA256 = "108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e"
SOURCE_MEMBER = "assets/bin/Data/sharedassets1.assets"


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def read_unity_yaml(path: Path) -> dict:
    text = path.read_bytes().decode("utf-8-sig")
    text = re.sub(r"(?m)^%.*\r?\n", "", text)
    text = re.sub(r"(?m)^--- !u!\d+ &-?\d+", "---", text)
    document = yaml.load(text, Loader=yaml.CSafeLoader)
    if not isinstance(document, dict) or len(document) != 1:
        raise ValueError(f"Expected a single Unity asset: {path}")
    return next(iter(document.values()))


def assert_equivalent(expected, actual, label: str) -> None:
    if isinstance(expected, dict):
        if not isinstance(actual, dict):
            raise ValueError(f"{label}: expected a mapping")
        for key, value in expected.items():
            if key not in actual:
                raise ValueError(f"{label}: missing {key}")
            assert_equivalent(value, actual[key], label + "." + key)
    elif isinstance(expected, (int, float)) and isinstance(actual, (int, float)):
        if not math.isclose(expected, actual, rel_tol=1e-6, abs_tol=1e-6):
            raise ValueError(f"{label}: {expected!r} != {actual!r}")
    elif expected != actual:
        raise ValueError(f"{label}: {expected!r} != {actual!r}")


def meta_guid(path: Path) -> str:
    match = re.search(r"(?m)^guid:\s*([0-9a-f]{32})\s*$", path.read_text(encoding="utf-8-sig"))
    if not match:
        raise ValueError(f"Missing Unity GUID: {path}")
    return match.group(1)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", type=Path, required=True)
    parser.add_argument("--project", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--apply", action="store_true", help="Apply the validated glyph-advance restoration")
    args = parser.parse_args()

    apk_bytes = args.apk.read_bytes()
    actual_apk_hash = sha256(apk_bytes)
    if actual_apk_hash != APK_SHA256:
        raise ValueError("The APK does not match the inspected original Totem Runner 1.0.1.")
    asset_root = (args.project / "Assets").resolve()
    font_root = (asset_root / "Font").resolve()
    guid_assets = {}
    for meta in asset_root.rglob("*.meta"):
        guid_assets[meta_guid(meta)] = Path(str(meta)[:-5])

    with zipfile.ZipFile(args.apk) as archive:
        source_bytes = archive.read(SOURCE_MEMBER)
    environment = UnityPy.load(source_bytes)
    objects = {obj.path_id: obj for obj in environment.objects}
    original_fonts = [obj for obj in environment.objects if obj.type.name == "Font"]
    if len(original_fonts) != 3:
        raise ValueError(f"Expected exactly three source fonts, found {len(original_fonts)}")

    plan = []
    evidence = []
    for source_object in original_fonts:
        source = source_object.read_typetree()
        destination = (font_root / (source["m_Name"] + ".asset")).resolve()
        destination.relative_to(font_root)
        original_bytes = destination.read_bytes()
        text = original_bytes.decode("utf-8-sig")
        exported = read_unity_yaml(destination)
        font_meta = Path(str(destination) + ".meta")
        original_meta = font_meta.read_bytes()
        source_glyphs = source["m_CharacterRects"]
        exported_glyphs = exported["m_CharacterRects"]
        if len(source_glyphs) != len(exported_glyphs):
            raise ValueError(f"Glyph count differs: {destination}")
        for key in ("m_Name", "m_LineSpacing", "m_Ascent", "m_FontSize", "m_ConvertCase", "m_AsciiStartOffset"):
            assert_equivalent(source[key], exported[key], source["m_Name"] + "." + key)

        glyph_evidence = []
        advances = []
        for index, (old, new) in enumerate(zip(source_glyphs, exported_glyphs)):
            for key in ("index", "uv", "vert"):
                assert_equivalent(old[key], new[key], f"{source['m_Name']}.glyph[{index}].{key}")
            width = old["width"]
            if not math.isfinite(width) or width != int(width):
                raise ValueError(f"Nonintegral original width requires explicit handling: {width}")
            advance = int(width)
            advances.append(advance)
            glyph_evidence.append({"character_index": old["index"], "original_width": width, "advance_before": new["advance"], "advance_after": advance})

        pointer_evidence = {}
        for key in ("m_Texture", "m_DefaultMaterial"):
            source_pointer = source[key]
            if source_pointer["m_FileID"] != 0:
                raise ValueError(f"Unexpected external source font pointer: {key}")
            pointed_source = objects[source_pointer["m_PathID"]].read_typetree()
            pointed_path = guid_assets[exported[key]["guid"]]
            pointed_export = read_unity_yaml(pointed_path)
            assert_equivalent(pointed_source["m_Name"], pointed_export["m_Name"], key + ".name")
            check = {"original_path_id": source_pointer["m_PathID"], "exported_guid": exported[key]["guid"], "exported_path": str(pointed_path), "name_matches": True}
            if key == "m_Texture":
                for field in ("m_Width", "m_Height", "m_TextureFormat", "m_CompleteImageSize"):
                    assert_equivalent(pointed_source[field], pointed_export[field], key + "." + field)
                raw = re.search(r"(?m)^\s*_typelessdata:\s*([0-9a-fA-F]+)\s*$", pointed_path.read_text(encoding="utf-8-sig"))
                if not raw:
                    raise ValueError(f"Missing exported font-atlas data: {pointed_path}")
                source_atlas_hash = sha256(bytes(pointed_source["image data"]))
                export_atlas_hash = sha256(bytes.fromhex(raw.group(1)))
                if source_atlas_hash != export_atlas_hash:
                    raise ValueError(f"Font-atlas bytes differ: {pointed_path}")
                check.update({"width": pointed_source["m_Width"], "height": pointed_source["m_Height"], "atlas_sha256": source_atlas_hash, "atlas_bytes_match": True})
            else:
                # Report the separate material-property loss without modifying
                # the material or inventing replacement properties here.
                check["original_texture_property_count"] = len(pointed_source["m_SavedProperties"]["m_TexEnvs"])
                check["exported_texture_property_count"] = len(pointed_export["m_SavedProperties"]["m_TexEnvs"])
            pointer_evidence[key] = check

        iterator = iter(advances)
        count = 0

        def replace_advance(match):
            nonlocal count
            count += 1
            return match.group(1) + str(next(iterator))

        replacement = re.sub(r"(?m)^(    advance:[ \t]*)[^\r\n]+", replace_advance, text)
        if count != len(advances):
            raise ValueError(f"Expected {len(advances)} advance scalars, found {count}")
        new_bytes = replacement.encode("utf-8")
        if original_bytes.startswith(b"\xef\xbb\xbf"):
            new_bytes = b"\xef\xbb\xbf" + new_bytes
        plan.append((destination, original_bytes, new_bytes, font_meta, original_meta))
        evidence.append({"source_member": SOURCE_MEMBER, "source_path_id": source_object.path_id, "font_name": source["m_Name"], "asset_path": str(destination), "guid": meta_guid(font_meta), "glyph_count": count, "changed_advances": sum(g["advance_before"] != g["advance_after"] for g in glyph_evidence), "uv_and_bearings_match": True, "line_spacing_and_ascent_match": True, "sha256_before": sha256(original_bytes), "sha256_after": sha256(new_bytes), "meta_sha256": sha256(original_meta), "references": pointer_evidence, "glyphs": glyph_evidence})

    # All three assets and their atlases have been validated before writing.
    for destination, before, after, meta, meta_before in plan:
        if destination.read_bytes() != before or meta.read_bytes() != meta_before:
            raise ValueError(f"File changed while preparing the recovery: {destination}")
    if args.apply:
        for destination, before, after, meta, meta_before in plan:
            destination.write_bytes(after)
            if destination.read_bytes() != after or meta.read_bytes() != meta_before:
                raise ValueError(f"Post-write verification failed: {destination}")

    report = {"source_apk": str(args.apk.resolve()), "source_apk_sha256": actual_apk_hash, "source_member_sha256": sha256(source_bytes), "applied": args.apply, "scope": "Only existing Font glyph advance scalars restored from original Unity 3.5 width. No Unity execution. Meta/GUIDs and all other font data retained.", "font_count": len(evidence), "glyph_count": sum(f["glyph_count"] for f in evidence), "changed_advances": sum(f["changed_advances"] for f in evidence), "fonts": evidence}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"applied": args.apply, "fonts": report["font_count"], "glyphs": report["glyph_count"], "changed_advances": report["changed_advances"], "report": str(args.report.resolve())}, ensure_ascii=False))


if __name__ == "__main__":
    main()
