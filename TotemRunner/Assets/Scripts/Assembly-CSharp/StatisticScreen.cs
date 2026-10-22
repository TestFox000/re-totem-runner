using System;
using UnityEngine;

public class StatisticScreen : GuiScreen
{
	public enum eStatisticState
	{
		disable = 0,
		finalizeLevel = 1,
		fadein = 2,
		showForm = 3,
		calculateScore = 4,
		showSummaryScore = 5,
		showStamps = 6,
		waitInput = 7,
		loading = 8
	}

	[Serializable]
	public class StatisticNodes
	{
		public Renderer backFillerRenderer;

		public Animation star1;

		public Animation starFx1;

		public Transform star1Empty;

		public Animation star2;

		public Animation starFx2;

		public Transform star2Empty;

		public Animation star3;

		public Animation starFx3;

		public Transform star3Empty;

		public Transform stampNewRecord;

		public Transform stampUnlockLevel;

		public Transform scoreLabelsFolder;

		public TextMesh scoreLabel;

		public TextMesh scoreValue;

		public TextMesh recordValue;

		public TextMesh scoreResult;

		public TextMesh[] starLimit = new TextMesh[3];

		public Transform levelFolder;

		public TextMesh levelLabel;

		public Transform buttonNextFern;

		public Transform buttonReplayFern;

		public Transform buttonMenuFern;
	}

	[Serializable]
	public class StatisticSounds
	{
		public AudioClip stampSound;

		public AudioClip scoreCounterSound;

		public float scoreCounterSoundVolume = 0.6f;

		public AudioClip starSound1;

		public AudioClip starSound2;

		public AudioClip starSound3;

		public float starSoundVolume = 1f;
	}

	private eStatisticState state;

	public float fadeinTime = 1f;

	public Vector2 buttonsSize = 128f * Vector2.one;

	public Texture2D buttonNextImage;

	public Texture2D buttonReplayImage;

	public Texture2D buttonMenuImage;

	public Texture2D buttonNextImageSelected;

	public Texture2D buttonReplayImageSelected;

	public Texture2D buttonMenuImageSelected;

	public string[] resultStrings = new string[4] { "poorly", "average", "great", "perfect" };

	public Animation endLevelFx;

	public StatisticNodes nodeList;

	public StatisticSounds sounds;

	public float stampInterval = 0.25f;

	public float stampAnimatedTime = 0.25f;

	public ECameraShakeType stampCamShake = ECameraShakeType.ground;

	public float fernScaleLength = 0.6f;

	public float nextFernScaleOffset = 0.8f;

	public float replayFernScaleOffset = 0.4f;

	public float menuFernScaleOffset;

	private float fernUpdateTime = -1f;

	private Rect rectButtonNext = new Rect(896f, 736f, 128f, 32f);

	private Rect rectButtonReplay = new Rect(448f, 736f, 128f, 32f);

	private Rect rectButtonMenu = new Rect(0f, 736f, 128f, 32f);

	public float scoreCounterTimePrev = 0.5f;

	public float scoreCounterTimeCalc = 0.6f;

	private float scoreCounterTimeCalcInv = 1.6666666f;

	public float soundTimeInterval = 0.2f;

	public float soundTimeIntervalIncrement = 0.1f;

	private float soundNextTime;

	private float soundNextInterval;

	public float scoreCounterTimePost = 1f;

	private float guiTimeFollow;

	private int guiFrameFollow;

	private int guiCounter;

	private int guiCScCommon;

	private int curShowStarIndex;

	private LevelCompleteData completeData;

	private int lvlIndexForLoading = -1;

	private bool waitForRate;

	private static StatisticScreen _singleton;

	public static StatisticScreen instance => _singleton;

	private void Awake()
	{
		_singleton = this;
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		if ((bool)endLevelFx)
		{
			endLevelFx.gameObject.SetActiveRecursivelyCompat(state: false);
		}
		base.enabled = false;
		if (scoreCounterTimeCalc > 0f)
		{
			scoreCounterTimeCalcInv = 1f / scoreCounterTimeCalc;
		}
		else
		{
			scoreCounterTimeCalcInv = 100f;
		}
		curShowStarIndex = 0;
	}

