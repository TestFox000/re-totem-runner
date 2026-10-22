"""Preserve original concave geometry while adding the player-query trigger bridge.

Run only after TotemConcaveTriggerProbe.Run has passed in the native Unity Editor.
No mesh/material data, existing component IDs or gameplay scripts are rewritten.
"""
from __future__ import annotations

import argparse
import collections
import hashlib
import json
import re
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / "TotemRunner"
REPORT = ROOT / "Analysis/TotemRunner/concave-trigger-conversion.json"
DOCUMENT = re.compile(r"(?m)^--- !u!(\d+) &(-?\d+)(?: stripped)?\r?\n")


def field(text, name):
    match = re.search(r"(?m)^  " + re.escape(name) + r": (.*)$", text)
    if not match:
        raise ValueError("missing " + name)
    return match.group(1).strip()


def pointer(text, name):
    return int(re.search(r"fileID: (-?\d+)", field(text, name)).group(1))


def guid(path):
    return re.search(r"(?m)^guid: ([a-f0-9]{32})$", path.read_text()).group(1)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()
    native = json.loads((ROOT / "Analysis/TotemRunner/concave-trigger-native-probe.json").read_text())
    if not native["passed"]:
        raise RuntimeError("Native Unity physics probe must pass before conversion")
    script_root = PROJECT / "Assets/Scripts/Assembly-CSharp"
    marker_guid = guid(script_root / "LegacyConcaveTrigger.cs.meta")
    danger_guids = {guid(script_root / (name + ".cs.meta")): name for name in ("DangerBarrier", "DangerObstacle")}
    report = {"applied": args.apply, "nativeProbePassed": True, "markerScriptGuid": marker_guid,
              "conversion": "Only original MeshCollider.m_IsTrigger 1->0, one GO component reference and one marker document per collider; all existing IDs, other fields and mesh payloads preserved.",
              "originalMatrix": "All 32x32 pairs enabled in source PhysicsManager and current DynamicsManager; runtime checks GetIgnoreLayerCollision each move.",
              "querySemantics": ["Native CharacterController bounds determine actual world-Y capsule center/radius/height including lossy scale, rotated center and height>=2r clamp. No extra skinWidth inflation; native probe confirms rotation45 and scale(2,3,4).",
                                  "BeforeMove and AfterMove capsule overlaps plus forward/reverse casts over actual displacement (native backfaces are otherwise missed); transient crossings send Enter/Exit in one render frame.",
                                  "Original collider object is retained for DangerHelper.Raycast; only original CC is ignored physically.",
                                  "Disabled/reenabled markers and players clear prior contacts; externally moved/respawned players do not sweep across teleport distance."],
              "scenes": [], "references": []}
    scene_data = []
    total = 0
    for scene in sorted((PROJECT / "Assets/Levels").rglob("*.unity")):
        original_bytes = scene.read_bytes()
        text = original_bytes.decode("utf-8")
        newline = "\r\n" if "\r\n" in text else "\n"
        matches = list(DOCUMENT.finditer(text))
        docs = {int(m.group(2)): [int(m.group(1)), text[m.start():matches[i+1].start() if i+1<len(matches) else len(text)]] for i,m in enumerate(matches)}
        if any(kind == 114 and marker_guid in body for kind,body in docs.values()):
            raise RuntimeError("Already converted; inspect existing report before rerunning: " + str(scene))
        gameobjects = {fid: body for fid,(kind,body) in docs.items() if kind == 1}
        transforms = {fid: (pointer(body,"m_GameObject"),pointer(body,"m_Father")) for fid,(kind,body) in docs.items() if kind == 4}
        go_transforms = {go:fid for fid,(go,parent) in transforms.items()}
        scripts = collections.defaultdict(list)
        for fid,(kind,body) in docs.items():
            if kind != 114: continue
            script = field(body,"m_Script")
            for source_guid,name in danger_guids.items():
                if source_guid in script: scripts[pointer(body,"m_GameObject")].append(name)
        def go_path(go):
            parts=[]; seen=set()
            while go and go not in seen:
                seen.add(go); parts.append(field(gameobjects[go],"m_Name"))
                tf=go_transforms.get(go); parent=transforms.get(tf,(0,0))[1]
                go=transforms.get(parent,(0,0))[0]
            return "/".join(reversed(parts))
        changes={}; additions=[]; references=[]
        next_id=max(docs)+1
        for fid,(kind,body) in docs.items():
            if kind != 64 or field(body,"m_Convex") != "0" or field(body,"m_IsTrigger") != "1": continue
            go=pointer(body,"m_GameObject")
            if not scripts[go]: raise RuntimeError("Non-Danger concave trigger outside approved scope " + str(scene) + ":" + str(fid))
            marker_id=next_id; next_id+=1
            replacement,count=re.subn(r"(?m)^(  m_IsTrigger: )1(\r?)$",r"\g<1>0\2",body)
            assert count==1 and replacement.replace("  m_IsTrigger: 0","  m_IsTrigger: 1",1)==body
            changes[fid]=replacement
            old_go=changes.get(go,gameobjects[go])
            insertion="  - component: {fileID: " + str(marker_id) + "}" + newline
            new_go,count=re.subn(r"(?m)^(  m_Layer:)",insertion+r"\1",old_go,count=1)
            assert count==1 and new_go.replace(insertion,"",1)==old_go
            changes[go]=new_go
            marker=(f"--- !u!114 &{marker_id}\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n"
                    "  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
                    f"  m_GameObject: {{fileID: {go}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
                    f"  m_Script: {{fileID: 11500000, guid: {marker_guid}, type: 3}}\n  m_Name: \n  m_EditorClassIdentifier: \n"
                    f"  originalCollider: {{fileID: {fid}}}\n")
            additions.append(marker.replace("\n",newline))
            mesh=field(body,"m_Mesh")
            references.append({"scene":scene.relative_to(PROJECT).as_posix(),"colliderFileID":fid,"gameObjectFileID":go,
                               "gameObjectPath":go_path(go),"scripts":scripts[go],"markerFileID":marker_id,"mesh":mesh,
                               "meshGuid":re.search(r"guid: ([a-f0-9]{32})",mesh).group(1)})
        output=text[:matches[0].start()]+"".join(changes.get(int(m.group(2)),docs[int(m.group(2))][1]) for m in matches)
        if additions: output=output.rstrip("\r\n")+newline+"".join(additions)
        new_docs=list(DOCUMENT.finditer(output))
        assert len(new_docs)==len(docs)+len(additions)
        total+=len(additions)
        info={"scene":scene.relative_to(PROJECT).as_posix(),"converted":len(additions),"beforeSha256":hashlib.sha256(original_bytes).hexdigest(),
              "afterSha256":hashlib.sha256(output.encode("utf-8")).hexdigest(),"verifiedOriginalMeshColliderOtherFieldsUnchanged":True,"verifiedOnlyApprovedDocumentEdits":True}
        report["scenes"].append(info);report["references"].extend(references)
        scene_data.append((scene,original_bytes,output.encode("utf-8")))
    if total!=1330: raise RuntimeError("Expected precisely 1330 approved legacy triggers, got " + str(total))
    report["convertedMeshColliders"]=total
    report["affectedScenes"]=sum(s["converted"]>0 for s in report["scenes"])
    report["uniqueMeshGuids"]=len({ref["meshGuid"] for ref in report["references"]})
    if args.apply:
        backup=ROOT / "Analysis/TotemRunner/LegacyConcaveTriggerScenes.zip"
        if backup.exists(): raise RuntimeError("Backup already exists; refusing overwrite")
        with zipfile.ZipFile(backup,"x",zipfile.ZIP_DEFLATED,compresslevel=6) as archive:
            for scene,old,new in scene_data:
                if old!=new: archive.writestr(scene.relative_to(PROJECT).as_posix(),old)
        for scene,old,new in scene_data:
            if old!=new: scene.write_bytes(new)
        report["backup"]=str(backup)
    REPORT.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
    print(json.dumps({key:report[key] for key in ("applied","convertedMeshColliders","affectedScenes","uniqueMeshGuids")},ensure_ascii=False))


if __name__=="__main__": main()
