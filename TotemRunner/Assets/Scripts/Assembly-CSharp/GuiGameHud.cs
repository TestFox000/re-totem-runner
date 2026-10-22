using System;
using UnityEngine;

public class GuiGameHud : MonoBehaviour
{
	[Serializable]
	public class AccelerationParameters
	{
		[NonSerialized]
		public float baseAc;

		public float maxDelta = 0.3f;

		public float minDelta = 0.05f;

		public float maxDelta2 = 0.3f;

		public float minDelta2 = 0.1f;

		[NonSerialized]
		public float mul = 1f / 3f;

		[NonSerialized]
		public float prevSign = 1f;
	}

	public enum eButtonID
	{
		none = 0,
		grass = 1,
		speedup = 2,
		eagle = 3,
		boar = 4,
		pause = 5,
		vertical = 6,
		up = 7,
		down = 8,
		powerUpGhost = 9
	}

	public enum eMessageType
	{
		common = 0,
		good = 1,
		evil = 2
	}

	public enum eButtonState
	{
		hidden = 0,
		inactive = 1,
		activated = 2,
		disabled = 3
	}

	[Serializable]
	public class ButtonParams
	{
		public Vector2 pos;

		public float buttonSize = 128f;

		public float touchZoneScale = 1f;

		public Texture2D imageOff;

		public Texture2D imageOn;

		public Texture2D imageDisable;

		public Material imageMaterial;

		private eButtonState _state;

		[NonSerialized]
		public Matrix4x4 transformMatrix;

		[NonSerialized]
		public Vector2 vMinBound;

		[NonSerialized]
		public Vector2 vMaxBound;

		public eButtonState State()
		{
			return _state;
		}

		public void SetState(eButtonState st)
		{
			_state = st;
		}

		public void CalculateTransformMatrix(Camera cam)
		{
			Vector3 vector = new Vector3(Mathf.Floor(cam.pixelWidth * 0.01f * pos.x), Mathf.Floor(cam.pixelHeight * 0.01f * pos.y), guiZPlane);
			Vector3 vector2 = cam.ScreenToWorldPoint(vector);
			transformMatrix.SetTRS(vector2, Quaternion.identity, new Vector3(buttonSize, buttonSize, 1f));
			Vector2 vector3 = buttonSize * touchZoneScale * 0.5f * (cam.pixelHeight / 768f) * Vector2.one;
			vector3.x = Mathf.Floor(vector3.x);
			vector3.y = Mathf.Floor(vector3.y);
			vMinBound = (Vector2)vector - vector3;
			vMaxBound = (Vector2)vector + vector3;
		}

		public bool IsPointIntoSquare(Vector2 touchPos)
		{
			if (touchPos.x < vMinBound.x || touchPos.x > vMaxBound.x || touchPos.y < vMinBound.y || touchPos.y > vMaxBound.y)
			{
				return false;
			}
			return true;
		}
	}

	[Serializable]
	public class JoystickParams
	{
		[NonSerialized]
		public eButtonState state;

		[NonSerialized]
		public float stickPower;

		[NonSerialized]
		public float stickPowerBase;

		public float minDelta = 20f;
	}

	[Serializable]
	public class MessageString
	{
		public enum eMessageStringState
		{
			disable = 0,
			fadein = 1,
			visible = 2,
			fadeout = 3,
			changing = 4
		}

		public TextMesh textMesh;

		public float fadeinTime = 0.2f;

		public float fadeoutTime = 0.7f;

		public float changingTime = 0.5f;

		[NonSerialized]
		public float showingTime = 5f;

		[NonSerialized]
		public float timefollow = -1f;

		public Vector2 changingScale = 0.5f * Vector2.one;

		[NonSerialized]
		public eMessageStringState state;

		public Color colorCommon = new Color(1f, 0.875f, 0.215f, 1f);

		public Color colorGood = Color.green;

		public Color colorEvil = Color.red;
	}

	[Serializable]
	public class ZoneActivity
	{
		public bool actively;

		public Vector2 deltaPos;

		public Vector2 pos;

		public Vector2 basePos;
	}

	private enum eFormChoosed
	{
		none = 0,
		eagle = 1,
		boar = 2
	}

	private enum eControlForm
	{
		disable = 0,
		boarAndEagle = 1,
		EagleAndFly = 2,
		onlyFly = 3
	}

	public enum eGuiGameHudState
	{
		game = 0,
		pause = 1,
		levelComplete = 2,
		powerUp = 3,
		qte = 4,
		tutorial = 5,
		disable = 6
	}

	private class StoredButtonState
	{
		public bool isHiddenBoar;

		public bool isHiddenEagle;

		public bool isHiddenGhost;
	}

	public AccelerationParameters accelParam;

	public ProgressBar levelProgress;

	public CounterUI scoreCounter;

	public ProgressOnButton ghostProgress;

	public static int ghostCost = 50;

	public InnerRenderingObject[] preRenderedList;

	public InnerRenderingObject[] postRenderedList;

	public GUISkin guiSkin;

	public TextMesh grassTextMesh;

	public MeshRenderer berryPicture;

	public Material berryBlinkMaterial;

	private bool isExtraPointsMode;

	private float berryBlinkTime;

	private float berryBlinkTimeMax;

	public float berryBlinkTimeout = 0.5f;

	public Color berryColor = Color.magenta;

	public Color berryBlinkColor = Color.white;

	private bool grassDontShow;

	public MessageString messageString;

	public float mulViewportToStickPower = 0.2f;

	public ButtonParams buttonGrass;

	public ButtonParams buttonSpeedup;

	public ButtonParams buttonEagle;

	public ButtonParams buttonBoar;

	public ButtonParams buttonPause;

	public ButtonParams buttonUp;

	public ButtonParams buttonDown;

	public JoystickParams joystickVertical;

	public ButtonParams buttonPwrUpGhost;

	private ZoneActivity zoneAction = new ZoneActivity();

