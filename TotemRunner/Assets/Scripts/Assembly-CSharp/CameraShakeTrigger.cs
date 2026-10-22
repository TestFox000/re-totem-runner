using UnityEngine;

public class CameraShakeTrigger : BaseTrigger
{
	public ECameraShakeType shakeType = ECameraShakeType.none;

	public float shakeInterval;

	public float shakeTimeout;

	private float shakeTimer;

	public AudioClip sound;

	public float soundVolume = 1f;

	public override void ExecuteTrigger()
	{
		if (shakeTimer == 0f)
		{
			RestartObjectsList.IncludeRestartObject(this);
		}
		CameraShake.instance.LaunchShake(shakeType, 1f);
		if (sound != null)
		{
			SoundEnvironment.Launch2DSound(sound, soundVolume, 0f, -1f);
		}
		if (shakeInterval > 0f)
		{
			shakeTimer += shakeInterval;
			if (shakeTimer <= shakeTimeout)
			{
				timeLeft = shakeInterval;
			}
		}
	}

	public override bool RestartToX(float x)
	{
		base.RestartToX(x);
		shakeTimer = 0f;
		return true;
	}
}
