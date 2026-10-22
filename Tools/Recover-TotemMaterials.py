"""Restore original material property sheets without changing material/shader GUIDs.

Identity uses exact scene renderer path IDs and material slots, original Font
pointers, and unique names after those anchors. Texture pointers use exact RGBA
pixel fingerprints, unique matching names, and one-to-one candidate elimination.
Only material property sheets/queue and explicitly authorized texture wrapV/W
fields are modified. Requires the existing UnityPy/Pillow/PyYAML installation.
"""
from __future__ import annotations

import argparse
import collections
import hashlib
import json
import math
import re
import struct
from pathlib import Path

import UnityPy
import yaml
from PIL import Image


APK_SHA256 = "108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e"
GUID = re.compile(r"(?m)^guid: ([0-9a-f]{32})\s*$")
NAME = re.compile(r"(?m)^  m_Name: (.*)$")
DOC = re.compile(r"(?m)^--- !u!(\d+) &(-?\d+)(?: stripped)?\n")
REF = re.compile(r"\{fileID: (-?\d+)(?:, guid: ([0-9a-f]{32}), type: (\d+))?\}")
SAVED = re.compile(r"(?ms)^  m_SavedProperties:\n.*?(?=^  m_BuildTextureStacks:|\Z)")
QUEUE = re.compile(r"(?m)^  m_CustomRenderQueue: (-?\d+)$")


def sha(data):
    return hashlib.sha256(data).hexdigest()


def key(obj):
    return (obj.assets_file.name, obj.path_id)


def identify(path):
    text = path.read_text(encoding="utf-8-sig")
    meta_path = Path(str(path) + ".meta")
    meta = meta_path.read_bytes()
    guid = GUID.search(meta.decode("utf-8-sig")).group(1)
    match = NAME.search(text)
    name = yaml.safe_load(match.group(1)) if match else path.stem
    return {"path": path, "guid": guid, "name": str(name), "text": text, "meta_sha256": sha(meta)}


def add_identity(mapping, reverse, evidence, source, guid, reason):
    if source in mapping and mapping[source] != guid:
        raise ValueError(f"Conflicting source identity {source}: {mapping[source]} vs {guid}")
    if guid in reverse and reverse[guid] != source:
        raise ValueError(f"Conflicting exported identity {guid}: {reverse[guid]} vs {source}")
    if source not in mapping:
        mapping[source] = guid
        reverse[guid] = source
        evidence[source] = reason


def map_materials(env, project, source, exported):
    mapping, reverse, evidence = {}, {}, {}
    assets = {asset.name: asset for asset in env.assets}
    levels = next(obj for obj in assets["mainData"].objects.values() if obj.type.name == "BuildSettings").read_typetree()["levels"]
    for index, level in enumerate(levels):
        scene = (project / level).read_text(encoding="utf-8-sig")
        headers = list(DOC.finditer(scene))
        docs = {int(header.group(2)): scene[header.end():headers[i + 1].start() if i + 1 < len(headers) else len(scene)] for i, header in enumerate(headers)}
        asset = assets["mainData" if index == 0 else f"level{index - 1}"]
        for obj in asset.objects.values():
            if not obj.type.name.endswith("Renderer") or obj.path_id not in docs:
                continue
            pointers = getattr(obj.read(), "m_Materials", [])
            block = re.search(r"(?ms)^  m_Materials:\n(.*?)(?=^  \w|\Z)", docs[obj.path_id])
            if not block:
                continue
            refs = REF.findall(block.group(1))
            if len(pointers) != len(refs):
                raise ValueError(f"Renderer slot count differs: {level}:{obj.path_id}")
            for slot, (pointer, reference) in enumerate(zip(pointers, refs)):
                if not pointer.m_PathID or reference[1] not in exported:
                    continue
                original = pointer.deref()
                if key(original) in source:
                    add_identity(mapping, reverse, evidence, key(original), reference[1], {"kind": "renderer pathID and material slot", "scene": level, "renderer_path_id": obj.path_id, "slot": slot})
    renderer_count = len(mapping)
    fonts = [identify(path) for path in (project / "Assets/Font").glob("*.asset")]
    texture_anchors = {}
    for obj in env.objects:
        if obj.type.name != "Font" or obj.assets_file.name == "unity default resources":
            continue
        font = obj.read()
        candidates = [item for item in fonts if item["name"] == font.m_Name]
        if len(candidates) != 1:
            raise ValueError(f"Font identity is not unique: {font.m_Name}")
        exported_font = candidates[0]
        material_ref = REF.search(re.search(r"(?m)^  m_DefaultMaterial: (.*)$", exported_font["text"]).group(1))
        texture_ref = REF.search(re.search(r"(?m)^  m_Texture: (.*)$", exported_font["text"]).group(1))
        material_obj = font.m_DefaultMaterial.deref()
        texture_obj = font.m_Texture.deref()
        add_identity(mapping, reverse, evidence, key(material_obj), material_ref.group(2), {"kind": "unique Font name and original m_DefaultMaterial pointer", "font": font.m_Name, "font_path_id": obj.path_id})
        texture_anchors[key(texture_obj)] = texture_ref.group(2)
    source_names = collections.defaultdict(list)
    exported_names = collections.defaultdict(list)
    for source_key, obj in source.items():
        source_names[obj.read().m_Name].append(source_key)
    for guid, item in exported.items():
        exported_names[item["name"]].append(guid)
    for name, source_keys in source_names.items():
        remaining_source = [source_key for source_key in source_keys if source_key not in mapping]
        remaining_exported = [guid for guid in exported_names[name] if guid not in reverse]
        if len(remaining_source) == len(remaining_exported) == 1:
            add_identity(mapping, reverse, evidence, remaining_source[0], remaining_exported[0], {"kind": "unique remaining exact material name after pointer anchors", "name": name})
    if len(mapping) != len(source) or len(reverse) != len(exported):
        raise ValueError(f"Material identities unresolved: {[(source_key, source[source_key].read().m_Name) for source_key in source if source_key not in mapping]}")
    return mapping, evidence, texture_anchors, renderer_count