	private void Start()
	{
		Vector2 vector = buttonsSize;
		if (refCamera.pixelHeight != 768f)
		{
			vector.y *= refCamera.pixelHeight / 768f;
			vector.x *= refCamera.pixelHeight / 768f;
		}
		Vector3 vector2 = refCamera.WorldToScreenPoint(nodeList.buttonNextFern.position);
		Vector3 vector3 = refCamera.WorldToScreenPoint(nodeList.buttonReplayFern.position);
		Vector3 vector4 = refCamera.WorldToScreenPoint(nodeList.buttonMenuFern.position);
		vector2.y = refCamera.pixelHeight - vector2.y;
		vector3.y = refCamera.pixelHeight - vector3.y;
		vector4.y = refCamera.pixelHeight - vector4.y;
		rectButtonNext = new Rect(vector2.x - 0.5f * vector.x, vector2.y - 0.5f * vector.y, vector.x, vector.y);
		rectButtonReplay = new Rect(vector3.x - 0.5f * vector.x, vector3.y - 0.5f * vector.y, vector.x, vector.y);
		rectButtonMenu = new Rect(vector4.x - 0.5f * vector.x, vector4.y - 0.5f * vector.y, vector.x, vector.y);
		fernUpdateTime = -1f;
	}

	public void LaunchAfterTask()
	{
		ChangeState(eStatisticState.fadein);
	}

	private void ShowStar(int starIndex)
	{
		switch (starIndex)
		{
		case 1:
			nodeList.star1Empty.gameObject.SetActive(false);
			nodeList.starFx1.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.starFx1.Play();
			nodeList.star1.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.star1.Play();
			if ((bool)sounds.starSound1)
			{
				AudioSource.PlayClipAtPoint(sounds.starSound1, Camera.main.transform.position, sounds.starSoundVolume);
			}
			break;
		case 2:
			nodeList.star2Empty.gameObject.SetActive(false);
			nodeList.starFx2.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.starFx2.Play();
			nodeList.star2.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.star2.Play();
			if ((bool)sounds.starSound2)
			{
				AudioSource.PlayClipAtPoint(sounds.starSound2, Camera.main.transform.position, sounds.starSoundVolume);
			}
			break;
		case 3:
			nodeList.star3Empty.gameObject.SetActive(false);
			nodeList.starFx3.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.starFx3.Play();
			nodeList.star3.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.star3.Play();
			if ((bool)sounds.starSound3)
			{
				AudioSource.PlayClipAtPoint(sounds.starSound3, Camera.main.transform.position, sounds.starSoundVolume);
			}
			break;
		}
	}

