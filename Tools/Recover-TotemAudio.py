"""Recover the original Totem Runner audio while retaining exported Unity GUIDs.

Requires the already-installed UnityPy package. Does not run Unity or alter scenes.
The complete extraction is validated before any placeholder is moved.
"""

from __future__ import annotations

import argparse
import hashlib
import io
import json
import re
import shutil
import wave
from collections import defaultdict
from pathlib import Path

import UnityPy


ORIGINAL_APK_SHA256 = "108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e"
GUID_RE = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.MULTILINE)
NAME_RE = re.compile(r"^  m_Name:\s*(.*?)\s*$", re.MULTILINE)


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def contained(path: Path, root: Path) -> Path:
    resolved = path.resolve()
    resolved.relative_to(root.resolve())
    return resolved


def legacy_entries(audio_root: Path, backup_root: Path) -> list[dict]:
    entries: dict[str, dict] = {}
    for root in (backup_root, audio_root):
        if not root.exists():
            continue
        for asset in sorted(root.glob("*.audioclip")):
            asset = contained(asset, root)
            meta = contained(Path(str(asset) + ".meta"), root)
            source = asset.read_text(encoding="utf-8-sig")
            meta_text = meta.read_text(encoding="utf-8-sig")
            name_match = NAME_RE.search(source)
            guid_match = GUID_RE.search(meta_text)
            if not name_match or not guid_match:
                raise ValueError(f"Missing original name or GUID: {asset}")
            # Output filenames always come from an existing, confined asset path.
            entry = {
                "name": name_match.group(1),
                "stem": asset.stem,
                "guid": guid_match.group(1),
                "legacy_filename": asset.name,
                "legacy_meta": meta_text,
                "legacy_sha256": sha256(asset.read_bytes()),
                "legacy_meta_sha256": sha256(meta.read_bytes()),
            }
            previous = entries.get(asset.name)
            if previous and previous != entry:
                raise ValueError(f"Different live and backed-up placeholders: {asset.name}")
            entries[asset.name] = entry
    if len(entries) != 91:
        raise ValueError(f"Expected 91 original AudioClip assets, found {len(entries)}")
    names = [entry["name"] for entry in entries.values()]
    guids = [entry["guid"] for entry in entries.values()]
    if len(set(names)) != len(names) or len(set(guids)) != len(guids):
        raise ValueError("Duplicate placeholder names or GUIDs require manual mapping")
    return sorted(entries.values(), key=lambda entry: entry["stem"].casefold())


def validate_audio(data: bytes, suffix: str) -> dict:
    if suffix == ".wav":
        with wave.open(io.BytesIO(data), "rb") as wav:
            result = {
                "channels": wav.getnchannels(),
                "frequency": wav.getframerate(),
                "sample_width": wav.getsampwidth(),
                "sample_frames": wav.getnframes(),
            }
            if not all(result.values()):
                raise ValueError("Empty WAV stream")
            pcm = wav.readframes(result["sample_frames"])
            expected = result["sample_frames"] * result["channels"] * result["sample_width"]
            if len(pcm) != expected:
                raise ValueError(f"Truncated WAV: {len(pcm)} of {expected} PCM bytes")
            result["duration_seconds"] = result["sample_frames"] / result["frequency"]
            result["pcm_sha256"] = sha256(pcm)
            return result
    if suffix == ".mp3":
        if not (data.startswith(b"ID3") or (len(data) > 2 and data[0] == 255 and data[1] & 224 == 224)):
            raise ValueError("Missing MP3 frame or ID3 header")
        if len(data) < 1000:
            raise ValueError("Unexpectedly short MP3 stream")
        return {"validation": "Original MPEG audio frame header and complete source bytes; no transcoding"}
    raise ValueError(f"Unsupported audio extension: {suffix}")


