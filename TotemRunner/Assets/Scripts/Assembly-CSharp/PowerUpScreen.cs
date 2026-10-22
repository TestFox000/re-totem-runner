using UnityEngine;

public class PowerUpScreen : GuiScreen
{
	public float timeout = 2f;

	public Animation refAnimation;

	public float timeoutAnimation;

	private float prevAnimationTime = 1f;

	private float startRealTime = -100f;

	private int touchIndex = -1;

	private Vector2 startTouchPoint = Vector2.zero;

	private int startTouchCount;

	private void Awake()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: false);
	}

	public override void StartShow(Camera refCam)
	{
		base.StartShow(refCam);
		base.gameObject.SetActiveRecursivelyCompat(state: true);
		if ((bool)refAnimation)
		{
			refAnimation.Play();
			prevAnimationTime = Time.realtimeSinceStartup;
		}
		PlayerController.instance.DisableInput(isDisable: true);
		TimeController.StartSlowMo(0.2f, 0f, 1000f, 0f, eSlowMoMode.nonBreakable);
		startRealTime = Time.realtimeSinceStartup;
		touchIndex = -1;
	}

	public override void StopShow()
	{
		PlayerController.instance.DisableInput(isDisable: false);
		TimeController.StopSlowMo(forced: true);
		if ((bool)refAnimation)
		{
			refAnimation.Stop();
		}
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.game);
	}

	private void Update()
	{
		if (timeout >= 0f && Time.realtimeSinceStartup - startRealTime >= timeout)
		{
			if (touchIndex < 0 || !CheckPowerUpOn(GetTouchPoint(touchIndex)))
			{
				StopShow();
			}
			return;
		}
		if ((bool)refAnimation)
		{
			AnimationState animationState = refAnimation[refAnimation.clip.name];
			animationState.time += Time.realtimeSinceStartup - prevAnimationTime;
			prevAnimationTime = Time.realtimeSinceStartup;
			if (animationState.time >= animationState.length + timeoutAnimation)
			{
				animationState.time = 0f;
				refAnimation.Play();
			}
		}
		if (touchIndex < 0)
		{
			touchIndex = GetTouchStart();
			if (touchIndex >= 0)
			{
				startTouchPoint = GetTouchPoint(touchIndex);
			}
		}
		else if (IsTouchEnd(touchIndex))
		{
			CheckPowerUpOn(GetTouchPoint(touchIndex));
			touchIndex = -1;
		}
	}

	private int GetTouchStart()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			if (Input.GetTouch(i).phase == TouchPhase.Began)
			{
				startTouchCount = Input.touchCount;
				return i;
			}
		}
		return -1;
	}

	private bool IsTouchEnd(int idx)
	{
		if (startTouchCount != Input.touchCount)
		{
			return true;
		}
		if (Input.GetTouch(idx).phase == TouchPhase.Ended || Input.GetTouch(idx).phase == TouchPhase.Canceled)
		{
			return true;
		}
		return false;
	}

	private Vector2 GetTouchPoint(int idx)
	{
		return Input.GetTouch(idx).position;
	}

	private bool CheckPowerUpOn(Vector2 endPos)
	{
		if (endPos.x - startTouchPoint.x > refCamera.pixelWidth * 0.1f)
		{
			if (PlayerController.instance.ChangeSkin(EPlayerSkin.Ghost))
			{
				GuiGameHud.UpdateCrystalProgress();
			}
			StopShow();
			return true;
		}
		return false;
	}
}
