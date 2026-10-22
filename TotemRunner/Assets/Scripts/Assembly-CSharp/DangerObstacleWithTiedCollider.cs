using UnityEngine;

public class DangerObstacleWithTiedCollider : DangerObstacle
{
	public Transform colliderTiedBone;

	private BoxCollider boxCollider;

	private Vector3 baseCenterPos;

	public override void DangerAwake()
	{
		boxCollider = base.GetComponent<Collider>() as BoxCollider;
		if (boxCollider != null)
		{
			baseCenterPos = boxCollider.center;
		}
	}

	public override void DangerUpdate()
	{
		if (boxCollider != null)
		{
			boxCollider.center = base.transform.InverseTransformPoint(colliderTiedBone.TransformPoint(baseCenterPos));
		}
		base.DangerUpdate();
	}
}