	private ZoneActivity zoneBird = new ZoneActivity();

	private ZoneActivity zoneBoar = new ZoneActivity();

	private ZoneActivity zoneFlyUp = new ZoneActivity();

	private ZoneActivity zoneFlyDown = new ZoneActivity();

	private eFormChoosed formChoosed;

	private eControlForm curControlForm;

	private bool prevFlyForm;

	public static float guiZPlane = 100f;

	private float guiScreenWidth = 1024f;

	private GUIText unlockContentGUI;

	private float unlockContentVisibleTime;

	private eGuiGameHudState menuState;

	private bool isDisableGUI;

	private float guiTimeFollow;

	public float minFlyHeight = -300f;

	public static float mulStickPow = 1.5f;

	public GuiScreen levelCompleteScreen;

	public GuiScreen pauseScreen;

	public GuiScreen powerUpScreen;

	public GuiScreen qteScreen;

	public TutorialMessage deathMsgHelp;

	public BaseProgress berryProgress;

	private StoredButtonState storedButton = new StoredButtonState();

	public static eGuiGameHudState oldDisableState = eGuiGameHudState.disable;

	private static GuiGameHud _singleton;

	public static GuiGameHud instance
	{
		get
		{
			if (!_singleton)
			{
				_singleton = UnityEngine.Object.FindObjectOfType(typeof(GuiGameHud)) as GuiGameHud;
			}
			return _singleton;
		}
	}

	public void DisableGrassGUI(bool isDisable)
	{
		grassDontShow = isDisable;
		if (isDisable)
		{
			grassTextMesh.gameObject.SetActive(false);
			berryPicture.gameObject.SetActive(false);
		}
	}

	public static void DisableGUI(bool isDisable)
	{
		if (_singleton == null)
		{
			_singleton = instance;
		}
		if (_singleton != null)
		{
			if (isDisable && _singleton.menuState != eGuiGameHudState.disable)
			{
				oldDisableState = _singleton.menuState;
				_singleton.SetGuiState(eGuiGameHudState.disable);
			}
			if (!isDisable)
			{
				_singleton.SetGuiState(oldDisableState);
			}
		}
	}

	private void Awake()
	{
		_singleton = this;
		isExtraPointsMode = false;
		grassTextMesh.gameObject.SetActive(false);
		berryPicture.gameObject.SetActive(false);
		SetGrassCounters(0, 0);
		unlockContentGUI = base.transform.Find("HUDUnlockContent").gameObject.GetComponent<GUIText>();
		unlockContentGUI.gameObject.SetActiveRecursivelyCompat(state: false);
		unlockContentVisibleTime = -1f;
		buttonPause.SetState(eButtonState.inactive);
		ChangeButtonState(eButtonID.powerUpGhost, eButtonState.inactive);
		buttonEagle.SetState(eButtonState.inactive);
		buttonBoar.SetState(eButtonState.inactive);
		buttonUp.SetState(eButtonState.inactive);
		buttonDown.SetState(eButtonState.inactive);
		Camera camera = base.GetComponent<Camera>();
		guiScreenWidth = camera.pixelWidth;
		buttonGrass.CalculateTransformMatrix(camera);
		buttonSpeedup.CalculateTransformMatrix(camera);
		buttonEagle.CalculateTransformMatrix(camera);
		buttonBoar.CalculateTransformMatrix(camera);
		buttonPause.CalculateTransformMatrix(camera);
		buttonUp.CalculateTransformMatrix(camera);
		buttonDown.CalculateTransformMatrix(camera);
		buttonPwrUpGhost.CalculateTransformMatrix(camera);
		if ((bool)ghostProgress)
		{
			ghostProgress.transform.position = buttonPwrUpGhost.transformMatrix.MultiplyPoint(Vector3.zero);
		}
		accelParam.maxDelta = 0.45f;
		accelParam.minDelta = 0.085f;
		accelParam.mul = 1f / accelParam.maxDelta;
	}

	public void UpdateAfterCamera()
	{
		switch (menuState)
		{
		case eGuiGameHudState.game:
			UpdateInGameHud();
			if (berryBlinkTime > 0f)
			{
				berryBlinkTime = Mathf.Clamp(berryBlinkTime - Time.deltaTime, 0f, berryBlinkTime);
				float t = berryBlinkTime / berryBlinkTimeMax;
				berryPicture.material.color = Color.Lerp(berryColor, berryBlinkColor, t);
				grassTextMesh.GetComponent<Renderer>().material.color = Color.Lerp(Color.magenta, Color.white, t);
			}
			if ((bool)messageString.textMesh && messageString.state != MessageString.eMessageStringState.disable)
			{
				UpdateMessageString();
			}
			break;
		case eGuiGameHudState.tutorial:
			UpdateInGameHud();
			break;
		}
	}

	private void Update()
	{
		if ((bool)levelProgress)
		{
			float num = LevelAttributes.instance.bounds.xMax;
			if (num < 1f)
			{
				num = 1f;
			}
			levelProgress.SetProgress(Mathf.Clamp01(PlayerController.instance.transform.position.x / num));
		}
		UpdateAfterCamera();
	}

