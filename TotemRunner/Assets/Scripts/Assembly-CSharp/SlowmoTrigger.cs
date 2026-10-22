using UnityEngine;

public class SlowmoTrigger : MonoBehaviour
{
	public float slowmoScale = 1f;

	public float timeFadeIn = 0.2f;

	public float timeFixed = 0.6f;

	public float timeFadeOut = 0.2f;

	private void OnTriggerEnter(Collider other)
	{
		TimeController.StartSlowMo(slowmoScale, timeFadeIn, timeFixed, timeFadeOut, eSlowMoMode.nonBreakable);
	}
}
