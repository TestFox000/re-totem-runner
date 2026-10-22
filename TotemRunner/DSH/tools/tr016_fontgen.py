# -*- coding: utf-8 -*-
"""TR-016: same generator as TR-013 with an extended charset (About page glyphs).
TR-013: rebuild CJK glyphs (ZCOOL KuaiLe, 0.9x cap) with the vertical placement fixed to
Unity TextMesh's real convention: glyph top = vert.y (baseline-relative), so vert.y must be the
absolute top-above-baseline, NOT top-minus-ascent (that omission dropped every glyph by one ascent)."""
import re, os, json, hashlib
from PIL import Image, ImageDraw, ImageFont

WS = r"D:\User\Desktop\UnityAndroidTest"
RUN = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261021-16")
OUT = os.path.join(RUN, "font-build")
ORIG = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261020-03", "backup")
CHARSET = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261021-16", "charset.txt")
CJK_TTF = r"C:\Windows\Fonts\SIMYOU.TTF"
FONT_SCALE = 0.9
NEW = 2048
BLOCK_RE = re.compile(
    r"- serializedVersion: 2\n    index: (\d+)\n"
    r"    uv:\n      serializedVersion: 2\n      x: ([\d.]+)\n      y: ([\d.]+)\n      width: ([\d.]+)\n      height: ([\d.]+)\n"
    r"    vert:\n      serializedVersion: 2\n      x: (-?[\d.]+)\n      y: (-?[\d.]+)\n      width: ([\d.]+)\n      height: (-?[\d.]+)\n"
    r"    advance: ([\d.]+)")

FONTS = [
    dict(name="berlin_sans_fb_demi_bold", tex="Font Texture.texture2D"),
    dict(name="berlin_sans_buymenu", tex="Font Texture_0.texture2D"),
    dict(name="LithosPro-Black", tex="Font Texture_1.texture2D"),
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

def ink_of(ch, em):
    f = ImageFont.truetype(CJK_TTF, em)
    img = Image.new("L", (em * 3, em * 3), 0)
    d = ImageDraw.Draw(img)
    d.text((em, em * 2), ch, font=f, fill=255)
    b = img.getbbox()
    return (b[2] - b[0], b[3] - b[1]) if b else (0, 0)

def make_glyph(ch, target_h, cap_center, ascent, stroke):
    em = max(6, int(round(target_h / 0.92)))
    for _ in range(8):
        w, h = ink_of(ch, em)
        if h == 0:
            return None
        if abs(h - target_h) <= 0.7:
            break
        em = max(6, int(round(em * target_h / h)))
    f = ImageFont.truetype(CJK_TTF, em)
    img = Image.new("L", (em * 3, em * 3), 0)
    d = ImageDraw.Draw(img)
    strokew = max(0, int(round(stroke)))
    d.text((em, em * 2), ch, font=f, fill=255, stroke_width=strokew, stroke_fill=255)
    bbox = img.getbbox()
    if bbox is None:
        return None
    g = img.crop(bbox)
    h = g.size[1]
    # Unity TextMesh places the quad top at vert.y measured up from the baseline, so vert.y is
    # the absolute top-above-baseline. (Subtracting ascent here was the /one ascent too low/ bug.)
    vy = cap_center + h / 2.0
    return g, int(round(vy)), em

def main():
    os.makedirs(os.path.join(OUT, "fonts"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "textures"), exist_ok=True)
    chars = parse_charset(CHARSET)
    print("charset unique:", len(chars))
    manifest = {"charset_count": len(chars), "chars": "".join(chars), "atlas": NEW, "source_font": "ZCOOLKuaiLe-Regular.ttf", "fonts": []}
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
        caps = [e for e in entries if 65 <= e[0] <= 90]
        cap_h = sorted(-e[8] for e in caps)[len(caps) // 2]
        cap_vy = sorted(e[6] for e in caps)[len(caps) // 2]
        cap_top = ascent + cap_vy
        cap_center = cap_top - cap_h / 2.0
        target = round(cap_h * FONT_SCALE)
        stroke = max(0.0, cap_h * 0.02)
        print("\n== %s tex %dx%d entries %d ascent %.3f capH %.1f capTop %.2f target %.1f stroke %.2f"
              % (name, TW, TH, len(entries), ascent, cap_h, cap_top, target, stroke))
        missing = [ch for ch in chars if ord(ch) not in have]
        glyphs = {}
        for ch in missing:
            g = make_glyph(ch, target, cap_center, ascent, stroke)
            if g:
                glyphs[ch] = g
        cell = int(target) + 10
        cols = max(1, (NEW - 8) // cell)
        y0 = TH + 8
        Vnew = Image.new("L", (NEW, NEW), 0)
        Vnew.paste(V, (0, 0))
        new_entries = []
        idx = 0
        for ch in missing:
            g = glyphs.get(ch)
            if not g:
                continue
            gimg, vy, em = g
            w, h = gimg.size
            col = idx % cols
            row = idx // cols
            x = 8 + col * cell
            y = y0 + row * cell
            if y + h > NEW:
                raise SystemExit("atlas overflow at %s row %d" % (name, row))
            Vnew.paste(gimg, (x, y))
            ux = x / NEW
            uy = 1 - (y + h) / NEW
            uw = w / NEW
            uh = h / NEW
            adv = max(target, w)
            vx = int(round((adv - w) / 2.0))
            new_entries.append([ord(ch), ux, uy, uw, uh, vx, vy, w, -h, adv])
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
        assert s2.count("index:") == len(all_entries)
        data = Vnew.transpose(Image.FLIP_TOP_BOTTOM).tobytes()
        tex2 = save_texture(tex, NEW, NEW, data)
        open(os.path.join(OUT, "fonts", name + ".asset"), "w", encoding="utf-8-sig", newline="\n").write(s2)
        open(os.path.join(OUT, "textures", texname), "w", encoding="utf-8-sig", newline="\n").write(tex2)
        Vnew.save(os.path.join(OUT, name + "_atlas.png"))
        manifest["fonts"].append(dict(name=name, texture=texname, entries=len(all_entries), new=len(new_entries),
                                      ascent=ascent, cap_h=cap_h, target=target,
                                      font_sha256=hashlib.sha256(s2.encode("utf-8")).hexdigest(),
                                      texture_sha256=hashlib.sha256(tex2.encode("utf-8")).hexdigest()))
        print("   wrote", name, "entries", len(all_entries), "new", len(new_entries))
    json.dump(manifest, open(os.path.join(OUT, "manifest.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=2)
    print("DONE", OUT)

if __name__ == "__main__":
    main()