	private void OnPostRender()
	{
		if (isDisableGUI)
		{
			return;
		}
		switch (menuState)
		{
		case eGuiGameHudState.game:
		case eGuiGameHudState.tutorial:
		{
			InnerRenderingObject[] array = preRenderedList;
			foreach (InnerRenderingObject innerRenderingObject in array)
			{
				innerRenderingObject.DoRender();
			}
			if (buttonPause.State() != eButtonState.hidden)
			{
				DrawInputButtons(buttonPause);
			}
			if (buttonPwrUpGhost.State() != eButtonState.hidden)
			{
				DrawInputButtons(buttonPwrUpGhost);
			}
			if ((curControlForm == eControlForm.boarAndEagle || curControlForm == eControlForm.EagleAndFly) && buttonEagle.State() != eButtonState.hidden)
			{
				DrawInputButtons(buttonEagle);
			}
			if (curControlForm == eControlForm.boarAndEagle && buttonBoar.State() != eButtonState.hidden)
			{
				DrawInputButtons(buttonBoar);
			}
			if (menuState == eGuiGameHudState.game && ((curControlForm == eControlForm.EagleAndFly || curControlForm == eControlForm.onlyFly) ? true : false))
			{
				if (buttonUp.State() != eButtonState.hidden)
				{
					DrawInputButtons(buttonUp);
				}
				if (buttonDown.State() != eButtonState.hidden)
				{
					DrawInputButtons(buttonDown);
				}
			}
			InnerRenderingObject[] array2 = postRenderedList;
			foreach (InnerRenderingObject innerRenderingObject2 in array2)
			{
				innerRenderingObject2.DoRender();
			}
			break;
		}
		case eGuiGameHudState.powerUp:
			if (buttonPwrUpGhost.State() != eButtonState.hidden)
			{
				DrawInputButtons(buttonPwrUpGhost);
			}
			break;
		case eGuiGameHudState.pause:
		case eGuiGameHudState.levelComplete:
		case eGuiGameHudState.qte:
			break;
		}
	}

