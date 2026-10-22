using UnityEngine;

public class CheckPoint : ObjectEnabledByDistance
{
	private Animation refAnimation;

	public AudioClip sound;

	public float soundVolume = 1f;

	private bool isOnceUsed;

	private void Awake()
	{
		Animation[] componentsInChildren = GetComponentsInChildren<Animation>();
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			refAnimation = componentsInChildren[0];
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isOnceUsed)
		{
			PlayerController component = other.gameObject.GetComponent<PlayerController>();
			if (component != null)
			{
				isOnceUsed = true;
				component.EnterToCheckPoint(base.transform);
				GameSettings.SaveRestoreData();
				LaunchAnimation("checkpoint");
			}
		}
	}

	public void OnRespawn()
	{
		LaunchAnimation("spawn");
	}

	private void LaunchAnimation(string aniName)
	{
		if (aniName == "checkpoint" && !refAnimation.IsPlaying(aniName))
		{
			SoundEnvironment.Launch3DSound(sound, soundVolume, base.transform.position, 0f, isLoop: false, null);
		}
		refAnimation.Stop();
		refAnimation.Play(aniName);
	}
}
