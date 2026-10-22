using UnityEngine;

public class SoundLoopForAnimation : MonoBehaviour
{
	public AudioClip soundClip;

	public bool isLoop = true;

	public Animation refAnimation;

	public string[] workAnimationNames;

	public float workBoundLeft = 1000f;

	public float workBoundRight = 800f;

	private Transform refTransform;

	private bool isSoundPlaying;

	private void Awake()
	{
		refTransform = base.transform;
		isSoundPlaying = false;
	}

	private void Update()
	{
		if (refTransform.position.x - ScrollingCamera.rightCameraBound > workBoundLeft)
		{
			return;
		}
		if (ScrollingCamera.leftCameraBound - refTransform.position.x > workBoundRight)
		{
			if (isSoundPlaying)
			{
				if ((bool)soundClip)
				{
					SoundEnvironment.Stop3DSound(soundClip, waitForMute: true);
				}
				isSoundPlaying = false;
			}
			return;
		}
		int i;
		for (i = 0; i < workAnimationNames.Length && !refAnimation.IsPlaying(workAnimationNames[i]); i++)
		{
		}
		if (i < workAnimationNames.Length)
		{
			if (!isSoundPlaying)
			{
				isSoundPlaying = true;
				SoundEnvironment.Launch3DSound(soundClip, 1f, refTransform.position, 0f, isLoop, null);
			}
		}
		else if (isSoundPlaying)
		{
			isSoundPlaying = false;
			SoundEnvironment.Stop3DSound(soundClip, waitForMute: true);
		}
	}

	private void OnDisable()
	{
		if (isSoundPlaying)
		{
			isSoundPlaying = false;
			SoundEnvironment.Stop3DSound(soundClip, waitForMute: true);
		}
	}
}