def pixel_fingerprint(image):
    rgba = image.convert("RGBA")
    return (rgba.size, sha(rgba.tobytes()))


def sampler_matches(texture, importer):
    original = texture.m_TextureSettings
    target = importer["textureSettings"]
    return (
        int(original.m_FilterMode) == int(target["filterMode"])
        and int(original.m_Aniso) == int(target["aniso"])
        and float(original.m_MipBias) == float(target["mipBias"])
        # AssetRipper filled only wrapU from the old single m_WrapMode field.
        # Missing modern wrapV/W defaults are reported, never silently rewritten here.
        and int(original.m_WrapMode) == int(target["wrapU"])
        and bool(texture.m_MipMap) == bool(importer["mipmaps"]["enableMipMap"])
        and bool(texture.m_IsReadable) == bool(importer["isReadable"])
        and int(texture.m_TextureFormat) == int(importer["textureFormat"])
        and bool(texture.m_ColorSpace) == bool(importer["mipmaps"]["sRGBTexture"])
    )


def map_textures(env, project, anchors):
    original = {key(obj): obj for obj in env.objects if obj.type.name == "Texture2D" and obj.assets_file.name != "unity default resources"}
    exported = {}
    pixels = collections.defaultdict(list)
    for path in (project / "Assets/Texture2D").iterdir():
        if path.suffix not in (".png", ".texture2D"):
            continue
        item = identify(path) if path.suffix != ".png" else {"path": path, "name": path.stem, "guid": GUID.search(Path(str(path) + ".meta").read_text(encoding="utf-8-sig")).group(1)}
        item["reference_type"] = 3 if path.suffix == ".png" else 2
        if path.suffix == ".png":
            item["importer"] = yaml.safe_load(Path(str(path) + ".meta").read_text(encoding="utf-8-sig"))["TextureImporter"]
        exported[item["guid"]] = item
        if path.suffix == ".png":
            with Image.open(path) as image:
                item["fingerprint"] = pixel_fingerprint(image)
            pixels[item["fingerprint"]].append(item["guid"])
    if len(original) != len(exported):
        raise ValueError(f"Original/exported external texture counts differ: {len(original)} vs {len(exported)}")
    mapping, reverse, evidence = {}, {}, {}
    candidates = {}
    raw_groups = collections.defaultdict(list)
    fingerprints = {}
    for source_key, guid in anchors.items():
        add_identity(mapping, reverse, evidence, source_key, guid, {"kind": "unique Font name and original m_Texture pointer"})
        texture = original[source_key].read()
        target = exported[guid]["path"].read_text(encoding="utf-8-sig")
        image_data = bytes.fromhex(re.search(r"(?m)^  _typelessdata: ([0-9a-fA-F]+)$", target).group(1))
        if image_data != bytes(texture.image_data):
            raise ValueError(f"Native font texture pixel bytes differ: {source_key}")
    for source_key, obj in original.items():
        if source_key in mapping:
            continue
        texture = obj.read()
        raw_groups[sha(obj.get_raw_data())].append(source_key)
        fingerprint = pixel_fingerprint(texture.image)
        fingerprints[source_key] = fingerprint
        choices = [guid for guid in pixels[fingerprint] if sampler_matches(texture, exported[guid]["importer"])]
        if not choices:
            raise ValueError(f"No exact original RGBA fingerprint for {source_key}:{texture.m_Name}")
        candidates[source_key] = list(choices)
    for raw_sha256, group in raw_groups.items():
        if len(group) != 1:
            continue
        source_key = group[0]
        choices = candidates[source_key]
        same_name = [guid for guid in choices if exported[guid]["name"] == original[source_key].read().m_Name]
        if len(choices) == 1:
            add_identity(mapping, reverse, evidence, source_key, choices[0], {"kind": "unique exact RGBA fingerprint and matching original sampler/format"})
        elif len(same_name) == 1:
            add_identity(mapping, reverse, evidence, source_key, same_name[0], {"kind": "exact RGBA fingerprint, sampler/format and unique exact name"})
    while True:
        progress = False
        for raw_sha256, group in raw_groups.items():
            if all(source_key in mapping for source_key in group):
                continue
            if any(source_key in mapping for source_key in group):
                raise ValueError(f"Partially mapped equivalent source group {raw_sha256}")
            choices = candidates[group[0]]
            remaining = [guid for guid in choices if guid not in reverse]
            if len(group) == 1 and len(remaining) == 1:
                add_identity(mapping, reverse, evidence, group[0], remaining[0], {"kind": "unique remaining exact RGBA fingerprint/sampler candidate after other texture identities anchored"})
                progress = True
            elif len(group) > 1 and len(remaining) == len(group):
                importer_settings = [exported[guid]["importer"] for guid in remaining]
                if any(settings != importer_settings[0] for settings in importer_settings[1:]):
                    raise ValueError(f"Equivalent source textures have different exported importer settings: {group}")
                payload_hashes = {sha(exported[guid]["path"].read_bytes()) for guid in remaining}
                if len(payload_hashes) != 1:
                    raise ValueError(f"Equivalent texture exports have different PNG bytes: {group}")
                for source_key, guid in zip(sorted(group), sorted(remaining, key=lambda guid: exported[guid]["path"].name)):
                    add_identity(mapping, reverse, evidence, source_key, guid, {"kind": "equivalent_duplicate_mapping", "original_complete_serialized_object_sha256": raw_sha256, "equivalent_source_objects": [list(item) for item in sorted(group)], "export_png_sha256": next(iter(payload_hashes)), "all_export_importer_settings_identical": True, "pairing": "stable source file/pathID to exported filename; equivalent content, not claimed unique original identity"})
                progress = True
        if not progress:
            break
    if len(mapping) != len(original):
        raise ValueError(f"Ambiguous texture identities: {[(source_key, original[source_key].read().m_Name, choices) for source_key, choices in candidates.items() if source_key not in mapping]}")
    return mapping, evidence, exported, fingerprints


