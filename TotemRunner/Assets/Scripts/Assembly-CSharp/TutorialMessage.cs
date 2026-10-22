using UnityEngine;

public class TutorialMessage : MonoBehaviour
{
	public enum eBreakTutorialType
	{
		anyAction = 0,
		boarForm = 1,
		eagleForm = 2,
		grass = 3,
		speedUp = 4,
		flying = 5,
		animalForm = 6,
		powerUp = 7,
		powerUpSliding = 8,
		touchScreen = 9,
		verticalControl = 10,
		chooseControl = 11,
		touchScreenUp = 12
	}

	public MeshRenderer refMessageRender;

	public Texture2D backPictTexture;

	public TextAsset backPicture;

	public TextAsset backPictureAlt;

	public TextAsset[] backPictList;

	public TextAsset[] backPictAltList;

	public float backFrameSpeed = 1f;

	private int backFrame;

	private float backFramePrevTime;

	public eBreakTutorialType breakAction;

	public Animation refHelpAnimation;

	public float timeoutAnimation = 3f;

	public Animation animationEffect;

	public float animationEffectDelay = 0.6f;

	public AudioClip effectSound;

	public GameObject[] onShowActivate;

	public GameObject[] onHideDeactivate;

	public GameObject helpMessageText;

	public TouchButtonSquare[] innerButtons;

	private bool isTriggered;

	protected bool isUpdating;

	private Camera refCamera;

	private int waitReleaseButton = -1;

	private float prevTimer;

	private float aniEffectPrevTime;

	private bool isEnablePlayerControl;

	public TutorialMessage nextTutorialMsg;

	private float oldVerticalControlValue;

	private void Awake()
	{
		isTriggered = false;
		isUpdating = false;
	}

	private void Start()
	{
		refCamera = ScrollingCamera.instance.GetComponent<Camera>();
	}

	protected virtual void NextAnimFrame()
	{
		backFrame++;
		if (breakAction == eBreakTutorialType.verticalControl && (GameInput.inputStyle == eInputStyle.eisAcceleration || GameInput.inputStyle == eInputStyle.eisAcceleration2))
		{
			if (backFrame >= backPictAltList.Length)
			{
				backFrame = 0;
			}
			GUtils.LoadPictImage(refMessageRender, backPictAltList[backFrame], ref backPictTexture);
		}
		else
		{
			if (backFrame >= backPictList.Length)
			{
				backFrame = 0;
			}
			GUtils.LoadPictImage(refMessageRender, backPictList[backFrame], ref backPictTexture);
		}
	}

