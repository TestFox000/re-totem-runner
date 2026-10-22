using UnityEngine;

public class CameraFadeAndTeleportEvent : EventExecutor
{
	private enum eState
	{
		none = 0,
		fadeIn = 1,
		fade = 2,
		fadeOut = 3,
		scene = 4,
		finish = 5
	}

	private eState eventState;

	public float fadeInTime = 0.3f;

	public float fadeTimeLength = 2f;

	public float fadeOutTime = 0.3f;

	public GameObject sceneFolder;

	public float sceneTimeOut = 4f;

	public Transform cameraTiedObject;

	public float cameraTiedOffsetX;

	public BaseCamera.ECameraZoomingType cameraZoomType = BaseCamera.ECameraZoomingType.none;

	public MeshRenderer fader;

	private float eventTimer;

	private void Awake()
	{
		if (fader != null)
		{
			GUtils.SetTransform2D(fader.transform, 0f, 0f);
			GUtils.SetLocalScaleX(fader.transform, Camera.main.aspect * 768f);
		}
	}

	public override void StartExecutor()
	{
		if (eventState == eState.none)
		{
			ChangeState(eState.fadeIn);
		}
		base.StartExecutor();
	}

	private void Update()
	{
		if (eventState == eState.none)
		{
			return;
		}
		switch (eventState)
		{
		case eState.fadeIn:
			eventTimer -= Time.deltaTime;
			if ((bool)fader && fadeInTime > 0f)
			{
				GUtils.SetMaterialAlpha(fader.material, Mathf.Clamp01(1f - eventTimer / fadeInTime));
			}
			if (eventTimer <= 0f)
			{
				ChangeState(eState.fade);
			}
			break;
		case eState.fade:
			eventTimer -= Time.deltaTime;
			if (eventTimer <= 0f)
			{
				ChangeState(eState.fadeOut);
			}
			break;
		case eState.fadeOut:
			eventTimer -= Time.deltaTime;
			if ((bool)fader && fadeOutTime > 0f)
			{
				GUtils.SetMaterialAlpha(fader.material, Mathf.Clamp01(eventTimer / fadeOutTime));
			}
			if (eventTimer <= 0f)
			{
				ChangeState(eState.scene);
			}
			break;
		case eState.scene:
			eventTimer -= Time.deltaTime;
			if (eventTimer <= 0f)
			{
				ChangeState(eState.finish);
			}
			break;
		case eState.finish:
			ChangeState(eState.none);
			LevelCompleteEvent.Launch();
			break;
		}
	}

	private void ChangeState(eState state)
	{
		switch (state)
		{
		case eState.fadeIn:
			PlayerController.instance.DisableController(disable: true);
			PlayerController.instance.PlayerHide(isHide: true);
			ScrollingCamera.instance.LockToTarget(null);
			ScrollingCamera.instance.FreezeCamera(freezed: true);
			GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.disable);
			if (fader != null)
			{
				fader.gameObject.SetActive(true);
				GUtils.SetMaterialAlpha(fader.material, 0f);
			}
			eventTimer = fadeInTime;
			break;
		case eState.fade:
			if (fader != null)
			{
				GUtils.SetMaterialAlpha(fader.material, 1f);
			}
			eventTimer = fadeTimeLength;
			ScrollingCamera.instance.FreezeCamera(freezed: false);
			ScrollingCamera.instance.SetOffsetX(cameraTiedOffsetX);
			ScrollingCamera.instance.LockToTarget(cameraTiedObject);
			ScrollingCamera.instance.SetZoom(cameraZoomType);
			ScrollingCamera.instance.TeleportToTarget();
			break;
		case eState.fadeOut:
			if (sceneFolder != null)
			{
				sceneFolder.SetActiveRecursivelyCompat(state: true);
			}
			eventTimer = fadeOutTime;
			break;
		case eState.scene:
			eventTimer = sceneTimeOut;
			break;
		case eState.finish:
			if (fader != null)
			{
				fader.gameObject.SetActive(false);
			}
			break;
		}
		eventState = state;
	}
}
