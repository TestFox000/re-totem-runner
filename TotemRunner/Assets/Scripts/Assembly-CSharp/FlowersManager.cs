using System;
using UnityEngine;

[Serializable]
public class FlowersManager
{
	private class FlowersTriangles
	{
		public int quantity;

		public int[] indices;

		private int curIndex;

		public FlowersTriangles()
		{
			quantity = 0;
			curIndex = 0;
		}

		public void InsertSquare(int i)
		{
			indices[curIndex++] = i;
			indices[curIndex++] = i + 2;
			indices[curIndex++] = i + 1;
			indices[curIndex++] = i + 1;
			indices[curIndex++] = i + 2;
			indices[curIndex++] = i + 3;
		}
	}

	[Serializable]
	public class FlowerSprite
	{
		public Vector2 pos;

		public float halfSize = 50f;

		public float angle;
	}

	private class FlowersVisible
	{
		public int index;

		public float growTimeLeft;

		public int frame;

		public float maxAlpha;

		public int materialIndex;
	}

	public float rangeX = 200f;

	public float offsetX;

	public float offsetZ = -0.5f;

	public float growingTime = 0.3f;

	public float growingDelay = 0.5f;

	public float minAlpha = 0.2f;

	public float maxAlpha = 1f;

	public FlowerSprite[] sprites;

	private FlowersVisible[] visibleList;

	private int lastInclude;

	private Mesh mesh;

	public Material[] materials;

	private Bounds meshBounds;

	private bool isMeshNeedRebuild;

	private int currentFlowersMaterial;

	private bool[] materialUses;

	public void InitFlowers(Bounds levelBounds)
	{
		meshBounds = levelBounds;
		mesh = new Mesh();
		mesh.bounds = meshBounds;
		mesh.subMeshCount = materials.Length;
		visibleList = null;
		lastInclude = 0;
		isMeshNeedRebuild = false;
		materialUses = ((materials == null || materials.Length <= 0) ? null : new bool[materials.Length]);
	}

	public void SetUsedFlowersMaterial(int matIndex)
	{
		currentFlowersMaterial = matIndex;
	}

	public void InsertBadFlowers(float fromX, float toX)
	{
		int num = lastInclude;
		int i;
		for (i = lastInclude; i < sprites.Length; i++)
		{
			if (sprites[num].pos.x < fromX)
			{
				num++;
			}
			if (sprites[i].pos.x >= toX)
			{
				break;
			}
		}
		IncludeFlowers(num, i, isFullGrowing: false, 1);
	}

	public void UpdateFlowers(float x, bool isActive)
	{
		float num = x + offsetX;
		if (isActive)
		{
			num += rangeX;
		}
		int i;
		for (i = lastInclude; i < sprites.Length && !(sprites[i].pos.x >= num); i++)
		{
		}
		float num2 = CalculateLeftVisibleBound();
		int j = 0;
		if (visibleList != null)
		{
			for (; j < visibleList.Length && !(sprites[visibleList[j].index].pos.x > num2); j++)
			{
			}
		}
		ExcludeFlowers(j);
		if (isActive)
		{
			IncludeFlowers(lastInclude, i, isFullGrowing: false, currentFlowersMaterial);
		}
		if (!isMeshNeedRebuild)
		{
			return;
		}
		isMeshNeedRebuild = false;
		mesh.Clear();
		mesh.subMeshCount = materials.Length;
		if (visibleList == null)
		{
			return;
		}
		int num3 = visibleList.Length * 4;
		Vector3[] array = new Vector3[num3];
		Vector2[] array2 = new Vector2[num3];
		Color[] array3 = new Color[num3];
		FlowersTriangles[] array4 = new FlowersTriangles[materials.Length];
		for (int k = 0; k < materials.Length; k++)
		{
			array4[k] = new FlowersTriangles();
		}
		for (int l = 0; l < visibleList.Length; l++)
		{
			array4[visibleList[l].materialIndex].quantity += 6;
		}
		for (int m = 0; m < materials.Length; m++)
		{
			array4[m].indices = ((array4[m].quantity <= 0) ? null : new int[array4[m].quantity]);
		}
		float num4 = 1f / growingTime;
		int num5 = 0;
		for (int n = 0; n < visibleList.Length; n++)
		{
			FlowerSprite flowerSprite = sprites[visibleList[n].index];
			float num6 = visibleList[n].maxAlpha;
			Color color;
			float num7;
			if (visibleList[n].growTimeLeft > 0f)
			{
				isMeshNeedRebuild = true;
				if (visibleList[n].growTimeLeft > growingTime)
				{
					color = new Color(0f, 0f, 0f, 0f);
					num7 = 0f;
				}
				else
				{
					float num8 = 1f - visibleList[n].growTimeLeft * num4;
					color = new Color(0f, 0f, 0f, num8 * num6);
					num7 = num8 * flowerSprite.halfSize;
				}
			}
			else
			{
				color = new Color(0f, 0f, 0f, num6);
				num7 = flowerSprite.halfSize;
			}
			Vector3 vector = new Vector3(flowerSprite.pos.x, flowerSprite.pos.y, offsetZ);
			float x2 = Mathf.Cos(flowerSprite.angle);
			float y = Mathf.Sin(flowerSprite.angle);
			Vector3 vector2 = new Vector3(x2, y, 0f) * num7;
			Vector3 vector3 = new Vector3(0f - vector2.y, vector2.x, 0f);
			ref Vector3 reference = ref array[num5];
			reference = vector - vector2 + vector3;
			ref Vector3 reference2 = ref array[num5 + 1];
			reference2 = vector - vector2 - vector3;
			ref Vector3 reference3 = ref array[num5 + 2];
			reference3 = vector + vector2 + vector3;
			ref Vector3 reference4 = ref array[num5 + 3];
			reference4 = vector + vector2 - vector3;
			float num9 = 0f;
			switch (visibleList[n].frame)
			{
			case 0:
				num9 = 0f;
				break;
			case 1:
				num9 = 0.5f;
				break;
			}
			ref Vector2 reference5 = ref array2[num5];
			reference5 = new Vector2(num9, 0f);
			ref Vector2 reference6 = ref array2[num5 + 1];
			reference6 = new Vector2(num9, 1f);
			num9 += 0.5f;
			ref Vector2 reference7 = ref array2[num5 + 2];
			reference7 = new Vector2(num9, 0f);
			ref Vector2 reference8 = ref array2[num5 + 3];
			reference8 = new Vector2(num9, 1f);
			array3[num5] = color;
			array3[num5 + 1] = color;
			array3[num5 + 2] = color;
			array3[num5 + 3] = color;
			array4[visibleList[n].materialIndex].InsertSquare(num5);
			num5 += 4;
			visibleList[n].growTimeLeft -= Time.deltaTime;
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.colors = array3;
		mesh.bounds = meshBounds;
		for (int num10 = 0; num10 < materials.Length; num10++)
		{
			if (array4[num10].quantity > 0)
			{
				materialUses[num10] = true;
				mesh.SetTriangles(array4[num10].indices, num10);
			}
			else
			{
				materialUses[num10] = false;
			}
		}
	}

	public void DrawFlowers()
	{
		if (!mesh || mesh.vertexCount <= 0)
		{
			return;
		}
		for (int i = 0; i < materials.Length; i++)
		{
			if (materialUses[i])
			{
				Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, materials[i], GrassGifManager.g_nGrassLayer, null, i);
			}
		}
	}