	public virtual void Update()
	{
		if (!isUpdating)
		{
			if (isTriggered && (refCamera.transform.position.x < base.transform.position.x - 500f || refCamera.transform.position.x > base.transform.position.x + 500f))
			{
				isTriggered = false;
			}
			return;
		}
		if (animationEffect != null && animationEffect.isPlaying)
		{
			AnimationState animationState = animationEffect[animationEffect.clip.name];
			animationState.time += Time.realtimeSinceStartup - aniEffectPrevTime;
			if (animationState.time >= animationState.length + animationEffectDelay)
			{
				animationState.time = 0f;
				animationEffect.Play();
			}
			aniEffectPrevTime = Time.realtimeSinceStartup;
		}
		if (backFrame >= 0 && (Time.realtimeSinceStartup - backFramePrevTime) * backFrameSpeed >= 1f)
		{
			backFramePrevTime = Time.realtimeSinceStartup;
			NextAnimFrame();
		}
		if (refHelpAnimation != null)
		{
			AnimationState animationState2 = refHelpAnimation[refHelpAnimation.clip.name];
			animationState2.time += Time.realtimeSinceStartup - prevTimer;
			prevTimer = Time.realtimeSinceStartup;
			if (animationState2.time >= animationState2.length + timeoutAnimation)
			{
				animationState2.time -= animationState2.length + timeoutAnimation;
				refHelpAnimation.Play();
			}
		}
		bool flag = KeyIsPressed();
		if (waitReleaseButton >= 0)
		{
			if (waitReleaseButton > 0 || !flag)
			{
				waitReleaseButton--;
			}
		}
		else
		{
			if (!flag)
			{
				return;
			}
			HideMessage();
			if (breakAction == eBreakTutorialType.powerUp)
			{
				PowerUpScreen powerUpScreen = (PowerUpScreen)GuiGameHud.instance.powerUpScreen;
				powerUpScreen.Enabled(val: true);
				powerUpScreen.timeout = -1f;
				GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.powerUp);
				TimeController.StopSlowMo(forced: true);
				Time.timeScale = 0f;
				isUpdating = true;
				breakAction = eBreakTutorialType.powerUpSliding;
				prevTimer = Time.realtimeSinceStartup;
				if ((bool)animationEffect)
				{
					animationEffect.Stop();
					animationEffect.gameObject.SetActiveRecursivelyCompat(state: false);
				}
				if (helpMessageText != null)
				{
					helpMessageText.SetActiveRecursivelyCompat(state: true);
				}
			}
			if (nextTutorialMsg != null && TutorialProcessor.instance != null)
			{
				TutorialProcessor.instance.SetNextTimeTutor(nextTutorialMsg, 2);
			}
		}
	}

	protected virtual bool KeyIsPressed()
	{
		bool flag = false;
		switch (breakAction)
		{
		case eBreakTutorialType.anyAction:
			flag = GameInput.GetButton(eGameInputButton.flyVertical) || GameInput.GetButton(eGameInputButton.growing) || GameInput.GetButton(eGameInputButton.skinBoar) || GameInput.GetButton(eGameInputButton.skinEagle);
			break;
		case eBreakTutorialType.boarForm:
			flag = GameInput.GetButton(eGameInputButton.skinBoar);
			break;
		case eBreakTutorialType.eagleForm:
			flag = GameInput.GetButton(eGameInputButton.skinEagle);
			break;
		case eBreakTutorialType.grass:
			flag = GameInput.GetButton(eGameInputButton.growing);
			break;
		case eBreakTutorialType.speedUp:
			flag = GameInput.GetButton(eGameInputButton.speedup) && GameInput.GetButton(eGameInputButton.skinBoar);
			break;
		case eBreakTutorialType.flying:
			flag = GameInput.GetButton(eGameInputButton.flyVertical) && GameInput.GetButton(eGameInputButton.skinEagle);
			break;
		case eBreakTutorialType.animalForm:
			flag = GameInput.GetButton(eGameInputButton.skinEagle) || GameInput.GetButton(eGameInputButton.skinBoar);
			break;
		case eBreakTutorialType.powerUp:
			flag = GameInput.GetButton(eGameInputButton.powerUp);
			break;
		case eBreakTutorialType.powerUpSliding:
			if (GuiGameHud.instance.GetGuiState() != GuiGameHud.eGuiGameHudState.powerUp)
			{
				isUpdating = false;
				if (helpMessageText != null)
				{
					helpMessageText.SetActiveRecursivelyCompat(state: false);
				}
			}
			break;
		case eBreakTutorialType.touchScreen:
			flag = Input.touchCount > 0 || Input.anyKey;
			break;
		case eBreakTutorialType.touchScreenUp:
			flag = Input.touchCount == 0 && !Input.anyKey;
			if (flag)
			{
				break;
			}
			if (innerButtons != null)
			{
				TouchButtonSquare[] array = innerButtons;
				foreach (TouchButtonSquare touchButtonSquare in array)
				{
					if (touchButtonSquare.IsTouched)
					{
						waitReleaseButton = 3;
						break;
					}
				}
			}
			if (waitReleaseButton > 0)
			{
				waitReleaseButton = 3;
			}
			break;
		case eBreakTutorialType.verticalControl:
		{
			float num = oldVerticalControlValue - GameInput.GetButtonValue(eGameInputButton.flyVertical);
			flag = num * num > 0.5f;
			break;
		}
		}
		return flag;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isTriggered)
		{
			isTriggered = true;
			ShowMessage();
		}
	}

	public virtual void HideMessage()
	{
		TimeController.StartPause(isPause: false);
		if (isEnablePlayerControl)
		{
			PlayerController.instance.DisableController(disable: false);
		}
		if (refMessageRender != null)
		{
			refMessageRender.gameObject.SetActiveRecursivelyCompat(state: false);
		}
		isUpdating = false;
		GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.game);
		GuiGameHud.instance.pauseScreen.Enabled(val: true);
		if ((bool)refHelpAnimation)
		{
			refHelpAnimation.Stop();
			refHelpAnimation.gameObject.SetActiveRecursivelyCompat(state: false);
		}
		if (onHideDeactivate != null)
		{
			for (int i = 0; i < onHideDeactivate.Length; i++)
			{
				onHideDeactivate[i].SetActiveRecursivelyCompat(state: false);
			}
		}
		if (helpMessageText != null)
		{
			helpMessageText.SetActiveRecursivelyCompat(state: false);
		}
		switch (breakAction)
		{
		case eBreakTutorialType.boarForm:
			GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: false);
			break;
		case eBreakTutorialType.eagleForm:
			GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: false);
			break;
		case eBreakTutorialType.powerUp:
			GameInput.instance.HideControl(eGameInputButton.powerUp, hideFlag: false);
			break;
		}
	}

	public virtual void ShowMessage()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: true);
		TimeController.StartPause(isPause: true);
		isEnablePlayerControl = PlayerController.instance.IsEnableControl();
		if (isEnablePlayerControl)
		{
			PlayerController.instance.DisableController(disable: true);
		}
		if (refMessageRender != null)
		{
			backFrame = -1;
			if (backPictList != null && backPictList.Length > 0)
			{
				GUtils.LoadPictImage(refMessageRender, backPictList[0], ref backPictTexture);
				backFrame = 0;
				backFramePrevTime = Time.realtimeSinceStartup;
			}
			else if (backPicture != null)
			{
				GUtils.LoadPictImage(refMessageRender, backPicture, ref backPictTexture);
			}
			refMessageRender.gameObject.SetActiveRecursivelyCompat(state: true);
		}
		isUpdating = true;
		waitReleaseButton = 2;
		GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.tutorial);
		if (LevelAttributes.instance.PlayerSkin == EPlayerSkin.Ghost && (breakAction == eBreakTutorialType.animalForm || breakAction == eBreakTutorialType.anyAction))
		{
			breakAction = eBreakTutorialType.touchScreenUp;
		}
		Vector3 vector = Vector3.zero;
		switch (breakAction)
		{
		case eBreakTutorialType.boarForm:
			GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: false);
			vector = GuiGameHud.instance.buttonBoar.transformMatrix.MultiplyPoint(Vector3.zero);
			break;
		case eBreakTutorialType.eagleForm:
			GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: false);
			vector = GuiGameHud.instance.buttonEagle.transformMatrix.MultiplyPoint(Vector3.zero);
			break;
		case eBreakTutorialType.grass:
			GameInput.instance.HideControl(eGameInputButton.growing, hideFlag: false);
			vector = GuiGameHud.instance.buttonGrass.transformMatrix.MultiplyPoint(Vector3.zero);
			break;
		case eBreakTutorialType.animalForm:
			GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: false);
			GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: false);
			break;
		case eBreakTutorialType.anyAction:
			GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: false);
			GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: false);
			break;
		case eBreakTutorialType.powerUp:
		{
			GameInput.instance.HideControl(eGameInputButton.powerUp, hideFlag: false);
			GuiGameHud.instance.powerUpScreen.Enabled(val: false);
			int num = GuiGameHud.ghostCost - GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.gem);
			if (num > 0)
			{
				GameStatistic.instance.AddItem(GameStatistic.eStatisticItem.gemSummary, num);
			}
			GuiGameHud.UpdateCrystalProgress();
			vector = GuiGameHud.instance.buttonPwrUpGhost.transformMatrix.MultiplyPoint(Vector3.zero);
			break;
		}
		case eBreakTutorialType.verticalControl:
			if (GameInput.inputStyle == eInputStyle.eisAcceleration || GameInput.inputStyle == eInputStyle.eisAcceleration2)
			{
				refHelpAnimation = null;
				backFrame = -1;
				if (refMessageRender != null)
				{
					if (backPictAltList != null && backPictAltList.Length > 0)
					{
						GUtils.LoadPictImage(refMessageRender, backPictAltList[0], ref backPictTexture);
						backFrame = 0;
						backFramePrevTime = Time.realtimeSinceStartup;
					}
					else if (backPictureAlt != null)
					{
						GUtils.LoadPictImage(refMessageRender, backPictureAlt, ref backPictTexture);
					}
				}
			}
			oldVerticalControlValue = GameInput.GetButtonValue(eGameInputButton.flyVertical);
			break;
		}
		GuiGameHud.instance.pauseScreen.Enabled(val: false);
		if ((bool)animationEffect)
		{
			animationEffect.gameObject.SetActiveRecursivelyCompat(state: true);
			animationEffect.transform.position = new Vector3(vector.x, vector.y, animationEffect.transform.position.z);
			animationEffect.Play();
			aniEffectPrevTime = Time.realtimeSinceStartup;
		}
		if (refHelpAnimation != null)
		{
			SetupHelpAnimPosition();
			refHelpAnimation.gameObject.SetActiveRecursivelyCompat(state: true);
			refHelpAnimation.Play();
			prevTimer = Time.realtimeSinceStartup;
		}
		if ((bool)effectSound)
		{
			SoundEnvironment.Launch2DSound(effectSound, 1f, 0f, -1f);
		}
		if (onShowActivate == null)
		{
			return;
		}
		bool flag = false;
		if (innerButtons != null && innerButtons.Length > 0 && ChooseLevelGUI.IsFreeVersion())
		{
			flag = true;
		}
		if (!flag)
		{
			GameObject[] array = onShowActivate;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActiveRecursivelyCompat(state: true);
			}
		}
	}

	private void SetupHelpAnimPosition()
	{
	}

	public void SkipTutorAction()
	{
		waitReleaseButton = 2;
	}
}
