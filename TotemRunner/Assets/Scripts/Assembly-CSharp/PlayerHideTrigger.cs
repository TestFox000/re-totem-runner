using UnityEngine;

public class PlayerHideTrigger : MonoBehaviour
{
	public Transform cameraLockTarget;

	private void OnTriggerEnter(Collider other)
	{
		DoPlayerHide();
	}

	private void DoPlayerHide()
	{
		PlayerController.instance.DisableController(disable: true);
		PlayerAnimation.instance.StopCurrent();
		PlayerController.instance.gameObject.SetActiveRecursivelyCompat(state: false);
		if ((bool)cameraLockTarget)
		{
			ScrollingCamera.instance.LockToTarget(cameraLockTarget);
		}
		else
		{
			ScrollingCamera.instance.FreezeCamera(freezed: true);
		}
	}
}
