using UnityEngine;

public class SwitchSkinToStarTrigger : MonoBehaviour
{
	public enum eSwitchSkinState
	{
		none = 0,
		animation = 1,
		fadeIn = 2,
		fade = 3,
		fadeOut = 4,
		finish = 5
	}

	public Animation animObject;

	public string animName;

	public float fadeInTime = 0.3f;

	public float fadeTimeLength = 2f;

	public float fadeOutTime = 0.3f;

	public AudioClip sound;

	public float soundVolume = 1f;

	public float soundDelay = -1f;

	public Transform teleportToObject;

	public MeshRenderer fader;

	public Transform camTiedTransform;

	public float camAnimTiedOffset = 100f;

	public float camStarOffset = 100f;

	public float cameraZoomingTimeDelay = -1f;

	public BaseCamera.ECameraZoomingType cameraZoomingType = BaseCamera.ECameraZoomingType.titanInsidingZoom;

	private eSwitchSkinState switchSkinState;

	private float switchSkinTimer;

	private float cameraZoomingTimeleft = -1f;

	private float soundDelayLeft = -1f;

	private void Awake()
	{
		if (fader != null)
		{
			GUtils.SetTransform2D(fader.transform, 0f, 0f);
			GUtils.SetLocalScaleX(fader.transform, Camera.main.aspect * 768f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (switchSkinState == eSwitchSkinState.none)
		{
			ChangeState(eSwitchSkinState.animation);
		}
	}

	private void Update()
	{
		if (switchSkinState == eSwitchSkinState.none)
		{
			return;
		}
		if (cameraZoomingTimeleft >= 0f)
		{
			cameraZoomingTimeleft -= Time.deltaTime;
			if (cameraZoomingTimeleft <= 0f)
			{
				cameraZoomingTimeleft = -1f;
				ScrollingCamera.instance.SetZoom(cameraZoomingType);
			}
		}
		if (soundDelayLeft >= 0f)
		{
			soundDelayLeft -= Time.deltaTime;
			if (soundDelayLeft <= 0f)
			{
				soundDelayLeft = -1f;
				if ((bool)sound)
				{
					SoundEnvironment.Launch2DSound(sound, soundVolume, 0f, -1f);
				}
			}
		}
		switch (switchSkinState)
		{
		case eSwitchSkinState.animation:
			if (!animObject || !animObject.isPlaying)
			{
				ChangeState(eSwitchSkinState.fadeIn);
			}
			break;
		case eSwitchSkinState.fadeIn:
			switchSkinTimer -= Time.deltaTime;
			if ((bool)fader && fadeInTime > 0f)
			{
				GUtils.SetMaterialAlpha(fader.material, Mathf.Clamp01(1f - switchSkinTimer / fadeInTime));
			}
			if (switchSkinTimer <= 0f)
			{
				ChangeState(eSwitchSkinState.fade);
			}
			break;
		case eSwitchSkinState.fade:
			switchSkinTimer -= Time.deltaTime;
			if (switchSkinTimer <= 0f)
			{
				ChangeState(eSwitchSkinState.fadeOut);
			}
			break;
		case eSwitchSkinState.fadeOut:
			switchSkinTimer -= Time.deltaTime;
			if ((bool)fader && fadeOutTime > 0f)
			{
				GUtils.SetMaterialAlpha(fader.material, Mathf.Clamp01(switchSkinTimer / fadeOutTime));
			}
			if (switchSkinTimer <= 0f)
			{
				ChangeState(eSwitchSkinState.finish);
			}
			break;
		case eSwitchSkinState.finish:
			ChangeState(eSwitchSkinState.none);
			break;
		}
	}

	private void ChangeState(eSwitchSkinState state)
	{
		switch (state)
		{
		case eSwitchSkinState.animation:
			LevelAttributes.instance.skinEnabled[0] = false;
			LevelAttributes.instance.skinEnabled[1] = false;
			LevelAttributes.instance.skinEnabled[2] = false;
			LevelAttributes.instance.skinEnabled[3] = true;
			LevelAttributes.instance.skinEnabled[4] = false;
			PlayerController.instance.ChangeSkin(EPlayerSkin.Star);
			PlayerController.instance.DisableController(disable: true);
			PlayerController.instance.PlayerHide(isHide: true);
			ScrollingCamera.instance.LockToTarget(camTiedTransform);
			ScrollingCamera.instance.SetOffsetX(camAnimTiedOffset);
			GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.disable);
			if (animObject != null)
			{
				animObject.gameObject.SetActiveRecursivelyCompat(state: true);
				animObject.Play(animName);
			}
			cameraZoomingTimeleft = cameraZoomingTimeDelay;
			soundDelayLeft = soundDelay;
			GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: true);
			GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: true);
			GameInput.instance.HideControl(eGameInputButton.powerUp, hideFlag: true);
			break;
		case eSwitchSkinState.fadeIn:
			if (animObject != null)
			{
				animObject.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			if (fader != null)
			{
				fader.gameObject.SetActive(true);
				GUtils.SetMaterialAlpha(fader.material, 0f);
			}
			switchSkinTimer = fadeInTime;
			if (sound != null && soundDelay < 0f)
			{
				SoundEnvironment.Launch2DSound(sound, soundVolume, 0f, -1f);
			}
			break;
		case eSwitchSkinState.fade:
			if (fader != null)
			{
				GUtils.SetMaterialAlpha(fader.material, 1f);
			}
			switchSkinTimer = fadeTimeLength;
			break;
		case eSwitchSkinState.fadeOut:
			if (teleportToObject != null)
			{
				PlayerController.instance.EnterToCheckPoint(teleportToObject);
				GameSettings.SaveRestoreData();
				PlayerController.instance.TeleportToPosition(teleportToObject.transform.position);
			}
			PlayerController.instance.PlayerHide(isHide: false);
			ScrollingCamera.instance.LockToTarget(PlayerController.instance.transform);
			ScrollingCamera.instance.SetOffsetX(camStarOffset);
			ScrollingCamera.instance.TeleportToTarget();
			switchSkinTimer = fadeOutTime;
			GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.game);
			GuiGameHud.instance.DisableGrassGUI(isDisable: true);
			break;
		case eSwitchSkinState.finish:
			if (fader != null)
			{
				fader.gameObject.SetActive(false);
			}
			PlayerController.instance.DisableController(disable: false);
			break;
		}
		switchSkinState = state;
	}
}
