using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Native static-font/TextMesh probe. Cloned fonts and temporary objects only.
public static class TotemTextNativeProbe
{
    [Serializable] private sealed class Sample
    {
        public string font; public string variant; public int glyphCount; public bool hasA; public int advance;
        public Vector2 minGlyph; public Vector2 maxGlyph; public int fontSize; public float pixelScale;
        public string material; public string shader; public Color tint; public string atlas;
        public Bounds bounds; public int visiblePixels; public int meshVertices = -1; public string components;
        public int nativeGeneratedVertices; public int vertexAlphaMin; public int vertexAlphaMax;
    }
    [Serializable] private sealed class MenuLabel { public string text; public string font; public string material; public float characterSize; public Color color; public Bounds nativeBounds; public int visiblePixels; }
    [Serializable] private sealed class Report { public string unityVersion; public bool passed = true; public int verifiedNativeGlyphAdvances; public string newStaticFontDefaults; public List<Sample> samples = new List<Sample>(); public List<MenuLabel> menuLabels = new List<MenuLabel>(); }
    public static void Run()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Report report = new Report { unityVersion = Application.unityVersion };
        Font blank = new Font();
        SerializedObject empty = new SerializedObject(blank);
        report.newStaticFontDefaults = "pixelScale=" + empty.FindProperty("m_PixelScale").floatValue + " fontSize=" + empty.FindProperty("m_FontSize").floatValue;
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Analysis/TotemRunner"));
        File.WriteAllText(Path.Combine(output, "text-native-empty-font.json"), EditorJsonUtility.ToJson(blank, true));
        UnityEngine.Object.DestroyImmediate(blank);
        foreach (string path in new[] { "Assets/Font/berlin_sans_fb_demi_bold.asset", "Assets/Font/berlin_sans_buymenu.asset", "Assets/Font/LithosPro-Black.asset" })
        {
            Font source = AssetDatabase.LoadAssetAtPath<Font>(path);
            MatchCollection expectedAdvances = Regex.Matches(File.ReadAllText(path), @"(?m)^    advance: (\d+)");
            CharacterInfo[] originalInfos = source.characterInfo;
            if (expectedAdvances.Count != originalInfos.Length) throw new InvalidOperationException("native glyph count differs");
            for (int i = 0; i < originalInfos.Length; i++)
            {
                CharacterInfo actual;
                if (!source.GetCharacterInfo((char)originalInfos[i].index, out actual) || actual.advance != int.Parse(expectedAdvances[i].Groups[1].Value))
                    throw new InvalidOperationException("native glyph advance mismatch: " + source.name + " " + originalInfos[i].index);
                report.verifiedNativeGlyphAdvances++;
            }
            File.WriteAllText(Path.Combine(output, "text-native-loaded-" + source.name + ".json"), EditorJsonUtility.ToJson(source, true));
            for (int variant = 0; variant < 4; variant++)
            {
                Font font = UnityEngine.Object.Instantiate(source);
                SerializedObject serialized = new SerializedObject(font);
                if (variant != 0) serialized.FindProperty("m_PixelScale").floatValue = .1f;
                if (variant >= 2) serialized.FindProperty("m_Tracking").floatValue = 1f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                if (variant == 3)
                {
                    CharacterInfo[] infos = font.characterInfo;
                    MatchCollection advances = Regex.Matches(File.ReadAllText(path), @"(?m)^    advance: (\d+)");
                    if (advances.Count != infos.Length) throw new InvalidOperationException("advance count");
                    for (int i = 0; i < infos.Length; i++) infos[i].advance = int.Parse(advances[i].Groups[1].Value);
                    font.characterInfo = infos;
                    File.WriteAllText(Path.Combine(output, "text-native-rebuilt-" + source.name + ".json"), EditorJsonUtility.ToJson(font, true));
                }
                CharacterInfo glyph;
                bool hasA = font.GetCharacterInfo('A', out glyph);
                GameObject go = new GameObject("native text");
                TextMesh text = go.AddComponent<TextMesh>();
                text.font = font; text.fontSize = 0; text.characterSize = 18f; text.anchor = TextAnchor.MiddleCenter; text.text = "PLAY"; text.color = Color.white;
                MeshRenderer renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = font.material;
                GameObject cameraGo = new GameObject("native text probe camera");
                Camera camera = cameraGo.AddComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = 180; camera.nearClipPlane = .1f; camera.farClipPlane = 1000;
                camera.transform.position = new Vector3(0, 0, -500); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
                RenderTexture rt = new RenderTexture(512, 256, 24); camera.targetTexture = rt; camera.Render();
                Texture2D image = new Texture2D(512, 256, TextureFormat.RGBA32, false);
                RenderTexture previous = RenderTexture.active; RenderTexture.active = rt; image.ReadPixels(new Rect(0, 0, 512, 256), 0, 0); image.Apply(); RenderTexture.active = previous;
                int visible = 0; foreach (Color32 pixel in image.GetPixels32()) if (pixel.r > 8 || pixel.g > 8 || pixel.b > 8) visible++;
                MeshFilter filter = go.GetComponent<MeshFilter>();
                List<string> componentNames = new List<string>(); foreach (Component component in go.GetComponents<Component>()) componentNames.Add(component.GetType().FullName);
                Material material = renderer.sharedMaterial;
                Sample sample = new Sample { font = source.name, variant = variant.ToString(), glyphCount = font.characterInfo.Length, hasA = hasA, advance = glyph.advance,
                    minGlyph = new Vector2(glyph.minX, glyph.minY), maxGlyph = new Vector2(glyph.maxX, glyph.maxY), fontSize = font.fontSize,
                    pixelScale = serialized.FindProperty("m_PixelScale").floatValue, material = material != null ? material.name : "null", shader = material != null && material.shader != null ? material.shader.name : "null",
                    tint = material != null && material.HasProperty("_Color") ? material.color : Color.clear, atlas = material != null && material.mainTexture != null ? material.mainTexture.name : "null",
                    bounds = renderer.bounds, visiblePixels = visible, meshVertices = filter != null && filter.sharedMesh != null ? filter.sharedMesh.vertexCount : -1, components = string.Join(", ", componentNames.ToArray()) };
                report.samples.Add(sample);
                TextGenerationSettings settings = new TextGenerationSettings { font = font, color = Color.white, fontSize = 0, scaleFactor = 1f, lineSpacing = 1f, textAnchor = TextAnchor.MiddleCenter, generationExtents = new Vector2(1000,1000), generateOutOfBounds = true, updateBounds = true };
                using (TextGenerator generator = new TextGenerator())
                {
                    if (!generator.Populate("PLAY", settings)) throw new InvalidOperationException("native TextGenerator failed");
                    sample.nativeGeneratedVertices = generator.vertexCount;
                    sample.vertexAlphaMin = 255; sample.vertexAlphaMax = 0;
                    foreach (UIVertex vertex in generator.verts)
                    {
                        sample.vertexAlphaMin = Mathf.Min(sample.vertexAlphaMin, vertex.color.a);
                        sample.vertexAlphaMax = Mathf.Max(sample.vertexAlphaMax, vertex.color.a);
                    }
                }
                report.passed &= sample.visiblePixels > 0 && sample.bounds.extents.x > 0 && sample.bounds.extents.y > 0 && sample.vertexAlphaMax == 255;
                File.WriteAllBytes(Path.Combine(output, "text-native-" + source.name + "-" + variant + ".png"), image.EncodeToPNG());
                camera.targetTexture = null; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(cameraGo); UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(font);
            }
        }
        // Read the real menu labels and clone only their text renderers; no
        // gameplay Awake callbacks or original scene activations are needed.
        EditorSceneManager.OpenScene("Assets/Levels/MainMenu.unity", OpenSceneMode.Single);
        foreach (TextMesh original in Resources.FindObjectsOfTypeAll<TextMesh>())
        {
            if (!original.gameObject.scene.IsValid() || (original.text != "Play" && original.text != "Options" && original.text != "Store")) continue;
            GameObject clone = new GameObject("temporary original menu label");
            TextMesh text = clone.AddComponent<TextMesh>();
            text.font = original.font; text.fontSize = original.fontSize; text.fontStyle = original.fontStyle; text.characterSize = original.characterSize;
            text.anchor = original.anchor; text.alignment = original.alignment; text.lineSpacing = original.lineSpacing; text.tabSize = original.tabSize; text.richText = original.richText; text.color = original.color; text.text = original.text;
            clone.transform.localScale = original.transform.lossyScale;
            MeshRenderer renderer = clone.GetComponent<MeshRenderer>(); renderer.sharedMaterial = original.GetComponent<MeshRenderer>().sharedMaterial;
            // Isolate the label from original menu geometry using its own layer.
            clone.layer = 31;
            GameObject cameraGo = new GameObject("original label probe camera"); Camera camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 90f; camera.nearClipPlane = .1f; camera.farClipPlane = 1000; camera.cullingMask = 1 << 31;
            camera.transform.position = new Vector3(0, 0, -500); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            RenderTexture rt = new RenderTexture(512, 256, 24); camera.targetTexture = rt; camera.Render();
            Texture2D image = new Texture2D(512, 256, TextureFormat.RGBA32, false); RenderTexture previous = RenderTexture.active; RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, 512, 256), 0, 0); image.Apply(); RenderTexture.active = previous;
            int pixels = 0; foreach (Color32 pixel in image.GetPixels32()) if (pixel.r > 8 || pixel.g > 8 || pixel.b > 8) pixels++;
            report.menuLabels.Add(new MenuLabel { text = text.text, font = text.font.name, material = renderer.sharedMaterial.name, characterSize = text.characterSize, color = text.color, nativeBounds = renderer.bounds, visiblePixels = pixels });
            report.passed &= pixels > 0 && renderer.bounds.extents.x > 0 && renderer.bounds.extents.y > 0;
            File.WriteAllBytes(Path.Combine(output, "text-native-menu-" + text.text + "-" + report.menuLabels.Count + ".png"), image.EncodeToPNG());
            camera.targetTexture = null; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(cameraGo); UnityEngine.Object.DestroyImmediate(clone);
        }
        File.WriteAllText(Path.Combine(output, "text-native-probe.json"), JsonUtility.ToJson(report, true));
        Debug.Log("TOTEM_TEXT_NATIVE_PROBE_DONE " + report.passed + " " + report.samples.Count + " glyphs=" + report.verifiedNativeGlyphAdvances + " menuLabels=" + report.menuLabels.Count);
        if (!report.passed) throw new InvalidOperationException("Text rendering checks failed");
    }
}
