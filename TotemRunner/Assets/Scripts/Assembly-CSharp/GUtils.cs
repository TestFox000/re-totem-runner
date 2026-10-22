using UnityEngine;

public class GUtils : MonoBehaviour
{
	public static void LoadPictImage(Renderer rndr, TextAsset imgFile, ref Texture2D texRef)
	{
		if (texRef != null)
		{
			Object.Destroy(texRef);
		}
		texRef = new Texture2D(4, 4, TextureFormat.RGB24, mipChain: false);
		texRef.LoadImage(imgFile.bytes);
		Material material = rndr.material;
		material.mainTexture = texRef;
		rndr.material = material;
	}

	public static void SetRendererAlpha(Renderer r, float a)
	{
		Color color = r.material.color;
		color.a = a;
		r.material.color = color;
	}

	public static void SetMaterialAlpha(Material m, float a)
	{
		Color color = m.color;
		color.a = a;
		m.color = color;
	}

	public static void SetLocalScaleX(Transform t, float scaleX)
	{
		Vector3 localScale = t.localScale;
		localScale.x = scaleX;
		t.localScale = localScale;
	}

	public static void SetLocalScale2D(Transform t, float scaleX, float scaleY)
	{
		Vector3 localScale = t.localScale;
		localScale.x = scaleX;
		localScale.y = scaleY;
		t.localScale = localScale;
	}

	public static void SetTransformY(Transform t, float y)
	{
		Vector3 position = t.position;
		position.y = y;
		t.position = position;
	}

	public static void SetTransformX(Transform t, float x)
	{
		Vector3 position = t.position;
		position.x = x;
		t.position = position;
	}

	public static void SetTransform2D(Transform t, float x, float y)
	{
		Vector3 position = t.position;
		position.x = x;
		position.y = y;
		t.position = position;
	}
}