def decode_original(apk: Path, wanted: set[str]) -> tuple[dict, dict]:
    env = UnityPy.load(str(apk))
    records: dict[str, list[dict]] = defaultdict(list)
    total_objects = 0
    audio_objects = 0
    for obj in env.objects:
        total_objects += 1
        if obj.type.name != "AudioClip":
            continue
        audio_objects += 1
        audio = obj.read()
        name = audio.m_Name
        if name not in wanted:
            continue
        raw = bytes(audio.m_AudioData or b"")
        method = "UnityPy AudioClip.samples"
        error = None
        try:
            samples = audio.samples
        except Exception as exc:
            samples = {}
            error = f"{type(exc).__name__}: {exc}"
        if samples:
            if len(samples) != 1:
                raise ValueError(f"Multiple audio substreams for {name}: {list(samples)}")
            sample_name, payload = next(iter(samples.items()))
            suffix = Path(sample_name).suffix.lower()
            payload = bytes(payload)
        elif raw and (raw.startswith(b"ID3") or (len(raw) > 2 and raw[0] == 255 and raw[1] & 224 == 224)):
            payload = raw
            suffix = ".mp3"
            method = "Exact original AudioClip.m_AudioData MP3; samples exporter returned empty"
        else:
            raise ValueError(f"Cannot recover {name}: {error or 'No samples returned'}")
        audio_info = validate_audio(payload, suffix)
        records[name].append({
            "serialized_file": str(obj.assets_file.name),
            "path_id": obj.path_id,
            "raw_source_bytes": len(raw),
            "raw_source_sha256": sha256(raw),
            "suffix": suffix,
            "payload": payload,
            "bytes": len(payload),
            "sha256": sha256(payload),
            "method": method,
            "decode_error": error,
            "audio_info": audio_info,
            "legacy_stream_mode": getattr(audio, "m_Stream", None),
            "legacy_3d": getattr(audio, "m_3D", False),
        })
    missing = sorted(wanted - records.keys())
    if missing:
        raise ValueError(f"Missing original clips: {missing}")
    for name, candidates in records.items():
        if len({(candidate["suffix"], candidate["sha256"]) for candidate in candidates}) != 1:
            raise ValueError(f"Different original payloads share one name: {name}")
    return records, {"all_objects_inspected": total_objects, "audio_objects_inspected": audio_objects}


def importer_meta(entry: dict, candidate: dict) -> str:
    created = re.search(r"^timeCreated:.*$", entry["legacy_meta"], re.MULTILINE)
    # PCM avoids a second lossy encoding of the recovered original sound.
    # Original MP3 music/ambience is streamed to avoid keeping decoded PCM in memory.
    load_type = 2 if candidate["suffix"] == ".mp3" else 0
    return (
        "fileFormatVersion: 2\n"
        f"guid: {entry['guid']}\n"
        + (created.group(0) + "\n" if created else "")
        + "AudioImporter:\n"
        "  externalObjects: {}\n"
        "  serializedVersion: 7\n"
        "  defaultSettings:\n"
        "    serializedVersion: 2\n"
        f"    loadType: {load_type}\n"
        "    sampleRateSetting: 0\n"
        "    sampleRateOverride: 44100\n"
        "    compressionFormat: 0\n"
        "    quality: 1\n"
        "    conversionMode: 0\n"
        "  platformSettingOverrides: {}\n"
        "  forceToMono: 0\n"
        "  normalize: 0\n"
        "  preloadAudioData: 1\n"
        "  loadInBackground: 0\n"
        "  ambisonic: 0\n"
        f"  3D: {int(bool(candidate['legacy_3d']))}\n"
        "  userData: Original APK audio restored by Tools/Recover-TotemAudio.py\n"
        "  assetBundleName:\n"
        "  assetBundleVariant:\n"
    )


def move_placeholder(source: Path, destination: Path, audio_root: Path, backup_root: Path) -> None:
    source = contained(source, audio_root)
    destination = contained(destination, backup_root)
    if not source.exists():
        if not destination.exists():
            raise FileNotFoundError(f"Neither live nor backed-up placeholder exists: {source.name}")
        return
    if destination.exists():
        if source.read_bytes() != destination.read_bytes():
            raise ValueError(f"Refusing to overwrite different backup: {destination}")
        source.unlink()
    else:
        shutil.move(str(source), str(destination))


