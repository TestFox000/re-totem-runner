using System;
using UnityEngine;

public class MovingTrackTimeline : TriggeredBehaviour
{
	public enum eTrackType
	{
		loop = 0,
		once = 1
	}

	[Serializable]
	public class TrackPoint
	{
		public Vector2 offset = Vector2.zero;

		public float timeToNext;
	}

	public bool startAutomatically;

	public float startTime;

	public TrackPoint[] track;

	public eTrackType trackType;

	public float turnOffDistance = 600f;

	public AudioClip soundClip;

	private Transform refTransform;

	private float currentTime;

	private int currentTrack;

	private Vector3 startPosition = Vector3.zero;

	private Vector2 beginOffset = Vector2.zero;

	private void Awake()
	{
		refTransform = base.transform;
		startPosition = refTransform.position;
		if (startAutomatically)
		{
			StartExecute();
		}
		else
		{
			base.enabled = false;
		}
		ResetTrackToBegin();
	}

	private void GotoNextTrackPoint()
	{
		currentTime -= track[currentTrack].timeToNext;
		beginOffset += track[currentTrack].offset;
		currentTrack++;
		if (currentTrack >= track.Length)
		{
			switch (trackType)
			{
			case eTrackType.loop:
				currentTrack = 0;
				beginOffset = new Vector2(startPosition.x, startPosition.y);
				break;
			case eTrackType.once:
				StopExecute();
				break;
			}
		}
	}

	private void Update()
	{
		if (!startAutomatically)
		{
			float num = ScrollingCamera.leftCameraBound - turnOffDistance;
			if (refTransform.position.x <= num)
			{
				StopExecute();
			}
		}
		currentTime += Time.deltaTime;
		SetObjectAccordingToTime();
	}

	private void SetObjectAccordingToTime()
	{
		while (currentTrack < track.Length && currentTime >= track[currentTrack].timeToNext)
		{
			GotoNextTrackPoint();
		}
		Vector2 vector = beginOffset;
		if (currentTrack < track.Length)
		{
			float num = Mathf.Clamp01(currentTime / track[currentTrack].timeToNext);
			vector += num * track[currentTrack].offset;
		}
		GUtils.SetTransform2D(refTransform, vector.x, vector.y);
	}

	public override void StartExecute()
	{
		if (!base.enabled && soundClip != null)
		{
			SoundEnvironment.Launch3DSound(soundClip, 1f, base.transform.position, 0f, isLoop: true, null);
		}
		base.enabled = true;
		ResetTrackToBegin();
	}

	public void StopExecute()
	{
		if (base.enabled && soundClip != null)
		{
			SoundEnvironment.Stop3DSound(soundClip, waitForMute: true);
		}
		base.enabled = false;
	}

	public override void RestartToX(float x)
	{
		base.enabled = false;
		ResetTrackToBegin();
	}

	private void ResetTrackToBegin()
	{
		currentTime = startTime;
		currentTrack = 0;
		beginOffset = new Vector2(startPosition.x, startPosition.y);
		SetObjectAccordingToTime();
	}
}
