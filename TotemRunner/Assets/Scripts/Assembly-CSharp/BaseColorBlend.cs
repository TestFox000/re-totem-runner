using UnityEngine;

public class BaseColorBlend : MonoBehaviour
{
	public Color minColor = Color.black;

	public Color maxColor = Color.white;

	public float blendFactor;

	public Renderer refRenderer;

	public int materialIndex;

	protected Material mat;

	public float BlendFactor
	{
		get
		{
			return blendFactor;
		}
		set
		{
			blendFactor = value;
			if (mat != null)
			{
				mat.color = Color.Lerp(minColor, maxColor, blendFactor);
			}
		}
	}

	public virtual void Awake()
	{
		if (!refRenderer)
		{
			refRenderer = base.GetComponent<Renderer>();
		}
		if (materialIndex >= 0 && refRenderer != null && refRenderer.materials != null && refRenderer.materials.Length > materialIndex)
		{
			mat = refRenderer.materials[materialIndex];
			mat.color = minColor;
		}
	}

	public virtual void ResetToStart()
	{
		BlendFactor = 0f;
	}
}
