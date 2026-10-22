# -*- coding: utf-8 -*-
"""TR-005b: legacy bitmap font atlas expansion, corrected baseline, 2048 atlas."""
import re, os, json, hashlib
from PIL import Image, ImageDraw, ImageFont

WS = r"D:\User\Desktop\UnityAndroidTest"
PROJ = os.path.join(WS, "TotemRunner")
OUT = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261021-08", "font-build2")
ORIG = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261020-03", "backup")
CHARSET = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261021-08", "charset.txt")
CJK_TTF = r"C:\Windows\Fonts\NotoSansSC-VF.ttf"
NEW = 2048
BLOCK_RE = re.compile(
    r"- serializedVersion: 2\n    index: (\d+)\n"
    r"    uv:\n      serializedVersion: 2\n      x: ([\d.]+)\n      y: ([\d.]+)\n      width: ([\d.]+)\n      height: ([\d.]+)\n"
    r"    vert:\n      serializedVersion: 2\n      x: (-?[\d.]+)\n      y: (-?[\d.]+)\n      width: ([\d.]+)\n      height: (-?[\d.]+)\n"
    r"    advance: ([\d.]+)")

FONTS = [
    dict(name="berlin_sans_fb_demi_bold", tex="Font Texture.texture2D", weight=700),
    dict(name="berlin_sans_buymenu", tex="Font Texture_0.texture2D", weight=700),
    dict(name="LithosPro-Black", tex="Font Texture_1.texture2D", weight=900),
]

def fmt(x):
    x = float(x)
    if abs(x - round(x)) < 1e-9:
        return str(int(round(x)))
    return ("%.8f" % x).rstrip("0").rstrip(".")

def parse_charset(path):
    t = open(path, encoding="utf-8").read()
    body = [ln for ln in t.split("\n") if ln and not ln.startswith("#")]
    chars = []
    for ch in body[0]:
        if ch not in chars:
            chars.append(ch)
    return chars

def read_texture(path):
    s = open(path, encoding="utf-8-sig").read()
    W = int(re.search(r"m_Width: (\d+)", s).group(1))
    H = int(re.search(r"m_Height: (\d+)", s).group(1))
    raw = bytes.fromhex(re.search(r"_typelessdata: ([0-9a-fA-F]+)", s).group(1))
    assert len(raw) == W * H, (len(raw), W, H)
    return s, W, H, raw

def save_texture(s, W, H, raw):
    s = re.sub(r"m_Width: \d+", "m_Width: %d" % W, s)
    s = re.sub(r"m_Height: \d+", "m_Height: %d" % H, s)
    s = re.sub(r"m_CompleteImageSize: \d+", "m_CompleteImageSize: %d" % (W * H), s)
    s = re.sub(r"image data: \d+", "image data: %d" % (W * H), s)
    s = re.sub(r"_typelessdata: [0-9a-fA-F]+", "_typelessdata: " + raw.hex(), s)
    return s

