using UnityEngine;

public class AfterCameraUpdated : MonoBehaviour
{
	public int order = 100;

	public AfterCameraUpdated nextSecondOrderedObject;

	public virtual void AwakeAfterCamera(Transform t)
	{
	}

	public virtual void UpdateAfterCamera()
	{
	}

	public virtual void RenderByOrder()
	{
	}

	public virtual int GetSecondOrder()
	{
		return -1;
	}

	public virtual void RenderByOrderSecond()
	{
	}
}
