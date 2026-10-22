using System;
using UnityEngine;

public class EnemyCrawler : DangerObject
{
	public enum eCrawlerState
	{
		idle = 0,
		run = 1,
		dig = 2,
		ambush = 3,
		destroy = 4,
		stand = 5,
		inactive = 6
	}

	[Serializable]
	public class TrackPoint
	{
		public eCrawlerState state;

		public float horzSpeed;

		public float endingDistance = -1f;

		public float endingPlayerDistance = -10000f;
	}

	private Vector3 startPosition;

	public float leftBound = 900f;

	public PlayerDeathType playerDeathType = PlayerDeathType.impact;

	public TrackPoint[] track;

	private int curTrackIndex;

	private float curTrackStartX;

	private eCrawlerState curCrawlerState = eCrawlerState.inactive;

	public float badGrassDelay = 0.5f;

	private bool isOnBadGrass;

	private float leftTime = -1f;

	public SlowMoParameters slowMoParameters;

	public string sampleAnimationName = "idle";

	private bool triggerIsActivated;

	public override void DangerAwake()
	{
		SamplingAnimation();
		startPosition = base.transform.position;
		isOnBadGrass = false;
	}

	public override void DangerUpdate()
	{
		if (dangerState == eDangerObjectState.inactive)
		{
			return;
		}
		bool flag = false;
		switch (dangerState)
		{
		case eDangerObjectState.wait:
			flag = true;
			break;
		case eDangerObjectState.run:
			flag = true;
			if (!(leftTime >= 0f))
			{
				break;
			}
			leftTime -= Time.deltaTime;
			if (leftTime <= 0f)
			{
				if (!isOnBadGrass)
				{
					GrassGifManager.TurnOnBadGrass();
					isOnBadGrass = true;
				}
				leftTime = -1f;
			}
			break;
		case eDangerObjectState.prepare:
			if (!refAnim.IsPlaying("dig"))
			{
				ChangeTrackTo(GetNextTrackIndex());
			}
			else
			{
				flag = true;
			}
			break;
		case eDangerObjectState.ambush:
			flag = true;
			break;
		case eDangerObjectState.attack:
			targetController.OnEnterToDangerZone(this, playerDeathType, eEnemyAttackStyle.nonBreakable);
			targetController.OnExitFromDangerZone(this);
			SetObjectState(eDangerObjectState.destroy);
			break;
		}
		if (flag)
		{
			Vector3 position = base.transform.position;
			position.x += track[curTrackIndex].horzSpeed * Time.deltaTime;
			if (dangerState == eDangerObjectState.run)
			{
				GrassGifManager.instance.flowers.InsertBadFlowers(base.transform.position.x, position.x);
			}
			MoveToPos(position);
			bool flag2 = false;
			if (track[curTrackIndex].endingDistance > 0f && base.transform.position.x - curTrackStartX >= track[curTrackIndex].endingDistance)
			{
				flag2 = true;
			}
			if (!flag2 && track[curTrackIndex].endingPlayerDistance > -5000f && base.transform.position.x - targetController.Position.x < track[curTrackIndex].endingPlayerDistance)
			{
				flag2 = true;
			}
			if (flag2)
			{
				ChangeTrackTo(GetNextTrackIndex());
			}
		}
		base.DangerUpdate();
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.bomb;
	}

	public override float GetDangerLeft()
	{
		return startPosition.x - leftBound;
	}

	public override float GetDangerLeftInEditor()
	{
		return base.transform.position.x - leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		curTrackIndex = -1;
		ChangeTrackTo(0);
		triggerIsActivated = false;
	}

	private int GetNextTrackIndex()
	{
		int num = curTrackIndex + 1;
		if (num >= track.Length)
		{
			num = track.Length - 1;
		}
		return num;
	}

