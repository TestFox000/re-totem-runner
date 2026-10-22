using UnityEngine;

public class SunObject : SpriteObject
{
	public float overlaySizePercent = 100f;

	public Vector3 offset = new Vector3(0f, -64f, -100f);

	public Material overlayMaterial;

	public int overlayOrder = 80;

	private Mesh sunOverlayMesh;

	public override void AwakeAfterCamera(Transform camTransform)
	{
		base.AwakeAfterCamera(camTransform);
		sunOverlayMesh = new Mesh();
		float num = overlaySizePercent * rect.width * 0.01f;
		float num2 = overlaySizePercent * rect.height * 0.01f;
		Vector3[] array = new Vector3[4];
		Vector3 vector = default;
		vector.x = (rect.xMax + rect.xMin - num) * 0.5f;
		vector.y = (rect.yMax + rect.yMin + num2) * 0.5f;
		vector.z = 0f;
		array[0] = vector;
		vector.y -= num2;
		array[1] = vector;
		vector.x += num;
		array[2] = vector;
		vector.y += num2;
		array[3] = vector;
		sunOverlayMesh.vertices = array;
		Vector2[] uv = new Vector2[4]
		{
			Vector2.up,
			Vector2.zero,
			Vector2.right,
			Vector2.one
		};
		sunOverlayMesh.uv = uv;
		int[] triangles = new int[6] { 0, 3, 1, 1, 3, 2 };
		sunOverlayMesh.triangles = triangles;
		sunOverlayMesh.RecalculateBounds();
	}

	public override int GetSecondOrder()
	{
		return overlayOrder;
	}

	public override void RenderByOrderSecond()
	{
		if (overlayMaterial != null)
		{
			overlayMaterial.SetPass(0);
		}
		if (sunOverlayMesh != null)
		{
			Graphics.DrawMeshNow(sunOverlayMesh, base.transform.position + offset, base.transform.rotation);
		}
	}
}
