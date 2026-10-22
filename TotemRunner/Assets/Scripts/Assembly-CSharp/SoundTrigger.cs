using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
	public enum eTriggerSoundType
	{
		sound2D = 0,
		sound3D = 1,
		music = 2,
		fadeoutMusic = 3
	}

	public AudioClip sound;

	public float volume = 1f;

	public eTriggerSoundType soundType = eTriggerSoundType.sound3D;

	public float timeStart;

	public float timeLength = 1f;

	public float fadeinTime = 1f;

	public float fadeoutTime = 2f;

	private float timeFollow = -1f;

	private void Update()
	{
		if (timeFollow >= 0f)
		{
			timeFollow -= Time.deltaTime;
			if (timeFollow <= 0f)
			{
				timeFollow = -1f;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(timeFollow >= 0f))
		{
			timeFollow = 0.5f;
			switch (soundType)
			{
			case eTriggerSoundType.sound3D:
			{
				Vector3 position = base.transform.position;
				position.z = ScrollingCamera.instance.transform.position.z;
				SoundEnvironment.Launch3DSound(sound, volume, position, 0f, isLoop: false, null);
				break;
			}
			case eTriggerSoundType.sound2D:
				SoundEnvironment.Launch2DSound(sound, volume, timeStart, timeLength);
				break;
			case eTriggerSoundType.music:
				SoundEnvironment.instance.LaunchMusicAddition(sound, volume, timeStart, timeLength, fadeinTime, fadeoutTime);
				break;
			case eTriggerSoundType.fadeoutMusic:
				SoundEnvironment.instance.FadeoutMusicAddition(fadeoutTime);
				break;
			}
		}
	}
}
