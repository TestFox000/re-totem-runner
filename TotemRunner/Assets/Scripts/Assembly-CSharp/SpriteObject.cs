using System;
using UnityEngine;

public class SpriteObject : AfterCameraUpdated
{
	[NonSerialized]
	public Mesh mesh;

	public Rect rect = new Rect(-100f, -100f, 200f, 200f);

	public Material material;

	public int drawLevel = 25;

	public bool rebuildMeshForEditor;

	public override void AwakeAfterCamera(Transform camTransform)
	{
		BuildMesh();
	}

	public override void UpdateAfterCamera()
	{
	}

	public override void RenderByOrder()
	{
		if (material != null)
		{
			material.SetPass(0);
		}
		if (mesh != null)
		{
			Graphics.DrawMeshNow(mesh, base.transform.localToWorldMatrix);
		}
	}

	public virtual void BuildMesh()
	{
		mesh = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(rect.xMin, rect.yMax, 0f),
			new Vector3(rect.xMin, rect.yMin, 0f),
			new Vector3(rect.xMax, rect.yMax, 0f),
			new Vector3(rect.xMax, rect.yMin, 0f)
		};
		mesh.vertices = vertices;
		Vector2[] uv = new Vector2[4]
		{
			Vector2.up,
			Vector2.zero,
			Vector2.one,
			Vector2.right
		};
		mesh.uv = uv;
		int[] triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
	}
}
