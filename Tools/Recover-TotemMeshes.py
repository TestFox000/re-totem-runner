"""Extract verified Unity 3.5 mesh data for TotemMeshRecovery.RecoverAll.

This tool writes evidence/payload JSON only. It never modifies project assets.
Defaults are relative to this workspace; all paths may be supplied explicitly.
Requires UnityPy 1.25.4. MeshHelper decodes vertex streams/compressed attributes;
legacy index topology and compressed four-weight skinning are handled explicitly.
"""
from __future__ import annotations

import argparse
from collections import Counter, defaultdict
import hashlib
import json
import math
from pathlib import Path
import re
import struct
import sys
import zipfile

import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler
from UnityPy.helpers.PackedBitVector import unpack_ints

DOCS = re.compile(r"^--- !u!(\d+) &(-?\d+)\r?\n(.*?)(?=^--- !u!|\Z)", re.M | re.S)


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def flat(rows, dimension: int):
    if not rows:
        return []
    result = [float(value) for row in rows for value in row[:dimension]]
    if not all(math.isfinite(value) for value in result):
        raise ValueError("Non-finite vertex attribute")
    return result


def basename(path: str) -> str:
    return path.replace("\\", "/").rsplit("/", 1)[-1]


def source_key(filename: str, path_id: int) -> tuple[str, int]:
    return basename(filename), int(path_id)


def pointer_key(file, pointer, current_filename):
    filename = current_filename if pointer.m_FileID == 0 else file.externals[pointer.m_FileID - 1].path
    return source_key(filename, pointer.m_PathID)


def raw_fingerprint(mesh):
    return (
        mesh.m_Name,
        mesh.m_VertexData.m_VertexCount,
        sha(bytes(mesh.m_VertexData.m_DataSize)),
        sha(bytes(mesh.m_IndexBuffer or [])),
    )


def yaml_fingerprint(text: str):
    def field(pattern):
        match = re.search(pattern, text, re.M)
        if match is None:
            raise ValueError("Missing mesh field: " + pattern)
        return match[1]
    return (
        field(r"^  m_Name: ?(.*?)\r?$"),
        int(field(r"^    m_VertexCount: (\d+)")),
        sha(bytes.fromhex(field(r"^    _typelessdata: ?([0-9a-f]*)"))),
        sha(bytes.fromhex(field(r"^  m_IndexBuffer: ?([0-9a-f]*)"))),
    )


def compressed_weights(mesh, vertex_count):
    """Unity's 5-bit weights sum to 31; the fourth is an implicit remainder.

    UnityPy 1.25.4 uses 1 - integer_sum for that fourth weight. Decode it here
    as (31 - integer_sum) / 31 and validate all counts and indices instead.
    """
    packed = mesh.m_CompressedMesh
    if not packed.m_Weights.m_NumItems:
        return None
    values = list(unpack_ints(packed.m_Weights))
    indices = list(unpack_ints(packed.m_BoneIndices))
    weights = [[0.0] * 4 for _ in range(vertex_count)]
    bones = [[0] * 4 for _ in range(vertex_count)]
    vertex = slot = total = index_cursor = 0
    implicit_fourths = 0
    for value in values:
        if vertex >= vertex_count or index_cursor >= len(indices):
            raise ValueError("Compressed skin data overruns vertex/bone array")
        weights[vertex][slot] = value / 31.0
        bones[vertex][slot] = indices[index_cursor]
        index_cursor += 1
        total += value
        slot += 1
        if total >= 31:
            if total != 31:
                raise ValueError("Compressed skin weights exceed 31")
            vertex += 1
            slot = total = 0
        elif slot == 3:
            if index_cursor >= len(indices):
                raise ValueError("Missing implicit-fourth bone index")
            weights[vertex][3] = (31 - total) / 31.0
            bones[vertex][3] = indices[index_cursor]
            index_cursor += 1
            implicit_fourths += 1
            vertex += 1
            slot = total = 0
    if vertex != vertex_count or slot != 0 or index_cursor != len(indices):
        raise ValueError("Compressed skin count mismatch")
    return weights, bones, implicit_fourths


