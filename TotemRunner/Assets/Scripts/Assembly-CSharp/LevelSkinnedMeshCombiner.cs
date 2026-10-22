using UnityEngine;

public class LevelSkinnedMeshCombiner : MonoBehaviour
{
	public SkinnedInstanceEntity[] instances;

	public GameObject[] prefabList;

	private Transform refCamera;

	private int oldCamGrid;

	private int nextVisibleIndex;

	private OrderListLoop visibleList = new OrderListLoop(32);

	private static float nGreedMultiply = 0.01f;

	private void Awake()
	{
		oldCamGrid = -1;
		nextVisibleIndex = 0;
		visibleList.ClearList();
	}

	private void Start()
	{
		refCamera = ScrollingCamera.instance.transform;
	}

	private void Update()
	{
		int num = Mathf.FloorToInt(refCamera.position.x * nGreedMultiply);
		if (num < 0)
		{
			num = 0;
		}
		if (oldCamGrid == num)
		{
			return;
		}
		while (visibleList.GetListQuantity() > 0)
		{
			SortableByInt firstSortable = visibleList.GetFirstSortable();
			if (firstSortable.GetOrder() >= num)
			{
				break;
			}
			visibleList.ShiftNod();
			MakeInvisible(firstSortable as SkinnedInstanceEntity);
		}
		while (nextVisibleIndex < instances.Length && instances[nextVisibleIndex].lGrid <= num)
		{
			MakeVisible(instances[nextVisibleIndex]);
			visibleList.InsertNod(instances[nextVisibleIndex]);
			nextVisibleIndex++;
		}
		oldCamGrid = num;
	}

	private void MakeInvisible(SkinnedInstanceEntity inst)
	{
		Object.Destroy(inst.inst, 0f);
	}

	private void MakeVisible(SkinnedInstanceEntity inst)
	{
		GameObject original = prefabList[inst.prefabId];
		Vector4 column = inst.mtxTransform.GetColumn(3);
		Quaternion rotation = Quaternion.LookRotation(inst.mtxTransform.GetColumn(2), inst.mtxTransform.GetColumn(1));
		inst.inst = (GameObject)Object.Instantiate(original, new Vector3(column.x, column.y, column.z), rotation);
		inst.inst.transform.parent = base.transform;
	}

	public static void ResetAllToX()
	{
		Object[] array = Object.FindObjectsOfType(typeof(LevelSkinnedMeshCombiner));
		for (int i = 0; i < array.Length; i++)
		{
			LevelSkinnedMeshCombiner levelSkinnedMeshCombiner = array[i] as LevelSkinnedMeshCombiner;
			if ((bool)levelSkinnedMeshCombiner)
			{
				levelSkinnedMeshCombiner.ResetToX();
			}
		}
	}

	private void ResetToX()
	{
		oldCamGrid = -1;
		visibleList.ClearList();
	}
}
