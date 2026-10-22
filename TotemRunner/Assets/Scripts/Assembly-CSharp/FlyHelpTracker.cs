using System;
using UnityEngine;

public class FlyHelpTracker : MonoBehaviour
{
	[Serializable]
	public class TrackPoint
	{
		public Vector2 pos = Vector2.zero;

		public float radius = 1f;

		public float power = 1f;
	}

	public TrackPoint[] trackPointsList;

	public float leftBoundX;

	public float rightBoundX;

	private PlayerController refPlayer;

	private Transform refPlayerTransform;

	private void Awake()
	{
		refPlayer = PlayerController.instance;
		refPlayerTransform = refPlayer.transform;
	}

	private void Update()
	{
		if (refPlayerTransform.position.x > leftBoundX && refPlayerTransform.position.x < rightBoundX)
		{
			for (int i = 0; i < trackPointsList.Length; i++)
			{
			}
		}
	}
}
