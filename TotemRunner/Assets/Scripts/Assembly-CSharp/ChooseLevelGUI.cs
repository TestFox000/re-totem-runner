using System;
using UnityEngine;

public class ChooseLevelGUI : BaseCamera, TouchEventListener
{
	public enum eStateGUI
	{
		common = 0,
		scrolling = 1,
		returnToCommon = 2,
		loading = 3,
		options = 4,
		controlSettings = 5,
		soundSettings = 6,
		chitMenu = 7,
		titleScreen = 8,
		grassScreen = 9,
		showCutScene = 10,
		achievements = 11,
		buyMenu = 12,
		aboutMenu = 13,
		socialPost = 14,
		unused = -1
	}

	[Serializable]
	public class BlockListLineElement
	{
		public GameObject openUnselected;

		public GameObject openSelected;

		public GameObject closeUnselected;

		public GameObject closeSelected;
	}

	[Serializable]
	public class GrassData
	{
		[Serializable]
		public class GrassObject
		{
			public Animation aniObj;
		}

		public int mask;

		public int disableMask;

		public Material grassNoneMaterial;

		public GrassObject grass1;

		public GrassObject grass2;

		public GrassObject grass3;

		public GrassObject grass4;

		public GrassObject grass5;

		public GrassObject grassSuper;
	}

	[Serializable]
	public class LogoDescribe
	{
		public Transform titleSmall;

		public Transform titleBig;

		public Transform backSmall;

		public Transform backBig;
	}

	public AudioClip music;

	private AudioSource musicSource;

	private bool afterIntroCutSceneDoLoadTutorial;

	public BlockListLineElement[] blockLine;

	public Transform backButton;

	public Transform grassButton;

	public Transform almanacButton;

	public Transform introButton;

	public Transform blockLockFolder;

	public TextMesh blockLockText;

	public Transform blockLockSquare;

	public Transform starTotalControl;

	public TextMesh starTotalCounter;

	public Transform ghostTotalControl;

	public TextMesh ghostTotalCounter;

	public GameObject buyMenu;

	public TextMesh[] buyProducts;

	public GameObject buyFullVersion;

	public TextMesh buyFullVerText;

	public TextMesh buyText;

	public GameObject btnBuyFullVersion;

	private bool buyMenuReady;

	private bool buyExitToCommon = true;

	public GameObject aboutMenu;

	public GameObject aboutButton;

	public GameObject socButtonFB;

	public GameObject crystalButton;

	public GameObject optionButton;

	public GameObject touchScreenLabel;

	public GameObject GCButton;

	public GameObject mainMenu;

	public int socialMode;

	private eStateGUI guiState;

	private bool isWaitAsync;

	private int buyIndex = -1;

	public Transform[] blockList;

	private int currentBlock;

	public Transform mainScreenBlock;

	public Transform grassScreenBlock;

	public Transform cutsceneScreenBlock;

	public Transform cutsceneSceneFolder;

	public GrassData grassData;

	public float scrollingSpeed = 2f;

	public float scrollingSpringiness = 4f;

	private float scrollingPhase;

	private float scrollingSpeedReal = 1f;

	private int scrollingDestinationBlock;

	private TouchControllerForGUI touchController;

	private bool isSlideReaction;

	public Material materialLevelBackPictureLock;

	public Material materialLevelBackPictureUnlock;

	public Material materialLevelBackPictureUncomplete;

	public Material materialLabelComplete;

	public Material materialLabelIncomplete;

	public TouchReactButton btnLeft;

	public TouchReactButton btnRight;

	private float showMessageTimeLeft;

	private string showMessageText = string.Empty;

	private eStateGUI stateChangeFromGUI;

	private int mainScreenTouchDisableFrame;

	private bool achViewPopup = true;

	private float achViewTime = -1f;

	private Vector3 achViewStartPos = Vector3.zero;

	public Vector3 achViewMoveDist = Vector3.right * 512f;

	public float achViewMoveTime = 2f;

	public Transform achViewTransform;

	public TouchReactButton achViewBtn;

	public GameObject achLinePrefab;

	private int modMsgBox;

	private int modMsgBoxSub;

	public GameObject msgBoxRoot;

	public LogoDescribe logoDescribe;

	private static ChooseLevelGUI _singleton;

	private float lastMsgBoxTime;

	public new static ChooseLevelGUI instance => _singleton;

	private void HideMessageBox()
	{
		modMsgBox = 0;
		if (msgBoxRoot != null)
		{
			msgBoxRoot.SetActiveRecursivelyCompat(state: false);
		}
	}

