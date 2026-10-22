using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
	[Serializable]
	public class DangerParameters
	{
		public EnemyArcherParams archer;

		public EnemyFatmanParams fatman;

		public EnemyBrainParams brain;

		public EnemySpiderParams spider;

		public DangerObstacleParams obstacle;

		public DangerBarrierParams barrier;
	}

	[Serializable]
	public class ScoreItem
	{
		public int berryScore = 2;

		public int seedScore = 50;

		public int gemScore = 30;
	}

	[Serializable]
	public class LevelParameters
	{
		public float killZoneY = -400f;
	}

	[Serializable]
	public class SoundParameters
	{
		public float musicVolume = 1f;

		public float soundVolume = 1f;

		public SoundParameters(float musicVol, float soundVol)
		{
			musicVolume = musicVol;
			soundVolume = soundVol;
		}
	}

	public class SpawnPointRestoreData
	{
		public int grassMask;

		public int skinMask = 7;

		public bool manIsDying;
	}

	[Serializable]
	public class GameParameters
	{
		public bool hardMode;
	}

	public DangerParameters dangers;

	public ScoreItem scoreItem = new ScoreItem();

	public LevelParameters level;

	public static SoundParameters sound = new SoundParameters(1f, 1f);

	public GameRecordsData records;

	public AchievementSystem.AchievementData[] achievements = new AchievementSystem.AchievementData[45]
	{
		new AchievementSystem.AchievementData("Boost Master", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Harvester", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Collector", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Gems Master", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Ruiner", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Immortal", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Stomper", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Hammer", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Destroyer", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Summoner", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Golem Hunter", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Healer", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Dodger", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Block I", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Block II", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Block III", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Block IV", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Mastery", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Ascetic", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Herbalist", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Achievement 21", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Achievement 22", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Achievement 23", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Achievement 24", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Achievement 25", AchievementSystem.eAchievementType.achievement),
		new AchievementSystem.AchievementData("Kulaba", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Sitara", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Shikari Urdu", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Vizarda Makari", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Amenti Kara", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Bama Kara", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Urana Bama", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Hadda", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Burai Makari", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Atan Urdu", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Evil Spirit", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 12", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 13", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 14", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 15", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 16", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 17", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 18", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 19", AchievementSystem.eAchievementType.encounter),
		new AchievementSystem.AchievementData("Encounter 20", AchievementSystem.eAchievementType.encounter)
	};

	public AudioClip lvlCompleteSound;

	private LevelCompleteData levelCompleteData = new LevelCompleteData();

	private SpawnPointRestoreData spawPointRestoreData = new SpawnPointRestoreData();

	public GameParameters gameParam;

	public static GameSettings gameSettings;

	private float realTimeOld;

	public static float realDeltaTime = 0f;

	public static void InitForEditor()
	{
		if (!gameSettings)
		{
			gameSettings = UnityEngine.Object.FindObjectOfType(typeof(GameSettings)) as GameSettings;
			if (!gameSettings)
			{
				Debug.LogWarning("Warning! Scene is not contained script object GameSettings");
			}
		}
	}

	private void Awake()
	{
		Screen.autorotateToLandscapeLeft = false;
		Screen.autorotateToLandscapeRight = false;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		gameSettings = this;
		GameDataStorage.LoadRecords(records);
		GameDataStorage.LoadGameSettings();
		levelCompleteData.isStopTimer = false;
		levelCompleteData.timer = 0f;
		realTimeOld = Time.realtimeSinceStartup;
		LoadAchievements();
	}

	public void LoadAchievements()
	{
		int num = 0;
		int num2 = achievements.Length;
		int num3 = 0;
		while (num < num2)
		{
			int num4 = PlayerPrefs.GetInt("ach_" + num3, 0);
			for (int i = 0; i < 15; i++)
			{
				if (num >= num2)
				{
					break;
				}
				achievements[num].isReach = (num4 & 1) != 0;
				num++;
				num4 /= 2;
			}
			num3++;
		}
	}

	private void Start()
	{
		CalculateLevelCompleteData(GetCurrentLevelData());
		levelCompleteData.isStopTimer = false;
		SaveRestoreData();
		GuiGameHud.UpdateCrystalProgress();
	}

	private void Update()
	{
		realDeltaTime = Mathf.Clamp(Time.realtimeSinceStartup - realTimeOld, 0f, 0.2f);
		realTimeOld = Time.realtimeSinceStartup;
		if (!levelCompleteData.isStopTimer)
		{
			levelCompleteData.timer += Time.deltaTime;
		}
	}

	public bool IsLevelLock(int lvlNum)
	{
		int blockIndex = GetBlockIndex(lvlNum);
		if (blockIndex < 0 || blockIndex >= records.blockData.Length)
		{
			return true;
		}
		if (records.blockData[blockIndex].isLock)
		{
			return true;
		}
		int levelIndex = GetLevelIndex(lvlNum);
		if (levelIndex < 0 || levelIndex >= records.blockData[blockIndex].level.Length)
		{
			return true;
		}
		if (records.blockData[blockIndex].level[levelIndex].lockStatus == GameRecordsData.eLockStatus.locked)
		{
			return true;
		}
		if (ChooseLevelGUI.IsFreeVersion() && blockIndex > 1)
		{
			return true;
		}
		return false;
	}

	public static int GetBlockIndex(int lvlNum)
	{
		if (lvlNum < 3)
		{
			return -1;
		}
		return lvlNum / 4;
	}

	public static int GetLevelIndex(int lvlNum)
	{
		int blockIndex = GetBlockIndex(lvlNum);
		if (blockIndex < 0)
		{
			return 0;
		}
		if (blockIndex == 0)
		{
			return 0;
		}
		return lvlNum - blockIndex * 4;
	}

	public GameRecordsData.LevelData GetCurrentLevelData()
	{
		int blockIndex = GetBlockIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		if (blockIndex < 0)
		{
			return null;
		}
		int levelIndex = GetLevelIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		if (blockIndex >= records.blockData.Length || levelIndex >= records.blockData[blockIndex].level.Length)
		{
			return null;
		}
		return records.blockData[blockIndex].level[levelIndex];
	}

	public void CalculateLevelCompleteData(GameRecordsData.LevelData levelData)
	{
		levelCompleteData.score = 0;
		levelCompleteData.isNewRecord = false;
		levelCompleteData.prevScore = 0;
		levelCompleteData.masterRecord = 0;
		levelCompleteData.levelName = "Unknown";
		levelCompleteData.isStopTimer = false;
		if (levelData != null)
		{
			levelCompleteData.prevScore = levelData.record;
			levelCompleteData.masterRecord = levelData.masterRecord;
			levelCompleteData.levelName = levelData.name;
			levelCompleteData.isStopTimer = true;
			levelCompleteData.score = ScoreManager.CalculatePartOfScore(ScoreManager.eScorePart.allComplete);
			if (levelData.record < levelCompleteData.score)
			{
				levelData.record = levelCompleteData.score;
			}
			int starQuantity = ScoreManager.GetStarQuantity();
			if (levelData.stars < starQuantity)
			{
				levelData.stars = starQuantity;
			}
		}
	}

	public static void SaveRestoreData()
	{
		GameStatistic.instance.SaveOnCheckpoint();
		int num = 0;
		int num2 = 1;
		if (GrassGifManager.instance != null)
		{
			GrassGifManager instance = GrassGifManager.instance;
			for (int i = 0; i < instance.herbals.Length; i++)
			{
				if (instance.herbals[i].isTaken)
				{
					num += num2;
				}
				num2 *= 2;
			}
		}
		gameSettings.spawPointRestoreData.grassMask = num;
		if (!LevelAttributes.IsInstalled)
		{
			return;
		}
		bool[] skinEnabled = LevelAttributes.instance.skinEnabled;
		int num3 = 1;
		int num4 = 0;
		for (int j = 0; j < skinEnabled.Length; j++)
		{
			if (skinEnabled[j])
			{
				num4 += num3;
			}
			num3 *= 2;
		}
		gameSettings.spawPointRestoreData.skinMask = num4;
		gameSettings.spawPointRestoreData.manIsDying = LevelAttributes.instance.isManDying;
	}

	public static void LoadRestoreData()
	{
		GameStatistic.instance.LoadFromCheckpoint();
		int grassMask = gameSettings.spawPointRestoreData.grassMask;
		int num = 1;
		GrassGifManager instance = GrassGifManager.instance;
		if (instance != null)
		{
			for (int i = 0; i < instance.herbals.Length; i++)
			{
				if (instance.herbals[i].isTaken && (grassMask & num) == 0)
				{
					instance.DeleteHerbal(i);
				}
				num *= 2;
			}
		}
		GrassGifManager.ChooseRandomHerbal();
		if (LevelAttributes.instance != null)
		{
			int skinMask = gameSettings.spawPointRestoreData.skinMask;
			int num2 = 1;
			for (int j = 0; j < LevelAttributes.instance.skinEnabled.Length; j++)
			{
				LevelAttributes.instance.skinEnabled[j] = (skinMask & num2) != 0;
				num2 *= 2;
			}
			LevelAttributes.instance.isManDying = gameSettings.spawPointRestoreData.manIsDying;
		}
		GuiGameHud.UpdateCrystalProgress();
	}

	public static LevelCompleteData GetLevelCompleteData()
	{
		return gameSettings.levelCompleteData;
	}
}
