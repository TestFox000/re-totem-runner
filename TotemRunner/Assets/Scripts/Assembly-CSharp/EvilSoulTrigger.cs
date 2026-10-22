using System;
using UnityEngine;

public class EvilSoulTrigger : MonoBehaviour
{
	[Serializable]
	public class LaunchAnimation
	{
		public Animation refAnimation;

		public string animationName;
	}

	public LaunchAnimation[] launchAnimations;

	public AudioClip sound;

	public float soundVolume = 1f;

	public float soundDelay;

	private float soundDelayLeft = -1f;

	public float levelCompleteDelay = 2f;

	private float levelCompleteLeftTime = -1f;

	public EventExecutor completeEvent;

	private bool isWaiting = true;

	private void Update()
	{
		if (soundDelayLeft >= 0f)
		{
			soundDelayLeft -= Time.deltaTime;
			if (soundDelayLeft <= 0f)
			{
				if ((bool)sound)
				{
					SoundEnvironment.Launch2DSound(sound, soundVolume, 0f, -1f);
				}
				soundDelayLeft = -1f;
			}
		}
		if (!(levelCompleteLeftTime >= 0f))
		{
			return;
		}
		levelCompleteLeftTime -= Time.deltaTime;
		if (levelCompleteLeftTime <= 0f)
		{
			levelCompleteLeftTime = -1f;
			if ((bool)completeEvent)
			{
				completeEvent.StartExecutor();
			}
			else
			{
				LevelCompleteEvent.Launch();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isWaiting)
		{
			return;
		}
		isWaiting = false;
		LaunchAnimation[] array = launchAnimations;
		foreach (LaunchAnimation launchAnimation in array)
		{
			if (launchAnimation.refAnimation != null)
			{
				launchAnimation.refAnimation.Play(launchAnimation.animationName);
			}
		}
		soundDelayLeft = soundDelay;
		levelCompleteLeftTime = levelCompleteDelay;
		PlayerController.instance.DisableController(disable: true);
	}
}
