using System;
using UnityEngine;

public class ProgressCircle : BaseProgress
{
	public Material progressMaterial;

	public Camera refCameraGUI;

	public Material backMaterial;

	public Material foreMaterial;

	private bool isUsed = true;

	protected Matrix4x4 progressTransform = Matrix4x4.identity;

	private Mesh progressMesh;

	private Mesh backMesh;

	public bool drawBack = true;

	public bool drawFore;

	public bool autoRender;

	public float snapProgressStep;

	private float snapSegmentQ;

	public override void Start()
	{
		RecalculateTransform();
		progressMesh = new Mesh();
		backMesh = CreateSpriteMesh();
		if (snapProgressStep > 0f)
		{
			snapSegmentQ = Mathf.Floor(1f / snapProgressStep + 0.1f);
		}
		base.Start();
	}

	private void Update()
	{
		if (autoRender)
		{
			Graphics.DrawMesh(progressMesh, progressTransform, progressMaterial, base.gameObject.layer, refCameraGUI);
		}
	}

	protected virtual void RecalculateTransform()
	{
		progressTransform = base.transform.localToWorldMatrix;
	}

	public override void DoRender()
	{
		if (isUsed)
		{
			if (drawBack && backMaterial != null)
			{
				backMaterial.SetPass(0);
				Graphics.DrawMeshNow(backMesh, progressTransform);
			}
			if (progressMaterial != null && progressValue > 0f)
			{
				progressMaterial.SetPass(0);
				Graphics.DrawMeshNow(progressMesh, progressTransform);
			}
			if (drawFore && foreMaterial != null)
			{
				foreMaterial.SetPass(0);
				Graphics.DrawMeshNow(backMesh, progressTransform);
			}
		}
	}

	public override void SetProgress(float progressVal)
	{
		if (progressValue != progressVal)
		{
			base.SetProgress(progressVal);
			UpdateMesh();
		}
	}

	private void UpdateMesh()
	{
		Vector3[] array = null;
		Vector2[] array2 = null;
		int[] array3 = null;
		float num = Mathf.Clamp01(progressValue) * 4f;
		if (snapProgressStep > 0f)
		{
			float num2 = Mathf.Clamp01(snapProgressStep * Mathf.Floor(progressValue * snapSegmentQ + 0.5f)) * 2f;
			num = ((num2 < 0.25f) ? (0.5f * Mathf.Tan(num2 * (float)Math.PI)) : ((num2 < 0.75f) ? (1f + 0.5f * Mathf.Tan((num2 - 0.5f) * (float)Math.PI)) : ((num2 < 1.25f) ? (2f + 0.5f * Mathf.Tan((num2 - 1f) * (float)Math.PI)) : ((!(num2 < 1.75f)) ? (4f + 0.5f * Mathf.Tan((num2 - 2f) * (float)Math.PI)) : (3f + 0.5f * Mathf.Tan((num2 - 1.5f) * (float)Math.PI))))));
			num = Mathf.Clamp(num, 0f, 4f);
		}
		if (num > 0f && progressMesh != null)
		{
			int num3 = Mathf.FloorToInt(num + 0.5f);
			array = new Vector3[num3 + 3];
			array2 = new Vector2[num3 + 3];
			array3 = new int[(num3 + 1) * 3];
			ref Vector3 reference = ref array[0];
			reference = Vector3.zero;
			ref Vector2 reference2 = ref array2[0];
			reference2 = Vector2.one * 0.5f;
			ref Vector3 reference3 = ref array[1];
			reference3 = Vector3.up * 0.5f;
			ref Vector2 reference4 = ref array2[1];
			reference4 = new Vector2(0.5f, 1f);
			int num4 = 2;
			if (num3 > 0)
			{
				ref Vector3 reference5 = ref array[num4];
				reference5 = new Vector3(0.5f, 0.5f, 0f);
				ref Vector2 reference6 = ref array2[num4];
				reference6 = Vector2.one;
				num4++;
				if (num3 > 1)
				{
					ref Vector3 reference7 = ref array[num4];
					reference7 = new Vector3(0.5f, -0.5f, 0f);
					ref Vector2 reference8 = ref array2[num4];
					reference8 = new Vector2(1f, 0f);
					num4++;
					if (num3 > 2)
					{
						ref Vector3 reference9 = ref array[num4];
						reference9 = new Vector3(-0.5f, -0.5f, 0f);
						ref Vector2 reference10 = ref array2[num4];
						reference10 = Vector2.zero;
						num4++;
						if (num3 > 3)
						{
							ref Vector3 reference11 = ref array[num4];
							reference11 = new Vector3(-0.5f, 0.5f, 0f);
							ref Vector2 reference12 = ref array2[num4];
							reference12 = Vector2.up;
							num4++;
							ref Vector3 reference13 = ref array[num4];
							reference13 = new Vector3(num - 4f, 0.5f, 0f);
							ref Vector2 reference14 = ref array2[num4];
							reference14 = new Vector2(num - 3.5f, 1f);
						}
						else
						{
							ref Vector3 reference15 = ref array[num4];
							reference15 = new Vector3(-0.5f, num - 3f, 0f);
							ref Vector2 reference16 = ref array2[num4];
							reference16 = new Vector2(0f, num - 2.5f);
						}
					}
					else
					{
						ref Vector3 reference17 = ref array[num4];
						reference17 = new Vector3(2f - num, -0.5f, 0f);
						ref Vector2 reference18 = ref array2[num4];
						reference18 = new Vector2(2.5f - num, 0f);
					}
				}
				else
				{
					ref Vector3 reference19 = ref array[num4];
					reference19 = new Vector3(0.5f, 1f - num, 0f);
					ref Vector2 reference20 = ref array2[num4];
					reference20 = new Vector2(1f, 1.5f - num);
				}
			}
			else
			{
				ref Vector3 reference21 = ref array[num4];
				reference21 = new Vector3(num, 0.5f, 0f);
				ref Vector2 reference22 = ref array2[num4];
				reference22 = new Vector2(0.5f + num, 1f);
			}
			for (num4 = 0; num4 <= num3; num4++)
			{
				int num5 = num4 * 3;
				array3[num5++] = 0;
				array3[num5++] = num4 + 1;
				array3[num5++] = num4 + 2;
			}
		}
		if (array != null && array2 != null && array3 != null)
		{
			progressMesh.Clear();
			progressMesh.vertices = array;
			progressMesh.uv = array2;
			progressMesh.triangles = array3;
			progressMesh.RecalculateBounds();
		}
	}
}
