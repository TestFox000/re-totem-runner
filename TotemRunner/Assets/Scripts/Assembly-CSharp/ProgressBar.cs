using UnityEngine;

public class ProgressBar : MonoBehaviour
{
	public MeshFilter backMesh;

	public Vector2 progressOffset = Vector2.zero;

	public Material progressMaterial;

	public float startProgress;

	public float labelYOffset;

	public Vector2 labelSpriteSize = Vector2.zero;

	public Material labelInactiveMaterial;

	public Material labelActiveMaterial;

	public float[] labelOffsets;

	private MeshFilter[] labels;

	private float curProgress = -1f;

	private MeshFilter progressMesh;

	private Vector2 progressSize = Vector2.zero;

	private void Awake()
	{
		Bounds bounds = backMesh.mesh.bounds;
		progressSize.x = Mathf.Round(bounds.extents.x * backMesh.transform.lossyScale.x - progressOffset.x);
		progressSize.y = Mathf.Round(bounds.extents.y * backMesh.transform.lossyScale.y - progressOffset.y);
		progressMesh = CreateSprite("progress", progressSize, progressMaterial, -Vector3.forward);
		if (labelOffsets != null)
		{
			if (labelOffsets.Length > 0)
			{
				labels = new MeshFilter[labelOffsets.Length];
			}
			Vector3 offset = new Vector3(0f, labelYOffset, -2f);
			for (int i = 0; i < labelOffsets.Length; i++)
			{
				offset.x = labelOffsets[i] * 2f * progressSize.x - progressSize.x;
				labels[i] = CreateSprite("l" + (i + 1), labelSpriteSize, labelInactiveMaterial, offset);
			}
		}
		SetProgress(startProgress);
	}

	public float GetProgress()
	{
		return curProgress;
	}

	public void SetProgress(float newProgress)
	{
		if (labelOffsets != null)
		{
			for (int i = 0; i < labelOffsets.Length; i++)
			{
				if (newProgress >= labelOffsets[i])
				{
					if (curProgress < labelOffsets[i])
					{
						labels[i].GetComponent<Renderer>().sharedMaterial = labelActiveMaterial;
					}
				}
				else if (curProgress >= labelOffsets[i])
				{
					labels[i].GetComponent<Renderer>().sharedMaterial = labelInactiveMaterial;
				}
			}
		}
		curProgress = newProgress;
		UpdateProgress();
	}

	private MeshFilter CreateSprite(string spriteName, Vector2 size, Material mat, Vector3 offset)
	{
		GameObject gameObject = new GameObject(spriteName);
		gameObject.layer = base.gameObject.layer;
		gameObject.transform.position = base.transform.position + offset;
		gameObject.transform.parent = base.transform;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f - size.x, 0f - size.y, 0f),
			new Vector3(0f - size.x, size.y, 0f),
			new Vector3(size.x, 0f - size.y, 0f),
			new Vector3(size.x, size.y, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		mesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
		meshFilter.sharedMesh = mesh;
		Renderer renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = mat;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		return meshFilter;
	}

	private void UpdateProgress()
	{
		Vector3[] vertices = progressMesh.mesh.vertices;
		float x = Mathf.Floor(0f - progressSize.x + 2f * curProgress * progressSize.x);
		vertices[2].x = x;
		vertices[3].x = x;
		progressMesh.mesh.vertices = vertices;
		Vector2[] uv = progressMesh.mesh.uv;
		uv[2].x = curProgress;
		uv[3].x = curProgress;
		progressMesh.mesh.uv = uv;
	}
}