def decode_mesh(mesh):
    handler = MeshHandler(mesh)
    handler.process()
    vertex_count = len(handler.m_Vertices or [])
    if vertex_count == 0 or vertex_count != handler.m_VertexCount:
        raise ValueError("Original mesh has no complete position channel")
    fields = {
        "name": mesh.m_Name,
        "vertexCount": vertex_count,
        "positions": flat(handler.m_Vertices, 3),
        "normals": flat(handler.m_Normals, 3),
        "tangents": flat(handler.m_Tangents, 4),
        "colors": flat(handler.m_Colors, 4),
        "uv0": flat(handler.m_UV0, 2),
        "uv1": flat(handler.m_UV1, 2),
        "boneWeights": [],
        "boneIndices": [],
        "bindposes": [],
        "subMeshes": [],
        "boundsCenter": [mesh.m_LocalAABB.m_Center.x, mesh.m_LocalAABB.m_Center.y, mesh.m_LocalAABB.m_Center.z],
        "boundsSize": [2 * mesh.m_LocalAABB.m_Extent.x, 2 * mesh.m_LocalAABB.m_Extent.y, 2 * mesh.m_LocalAABB.m_Extent.z],
        "sourceMeshUsageFlags": mesh.m_MeshUsageFlags,
    }
    for key, dimension in [("positions", 3), ("normals", 3), ("tangents", 4), ("colors", 4), ("uv0", 2), ("uv1", 2)]:
        if fields[key] and len(fields[key]) != vertex_count * dimension:
            raise ValueError("Incomplete attribute: " + key)

    decoded_skin = compressed_weights(mesh, vertex_count)
    fourths = 0
    if decoded_skin is not None:
        weights, bones, fourths = decoded_skin
    else:
        weights = handler.m_BoneWeights or []
        bones = handler.m_BoneIndices or []
    if weights:
        if len(weights) != vertex_count or len(bones) != vertex_count:
            raise ValueError("Skin vertex count mismatch")
        for row in weights:
            if any(value < 0 or not math.isfinite(value) for value in row) or abs(sum(row) - 1) > 0.0001:
                raise ValueError("Invalid original skin weights")
        fields["boneWeights"] = flat(weights, 4)
        fields["boneIndices"] = [int(value) for row in bones for value in row]

    compressed_bindposes = getattr(handler, "m_BindPose", None)
    if compressed_bindposes:
        fields["bindposes"] = [float(x) for matrix in compressed_bindposes for row in matrix for x in row]
    else:
        fields["bindposes"] = [float(getattr(matrix, "e" + str(row) + str(column)))
                               for matrix in (mesh.m_BindPose or []) for row in range(4) for column in range(4)]
    bind_count = len(fields["bindposes"]) // 16
    for weight, bone in zip(fields["boneWeights"], fields["boneIndices"]):
        if weight > 0 and not 0 <= bone < bind_count:
            raise ValueError("Weighted bone index is outside bindposes")

    # MeshHandler.get_triangles treats all pre-4 meshes as strips. The original
    # integer isTriStrip flag must distinguish the game's 15 triangle-list sets.
    raw_indices = handler.m_IndexBuffer or []
    index_size = 2 if handler.m_Use16BitIndices else 4
    for submesh in mesh.m_SubMeshes:
        first = submesh.firstByte // index_size
        indices = list(raw_indices[first:first + submesh.indexCount])
        if len(indices) != submesh.indexCount:
            raise ValueError("Submesh index span is incomplete")
        triangles = []
        if submesh.isTriStrip == 0:
            if len(indices) % 3:
                raise ValueError("Triangle-list indices are not a multiple of three")
            triangles = indices
        elif submesh.isTriStrip in [1, 2]:
            for i in range(len(indices) - 2):
                a, b, c = indices[i:i + 3]
                if a == b or a == c or b == c:
                    continue
                triangles.extend((b, a, c) if i & 1 else (a, b, c))
        else:
            raise ValueError("Unknown legacy topology flag " + str(submesh.isTriStrip))
        if len(triangles) // 3 != submesh.triangleCount:
            raise ValueError("Recovered triangle count differs from original")
        if any(index < 0 or index >= vertex_count for index in triangles):
            raise ValueError("Triangle index outside vertex array")
        fields["subMeshes"].append({"indices": triangles, "sourceTopologyFlag": submesh.isTriStrip,
                                    "sourceTriangleCount": submesh.triangleCount})
    if mesh.m_Shapes:
        raise ValueError("Unexpected blend-shape payload requires explicit recovery")
    return fields, fourths