def property_name(name):
    name = name.name if hasattr(name, "name") else str(name)
    return name if re.fullmatch(r"[A-Za-z_]\w*", name) else json.dumps(name)


def number(value):
    value = float(value)
    if not math.isfinite(value):
        raise ValueError(f"Unexpected non-finite original material value {value}")
    return repr(value)


def property_sheet(material, texture_map, textures):
    props = material.m_SavedProperties
    lines = ["  m_SavedProperties:", "    serializedVersion: 3"]
    texture_refs = []
    tex_envs = props.m_TexEnvs or []
    lines.append("    m_TexEnvs:" + ("" if tex_envs else " {}"))
    for name, texture in tex_envs:
        if texture.m_Texture.m_PathID:
            target = texture.m_Texture.deref()
            source_key = key(target)
            guid = texture_map[source_key]
            item = textures[guid]
            pointer = f"{{fileID: 2800000, guid: {guid}, type: {item['reference_type']}}}"
            texture_refs.append({"property": property_name(name), "source_file": source_key[0], "source_path_id": source_key[1], "guid": guid, "exported_asset": str(item["path"]), "type": item["reference_type"]})
        else:
            pointer = "{fileID: 0}"
        lines.extend([f"    - {property_name(name)}:", f"        m_Texture: {pointer}", f"        m_Scale: {{x: {number(texture.m_Scale.x)}, y: {number(texture.m_Scale.y)}}}", f"        m_Offset: {{x: {number(texture.m_Offset.x)}, y: {number(texture.m_Offset.y)}}}"])
    ints = props.m_Ints or []
    lines.append("    m_Ints:" + ("" if ints else " {}"))
    for name, value in ints:
        lines.append(f"    - {property_name(name)}: {int(value)}")
    floats = props.m_Floats or []
    lines.append("    m_Floats:" + ("" if floats else " {}"))
    for name, value in floats:
        lines.append(f"    - {property_name(name)}: {number(value)}")
    colors = props.m_Colors or []
    lines.append("    m_Colors:" + ("" if colors else " {}"))
    for name, color in colors:
        lines.append(f"    - {property_name(name)}: {{r: {number(color.r)}, g: {number(color.g)}, b: {number(color.b)}, a: {number(color.a)}}}")
    return "\n".join(lines) + "\n", texture_refs