	private void ShowMessageBox(string msg, int mod, int subMod)
	{
		if (!(msgBoxRoot != null))
		{
			return;
		}
		modMsgBox = mod;
		modMsgBoxSub = subMod;
		msgBoxRoot.SetActiveRecursivelyCompat(state: true);
		Transform transform = null;
		switch (modMsgBox)
		{
		case 1:
			transform = msgBoxRoot.transform.Find("Ok");
			if (transform != null)
			{
				transform.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			break;
		case 2:
			transform = msgBoxRoot.transform.Find("Yes");
			if (transform != null)
			{
				transform.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			transform = msgBoxRoot.transform.Find("No");
			if (transform != null)
			{
				transform.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			break;
		case 3:
			transform = msgBoxRoot.transform.Find("Ok");
			if (transform != null)
			{
				transform.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			transform = msgBoxRoot.transform.Find("Yes");
			if (transform != null)
			{
				transform.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			transform = msgBoxRoot.transform.Find("No");
			if (transform != null)
			{
				transform.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			break;
		}
		string[] array = msg.Split("#".ToCharArray());
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < 3; i++)
		{
			transform = msgBoxRoot.transform.Find("tl" + (i + 1));
			if (transform != null)
			{
				TextMesh component = transform.GetComponent<TextMesh>();
				component.text = ((i >= num) ? string.Empty : array[i]);
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		_singleton = this;
		TimeController.ResetToInit();
		if (!PluginUtils.IsShowFacebook())
		{
			socButtonFB = null;
		}
		HideMessageBox();
		TitleTurnOn(isOn: true);
		if (mainScreenBlock != null)
		{
			GUtils.SetTransformX(base.transform, mainScreenBlock.position.x);
		}
		touchController = TouchControllerForGUI.instance;
		touchController.AppendListener(this);
		if (achViewTransform != null)
		{
			achViewStartPos = achViewTransform.position;
			achViewStartPos.x -= base.GetComponent<Camera>().ViewportToWorldPoint(Vector3.one).x - base.GetComponent<Camera>().transform.position.x - 512f;
			achViewTransform.position = achViewStartPos;
			InitAchievementList();
		}
	}

	private void TitleTurnOn(bool isOn)
	{
	}

	private void UpdateAchievementList()
	{
		Transform transform = achViewTransform.Find("VertPanel");
		if (!(transform != null))
		{
			return;
		}
		int num = 1;
		for (int i = 0; i < 52; i++)
		{
			if (GameSettings.gameSettings.achievements[i].achType != AchievementSystem.eAchievementType.achievement)
			{
				continue;
			}
			Transform transform2 = transform.Find("line" + num);
			if (transform2 != null)
			{
				Transform transform3 = transform2.Find("Check");
				if (transform3 != null)
				{
					transform3.gameObject.GetComponent<Renderer>().enabled = GameSettings.gameSettings.achievements[i].isReach;
				}
			}
			num++;
		}
	}

	private void InitAchievementList()
	{
		if (achLinePrefab != null)
		{
			Transform transform = achViewTransform.Find("VertPanel");
			if (transform != null)
			{
				Vector3 position = transform.position;
				position.y += 300f;
				Quaternion identity = Quaternion.identity;
				int num = 1;
				string[] array = new string[2];
				for (int i = 0; i < 52; i++)
				{
					if (GameSettings.gameSettings.achievements[i].achType != AchievementSystem.eAchievementType.achievement || GameSettings.gameSettings.achievements[i].descr == null || GameSettings.gameSettings.achievements[i].descr.Length == 0)
					{
						continue;
					}
					GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(achLinePrefab, position, identity);
					gameObject.name = "line" + num;
					gameObject.transform.parent = transform;
					position.y -= 164f;
					num++;
					Transform transform2 = gameObject.transform.Find("TopText");
					if (transform2 != null)
					{
						TextMesh component = transform2.GetComponent<TextMesh>();
						if (component != null)
						{
							component.text = GameSettings.gameSettings.achievements[i].name;
						}
					}
					transform2 = gameObject.transform.Find("Check");
					if (transform2 != null)
					{
						transform2.gameObject.GetComponent<Renderer>().enabled = GameSettings.gameSettings.achievements[i].isReach;
					}
					int num2 = GameSettings.gameSettings.achievements[i].descr.IndexOf("#");
					if (num2 >= 0)
					{
						array[0] = GameSettings.gameSettings.achievements[i].descr.Substring(0, num2);
						array[1] = GameSettings.gameSettings.achievements[i].descr.Substring(num2 + 1);
					}
					else
					{
						array[0] = GameSettings.gameSettings.achievements[i].descr;
						array[1] = string.Empty;
					}
					transform2 = gameObject.transform.Find("BottomText1");
					if (transform2 != null)
					{
						TextMesh component = transform2.GetComponent<TextMesh>();
						if (component != null)
						{
							component.text = array[0];
						}
					}
					transform2 = gameObject.transform.Find("BottomText2");
					if (transform2 != null)
					{
						TextMesh component = transform2.GetComponent<TextMesh>();
						if (component != null)
						{
							component.text = array[1];
						}
					}
				}
				TouchReactSlider component2 = achViewTransform.GetComponent<TouchReactSlider>();
				if (component2 != null)
				{
					component2.maxY = (float)(num - 5) * component2.snapStepH;
					if (component2.maxY < component2.minY)
					{
						component2.maxY = component2.minY;
					}
				}
			}
			else
			{
				Debug.LogWarning("Warning! Init Achievement List: cant find VertPanel");
			}
		}
		else
		{
			Debug.LogWarning("Warning! Init Achievement List: empty achievement line prefab");
		}
	}

	private void HideBlockLocking(bool isHide)
	{
		if (blockLockFolder.gameObject.activeSelf != !isHide)
		{
			blockLockFolder.gameObject.SetActiveRecursivelyCompat(!isHide);
		}
		if (isHide)
		{
			return;
		}
		if (IsFreeVersion() && currentBlock > 1)
		{
			if (btnBuyFullVersion != null)
			{
				btnBuyFullVersion.SetActiveRecursivelyCompat(state: true);
			}
			if (blockLockSquare != null)
			{
				Vector3 localScale = blockLockSquare.localScale;
				localScale.y = 384f;
				blockLockSquare.localScale = localScale;
				localScale = blockLockSquare.localPosition;
				localScale.y = -64f;
				blockLockSquare.localPosition = localScale;
			}
		}
		else
		{
			if (btnBuyFullVersion != null)
			{
				btnBuyFullVersion.SetActiveRecursivelyCompat(state: false);
			}
			if (blockLockSquare != null)
			{
				Vector3 localScale2 = blockLockSquare.localScale;
				localScale2.y = 256f;
				blockLockSquare.localScale = localScale2;
				localScale2 = blockLockSquare.localPosition;
				localScale2.y = 0f;
				blockLockSquare.localPosition = localScale2;
			}
		}
	}

	private bool IsBlockLocked(int blockIdx)
	{
		return false;
	}

	private void SetupBlockLocking()
	{
		GameRecordsData records = GameSettings.gameSettings.records;
		if (IsBlockLocked(currentBlock))
		{
			if (currentBlock == 1)
			{
				blockLockText.text = "完成序章后解锁";
			}
			else if (IsFreeVersion() && currentBlock > 1)
			{
				blockLockText.text = "需要完整版";
			}
			else
			{
				blockLockText.text = "集齐" + records.blockData[currentBlock].starsToUnlock + "颗星后解锁";
			}
			HideBlockLocking(isHide: false);
		}
		else
		{
			HideBlockLocking(isHide: true);
		}
	}

	private void HideBlockLine()
	{
		for (int i = 0; i < blockLine.Length; i++)
		{
			blockLine[i].openUnselected.SetActive(false);
			blockLine[i].openSelected.SetActive(false);
			blockLine[i].closeUnselected.SetActive(false);
			blockLine[i].closeSelected.SetActive(false);
		}
	}

	private int SetupBlockLine()
	{
		GameRecordsData records = GameSettings.gameSettings.records;
		int result = 0;
		bool flag = false;
		for (int i = 1; i < records.blockData.Length; i++)
		{
			bool flag2 = IsBlockLocked(i);
			blockLine[i].openUnselected.SetActive(!flag2 && i != currentBlock);
			blockLine[i].openSelected.SetActive(!flag2 && i == currentBlock);
			blockLine[i].closeUnselected.SetActive(flag2 && i != currentBlock);
			blockLine[i].closeSelected.SetActive(flag2 && i == currentBlock);
			if (!flag2 && records.blockData[i - 1].level[records.blockData[i - 1].level.Length - 1].lockStatus == GameRecordsData.eLockStatus.unlock)
			{
				flag = true;
				result = i;
			}
		}
		if (IsFreeVersion() && records.blockData[1].level[3].lockStatus == GameRecordsData.eLockStatus.unlock)
		{
			return 2;
		}
		if (flag)
		{
			return result;
		}
		return -1;
	}

	private void Start()
	{
		GameDataStorage.LoadGameSettings();
		ReadAllLevelData();
		int num = SetupBlockLine();
		if (num >= 0)
		{
			SetCurrentBlock(num);
			if (blockList != null)
			{
				GUtils.SetTransformX(base.transform, blockList[currentBlock].position.x);
			}
			guiState = eStateGUI.unused;
			SetGUIState(eStateGUI.common);
		}
		else
		{
			SetGUIState(eStateGUI.titleScreen);
		}
		GameObject gameObject = new GameObject("Music");
		gameObject.transform.parent = base.transform;
		musicSource = gameObject.AddComponent<AudioSource>();
		musicSource.clip = music;
		musicSource.loop = true;
		musicSource.spatialBlend = 0f;
		musicSource.volume = GameSettings.sound.musicVolume;
		musicSource.Play();
	}

	private void Update()
	{
		if (showMessageTimeLeft > 0f)
		{
			showMessageTimeLeft -= Time.deltaTime;
		}
		if (IsLoading())
		{
			return;
		}
		if (modMsgBox != 0)
		{
			if (modMsgBox == 3 && modMsgBoxSub == 0 && !CrystalManager.IsConnectingGC())
			{
				if (CrystalManager.IsAvailableGC())
				{
					HideMessageBox();
				}
				else
				{
					ShowMessageBox("Game Center.#Unable to connect.", 2, -1);
				}
			}
			if (GameInput.IsBackPressed())
			{
				HideMessageBox();
			}
			return;
		}
		switch (guiState)
		{
		case eStateGUI.common:
			if (GameInput.IsBackPressed())
			{
				SetGUIState(eStateGUI.titleScreen);
			}
			break;
		case eStateGUI.scrolling:
		{
			scrollingSpeedReal = Mathf.Lerp(scrollingSpeedReal, scrollingSpeed, Time.deltaTime * scrollingSpringiness);
			scrollingPhase = Mathf.Clamp01(scrollingPhase + scrollingSpeedReal * Time.deltaTime);
			float x = blockList[currentBlock].position.x;
			float x2 = blockList[scrollingDestinationBlock].position.x;
			if (scrollingPhase >= 1f)
			{
				SetGUIState(eStateGUI.common);
				GUtils.SetTransformX(base.transform, x2);
				SetCurrentBlock(scrollingDestinationBlock);
				if (btnLeft != null)
				{
					if (scrollingDestinationBlock > 0)
					{
						if (btnLeft.buttonState == TouchReactButton.eReactButtonState.hidden)
						{
							btnLeft.SetButtonState(TouchReactButton.eReactButtonState.enable);
						}
					}
					else if (btnLeft.buttonState != TouchReactButton.eReactButtonState.hidden)
					{
						btnLeft.SetButtonState(TouchReactButton.eReactButtonState.hidden);
					}
				}
				if (!(btnRight != null))
				{
					break;
				}
				if (scrollingDestinationBlock < blockList.Length - 1)
				{
					if (btnRight.buttonState == TouchReactButton.eReactButtonState.hidden)
					{
						btnRight.SetButtonState(TouchReactButton.eReactButtonState.enable);
					}
				}
				else if (btnRight.buttonState != TouchReactButton.eReactButtonState.hidden)
				{
					btnRight.SetButtonState(TouchReactButton.eReactButtonState.hidden);
				}
			}
			else
			{
				GUtils.SetTransformX(base.transform, x + (x2 - x) * scrollingPhase);
			}
			break;
		}
		case eStateGUI.returnToCommon:
			scrollingSpeedReal = Mathf.Lerp(scrollingSpeedReal, 200f * scrollingSpeed, Time.deltaTime * scrollingSpringiness);
			if (scrollingPhase < 0f)
			{
				scrollingPhase = Mathf.Clamp(scrollingPhase + scrollingSpeedReal * Time.deltaTime, scrollingPhase, 0f);
			}
			else
			{
				scrollingPhase = Mathf.Clamp(scrollingPhase - scrollingSpeedReal * Time.deltaTime, 0f, scrollingPhase);
			}
			GUtils.SetTransformX(base.transform, blockList[currentBlock].position.x + scrollingPhase);
			if (scrollingPhase == 0f)
			{
				SetGUIState(eStateGUI.common);
			}
			break;
		case eStateGUI.titleScreen:
			if (mainScreenTouchDisableFrame > 0)
			{
				mainScreenTouchDisableFrame--;
			}
			if (GameInput.IsBackPressed())
			{
				ShowMessageBox("确定要#退出游戏吗？", 1, 2);
			}
			break;
		case eStateGUI.grassScreen:
			if (grassData.grass1.aniObj.gameObject.activeSelf && !grassData.grass1.aniObj.isPlaying)
			{
				grassData.grass1.aniObj.Play(((grassData.disableMask & 1) == 0) ? "idleEnabled" : "idleDisabled");
			}
			if (grassData.grass2.aniObj.gameObject.activeSelf && !grassData.grass2.aniObj.isPlaying)
			{
				grassData.grass2.aniObj.Play(((grassData.disableMask & 4) == 0) ? "idleEnabled" : "idleDisabled");
			}
			if (grassData.grass3.aniObj.gameObject.activeSelf && !grassData.grass3.aniObj.isPlaying)
			{
				grassData.grass3.aniObj.Play(((grassData.disableMask & 8) == 0) ? "idleEnabled" : "idleDisabled");
			}
			if (grassData.grass4.aniObj.gameObject.activeSelf && !grassData.grass4.aniObj.isPlaying)
			{
				grassData.grass4.aniObj.Play(((grassData.disableMask & 0x10) == 0) ? "idleEnabled" : "idleDisabled");
			}
			if (grassData.grass5.aniObj.gameObject.activeSelf && !grassData.grass5.aniObj.isPlaying)
			{
				grassData.grass5.aniObj.Play(((grassData.disableMask & 0x20) == 0) ? "idleEnabled" : "idleDisabled");
			}
			if (grassData.grassSuper.aniObj.gameObject.activeSelf && !grassData.grassSuper.aniObj.isPlaying)
			{
				grassData.grassSuper.aniObj.Play(((grassData.disableMask & 0x40) == 0) ? "idleEnabled" : "idleDisabled");
			}
			break;
		case eStateGUI.showCutScene:
			if (CutSceneController.IsFinishedCutScene())
			{
				if (afterIntroCutSceneDoLoadTutorial)
				{
					SetGUIState(eStateGUI.loading);
					LaunchLoading("Tutorial");
				}
				else
				{
					SetGUIState(eStateGUI.titleScreen);
				}
			}
			break;
		case eStateGUI.achievements:
			if (achViewTime >= 0f)
			{
				achViewTime -= Time.deltaTime;
				if (achViewTime > 0f)
				{
					if (achViewPopup)
					{
						achViewTransform.position = Vector3.Lerp(achViewStartPos + achViewMoveDist, achViewStartPos, achViewTime / achViewMoveTime);
					}
					else
					{
						achViewTransform.position = Vector3.Lerp(achViewStartPos, achViewStartPos + achViewMoveDist, achViewTime / achViewMoveTime);
					}
				}
				else
				{
					achViewTime = -1f;
					if (achViewPopup)
					{
						achViewTransform.position = achViewStartPos + achViewMoveDist;
					}
					else
					{
						achViewTransform.position = achViewStartPos;
						SetGUIState(eStateGUI.titleScreen);
					}
				}
			}
			if (GameInput.IsBackPressed())
			{
				MakeAchievementVisible(!achViewPopup);
			}
			break;
		case eStateGUI.buyMenu:
			if (GameInput.IsBackPressed())
			{
				SetGUIState((!buyExitToCommon) ? eStateGUI.titleScreen : eStateGUI.common);
			}
			else if (buyMenuReady)
			{
				if (!isWaitAsync)
				{
					break;
				}
				string responseForBuy = InAppPurshase.GetResponseForBuy();
				if (!(responseForBuy != string.Empty))
				{
					break;
				}
				if (responseForBuy == "ok")
				{
					int num = 0;
					switch (buyIndex)
					{
					case 0:
						num = 5 * GuiGameHud.ghostCost;
						break;
					case 1:
						num = 15 * GuiGameHud.ghostCost;
						break;
					case 2:
						num = 30 * GuiGameHud.ghostCost;
						break;
					case 3:
						PlayerPrefs.SetInt("unlockfull", 1);
						ReadAllLevelData();
						break;
					}
					if (num > 0)
					{
						int crystalSummary = GameDataStorage.GetCrystalSummary();
						GameDataStorage.SetCrystalSummary(crystalSummary + num);
						UpdateGhostCounter();
						if (!PlayerPrefs.HasKey("userHasBoughtSomething"))
						{
							PlayerPrefs.SetInt("userHasBoughtSomething", 1);
							AnalyticsManager.LogEvent("uniquePayingUser");
						}
						int value = PlayerPrefs.GetInt("buyGems", 0) + num / GuiGameHud.ghostCost;
						PlayerPrefs.SetInt("buyGems", value);
						AnalyticsManager.LogEventWithParam("purchaseMade", "IAP", "com.chillingo.totemrunner.f" + (buyIndex + 1));
					}
				}
				WaitAsyncRequest(isWait: false);
				if (IsFreeVersion())
				{
					if (buyFullVersion != null)
					{
						buyFullVersion.SetActiveRecursivelyCompat(state: true);
					}
				}
				else if (buyProducts != null)
				{
					for (int i = 0; i < buyProducts.Length; i++)
					{
						if (buyProducts[i] != null)
						{
							buyProducts[i].gameObject.SetActiveRecursivelyCompat(state: true);
						}
					}
				}
				if (buyText != null)
				{
					buyText.gameObject.SetActive(false);
				}
			}
			else
			{
				if (!InAppPurshase.IsPurchaseReady())
				{
					break;
				}
				buyMenuReady = true;
				string productPrice = InAppPurshase.GetProductPrice(0);
				if (productPrice == null || productPrice == string.Empty)
				{
					if (buyText != null)
					{
						buyText.text = "Unable connect to store";
					}
					break;
				}
				if (buyText != null)
				{
					buyText.gameObject.SetActive(false);
				}
				if (IsFreeVersion())
				{
					if (buyFullVersion != null)
					{
						buyFullVersion.SetActiveRecursivelyCompat(state: true);
					}
					if (buyFullVerText != null)
					{
						buyFullVerText.text = GetBuyProductText(3);
					}
				}
				else
				{
					if (buyProducts == null)
					{
						break;
					}
					for (int j = 0; j < buyProducts.Length; j++)
					{
						if (buyProducts[j] != null)
						{
							buyProducts[j].gameObject.SetActiveRecursivelyCompat(state: true);
							buyProducts[j].text = GetBuyProductText(j);
						}
					}
				}
			}
			break;
		case eStateGUI.aboutMenu:
			if (GameInput.IsBackPressed())
			{
				SetGUIState(eStateGUI.titleScreen);
			}
			break;
		case eStateGUI.options:
			if (GameInput.IsBackPressed())
			{
				SetGUIState(eStateGUI.titleScreen);
			}
			break;
		case eStateGUI.socialPost:
			switch (SocialNetPoster.GetPostingStatus())
			{
			case -1:
				SetGUIState(eStateGUI.titleScreen);
				break;
			case 0:
				break;
			case 1:
				CheckSocialNetAward();
				SetGUIState(eStateGUI.titleScreen);
				break;
			case 2:
				ShowMessageBox("Post to facebook failed", 2, -1);
				SetGUIState(eStateGUI.titleScreen);
				break;
			}
			break;
		case eStateGUI.loading:
		case eStateGUI.controlSettings:
		case eStateGUI.soundSettings:
		case eStateGUI.chitMenu:
			break;
		}
	}

	private void CheckSocialNetAward()
	{
		bool flag = false;
		int num = PlayerPrefs.GetInt("socialNetAward", 0);
		if (socialMode == 0 && (num & 1) == 0)
		{
			flag = true;
			num++;
		}
		if (flag)
		{
			PlayerPrefs.SetInt("socialNetAward", num);
			int num2 = 100;
			ShowMessageBox("Thanks for posting#on facebook.#get reward " + num2 + " gems", 2, -1);
			int crystalSummary = GameDataStorage.GetCrystalSummary();
			crystalSummary += num2;
			GameDataStorage.SetCrystalSummary(crystalSummary);
			UpdateGhostCounter();
		}
	}

	public static string GetBuyProductText(int productIdx)
	{
		string text = string.Empty;
		switch (productIdx)
		{
		case 0:
			text = "x 5 = ";
			break;
		case 1:
			text = "x 15 = ";
			break;
		case 2:
			text = "x 30 = ";
			break;
		case 3:
			text = "Buy Full Version\nfor ";
			break;
		}
		return text + InAppPurshase.GetProductPrice(productIdx);
	}

	public override void LateUpdate()
	{
		if (stateChangeFromGUI != guiState)
		{
			SetGUIState(stateChangeFromGUI);
		}
		base.LateUpdate();
	}

	public override void OnGUI()
	{
		base.OnGUI();
		GUI.color = Color.white;
		switch (guiState)
		{
		case eStateGUI.loading:
			break;
		case eStateGUI.options:
			if (PluginUtils.IsChits())
			{
				Color color = GUI.color;
				color.a = 0f;
				GUI.color = color;
				if (GUI.Button(new Rect(0f, 0f, 64f, 64f), string.Empty))
				{
					SetGUIState(eStateGUI.chitMenu);
					showMessageTimeLeft = 0f;
				}
				color.a = 1f;
				GUI.color = color;
			}
			break;
		case eStateGUI.chitMenu:
			OnGuiChitMenu();
			break;
		case eStateGUI.titleScreen:
			break;
		case eStateGUI.controlSettings:
		case eStateGUI.soundSettings:
			break;
		}
	}

	private void OnGuiChitMenu()
	{
		float pixelWidth = base.GetComponent<Camera>().pixelWidth;
		float pixelHeight = base.GetComponent<Camera>().pixelHeight;
		float num = 300f;
		float num2 = 42f;
		float num3 = 16f;
		float num4 = 60f;
		for (int num5 = (int)pixelHeight; num5 < 480; num5 *= 2)
		{
			num2 *= 0.5f;
			num3 *= 0.5f;
			num4 *= 0.5f;
		}
		int num6 = 7;
		bool flag = true;
		if (PlayerPrefs.GetInt("unlockfull", 0) != 0)
		{
			flag = false;
		}
		if (flag)
		{
			num6++;
		}
		float num7 = num2 + num3;
		float num8 = (pixelWidth - num) * 0.5f;
		float num9 = num7 * (float)num6 + num4 + num3;
		float num10 = (pixelHeight - num9) * 0.5f;
		GUI.Label(new Rect(num8 - 20f, num10, num + 40f, num9), "Chit Menu", new GUIStyle("Box"));
		num10 += num4;
		if (GUI.Button(new Rect(num8, num10, num, num2), "Unlock all levels"))
		{
			showMessageTimeLeft = 2f;
			showMessageText = "All levels unlocked!";
			ChitUtils.ChitUnlockAllLevels(isComplete: false);
			ReadAllLevelData();
		}
		num10 += num7;
		if (GUI.Button(new Rect(num8, num10, num, num2), "Complete all levels"))
		{
			showMessageTimeLeft = 2f;
			showMessageText = "All levels complete!";
			ChitUtils.ChitUnlockAllLevels(isComplete: true);
			ReadAllLevelData();
		}
		num10 += num7;
		if (GUI.Button(new Rect(num8, num10, num, num2), "Take all seeds"))
		{
			showMessageTimeLeft = 2f;
			showMessageText = "All seeds taken!";
			PlayerPrefs.SetInt("HerbariumMask", 125);
		}
		num10 += num7;
		if (GUI.Button(new Rect(num8, num10, num, num2), "Reset all saved data"))
		{
			showMessageTimeLeft = 2f;
			showMessageText = "All saved data was reseted!";
			PlayerPrefs.DeleteAll();
			GameDataStorage.LoadRecords(GameSettings.gameSettings.records);
			ReadAllLevelData();
		}
		num10 += num7;
		if (GUI.Button(new Rect(num8, num10, num, num2), "Add 100 Crystal"))
		{
			int crystalSummary = GameDataStorage.GetCrystalSummary();
			crystalSummary += 100;
			GameDataStorage.SetCrystalSummary(crystalSummary);
			showMessageTimeLeft = 2f;
			showMessageText = "Crystall quantity = " + crystalSummary + " now!";
			UpdateGhostCounter();
		}
		num10 += num7;
		int num11 = PlayerPrefs.GetInt("dbgShowDbgText", 0);
		if (GUI.Button(new Rect(num8, num10, num, num2), (num11 != 0) ? "Off Debug" : "On Debug"))
		{
			num11 = 1 - num11;
			PlayerPrefs.SetInt("dbgShowDbgText", num11);
		}
		num10 += num7;
		if (flag)
		{
			if (GUI.Button(new Rect(num8, num10, num, num2), "Unlock full version"))
			{
				PlayerPrefs.SetInt("unlockfull", 1);
				ReadAllLevelData();
			}
			num10 += num7;
		}
		num10 += 10f;
		if (GUI.Button(new Rect(num8, num10, num, num2), "Back"))
		{
			SetGUIState(eStateGUI.options);
		}
		num10 += num7;
		if (showMessageTimeLeft > 0f)
		{
			num10 += num7;
			if (showMessageTimeLeft < 0.5f)
			{
				GUI.color = new Color(1f, 1f, 1f, showMessageTimeLeft / 0.5f);
				GUI.Label(new Rect(num8, num10, num, num2), showMessageText);
				GUI.color = Color.white;
			}
			else
			{
				GUI.Label(new Rect(num8, num10, num, num2), showMessageText);
			}
			num10 += num7;
		}
	}

	public void LaunchBlockScrolling(float deltaX, float deltaTime, bool sideIsRight)
	{
		scrollingDestinationBlock = ((!sideIsRight) ? (currentBlock - 1) : (currentBlock + 1));
		if (scrollingDestinationBlock < 0 || scrollingDestinationBlock >= blockList.Length)
		{
			SetGUIState(eStateGUI.returnToCommon);
			return;
		}
		SetGUIState(eStateGUI.scrolling);
		scrollingPhase = deltaX;
		scrollingSpeedReal = deltaX / deltaTime;
	}

	public bool TouchStart(Vector2 pos)
	{
		isSlideReaction = false;
		if (guiState == eStateGUI.common)
		{
			return true;
		}
		return false;
	}

	public void TouchFinish(Vector2 posStart, Vector2 posEnd, float deltaTime)
	{
		if (guiState != eStateGUI.common)
		{
			return;
		}
		if (isSlideReaction)
		{
			Vector2 zero = Vector2.zero;
			zero.x = blockList[currentBlock].position.x - base.transform.position.x;
			zero.x /= Camera.main.pixelWidth;
			if (zero.x > 0.01f)
			{
				LaunchBlockScrolling(zero.x, deltaTime, sideIsRight: false);
			}
			else if (zero.x < -0.01f)
			{
				LaunchBlockScrolling(0f - zero.x, deltaTime, sideIsRight: true);
			}
			isSlideReaction = false;
		}
		else
		{
			SetGUIState(eStateGUI.returnToCommon);
		}
	}

	public void TouchMoving(Vector2 posDelta, float deltaTime)
	{
		if (guiState == eStateGUI.common && (!(posDelta.x > 0f) || currentBlock != 0) && (!(posDelta.x < 0f) || currentBlock != blockList.Length - 1))
		{
			float x = blockList[currentBlock].position.x - posDelta.x;
			GUtils.SetTransformX(base.transform, x);
		}
	}

	public int GetZOrder()
	{
		return 0;
	}

	public bool TouchSlide(int slideNum, Vector2 deltaPos, float deltaTime)
	{
		isSlideReaction = true;
		return false;
	}

	public bool TouchClick(int clickNum)
	{
		return false;
	}

	public eStateGUI GetGUIState()
	{
		return guiState;
	}

	private void SetGUIState(eStateGUI state)
	{
		if (guiState != state)
		{
			switch (guiState)
			{
			case eStateGUI.options:
				GameDataStorage.SaveGameSettings();
				MakeOptionVisible(isVisible: false);
				if ((bool)optionButton)
				{
					optionButton.SetActive(false);
				}
				break;
			case eStateGUI.grassScreen:
				if (grassData.disableMask != GameDataStorage.GetHerbariumDisableMask())
				{
					GameDataStorage.SetHerbariumDisableMask(grassData.disableMask);
				}
				if (grassScreenBlock != null)
				{
					ExecutorEnableByProfile componentInChildren = grassScreenBlock.GetComponentInChildren<ExecutorEnableByProfile>();
					if (componentInChildren != null)
					{
						componentInChildren.DisableThisObject(isDisable: true);
					}
				}
				break;
			case eStateGUI.showCutScene:
				if (musicSource != null)
				{
					musicSource.Play();
				}
				CutSceneController.StopCutScene();
				if (cutsceneSceneFolder != null)
				{
					cutsceneSceneFolder.gameObject.SetActiveRecursivelyCompat(state: false);
				}
				break;
			case eStateGUI.titleScreen:
				if (introButton != null)
				{
					introButton.gameObject.SetActive(false);
				}
				if (achViewBtn != null)
				{
					achViewBtn.gameObject.SetActive(false);
				}
				if (aboutButton != null)
				{
					aboutButton.SetActive(false);
				}
				if (socButtonFB != null)
				{
					socButtonFB.SetActive(false);
				}
				if (crystalButton != null)
				{
					crystalButton.SetActive(false);
				}
				if (optionButton != null)
				{
					optionButton.SetActive(false);
				}
				if (touchScreenLabel != null)
				{
					touchScreenLabel.SetActive(false);
				}
				if (GCButton != null)
				{
					GCButton.SetActive(false);
				}
				if (mainMenu != null)
				{
					mainMenu.SetActiveRecursivelyCompat(state: false);
				}
				if (state != eStateGUI.aboutMenu && state != eStateGUI.socialPost)
				{
					CrystalManager.ShowProfile(isShow: false);
				}
				break;
			case eStateGUI.achievements:
				if (achViewTransform != null)
				{
					achViewTransform.gameObject.SetActiveRecursivelyCompat(state: false);
					achViewBtn.gameObject.SetActiveRecursivelyCompat(state: true);
					achViewTransform.position = achViewStartPos;
				}
				break;
			case eStateGUI.buyMenu:
				if (buyMenu != null)
				{
					buyMenu.SetActiveRecursivelyCompat(state: false);
					InAppPurshase.FinishPurchase();
				}
				break;
			case eStateGUI.aboutMenu:
				if (aboutMenu != null)
				{
					aboutMenu.SetActiveRecursivelyCompat(state: false);
				}
				break;
			}
			switch (state)
			{
			case eStateGUI.common:
				backButton.gameObject.SetActive(true);
				if (grassButton != null)
				{
					grassButton.gameObject.SetActive(true);
				}
				almanacButton.gameObject.SetActive(true);
				starTotalControl.gameObject.SetActiveRecursivelyCompat(state: true);
				ghostTotalControl.gameObject.SetActiveRecursivelyCompat(state: true);
				SetupBlockLocking();
				break;
			case eStateGUI.scrolling:
				loadingLevelName = string.Empty;
				if (!blockList[scrollingDestinationBlock].gameObject.activeSelf)
				{
					blockList[scrollingDestinationBlock].gameObject.SetActiveRecursivelyCompat(state: true);
				}
				break;
			case eStateGUI.returnToCommon:
				scrollingPhase = base.transform.position.x - blockList[currentBlock].position.x;
				scrollingSpeedReal = scrollingSpeed * 20f;
				break;
			case eStateGUI.titleScreen:
				loadingLevelName = string.Empty;
				if (mainScreenBlock != null)
				{
					GUtils.SetTransformX(base.transform, mainScreenBlock.position.x);
				}
				HideBlockLine();
				HideBlockLocking(isHide: true);
				if (backButton != null)
				{
					backButton.gameObject.SetActive(false);
				}
				if (grassButton != null)
				{
					grassButton.gameObject.SetActive(false);
				}
				if (almanacButton != null)
				{
					almanacButton.gameObject.SetActive(false);
				}
				if (aboutButton != null)
				{
					aboutButton.SetActive(true);
				}
				if (socButtonFB != null)
				{
					socButtonFB.SetActive(true);
				}
				if (crystalButton != null)
				{
					crystalButton.SetActive(true);
				}
				if (optionButton != null)
				{
					optionButton.SetActive(true);
				}
				if (touchScreenLabel != null)
				{
					touchScreenLabel.SetActive(true);
				}
				if (GCButton != null)
				{
					GCButton.SetActive(true);
				}
				if (mainMenu != null)
				{
					mainMenu.SetActiveRecursivelyCompat(state: true);
				}
				starTotalControl.gameObject.SetActiveRecursivelyCompat(state: false);
				ghostTotalControl.gameObject.SetActiveRecursivelyCompat(state: false);
				if (introButton != null)
				{
					introButton.gameObject.SetActive(true);
				}
				if (achViewBtn != null)
				{
					achViewBtn.gameObject.SetActive(true);
				}
				mainScreenTouchDisableFrame = 2;
				MakeOptionVisible(isVisible: false);
				if (guiState != eStateGUI.aboutMenu && guiState != eStateGUI.socialPost)
				{
					CrystalManager.ShowProfile(isShow: true);
				}
				break;
			case eStateGUI.grassScreen:
				loadingLevelName = string.Empty;
				if (grassScreenBlock != null)
				{
					GUtils.SetTransformX(base.transform, grassScreenBlock.position.x);
				}
				HideBlockLine();
				HideBlockLocking(isHide: true);
				backButton.gameObject.SetActive(false);
				if (grassButton != null)
				{
					grassButton.gameObject.SetActive(false);
				}
				almanacButton.gameObject.SetActive(false);
				starTotalControl.gameObject.SetActiveRecursivelyCompat(state: false);
				ghostTotalControl.gameObject.SetActiveRecursivelyCompat(state: false);
				SetupGerbariumData();
				break;
			case eStateGUI.showCutScene:
				if (musicSource != null)
				{
					musicSource.Pause();
				}
				HideBlockLine();
				HideBlockLocking(isHide: true);
				if (cutsceneScreenBlock != null)
				{
					GUtils.SetTransformX(base.transform, cutsceneScreenBlock.position.x);
				}
				cutsceneSceneFolder.gameObject.SetActiveRecursivelyCompat(state: true);
				CutSceneController.StartCutScene();
				backButton.gameObject.SetActive(false);
				if (grassButton != null)
				{
					grassButton.gameObject.SetActive(false);
				}
				almanacButton.gameObject.SetActive(false);
				starTotalControl.gameObject.SetActiveRecursivelyCompat(state: false);
				ghostTotalControl.gameObject.SetActiveRecursivelyCompat(state: false);
				if (introButton != null)
				{
					introButton.gameObject.SetActive(false);
				}
				break;
			case eStateGUI.options:
				MakeOptionVisible(isVisible: true);
				UpdateVolumeValue("MusicOptionValue", GameSettings.sound.musicVolume);
				UpdateVolumeValue("SoundOptionValue", GameSettings.sound.soundVolume);
				UpdateControlValue();
				UpdateDifficultValue();
				if (optionButton != null)
				{
					optionButton.gameObject.SetActive(true);
				}
				break;
			case eStateGUI.achievements:
				if (achViewBtn != null)
				{
					achViewBtn.gameObject.SetActive(true);
				}
				achViewBtn.gameObject.SetActiveRecursivelyCompat(state: false);
				MakeAchievementVisible(doVisible: true);
				break;
			case eStateGUI.buyMenu:
				if (buyMenu != null)
				{
					buyMenu.SetActiveRecursivelyCompat(state: true);
				}
				if (buyProducts != null)
				{
					for (int i = 0; i < buyProducts.Length; i++)
					{
						if (buyProducts[i] != null)
						{
							buyProducts[i].gameObject.SetActiveRecursivelyCompat(state: false);
						}
					}
				}
				if (buyFullVersion != null)
				{
					buyFullVersion.SetActiveRecursivelyCompat(state: false);
				}
				if (buyText != null)
				{
					buyText.text = "Connecting to store";
				}
				buyMenuReady = false;
				InAppPurshase.StartPurchase(isSilent: false);
				backButton.gameObject.SetActive(false);
				if (grassButton != null)
				{
					grassButton.gameObject.SetActive(false);
				}
				almanacButton.gameObject.SetActive(false);
				break;
			case eStateGUI.aboutMenu:
				if (aboutMenu != null)
				{
					aboutMenu.SetActiveRecursivelyCompat(state: true);
				}
				aboutButton.SetActive(true);
				break;
			case eStateGUI.socialPost:
				if (socialMode == 0)
				{
					if (socButtonFB != null)
					{
						socButtonFB.SetActive(true);
					}
					SocialNetPoster.LaunchFacebookPost();
				}
				break;
			}
			guiState = state;
		}
		stateChangeFromGUI = guiState;
	}

	private void MakeAchievementVisible(bool doVisible)
	{
		if (achViewTime < 0f)
		{
			achViewTime = achViewMoveTime;
		}
		else if (achViewPopup != doVisible)
		{
			achViewTime = Mathf.Clamp(achViewMoveTime - achViewTime, 0f, achViewMoveTime);
		}
		achViewPopup = doVisible;
		if (doVisible && !achViewTransform.gameObject.activeSelf)
		{
			achViewTransform.gameObject.SetActiveRecursivelyCompat(state: true);
		}
	}

	private void SetupGerbariumData()
	{
		grassData.mask = GameDataStorage.GetHerbariumMask();
		grassData.disableMask = GameDataStorage.GetHerbariumDisableMask();
		bool flag = false;
		bool flag2 = false;
		flag2 = (grassData.mask & 1) != 0;
		flag = (grassData.disableMask & 1) != 0;
		grassData.grass1.aniObj.gameObject.SetActiveRecursivelyCompat(flag2);
		if (flag2)
		{
			string text = ((!flag) ? "idleEnabled" : "idleDisabled");
			grassData.grass1.aniObj.Play(text);
			grassData.grass1.aniObj[text].normalizedTime = UnityEngine.Random.value;
		}
		flag2 = (grassData.mask & 4) != 0;
		flag = (grassData.disableMask & 4) != 0;
		grassData.grass2.aniObj.gameObject.SetActiveRecursivelyCompat(flag2);
		if (flag2)
		{
			string text = ((!flag) ? "idleEnabled" : "idleDisabled");
			grassData.grass2.aniObj.Play(text);
			grassData.grass2.aniObj[text].normalizedTime = UnityEngine.Random.value;
		}
		flag2 = (grassData.mask & 8) != 0;
		flag = (grassData.disableMask & 8) != 0;
		grassData.grass3.aniObj.gameObject.SetActiveRecursivelyCompat(flag2);
		if (flag2)
		{
			string text = ((!flag) ? "idleEnabled" : "idleDisabled");
			grassData.grass3.aniObj.Play(text);
			grassData.grass3.aniObj[text].normalizedTime = UnityEngine.Random.value;
		}
		flag2 = (grassData.mask & 0x10) != 0;
		flag = (grassData.disableMask & 0x10) != 0;
		grassData.grass4.aniObj.gameObject.SetActiveRecursivelyCompat(flag2);
		if (flag2)
		{
			string text = ((!flag) ? "idleEnabled" : "idleDisabled");
			grassData.grass4.aniObj.Play(text);
			grassData.grass4.aniObj[text].normalizedTime = UnityEngine.Random.value;
		}
		flag2 = (grassData.mask & 0x20) != 0;
		flag = (grassData.disableMask & 0x20) != 0;
		grassData.grass5.aniObj.gameObject.SetActiveRecursivelyCompat(flag2);
		if (flag2)
		{
			string text = ((!flag) ? "idleEnabled" : "idleDisabled");
			grassData.grass5.aniObj.Play(text);
			grassData.grass5.aniObj[text].normalizedTime = UnityEngine.Random.value;
		}
		flag2 = (grassData.mask & 0x40) != 0;
		flag = (grassData.disableMask & 0x40) != 0;
		grassData.grassSuper.aniObj.gameObject.SetActiveRecursivelyCompat(flag2);
		if (flag2)
		{
			string text = ((!flag) ? "idleEnabled" : "idleDisabled");
			grassData.grassSuper.aniObj.Play(text);
			grassData.grassSuper.aniObj[text].normalizedTime = UnityEngine.Random.value;
		}
	}

	private void MakeOptionVisible(bool isVisible)
	{
		Transform transform = mainScreenBlock.Find("OptionButtons");
		if (transform != null)
		{
			transform.gameObject.SetActiveRecursivelyCompat(isVisible);
		}
		transform = mainScreenBlock.Find("Title");
		if (transform != null)
		{
			transform.gameObject.SetActive(!isVisible);
		}
		TitleTurnOn(!isVisible);
	}

	public void LaunchLoading(string levelName, int launchFrameDelay)
	{
		if (guiState == eStateGUI.common)
		{
			SetGUIState(eStateGUI.loading);
			LaunchLoading(levelName);
		}
	}

	private void GotoLevelChoose()
	{
		int num = SetupBlockLine();
		if (num < 0)
		{
			num = 0;
		}
		SetCurrentBlock(num);
		if (blockList != null)
		{
			GUtils.SetTransformX(base.transform, blockList[currentBlock].position.x);
		}
		SetGUIState(eStateGUI.common);
	}

	private void ResetGameData()
	{
		int num = PlayerPrefs.GetInt("buyGems", 0);
		int num2 = PlayerPrefs.GetInt("unlockfull", 0);
		PlayerPrefs.DeleteAll();
		CrystalManager.ResetAchievements();
		GameDataStorage.LoadRecords(GameSettings.gameSettings.records);
		if (num > 0)
		{
			PlayerPrefs.SetInt("buyGems", num);
			GameDataStorage.SetCrystalSummary(num * GuiGameHud.ghostCost);
		}
		if (num2 != 0)
		{
			PlayerPrefs.SetInt("unlockfull", 1);
		}
		ReadAllLevelData();
		if (GameSettings.gameSettings != null)
		{
			GameSettings.gameSettings.LoadAchievements();
			UpdateAchievementList();
		}
	}

	public void TouchButton(string btnID)
	{
		if (modMsgBox != 0)
		{
			if (GameInput.IsBackPressed())
			{
				HideMessageBox();
				return;
			}
			switch (btnID)
			{
			case "msgYes":
				if (modMsgBox == 1)
				{
					if (modMsgBoxSub == 1)
					{
						ResetGameData();
					}
					else if (modMsgBoxSub == 2)
					{
						Application.Quit();
					}
				}
				HideMessageBox();
				lastMsgBoxTime = Time.timeSinceLevelLoad;
				break;
			case "msgNo":
				HideMessageBox();
				break;
			}
			return;
		}
		switch (btnID)
		{
		case "mainscreen":
			break;
		case "option":
			if (guiState == eStateGUI.titleScreen)
			{
				SetGUIState(eStateGUI.options);
			}
			else if (guiState == eStateGUI.options)
			{
				SetGUIState(eStateGUI.titleScreen);
			}
			break;
		case "SwitchMusic":
			GameSettings.sound.musicVolume = GetNextVolume(GameSettings.sound.musicVolume);
			if (musicSource != null)
			{
				musicSource.volume = GameSettings.sound.musicVolume;
			}
			UpdateVolumeValue("MusicOptionValue", GameSettings.sound.musicVolume);
			break;
		case "SwitchSound":
			GameSettings.sound.soundVolume = GetNextVolume(GameSettings.sound.soundVolume);
			UpdateVolumeValue("SoundOptionValue", GameSettings.sound.soundVolume);
			break;
		case "SwitchControls":
			switch (GameInput.inputStyle)
			{
			case eInputStyle.eisJoystick:
				GameInput.inputStyle = eInputStyle.eisAcceleration;
				break;
			case eInputStyle.eisAcceleration:
				GameInput.inputStyle = eInputStyle.eisAcceleration2;
				break;
			case eInputStyle.eisAcceleration2:
				GameInput.inputStyle = eInputStyle.eisButton;
				break;
			case eInputStyle.eisButton:
				if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
				{
					GameInput.inputStyle = eInputStyle.eisAcceleration;
				}
				else
				{
					GameInput.inputStyle = eInputStyle.eisKeyboard;
				}
				break;
			case eInputStyle.eisKeyboard:
				GameInput.inputStyle = eInputStyle.eisJoystick;
				break;
			default:
				GameInput.inputStyle = eInputStyle.eisButton;
				break;
			}
			UpdateControlValue();
			break;
		case "SwitchDifficult":
			GameSettings.gameSettings.gameParam.hardMode = !GameSettings.gameSettings.gameParam.hardMode;
			UpdateDifficultValue();
			break;
		case "ResetAll":
			if (Time.timeSinceLevelLoad - lastMsgBoxTime < 0f || Time.timeSinceLevelLoad - lastMsgBoxTime > 1f)
			{
				ShowMessageBox("确定要#重置全部#进度数据吗？", 1, 1);
			}
			break;
		case "back":
			SetGUIState(eStateGUI.titleScreen);
			break;
		case "backToLevelSelect":
			BackToLevelSelectBlock();
			break;
		case "grass":
			SetGUIState(eStateGUI.grassScreen);
			break;
		case "almanac":
			SetGUIState(eStateGUI.loading);
			LaunchLoading("Stone_Enemy_Level");
			break;
		case "Intro":
			SetGUIState(eStateGUI.showCutScene);
			break;
		case "crystalProfile":
			if (guiState == eStateGUI.titleScreen)
			{
				CrystalManager.ShowProfile(!CrystalManager.IsCrystalUIActive());
			}
			break;
		case "achievView":
			if (guiState == eStateGUI.titleScreen)
			{
				SetGUIState(eStateGUI.achievements);
			}
			else if (guiState == eStateGUI.achievements)
			{
				MakeAchievementVisible(!achViewPopup);
			}
			break;
		case "achUp":
		{
			TouchReactSlider component = achViewTransform.GetComponent<TouchReactSlider>();
			component.VScroll(isUp: true);
			break;
		}
		case "achDown":
		{
			TouchReactSlider component = achViewTransform.GetComponent<TouchReactSlider>();
			component.VScroll(isUp: false);
			break;
		}
		case "touchGrass1":
			TouchGrassIndex(0);
			break;
		case "touchGrass2":
			TouchGrassIndex(1);
			break;
		case "touchGrass3":
			TouchGrassIndex(2);
			break;
		case "touchGrass4":
			TouchGrassIndex(3);
			break;
		case "touchGrass5":
			TouchGrassIndex(4);
			break;
		case "touchGrassSuper":
			TouchGrassIndex(5);
			break;
		case "BayGhost":
			if (guiState == eStateGUI.buyMenu)
			{
				if (isWaitAsync)
				{
					WaitAsyncRequest(isWait: false);
				}
				SetGUIState(eStateGUI.common);
			}
			else
			{
				buyExitToCommon = true;
				SetGUIState(eStateGUI.buyMenu);
			}
			break;
		case "buyExit":
			SetGUIState((!buyExitToCommon) ? eStateGUI.titleScreen : eStateGUI.common);
			break;
		case "BuyFullVersion":
			if (guiState == eStateGUI.buyMenu && buyMenuReady)
			{
				AddBuyedGhost(3);
			}
			break;
		case "BayGhost_lo":
			if (guiState == eStateGUI.buyMenu && buyMenuReady)
			{
				AddBuyedGhost(0);
			}
			break;
		case "BayGhost_hi":
			if (guiState == eStateGUI.buyMenu && buyMenuReady)
			{
				AddBuyedGhost(2);
			}
			break;
		case "BayGhost_med":
			if (guiState == eStateGUI.buyMenu && buyMenuReady)
			{
				AddBuyedGhost(1);
			}
			break;
		case "about":
			if (guiState == eStateGUI.aboutMenu)
			{
				SetGUIState(eStateGUI.titleScreen);
			}
			else if (guiState == eStateGUI.titleScreen)
			{
				SetGUIState(eStateGUI.aboutMenu);
			}
			break;
		case "show_eula":
			HttpBrowser.OpenBrowser("http://tos.ea.com/legalapp/mobileeula/US/en/OTHER/");
			break;
		case "show_pp":
			HttpBrowser.OpenBrowser("http://tos.ea.com/legalapp/WEBPRIVACY/US/en/PC/");
			break;
		case "show_tos":
			HttpBrowser.OpenBrowser("http://tos.ea.com/legalapp/WEBTERMS/US/en/PC/");
			break;
		case "facebook":
			if (guiState == eStateGUI.socialPost && socialMode == 0)
			{
				SetGUIState(eStateGUI.titleScreen);
			}
			else if (guiState == eStateGUI.titleScreen)
			{
				socialMode = 0;
				SetGUIState(eStateGUI.socialPost);
			}
			break;
		case "gamecenter":
			if (guiState == eStateGUI.titleScreen)
			{
				CrystalManager.LaunchGCUI();
				ShowMessageBox("Connect to GameCenter", 3, -1);
			}
			break;
		case "mPlay":
			GotoLevelChoose();
			break;
		case "mStore":
			buyExitToCommon = false;
			SetGUIState(eStateGUI.buyMenu);
			break;
		case "mControls":
			SetGUIState(eStateGUI.options);
			break;
		case "controlsExit":
			SetGUIState(eStateGUI.titleScreen);
			break;
		case "mReset":
			SetGUIState(eStateGUI.titleScreen);
			break;
		case "aboutExit":
			SetGUIState(eStateGUI.titleScreen);
			break;
		}
	}

	private void WaitAsyncRequest(bool isWait)
	{
		isWaitAsync = isWait;
	}

	private void AddBuyedGhost(int addQ)
	{
		switch (addQ)
		{
		case 0:
		case 1:
		case 2:
		case 3:
			if (!InAppPurshase.RequestForBuy(addQ))
			{
				break;
			}
			buyIndex = addQ;
			WaitAsyncRequest(isWait: true);
			if (buyProducts != null)
			{
				for (int i = 0; i < buyProducts.Length; i++)
				{
					if (buyProducts[i] != null)
					{
						buyProducts[i].gameObject.SetActiveRecursivelyCompat(state: false);
					}
				}
			}
			if (buyFullVersion != null)
			{
				buyFullVersion.SetActiveRecursivelyCompat(state: false);
			}
			if (buyText != null)
			{
				buyText.text = "Purchase processing";
				buyText.gameObject.SetActive(true);
			}
			break;
		}
	}

	public static bool IsFreeVersion()
	{
		return false;
	}

	private void UpdateGhostCounter()
	{
		if (ghostTotalCounter != null)
		{
			ghostTotalCounter.text = "x 999";
		}
	}

	private void BackToLevelSelectBlock()
	{
		int num = currentBlock;
		SetupBlockLine();
		SetCurrentBlock(num);
		if (blockList != null)
		{
			GUtils.SetTransformX(base.transform, blockList[currentBlock].position.x);
		}
		SetGUIState(eStateGUI.common);
	}

	private void TouchGrassIndex(int grassIdx)
	{
		switch (grassIdx)
		{
		case 0:
			if ((grassData.mask & 1) != 0 && (grassData.disableMask ^ grassData.mask) != 1)
			{
				grassData.disableMask ^= 1;
				bool flag = (grassData.disableMask & 1) != 0;
				grassData.grass1.aniObj.Play((!flag) ? "toEnable" : "toDisable");
			}
			break;
		case 1:
			if ((grassData.mask & 4) != 0 && (grassData.disableMask ^ grassData.mask) != 4)
			{
				grassData.disableMask ^= 4;
				bool flag = (grassData.disableMask & 4) != 0;
				grassData.grass2.aniObj.Play((!flag) ? "toEnable" : "toDisable");
			}
			break;
		case 2:
			if ((grassData.mask & 8) != 0 && (grassData.disableMask ^ grassData.mask) != 8)
			{
				grassData.disableMask ^= 8;
				bool flag = (grassData.disableMask & 8) != 0;
				grassData.grass3.aniObj.Play((!flag) ? "toEnable" : "toDisable");
			}
			break;
		case 3:
			if ((grassData.mask & 0x10) != 0 && (grassData.disableMask ^ grassData.mask) != 16)
			{
				grassData.disableMask ^= 0x10;
				bool flag = (grassData.disableMask & 0x10) != 0;
				grassData.grass4.aniObj.Play((!flag) ? "toEnable" : "toDisable");
			}
			break;
		case 4:
			if ((grassData.mask & 0x20) != 0 && (grassData.disableMask ^ grassData.mask) != 32)
			{
				grassData.disableMask ^= 0x20;
				bool flag = (grassData.disableMask & 0x20) != 0;
				grassData.grass5.aniObj.Play((!flag) ? "toEnable" : "toDisable");
			}
			break;
		case 5:
			if ((grassData.mask & 0x40) != 0 && (grassData.disableMask ^ grassData.mask) != 64)
			{
				grassData.disableMask ^= 0x40;
				bool flag = (grassData.disableMask & 0x40) != 0;
				grassData.grassSuper.aniObj.Play((!flag) ? "toEnable" : "toDisable");
			}
			break;
		}
	}

	private float GetNextVolume(float volume)
	{
		if (volume < 0.1f)
		{
			return 0.3f;
		}
		if (volume > 0.9f)
		{
			return 0f;
		}
		return 1f;
	}

	private void UpdateVolumeValue(string optValueName, float volume)
	{
		Transform transform = mainScreenBlock.Find("OptionButtons/" + optValueName);
		if (transform != null)
		{
			TextMesh component = transform.GetComponent<TextMesh>();
			if (component != null)
			{
				component.text = ((volume < 0.1f) ? "Off" : ((!(volume > 0.9f)) ? "Quiet" : "Loud"));
			}
		}
	}

	private void UpdateControlValue()
	{
		Transform transform = mainScreenBlock.Find("OptionButtons/ControlsOptionValue");
		if (!(transform != null))
		{
			return;
		}
		TextMesh component = transform.GetComponent<TextMesh>();
		if (component != null)
		{
			switch (GameInput.inputStyle)
			{
			case eInputStyle.eisJoystick:
				component.text = "摇杆";
				break;
			case eInputStyle.eisAcceleration:
				component.text = "重力感应";
				break;
			case eInputStyle.eisAcceleration2:
				component.text = "重力感应2";
				break;
			case eInputStyle.eisButton:
				component.text = "按钮";
				break;
			case eInputStyle.eisKeyboard:
				component.text = "键盘";
				break;
			default:
				component.text = "按钮";
				break;
			}
		}
	}

	private void UpdateDifficultValue()
	{
		Transform transform = mainScreenBlock.Find("OptionButtons/DifficultValue");
		if (transform != null)
		{
			TextMesh component = transform.GetComponent<TextMesh>();
			if (component != null)
			{
				component.text = ((!GameSettings.gameSettings.gameParam.hardMode) ? "普通" : "困难");
			}
		}
	}

	private void SetCurrentBlock(int newBlockNum)
	{
		if (currentBlock != newBlockNum)
		{
			if (currentBlock >= 0 && currentBlock < blockLine.Length)
			{
				bool flag = IsBlockLocked(currentBlock);
				blockLine[currentBlock].openUnselected.SetActive(!flag);
				blockLine[currentBlock].openSelected.SetActive(false);
				blockLine[currentBlock].closeUnselected.SetActive(flag);
				blockLine[currentBlock].closeSelected.SetActive(false);
			}
			currentBlock = newBlockNum;
			if (currentBlock >= 0 && currentBlock < blockLine.Length)
			{
				bool flag2 = IsBlockLocked(currentBlock);
				blockLine[currentBlock].openUnselected.SetActive(false);
				blockLine[currentBlock].openSelected.SetActive(!flag2);
				blockLine[currentBlock].closeUnselected.SetActive(false);
				blockLine[currentBlock].closeSelected.SetActive(flag2);
			}
			SetupBlockLocking();
		}
	}

	private void ReadAllLevelData()
	{
		GameRecordsData records = GameSettings.gameSettings.records;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < records.blockData.Length; i++)
		{
			GameRecordsData.BlockData blockData = records.blockData[i];
			GameObject gameObject = GameObject.Find("BlockEnvironment_" + i);
			if (gameObject == null)
			{
				continue;
			}
			Transform transform = gameObject.transform.Find("Block Title");
			if (transform != null)
			{
				transform.GetComponent<TextMesh>().text = blockData.blockName;
			}
			for (int j = 0; j < blockData.level.Length; j++)
			{
				string text = "Level";
				text = ((i == 0 && j == 0) ? (text + "Tutorial") : ((i != 5 || j != 0) ? (text + " " + (j + 1)) : (text + "Boss")));
				Transform transform2 = gameObject.transform.Find(text);
				if (transform2 == null)
				{
					continue;
				}
				GameRecordsData.LevelData levelData = blockData.level[j];
				GameRecordsData.eLockStatus eLockStatus = levelData.lockStatus;
				if (IsBlockLocked(i))
				{
					eLockStatus = GameRecordsData.eLockStatus.locked;
				}
				num += levelData.stars;
				num2 += 3;
				TouchReactButton componentInChildren = transform2.gameObject.GetComponentInChildren<TouchReactButton>();
				MeshRenderer meshRenderer = ((!componentInChildren) ? null : componentInChildren.GetComponent<MeshRenderer>());
				Transform transform3 = transform2.transform.Find("LevelLabel");
				TextMesh textMesh = ((!transform3) ? null : transform3.GetComponent<TextMesh>());
				if (textMesh != null)
				{
					textMesh.text = levelData.name;
					if (textMesh.GetComponent<Renderer>() != null)
					{
						textMesh.GetComponent<Renderer>().material = ((eLockStatus != GameRecordsData.eLockStatus.unlock) ? materialLabelIncomplete : materialLabelComplete);
					}
				}
				transform3 = transform2.transform.Find("ScoreLabel");
				TextMesh textMesh2 = ((!transform3) ? null : transform3.GetComponent<TextMesh>());
				if (textMesh2 != null)
				{
					textMesh2.text = "Score: " + levelData.record;
				}
				Transform[] array = new Transform[3];
				Transform[] array2 = new Transform[3];
				for (int k = 0; k < array2.Length; k++)
				{
					array[k] = transform2.transform.Find("StarSlot" + (k + 1) + "_Empty");
					array2[k] = transform2.transform.Find("StarSlot" + (k + 1));
				}
				switch (eLockStatus)
				{
				case GameRecordsData.eLockStatus.locked:
				{
					if (componentInChildren != null)
					{
						componentInChildren.SetButtonState(TouchReactButton.eReactButtonState.disable);
					}
					if (meshRenderer != null)
					{
						meshRenderer.sharedMaterial = materialLevelBackPictureLock;
					}
					if (textMesh != null)
					{
						textMesh.gameObject.SetActive(false);
					}
					if (textMesh2 != null)
					{
						textMesh2.gameObject.SetActive(false);
					}
					for (int l = 0; l < array2.Length; l++)
					{
						if (array[l] != null)
						{
							array[l].gameObject.SetActive(false);
						}
						if (array2[l] != null)
						{
							array2[l].gameObject.SetActive(false);
						}
					}
					continue;
				}
				case GameRecordsData.eLockStatus.incomplete:
				{
					componentInChildren.SetButtonState(TouchReactButton.eReactButtonState.enable);
					meshRenderer.sharedMaterial = materialLevelBackPictureUncomplete;
					if (textMesh != null)
					{
						textMesh.gameObject.SetActive(true);
					}
					if (textMesh2 != null)
					{
						textMesh2.gameObject.SetActive(false);
					}
					for (int m = 0; m < array2.Length; m++)
					{
						if (array[m] != null)
						{
							array[m].gameObject.SetActive(false);
						}
						if (array2[m] != null)
						{
							array2[m].gameObject.SetActive(false);
						}
					}
					continue;
				}
				}
				componentInChildren.SetButtonState(TouchReactButton.eReactButtonState.enable);
				meshRenderer.sharedMaterial = materialLevelBackPictureUnlock;
				if (textMesh != null)
				{
					textMesh.gameObject.SetActive(true);
				}
				if (textMesh2 != null)
				{
					textMesh2.gameObject.SetActive(true);
				}
				for (int n = 0; n < array2.Length; n++)
				{
					if (array2[n] != null)
					{
						array2[n].gameObject.SetActive(n < levelData.stars);
					}
					if (array[n] != null)
					{
						array[n].gameObject.SetActive(n >= levelData.stars);
					}
				}
			}
		}
		starTotalCounter.text = "x " + num;
		UpdateGhostCounter();
	}
}
