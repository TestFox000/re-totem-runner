using UnityEngine;

public class VideoLauncher : MonoBehaviour
{
	public float timeout;

	public string videoName = string.Empty;

	public GameObject activateObjectAfterExecution;

	private float timeLeft = -1f;

	private bool isPlayingVideo;

	private void Start()
	{
		timeLeft = timeout;
		InAppPurshase.StartPurchase(isSilent: true);
	}

	private void Update()
	{
		if (timeLeft >= 0f)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				timeLeft = -1f;
				IngameVideoPlayer.PlayMovie(videoName);
				isPlayingVideo = true;
			}
		}
		if (isPlayingVideo)
		{
			isPlayingVideo = IngameVideoPlayer.IsPlaying();
			if (!isPlayingVideo)
			{
				FinishExecution();
			}
		}
	}

	private void FinishExecution()
	{
		base.gameObject.SetActive(false);
		if (activateObjectAfterExecution != null)
		{
			activateObjectAfterExecution.SetActiveRecursivelyCompat(state: true);
		}
	}
}
