using UnityEngine;

public class TieAnimatedObjectTrigger : MonoBehaviour
{
	public Transform tieToObject;

	public Animation animatedObject;

	public float animatedDistance = 600f;

	public string animName = "idle";

	private bool isWorked;

	private void Awake()
	{
		animatedObject.gameObject.SetActiveRecursivelyCompat(state: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isWorked)
		{
			isWorked = true;
			animatedObject.gameObject.SetActiveRecursivelyCompat(state: true);
			animatedObject.Stop();
			animatedObject.Play(animName);
		}
	}

	private void Update()
	{
		if (isWorked)
		{
			float num = tieToObject.position.x - base.transform.position.x;
			if (num < -500f || num >= animatedDistance)
			{
				animatedObject.Stop();
				animatedObject.gameObject.SetActiveRecursivelyCompat(state: false);
				isWorked = false;
			}
			else
			{
				animatedObject.transform.position = tieToObject.position;
				animatedObject[animName].normalizedTime = Mathf.Clamp01(num / animatedDistance);
			}
		}
	}
}
