"""Restore each exported scene GameObject's original active flag by exact path ID.

Only the single m_IsActive digit in GameObject YAML documents is changed.
Scene ordering is read from BuildSettings inside the verified original APK.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import zipfile
from pathlib import Path

import UnityPy


EXPECTED_SHA256 = "108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e"
DOCUMENT = re.compile(rb"(?m)^--- !u!(\d+) &(-?\d+)(?: stripped)?\r?\n")
ACTIVE = re.compile(rb"(?m)^  m_IsActive: ([01])(?=\r?$)")


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def scene_data(raw: bytes) -> dict[int, dict]:
    env = UnityPy.load(raw)
    result = {}
    for obj in env.objects:
        if obj.type.name != "GameObject":
            continue
        game_object = obj.read()
        if obj.path_id in result:
            raise ValueError(f"Duplicate original path ID {obj.path_id}")
        value = int(game_object.m_IsActive)
        if value not in (0, 1):
            raise ValueError(f"Invalid original active state {value} on {obj.path_id}")
        result[obj.path_id] = {"active": value, "name": game_object.m_Name}
    return result


def exported_flags(data: bytes) -> dict[int, dict]:
    documents = list(DOCUMENT.finditer(data))
    result = {}
    for index, header in enumerate(documents):
        if int(header.group(1)) != 1:
            continue
        file_id = int(header.group(2))
        end = documents[index + 1].start() if index + 1 < len(documents) else len(data)
        block = data[header.end():end]
        flags = list(ACTIVE.finditer(block))
        if len(flags) != 1:
            raise ValueError(f"Expected one GameObject active flag for file ID {file_id}, found {len(flags)}")
        if file_id in result:
            raise ValueError(f"Duplicate exported GameObject file ID {file_id}")
        flag = flags[0]
        result[file_id] = {"active": int(flag.group(1)), "offset": header.end() + flag.start(1)}
    return result


def prepare_scene(path: Path, original: dict[int, dict]) -> tuple[bytes, bytes, dict]:
    before = path.read_bytes()
    exported = exported_flags(before)
    if exported.keys() != original.keys():
        missing = sorted(original.keys() - exported.keys())
        extra = sorted(exported.keys() - original.keys())
        raise ValueError(f"GameObject path ID mismatch in {path}: missing={missing[:20]}, extra={extra[:20]}")
    after = bytearray(before)
    changed = []
    for file_id, exported_state in exported.items():
        state = original[file_id]["active"]
        if exported_state["active"] != state:
            after[exported_state["offset"]] = ord(str(state))
            changed.append(file_id)
    after = bytes(after)
    verified = exported_flags(after)
    if any(verified[file_id]["active"] != state["active"] for file_id, state in original.items()):
        raise ValueError(f"Restored active state verification failed: {path}")
    # Compare all bytes except the explicitly allowed single-digit locations.
    masked_before = bytearray(before)
    masked_after = bytearray(after)
    for state in exported.values():
        masked_before[state["offset"]] = 63
        masked_after[state["offset"]] = 63
    if masked_before != masked_after:
        raise ValueError(f"A field other than a GameObject active digit changed: {path}")
    inactive_ids = [file_id for file_id, state in original.items() if not state["active"]]
    active_ids = [file_id for file_id, state in original.items() if state["active"]]
    # All launch-scene objects and both active/inactive examples in every scene.
    sample_ids = list(original) if len(original) <= 16 else sorted(set(list(original)[:6] + active_ids[:5] + inactive_ids[:5] + list(original)[-3:]))
    samples = [{
        "original_path_id": file_id,
        "exported_file_id": file_id,
        "original_name": original[file_id]["name"],
        "original_active": original[file_id]["active"],
        "exported_before_active": exported[file_id]["active"],
        "restored_active": verified[file_id]["active"],
        "match": original[file_id]["active"] == verified[file_id]["active"],
    } for file_id in sample_ids]
    report = {
        "game_objects": len(original),
        "original_active": len(active_ids),
        "original_inactive": len(inactive_ids),
        "exported_before_active": sum(state["active"] for state in exported.values()),
        "changed_game_objects": len(changed),
        "changed_game_object_file_ids": changed,
        "restored_active": sum(state["active"] for state in verified.values()),
        "all_game_object_ids_match": True,
        "all_active_flags_match": True,
        "other_bytes_unchanged": True,
        "before_sha256": digest(before),
        "after_sha256": digest(after),
        "samples": samples,
    }
    return before, after, report


def main() -> None:
    workspace = Path(__file__).resolve().parent.parent
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", type=Path, default=workspace.parent / "com.chillingo.totemrunner.android.rowgplay_1.0.1.apk")
    parser.add_argument("--project", type=Path, default=workspace / "TotemRunner")
    parser.add_argument("--report", type=Path, default=workspace / "Analysis/TotemRunner/activation-recovery.json")
    args = parser.parse_args()
    apk = args.apk.resolve(strict=True)
    if digest(apk.read_bytes()) != EXPECTED_SHA256:
        raise ValueError("Input is not the verified original Totem Runner APK")
    project = args.project.resolve(strict=True)
    assets = (project / "Assets").resolve(strict=True)
    plan = []
    with zipfile.ZipFile(apk) as archive:
        main_data = archive.read("assets/bin/Data/mainData")
        settings_env = UnityPy.load(main_data)
        settings = [obj for obj in settings_env.objects if obj.type.name == "BuildSettings"]
        if len(settings) != 1:
            raise ValueError("Expected exactly one original BuildSettings object")
        levels = settings[0].read_typetree()["levels"]
        if len(levels) != 21:
            raise ValueError(f"Expected 21 original build scenes, found {len(levels)}")
        build_paths = set()
        for index, scene in enumerate(levels):
            path = (project / scene).resolve(strict=True)
            path.relative_to(assets)
            if path.suffix != ".unity":
                raise ValueError(f"Unexpected scene file extension: {path}")
            build_paths.add(path)
            serialized_name = "mainData" if index == 0 else f"level{index - 1}"
            raw = main_data if index == 0 else archive.read("assets/bin/Data/" + serialized_name)
            original = scene_data(raw)
            before, after, report = prepare_scene(path, original)
            report.update({"build_index": index, "scene": scene, "original_serialized_file": serialized_name})
            plan.append((path, before, after, report))
        actual_paths = {path.resolve() for path in assets.rglob("*.unity")}
        if build_paths != actual_paths:
            raise ValueError("Project scene files differ from the original build scene list")
    # Avoid replacing scene data that another cooperating agent changed meanwhile.
    for path, before, _, _ in plan:
        if path.read_bytes() != before:
            raise RuntimeError(f"Scene changed while activation restoration was being prepared: {path}")
    print(f"Validated exact GameObject IDs in all {len(plan)} scenes; applying active digits only.", flush=True)
    for path, before, after, _ in plan:
        if before != after:
            if path.read_bytes() != before:
                raise RuntimeError(f"Concurrent scene modification: {path}")
            path.write_bytes(after)
        if path.read_bytes() != after:
            raise RuntimeError(f"On-disk scene verification failed: {path}")
    report = {
        "source_apk": str(apk),
        "source_apk_sha256": EXPECTED_SHA256,
        "unitypy_version": UnityPy.__version__,
        "project": str(project),
        "method": "Original APK BuildSettings scene order and serialized-file GameObject pathID mapped exactly to exported scene GameObject fileID",
        "allowed_change": "Only each GameObject YAML document's single m_IsActive digit",
        "scenes": len(plan),
        "game_objects": sum(item[3]["game_objects"] for item in plan),
        "changed_game_objects": sum(item[3]["changed_game_objects"] for item in plan),
        "original_active": sum(item[3]["original_active"] for item in plan),
        "original_inactive": sum(item[3]["original_inactive"] for item in plan),
        "all_original_ids_and_active_flags_match": True,
        "all_other_scene_bytes_unchanged": True,
        "unity_or_game_executed": False,
        "per_scene": [item[3] for item in plan],
    }
    report_path = args.report.resolve()
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({key: report[key] for key in ("scenes", "game_objects", "changed_game_objects", "original_active", "original_inactive", "all_other_scene_bytes_unchanged")}), flush=True)


if __name__ == "__main__":
    main()