def verify_original_values(material, parsed):
    """Check parsed YAML numbers against the actual original float32 values."""
    props = material.m_SavedProperties
    for field in ("m_Floats", "m_Colors", "m_TexEnvs"):
        original = getattr(props, field) or []
        actual = parsed[field]
        for (name, value), restored in zip(original, actual):
            raw_name = name.name if hasattr(name, "name") else str(name)
            target = restored[raw_name]
            if field == "m_Floats":
                pairs = [(value, target)]
            elif field == "m_Colors":
                pairs = [(getattr(value, axis), target[axis]) for axis in ("r", "g", "b", "a")]
            else:
                pairs = [(getattr(getattr(value, attr), axis), target[label][axis]) for attr, label in (("m_Scale", "m_Scale"), ("m_Offset", "m_Offset")) for axis in ("x", "y")]
            if any(struct.pack("<f", float(source)) != struct.pack("<f", float(output)) for source, output in pairs):
                raise ValueError(f"Original float32 payload differs: {material.m_Name}:{raw_name}")


def restore_texture_wrap(env, mapping, exported, report_path):
    original = {key(obj): obj for obj in env.objects if obj.type.name == "Texture2D" and obj.assets_file.name != "unity default resources"}
    changes = []
    all_verified = []
    for source_key, guid in mapping.items():
        item = exported[guid]
        path = item["path"] if item["path"].suffix == ".texture2D" else Path(str(item["path"]) + ".meta")
        path = path.resolve(strict=True)
        path.relative_to(item["path"].parents[2] / "Assets/Texture2D")
        source_mode = int(original[source_key].read().m_TextureSettings.m_WrapMode)
        prefix = b"m_Wrap" if item["path"].suffix == ".texture2D" else b"wrap"
        before = path.read_bytes()
        patterns = {axis: re.compile(rb"(?m)^([ \t]*" + prefix + axis.encode() + rb": )(-?\d+)(?=\r?$)") for axis in "UVW"}
        values = {}
        for axis, pattern in patterns.items():
            matches = list(pattern.finditer(before))
            if len(matches) != 1:
                raise ValueError(f"Expected exactly one wrap{axis} in {path}")
            values[axis] = int(matches[0].group(2))
        if values["U"] != source_mode:
            raise ValueError(f"Exported wrapU differs from original WrapMode: {path}")
        after = before
        for axis in "VW":
            after = patterns[axis].sub(lambda match: match.group(1) + str(source_mode).encode(), after)
        masked_before, masked_after = before, after
        for axis in "VW":
            masked_before = patterns[axis].sub(lambda match: match.group(1) + b"<allowed>", masked_before)
            masked_after = patterns[axis].sub(lambda match: match.group(1) + b"<allowed>", masked_after)
        if masked_before != masked_after:
            raise ValueError(f"Non-wrapV/W bytes changed: {path}")
        detail = {"source_file": source_key[0], "source_path_id": source_key[1], "guid": guid, "path": str(path), "original_single_wrap_mode": source_mode, "before": values, "after": {axis: source_mode for axis in "UVW"}, "guid_and_all_other_bytes_unchanged": True, "before_sha256": sha(before), "after_sha256": sha(after)}
        if before != after:
            path.write_bytes(after)
            changes.append(detail)
        if path.read_bytes() != after:
            raise ValueError(f"Texture wrap on-disk validation failed: {path}")
        all_verified.append(detail)
    report = {"source_apk_sha256": APK_SHA256, "verified_textures": len(all_verified), "changed_files": len(changes), "changed_axes": sum(item["before"][axis] != item["original_single_wrap_mode"] for item in changes for axis in "VW"), "policy": "Original Unity 3.5 m_WrapMode applies to all axes; retain exact original wrapU and fill modern wrapV/W with the same original value", "only_wrap_v_w_changed": True, "all_guid_and_other_bytes_unchanged": True, "changes": changes, "verified_textures_detail": all_verified}
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return {name: report[name] for name in ("verified_textures", "changed_files", "changed_axes", "only_wrap_v_w_changed")}