	public static void ShowMessageString(string text, float timeout, bool isAddition, eMessageType messageType)
	{
		MessageString messageString = _singleton.messageString;
		switch (messageType)
		{
		case eMessageType.common:
			messageString.textMesh.GetComponent<Renderer>().material.color = messageString.colorCommon;
			break;
		case eMessageType.good:
			messageString.textMesh.GetComponent<Renderer>().material.color = messageString.colorGood;
			break;
		case eMessageType.evil:
			messageString.textMesh.GetComponent<Renderer>().material.color = messageString.colorEvil;
			break;
		}
		if (isAddition)
		{
			messageString.state = MessageString.eMessageStringState.changing;
			GUtils.SetLocalScale2D(messageString.textMesh.transform, 1f + messageString.changingScale.x, 1f + messageString.changingScale.y);
			GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), 1f);
		}
		else
		{
			messageString.state = MessageString.eMessageStringState.fadein;
			GUtils.SetLocalScale2D(messageString.textMesh.transform, 1f, 1f);
			GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), 0f);
		}
		messageString.textMesh.gameObject.SetActive(true);
		messageString.textMesh.text = text;
		messageString.showingTime = timeout;
		messageString.timefollow = 0f;
	}

	private void UpdateMessageString()
	{
		messageString.timefollow += Time.deltaTime;
		switch (messageString.state)
		{
		case MessageString.eMessageStringState.fadein:
			if (messageString.timefollow < messageString.fadeinTime)
			{
				GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), Mathf.Clamp01(messageString.timefollow / messageString.fadeinTime));
				break;
			}
			messageString.state = MessageString.eMessageStringState.visible;
			messageString.timefollow -= messageString.fadeinTime;
			GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), 1f);
			break;
		case MessageString.eMessageStringState.visible:
			if (messageString.timefollow >= messageString.showingTime)
			{
				if (messageString.fadeoutTime > 0f)
				{
					messageString.state = MessageString.eMessageStringState.fadeout;
					messageString.timefollow = 0f;
					GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), Mathf.Clamp01(1f - messageString.timefollow / messageString.fadeoutTime));
				}
				else
				{
					messageString.state = MessageString.eMessageStringState.disable;
					messageString.textMesh.gameObject.SetActive(false);
					GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), 1f);
				}
			}
			break;
		case MessageString.eMessageStringState.fadeout:
			if (messageString.timefollow < messageString.fadeoutTime)
			{
				GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), Mathf.Clamp01(1f - messageString.timefollow / messageString.fadeinTime));
				break;
			}
			messageString.state = MessageString.eMessageStringState.disable;
			messageString.textMesh.gameObject.SetActive(false);
			GUtils.SetRendererAlpha(messageString.textMesh.GetComponent<Renderer>(), 1f);
			break;
		case MessageString.eMessageStringState.changing:
			if (messageString.timefollow < messageString.changingTime)
			{
				float num = Mathf.Clamp01(1f - messageString.timefollow / messageString.changingTime);
				GUtils.SetLocalScale2D(messageString.textMesh.transform, 1f + messageString.changingScale.x * num, 1f + messageString.changingScale.y * num);
			}
			else
			{
				messageString.state = MessageString.eMessageStringState.visible;
				messageString.timefollow = 0f;
				GUtils.SetLocalScale2D(messageString.textMesh.transform, 1f, 1f);
			}
			break;
		}
	}

	private void OnGUI()
	{
		if (isDisableGUI)
		{
			return;
		}
		switch (menuState)
		{
		case eGuiGameHudState.pause:
			if ((bool)pauseScreen)
			{
				pauseScreen.UpdateInGUI();
			}
			break;
		case eGuiGameHudState.levelComplete:
			if ((bool)levelCompleteScreen)
			{
				levelCompleteScreen.UpdateInGUI();
			}
			break;
		case eGuiGameHudState.powerUp:
			if ((bool)powerUpScreen)
			{
				powerUpScreen.UpdateInGUI();
			}
			break;
		case eGuiGameHudState.qte:
			if ((bool)qteScreen)
			{
				qteScreen.UpdateInGUI();
			}
			break;
		}
	}

	private float smoothedAngle;

	private bool hasSmoothedAngle;

	private float GetDeviceAngle()
	{
		float num = Mathf.Atan2(Input.acceleration.z, Input.acceleration.x);
		if (!hasSmoothedAngle)
		{
			smoothedAngle = num;
			hasSmoothedAngle = true;
			return smoothedAngle;
		}
		float num2 = Mathf.DeltaAngle(num * 57.29578f, smoothedAngle * 57.29578f) * 0.017453292f;
		float num3 = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);
		float num4 = 1f - Mathf.Exp((0f - num3) / 0.12f);
		smoothedAngle += num2 * num4;
		return smoothedAngle;
	}

	private eControlForm GetControlForm()
	{
		eControlForm result = eControlForm.disable;
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Man:
			result = eControlForm.boarAndEagle;
			break;
		case EPlayerSkin.Eagle:
			result = ((GameInput.inputStyle == eInputStyle.eisAcceleration || GameInput.inputStyle == eInputStyle.eisAcceleration2) ? eControlForm.boarAndEagle : eControlForm.EagleAndFly);
			break;
		case EPlayerSkin.Star:
			result = ((GameInput.inputStyle != eInputStyle.eisAcceleration && GameInput.inputStyle != eInputStyle.eisAcceleration2) ? eControlForm.onlyFly : eControlForm.disable);
			break;
		case EPlayerSkin.Ghost:
			result = eControlForm.disable;
			break;
		case EPlayerSkin.Boar:
			result = eControlForm.boarAndEagle;
			break;
		}
		return result;
	}

	private void UpdateInGameHud()
	{
		if (unlockContentVisibleTime >= 0f)
		{
			unlockContentVisibleTime -= Time.deltaTime;
			if (unlockContentVisibleTime <= 0f)
			{
				unlockContentVisibleTime = -1f;
				unlockContentGUI.gameObject.SetActiveRecursivelyCompat(state: false);
			}
		}
		eControlForm controlForm = GetControlForm();
		if (controlForm != curControlForm)
		{
			curControlForm = controlForm;
		}
		bool isFlyForm = LevelAttributes.instance.IsFlyForm;
		if (prevFlyForm != isFlyForm)
		{
			prevFlyForm = isFlyForm;
			if (isFlyForm)
			{
				if (GameInput.inputStyle == eInputStyle.eisAcceleration)
				{
					float maxFlyHeight = PlayerController.instance.eagleBehaviour.maxFlyHeight;
					joystickVertical.stickPower = Mathf.Clamp((PlayerController.instance.transform.position.y - minFlyHeight) / (maxFlyHeight - minFlyHeight) * 2f - 1f, -1f, 1f);
					joystickVertical.stickPowerBase = joystickVertical.stickPower;
					accelParam.baseAc = GetDeviceAngle();
				}
				else if (GameInput.inputStyle == eInputStyle.eisAcceleration2)
				{
					joystickVertical.stickPower = 0f;
					joystickVertical.stickPowerBase = 0f;
					accelParam.baseAc = GetDeviceAngle();
				}
				else
				{
					joystickVertical.stickPower = 0f;
					joystickVertical.stickPowerBase = 0f;
				}
			}
		}
		UpdateInputStates();
		if (!isDisableGUI)
		{
			if (buttonPwrUpGhost.State() == eButtonState.activated && !PlayerController.IsFirstSpawn() && LevelAttributes.instance.PlayerSkin != EPlayerSkin.Ghost && true)
			{
				SetGuiState(eGuiGameHudState.powerUp);
			}
			if (buttonPause.State() == eButtonState.activated || GameInput.IsBackPressed() || GameInput.IsMenuPressed())
			{
				SetGuiState(eGuiGameHudState.pause);
				ChangeButtonState(eButtonID.pause, eButtonState.inactive);
			}
		}
	}

	public void SetGrassCounters(int curGrass, int possibleGrass)
	{
		if (curGrass >= 300)
		{
			if (!isExtraPointsMode)
			{
				isExtraPointsMode = true;
				grassTextMesh.gameObject.SetActive(true);
				berryPicture.gameObject.SetActive(true);
			}
			grassTextMesh.text = "Extra: " + (curGrass - 300);
			BerryBlink(berryBlinkTimeout);
		}
		else
		{
			if (isExtraPointsMode)
			{
				isExtraPointsMode = false;
				grassTextMesh.gameObject.SetActive(false);
				berryPicture.gameObject.SetActive(false);
			}
			grassTextMesh.text = string.Empty + (300 - curGrass);
		}
		if (berryProgress != null)
		{
			berryProgress.SetProgress(Mathf.Clamp01((float)curGrass / 300f));
		}
	}

	private void BerryBlink(float blinkTime)
	{
		if (blinkTime > berryBlinkTime)
		{
			berryBlinkTime = blinkTime;
			berryBlinkTimeMax = blinkTime;
			berryBlinkMaterial.color = berryBlinkColor;
		}
	}

	private void DrawInputButtons(ButtonParams btn)
	{
		if ((bool)btn.imageMaterial)
		{
			btn.imageMaterial.SetPass(0);
			Graphics.DrawMeshNow(LevelAttributes.spriteMesh, btn.transformMatrix);
		}
	}

	private eButtonID FindButtonByPos(Vector2 touchPos)
	{
		if (buttonPause.State() != eButtonState.hidden && buttonPause.IsPointIntoSquare(touchPos))
		{
			return eButtonID.pause;
		}
		if (buttonPwrUpGhost.State() != eButtonState.hidden && buttonPwrUpGhost.IsPointIntoSquare(touchPos))
		{
			return eButtonID.powerUpGhost;
		}
		if ((curControlForm == eControlForm.EagleAndFly || curControlForm == eControlForm.onlyFly) && buttonUp.State() != eButtonState.hidden)
		{
			if (buttonUp.IsPointIntoSquare(touchPos))
			{
				return eButtonID.up;
			}
			if (buttonDown.IsPointIntoSquare(touchPos))
			{
				return eButtonID.down;
			}
		}
		if (curControlForm == eControlForm.boarAndEagle && touchPos.x < 0.5f * guiScreenWidth && buttonBoar.State() != eButtonState.hidden)
		{
			return eButtonID.boar;
		}
		if ((curControlForm == eControlForm.boarAndEagle || curControlForm == eControlForm.EagleAndFly) && buttonEagle.State() != eButtonState.hidden && touchPos.x > 0.5f * guiScreenWidth)
		{
			return eButtonID.eagle;
		}
		return eButtonID.none;
	}

	public ButtonParams GetButtonParams(eButtonID btnId)
	{
		ButtonParams result = null;
		switch (btnId)
		{
		case eButtonID.eagle:
			result = buttonEagle;
			break;
		case eButtonID.boar:
			result = buttonBoar;
			break;
		case eButtonID.grass:
			result = buttonGrass;
			break;
		case eButtonID.speedup:
			result = buttonSpeedup;
			break;
		case eButtonID.pause:
			result = buttonPause;
			break;
		case eButtonID.up:
			result = buttonUp;
			break;
		case eButtonID.down:
			result = buttonDown;
			break;
		case eButtonID.powerUpGhost:
			result = buttonPwrUpGhost;
			break;
		}
		return result;
	}

	private void ChangeButtonState(eButtonID btnId, eButtonState state)
	{
		ButtonParams buttonParams = GetButtonParams(btnId);
		if (buttonParams == null)
		{
			return;
		}
		if ((bool)buttonParams.imageMaterial)
		{
			Texture2D mainTexture = buttonParams.imageOff;
			switch (state)
			{
			case eButtonState.activated:
				if ((bool)buttonParams.imageOn)
				{
					mainTexture = buttonParams.imageOn;
				}
				break;
			case eButtonState.disabled:
				if ((bool)buttonParams.imageDisable)
				{
					mainTexture = buttonParams.imageDisable;
				}
				break;
			}
			buttonParams.imageMaterial.mainTexture = mainTexture;
		}
		buttonParams.SetState(state);
	}


	private int[] latchedFinger = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

	private int[] latchedButton = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

	private int LatchSlot(int fingerId)
	{
		int num = -1;
		for (int i = 0; i < latchedFinger.Length; i++)
		{
			if (latchedFinger[i] == fingerId)
			{
				return i;
			}
			if (latchedFinger[i] < 0 && num < 0)
			{
				num = i;
			}
		}
		if (num < 0)
		{
			num = 0;
		}
		latchedFinger[num] = fingerId;
		latchedButton[num] = -1;
		return num;
	}

	private bool IsPointIntoButtonExpanded(ButtonParams btn, Vector2 touchPos, float scale)
	{
		Vector2 vector = (btn.vMinBound + btn.vMaxBound) * 0.5f;
		Vector2 vector2 = (btn.vMaxBound - btn.vMinBound) * 0.5f * scale;
		return Mathf.Abs(touchPos.x - vector.x) <= vector2.x && Mathf.Abs(touchPos.y - vector.y) <= vector2.y;
	}

	private eButtonID FindButtonByPosExpanded(Vector2 touchPos)
	{
		if (buttonPause.State() != eButtonState.hidden && IsPointIntoButtonExpanded(buttonPause, touchPos, 1.25f))
		{
			return eButtonID.pause;
		}
		if (buttonPwrUpGhost.State() != eButtonState.hidden && IsPointIntoButtonExpanded(buttonPwrUpGhost, touchPos, 1.25f))
		{
			return eButtonID.powerUpGhost;
		}
		if ((curControlForm == eControlForm.EagleAndFly || curControlForm == eControlForm.onlyFly) && buttonUp.State() != eButtonState.hidden)
		{
			if (IsPointIntoButtonExpanded(buttonUp, touchPos, 1.6f))
			{
				return eButtonID.up;
			}
			if (IsPointIntoButtonExpanded(buttonDown, touchPos, 1.6f))
			{
				return eButtonID.down;
			}
		}
		if (curControlForm == eControlForm.boarAndEagle && touchPos.x < 0.5f * guiScreenWidth && buttonBoar.State() != eButtonState.hidden)
		{
			return eButtonID.boar;
		}
		if ((curControlForm == eControlForm.boarAndEagle || curControlForm == eControlForm.EagleAndFly) && buttonEagle.State() != eButtonState.hidden && touchPos.x > 0.5f * guiScreenWidth)
		{
			return eButtonID.eagle;
		}
		return eButtonID.none;
	}

	private void UpdateZoneByTouch(Vector2 touchPos, Vector2 touchDelta)
	{
		UpdateZoneByButton(FindButtonByPosExpanded(touchPos), touchPos, touchDelta);
	}

	private void UpdateZoneByButton(eButtonID btnId, Vector2 touchPos, Vector2 touchDelta)
	{
		switch (btnId)
		{
		case eButtonID.pause:
			ChangeButtonState(eButtonID.pause, eButtonState.activated);
			break;
		case eButtonID.powerUpGhost:
			if (buttonPwrUpGhost.State() == eButtonState.inactive)
			{
				ChangeButtonState(eButtonID.powerUpGhost, eButtonState.activated);
			}
			break;
		case eButtonID.grass:
		case eButtonID.speedup:
		case eButtonID.vertical:
			zoneAction.actively = true;
			zoneAction.deltaPos = touchDelta;
			zoneAction.pos = touchPos;
			break;
		case eButtonID.eagle:
			zoneBird.actively = true;
			break;
		case eButtonID.boar:
			zoneBoar.actively = true;
			break;
		case eButtonID.up:
			zoneFlyUp.actively = true;
			break;
		case eButtonID.down:
			zoneFlyDown.actively = true;
			break;
		}
	}
	private void UpdateInputStates()
	{
		bool actively = zoneAction.actively;
		bool actively2 = zoneBird.actively;
		bool actively3 = zoneBoar.actively;
		zoneAction.actively = false;
		zoneBird.actively = false;
		zoneBoar.actively = false;
		zoneFlyUp.actively = false;
		zoneFlyDown.actively = false;
		if (buttonPwrUpGhost.State() == eButtonState.activated)
		{
			ChangeButtonState(eButtonID.powerUpGhost, eButtonState.inactive);
		}
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			int slot = LatchSlot(touch.fingerId);
			switch (touch.phase)
			{
			case TouchPhase.Began:
				latchedButton[slot] = (int)FindButtonByPosExpanded(touch.position);
				if (latchedButton[slot] > 0)
				{
					UpdateZoneByButton((eButtonID)latchedButton[slot], touch.position, touch.deltaPosition);
				}
				break;
			case TouchPhase.Moved:
			case TouchPhase.Stationary:
				if (latchedButton[slot] <= 0)
				{
					latchedButton[slot] = (int)FindButtonByPosExpanded(touch.position);
				}
				if (latchedButton[slot] > 0)
				{
					UpdateZoneByButton((eButtonID)latchedButton[slot], touch.position, touch.deltaPosition);
				}
				break;
			case TouchPhase.Ended:
			case TouchPhase.Canceled:
				latchedFinger[slot] = -1;
				latchedButton[slot] = -1;
				break;
			}
		}
		float realDelta = TimeController.instance.RealDelta;
		if (buttonUp.State() != eButtonState.hidden)
		{
			switch (GameInput.inputStyle)
			{
			case eInputStyle.eisAcceleration:
			{
				float deviceAngle = GetDeviceAngle();
				float num = deviceAngle - accelParam.baseAc;
				if (num > (float)Math.PI)
				{
					num -= (float)Math.PI * 2f;
				}
				else if (num < -(float)Math.PI)
				{
					num += (float)Math.PI * 2f;
				}
				float num2 = Mathf.Max(0.05f, accelParam.maxDelta - accelParam.minDelta);
				float num3;
				if (num > accelParam.minDelta)
				{
					num3 = Mathf.Clamp01((num - accelParam.minDelta) / num2);
				}
				else if (num < 0f - accelParam.minDelta)
				{
					num3 = 0f - Mathf.Clamp01((0f - num - accelParam.minDelta) / num2);
				}
				else
				{
					num3 = 0f;
				}
				joystickVertical.stickPower = Mathf.MoveTowards(joystickVertical.stickPower, num3, realDelta * 6f);
				joystickVertical.stickPowerBase = joystickVertical.stickPower;
				break;
			}
			case eInputStyle.eisAcceleration2:
			{
				float deviceAngle = GetDeviceAngle();
				float num = deviceAngle - accelParam.baseAc;
				if (num > (float)Math.PI)
				{
					num -= (float)Math.PI * 2f;
				}
				else if (num < -(float)Math.PI)
				{
					num += (float)Math.PI * 2f;
				}
				if (joystickVertical.stickPower > 0f)
				{
					if (num > accelParam.maxDelta2)
					{
						accelParam.baseAc = deviceAngle - accelParam.maxDelta2;
						num = accelParam.maxDelta2;
					}
					if (num < accelParam.minDelta2)
					{
						joystickVertical.stickPower = 0f;
					}
					else
					{
						joystickVertical.stickPower = Mathf.Clamp01((num - accelParam.minDelta2) / (accelParam.maxDelta2 - accelParam.minDelta2));
					}
				}
				else if (joystickVertical.stickPower < -0f)
				{
					if (num < 0f - accelParam.maxDelta2)
					{
						accelParam.baseAc = deviceAngle + accelParam.maxDelta2;
						num = 0f - accelParam.maxDelta2;
					}
					if (num > 0f - accelParam.minDelta2)
					{
						joystickVertical.stickPower = 0f;
					}
					else
					{
						joystickVertical.stickPower = 0f - Mathf.Clamp01((accelParam.minDelta2 + num) / (accelParam.minDelta2 - accelParam.maxDelta2));
					}
				}
				else if (num > accelParam.minDelta2)
				{
					joystickVertical.stickPower = Mathf.Clamp01((num - accelParam.minDelta2) / (accelParam.maxDelta2 - accelParam.minDelta2));
				}
				else if (num < 0f - accelParam.minDelta2)
				{
					joystickVertical.stickPower = 0f - Mathf.Clamp01((num + accelParam.minDelta2) / (accelParam.minDelta2 - accelParam.maxDelta2));
				}
				else
				{
					joystickVertical.stickPower = 0f;
				}
				break;
			}
			case eInputStyle.eisJoystick:
			{
				if (!actively)
				{
					zoneAction.basePos = zoneAction.pos;
					joystickVertical.stickPower = 0f;
					joystickVertical.stickPowerBase = 0f;
					break;
				}
				float num3 = zoneAction.pos.y - zoneAction.basePos.y;
				if (num3 > joystickVertical.minDelta)
				{
					joystickVertical.stickPowerBase = 1f;
					if (joystickVertical.stickPower < 0f)
					{
						joystickVertical.stickPower = 0f;
					}
					zoneAction.basePos = zoneAction.pos;
				}
				else if (num3 < 0f - joystickVertical.minDelta)
				{
					joystickVertical.stickPowerBase = -1f;
					if (joystickVertical.stickPower > 0f)
					{
						joystickVertical.stickPower = 0f;
					}
					zoneAction.basePos = zoneAction.pos;
				}
				if (joystickVertical.stickPower != joystickVertical.stickPowerBase)
				{
					joystickVertical.stickPower = Mathf.Lerp(joystickVertical.stickPower, joystickVertical.stickPowerBase, realDelta * 12f);
				}
				break;
			}
			case eInputStyle.eisButton:
				if (zoneFlyUp.actively)
				{
					joystickVertical.stickPower = Mathf.Lerp(joystickVertical.stickPower, 1f, realDelta * 4f);
				}
				else if (zoneFlyDown.actively)
				{
					joystickVertical.stickPower = Mathf.Lerp(joystickVertical.stickPower, -1f, realDelta * 4f);
				}
				else
				{
					joystickVertical.stickPower = 0f;
				}
				break;
			}
			if (joystickVertical.stickPower > 0.05f)
			{
				if (buttonUp.State() == eButtonState.inactive)
				{
					ChangeButtonState(eButtonID.up, eButtonState.activated);
				}
				if (buttonDown.State() == eButtonState.activated)
				{
					ChangeButtonState(eButtonID.down, eButtonState.inactive);
				}
			}
			else if (joystickVertical.stickPower < -0.05f)
			{
				if (buttonUp.State() == eButtonState.activated)
				{
					ChangeButtonState(eButtonID.up, eButtonState.inactive);
				}
				if (buttonDown.State() == eButtonState.inactive)
				{
					ChangeButtonState(eButtonID.down, eButtonState.activated);
				}
			}
			else
			{
				if (buttonUp.State() == eButtonState.activated)
				{
					ChangeButtonState(eButtonID.up, eButtonState.inactive);
				}
				if (buttonDown.State() == eButtonState.activated)
				{
					ChangeButtonState(eButtonID.down, eButtonState.inactive);
				}
			}
		}
		if (zoneBird.actively && zoneBoar.actively)
		{
			if (!actively2 || GameInput.inputStyle == eInputStyle.eisButton)
			{
				SetFormChoosed(eFormChoosed.eagle);
			}
			else if (!actively3)
			{
				SetFormChoosed(eFormChoosed.boar);
			}
		}
		else if (zoneBird.actively)
		{
			SetFormChoosed(eFormChoosed.eagle);
		}
		else if (zoneBoar.actively)
		{
			SetFormChoosed(eFormChoosed.boar);
		}
		else
		{
			SetFormChoosed(eFormChoosed.none);
		}
		if (formChoosed != eFormChoosed.eagle && buttonEagle.State() == eButtonState.activated)
		{
			ChangeButtonState(eButtonID.eagle, eButtonState.inactive);
		}
		if (formChoosed == eFormChoosed.eagle && buttonEagle.State() == eButtonState.inactive)
		{
			ChangeButtonState(eButtonID.eagle, eButtonState.activated);
		}
		if (formChoosed != eFormChoosed.boar && buttonBoar.State() == eButtonState.activated)
		{
			ChangeButtonState(eButtonID.boar, eButtonState.inactive);
		}
		if (formChoosed == eFormChoosed.boar && buttonBoar.State() == eButtonState.inactive)
		{
			ChangeButtonState(eButtonID.boar, eButtonState.activated);
		}
	}

	private void SetFormChoosed(eFormChoosed newForm)
	{
		if (formChoosed != newForm)
		{
			formChoosed = newForm;
		}
	}

	public static float GetJoystickValue(eGameInputButton btnId)
	{
		if (!_singleton)
		{
			return 0f;
		}
		float result = 0f;
		if (btnId == eGameInputButton.flyVertical)
		{
			result = _singleton.joystickVertical.stickPower;
		}
		return result;
	}

	public static bool GetButtonState(eGameInputButton btnId)
	{
		if (!_singleton)
		{
			return false;
		}
		bool result = false;
		switch (btnId)
		{
		case eGameInputButton.flyVertical:
		case eGameInputButton.growing:
		case eGameInputButton.speedup:
			if (_singleton.buttonGrass.State() == eButtonState.activated || _singleton.buttonSpeedup.State() == eButtonState.activated || _singleton.buttonUp.State() == eButtonState.activated || _singleton.buttonDown.State() == eButtonState.activated)
			{
				result = true;
			}
			break;
		case eGameInputButton.skinBoar:
			result = _singleton.buttonBoar.State() == eButtonState.activated;
			break;
		case eGameInputButton.skinEagle:
			result = _singleton.buttonEagle.State() == eButtonState.activated;
			break;
		case eGameInputButton.powerUp:
			result = _singleton.buttonPwrUpGhost.State() == eButtonState.activated;
			break;
		}
		return result;
	}

	private static eButtonState GetButtonStateByID(eButtonID btnId)
	{
		return _singleton.GetButtonParams(btnId)?.State() ?? eButtonState.hidden;
	}

	private static eButtonID GIBtnToButtonID(eGameInputButton eGIB)
	{
		eButtonID result = eButtonID.none;
		switch (eGIB)
		{
		case eGameInputButton.growing:
			result = eButtonID.grass;
			break;
		case eGameInputButton.speedup:
			result = eButtonID.speedup;
			break;
		case eGameInputButton.skinBoar:
			result = eButtonID.boar;
			break;
		case eGameInputButton.skinEagle:
			result = eButtonID.eagle;
			break;
		case eGameInputButton.powerUp:
			result = eButtonID.powerUpGhost;
			break;
		}
		return result;
	}

	private static void DisableButton(eButtonID btnId, bool isDisable)
	{
		ButtonParams buttonParams = _singleton.GetButtonParams(btnId);
		if (buttonParams != null && buttonParams.State() != eButtonState.hidden)
		{
			eButtonState state = ((!isDisable) ? eButtonState.inactive : eButtonState.disabled);
			_singleton.ChangeButtonState(btnId, state);
		}
	}

	public void HideButton(eGameInputButton btnId, bool hideFlag)
	{
		if (hideFlag)
		{
			_singleton.ChangeButtonState(GIBtnToButtonID(btnId), eButtonState.hidden);
			return;
		}
		switch (btnId)
		{
		case eGameInputButton.growing:
			if (LevelAttributes.instance.PlayerSkin == EPlayerSkin.Man && _singleton.buttonGrass.State() == eButtonState.hidden)
			{
				_singleton.ChangeButtonState(eButtonID.grass, eButtonState.inactive);
			}
			break;
		case eGameInputButton.speedup:
			if (LevelAttributes.instance.PlayerSkin == EPlayerSkin.Boar && _singleton.buttonSpeedup.State() == eButtonState.hidden)
			{
				_singleton.ChangeButtonState(eButtonID.speedup, eButtonState.inactive);
			}
			break;
		case eGameInputButton.skinBoar:
			_singleton.ChangeButtonState(eButtonID.boar, eButtonState.inactive);
			break;
		case eGameInputButton.skinEagle:
			_singleton.ChangeButtonState(eButtonID.eagle, eButtonState.inactive);
			break;
		case eGameInputButton.powerUp:
			_singleton.ChangeButtonState(eButtonID.powerUpGhost, eButtonState.inactive);
			break;
		}
	}

	public void UnlockContent()
	{
		unlockContentGUI.material.color = Color.yellow;
		unlockContentGUI.text = "解锁新的概念原画";
		unlockContentGUI.gameObject.SetActiveRecursivelyCompat(state: true);
		unlockContentVisibleTime = 3f;
	}

	public eGuiGameHudState GetGuiState()
	{
		return menuState;
	}

	public void SetGuiState(eGuiGameHudState state)
	{
		if (state == menuState)
		{
			return;
		}
		eGuiGameHudState eGuiGameHudState2 = menuState;
		switch (state)
		{
		case eGuiGameHudState.pause:
			if ((bool)pauseScreen && pauseScreen.Enabled())
			{
				pauseScreen.StartShow(base.GetComponent<Camera>());
			}
			else
			{
				state = menuState;
			}
			break;
		case eGuiGameHudState.game:
			if ((bool)levelProgress)
			{
				levelProgress.gameObject.SetActiveRecursivelyCompat(state: true);
			}
			TimeController.StartPause(isPause: false);
			PlayerController.instance.DisableController(disable: false);
			GameInput.instance.HideControl(eGameInputButton.powerUp, storedButton.isHiddenGhost);
			GameInput.instance.HideControl(eGameInputButton.skinBoar, storedButton.isHiddenBoar);
			GameInput.instance.HideControl(eGameInputButton.skinEagle, storedButton.isHiddenEagle);
			if (messageString.state != MessageString.eMessageStringState.disable && (bool)messageString.textMesh)
			{
				messageString.textMesh.gameObject.SetActive(true);
			}
			if (!grassDontShow && GrassGifManager.instance.GetGrassCount() >= 300)
			{
				grassTextMesh.gameObject.SetActive(true);
				berryPicture.gameObject.SetActive(true);
			}
			break;
		case eGuiGameHudState.levelComplete:
			if ((bool)levelCompleteScreen && levelCompleteScreen.Enabled())
			{
				levelCompleteScreen.StartShow(base.GetComponent<Camera>());
			}
			else
			{
				state = menuState;
			}
			break;
		case eGuiGameHudState.powerUp:
			if (eGuiGameHudState2 != eGuiGameHudState.qte && (bool)powerUpScreen && powerUpScreen.Enabled())
			{
				powerUpScreen.StartShow(base.GetComponent<Camera>());
			}
			else
			{
				state = menuState;
			}
			break;
		case eGuiGameHudState.qte:
			if ((bool)qteScreen && qteScreen.Enabled())
			{
				if (eGuiGameHudState2 == eGuiGameHudState.powerUp)
				{
					powerUpScreen.StopShow();
					eGuiGameHudState2 = menuState;
				}
				qteScreen.StartShow(base.GetComponent<Camera>());
			}
			else
			{
				state = menuState;
			}
			break;
		}
		if (menuState == state)
		{
			return;
		}
		menuState = state;
		switch (eGuiGameHudState2)
		{
		case eGuiGameHudState.game:
			grassTextMesh.gameObject.SetActive(false);
			berryPicture.gameObject.SetActive(false);
			if ((bool)levelProgress)
			{
				levelProgress.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			if ((bool)messageString.textMesh)
			{
				messageString.textMesh.gameObject.SetActive(false);
			}
			storedButton.isHiddenBoar = GameInput.IsHiddenButton(eGameInputButton.skinBoar);
			storedButton.isHiddenEagle = GameInput.IsHiddenButton(eGameInputButton.skinEagle);
			storedButton.isHiddenGhost = GameInput.IsHiddenButton(eGameInputButton.powerUp);
			if (state != eGuiGameHudState.powerUp)
			{
				GameInput.instance.HideControl(eGameInputButton.powerUp, hideFlag: true);
			}
			GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: true);
			GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: true);
			break;
		case eGuiGameHudState.powerUp:
			if (GetButtonStateByID(eButtonID.powerUpGhost) == eButtonState.activated)
			{
				ChangeButtonState(eButtonID.powerUpGhost, eButtonState.inactive);
			}
			break;
		case eGuiGameHudState.qte:
			powerUpScreen.SetDisableTimeout(1f);
			break;
		case eGuiGameHudState.pause:
		case eGuiGameHudState.levelComplete:
			break;
		}
	}

	public static void SetScoreCounter(int score)
	{
		if (_singleton != null && _singleton.scoreCounter != null)
		{
			_singleton.scoreCounter.SetCounter(score);
		}
	}

	public static void UpdateCrystalProgress()
	{
		if (!(_singleton != null) || !(instance.ghostProgress != null))
		{
			return;
		}
		// KMFIX run-14: 灵体(羽蛇神)变身恒可用、计数固定 999（不改存档/不扣宝石）。
		DisableButton(eButtonID.powerUpGhost, isDisable: false);
		_singleton.ghostProgress.SetCounter(999);
		_singleton.ghostProgress.SetProgress(1f);
	}

	public static void SetQteTapCounter(int val)
	{
		if (_singleton != null && _singleton.qteScreen != null)
		{
			ScreenQTE screenQTE = _singleton.qteScreen as ScreenQTE;
			if ((bool)screenQTE)
			{
				screenQTE.SetQteTapCounter(val);
			}
		}
	}

	public static ScreenQTE GetQteScreen()
	{
		if (_singleton != null && _singleton.qteScreen != null)
		{
			return _singleton.qteScreen as ScreenQTE;
		}
		return null;
	}

	public static void ShowDeathHelp()
	{
		if (_singleton != null && _singleton.deathMsgHelp != null)
		{
			_singleton.deathMsgHelp.ShowMessage();
		}
	}
}
