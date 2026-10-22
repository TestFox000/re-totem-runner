"""Restore native enabled defaults omitted by the Unity 3.5 CC serialized format.

Checks source type trees and a successful native Editor Move probe first.
Only edits m_Enabled inside the corresponding original CharacterController doc.
"""
from pathlib import Path
import hashlib
import json
import re
import UnityPy

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / "TotemRunner"
APK = ROOT.parent / "com.chillingo.totemrunner.android.rowgplay_1.0.1.apk"
REPORT = ROOT / "Analysis/TotemRunner/character-controller-recovery.json"


def main():
    probe = json.loads((ROOT / "Analysis/TotemRunner/character-controller-native-probe.json").read_text())
    assert probe["nativeDefaultEnabled"] and probe["nativeMoveEffective"]
    env = UnityPy.load(str(APK))
    originals = {asset.name:asset for asset in env.assets}
    build = (PROJECT / "ProjectSettings/EditorBuildSettings.asset").read_text(encoding="utf-8")
    scenes = re.findall(r"(?m)^\s+path: (Assets/.*\.unity)\s*$",build)
    assert len(scenes) == 21, len(scenes)
    changes = []
    counts = {"matchedSourceControllers":0,"changedEnabledFields":0,"alreadyCorrect":0}
    doc_rx = re.compile(rb"(?m)^--- !u!(\d+) &(-?\d+)(?: stripped)?\r?\n")
    pending = []
    for index,scene in enumerate(scenes):
        original_file = "mainData" if index == 0 else "level" + str(index-1)
        source = originals[original_file]
        path = PROJECT / scene
        before = path.read_bytes()
        matches = list(doc_rx.finditer(before))
        replacements = []
        for i,m in enumerate(matches):
            if int(m[1]) != 143: continue
            fid = int(m[2]); source_object = source.objects[fid]
            assert source_object.type.name == "CharacterController"
            tree = source_object.read_typetree()
            assert "m_Enabled" not in tree, (original_file,fid,tree)
            old = before[m.start():matches[i+1].start() if i+1<len(matches) else len(before)]
            current = re.search(rb"(?m)^  m_Enabled: ([01])\r?$",old)
            assert current
            counts["matchedSourceControllers"] += 1
            new,count = re.subn(rb"(?m)^(  m_Enabled: )0(\r?)$",rb"\g<1>1\2",old)
            if count: counts["changedEnabledFields"] += 1
            else: counts["alreadyCorrect"] += 1
            assert new.replace(b"  m_Enabled: 1",b"  m_Enabled: 0",1)==old if count else new==old
            replacements.append((m.start(),m.start()+len(old),new))
            changes.append({"scene":scene,"sourceSerializedFile":original_file,"componentFileID":fid,
                            "gameObjectFileID":int(re.search(rb"m_GameObject: \{fileID: (-?\d+)\}",old)[1]),
                            "sourceFields":sorted(tree),"sourceHasEnabledField":False,
                            "previousEnabled":int(current[1]),"restoredEnabled":1,
                            "originalComponentYaml":old.decode("utf-8")})
        after = before
        for start,end,replacement in reversed(replacements): after = after[:start]+replacement+after[end:]
        # No other original bytes change, including component IDs and whitespace.
        reverse = after
        for start,end,replacement in replacements:
            if before[start:end] != replacement:
                reverse = reverse[:start]+replacement.replace(b"  m_Enabled: 1",b"  m_Enabled: 0",1)+reverse[end:]
        assert reverse == before
        pending.append((path,before,after))
    assert counts["matchedSourceControllers"] == 18, counts
    report={"counts":counts,"nativeProbe":probe,
            "reason":"Unity 3.5 original CharacterController type tree has no m_Enabled field. Exporter inserted 0; modern native AddComponent default is true. Restore only this absent-field native default.",
            "scope":"Only 18 original CharacterController documents' m_Enabled fields; same fileIDs, geometry, references, metadata, and all other scene bytes.",
            "components":changes,
            "scenes":[{"scene":p.relative_to(PROJECT).as_posix(),"beforeSha256":hashlib.sha256(b).hexdigest(),"afterSha256":hashlib.sha256(a).hexdigest(),"changed":b!=a} for p,b,a in pending]}
    REPORT.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
    for path,before,after in pending:
        if before!=after: path.write_bytes(after)
    print(json.dumps(counts))


if __name__ == "__main__": main()
