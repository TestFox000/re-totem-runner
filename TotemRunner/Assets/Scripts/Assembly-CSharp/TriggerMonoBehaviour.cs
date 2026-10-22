using UnityEngine;

public class TriggerMonoBehaviour : MonoBehaviour
{
	public MonoBehaviour enableScript;

	private void OnTriggerEnter(Collider other)
	{
		if ((bool)enableScript)
		{
			enableScript.enabled = true;
		}
	}
}
