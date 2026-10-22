using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	[Serializable]
	public class Ani2Ani
	{
		public string id = string.Empty;

		public float blend = 0.1f;
	}

	[Serializable]
	public class AnimationDescriber
	{
		public string animationName = string.Empty;

		public float animationSpeedModifier = 1f;

		public WrapMode wrapMode = WrapMode.Loop;

		public float blend;

		public int layer = -1;

		public Ani2Ani[] toAnim;
	}

	public AnimationDescriber[] animSet;

	private AnimationState currentAniState;

	public int currentAnimation = -1;

	private ModelSwaper modelManager;

	private static PlayerAnimation _singleton;

	public static PlayerAnimation instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = UnityEngine.Object.FindObjectOfType(typeof(PlayerAnimation)) as PlayerAnimation;
			}
			return _singleton;
		}
	}

	private void Awake()
	{
		_singleton = this;
		modelManager = GetComponent<ModelSwaper>();
		if (modelManager != null)
		{
			modelManager.AnimationStartEnum();
			while ((bool)modelManager.AnimationGetEnum())
			{
				foreach (AnimationState item in modelManager.AnimationGetEnum())
				{
					int num = FindAnimationByName(item.name);
					if (num >= 0)
					{
						item.wrapMode = animSet[num].wrapMode;
						item.speed = animSet[num].animationSpeedModifier;
						if (animSet[num].layer >= 0)
						{
							item.layer = animSet[num].layer;
						}
					}
				}
				modelManager.AnimationNextEnum();
			}
		}
		StartPlay();
	}

	private void Update()
	{
		currentAniState = null;
		if (currentAnimation < 0)
		{
			return;
		}
		Animation animation = modelManager.AnimationGetCurrent();
		if (!(animation == null))
		{
			currentAniState = animation[animSet[currentAnimation].animationName];
			if (currentAniState != null && currentAniState.time > currentAniState.length)
			{
				currentAniState.time -= currentAniState.length;
			}
		}
	}

	public void StartPlay()
	{
		if (currentAnimation >= 0)
		{
			int idx = currentAnimation;
			currentAnimation = -1;
			SwitchAnimation(idx, noBlend: true, sincTime: false);
		}
	}

	public void SampleToFisrtFrame()
	{
		if ((bool)currentAniState)
		{
			currentAniState.time = 0f;
			Animation animation = modelManager.AnimationGetCurrent();
			animation.Sample();
		}
	}

	public void StopCurrent()
	{
		if ((bool)currentAniState)
		{
			currentAniState.enabled = false;
		}
	}

	public void PauseCurrent(bool isPause)
	{
		if ((bool)currentAniState)
		{
			currentAniState.enabled = !isPause;
		}
	}

	public void SwitchAnimation(string id, bool noBlend, bool sincTime)
	{
		int num = FindAnimationByName(id);
		if (num >= 0)
		{
			SwitchAnimation(num, noBlend, sincTime);
		}
		else
		{
			Debug.LogError("Unknown animation " + id);
		}
	}

	public AnimationState GetCurentAnimationState()
	{
		return currentAniState;
	}

	public void SwitchAnimation(int idx, bool noBlend, bool sincTime)
	{
		if (!modelManager)
		{
			return;
		}
		Animation animation = modelManager.AnimationGetCurrent();
		if (!animation)
		{
			return;
		}
		if (idx >= 0 && currentAnimation != idx)
		{
			if (noBlend)
			{
				animation.Play(animSet[idx].animationName);
			}
			else
			{
				float num = 0f;
				if (sincTime && currentAnimation >= 0)
				{
					AnimationState animationState = animation[animSet[currentAnimation].animationName];
					if ((bool)animationState)
					{
						num = animationState.time / animationState.length;
					}
				}
				AnimationState animationState2 = animation[animSet[idx].animationName];
				if ((bool)animationState2)
				{
					animationState2.time = num * animationState2.length;
					float defBlend = 0.1f;
					if (animSet[idx].blend > 0f)
					{
						defBlend = animSet[idx].blend;
					}
					defBlend = GetAni2AniBlend(currentAnimation, idx, defBlend);
					animation.CrossFade(animSet[idx].animationName, defBlend);
				}
			}
			currentAnimation = idx;
		}
		if (currentAnimation >= 0)
		{
			currentAniState = animation[animSet[currentAnimation].animationName];
		}
	}

	public float GetAni2AniBlend(int nSrc, int nDst, float defBlend)
	{
		float result = defBlend;
		if (nSrc >= 0 && nSrc < animSet.Length && nDst >= 0 && nDst < animSet.Length)
		{
			string animationName = animSet[nDst].animationName;
			Ani2Ani[] toAnim = animSet[nSrc].toAnim;
			foreach (Ani2Ani ani2Ani in toAnim)
			{
				if (ani2Ani.id == animationName)
				{
					result = ani2Ani.blend;
					break;
				}
			}
		}
		return result;
	}

	public int FindAnimationByName(string id)
	{
		for (int i = 0; i < animSet.Length; i++)
		{
			if (animSet[i].animationName == id)
			{
				return i;
			}
		}
		return -1;
	}

	public float GetAnimationLength(string name)
	{
		if (modelManager == null)
		{
			return 0f;
		}
		Animation animation = modelManager.AnimationGetCurrent();
		if (animation == null)
		{
			return 0f;
		}
		AnimationState animationState = animation[name];
		if (animationState == null)
		{
			return 0f;
		}
		return animationState.length;
	}
}