	private float CalculateLeftVisibleBound()
	{
		return Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f - Camera.main.transform.position.z)).x - 100f;
	}

	private void ExcludeFlowers(int nTillExclude)
	{
		if (nTillExclude <= 0)
		{
			return;
		}
		isMeshNeedRebuild = true;
		int num = ((visibleList != null) ? visibleList.Length : 0);
		num -= nTillExclude;
		if (num > 0)
		{
			FlowersVisible[] array = new FlowersVisible[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = visibleList[i + nTillExclude];
			}
			visibleList = array;
		}
		else
		{
			visibleList = null;
		}
	}

	private void IncludeFlowers(int nFrom, int nTill, bool isFullGrowing, int matIndex)
	{
		if (nFrom < nTill)
		{
			isMeshNeedRebuild = true;
			int num = ((visibleList != null) ? visibleList.Length : 0);
			int num2 = num + nTill - nFrom;
			FlowersVisible[] array = new FlowersVisible[num2];
			for (int i = 0; i < num; i++)
			{
				array[i] = visibleList[i];
			}
			int num3 = num;
			while (nFrom < nTill)
			{
				FlowersVisible flowersVisible = new FlowersVisible();
				flowersVisible.index = nFrom;
				if (isFullGrowing)
				{
					flowersVisible.growTimeLeft = 0f;
				}
				else
				{
					flowersVisible.growTimeLeft = growingTime + UnityEngine.Random.Range(0f, growingDelay);
				}
				flowersVisible.frame = UnityEngine.Random.Range(0, 2);
				flowersVisible.maxAlpha = UnityEngine.Random.Range(minAlpha, maxAlpha);
				flowersVisible.materialIndex = matIndex;
				array[num3++] = flowersVisible;
				nFrom++;
			}
			visibleList = array;
		}
		if (lastInclude < nTill)
		{
			lastInclude = nTill;
		}
	}

	private int FindSpriteIndexFromX(float x)
	{
		if (sprites.Length == 0)
		{
			return -1;
		}
		if (x < sprites[0].pos.x)
		{
			return 0;
		}
		int num = 0;
		int num2 = sprites.Length - 1;
		while (num2 > num + 1)
		{
			int num3 = (num + num2) / 2;
			if (x < sprites[num3].pos.x)
			{
				num2 = num3;
			}
			else
			{
				num = num3;
			}
		}
		if (x < sprites[num2].pos.x)
		{
			return num2;
		}
		return -1;
	}

	public void ResetToX(float x)
	{
		visibleList = null;
		int num = FindSpriteIndexFromX(CalculateLeftVisibleBound());
		if (num >= 0)
		{
			float num2 = x + offsetX;
			int i;
			for (i = num; i < sprites.Length && !(sprites[i].pos.x >= num2); i++)
			{
			}
			IncludeFlowers(num, i, isFullGrowing: true, currentFlowersMaterial);
			lastInclude = i;
		}
	}
}
