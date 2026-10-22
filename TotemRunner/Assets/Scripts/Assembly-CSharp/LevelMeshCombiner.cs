using System;
using UnityEngine;

public class LevelMeshCombiner : MonoBehaviour
{
	[Serializable]
	public class InstanceEntity
	{
		public int nameId;

		public Matrix4x4 mtxTransform = Matrix4x4.identity;

		public int meshId;

		public int materialId;

		public int parent = -1;

		public int lGrid;

		public int rGrid;
	}

	public InstanceEntity[] instances;

	public Mesh[] meshList;

	public Material[] materialList;

	private Mesh[] perMaterialCombinedMesh;

	public string[] nameList;

	private int oldCamGrid;

	private Transform refCamera;

	private int lastPrevisible;

	public static int visibleSize = 128;

	private int[] visibleList = new int[visibleSize];

	private int visibleFirst;

	private int visibleQuantity;

	public static float nGreedMultiply = 0.002f;

	private void Awake()
	{
		oldCamGrid = -1;
		if (materialList != null && materialList.Length > 0)
		{
			perMaterialCombinedMesh = new Mesh[materialList.Length];
			for (int i = 0; i < perMaterialCombinedMesh.Length; i++)
			{
				perMaterialCombinedMesh[i] = new Mesh();
			}
		}
	}

	private void Start()
	{
		refCamera = ScrollingCamera.instance.transform;
	}

	private void Update()
	{
		if (instances == null || instances.Length == 0)
		{
			return;
		}
		int num = Mathf.FloorToInt(refCamera.position.x * nGreedMultiply);
		if (num < 0)
		{
			num = 0;
		}
		if (oldCamGrid == num)
		{
			return;
		}
		while (visibleQuantity > 0 && instances[visibleList[visibleFirst]].rGrid < num)
		{
			perMaterialCombinedMesh[instances[visibleList[visibleFirst]].materialId].Clear();
			if (++visibleFirst >= visibleSize)
			{
				visibleFirst -= visibleSize;
			}
			visibleQuantity--;
		}
		while (lastPrevisible < instances.Length && instances[lastPrevisible].lGrid <= num)
		{
			if (instances[lastPrevisible].rGrid >= num)
			{
				InsertIntoVisibleList(lastPrevisible);
				perMaterialCombinedMesh[instances[lastPrevisible].materialId].Clear();
			}
			lastPrevisible++;
		}
		CombineVisibleMeshes();
		oldCamGrid = num;
	}

	public void InsertIntoVisibleList(int instIndex)
	{
		if (visibleQuantity >= visibleSize)
		{
			Debug.LogWarning("Warning! LevelMeshCombiner list (size=" + visibleSize + ") is overflow on position " + refCamera.position.x);
		}
		int num = 0;
		int num2 = visibleQuantity - 1;
		int rGrid = instances[instIndex].rGrid;
		int num3;
		while (num < num2 - 1)
		{
			num3 = (num + num2) / 2;
			if (GetInstanceRightBound(num3) - rGrid <= 0)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		if (num2 < 0 || GetInstanceRightBound(num2) - rGrid <= 0)
		{
			num3 = visibleFirst + visibleQuantity;
			if (num3 >= visibleSize)
			{
				num3 -= visibleSize;
			}
		}
		else if (GetInstanceRightBound(num) - rGrid > 0)
		{
			if (--visibleFirst < 0)
			{
				visibleFirst = visibleSize - 1;
			}
			num3 = visibleFirst;
		}
		else
		{
			int num4 = visibleFirst + visibleQuantity;
			if (num4 >= visibleSize)
			{
				num4 -= visibleSize;
			}
			for (num3 = num2; num3 < visibleQuantity; num3++)
			{
				int num5 = num4 - 1;
				if (num5 < 0)
				{
					num5 = visibleSize - 1;
				}
				visibleList[num4] = visibleList[num5];
				num4 = num5;
			}
			num3 = visibleFirst + num2;
			if (num3 >= visibleSize)
			{
				num3 -= visibleSize;
			}
		}
		visibleList[num3] = instIndex;
		visibleQuantity++;
	}

	public int GetInstanceRightBound(int visibleIndex)
	{
		int num = visibleFirst + visibleIndex;
		if (num >= visibleSize)
		{
			num -= visibleSize;
		}
		return instances[visibleList[num]].rGrid;
	}

	public static void ResetAllToX()
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(LevelMeshCombiner));
		for (int i = 0; i < array.Length; i++)
		{
			LevelMeshCombiner levelMeshCombiner = array[i] as LevelMeshCombiner;
			if ((bool)levelMeshCombiner)
			{
				levelMeshCombiner.ResetToX();
			}
		}
	}

	private void ResetToX()
	{
		oldCamGrid = -1;
		visibleFirst = 0;
		visibleQuantity = 0;
		lastPrevisible = 0;
	}

	public void CombineVisibleMeshes()
	{
		if (visibleQuantity <= 0)
		{
			return;
		}
		int[] array = new int[visibleQuantity];
		for (int i = 0; i < materialList.Length; i++)
		{
			int num = visibleFirst;
			int num2 = 0;
			for (int j = 0; j < visibleQuantity; j++)
			{
				if (instances[visibleList[num]].materialId == i)
				{
					array[num2++] = visibleList[num];
				}
				if (++num >= visibleSize)
				{
					num = 0;
				}
			}
			if (num2 <= 0 || perMaterialCombinedMesh[i].vertexCount != 0)
			{
				continue;
			}
			bool flag = true;
			while (flag)
			{
				flag = false;
				for (int j = 1; j < num2; j++)
				{
					float num3 = instances[array[j]].mtxTransform[14] - instances[array[j - 1]].mtxTransform[14];
					if (num3 < -0.1f)
					{
						flag = true;
						int num4 = array[j - 1];
						array[j - 1] = array[j];
						array[j] = num4;
					}
				}
			}
			CombineInstance[] array2 = new CombineInstance[num2];
			for (int j = 0; j < num2; j++)
			{
				InstanceEntity instanceEntity = instances[array[j]];
				array2[j].mesh = meshList[instanceEntity.meshId];
				array2[j].subMeshIndex = 0;
				array2[j].transform = instanceEntity.mtxTransform;
			}
			perMaterialCombinedMesh[i].CombineMeshes(array2);
		}
	}

	private void OnRenderObject()
	{
		if (Camera.current == null || (Camera.current.cullingMask & 1) == 0 || visibleQuantity <= 0)
		{
			return;
		}
		for (int i = 0; i < perMaterialCombinedMesh.Length; i++)
		{
			if (perMaterialCombinedMesh[i].vertexCount > 0)
			{
				materialList[i].SetPass(0);
				Graphics.DrawMeshNow(perMaterialCombinedMesh[i], Matrix4x4.identity);
			}
		}
	}
}
