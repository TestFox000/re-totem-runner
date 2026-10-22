"""Decode old Android format-2 texture bytes as GPU RGBA4444.

The original Unity 3.5 Android payload uses R:G:B:A high-to-low nibbles in a
little-endian 16-bit word. Generic modern ARGB4444 decoding instead produced
G:B:A:R and moved red into alpha. Only original-format-2 PNGs are replaced;
all source pixels are deterministically recovered from the APK, with no painting.
Run once without --apply to create before/after previews and validate the plan.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import struct
import zipfile

from PIL import Image
import UnityPy


def sha(data):
    return hashlib.sha256(data).hexdigest()


def image_sha(image):
    return sha(image.convert("RGBA").tobytes())


def alpha_stats(image):
    alpha = image.getchannel("A")
    histogram = alpha.histogram()
    return {"min": alpha.getextrema()[0], "max": alpha.getextrema()[1],
            "zeroPixels": histogram[0], "opaquePixels": histogram[255],
            "cornerRGBA": list(image.getpixel((0, 0)))}


def checker_preview(image, path):
    background = Image.new("RGB", image.size)
    pixels = background.load()
    for y in range(image.height):
        for x in range(image.width):
            value = 160 if ((x // 8) + (y // 8)) % 2 else 110
            pixels[x, y] = (value, value, value)
    background.paste(image, (0, 0), image.getchannel("A"))
    background.save(path)


def decode_android_rgba4444(texture):
    width, height = texture.m_Width, texture.m_Height
    raw = bytes(texture.image_data)
    required = width * height * 2
    if len(raw) < required:
        raise ValueError("Incomplete RGBA4444 base mip")
    pixels = bytearray(width * height * 4)
    for i, (value,) in enumerate(struct.iter_unpack("<H", raw[:required])):
        pixels[i * 4:i * 4 + 4] = bytes((((value >> 12) & 15) * 17,
                                        ((value >> 8) & 15) * 17,
                                        ((value >> 4) & 15) * 17,
                                        (value & 15) * 17))
    # GPU texture rows have a bottom-left origin; exported PNGs use top-left.
    image = Image.frombytes("RGBA", (width, height), bytes(pixels)).transpose(Image.Transpose.FLIP_TOP_BOTTOM)
    return image, sha(raw[:required])


def main():
    workspace = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true")
    parser.add_argument("--apk", type=Path, default=workspace.parent / "com.chillingo.totemrunner.android.rowgplay_1.0.1.apk")
    parser.add_argument("--project", type=Path, default=workspace / "TotemRunner")
    parser.add_argument("--analysis", type=Path, default=workspace / "Analysis/TotemRunner")
    args = parser.parse_args()
    material_report = json.loads((args.analysis / "material-recovery.json").read_text(encoding="utf-8"))
    mapping = {(item["source_file"], item["source_path_id"]): item for item in material_report["texture_mapping"]}
    original_backups = args.analysis / "Texture4444Originals"
    previews = args.analysis / "Texture4444Previews"
    previews.mkdir(parents=True, exist_ok=True)

    texture_root = args.project / "Assets/Texture2D"
    original_png_hashes = {path: sha(path.read_bytes()) for path in texture_root.glob("*.png")}
    original_meta_hashes = {path: sha(path.read_bytes()) for path in texture_root.glob("*.meta")}
    shader_hashes = {path: sha(path.read_bytes()) for path in (args.project / "Assets/Shader").glob("*.shader")}
    formats = Counter()
    plans = []
    with zipfile.ZipFile(args.apk) as archive:
        for entry in sorted(archive.namelist()):
            if not (entry.startswith("assets/bin/Data/") and (entry.endswith(".assets") or re.search(r"/level\d+$", entry))):
                continue
            environment = UnityPy.load(archive.read(entry))
            for obj in environment.objects:
                if obj.type.name != "Texture2D":
                    continue
                texture = obj.read()
                formats[int(texture.m_TextureFormat)] += 1
                if int(texture.m_TextureFormat) != 2:
                    continue
                identity = mapping[(entry.rsplit("/", 1)[-1], obj.path_id)]
                path = args.project / identity["asset"]
                if path.suffix.lower() != ".png" or path.parent != texture_root:
                    raise ValueError("Unexpected format-2 destination: " + str(path))
                meta = path.with_name(path.name + ".meta")
                guid = re.search(rb"^guid: (\w+)", meta.read_bytes(), re.M)[1].decode()
                if guid != identity["guid"]:
                    raise ValueError("Texture GUID mismatch: " + str(path))

                generic = texture.image.convert("RGBA")
                corrected, raw_hash = decode_android_rgba4444(texture)
                red, green, blue, alpha = generic.split()
                channel_permutation = Image.merge("RGBA", (alpha, red, green, blue))
                if corrected.tobytes() != channel_permutation.tobytes():
                    raise ValueError("Direct packed-nibble decode differs from verified channel permutation")
                current = Image.open(path).convert("RGBA")
                if current.size != corrected.size:
                    raise ValueError("PNG dimensions differ from original: " + str(path))
                current_sha = image_sha(current)
                generic_sha = image_sha(generic)
                corrected_sha = image_sha(corrected)
                if current_sha not in [generic_sha, corrected_sha]:
                    raise ValueError("PNG differs from both original generic decode and corrected decode: " + str(path))
                backup = original_backups / path.name
                record = {"asset": identity["asset"], "guid": guid, "sourceFile": entry,
                          "sourcePathId": obj.path_id, "sourceName": texture.m_Name,
                          "sourceTextureFormat": 2, "width": texture.m_Width, "height": texture.m_Height,
                          "baseMipRawSha256": raw_hash, "genericDecodedPixelSha256": generic_sha,
                          "restoredPixelSha256": corrected_sha, "beforePngFileSha256": sha(path.read_bytes()),
                          "beforeAlpha": alpha_stats(generic), "afterAlpha": alpha_stats(corrected),
                          "backup": str(backup), "alreadyCorrect": current_sha == corrected_sha,
                          "metaSha256": sha(meta.read_bytes()), "identityEvidence": identity["evidence"]}
                plans.append((path, generic, corrected, record))
                if texture.m_Name in ["glowworm", "Soul_Light", "Button_Menu", "Stroke"]:
                    generic.save(previews / (path.stem + ".before.png"))
                    corrected.save(previews / (path.stem + ".after.png"))
                    checker_preview(generic, previews / (path.stem + ".before.checker.png"))
                    checker_preview(corrected, previews / (path.stem + ".after.checker.png"))

    if formats[2] != 193 or len(plans) != 193 or len({item[0] for item in plans}) != 193:
        raise ValueError("Expected exactly 193 uniquely mapped original format-2 PNGs")
    affected = {item[0] for item in plans}
    changed = 0
    if args.apply:
        original_backups.mkdir(parents=True, exist_ok=True)
        for path, generic, corrected, record in plans:
            backup = Path(record["backup"])
            if not backup.exists():
                # Reconstruct the original generic pixels if this asset was already
                # corrected by an earlier invocation; otherwise keep exact PNG bytes.
                if record["alreadyCorrect"]:
                    generic.save(backup)
                else:
                    backup.write_bytes(path.read_bytes())
            if image_sha(Image.open(backup).convert("RGBA")) != record["genericDecodedPixelSha256"]:
                raise ValueError("Backup does not preserve original exported pixels: " + str(backup))
            if not record["alreadyCorrect"]:
                corrected.save(path)
                changed += 1
            if image_sha(Image.open(path).convert("RGBA")) != record["restoredPixelSha256"]:
                raise ValueError("Written PNG does not match direct source decode: " + str(path))
            record["afterPngFileSha256"] = sha(path.read_bytes())

    untouched = [path for path in original_png_hashes if path not in affected]
    for path in untouched:
        if sha(path.read_bytes()) != original_png_hashes[path]:
            raise ValueError("A non-format-2 PNG changed: " + str(path))
    for path, value in original_meta_hashes.items():
        if sha(path.read_bytes()) != value:
            raise ValueError("A texture meta changed: " + str(path))
    for path, value in shader_hashes.items():
        if sha(path.read_bytes()) != value:
            raise ValueError("A shader changed: " + str(path))
    report = {"status": "applied" if args.apply else "validated-plan", "sourceApk": str(args.apk),
              "rule": "For original Android format 2 only, decode little-endian 16-bit words as high-to-low RGBA nibbles, each nibble multiplied by 17. Equivalently generic modern ARGB4444 image RGBA is permuted to (A,R,G,B). Pixel positions unchanged.",
              "cause": "Generic ARGB4444 decoding placed the original red nibble in alpha and original alpha in blue. This produced opaque yellow/green rectangles and magenta effects before blending.",
              "sourceFormatCounts": dict(formats), "format2PngCount": len(plans), "changedPngCount": changed,
              "untouchedOtherFormatPngCount": len(untouched), "allOtherFormatPngsByteIdentical": True,
              "allTextureMetasByteIdentical": True, "all13ShadersByteIdentical": True,
              "previews": str(previews), "textures": [item[3] for item in plans]}
    path = args.analysis / "texture4444-recovery.json"
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"status": report["status"], "format2Pngs": len(plans), "changed": changed,
                      "otherFormatPngsUnchanged": len(untouched), "previews": str(previews), "report": str(path)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