def main() -> None:
    workspace = Path(__file__).resolve().parent.parent
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", type=Path, default=workspace.parent / "com.chillingo.totemrunner.android.rowgplay_1.0.1.apk")
    parser.add_argument("--project", type=Path, default=workspace / "TotemRunner")
    parser.add_argument("--backup", type=Path, default=workspace / "Analysis/TotemRunner/LegacyAudioYaml")
    parser.add_argument("--report", type=Path, default=workspace / "Analysis/TotemRunner/audio-recovery.json")
    args = parser.parse_args()
    apk = args.apk.resolve(strict=True)
    if sha256(apk.read_bytes()) != ORIGINAL_APK_SHA256:
        raise ValueError("Input APK differs from the user's verified original Totem Runner 1.0.1")
    project = args.project.resolve(strict=True)
    audio_root = contained(project / "Assets/AudioClip", project).resolve(strict=True)
    backup_root = args.backup.resolve()
    report_path = args.report.resolve()
    if backup_root == audio_root or backup_root.is_relative_to(audio_root):
        raise ValueError("Backup directory must be outside the Unity AudioClip import directory")
    entries = legacy_entries(audio_root, backup_root)
    originals, scan = decode_original(apk, {entry["name"] for entry in entries})
    plan = []
    for entry in entries:
        candidate = originals[entry["name"]][0]
        output = contained(audio_root / (entry["stem"] + candidate["suffix"]), audio_root)
        meta = contained(Path(str(output) + ".meta"), audio_root)
        meta_data = importer_meta(entry, candidate).encode("utf-8")
        if output.exists() and output.read_bytes() != candidate["payload"]:
            raise ValueError(f"Refusing to overwrite different restored audio: {output}")
        if meta.exists() and meta.read_bytes() != meta_data:
            existing_guid = GUID_RE.search(meta.read_text(encoding="utf-8-sig"))
            if not existing_guid or existing_guid.group(1) != entry["guid"]:
                raise ValueError(f"Different existing importer GUID: {meta}")
        for legacy_name in (entry["legacy_filename"], entry["legacy_filename"] + ".meta"):
            source = contained(audio_root / legacy_name, audio_root)
            destination = contained(backup_root / legacy_name, backup_root)
            if source.exists() and destination.exists() and source.read_bytes() != destination.read_bytes():
                raise ValueError(f"Different existing backup: {destination}")
        plan.append((entry, candidate, output, meta, meta_data))
    print(f"Validated all {len(plan)} original clips; restoring audio with original GUIDs.", flush=True)
    backup_root.mkdir(parents=True, exist_ok=True)
    for entry, candidate, output, meta, meta_data in plan:
        for legacy_name in (entry["legacy_filename"], entry["legacy_filename"] + ".meta"):
            move_placeholder(audio_root / legacy_name, backup_root / legacy_name, audio_root, backup_root)
        if not output.exists() or output.read_bytes() != candidate["payload"]:
            output.write_bytes(candidate["payload"])
        if not meta.exists() or meta.read_bytes() != meta_data:
            meta.write_bytes(meta_data)
    report = {
        "source_apk": str(apk),
        "source_sha256": ORIGINAL_APK_SHA256,
        "unitypy_version": UnityPy.__version__,
        "project": str(project),
        "backup_directory": str(backup_root),
        "scan": scan,
        "expected_clips": 91,
        "restored_clips": len(plan),
        "total_audio_bytes": sum(candidate["bytes"] for _, candidate, *_ in plan),
        "formats": dict((suffix, sum(candidate["suffix"] == suffix for _, candidate, *_ in plan)) for suffix in (".wav", ".mp3")),
        "unity_audio_file_id": 8300000,
        "scene_references_require_type_change": {"from": 2, "to": 3},
        "import_settings": {"compression": "PCM, avoids second lossy encoding", "normalize": False, "resample": False, "mp3_load_type": "Streaming", "wav_load_type": "DecompressOnLoad"},
        "limitations": ["No Unity import, game execution, audio playback, or subjective listening test was performed.", "Clips for which the samples decoder returns no samples are preserved as complete original MP3 bytes after validating the MPEG/ID3 header."],
        "clips": [],
    }
    mapping = {}
    for entry, candidate, output, meta, _ in plan:
        detail = {key: value for key, value in candidate.items() if key != "payload"}
        detail.update({
            "name": entry["name"], "guid": entry["guid"],
            "old_asset": "Assets/AudioClip/" + entry["legacy_filename"],
            "new_asset": str(output.relative_to(project)).replace("\\", "/"),
            "legacy_yaml_sha256": entry["legacy_sha256"],
            "legacy_meta_sha256": entry["legacy_meta_sha256"],
            "importer_meta_sha256": sha256(meta.read_bytes()),
            "original_occurrences": [{key: value for key, value in item.items() if key in ("serialized_file", "path_id", "raw_source_sha256")} for item in originals[entry["name"]]],
        })
        report["clips"].append(detail)
        mapping[entry["guid"]] = {"old_asset": detail["old_asset"], "new_asset": detail["new_asset"], "fileID": 8300000, "old_type": 2, "new_type": 3}
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    mapping_path = report_path.with_name("audio-guid-mapping.json")
    mapping_path.write_text(json.dumps(mapping, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"restored": len(plan), "formats": report["formats"], "bytes": report["total_audio_bytes"], "report": str(report_path), "guid_mapping": str(mapping_path)}, ensure_ascii=False), flush=True)


if __name__ == "__main__":
    main()
