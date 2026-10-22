using System;
using UnityEngine;

public class WaterSurface : AfterCameraUpdated
{
	[NonSerialized]
	public Mesh mesh;

	public Material material;

	public float farPlane = 2200f;

	public float nearPlane = 1400f;

	public float nearHeight = 0.05f;

	public float farHeight = 0.4f;

	public float[] zCutPlanes = new float[3] { 0.7f, 0.5f, 0.2f };

	public float[] speedPlanes = new float[3] { 0.003f, 0.0025f, 0.002f };

	private float[] offsetPlanes = new float[3];

	public int[] tileQuantity = new int[3] { 7, 5, 3 };

	private Camera relationCamera;

	private Vector3 baseCamPos = Vector3.zero;

	public float turnOnDistance;

	public float turnOffDistance = 100000f;

	public static float rClipBound = 1.1f;

	public static float lClipBound = -0.1f;

	public override void AwakeAfterCamera(Transform hostCamera)
	{
		relationCamera = hostCamera.GetComponent<Camera>();
		baseCamPos = hostCamera.position;
		InitSprite3D();
	}

	private void InitSprite3D()
	{
		BuildMesh();
	}

	public override void UpdateAfterCamera()
	{
		Vector3 position = relationCamera.transform.position;
		if (!(position.x < turnOnDistance) && !(position.x > turnOffDistance))
		{
			position.y = baseCamPos.y;
			position.z = baseCamPos.z;
			if (Time.deltaTime > 0f)
			{
				UpdateUVs();
			}
			Graphics.DrawMesh(mesh, position, Quaternion.identity, material, 2);
		}
	}

	public void UpdateUVs()
	{
		float num = relationCamera.transform.position.x - baseCamPos.x;
		offsetPlanes[0] = speedPlanes[0] * num;
		offsetPlanes[0] -= Mathf.Floor(offsetPlanes[0]);
		offsetPlanes[1] = speedPlanes[1] * num;
		offsetPlanes[1] -= Mathf.Floor(offsetPlanes[1]);
		offsetPlanes[2] = speedPlanes[2] * num;
		offsetPlanes[2] -= Mathf.Floor(offsetPlanes[2]);
		Vector2[] uv = mesh.uv;
		int num2 = 1;
		for (int i = 0; i < tileQuantity[0]; i++)
		{
			int num3 = (num2 + i) * 4;
			float num4 = (float)i + offsetPlanes[0];
			uv[num3].x = num4;
			uv[1 + num3].x = num4;
			uv[2 + num3].x = num4 + 1f;
			uv[3 + num3].x = num4 + 1f;
		}
		num2 += tileQuantity[0];
		for (int i = 0; i < tileQuantity[1]; i++)
		{
			int num3 = (num2 + i) * 4;
			float num4 = (float)i + offsetPlanes[1];
			uv[num3].x = num4;
			uv[1 + num3].x = num4;
			uv[2 + num3].x = num4 + 1f;
			uv[3 + num3].x = num4 + 1f;
		}
		num2 += tileQuantity[1];
		for (int i = 0; i < tileQuantity[2]; i++)
		{
			int num3 = (num2 + i) * 4;
			float num4 = (float)i + offsetPlanes[2];
			uv[num3].x = num4;
			uv[1 + num3].x = num4;
			uv[2 + num3].x = num4 + 1f;
			uv[3 + num3].x = num4 + 1f;
		}
		mesh.uv = uv;
	}

