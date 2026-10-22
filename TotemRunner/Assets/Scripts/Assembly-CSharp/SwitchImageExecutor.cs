using UnityEngine;

public class SwitchImageExecutor : EventExecutor
{
	public Texture2D[] textures;

	public float switchTimeout = 0.5f;

	public Renderer refRenderer;

	public Shader shaderForSwitcher;

	private int curIndex;

	private float switchTimeMul;

	private float processingTime = -1f;

	private Material storedMaterial;

	private void Awake()
	{
		if (switchTimeout > 0.001f)
		{
			switchTimeMul = 1f / switchTimeout;
		}
		if (refRenderer == null)
		{
			refRenderer = base.GetComponent<Renderer>();
		}
	}

	public override void StartExecutor()
	{
		if (processingTime >= 0f)
		{
			return;
		}
		if (refRenderer != null)
		{
			storedMaterial = refRenderer.material;
			processingTime = switchTimeout;
			Material material = new Material(shaderForSwitcher);
			material.SetTexture("_MainTex", storedMaterial.mainTexture);
			curIndex++;
			if (curIndex >= textures.Length)
			{
				curIndex = 0;
			}
			material.SetTexture("_BlendTex", textures[curIndex]);
			material.SetFloat("_Blend", 0f);
			refRenderer.material = material;
		}
		base.StartExecutor();
	}

	private void Update()
	{
		if (processingTime >= 0f)
		{
			processingTime -= Time.deltaTime;
			if (processingTime <= 0f)
			{
				processingTime = -1f;
				storedMaterial.mainTexture = textures[curIndex];
				refRenderer.material = storedMaterial;
				storedMaterial = null;
			}
			else
			{
				float value = Mathf.Clamp01(1f - processingTime * switchTimeMul);
				refRenderer.material.SetFloat("_Blend", value);
			}
		}
	}
}
