using UnityEngine;

// Keeps legacy GUIText fileIDs and the text/material API used by the recovered HUD.
public sealed class GUIText : MonoBehaviour
{
    [TextArea] public string m_Text = "";
    public int m_Anchor;
    public int m_Alignment;
    public Vector2 m_PixelOffset;
    public float m_LineSpacing = 1f;
    public float m_TabSize = 4f;
    public Font m_Font;
    public Material m_Material;
    public int m_FontSize;
    public FontStyle m_FontStyle;
    public Color m_Color = Color.white;
    public bool m_PixelCorrect = true;
    public bool m_RichText;

    private Material ownedMaterial;
    private Font fallbackFont;
    private GUIStyle style;

    public string text
    {
        get { return m_Text; }
        set { m_Text = value ?? ""; }
    }

    public Material material
    {
        get
        {
            if (m_Material != null)
                return m_Material;
            if (ownedMaterial == null)
            {
                Shader shader = Shader.Find("UI/Default");
                if (shader == null)
                    shader = Shader.Find("Hidden/Internal-Colored");
                ownedMaterial = new Material(shader);
                ownedMaterial.name = "Legacy GUIText Color";
                ownedMaterial.hideFlags = HideFlags.HideAndDontSave;
                ownedMaterial.color = Color.white;
            }
            return ownedMaterial;
        }
        set { m_Material = value; }
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(m_Text))
            return;
        if (style == null)
            style = new GUIStyle(GUI.skin.label);
        if (fallbackFont == null && m_Font == null)
            fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        style.font = m_Font != null ? m_Font : fallbackFont;
        style.fontSize = m_FontSize > 0 ? m_FontSize : 14;
        style.fontStyle = m_FontStyle;
        style.richText = m_RichText;
        style.wordWrap = false;
        style.clipping = TextClipping.Overflow;
        style.padding = new RectOffset(0, 0, 0, 0);
        style.margin = new RectOffset(0, 0, 0, 0);
        style.alignment = (TextAnchor)(Mathf.Clamp(m_Anchor / 3, 0, 2) * 3 + Mathf.Clamp(m_Alignment, 0, 2));
        style.normal.textColor = Color.white;

        string display = m_Text.Replace("\t", new string(' ', Mathf.Max(1, Mathf.RoundToInt(m_TabSize))));
        GUIContent content = new GUIContent(display);
        Vector2 size = style.CalcSize(content);
        Vector3 position = transform.position;
        float x = position.x * Screen.width + m_PixelOffset.x;
        float y = (1f - position.y) * Screen.height - m_PixelOffset.y;
        int anchor = Mathf.Clamp(m_Anchor, 0, 8);
        x -= size.x * (anchor % 3) * 0.5f;
        y -= size.y * (anchor / 3) * 0.5f;
        if (m_PixelCorrect)
        {
            x = Mathf.Round(x);
            y = Mathf.Round(y);
        }

        Color previousColor = GUI.color;
        int previousDepth = GUI.depth;
        // Unity 3.5 GUIText did not serialize the newer m_Color field. AssetRipper
        // initializes it to zero in every recovered scene; use its material tint.
        Color tint = m_Color.a == 0f ? Color.white : m_Color;
        GUI.color = previousColor * tint * material.color;
        GUI.depth = -Mathf.RoundToInt(position.z * 1000f);
        GUI.Label(new Rect(x, y, Mathf.Max(1f, size.x), Mathf.Max(1f, size.y)), content, style);
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private void OnDestroy()
    {
        if (ownedMaterial != null)
            Destroy(ownedMaterial);
    }
}
