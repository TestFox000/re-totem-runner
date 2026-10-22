using System;
using UnityEngine;

public class DangerLevitate : DangerObject
{
	[Serializable]
	public class TrackPoint
	{
		public Vector2 pos = Vector2.zero;

		public float reachTime;

		public float pauseTime;
	}

	public enum eTrackType
	{
		clamp = 0,
		loop = 1
	}

	public PlayerDeathType targetDeathType;

	public TrackPoint[] trackPoints;

	public eTrackType trackType;

	private int currentTrackPoint = -1;

	private float trackTime;

	private float trackPauseTimeLeft;

	private Vector2 currentSegmentStartPoint = Vector2.zero;

	private bool targetIsAlive = true;

	private Vector3 startPoint = Vector3.zero;

	private static float leftBound = -800f;

	private void TrackStart()
	{
		currentTrackPoint = 0;
		trackTime = 0f;
		if (trackPoints.Length > currentTrackPoint)
		{
			trackPauseTimeLeft = trackPoints[currentTrackPoint].pauseTime;
			currentSegmentStartPoint = trackPoints[currentTrackPoint].pos;
		}
		else
		{
			trackPauseTimeLeft = 0f;
			currentSegmentStartPoint = Vector2.zero;
		}
	}

	private int NextPointIndex(int curIndex)
	{
		int num = curIndex + 1;
		if (num >= trackPoints.Length)
		{
			switch (trackType)
			{
			case eTrackType.clamp:
				num = -1;
				break;
			case eTrackType.loop:
				num = 0;
				break;
			}
		}
		return num;
	}

	private void TrackUpdate()
	{
		if (trackPauseTimeLeft > 0f)
		{
			trackPauseTimeLeft -= Time.deltaTime;
			if (trackPauseTimeLeft <= 0f)
			{
				trackTime -= trackPauseTimeLeft;
				trackPauseTimeLeft = 0f;
			}
		}
		else
		{
			trackTime += Time.deltaTime;
		}
		if (trackPoints.Length <= 0)
		{
			return;
		}
		Vector2 vector = currentSegmentStartPoint;
		if (trackPoints.Length > 1)
		{
			int num = NextPointIndex(currentTrackPoint);
			while (num >= 0 && trackTime >= trackPoints[num].reachTime)
			{
				TrackChangeCurrentPoint(num);
				num = NextPointIndex(num);
			}
			vector = ((num < 0) ? currentSegmentStartPoint : ((!(trackPoints[num].reachTime > 0.01f)) ? (currentSegmentStartPoint + trackPoints[num].pos) : (currentSegmentStartPoint + trackPoints[num].pos * Mathf.Clamp01(trackTime / trackPoints[num].reachTime))));
		}
		MoveToPos(startPoint + new Vector3(vector.x, vector.y, 0f));
	}

	private void TrackChangeCurrentPoint(int nextPointIndex)
	{
		trackTime -= trackPoints[nextPointIndex].reachTime;
		trackTime -= trackPoints[nextPointIndex].pauseTime;
		currentSegmentStartPoint += trackPoints[nextPointIndex].pos;
		if (trackTime < 0f)
		{
			trackPauseTimeLeft -= trackTime;
			trackTime = 0f;
		}
		currentTrackPoint = nextPointIndex;
	}

	public override void DangerAwake()
	{
		startPoint = base.transform.position;
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.inactive);
	}

	public override void DangerUpdate()
	{
		if (dangerState == eDangerObjectState.inactive)
		{
			return;
		}
		switch (dangerState)
		{
		case eDangerObjectState.attack:
			if (targetIsAlive)
			{
				targetController.OnEnterToDangerZone(this, targetDeathType, eEnemyAttackStyle.nonBreakable);
				targetController.OnExitFromDangerZone(this);
				targetIsAlive = false;
			}
			break;
		case eDangerObjectState.wait:
			TrackUpdate();
			break;
		}
		base.DangerUpdate();
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.obstacle;
	}

	public override float GetDangerLeft()
	{
		return base.transform.position.x + leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		SetObjectState(eDangerObjectState.wait);
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			refAnim.Play("idle");
			refAnim.Sample();
			refAnim.Stop();
			break;
		case eDangerObjectState.wait:
			refAnim.Play("idle");
			TrackStart();
			break;
		case eDangerObjectState.attack:
			refAnim.CrossFade("attack", 0.2f);
			targetIsAlive = true;
			break;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (dangerState == eDangerObjectState.wait)
		{
			if (targetController != null && targetController.HaveCollider(other))
			{
				SetObjectState(eDangerObjectState.attack);
				return;
			}
			Debug.LogWarning(string.Concat("Other collider ", other, " entered instead target ", targetController));
		}
	}

	public override void OnResetDanger()
	{
		base.transform.position = startPoint;
		SetObjectState(eDangerObjectState.inactive);
	}

	public override bool OnPlayerReenter()
	{
		if (targetController == null)
		{
			return false;
		}
		if (targetController.Position.x < startPoint.x)
		{
			MoveToPos(startPoint);
			if (targetController.Position.x >= GetDangerLeft())
			{
				SetObjectState(eDangerObjectState.wait);
			}
			return true;
		}
		return false;
	}
}
