using UnityEngine;

public class Sprite3DWithDrawLevel : AfterCameraUpdated
{
	public Mesh mesh;

	public Material material;

	public Vector2 spriteSize = Vector2.one;

	public Transform refParentTransform;

	private Matrix4x4 spriteTransform = Matrix4x4.identity;

	private void Awake()
	{
		if (!refParentTransform)
		{
			refParentTransform = base.transform;
		}
		RecalculateTransform();
		if (mesh == null)
		{
			BuildMesh();
		}
	}

	public override void UpdateAfterCamera()
	{
		if (base.enabled)
		{
			RecalculateTransform();
		}
	}

	public void SetRefParentTransform(Transform refTransform)
	{
		refParentTransform = refTransform;
	}

	private void RecalculateTransform()
	{
		Vector3 lossyScale = refParentTransform.lossyScale;
		lossyScale.x *= spriteSize.x;
		lossyScale.y *= spriteSize.y;
		spriteTransform = Matrix4x4.TRS(refParentTransform.position, refParentTransform.rotation, lossyScale);
	}

	public override void RenderByOrder()
	{
		if (base.enabled)
		{
			material.SetPass(0);
			Graphics.DrawMeshNow(mesh, spriteTransform);
		}
	}

	private void BuildMesh()
	{
		mesh = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-0.5f, 1f, 0f),
			new Vector3(-0.5f, 0f, 0f),
			new Vector3(0.5f, 1f, 0f),
			new Vector3(0.5f, 0f, 0f)
		};
		mesh.vertices = vertices;
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 1f),
			new Vector2(0f, 0f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		mesh.uv = uv;
		int[] triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
	}
}