	private void Update()
	{
		if (state == eStatisticState.disable)
		{
			return;
		}
		if (state == eStatisticState.finalizeLevel && (endLevelFx == null || !endLevelFx.isPlaying))
		{
			ChangeState(eStatisticState.fadein);
		}
		if (state > eStatisticState.showForm && !nodeList.buttonNextFern.gameObject.activeSelf)
		{
			bool flag = false;
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
			{
				flag = true;
			}
			if (flag)
			{
				StartButtonsShowing();
			}
		}
		if (GameInput.IsBackPressed())
		{
			waitForRate = IsNeedRate();
			if (waitForRate)
			{
				PluginRateApp.ShowRatePromt();
			}
			lvlIndexForLoading = 1;
			ChangeState(eStatisticState.loading);
		}
		switch (state)
		{
		case eStatisticState.fadein:
			guiTimeFollow += Time.deltaTime;
			if ((bool)nodeList.backFillerRenderer)
			{
				if (fadeinTime > 0f)
				{
					nodeList.backFillerRenderer.material.color = new Color(1f, 1f, 1f, Mathf.Clamp01(guiTimeFollow / fadeinTime));
				}
				else
				{
					nodeList.backFillerRenderer.material.color = Color.white;
				}
			}
			if (guiTimeFollow >= fadeinTime)
			{
				ChangeState(eStatisticState.showForm);
			}
			break;
		case eStatisticState.showForm:
			ChangeState(eStatisticState.calculateScore);
			break;
		case eStatisticState.calculateScore:
		{
			guiTimeFollow += Time.deltaTime;
			float num2 = 0f;
			num2 = ((!(guiTimeFollow >= scoreCounterTimePrev + scoreCounterTimeCalc + scoreCounterTimePost)) ? Mathf.Clamp01((guiTimeFollow - scoreCounterTimePrev) * scoreCounterTimeCalcInv) : 1f);
			if (guiTimeFollow >= soundNextTime)
			{
				AudioSource.PlayClipAtPoint(sounds.scoreCounterSound, Camera.main.transform.position, sounds.scoreCounterSoundVolume);
				soundNextTime += soundNextInterval;
				soundNextInterval += soundTimeIntervalIncrement;
			}
			int num3 = Mathf.FloorToInt(Mathf.Lerp(0f, guiCScCommon, num2));
			nodeList.scoreValue.text = string.Empty + num3;
			if (LevelAttributes.instance.scoreForStars != null && LevelAttributes.instance.scoreForStars.Length > curShowStarIndex && num3 >= LevelAttributes.instance.scoreForStars[curShowStarIndex])
			{
				ShowStar(++curShowStarIndex);
			}
			if (num2 >= 1f)
			{
				ChangeState(eStatisticState.showSummaryScore);
			}
			break;
		}
		case eStatisticState.showSummaryScore:
			ChangeState(eStatisticState.showStamps);
			break;
		case eStatisticState.showStamps:
			if (guiFrameFollow < 3)
			{
				guiTimeFollow -= Time.deltaTime;
				if (guiTimeFollow <= 0f)
				{
					if (guiFrameFollow == 0)
					{
						nodeList.stampNewRecord.localScale = new Vector3(128f, 128f, 1f);
						CameraShake.instance.LaunchShake(stampCamShake, 1f);
						AudioSource.PlayClipAtPoint(sounds.stampSound, Camera.main.transform.position, 1f);
						if (completeData.isUnlockBlock || completeData.isUnlockLevel)
						{
							guiFrameFollow = 1;
							guiTimeFollow = stampInterval;
						}
						else
						{
							guiFrameFollow = 3;
						}
					}
					else if (guiFrameFollow == 1)
					{
						nodeList.stampUnlockLevel.gameObject.SetActive(true);
						nodeList.stampUnlockLevel.localScale = new Vector3(384f, 384f, 1f);
						guiFrameFollow = 2;
						guiTimeFollow = stampAnimatedTime;
					}
					else
					{
						nodeList.stampUnlockLevel.localScale = new Vector3(128f, 128f, 1f);
						CameraShake.instance.LaunchShake(stampCamShake, 1f);
						AudioSource.PlayClipAtPoint(sounds.stampSound, Camera.main.transform.position, 1f);
						guiFrameFollow = 3;
					}
				}
				else
				{
					int num = Mathf.FloorToInt(Mathf.Clamp(128f + 256f * guiTimeFollow / stampAnimatedTime, 128f, 384f));
					if (guiFrameFollow == 0)
					{
						nodeList.stampNewRecord.localScale = new Vector3(num, num, 1f);
					}
					else if (guiFrameFollow == 2)
					{
						nodeList.stampUnlockLevel.localScale = new Vector3(num, num, 1f);
					}
				}
			}
			if (guiFrameFollow >= 3)
			{
				ChangeState(eStatisticState.waitInput);
			}
			break;
		case eStatisticState.loading:
			if (waitForRate)
			{
				waitForRate = PluginRateApp.IsRateProcess();
			}
			if (!waitForRate && lvlIndexForLoading >= 0)
			{
				if (PlayerPrefs.GetInt("rateDid", 0) != 0)
				{
					ChangeState(eStatisticState.waitInput);
					break;
				}
				BaseCamera.GLaunchLoading(lvlIndexForLoading);
				lvlIndexForLoading = -1;
			}
			break;
		}
		if (state >= eStatisticState.showForm && fernUpdateTime >= 0f)
		{
			fernUpdateTime += Time.deltaTime;
			float num4 = CalculateFernSizeByScaleTime((fernUpdateTime - nextFernScaleOffset) / fernScaleLength);
			nodeList.buttonNextFern.localScale = new Vector3(num4, num4, 1f);
			num4 = CalculateFernSizeByScaleTime((fernUpdateTime - replayFernScaleOffset) / fernScaleLength);
			nodeList.buttonReplayFern.localScale = new Vector3(num4, num4, 1f);
			num4 = CalculateFernSizeByScaleTime((fernUpdateTime - menuFernScaleOffset) / fernScaleLength);
			nodeList.buttonMenuFern.localScale = new Vector3(0f - num4, num4, 1f);
			if (fernUpdateTime > nextFernScaleOffset + fernScaleLength)
			{
				fernUpdateTime = -1f;
			}
		}
	}

	private int CalculateFernSizeByScaleTime(float timeRel)
	{
		int num = 128;
		int num2 = 320;
		int num3 = 256;
		if (timeRel < 0f)
		{
			return 0;
		}
		if (timeRel <= 0f)
		{
			return num;
		}
		if (timeRel >= 1f)
		{
			return num3;
		}
		if (timeRel < 0.5f)
		{
			return num + Mathf.RoundToInt((float)(num2 - num) * 2f * timeRel);
		}
		return num3 + Mathf.FloorToInt((float)(num2 - num3) * 2f * (1f - timeRel));
	}