	private void ChangeTrackTo(int trackIndex)
	{
		if (curTrackIndex == trackIndex)
		{
			return;
		}
		if (trackIndex < track.Length)
		{
			eCrawlerState eCrawlerState2 = curCrawlerState;
			curCrawlerState = track[trackIndex].state;
			switch (curCrawlerState)
			{
			case eCrawlerState.idle:
				if (eCrawlerState2 == eCrawlerState.stand)
				{
					dangerState = eDangerObjectState.unused;
				}
				SetObjectState(eDangerObjectState.wait);
				break;
			case eCrawlerState.run:
				SetObjectState(eDangerObjectState.run);
				break;
			case eCrawlerState.dig:
				SetObjectState(eDangerObjectState.prepare);
				if (slowMoParameters.scale != 1f)
				{
					TimeController.StartSlowMo(slowMoParameters.scale, slowMoParameters.timeFadeIn, slowMoParameters.timeFixed, slowMoParameters.timeFadeOut, eSlowMoMode.normal);
				}
				break;
			case eCrawlerState.ambush:
				SetObjectState(eDangerObjectState.ambush);
				break;
			case eCrawlerState.destroy:
				SetObjectState(eDangerObjectState.destroy);
				break;
			case eCrawlerState.stand:
				if (eCrawlerState2 == eCrawlerState.idle)
				{
					dangerState = eDangerObjectState.unused;
				}
				SetObjectState(eDangerObjectState.wait);
				break;
			case eCrawlerState.inactive:
				SetObjectState(eDangerObjectState.inactive);
				break;
			}
		}
		else
		{
			curCrawlerState = eCrawlerState.inactive;
		}
		curTrackStartX = base.transform.position.x;
		curTrackIndex = trackIndex;
	}

	private void SamplingAnimation()
	{
		refAnim.Play(sampleAnimationName);
		refAnim.Sample();
		refAnim.Stop();
	}

	public override void OnExitFromState(eDangerObjectState state)
	{
		if (state == eDangerObjectState.run && isOnBadGrass)
		{
			GrassGifManager.TurnOffBadGrass();
			isOnBadGrass = false;
		}
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			SamplingAnimation();
			triggerIsActivated = false;
			break;
		case eDangerObjectState.wait:
			if (curCrawlerState == eCrawlerState.stand)
			{
				refAnim.Play("stand");
			}
			else
			{
				refAnim.Play("idle");
			}
			break;
		case eDangerObjectState.destroy:
			refAnim.Play((dangerState != eDangerObjectState.attack) ? "explosion" : "explosion_fast");
			break;
		case eDangerObjectState.run:
			refAnim.CrossFade("run");
			leftTime = badGrassDelay;
			break;
		case eDangerObjectState.prepare:
			refAnim.CrossFade("dig");
			break;
		case eDangerObjectState.ambush:
			refAnim.CrossFade("ambush");
			break;
		case eDangerObjectState.attack:
			break;
		case eDangerObjectState.chase:
		case eDangerObjectState.turnToPlayer:
		case eDangerObjectState.attackAlternative:
		case eDangerObjectState.hide:
		case eDangerObjectState.teleport:
		case eDangerObjectState.maneuver:
			break;
		}
	}

	public override void OnResetDanger()
	{
		SetObjectState(eDangerObjectState.inactive);
		MoveToPos(startPosition);
	}

	public void OnTriggerEnter(Collider other)
	{
		if (dangerState == eDangerObjectState.inactive)
		{
			return;
		}
		if (dangerState == eDangerObjectState.destroy)
		{
			if (!triggerIsActivated)
			{
				triggerIsActivated = true;
				targetController.OnEnterToDangerZone(this, playerDeathType, eEnemyAttackStyle.nonBreakable);
				targetController.OnExitFromDangerZone(this);
			}
		}
		else
		{
			triggerIsActivated = true;
			if (targetController != null && targetController.HaveCollider(other))
			{
				SetObjectState(eDangerObjectState.attack);
			}
			else
			{
				Debug.LogWarning("Other collider enter!");
			}
		}
	}

	public override void DoVisible(bool makeVisible)
	{
		base.DoVisible(makeVisible);
		if (!makeVisible && isOnBadGrass)
		{
			GrassGifManager.TurnOffBadGrass();
			isOnBadGrass = false;
		}
	}
}