def main():
    workspace = Path(__file__).resolve().parent.parent
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", type=Path, default=workspace.parent / "com.chillingo.totemrunner.android.rowgplay_1.0.1.apk")
    parser.add_argument("--project", type=Path, default=workspace / "TotemRunner")
    parser.add_argument("--report", type=Path, default=workspace / "Analysis/TotemRunner/material-recovery.json")
    args = parser.parse_args()
    apk = args.apk.resolve(strict=True)
    if sha(apk.read_bytes()) != APK_SHA256:
        raise ValueError("Input differs from verified original APK")
    project = args.project.resolve(strict=True)
    material_root = (project / "Assets/Material").resolve(strict=True)
    env = UnityPy.load(str(apk))
    source = {key(obj): obj for obj in env.objects if obj.type.name == "Material" and obj.assets_file.name != "unity default resources"}
    exported = {item["guid"]: item for item in (identify(path) for path in material_root.glob("*.mat"))}
    if len(source) != 694 or len(exported) != 694:
        raise ValueError(f"Expected 694 external materials: {len(source)} source, {len(exported)} export")
    materials, material_evidence, font_textures, renderer_count = map_materials(env, project, source, exported)
    textures, texture_evidence, exported_textures, pixel_fingerprints = map_textures(env, project, font_textures)
    plan = []
    original_counts = collections.Counter()
    empty_counts = collections.Counter()
    queue_field_counts = collections.Counter()
    for source_key, guid in materials.items():
        obj = source[source_key]
        data = obj.read()
        original_tree = obj.read_typetree()
        item = exported[guid]
        path = item["path"].resolve(strict=True)
        path.relative_to(material_root)
        before_bytes = path.read_bytes()
        before = before_bytes.decode("utf-8-sig").replace("\r\n", "\n")
        old_sheet = SAVED.search(before)
        if not old_sheet or len(SAVED.findall(before)) != 1:
            raise ValueError(f"Missing/unexpected property sheet in {path}")
        sheet, texture_refs = property_sheet(data, textures, exported_textures)
        old_props = yaml.safe_load(old_sheet.group(0))["m_SavedProperties"]
        new_props = yaml.safe_load(sheet)["m_SavedProperties"]
        for field in ("m_TexEnvs", "m_Ints", "m_Floats", "m_Colors"):
            original_counts[field] += len(getattr(data.m_SavedProperties, field) or [])
            if not old_props.get(field):
                empty_counts[field] += 1
            if len(new_props[field]) != len(getattr(data.m_SavedProperties, field) or []):
                raise ValueError(f"Property count validation failed for {path}:{field}")
        verify_original_values(data, new_props)
        after = SAVED.sub(lambda _: sheet, before)
        queue_match = QUEUE.search(before)
        if not queue_match:
            raise ValueError(f"Missing modern render queue field: {path}")
        queue_present = "m_CustomRenderQueue" in original_tree
        queue_value = int(original_tree["m_CustomRenderQueue"]) if queue_present else -1
        queue_field_counts["present" if queue_present else "absent"] += 1
        after = QUEUE.sub(f"  m_CustomRenderQueue: {queue_value}", after)
        # No other material field, including m_Shader, may be rewritten.
        if QUEUE.sub("  m_CustomRenderQueue: <allowed>", SAVED.sub("<allowed property sheet>\n", before)) != QUEUE.sub("  m_CustomRenderQueue: <allowed>", SAVED.sub("<allowed property sheet>\n", after)):
            raise ValueError(f"Unexpected material field changed: {path}")
        newline = "\r\n" if b"\r\n" in before_bytes else "\n"
        after_bytes = after.replace("\n", newline).encode("utf-8")
        if before_bytes.startswith(b"\xef\xbb\xbf"):
            after_bytes = b"\xef\xbb\xbf" + after_bytes
        detail = {"source_file": source_key[0], "source_path_id": source_key[1], "source_name": data.m_Name, "guid": guid, "asset": str(path.relative_to(project)).replace("\\", "/"), "identity_evidence": material_evidence[source_key], "original_property_counts": {field: len(getattr(data.m_SavedProperties, field) or []) for field in ("m_TexEnvs", "m_Ints", "m_Floats", "m_Colors")}, "texture_references": texture_refs, "original_render_queue_field_present": queue_present, "old_render_queue": int(queue_match.group(1)), "restored_render_queue": queue_value, "original_typetree_top_level_keys": list(original_tree), "shader_reference_unchanged": True, "guid_and_meta_unchanged": True, "meta_sha256": item["meta_sha256"], "before_sha256": sha(before_bytes), "after_sha256": sha(after_bytes)}
        plan.append((path, before_bytes, after_bytes, detail))
    print(f"Validated {len(plan)} material identities, {len(textures)} texture identities and all original property sheets.", flush=True)
    for path, before, _, _ in plan:
        if path.read_bytes() != before:
            raise RuntimeError(f"Material changed concurrently: {path}")
    for path, before, after, detail in plan:
        if before != after:
            path.write_bytes(after)
        if path.read_bytes() != after or sha(Path(str(path) + ".meta").read_bytes()) != detail["meta_sha256"]:
            raise RuntimeError(f"Written material or unchanged GUID validation failed: {path}")
    report_path = args.report.resolve()
    report_path.parent.mkdir(parents=True, exist_ok=True)
    wrap_summary = restore_texture_wrap(env, textures, exported_textures, report_path.with_name("texture-wrap-recovery.json"))
    baseline_empty = collections.Counter()
    baseline = workspace / "Recovery/TotemRunner-Unity2021/ExportedProject/Assets/Material"
    for path in baseline.glob("*.mat"):
        text = path.read_text(encoding="utf-8-sig")
        for field in ("m_TexEnvs", "m_Floats", "m_Colors"):
            if re.search(r"(?m)^    " + field + r": \{\}$", text):
                baseline_empty[field] += 1
    report = {"source_apk": str(apk), "source_sha256": APK_SHA256, "unitypy_version": UnityPy.__version__, "materials": len(plan), "renderer_pointer_anchored_materials": renderer_count, "material_identity_methods": dict(collections.Counter(item[3]["identity_evidence"]["kind"] for item in plan)), "baseline_export_empty_property_sheet_counts": dict(baseline_empty), "before_empty_property_sheet_counts": dict(empty_counts), "restored_property_counts": dict(original_counts), "mapped_external_textures": len(textures), "texture_identity_methods": dict(collections.Counter(item["kind"] for item in texture_evidence.values())), "texture_wrap_recovery": wrap_summary, "original_custom_render_queue_field_counts": dict(queue_field_counts), "queue_policy": "Unity 3.5 original material has no override field: modern -1 inherits unchanged Shader queue", "all_shader_and_material_guids_unchanged": True, "all_original_property_counts_verified": True, "all_original_float32_value_bits_verified": True, "scope": "Material m_SavedProperties/m_CustomRenderQueue and specifically authorized Texture2D wrapV/W only; no scene, shader or GUID changes", "unity_executed": False, "per_material": [item[3] for item in plan], "texture_mapping": [{"source_file": source_key[0], "source_path_id": source_key[1], "source_name": next_obj.read().m_Name, "guid": textures[source_key], "asset": str(exported_textures[textures[source_key]]["path"].relative_to(project)).replace("\\", "/"), "evidence": texture_evidence[source_key], "rgba_fingerprint": pixel_fingerprints.get(source_key)} for source_key, next_obj in {key(obj): obj for obj in env.objects if obj.type.name == "Texture2D" and obj.assets_file.name != "unity default resources"}.items()]}
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({field: report[field] for field in ("materials", "renderer_pointer_anchored_materials", "restored_property_counts", "mapped_external_textures", "original_custom_render_queue_field_counts")}), flush=True)


if __name__ == "__main__":
    main()
