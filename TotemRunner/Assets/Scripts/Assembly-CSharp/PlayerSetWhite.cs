using UnityEngine;

public class PlayerSetWhite : MonoBehaviour
{
	public float timeOut = 0.5f;

	private float timeLeft = -1f;

	private void Awake()
	{
		timeLeft = -1f;
	}

	private void OnEnable()
	{
		timeLeft = timeOut;
		PlayerController.instance.SetManWhite(isWhite: true);
	}

	private void Update()
	{
		if (timeLeft >= 0f)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				timeLeft = -1f;
				PlayerController.instance.SetManWhite(isWhite: false);
			}
		}
	}
}
