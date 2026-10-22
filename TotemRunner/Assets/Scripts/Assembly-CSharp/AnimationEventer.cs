using System;
using UnityEngine;

public class AnimationEventer : MonoBehaviour
{
	[Serializable]
	public class AniEventDescribe
	{
		public float timeStart;

		public EventExecutor executor;

		[NonSerialized]
		public bool executed;
	}

	public enum eTimeFollowType
	{
		byDeltaTime = 0,
		fromAnimation = 1
	}

	public Animation listenerAnimation;

	public string animName;

	public string[] animNames;

	private AnimationState animState;

	public AniEventDescribe[] executes;

	private bool makeExecutes;

	private float timeFollow;

	private int nCurExecute;

	public eTimeFollowType timeFollowType;

	public bool loopExecutes;

	private bool IsPlayingAnimation()
	{
		if (animName != null)
		{
			if (listenerAnimation.IsPlaying(animName))
			{
				animState = listenerAnimation[animName];
				return true;
			}
		}
		else
		{
			for (int i = 0; i < animNames.Length; i++)
			{
				if (listenerAnimation.IsPlaying(animNames[i]))
				{
					animState = listenerAnimation[animNames[i]];
					return true;
				}
			}
		}
		return false;
	}

	private void Update()
	{
		if (!makeExecutes)
		{
			if (IsPlayingAnimation())
			{
				makeExecutes = true;
				timeFollow = animState.time;
				nCurExecute = 0;
			}
		}
		else
		{
			switch (timeFollowType)
			{
			case eTimeFollowType.byDeltaTime:
				timeFollow += Time.deltaTime;
				break;
			case eTimeFollowType.fromAnimation:
				timeFollow = animState.time;
				if (timeFollow >= animState.length)
				{
					timeFollow = Mathf.Clamp(timeFollow - animState.length, 0f, animState.length);
					animState.time = timeFollow;
					nCurExecute = 0;
				}
				break;
			}
		}
		if (!makeExecutes)
		{
			return;
		}
		while (nCurExecute < executes.Length && executes[nCurExecute].timeStart < timeFollow)
		{
			executes[nCurExecute++].executor.StartExecutor();
		}
		if (animState.enabled)
		{
			return;
		}
		switch (timeFollowType)
		{
		case eTimeFollowType.byDeltaTime:
			if (nCurExecute >= executes.Length)
			{
				makeExecutes = false;
			}
			break;
		case eTimeFollowType.fromAnimation:
			makeExecutes = false;
			break;
		}
	}
}
