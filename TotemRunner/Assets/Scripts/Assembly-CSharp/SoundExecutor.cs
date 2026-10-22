using UnityEngine;

public class SoundExecutor : EventExecutor
{
	public AudioClip sound;

	public float soundVolume = 1f;

	public Transform refTransform;

	private void Awake()
	{
		if (refTransform == null)
		{
			refTransform = base.transform;
		}
	}

	public override void StartExecutor()
	{
		SoundEnvironment.Launch3DSound(sound, soundVolume, refTransform.position, 0f, isLoop: false, null);
		base.StartExecutor();
	}
}