	public override void UpdateInGUI()
	{
		if (state == eStatisticState.loading || !nodeList.buttonNextFern.gameObject.activeSelf)
		{
			return;
		}
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.normal.background = buttonNextImage;
		gUIStyle.active.background = buttonNextImageSelected;
		Texture image = null;
		if (GUI.Button(rectButtonNext, image, gUIStyle))
		{
			waitForRate = IsNeedRate();
			if (waitForRate)
			{
				PluginRateApp.ShowRatePromt();
			}
			lvlIndexForLoading = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
			if (GameSettings.gameSettings.IsLevelLock(lvlIndexForLoading))
			{
				lvlIndexForLoading = 1;
			}
			ChangeState(eStatisticState.loading);
		}
		gUIStyle.normal.background = buttonReplayImage;
		gUIStyle.active.background = buttonReplayImageSelected;
		if (GUI.Button(rectButtonReplay, image, gUIStyle))
		{
			waitForRate = IsNeedRate();
			if (waitForRate)
			{
				PluginRateApp.ShowRatePromt();
			}
			lvlIndexForLoading = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
			ChangeState(eStatisticState.loading);
		}
		gUIStyle.normal.background = buttonMenuImage;
		gUIStyle.active.background = buttonMenuImageSelected;
		if (GUI.Button(rectButtonMenu, image, gUIStyle))
		{
			waitForRate = IsNeedRate();
			if (waitForRate)
			{
				PluginRateApp.ShowRatePromt();
			}
			lvlIndexForLoading = 1;
			ChangeState(eStatisticState.loading);
		}
	}

