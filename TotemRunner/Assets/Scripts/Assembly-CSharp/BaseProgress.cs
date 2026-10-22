using UnityEngine;

public class BaseProgress : InnerRenderingObject
{
	public float progressValue = 1f;

	public virtual void Start()
	{
		float progress = progressValue;
		progressValue = -1f;
		SetProgress(progress);
	}

	public virtual void SetProgress(float progressVal)
	{
		progressValue = progressVal;
	}

	protected Mesh CreateSpriteMesh()
	{
		Mesh mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		mesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
		return mesh;
	}
}
