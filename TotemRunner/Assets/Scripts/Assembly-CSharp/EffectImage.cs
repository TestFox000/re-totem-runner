using System;
using UnityEngine;

public class EffectImage : MonoBehaviour
{
	[Serializable]
	public class EffectImageDescribe
	{
		public Material material;

		public float lifeTime;

		public float delayTime;

		public float startSize;

		public float endSize;

		public float startAngle;

		public float endAngle;
	}

	public EffectImageDescribe[] images;

	public int drawLevel = 10;

	private Mesh mesh;

	private void Awake()
	{
		BuildMesh();
	}

	private void Update()
	{
		EffectImageDescribe[] array = images;
		foreach (EffectImageDescribe effectImageDescribe in array)
		{
			Graphics.DrawMesh(mesh, base.transform.position, base.transform.rotation, effectImageDescribe.material, drawLevel);
		}
	}

	private void BuildMesh()
	{
		mesh = new Mesh();
		mesh.vertices = new Vector3[4 * images.Length];
		mesh.uv = new Vector2[4 * images.Length];
		int[] triangles = new int[6];
		mesh.triangles = triangles;
	}
}