def render_glyphs(chars, em, weight, ascent):
    f = ImageFont.truetype(CJK_TTF, em)
    try:
        f.set_variation_by_axes([weight])
    except Exception as e:
        print("WARN variation failed:", e)
    out = {}
    base = int(round(ascent))
    cell_w, cell_h = int(em * 2), int(ascent + em * 0.9)
    for ch in chars:
        img = Image.new("L", (cell_w, cell_h), 0)
        d = ImageDraw.Draw(img)
        d.text((em // 2, base), ch, font=f, fill=255, anchor="ls")
        bbox = img.getbbox()
        if bbox is None:
            print("WARN empty glyph", ch, hex(ord(ch)))
            continue
        out[ch] = (img.crop(bbox), bbox)
    return out

def main():
    os.makedirs(os.path.join(OUT, "fonts"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "textures"), exist_ok=True)
    chars = parse_charset(CHARSET)
    print("charset unique:", len(chars))
    manifest = {"charset_count": len(chars), "chars": "".join(chars), "atlas": NEW, "fonts": []}
    for cfg in FONTS:
        name, texname = cfg["name"], cfg["tex"]
        fpath = os.path.join(ORIG, "font-assets", name + ".asset")
        tpath = os.path.join(ORIG, "textures", texname)
        s = open(fpath, encoding="utf-8-sig").read()
        tex, TW, TH, raw = read_texture(tpath)
        V = Image.frombytes("L", (TW, TH), raw).transpose(Image.FLIP_TOP_BOTTOM)
        ascent = float(re.search(r"m_Ascent: ([\d.]+)", s).group(1))
        entries = []
        for m in BLOCK_RE.finditer(s):
            entries.append([int(m.group(1))] + [float(m.group(i)) for i in range(2, 11)])
        have = {e[0] for e in entries}
        cap = sorted(abs(e[8]) for e in entries if 65 <= e[0] <= 90 and float(e[8]) != 0)
        cap_med = cap[len(cap) // 2] if cap else ascent / 1.3
        em = int(round(cap_med / 0.72))
        print("\n== %s: tex %dx%d entries %d ascent %.3f cap %.1f em %d" % (name, TW, TH, len(entries), ascent, cap_med, em))
        missing = [ch for ch in chars if ord(ch) not in have]
        glyphs = render_glyphs(missing, em, cfg["weight"], ascent)
        stride = em + 8
        cols = (NEW - 4) // stride
        y0 = TH + 6
        Vnew = Image.new("L", (NEW, NEW), 0)
        Vnew.paste(V, (0, 0))
        new_entries = []
        idx = 0
        for ch in missing:
            g = glyphs.get(ch)
            if not g:
                continue
            gimg, bbox = g
            w, h = gimg.size
            col = idx % cols
            row = idx // cols
            x = 4 + col * stride
            y = y0 + row * stride
            assert y + h <= NEW and x + w <= NEW, (name, ch, x, y, w, h)
            Vnew.paste(gimg, (x, y))
            ux = x / NEW; uy = 1 - (y + h) / NEW; uw = w / NEW; uh = h / NEW
            vx = (em - w) // 2
            vy = -bbox[1]
            new_entries.append([ord(ch), ux, uy, uw, uh, vx, vy, w, -h, em])
            idx += 1
        all_entries = []
        for e in entries:
            idxc, ux, uy, uw, uh, vx, vy, vw, vh, adv = e
            all_entries.append([idxc, ux * TW / NEW, (NEW - TH) / NEW + uy * TH / NEW, uw * TW / NEW, uh * TH / NEW, vx, vy, vw, vh, adv])
        all_entries += new_entries
        all_entries.sort(key=lambda e: e[0])
        ids = [e[0] for e in all_entries]
        assert len(ids) == len(set(ids))
        blocks = []
        for e in all_entries:
            (idxc, ux, uy, uw, uh, vx, vy, vw, vh, adv) = e
            blocks.append(
                "  - serializedVersion: 2\n"
                "    index: %d\n"
                "    uv:\n      serializedVersion: 2\n      x: %s\n      y: %s\n      width: %s\n      height: %s\n"
                "    vert:\n      serializedVersion: 2\n      x: %s\n      y: %s\n      width: %s\n      height: %s\n"
                "    advance: %s\n    flipped: 0" % (idxc, fmt(ux), fmt(uy), fmt(uw), fmt(uh), fmt(vx), fmt(vy), fmt(vw), fmt(vh), fmt(adv)))
        block_text = "\n".join(blocks)
        s2 = re.sub(r"(  m_CharacterRects:\n).*?(\n  m_KerningValues:)", lambda mm: mm.group(1) + block_text + mm.group(2), s, flags=re.S)
        assert s2.count("index:") == len(all_entries), (name, s2.count("index:"))
        data = Vnew.transpose(Image.FLIP_TOP_BOTTOM).tobytes()
        tex2 = save_texture(tex, NEW, NEW, data)
        open(os.path.join(OUT, "fonts", name + ".asset"), "w", encoding="utf-8-sig", newline="\n").write(s2)
        open(os.path.join(OUT, "textures", texname), "w", encoding="utf-8-sig", newline="\n").write(tex2)
        Vnew.save(os.path.join(OUT, name + "_atlas.png"))
        # mixed sample render using entries (Latin + CJK)
        look = {e[0]: e for e in all_entries}
        text = "开始 Play 选项 重置 滑动变身 商店 难度 普通 困难"
        height = int(em * 2.2)
        width = 20 + sum(int(look[ord(c)][9]) if ord(c) in look else em for c in text)
        canvas = Image.new("L", (max(width, 64), height), 0)
        pen = 10
        for c in text:
            if c == " ":
                pen += em // 2
                continue
            e = look.get(ord(c))
            if not e:
                pen += em
                continue
            ux, uy, uw, uh, vx, vy, vw, vh, adv = e[1:]
            gw = int(round(uw * NEW)); gh = int(round(uh * NEW))
            gx = int(round(ux * NEW)); gy = int(round((1 - uy - uh) * NEW))
            glyph_img = Vnew.crop((gx, gy, gx + gw, gy + gh))
            top = int(round(-vy))
            canvas.paste(glyph_img, (pen + int(round(vx)), top))
            pen += int(round(adv))
        canvas.save(os.path.join(OUT, name + "_sample.png"))
        manifest["fonts"].append(dict(name=name, texture=texname, entries=len(all_entries), new=len(new_entries), em=em,
                                      font_sha256=hashlib.sha256(s2.encode("utf-8")).hexdigest(),
                                      texture_sha256=hashlib.sha256(tex2.encode("utf-8")).hexdigest()))
        print("   wrote", name, "entries", len(all_entries), "new", len(new_entries))
    json.dump(manifest, open(os.path.join(OUT, "manifest.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=2)
    print("DONE", OUT)

if __name__ == "__main__":
    main()
