using UnityEngine;

public class MovingPlane : AfterCameraUpdated
{
	private Transform target;

	private Transform selfTransform;

	private float moveOffset;

	private float zOffset;

	public float moveRelation;

	public override void AwakeAfterCamera(Transform relationCamera)
	{
		target = relationCamera;
		selfTransform = base.transform;
		moveOffset = selfTransform.position.x;
		zOffset = selfTransform.position.z - target.position.z;
	}

	public override void UpdateAfterCamera()
	{
		if ((bool)selfTransform)
		{
			selfTransform.position = new Vector3(moveOffset + target.position.x * moveRelation, selfTransform.position.y, target.position.z + zOffset);
		}
	}
}
