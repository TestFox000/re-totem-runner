using System;
using UnityEngine;

public class Sprite3D : MonoBehaviour
{
	[NonSerialized]
	public Mesh mesh;

	public Material material;

	public Vector2 spriteSize = Vector2.one * 100f;

	public Vector2 tileSize = Vector2.one;

	protected Matrix4x4 spriteTransform = Matrix4x4.identity;

	private void Awake()
	{
		InitSprite3D();
	}

	public virtual void InitSprite3D()
	{
		spriteTransform = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(spriteSize.x, spriteSize.y, 1f));
		BuildMesh();
	}

	private void ClampV(float top, float bottom)
	{
		float num = 1f - top;
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-0.5f, num, 0f),
			new Vector3(-0.5f, bottom, 0f),
			new Vector3(0.5f, num, 0f),
			new Vector3(0.5f, bottom, 0f)
		};
		mesh.vertices = vertices;
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, tileSize.y * num),
			new Vector2(0f, tileSize.y * bottom),
			new Vector2(tileSize.x, tileSize.y * num),
			new Vector2(tileSize.x, tileSize.y * bottom)
		};
		mesh.uv = uv;
	}

	public virtual void Update()
	{
		Graphics.DrawMesh(mesh, spriteTransform, material, base.gameObject.layer);
	}

	public virtual void BuildMesh()
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
			new Vector2(0f, tileSize.y),
			new Vector2(0f, 0f),
			new Vector2(tileSize.x, tileSize.y),
			new Vector2(tileSize.x, 0f)
		};
		mesh.uv = uv;
		int[] triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
	}

	private Bounds GetBounds(Rect r)
	{
		Vector3 center = new Vector3((r.xMin + r.xMax) * 0.5f, (r.yMin + r.yMax) * 0.5f, 0f);
		Vector3 size = new Vector3(r.width, r.height, 1f);
		return new Bounds(center, size);
	}
}