	private void BuildMesh()
	{
		int num = 1 + tileQuantity[0] + tileQuantity[1] + tileQuantity[2];
		mesh = new Mesh();
		Vector3[] array = new Vector3[4 * num];
		Vector3[] array2 = new Vector3[5];
		float[] array3 = new float[5];
		float z = relationCamera.transform.position.z;
		float y = farHeight;
		float z2 = farPlane - z;
		Vector3 vector = relationCamera.ViewportToWorldPoint(new Vector3(rClipBound, y, z2));
		Vector3 vector2 = relationCamera.ViewportToWorldPoint(new Vector3(lClipBound, y, z2));
		array3[0] = vector.x - vector2.x;
		ref Vector3 reference = ref array2[0];
		reference = (vector2 + vector) * 0.5f;
		y = nearHeight + (farHeight - nearHeight) * zCutPlanes[0];
		z2 = nearPlane + (farPlane - nearPlane) * zCutPlanes[0] - z;
		vector = relationCamera.ViewportToWorldPoint(new Vector3(rClipBound, y, z2));
		vector2 = relationCamera.ViewportToWorldPoint(new Vector3(lClipBound, y, z2));
		array3[1] = vector.x - vector2.x;
		ref Vector3 reference2 = ref array2[1];
		reference2 = (vector2 + vector) * 0.5f;
		y = nearHeight + (farHeight - nearHeight) * zCutPlanes[1];
		z2 = nearPlane + (farPlane - nearPlane) * zCutPlanes[1] - z;
		vector = relationCamera.ViewportToWorldPoint(new Vector3(rClipBound, y, z2));
		vector2 = relationCamera.ViewportToWorldPoint(new Vector3(lClipBound, y, z2));
		array3[2] = vector.x - vector2.x;
		ref Vector3 reference3 = ref array2[2];
		reference3 = (vector2 + vector) * 0.5f;
		y = nearHeight + (farHeight - nearHeight) * zCutPlanes[2];
		z2 = nearPlane + (farPlane - nearPlane) * zCutPlanes[2] - z;
		vector = relationCamera.ViewportToWorldPoint(new Vector3(rClipBound, y, z2));
		vector2 = relationCamera.ViewportToWorldPoint(new Vector3(lClipBound, y, z2));
		array3[3] = vector.x - vector2.x;
		ref Vector3 reference4 = ref array2[3];
		reference4 = (vector2 + vector) * 0.5f;
		y = nearHeight;
		z2 = nearPlane - z;
		vector = relationCamera.ViewportToWorldPoint(new Vector3(rClipBound, y, z2));
		vector2 = relationCamera.ViewportToWorldPoint(new Vector3(lClipBound, y, z2));
		array3[4] = vector.x - vector2.x;
		ref Vector3 reference5 = ref array2[4];
		reference5 = (vector2 + vector) * 0.5f;
		ref Vector3 reference6 = ref array[0];
		reference6 = new Vector3(array2[0].x - array3[0] * 0.5f, array2[0].y, array2[0].z);
		ref Vector3 reference7 = ref array[1];
		reference7 = new Vector3(array2[1].x - array3[1] * 0.5f, array2[1].y, array2[1].z);
		ref Vector3 reference8 = ref array[2];
		reference8 = new Vector3(array2[0].x + array3[0] * 0.5f, array2[0].y, array2[0].z);
		ref Vector3 reference9 = ref array[3];
		reference9 = new Vector3(array2[1].x + array3[1] * 0.5f, array2[1].y, array2[1].z);
		int num2 = 1;
		float num3 = array3[1] / (float)tileQuantity[0];
		float num4 = (0f - array3[1]) * 0.5f;
		float num5 = array3[2] / (float)tileQuantity[0];
		float num6 = (0f - array3[2]) * 0.5f;
		for (int i = 0; i < tileQuantity[0]; i++)
		{
			int num7 = (num2 + i) * 4;
			float num8 = num4 + num3;
			float num9 = num6 + num5;
			ref Vector3 reference10 = ref array[num7];
			reference10 = new Vector3(array2[1].x + num4, array2[1].y, array2[1].z);
			ref Vector3 reference11 = ref array[1 + num7];
			reference11 = new Vector3(array2[2].x + num6, array2[2].y, array2[2].z);
			ref Vector3 reference12 = ref array[2 + num7];
			reference12 = new Vector3(array2[1].x + num8, array2[1].y, array2[1].z);
			ref Vector3 reference13 = ref array[3 + num7];
			reference13 = new Vector3(array2[2].x + num9, array2[2].y, array2[2].z);
			num4 = num8;
			num6 = num9;
		}
		num2 += tileQuantity[0];
		num3 = array3[2] / (float)tileQuantity[1];
		num4 = (0f - array3[2]) * 0.5f;
		num5 = array3[3] / (float)tileQuantity[1];
		num6 = (0f - array3[3]) * 0.5f;
		for (int i = 0; i < tileQuantity[1]; i++)
		{
			int num7 = (num2 + i) * 4;
			float num8 = num4 + num3;
			float num9 = num6 + num5;
			ref Vector3 reference14 = ref array[num7];
			reference14 = new Vector3(array2[2].x + num4, array2[2].y, array2[2].z);
			ref Vector3 reference15 = ref array[1 + num7];
			reference15 = new Vector3(array2[3].x + num6, array2[3].y, array2[3].z);
			ref Vector3 reference16 = ref array[2 + num7];
			reference16 = new Vector3(array2[2].x + num8, array2[2].y, array2[2].z);
			ref Vector3 reference17 = ref array[3 + num7];
			reference17 = new Vector3(array2[3].x + num9, array2[3].y, array2[3].z);
			num4 = num8;
			num6 = num9;
		}
		num2 += tileQuantity[1];
		num3 = array3[3] / (float)tileQuantity[2];
		num4 = (0f - array3[3]) * 0.5f;
		num5 = array3[4] / (float)tileQuantity[2];
		num6 = (0f - array3[4]) * 0.5f;
		for (int i = 0; i < tileQuantity[2]; i++)
		{
			int num7 = (num2 + i) * 4;
			float num8 = num4 + num3;
			float num9 = num6 + num5;
			ref Vector3 reference18 = ref array[num7];
			reference18 = new Vector3(array2[3].x + num4, array2[3].y, array2[3].z);
			ref Vector3 reference19 = ref array[1 + num7];
			reference19 = new Vector3(array2[4].x + num6, array2[4].y, array2[4].z);
			ref Vector3 reference20 = ref array[2 + num7];
			reference20 = new Vector3(array2[3].x + num8, array2[3].y, array2[3].z);
			ref Vector3 reference21 = ref array[3 + num7];
			reference21 = new Vector3(array2[4].x + num9, array2[4].y, array2[4].z);
			num4 = num8;
			num6 = num9;
		}
		mesh.vertices = array;
		Vector2[] array4 = new Vector2[4 * num];
		ref Vector2 reference22 = ref array4[0];
		reference22 = Vector2.up;
		ref Vector2 reference23 = ref array4[1];
		reference23 = new Vector2(0f, zCutPlanes[0]);
		ref Vector2 reference24 = ref array4[2];
		reference24 = Vector2.one;
		ref Vector2 reference25 = ref array4[3];
		reference25 = new Vector2(1f, zCutPlanes[0]);
		num2 = 1;
		for (int i = 0; i < tileQuantity[0]; i++)
		{
			int num7 = (num2 + i) * 4;
			ref Vector2 reference26 = ref array4[num7];
			reference26 = new Vector2(i, zCutPlanes[0]);
			ref Vector2 reference27 = ref array4[1 + num7];
			reference27 = new Vector2(i, zCutPlanes[1]);
			ref Vector2 reference28 = ref array4[2 + num7];
			reference28 = new Vector2((float)i + 1f, zCutPlanes[0]);
			ref Vector2 reference29 = ref array4[3 + num7];
			reference29 = new Vector2((float)i + 1f, zCutPlanes[1]);
		}
		num2 += tileQuantity[0];
		for (int i = 0; i < tileQuantity[1]; i++)
		{
			int num7 = (num2 + i) * 4;
			ref Vector2 reference30 = ref array4[num7];
			reference30 = new Vector2(i, zCutPlanes[1]);
			ref Vector2 reference31 = ref array4[1 + num7];
			reference31 = new Vector2(i, zCutPlanes[2]);
			ref Vector2 reference32 = ref array4[2 + num7];
			reference32 = new Vector2((float)i + 1f, zCutPlanes[1]);
			ref Vector2 reference33 = ref array4[3 + num7];
			reference33 = new Vector2((float)i + 1f, zCutPlanes[2]);
		}
		num2 += tileQuantity[1];
		for (int i = 0; i < tileQuantity[2]; i++)
		{
			int num7 = (num2 + i) * 4;
			ref Vector2 reference34 = ref array4[num7];
			reference34 = new Vector2(i, zCutPlanes[2]);
			ref Vector2 reference35 = ref array4[1 + num7];
			reference35 = new Vector2(i, 0f);
			ref Vector2 reference36 = ref array4[2 + num7];
			reference36 = new Vector2((float)i + 1f, zCutPlanes[2]);
			ref Vector2 reference37 = ref array4[3 + num7];
			reference37 = new Vector2((float)i + 1f, 0f);
		}
		mesh.uv = array4;
		int[] array5 = new int[6 * num];
		for (int i = 0; i < num; i++)
		{
			int num7 = i * 4;
			int num10 = i * 6;
			array5[num10] = num7;
			array5[1 + num10] = 2 + num7;
			array5[2 + num10] = 1 + num7;
			array5[3 + num10] = 1 + num7;
			array5[4 + num10] = 2 + num7;
			array5[5 + num10] = 3 + num7;
		}
		mesh.triangles = array5;
		mesh.bounds = GetBounds(LevelAttributes.instance.bounds);
	}

	private Bounds GetBounds(Rect r)
	{
		Vector3 center = new Vector3((r.xMin + r.xMax) * 0.5f, (r.yMin + r.yMax) * 0.5f, 800f);
		Vector3 size = new Vector3(r.width, r.height, 800f);
		return new Bounds(center, size);
	}
}
