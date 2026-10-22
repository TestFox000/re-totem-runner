"""Restore implicit camera projections after the observed AssetRipper export bug.

Only Camera.m_projectionMatrixMode is changed. The original Unity 3.5 files have
no projection-matrix override and no recovered script assigns projectionMatrix.
The Unity2021 probe demonstrates mode 0 loading as identity, and native mode 1
computing the original FOV/orthographic projection. No other field is altered.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    workspace = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", type=Path, default=workspace / "TotemRunner")
    parser.add_argument("--report", type=Path, default=workspace / "Analysis/TotemRunner/camera-projection-repair.json")
    args = parser.parse_args()
    document = re.compile(r"^--- !u!(\d+) &(-?\d+)\r?\n(.*?)(?=^--- !u!|\Z)", re.M | re.S)
    field = re.compile(r"^(  m_projectionMatrixMode:) ([012])(?=\r?$)", re.M)
    planned = []
    camera_count = 0
    for path in sorted((args.project / "Assets").rglob("*.unity")):
        before = path.read_bytes()
        text = before.decode("utf-8")
        records = []

        def replace(match):
            nonlocal camera_count
            if match[1] != "20":
                return match[0]
            mode = field.search(match[0])
            if mode is None or mode[2] not in ["0", "1"]:
                raise ValueError("Unexpected Camera projection mode: " + str(path) + " fileID " + match[2])
            camera_count += 1
            records.append({"fileID": int(match[2]), "before": int(mode[2]), "after": 1, "changed": mode[2] == "0"})
            return field.sub(lambda value: value[1] + " 1", match[0], count=1)

        after = document.sub(replace, text).encode("utf-8")
        # A whole-file comparison after masking just this one field proves that
        # FOV, clip planes, orthographic size, transforms and all other YAML stay.
        mask = lambda data: re.sub(rb"(?m)^(  m_projectionMatrixMode:) [01](?=\r?$)", rb"\1 <same>", data)
        if mask(before) != mask(after):
            raise ValueError("Unexpected non-projection edit: " + str(path))
        if records:
            meta = path.with_name(path.name + ".meta")
            planned.append((path, before, after, meta, digest(meta.read_bytes()), records))
    if camera_count != 59 or len(planned) != 21:
        raise ValueError("Expected 59 cameras in 21 original scenes; got " + str(camera_count) + " / " + str(len(planned)))

    report = {
        "reason": "AssetRipper filled a new Camera projection mode with 0 (Explicit), loading an identity projection. Native Unity2021 defaults to 1 (Implicit); ResetProjectionMatrix restores the expected FOV/ortho projection.",
        "probe": str(workspace / "Analysis/TotemRunner/camera-projection-probe.json"),
        "cameraCount": camera_count,
        "sceneCount": len(planned),
        "changedCameras": sum(record["changed"] for item in planned for record in item[5]),
        "onlyProjectionModeFieldChanged": True,
        "scenes": [],
    }
    for path, before, after, meta, meta_hash, records in planned:
        if after != before:
            path.write_bytes(after)
        assert digest(meta.read_bytes()) == meta_hash
        report["scenes"].append({"path": str(path), "sha256Before": digest(before), "sha256After": digest(after), "metaUnchanged": True, "cameras": records})
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"cameras": camera_count, "scenes": len(planned), "changed": report["changedCameras"], "report": str(args.report)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
