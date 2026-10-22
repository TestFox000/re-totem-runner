using System;
using UnityEngine;

public class AnimationChainTrigger : BaseTrigger
{
	[Serializable]
	public class AnimationEntity
	{
		public Animation objectAnimation;

		public string startAnimationName;

		public float followLength;

		public Vector2 objOffset = Vector2.zero;

		public float timeOut = -1f;

		public bool doSyncAnimation = true;

		public AudioClip sound;

		public float soundVolume = 1f;

		public float soundInterval;

		public float[] aniSyncPoints;
	}

	public AnimationEntity[] list;

	private int nCurAnimIndex = -1;

	private Vector3 startPoint = Vector3.zero;

	private Vector3 curPoint = Vector3.zero;

	private float relativePlayerX;

	private float relativePlayerMul;

	private Vector3 prevPos = Vector3.zero;

	private Vector3 nextPos = Vector3.zero;

	private float waitTimeLeft = -1f;

	private float soundTimeLeft;

	private void Awake()
	{
		if (list.Length > 0)
		{
			startPoint = list[0].objectAnimation.transform.position;
		}
		AnimationEntity[] array = list;
		foreach (AnimationEntity animationEntity in array)
		{
			animationEntity.objectAnimation.gameObject.SetActiveRecursivelyCompat(state: false);
		}
	}

	public override void Update()
	{
		if (nCurAnimIndex >= 0)
		{
			bool flag = false;
			if (relativePlayerMul > 0f)
			{
				float num = PlayerController.instance.transform.position.x - relativePlayerX;
				num *= relativePlayerMul;
				curPoint = Vector3.Lerp(prevPos, nextPos, num);
				list[nCurAnimIndex].objectAnimation.transform.position = curPoint;
				if (!string.IsNullOrEmpty(list[nCurAnimIndex].startAnimationName) && list[nCurAnimIndex].doSyncAnimation)
				{
					list[nCurAnimIndex].objectAnimation[list[nCurAnimIndex].startAnimationName].normalizedTime = num;
				}
				if (num >= 1f)
				{
					flag = true;
				}
			}
			else if (waitTimeLeft >= 0f)
			{
				waitTimeLeft -= Time.deltaTime;
				if (waitTimeLeft <= 0f)
				{
					flag = true;
					waitTimeLeft = -1f;
				}
			}
			else if (!list[nCurAnimIndex].objectAnimation.isPlaying)
			{
				flag = true;
			}
			if (flag)
			{
				GoToNextEntity();
			}
			else if (soundTimeLeft > 0f && list[nCurAnimIndex].sound != null)
			{
				soundTimeLeft -= Time.deltaTime;
				if (soundTimeLeft <= 0f)
				{
					soundTimeLeft += list[nCurAnimIndex].soundInterval;
					LaunchCurrentSound();
				}
			}
		}
		base.Update();
	}

	private void LaunchCurrentSound()
	{
		Vector3 pos = curPoint;
		pos.z = ScrollingCamera.instance.transform.position.z + 10f;
		SoundEnvironment.Launch3DSound(list[nCurAnimIndex].sound, list[nCurAnimIndex].soundVolume, pos, 0f, isLoop: false, null);
	}

	private void GoToNextEntity()
	{
		int num = nCurAnimIndex;
		if (nCurAnimIndex < 0)
		{
			nCurAnimIndex = 0;
			curPoint = startPoint;
			relativePlayerX = base.transform.position.x;
		}
		else
		{
			nCurAnimIndex++;
			relativePlayerX = PlayerController.instance.transform.position.x;
		}
		if (nCurAnimIndex >= list.Length)
		{
			nCurAnimIndex = -1;
		}
		if (num >= 0 && (nCurAnimIndex < 0 || list[nCurAnimIndex].objectAnimation != list[num].objectAnimation))
		{
			list[num].objectAnimation.gameObject.SetActiveRecursivelyCompat(state: false);
		}
		if (nCurAnimIndex < 0)
		{
			return;
		}
		waitTimeLeft = list[nCurAnimIndex].timeOut;
		list[nCurAnimIndex].objectAnimation.transform.position = curPoint;
		bool flag = false;
		if (num < 0 || list[nCurAnimIndex].objectAnimation != list[num].objectAnimation)
		{
			list[nCurAnimIndex].objectAnimation.gameObject.SetActiveRecursivelyCompat(state: true);
		}
		else
		{
			flag = true;
		}
		if (!string.IsNullOrEmpty(list[nCurAnimIndex].startAnimationName))
		{
			if (flag)
			{
				if (list[nCurAnimIndex].startAnimationName != list[num].startAnimationName)
				{
					if (list[nCurAnimIndex].aniSyncPoints != null && list[nCurAnimIndex].aniSyncPoints.Length > 0)
					{
						Utilites.AnimSyncCrossFade(list[nCurAnimIndex].objectAnimation, list[num].startAnimationName, list[nCurAnimIndex].startAnimationName, list[nCurAnimIndex].aniSyncPoints);
					}
					else
					{
						list[nCurAnimIndex].objectAnimation.CrossFade(list[nCurAnimIndex].startAnimationName, 0.3f);
					}
				}
			}
			else
			{
				list[nCurAnimIndex].objectAnimation.Stop();
				list[nCurAnimIndex].objectAnimation.Play(list[nCurAnimIndex].startAnimationName);
			}
		}
		if (list[nCurAnimIndex].followLength > 0f)
		{
			relativePlayerMul = 1f / list[nCurAnimIndex].followLength;
			prevPos = curPoint;
			nextPos = prevPos;
			nextPos.x += list[nCurAnimIndex].followLength + list[nCurAnimIndex].objOffset.x;
			nextPos.y += list[nCurAnimIndex].objOffset.y;
		}
		else
		{
			relativePlayerMul = 0f;
		}
		soundTimeLeft = 0f;
		if (list[nCurAnimIndex].sound != null)
		{
			LaunchCurrentSound();
			if (list[nCurAnimIndex].soundInterval > 0f)
			{
				soundTimeLeft = list[nCurAnimIndex].soundInterval;
			}
		}
	}

	public override void ExecuteTrigger()
	{
		if (nCurAnimIndex < 0)
		{
			RestartObjectsList.IncludeRestartObject(this);
			GoToNextEntity();
		}
	}

	public override bool RestartToX(float x)
	{
		if (nCurAnimIndex >= 0)
		{
			list[nCurAnimIndex].objectAnimation.gameObject.SetActiveRecursivelyCompat(state: false);
		}
		nCurAnimIndex = -1;
		return true;
	}
}
