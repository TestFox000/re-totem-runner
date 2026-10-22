using UnityEngine;

public class MaterialBlendByPlayerSpeed : MonoBehaviour
{
	public Color minColor = Color.black;

	public Color maxColor = Color.white;

	public float maxSpeed = 1000f;

	public float minSpeed = 300f;

	private float blinkMul = 1f;

	public Renderer refRenderer;

	public int materialIndex;

	private Material mat;

	private void Awake()
	{
		blinkMul = 1f / (maxSpeed - minSpeed);
		if (refRenderer == null)
		{
			refRenderer = base.GetComponent<Renderer>();
		}
		mat = refRenderer.materials[materialIndex];
		mat.color = minColor;
	}

	private void Update()
	{
		float speed = PlayerController.instance.GetSpeed();
		speed -= minSpeed;
		Color color = ((!(speed <= 0f)) ? Color.Lerp(minColor, maxColor, Mathf.Clamp01(speed * blinkMul)) : minColor);
		mat.color = color;
		if (color.a < 0.1f)
		{
			refRenderer.enabled = false;
		}
		else
		{
			refRenderer.enabled = true;
		}
	}
}