def main():
    workspace = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apk", type=Path, default=workspace.parent / "com.chillingo.totemrunner.android.rowgplay_1.0.1.apk")
    parser.add_argument("--project", type=Path, default=workspace / "TotemRunner")
    parser.add_argument("--reference-root", type=Path, default=workspace / "Recovery/TotemRunner-Unity2021/ExportedProject")
    parser.add_argument("--output-root", type=Path, default=workspace / "Analysis/TotemRunner")
    args = parser.parse_args()
    args.output_root.mkdir(parents=True, exist_ok=True)
    files = {}
    originals = {}
    raw_candidates = defaultdict(list)
    stats = Counter()
    with zipfile.ZipFile(args.apk) as archive:
        for entry in sorted(archive.namelist()):
            if not (entry.startswith("assets/bin/Data/") and (entry.endswith(".assets") or re.search(r"/level\d+$", entry) or entry.endswith("/mainData"))):
                continue
            environment = UnityPy.load(archive.read(entry))
            serialized_file = next(iter(environment.files.values()))
            files[basename(entry)] = serialized_file
            for obj in environment.objects:
                if obj.type.name != "Mesh":
                    continue
                mesh = obj.read()
                key = source_key(entry, obj.path_id)
                geometry, fourths = decode_mesh(mesh)
                identity = sha(json.dumps(geometry, sort_keys=True, separators=(",", ":"), allow_nan=False).encode())
                originals[key] = {"geometry": geometry, "fingerprint": raw_fingerprint(mesh), "geometryHash": identity,
                                  "rawObjectHash": sha(obj.get_raw_data())}
                raw_candidates[originals[key]["fingerprint"]].append(key)
                stats["sourceMeshCount"] += 1
                stats["sourceVertices"] += geometry["vertexCount"]
                stats["sourceSkinnedMeshes"] += bool(geometry["boneWeights"])
                stats["compressedSkinFourthWeightsCorrected"] += fourths
                for submesh in geometry["subMeshes"]:
                    stats["sourceTriangleListSubmeshes" if submesh["sourceTopologyFlag"] == 0 else "sourceStripSubmeshes"] += 1

    main_file = files["mainData"]
    levels = next(obj.read().levels for obj in main_file.objects.values() if obj.type.name == "BuildSettings")
    anchors = defaultdict(set)
    for index, level in enumerate(levels):
        filename = "mainData" if index == 0 else "level" + str(index - 1)
        file = files[filename]
        text = (args.reference_root / level).read_text(encoding="utf-8-sig")
        docs = {(int(c), int(fid)): body for c, fid, body in DOCS.findall(text) if int(c) in [33, 64, 137]}
        for obj in file.objects.values():
            if obj.type.value not in [33, 64, 137]:
                continue
            pointer = getattr(obj.read(), "m_Mesh", None)
            if pointer is None or pointer.m_PathID == 0:
                continue
            body = docs.get((obj.type.value, obj.path_id), "")
            target = re.search(r"^  m_Mesh: \{fileID: 4300000, guid: (\w+), type: \d+\}", body, re.M)
            if target:
                anchors[target[1]].add(pointer_key(file, pointer, filename))

    records = []
    mappings = []
    errors = []
    for path in sorted((args.project / "Assets/Mesh").glob("*.asset")):
        asset_path = path.relative_to(args.project).as_posix()
        meta = path.with_name(path.name + ".meta").read_bytes()
        guid = re.search(rb"^guid: (\w+)", meta, re.M)[1].decode()
        candidates = anchors.get(guid)
        method = "source scene component fileID to exported mesh GUID"
        if not candidates:
            reference = args.reference_root / asset_path
            candidates = set(raw_candidates.get(yaml_fingerprint(reference.read_text(encoding="utf-8-sig")), []))
            method = "original name + original vertex count + raw vertex/index SHA256"
        if not candidates or any(key not in originals for key in candidates):
            errors.append({"assetPath": asset_path, "error": "No original mesh mapping", "candidates": sorted(candidates or [])})
            continue
        hashes = {originals[key]["geometryHash"] for key in candidates}
        if len(hashes) != 1:
            errors.append({"assetPath": asset_path, "error": "Ambiguous sources have different full geometry; no guess made", "candidates": sorted(candidates)})
            continue
        key = sorted(candidates)[0]
        source = originals[key]
        geometry = dict(source["geometry"])
        geometry.update(assetPath=asset_path, guid=guid, expectedLocalFileId=4300000,
                        metaSha256=sha(meta), geometrySha256=source["geometryHash"],
                        originalSources=[{"file": item[0], "pathId": item[1]} for item in sorted(candidates)])
        records.append(geometry)
        mappings.append({"assetPath": asset_path, "guid": guid, "method": method, "sources": geometry["originalSources"],
                         "geometrySha256": source["geometryHash"], "vertexCount": geometry["vertexCount"],
                         "triangles": sum(len(s["indices"]) // 3 for s in geometry["subMeshes"]),
                         "skinned": bool(geometry["boneWeights"]), "bindposeCount": len(geometry["bindposes"]) // 16})
        stats["mappedByScene" if guid in anchors else "mappedByFingerprint"] += 1
        stats["multipleIdenticalPayloadSources"] += len(candidates) > 1

    payload_path = args.output_root / "mesh-recovery-payload.json"
    report_path = args.output_root / "mesh-recovery-extraction.json"
    report = {"apk": str(args.apk.resolve()), "apkSha256": sha(args.apk.read_bytes()), "unityPyVersion": UnityPy.__version__,
              "project": str(args.project.resolve()), "referenceRoot": str(args.reference_root.resolve()),
              "payload": str(payload_path.resolve()), "stats": dict(stats), "mappings": mappings, "errors": errors,
              "method": "Source scene pointer graph first; raw geometry fingerprint only for unreferenced meshes. Differing ambiguous geometry is rejected. No asset writes by Python.",
              "corrections": ["Legacy topology flag 0 kept as triangle list; flags 1/2 expanded from strip with parity and degenerate-index removal. Every triangle count matches original metadata.",
                              "Compressed 5-bit skin fourth weight decoded as (31-sum)/31, correcting UnityPy 1.25.4 implicit-fourth arithmetic. All bone weights and indices validated.",
                              "Original vertex streams/compressed attributes are decoded without coordinate flips, normal/tangent recalculation or mesh optimization."],
              "editorEntryPoint": "TotemMeshRecovery.RecoverAll"}
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2, allow_nan=False), encoding="utf-8")
    if errors or len(records) != stats["sourceMeshCount"]:
        print(json.dumps({"status": "failed", "records": len(records), "stats": dict(stats), "errors": errors, "report": str(report_path)}, ensure_ascii=False))
        return 1
    payload = {"schemaVersion": 1, "expectedMeshCount": len(records), "records": records}
    payload_path.write_text(json.dumps(payload, ensure_ascii=False, separators=(",", ":"), allow_nan=False), encoding="utf-8")
    print(json.dumps({"status": "ready", "records": len(records), "stats": dict(stats), "payloadBytes": payload_path.stat().st_size,
                      "payload": str(payload_path), "report": str(report_path), "editorMethod": "TotemMeshRecovery.RecoverAll"}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.exit(main())
