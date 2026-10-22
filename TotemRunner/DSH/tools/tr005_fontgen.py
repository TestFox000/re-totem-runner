# -*- coding: utf-8 -*-
"""TR-005: expand legacy Unity bitmap font atlases with CJK glyphs (pilot)."""
import re, os, json, hashlib
from PIL import Image, ImageDraw, ImageFont

WS = r"D:\User\Desktop\UnityAndroidTest"
PROJ = os.path.join(WS, "TotemRunner")
OUT = os.environ.get("TR005_OUT", os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261020-03", "font-build"))
CHARSET = os.environ.get("TR005_CHARSET", os.path.join(PROJ, "DSH", "reports", "TR-004", "1", "data", "charset.txt"))
CJK_TTF = r"C:\Windows\Fonts\NotoSansSC-VF.ttf"
NEW = 1024
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
    han = re.search(r"## 汉字（\d+ 个）\n\n([^\n]+)", t).group(1).strip()
    punct = re.search(r"## 非汉字的全角/中文标点（\d+ 个）\n\n([^\n]+)", t).group(1).strip()
    msupp = re.search(r"## 按 style-notes §9 强制补充（\d+ 个[^\n]*）\n\n([^\n]+)", t)
    supp = msupp.group(1).strip() if msupp else ""
    chars = []
    for ch in han + punct + supp:
        if ch not in chars and ch != " ":
            chars.append(ch)
    return chars, {"han": len(han), "punct": len(punct), "supp": len(supp)}

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

def render_glyphs(chars, em, weight):
    f = ImageFont.truetype(CJK_TTF, em)
    try:
        f.set_variation_by_axes([weight])
    except Exception as e:
        print("WARN variation failed:", e)
    out = {}
    for ch in chars:
        cw, ch_h = int(em * 2), int(em * 2)
        img = Image.new("L", (cw, ch_h), 0)
        d = ImageDraw.Draw(img)
        base = int(em * 1.3)
        d.text((em // 2, base), ch, font=f, fill=255, anchor="ls")
        bbox = img.getbbox()
        if bbox is None:
            print("WARN empty glyph", ch, hex(ord(ch)))
            continue
        out[ch] = (img.crop(bbox), bbox, base)
    return out

def main():
    os.makedirs(os.path.join(OUT, "fonts"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "textures"), exist_ok=True)
    chars, counts = parse_charset(CHARSET)
    print("charset:", counts, "unique", len(chars))
    manifest = {"charset_counts": counts, "chars": "".join(chars), "fonts": []}

    for cfg in FONTS:
        name, texname = cfg["name"], cfg["tex"]
        fpath = os.path.join(PROJ, "Assets", "Font", name + ".asset")
        tpath = os.path.join(PROJ, "Assets", "Texture2D", texname)
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
        print("\n== %s: tex %dx%d, entries %d, ascent %.3f, cap %.1f -> em %d" % (name, TW, TH, len(entries), ascent, cap_med, em))
        missing = [ch for ch in chars if ord(ch) not in have]
        print("   missing glyphs:", len(missing))
        glyphs = render_glyphs(missing, em, cfg["weight"])
        stride = em + 8
        cols = (NEW - 4) // stride
        y0 = TH + 6
        rows = 0
        new_entries = []
        Vnew = Image.new("L", (NEW, NEW), 0)
        Vnew.paste(V, (0, 0))
        idx = 0
        for ch in missing:
            g = glyphs.get(ch)
            if not g:
                continue
            gimg, bbox, base = g
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
            rows = max(rows, row + 1)
        # transform existing uv
        all_entries = []
        for e in entries:
            idxc, ux, uy, uw, uh, vx, vy, vw, vh, adv = e
            nux = ux * TW / NEW
            nuy = (NEW - TH) / NEW + uy * TH / NEW
            nuw = uw * TW / NEW
            nuh = uh * TH / NEW
            all_entries.append([idxc, nux, nuy, nuw, nuh, vx, vy, vw, vh, adv])
        all_entries += new_entries
        all_entries.sort(key=lambda e: e[0])
        # check uniqueness
        ids = [e[0] for e in all_entries]
        assert len(ids) == len(set(ids)), "duplicate indices " + str(name)
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
        s2 = re.sub(r"(  m_CharacterRects:\n).*?(\n  m_KerningValues:)", r"\1" + block_text.replace("\\", "\\\\") + r"\2", s, flags=re.S)
        # verify replacement sanity
        assert "m_KerningValues" in s2 and s2.count("index:") == len(all_entries), (name, s2.count("index:"))
        data = Vnew.transpose(Image.FLIP_TOP_BOTTOM).tobytes()
        tex2 = save_texture(tex, NEW, NEW, data)
        open(os.path.join(OUT, "fonts", name + ".asset"), "w", encoding="utf-8-sig", newline="\n").write(s2)
        open(os.path.join(OUT, "textures", texname), "w", encoding="utf-8-sig", newline="\n").write(tex2)
        Vnew.save(os.path.join(OUT, name + "_atlas.png"))
        # sample render
        sample = "开始 选项 重置 滑动变身 图腾行者"
        # build lookup for samples
        look = {e[0]: e for e in all_entries}
        Wpx = sum(int(look[ord(c)][9]) if ord(c) in look else em for c in sample) + 20
        canvas = Image.new("L", (max(Wpx, 64), int(em * 2.2)), 0)
        pen = 10
        for c in sample:
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
        h = hashlib.sha256(s2.encode("utf-8")).hexdigest()
        th = hashlib.sha256(tex2.encode("utf-8")).hexdigest()
        manifest["fonts"].append(dict(name=name, texture=texname, width=NEW, height=NEW,
            entries=len(all_entries), new=len(new_entries), em=em,
            font_sha256=h, texture_sha256=th))
        print("   wrote", name, "entries", len(all_entries), "new", len(new_entries), "grid rows", rows)
    json.dump(manifest, open(os.path.join(OUT, "manifest.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=2)
    print("\nDONE ->", OUT)

if __name__ == "__main__":
    main()