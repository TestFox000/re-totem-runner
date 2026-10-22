using System;
using UnityEngine;

[Serializable]
public class SkinnedInstanceEntity : SortableByInt
{
	public Matrix4x4 mtxTransform = Matrix4x4.identity;

	public int prefabId;

	public int lGrid;

	public int rGrid;

	[NonSerialized]
	public GameObject inst;

	public int GetOrder()
	{
		return rGrid;
	}
}