	private bool IsNeedRate()
	{
		if (PlayerPrefs.GetInt("rateDecline", 0) != 0)
		{
			return false;
		}
		if (PlayerPrefs.GetInt("rateDid", 0) != 0)
		{
			return false;
		}
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex < 6)
		{
			return false;
		}
		if (ScoreManager.GetStarQuantity() < 3 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 12)
		{
			return false;
		}
		int num = PlayerPrefs.GetInt("rateLater", 0);
		if (num > 0)
		{
			num--;
			PlayerPrefs.SetInt("rateLater", num);
			if (num > 0)
			{
				return false;
			}
		}
		return true;
	}

	public override void StartShow(Camera cam)
	{
		base.StartShow(cam);
		lvlIndexForLoading = -1;
		TimeController.StartPause(isPause: false);
		PlayerController.instance.DisableController(disable: true);
		ChangeState(eStatisticState.finalizeLevel);
	}

	public static int CalculateCommonScores()
	{
		int num = 0;
		GameRecordsData.BlockData[] blockData = GameSettings.gameSettings.records.blockData;
		foreach (GameRecordsData.BlockData blockData2 in blockData)
		{
			GameRecordsData.LevelData[] level = blockData2.level;
			foreach (GameRecordsData.LevelData levelData in level)
			{
				num += levelData.record;
			}
		}
		return num;
	}

	private void ChangeState(eStatisticState newState)
	{
		if (state == newState)
		{
			return;
		}
		switch (newState)
		{
		case eStatisticState.disable:
			base.gameObject.SetActiveRecursivelyCompat(state: false);
			base.enabled = false;
			break;
		case eStatisticState.finalizeLevel:
			base.enabled = true;
			base.gameObject.SetActive(true);
			if ((bool)endLevelFx)
			{
				endLevelFx.gameObject.SetActiveRecursivelyCompat(state: true);
				endLevelFx.Play();
			}
			break;
		case eStatisticState.fadein:
			if ((bool)nodeList.backFillerRenderer)
			{
				nodeList.backFillerRenderer.gameObject.SetActive(true);
				nodeList.backFillerRenderer.material.color = new Color(1f, 1f, 1f, 0f);
			}
			guiTimeFollow = 0f;
			GuiGameHud.instance.scoreCounter.gameObject.SetActiveRecursivelyCompat(state: false);
			break;
		case eStatisticState.showForm:
		{
			base.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.buttonNextFern.gameObject.SetActive(false);
			nodeList.buttonNextFern.localScale = new Vector3(0f, 0f, 1f);
			nodeList.buttonReplayFern.gameObject.SetActive(false);
			nodeList.buttonReplayFern.localScale = new Vector3(0f, 0f, 1f);
			nodeList.buttonMenuFern.gameObject.SetActive(false);
			nodeList.buttonMenuFern.localScale = new Vector3(0f, 0f, 1f);
			completeData = GameSettings.GetLevelCompleteData();
			nodeList.star1.gameObject.SetActiveRecursivelyCompat(state: false);
			nodeList.starFx1.gameObject.SetActiveRecursivelyCompat(state: false);
			nodeList.star1Empty.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.star2.gameObject.SetActiveRecursivelyCompat(state: false);
			nodeList.starFx2.gameObject.SetActiveRecursivelyCompat(state: false);
			nodeList.star2Empty.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.star3.gameObject.SetActiveRecursivelyCompat(state: false);
			nodeList.starFx3.gameObject.SetActiveRecursivelyCompat(state: false);
			nodeList.star3Empty.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.levelFolder.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.scoreLabelsFolder.gameObject.SetActiveRecursivelyCompat(state: false);
			nodeList.stampNewRecord.gameObject.SetActive(false);
			nodeList.stampUnlockLevel.gameObject.SetActive(false);
			nodeList.scoreValue.text = "0";
			nodeList.recordValue.text = string.Empty + completeData.prevScore;
			nodeList.levelLabel.text = completeData.levelName;
			for (int i = 0; i < 3; i++)
			{
				nodeList.starLimit[i].text = string.Empty + LevelAttributes.instance.scoreForStars[i];
			}
			SaveCompleteData();
			break;
		}
		case eStatisticState.calculateScore:
			curShowStarIndex = 0;
			nodeList.scoreLabelsFolder.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.scoreResult.gameObject.SetActiveRecursivelyCompat(state: false);
			guiCScCommon = ScoreManager.CalculatePartOfScore(ScoreManager.eScorePart.allComplete);
			guiTimeFollow = 0f;
			soundNextTime = scoreCounterTimePrev;
			soundNextInterval = soundTimeInterval;
			break;
		case eStatisticState.showSummaryScore:
			nodeList.scoreResult.text = GetCompleteResultString();
			nodeList.scoreResult.gameObject.SetActiveRecursivelyCompat(state: true);
			break;
		case eStatisticState.showStamps:
			if (guiCounter > completeData.prevScore)
			{
				completeData.isNewRecord = true;
				nodeList.recordValue.text = string.Empty + guiCounter;
			}
			else
			{
				completeData.isNewRecord = false;
			}
			guiFrameFollow = 3;
			guiTimeFollow = 0f;
			if (completeData.isNewRecord)
			{
				nodeList.stampNewRecord.gameObject.SetActive(true);
				nodeList.stampNewRecord.localScale = new Vector3(384f, 384f, 1f);
				guiFrameFollow = 0;
				guiTimeFollow = stampAnimatedTime;
			}
			else if (completeData.isUnlockBlock || completeData.isUnlockLevel)
			{
				nodeList.stampUnlockLevel.gameObject.SetActive(true);
				nodeList.stampUnlockLevel.localScale = new Vector3(768f, 384f, 1f);
				guiFrameFollow = 2;
				guiTimeFollow = stampAnimatedTime;
			}
			break;
		case eStatisticState.waitInput:
			StartButtonsShowing();
			break;
		case eStatisticState.loading:
			if (!waitForRate && lvlIndexForLoading >= 0)
			{
				BaseCamera.GLaunchLoading(lvlIndexForLoading);
				lvlIndexForLoading = -1;
			}
			break;
		}
		state = newState;
	}

	public void SaveCompleteData()
	{
		guiCounter = ScoreManager.CalculatePartOfScore(ScoreManager.eScorePart.allComplete);
		if (guiCounter > completeData.prevScore)
		{
			LevelCompleteEvent.curLevelData.record = guiCounter;
			GameDataStorage.SaveRecords(GameSettings.gameSettings.records);
			CrystalManager.SetLeaderboardValue(CrystalManager.eLeaderBoardID.common, CalculateCommonScores());
		}
		GameStatistic.instance.SaveInStorage();
		AchievementCollector.CompleteLevelCheck();
	}

	private void StartButtonsShowing()
	{
		if (!nodeList.buttonNextFern.gameObject.activeSelf)
		{
			fernUpdateTime = 0f;
			nodeList.buttonNextFern.gameObject.SetActive(true);
			nodeList.buttonReplayFern.gameObject.SetActive(true);
			nodeList.buttonMenuFern.gameObject.SetActive(true);
		}
	}

	private string GetCompleteResultString()
	{
		if (resultStrings == null || resultStrings.Length == 0)
		{
			return "Unknown";
		}
		int num = ScoreManager.GetStarQuantity();
		if (num >= resultStrings.Length)
		{
			num = resultStrings.Length - 1;
		}
		return resultStrings[num];
	}

	public static string SecsToTimeString(int secs)
	{
		string text = string.Empty;
		int num = secs / 60;
		if (num < 10)
		{
			text += "0";
		}
		text = text + num + " : ";
		int num2 = secs - num * 60;
		if (num2 < 10)
		{
			text += "0";
		}
		return text + num2;
	}
}
