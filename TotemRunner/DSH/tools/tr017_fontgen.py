
# -*- coding: utf-8 -*-
"""TR-017: same as TR-016 (ZCOOL-era charset, SIMYOU source, 2048 atlas) but glyph metrics
now follow the true font metrics: one common em for every char (sized off a reference
ideograph) and each glyph placed by its real baseline offset, so punctuation (，。、·)
keeps its natural size/position instead of being blown up to cap height."""
import re, os, json, hashlib
from PIL import Image, ImageDraw, ImageFont

WS = r"D:\User\Desktop\UnityAndroidTest"
RUN = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261021-17")
OUT = os.path.join(RUN, "font-build")
ORIG = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261020-03", "backup")
CHARSET = os.path.join(WS, "Analysis", "TotemRunner", "runs", "run-20261021-16", "charset.txt")
CJK_TTF = r"C:\Windows\Fonts\SIMYOU.TTF"
FONT_SCALE = 0.9
NEW = 2048
REF_CHAR = "国"
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

def render_ink(ch, em, stroke, baseline):
    """Render one glyph with its baseline at 'baseline' px; return (crop, w, h, vy)."""
    f = ImageFont.truetype(CJK_TTF, em)
    img = Image.new("L", (em * 4, em * 4), 0)
    d = ImageDraw.Draw(img)
    strokew = max(0, int(round(stroke)))
    d.text((em, baseline), ch, font=f, fill=255, stroke_width=strokew, stroke_fill=255, anchor="ls")
    bbox = img.getbbox()
    if bbox is None:
        return None
    g = img.crop(bbox)
    w, h = g.size
    vy = baseline - bbox[1]
    return g, w, h, vy

def pick_em(target):
    em = max(6, int(round(target / 0.92)))
    for _ in range(12):
        r = render_ink(REF_CHAR, em, 0, em * 2)
        if not r:
            break
        h = r[2]
        if abs(h - target) <= 0.5:
            break
        em = max(6, int(round(em * float(target) / h)))
    return em

def main():
    os.makedirs(os.path.join(OUT, "fonts"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "textures"), exist_ok=True)
    chars = parse_charset(CHARSET)
    print("charset unique:", len(chars))
    manifest = {"charset_count": len(chars), "chars": "".join(chars), "atlas": NEW,
                "source_font": "C:/Windows/Fonts/SIMYOU.TTF", "metrics": "natural baseline", "fonts": []}
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
        target = round(cap_h * FONT_SCALE)
        stroke = max(0.0, cap_h * 0.02)
        em = pick_em(target)
        print("\n== %s tex %dx%d entries %d ascent %.3f capH %.1f target %d em %d stroke %.2f"
              % (name, TW, TH, len(entries), ascent, cap_h, target, em, stroke))
        missing = [ch for ch in chars if ord(ch) not in have]
        glyphs = []
        for ch in missing:
            r = render_ink(ch, em, stroke, em * 2)
            if r:
                glyphs.append((ch, r[0], r[1], r[2], r[3]))
        # keep the approved vertical placement (TR-013): centre ideograph ink on the cap centre,
        # while punctuation keeps its natural offset relative to the ideographs
        cap_vy = sorted(e[6] for e in caps)[len(caps) // 2]
        cap_top = ascent + cap_vy
        cap_center = cap_top - cap_h / 2.0
        centers = sorted(g[4] - g[3] / 2.0 for g in glyphs if 0x4E00 <= ord(g[0]) <= 0x9FFF)
        natural = centers[len(centers) // 2] if centers else 0.0
        delta = cap_center - natural
        glyphs = [(ch, gimg, w, h, vy + delta) for (ch, gimg, w, h, vy) in glyphs]
        print("   cap_center %.2f natural %.2f delta %.2f" % (cap_center, natural, delta))
        cell = max(int(target) + 10, max(g[2] for g in glyphs) + 4, max(g[3] for g in glyphs) + 4)
        cols = max(1, (NEW - 8) // cell)
        y0 = TH + 8
        Vnew = Image.new("L", (NEW, NEW), 0)
        Vnew.paste(V, (0, 0))
        new_entries = []
        for idx, (ch, gimg, w, h, vy) in enumerate(glyphs):
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
                                      ascent=ascent, cap_h=cap_h, target=target, em=em, cell=cell,
                                      font_sha256=hashlib.sha256(s2.encode("utf-8")).hexdigest(),
                                      texture_sha256=hashlib.sha256(tex2.encode("utf-8")).hexdigest()))
        print("   wrote", name, "entries", len(all_entries), "new", len(new_entries), "cell", cell)
    json.dump(manifest, open(os.path.join(OUT, "manifest.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=2)
    print("DONE", OUT)

if __name__ == "__main__":
    main()
