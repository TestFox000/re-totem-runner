using UnityEngine;

public class ActivateObjectsTrigger : MonoBehaviour
{
	public GameObject activatedObject;

	public bool activatedState = true;

	private void OnTriggerEnter(Collider other)
	{
		activatedObject.SetActiveRecursivelyCompat(activatedState);
	}
}
