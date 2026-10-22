using System;
using UnityEngine;

[Serializable]
public class ButterflyManager
{
	[Serializable]
	public class FlyTrack
	{
		public AnimationCurve xPos;

		public AnimationCurve yPos;

		public AnimationCurve zAngle;

		public AnimationCurve xAngle;

		public AnimationCurve scale;
	}

	private class Butterfly
	{
		public int spriteNum;

		public int trackNum;

		public float flyTimeLeft;

		public Vector2 pos;
	}

	public Vector3 offset = new Vector3(0f, 0f, 0f);

	public float ranomOffsetX = 30f;

	public float flyTime = 1.5f;

	private float divFlyTime;

	public float timeRisePeriod = 1f;

	public float timeRiseVariable = 0.2f;

	public float timeVariableLength = 2f;

	public float timeStartDelay = 1f;

	private float timeLastRise;

	private float timeActive;

	private int lastButterflySprite = -1;

	public FlyTrack[] tracks = new FlyTrack[2];

	private Butterfly[] butterflys = new Butterfly[6];

	private Mesh mesh;

	public Material material;

	public void SetMaterial(Material mat)
	{
		material = mat;
	}

	public void InitButterfly(Bounds bounds)
	{
		mesh = new Mesh();
		mesh.vertices = new Vector3[24];
		mesh.uv = new Vector2[24];
		mesh.SetTriangles(new int[36]
		{
			0, 2, 1, 1, 2, 3, 4, 6, 5, 5,
			6, 7, 8, 10, 9, 9, 10, 11, 12, 14,
			13, 13, 14, 15, 16, 18, 17, 17, 18, 19,
			20, 22, 21, 21, 22, 23
		}, 0);
		divFlyTime = 1f / flyTime;
		timeLastRise = 0f;
		timeActive = 0f;
		mesh.bounds = bounds;
	}

	private void GetCornersFromTrack(float time, int nTrack, Vector2 pos, Vector3[] corners)
	{
		FlyTrack flyTrack = tracks[nTrack];
		Vector3 vector = new Vector3(pos.x + flyTrack.xPos.Evaluate(time), pos.y + flyTrack.yPos.Evaluate(time), offset.z);
		float f = flyTrack.zAngle.Evaluate(time);
		float num = flyTrack.scale.Evaluate(time);
		float num2 = Mathf.Cos(f);
		float num3 = Mathf.Sin(f);
		Vector3 vector2 = new Vector3((0f - num3) * num, num2 * num, 0f);
		Vector3 vector3 = new Vector3(vector2.y, 0f - vector2.x, 0f);
		float num4 = Mathf.Cos(flyTrack.xAngle.Evaluate(time) * ((float)Math.PI / 180f));
		vector3.x *= num4;
		vector2.x *= num4;
		ref Vector3 reference = ref corners[0];
		reference = vector - vector3 + vector2;
		ref Vector3 reference2 = ref corners[1];
		reference2 = vector - vector3 - vector2;
		ref Vector3 reference3 = ref corners[2];
		reference3 = vector + vector3 + vector2;
		ref Vector3 reference4 = ref corners[3];
		reference4 = vector + vector3 - vector2;
	}

	public void UpdateButterfly(float x, float y, bool isActive)
	{
		Vector3[] vertices = mesh.vertices;
		Vector2[] array = null;
		timeLastRise += Time.deltaTime;
		if (isActive)
		{
			timeActive += Time.deltaTime;
			if (timeLastRise >= timeRisePeriod)
			{
				float f = timeActive / timeVariableLength;
				timeLastRise = Mathf.Lerp(0f - timeRiseVariable, 0f, Mathf.Log(f, 2f));
				int num = FindEmptyButterflySlot();
				if (num >= 0)
				{
					Butterfly butterfly = new Butterfly();
					butterfly.spriteNum = UnityEngine.Random.Range(0, 2);
					if (lastButterflySprite == butterfly.spriteNum)
					{
						butterfly.spriteNum++;
					}
					lastButterflySprite = butterfly.spriteNum;
					butterfly.trackNum = UnityEngine.Random.Range(0, 2);
					butterfly.flyTimeLeft = flyTime + Time.deltaTime;
					butterfly.pos = new Vector2(x + offset.x + UnityEngine.Random.Range(0f, ranomOffsetX), y + offset.y);
					butterflys[num] = butterfly;
					int num2 = num * 4;
					array = mesh.uv;
					float num3 = 0f;
					float num4 = 0.5f;
					if (butterfly.spriteNum == 2)
					{
						num4 = 0f;
					}
					if (butterfly.spriteNum == 1)
					{
						num3 = 0.5f;
					}
					ref Vector2 reference = ref array[num2++];
					reference = new Vector2(num3, num4 + 0.5f);
					ref Vector2 reference2 = ref array[num2++];
					reference2 = new Vector2(num3, num4);
					ref Vector2 reference3 = ref array[num2++];
					reference3 = new Vector2(num3 + 0.5f, num4 + 0.5f);
					ref Vector2 reference4 = ref array[num2++];
					reference4 = new Vector2(num3 + 0.5f, num4);
				}
			}
		}
		else
		{
			timeLastRise = 0f - timeStartDelay;
			timeActive = 0f;
		}
		Vector3[] array2 = new Vector3[4];
		for (int i = 0; i < butterflys.Length; i++)
		{
			Butterfly butterfly2 = butterflys[i];
			if (butterfly2 != null)
			{
				int num2 = i * 4;
				butterfly2.flyTimeLeft -= Time.deltaTime;
				if (butterfly2.flyTimeLeft <= 0f)
				{
					butterflys[i] = null;
					ref Vector3 reference5 = ref vertices[num2++];
					reference5 = Vector3.zero;
					ref Vector3 reference6 = ref vertices[num2++];
					reference6 = Vector3.zero;
					ref Vector3 reference7 = ref vertices[num2++];
					reference7 = Vector3.zero;
					ref Vector3 reference8 = ref vertices[num2++];
					reference8 = Vector3.zero;
				}
				else
				{
					GetCornersFromTrack(Mathf.Clamp01(1f - butterfly2.flyTimeLeft * divFlyTime), butterfly2.trackNum, butterfly2.pos, array2);
					ref Vector3 reference9 = ref vertices[num2++];
					reference9 = array2[0];
					ref Vector3 reference10 = ref vertices[num2++];
					reference10 = array2[1];
					ref Vector3 reference11 = ref vertices[num2++];
					reference11 = array2[2];
					ref Vector3 reference12 = ref vertices[num2++];
					reference12 = array2[3];
				}
			}
		}
		mesh.vertices = vertices;
		if (array != null)
		{
			mesh.uv = array;
		}
	}

	public void DrawButterflys()
	{
		if ((bool)mesh && mesh.vertexCount > 0)
		{
			material.SetPass(0);
			Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
		}
	}

	private int FindEmptyButterflySlot()
	{
		for (int i = 0; i < butterflys.Length; i++)
		{
			if (butterflys[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

	public void ResetToX(float x)
	{
	}
}
