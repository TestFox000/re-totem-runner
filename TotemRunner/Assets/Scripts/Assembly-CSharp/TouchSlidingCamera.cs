using UnityEngine;

public class TouchSlidingCamera : BaseCamera
{
	public enum eTouchState
	{
		start = 0,
		process = 1,
		finish = 2
	}

	public float slideToOffsetMultiply = 800f;

	public float snapOffset = 150f;

	public float snapStep = 1000f;

	public float outMaxOffset = 500f;

	private int lastSnapIndex = 9;

	private int currentSnapIndex;

	private Transform refTransform;

	private bool isSliding;

	private float slidingOffsetCurrent;

	private float slidingOffsetStart;

	private Vector2 startTouchPoint;

	public float minSlidingOffset = 120f;

	private float slideSpeedMax = 2000f;

	private float slideSpeedAccelerate = 8000f;

	private float slideSpeedCurrent;

	public AchievementSystem.eAchievementID[] enemyList = new AchievementSystem.eAchievementID[10]
	{
		AchievementSystem.eAchievementID.enc_golem,
		AchievementSystem.eAchievementID.enc_star,
		AchievementSystem.eAchievementID.enc_poacher,
		AchievementSystem.eAchievementID.enc_brain,
		AchievementSystem.eAchievementID.enc_crawler,
		AchievementSystem.eAchievementID.enc_flybomb,
		AchievementSystem.eAchievementID.enc_hornet,
		AchievementSystem.eAchievementID.enc_spider,
		AchievementSystem.eAchievementID.enc_titan,
		AchievementSystem.eAchievementID.enc_evilsoul
	};

	public override void Awake()
	{
		base.Awake();
		isSliding = false;
		refTransform = base.transform;
		Vector3 position = refTransform.position;
		position.x = snapOffset + snapStep * (float)currentSnapIndex;
		refTransform.position = position;
	}

	private void Start()
	{
		if (enemyList == null || enemyList.Length == 0)
		{
			lastSnapIndex = 0;
			return;
		}
		int i;
		for (i = 0; i < enemyList.Length && AchievementSystem.UtiliteIsReachAchievement(enemyList[i]); i++)
		{
		}
		lastSnapIndex = i;
	}

	public override void LateUpdate()
	{
		eTouchState touchState = GetTouchState();
		if (isSliding)
		{
			if (touchState == eTouchState.finish)
			{
				FinishSliding();
			}
			else
			{
				ProcessSliding();
			}
		}
		else if (touchState == eTouchState.start)
		{
			StartSliding();
		}
		else if (slidingOffsetCurrent != 0f)
		{
			AutomaticSliding();
		}
		if (GameInput.IsBackPressed())
		{
			BaseCamera.GLaunchLoading(1);
		}
		base.LateUpdate();
	}

	private void AutomaticSliding()
	{
		float num = slidingOffsetCurrent + slideSpeedCurrent * Time.deltaTime;
		if (slideSpeedCurrent < 0f)
		{
			slideSpeedCurrent -= Time.deltaTime * slideSpeedAccelerate;
			if (slideSpeedCurrent < 0f - slideSpeedMax)
			{
				slideSpeedCurrent = 0f - slideSpeedMax;
			}
		}
		else
		{
			slideSpeedCurrent += Time.deltaTime * slideSpeedAccelerate;
			if (slideSpeedCurrent > slideSpeedMax)
			{
				slideSpeedCurrent = slideSpeedMax;
			}
		}
		if (num >= snapStep)
		{
			num = 0f;
			currentSnapIndex++;
			if (currentSnapIndex > lastSnapIndex)
			{
				currentSnapIndex = lastSnapIndex;
			}
			slideSpeedCurrent = 0f;
		}
		else if (num <= 0f - snapStep)
		{
			num = 0f;
			currentSnapIndex--;
			if (currentSnapIndex < 0)
			{
				currentSnapIndex = 0;
			}
			slideSpeedCurrent = 0f;
		}
		else if (slideSpeedCurrent < 0f && slidingOffsetCurrent > 0f)
		{
			if (num <= 0f)
			{
				num = 0f;
				slideSpeedCurrent = 0f;
			}
		}
		else if (slideSpeedCurrent > 0f && slidingOffsetCurrent < 0f && num >= 0f)
		{
			num = 0f;
			slideSpeedCurrent = 0f;
		}
		slidingOffsetCurrent = num;
		UpdateCameraPos();
	}

	private void StartSliding()
	{
		isSliding = true;
		slidingOffsetStart = slidingOffsetCurrent;
		startTouchPoint = GetTouchPos();
	}

	private void ProcessSliding()
	{
		Vector2 touchPos = GetTouchPos();
		slidingOffsetCurrent = slidingOffsetStart + (startTouchPoint.x - touchPos.x) * slideToOffsetMultiply;
		if (currentSnapIndex == 0 && slidingOffsetCurrent < 0f - outMaxOffset)
		{
			slidingOffsetCurrent = 0f - outMaxOffset;
		}
		else if (currentSnapIndex == lastSnapIndex && slidingOffsetCurrent > outMaxOffset)
		{
			slidingOffsetCurrent = outMaxOffset;
		}
		UpdateCameraPos();
	}

	private void UpdateCameraPos()
	{
		Vector3 position = refTransform.position;
		position.x = snapOffset + snapStep * (float)currentSnapIndex + slidingOffsetCurrent;
		refTransform.position = position;
	}

	private void FinishSliding()
	{
		Vector2 touchPos = GetTouchPos();
		float num = slidingOffsetStart + (startTouchPoint.x - touchPos.x) * slideToOffsetMultiply;
		slideSpeedCurrent = num - slidingOffsetCurrent;
		if (Time.deltaTime > 0.001f)
		{
			slideSpeedCurrent /= Time.deltaTime;
		}
		bool flag = true;
		if (slidingOffsetCurrent >= minSlidingOffset || slidingOffsetCurrent <= 0f - minSlidingOffset)
		{
			flag = false;
		}
		if (currentSnapIndex == 0 && slideSpeedCurrent < 0f)
		{
			flag = true;
		}
		else if (currentSnapIndex == lastSnapIndex && slideSpeedCurrent >= 0f)
		{
			flag = true;
		}
		if (flag && slidingOffsetCurrent != 0f)
		{
			if (slidingOffsetCurrent > 0f && slideSpeedCurrent >= 0f)
			{
				slideSpeedCurrent = -100f;
			}
			if (slidingOffsetCurrent < 0f && slideSpeedCurrent <= 0f)
			{
				slideSpeedCurrent = 100f;
			}
		}
		isSliding = false;
	}

	private Vector2 GetTouchPos()
	{
		Vector2 result = Vector2.zero;
		if (Input.touchCount > 0)
		{
			result = Input.touches[0].position;
		}
		result.x /= Screen.width;
		result.y /= Screen.height;
		return result;
	}

	private eTouchState GetTouchState()
	{
		eTouchState result = eTouchState.process;
		if (Input.touchCount > 0)
		{
			if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
			{
				result = eTouchState.finish;
			}
			else if (!isSliding)
			{
				result = eTouchState.start;
			}
		}
		else
		{
			result = eTouchState.finish;
		}
		return result;
	}

	public float GetSnapFactor()
	{
		return slidingOffsetCurrent * 0.01f;
	}

	public int GetSnapIndex()
	{
		return currentSnapIndex;
	}
}
