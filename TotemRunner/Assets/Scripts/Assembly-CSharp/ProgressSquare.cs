using UnityEngine;

public class ProgressSquare : BaseProgress
{
	public enum ePlacementType
	{
		horizontal_baseleft = 0,
		horizontal_baseright = 1,
		vertical_basetop = 2,
		vertical_basebottom = 3
	}

	public MeshFilter refMeshProgress;

	private Mesh progressMesh;

	public ePlacementType placement;

	public override void Start()
	{
		progressMesh = CreateSpriteMesh();
		base.Start();
	}

	public override void SetProgress(float progressVal)
	{
		if (progressValue != progressVal)
		{
			base.SetProgress(progressVal);
			if (progressMesh != null)
			{
				UpdateMesh();
			}
		}
	}

	private void UpdateMesh()
	{
		Vector3[] vertices = progressMesh.vertices;
		Vector2[] uv = progressMesh.uv;
		switch (placement)
		{
		case ePlacementType.horizontal_baseleft:
			uv[2].x = progressValue;
			uv[3].x = progressValue;
			vertices[2].x = progressValue - 0.5f;
			vertices[3].x = progressValue - 0.5f;
			break;
		case ePlacementType.horizontal_baseright:
			uv[0].x = 1f - progressValue;
			uv[1].x = 1f - progressValue;
			vertices[0].x = 0.5f - progressValue;
			vertices[1].x = 0.5f - progressValue;
			break;
		case ePlacementType.vertical_basebottom:
			uv[1].y = progressValue;
			uv[3].y = progressValue;
			vertices[1].y = progressValue - 0.5f;
			vertices[3].y = progressValue - 0.5f;
			break;
		case ePlacementType.vertical_basetop:
			uv[0].y = 1f - progressValue;
			uv[2].y = 1f - progressValue;
			vertices[0].y = 0.5f - progressValue;
			vertices[2].y = 0.5f - progressValue;
			break;
		}
		progressMesh.vertices = vertices;
		progressMesh.uv = uv;
		if (refMeshProgress != null)
		{
			refMeshProgress.mesh = progressMesh;
		}
	}
}
