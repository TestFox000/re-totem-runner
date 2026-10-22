"""Restore legacy kerning->tracking and the native static-font pixel scale.

Requires the native Unity probe evidence. Only two scalar fields per existing
font are replaced; original glyphs, atlas/material references and GUIDs remain.
"""
import argparse
import hashlib
import json
import re
import zipfile
from pathlib import Path
import UnityPy


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apk', required=True, type=Path)
    parser.add_argument('--project', required=True, type=Path)
    parser.add_argument('--evidence', required=True, type=Path)
    parser.add_argument('--report', required=True, type=Path)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    apk = args.apk.read_bytes()
    if digest(apk) != '108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e':
        raise ValueError('Unexpected original APK')
    native = json.loads(args.evidence.read_text(encoding='utf-8'))['Font']
    scale = native['m_PixelScale']
    if abs(scale - .1) > 1e-7 or native['m_Tracking'] != 1:
        raise ValueError('Unexpected native static-font defaults')
    with zipfile.ZipFile(args.apk) as z:
        source_bytes = z.read('assets/bin/Data/sharedassets1.assets')
    env = UnityPy.load(source_bytes)
    plan = []
    for obj in env.objects:
        if obj.type.name != 'Font':
            continue
        src = obj.read_typetree()
        tracking = src['m_Kerning']
        if tracking != 1 or 'm_PixelScale' in src:
            raise ValueError('Source font differs from inspected Unity 3.5 schema')
        path = args.project / 'Assets' / 'Font' / (src['m_Name'] + '.asset')
        before = path.read_bytes()
        meta = Path(str(path) + '.meta').read_bytes()
        text = before.decode('utf-8-sig')
        changes = []
        for key, value, origin in [('m_Tracking', tracking, 'original m_Kerning'), ('m_PixelScale', .1, 'Unity 2021 new Font() native default; legacy equivalent')]:
            pattern = rf'(?m)^(  {key}: )[^\r\n]+'
            match = re.search(pattern, text)
            if not match:
                raise ValueError(f'{path}: missing {key}')
            old = float(match.group(0).split(':', 1)[1])
            text, count = re.subn(pattern, lambda m: m.group(1) + format(value, '.9g'), text)
            if count != 1:
                raise ValueError('Unexpected field count')
            changes.append({'field': key, 'before': old, 'after': value, 'origin': origin})
        after = text.encode('utf-8')
        if before.startswith(b'\xef\xbb\xbf'):
            after = b'\xef\xbb\xbf' + after
        plan.append((path, before, after, meta, {'font': src['m_Name'], 'source_path_id': obj.path_id, 'glyph_count': len(src['m_CharacterRects']), 'changes': changes, 'sha256_before': digest(before), 'sha256_after': digest(after), 'meta_sha256': digest(meta)}))
    if len(plan) != 3:
        raise ValueError('Expected three original fonts')
    for path, before, after, meta, _ in plan:
        if path.read_bytes() != before or Path(str(path) + '.meta').read_bytes() != meta:
            raise ValueError('Concurrent font change')
    if args.apply:
        for path, before, after, meta, _ in plan:
            path.write_bytes(after)
            if Path(str(path) + '.meta').read_bytes() != meta:
                raise ValueError('Font meta changed')
    report = {'applied': args.apply, 'source_apk_sha256': digest(apk), 'native_default_evidence': str(args.evidence), 'scope': 'Three Fonts, two scalar metrics each. No glyph, material, texture, scene or GUID changes.', 'fonts': [entry[-1] for entry in plan]}
    args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps({'applied': args.apply, 'fonts': len(plan), 'fields': 6}))


if __name__ == '__main__':
    main()
