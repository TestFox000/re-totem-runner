using UnityEngine;

public class ScreenQTE : GuiScreen
{
	private enum eAlterPlayerState
	{
		disable = 0,
		fall = 1,
		run = 2,
		death = 3
	}

	public float timeout = 7f;

	public float deathTime = 2f;

	public ExecutorTapProgress tapProgressExecutor;

	public Vector3 startOffset = Vector3.zero;

	private float timeFollow = -1f;

	public Animation alterPlayerAnimation;

	public string alterPlayerAniNameRun = "idle";

	public float alterPlayerAniSpeedRun = 1f;

	public string alterPlayerAniNameFall = "idle";

	public string alterPlayerAniNameDeath = "idle";

	public GameObject alterPlayerEffect;

	private eAlterPlayerState alterPlayerState;

	public float alterPlayerSpeed = 100f;

	private float alterPlayerFallSpeed = 300f;

	public float alterPlayerFallMaxSpeed = 2000f;

	public float alterPlayerYOffset = 10f;

	private PathTimeline earthPath;

	private bool isSuccessQTE;

	public GameObject refHelpMessage;

	private bool isHelpNeed = true;

	private void Awake()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: false);
	}

	public override void StartShow(Camera refCam)
	{
		base.StartShow(refCam);
		PlayerController.instance.DisableController(disable: true);
		PlayerController.instance.PlayerHide(isHide: true);
		base.gameObject.SetActiveRecursivelyCompat(state: true);
		if (isHelpNeed)
		{
			isHelpNeed = false;
		}
		else if (refHelpMessage != null)
		{
			refHelpMessage.SetActiveRecursivelyCompat(state: false);
		}
		earthPath = PathTimeline.instance;
		if ((bool)alterPlayerAnimation)
		{
			alterPlayerAnimation.transform.position = PlayerController.instance.transform.position + startOffset;
			ChangeAlterPlayerState(eAlterPlayerState.run);
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.qteZooming);
			ScrollingCamera.instance.LockToTarget(alterPlayerAnimation.transform);
			if ((bool)PlayerController.instance.commonBehaviour.effectOfSkinSwitch)
			{
				PlayerController.instance.commonBehaviour.effectOfSkinSwitch.StartEffect();
			}
			if ((bool)alterPlayerAnimation[alterPlayerAniNameRun])
			{
				alterPlayerAnimation[alterPlayerAniNameRun].speed = alterPlayerAniSpeedRun;
			}
		}
		timeFollow = 0f;
		isSuccessQTE = false;
		if ((bool)tapProgressExecutor)
		{
			tapProgressExecutor.ResetCounter();
		}
		if (WorldEvil.instance != null)
		{
			WorldEvil.instance.ExternControl(WorldEvil.eControlId.awayOneShot, 0f);
		}
	}

	public override void StopShow()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		timeFollow = -1f;
		ChangeAlterPlayerState(eAlterPlayerState.disable);
		PlayerController.instance.PlayerHide(isHide: false);
		PlayerController.instance.DisableController(disable: false);
		ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
		if (isSuccessQTE)
		{
			if ((bool)alterPlayerAnimation)
			{
				ScrollingCamera.instance.LockToTarget(PlayerController.instance.transform);
				PlayerController.instance.TeleportToPosition(alterPlayerAnimation.transform.position);
			}
		}
		else
		{
			PlayerController.instance.Spawn();
		}
		GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.game);
	}

	private void Update()
	{
		if (!(timeFollow >= 0f))
		{
			return;
		}
		timeFollow += Time.deltaTime;
		if (timeFollow >= timeout)
		{
			if (isSuccessQTE)
			{
				StopShow();
				return;
			}
			if (alterPlayerState != eAlterPlayerState.death)
			{
				MakePlayerDeath();
			}
			if (timeFollow > timeout + deathTime)
			{
				StopShow();
			}
		}
		else
		{
			if (!alterPlayerAnimation)
			{
				return;
			}
			Vector3 position = alterPlayerAnimation.transform.position;
			if (alterPlayerState == eAlterPlayerState.run)
			{
				position.x += alterPlayerSpeed * Time.deltaTime;
			}
			float num = earthPath.GetHeightByX(position.x) + alterPlayerYOffset;
			if (alterPlayerState == eAlterPlayerState.fall)
			{
				position.y -= alterPlayerFallSpeed * Time.deltaTime;
				alterPlayerFallSpeed += Time.deltaTime * 1000f;
				if (alterPlayerFallSpeed > alterPlayerFallMaxSpeed)
				{
					alterPlayerFallSpeed = alterPlayerFallMaxSpeed;
				}
			}
			if (num < position.y - 10f)
			{
				if (alterPlayerState == eAlterPlayerState.run)
				{
					ChangeAlterPlayerState(eAlterPlayerState.fall);
				}
			}
			else
			{
				if (alterPlayerState == eAlterPlayerState.fall)
				{
					ChangeAlterPlayerState(eAlterPlayerState.run);
				}
				position.y = num;
			}
			alterPlayerAnimation.transform.position = position;
		}
	}

	private void ChangeAlterPlayerState(eAlterPlayerState state)
	{
		if (alterPlayerState == state)
		{
			return;
		}
		switch (state)
		{
		case eAlterPlayerState.disable:
			if ((bool)alterPlayerAnimation)
			{
				alterPlayerAnimation.Stop();
			}
			break;
		case eAlterPlayerState.fall:
			if ((bool)alterPlayerAnimation)
			{
				alterPlayerAnimation.Play(alterPlayerAniNameFall);
			}
			alterPlayerFallSpeed = 0f;
			break;
		case eAlterPlayerState.run:
			if ((bool)alterPlayerAnimation)
			{
				alterPlayerAnimation.Play(alterPlayerAniNameRun);
			}
			if ((bool)alterPlayerEffect)
			{
				alterPlayerEffect.SetActiveRecursivelyCompat(state: true);
			}
			break;
		case eAlterPlayerState.death:
			if ((bool)alterPlayerAnimation)
			{
				alterPlayerAnimation.Play(alterPlayerAniNameDeath);
			}
			if ((bool)alterPlayerEffect)
			{
				alterPlayerEffect.SetActiveRecursivelyCompat(state: false);
			}
			break;
		}
		alterPlayerState = state;
	}

	public void SetSuccess(bool isSuccess)
	{
		isSuccessQTE = isSuccess;
		if (isSuccess)
		{
			StopShow();
		}
	}

	public bool GetSuccessFlag()
	{
		return isSuccessQTE;
	}

	public void SetQteTapCounter(int val)
	{
		if ((bool)tapProgressExecutor)
		{
			tapProgressExecutor.needCount = val;
		}
	}

	public Vector3 GetPlayerPos()
	{
		if ((bool)alterPlayerAnimation)
		{
			return alterPlayerAnimation.transform.position;
		}
		return PlayerController.instance.transform.position;
	}

	public void MakePlayerDeath()
	{
		ChangeAlterPlayerState(eAlterPlayerState.death);
		timeFollow = timeout;
		isSuccessQTE = false;
	}

	public void ForceCloseQteScreen(bool changeSuccessFlag, bool isSuccess)
	{
		if (changeSuccessFlag)
		{
			isSuccessQTE = isSuccess;
		}
		StopShow();
	}

	public bool IsDeath()
	{
		return alterPlayerState == eAlterPlayerState.death;
	}
}
