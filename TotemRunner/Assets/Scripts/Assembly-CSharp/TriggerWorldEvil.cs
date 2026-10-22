using UnityEngine;

public class TriggerWorldEvil : MonoBehaviour
{
	private bool isWaitTrigger = true;

	public bool isDisableWorldEvil = true;

	public WorldEvil.eControlId controlId;

	public float controlParam;

	private void OnTriggerEnter(Collider other)
	{
		if (isWaitTrigger && WorldEvil.instance != null)
		{
			isWaitTrigger = false;
			if (isDisableWorldEvil)
			{
				WorldEvil.instance.ExternControl(WorldEvil.eControlId.finalAway, 0f);
			}
			else
			{
				WorldEvil.instance.ExternControl(controlId, controlParam);
			}
		}
	}
}
