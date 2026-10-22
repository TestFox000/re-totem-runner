using System;
using UnityEngine;

public class AnimationTrigger : BaseTrigger
{
	[Serializable]
	public class AnimationEntity
	{
		public Animation objectAnimation;

		public string startAnimationName;

		public float timeDelayToNext;

		[NonSerialized]
		public bool isPlayingAnim;
	}

	public AnimationEntity[] list;

	public AudioClip sound;

	public Animation skipSoundForAnimation;

	private float timeFollowPrev = -1f;

	private float timeFollow = -1f;

	private int nPlayingAnimation;

	private void Awake()
	{
		AnimationEntity[] array = list;
		foreach (AnimationEntity animationEntity in array)
		{
			animationEntity.isPlayingAnim = false;
		}
	}

	public override void ExecuteTrigger()
	{
		if (timeFollow < 0f)
		{
			RestartObjectsList.IncludeRestartObject(this);
			timeFollow = 0f;
		}
		float num = 10000f;
		AnimationEntity[] array = list;
		foreach (AnimationEntity animationEntity in array)
		{
			if (animationEntity.timeDelayToNext <= timeFollow)
			{
				if (animationEntity.timeDelayToNext > timeFollowPrev)
				{
					ExecuteEntity(animationEntity);
				}
			}
			else if (animationEntity.timeDelayToNext < num)
			{
				num = animationEntity.timeDelayToNext;
			}
		}
		if (num < 10000f)
		{
			timeLeft = num - timeFollow;
			timeFollowPrev = timeFollow;
			timeFollow = num;
		}
	}

	private void ExecuteEntity(AnimationEntity entity)
	{
		if (!entity.isPlayingAnim)
		{
			entity.objectAnimation.enabled = true;
			SkinnedMeshRenderer[] componentsInChildren = entity.objectAnimation.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			if (componentsInChildren != null)
			{
				componentsInChildren[0].updateWhenOffscreen = true;
			}
			else
			{
				Debug.LogWarning(string.Concat(Time.time, ": Trigger ", this, " Animated Object ", entity.objectAnimation, " have not SkinnedMeshRenderer"));
			}
			entity.objectAnimation.Stop();
			entity.objectAnimation.Play(entity.startAnimationName);
			if (sound != null && entity.objectAnimation != skipSoundForAnimation)
			{
				SoundEnvironment.Launch3DSound(sound, 1f, entity.objectAnimation.transform.position, 0f, isLoop: false, null);
			}
			entity.isPlayingAnim = true;
			nPlayingAnimation++;
		}
	}

	private void SampleAnimationToStart(float x)
	{
		timeFollow = -1f;
		timeFollowPrev = -1f;
		AnimationEntity[] array = list;
		foreach (AnimationEntity animationEntity in array)
		{
			if (animationEntity.objectAnimation.transform.position.x >= x)
			{
				animationEntity.objectAnimation.Play(animationEntity.startAnimationName);
				animationEntity.objectAnimation[animationEntity.startAnimationName].time = 0f;
				animationEntity.objectAnimation.Sample();
				animationEntity.objectAnimation.Stop();
				animationEntity.isPlayingAnim = false;
				animationEntity.objectAnimation.enabled = false;
			}
		}
		nPlayingAnimation = 0;
	}

	public override void Update()
	{
		if (nPlayingAnimation > 0)
		{
			float num = ScrollingCamera.leftCameraBound - 500f;
			AnimationEntity[] array = list;
			foreach (AnimationEntity animationEntity in array)
			{
				if (animationEntity.isPlayingAnim)
				{
					Transform transform = animationEntity.objectAnimation.transform;
					if (transform.position.x + ScrollingCamera.fieldOfViewMultiply * transform.position.z < num)
					{
						SkinnedMeshRenderer[] componentsInChildren = animationEntity.objectAnimation.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
						SkinnedMeshRenderer skinnedMeshRenderer = componentsInChildren[0];
						skinnedMeshRenderer.updateWhenOffscreen = false;
						animationEntity.objectAnimation.enabled = false;
						animationEntity.isPlayingAnim = false;
						nPlayingAnimation--;
					}
				}
			}
		}
		base.Update();
	}

	public override bool RestartToX(float x)
	{
		float num = base.transform.position.x;
		if (base.GetComponent<Collider>() != null)
		{
			num = base.GetComponent<Collider>().bounds.min.x - 400f;
		}
		if (x < num)
		{
			base.RestartToX(x);
			SampleAnimationToStart(x);
			return true;
		}
		float num2 = x - base.transform.position.x;
		num2 = ((!(num2 <= 0f)) ? (num2 * 0.0025f) : 0f);
		ResetAnimationsToX(x, num2);
		return false;
	}

	private void ResetAnimationsToX(float x, float triggerTime)
	{
		timeLeft = 10000f;
		timeFollow = Mathf.Clamp(triggerTime, 0f, 10000f);
		timeFollowPrev = triggerTime;
		nPlayingAnimation = 0;
		AnimationEntity[] array = list;
		foreach (AnimationEntity animationEntity in array)
		{
			float num = animationEntity.timeDelayToNext - triggerTime;
			if (num > 0f)
			{
				if (timeLeft < num)
				{
					timeLeft = num;
				}
				animationEntity.objectAnimation.Play(animationEntity.startAnimationName);
				animationEntity.objectAnimation[animationEntity.startAnimationName].time = 0f;
				animationEntity.objectAnimation.Sample();
				animationEntity.objectAnimation.Stop();
				animationEntity.isPlayingAnim = false;
				animationEntity.objectAnimation.enabled = false;
			}
			else
			{
				animationEntity.objectAnimation.Stop();
				animationEntity.objectAnimation.Play(animationEntity.startAnimationName);
				animationEntity.objectAnimation[animationEntity.startAnimationName].time = 0f - num;
				animationEntity.isPlayingAnim = true;
				animationEntity.objectAnimation.enabled = true;
				nPlayingAnimation++;
			}
		}
		if (timeLeft >= 10000f)
		{
			timeLeft = 0f;
		}
	}
}
