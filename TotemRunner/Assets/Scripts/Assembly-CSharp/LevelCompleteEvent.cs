using UnityEngine;

public class LevelCompleteEvent : MonoBehaviour
{
	private static LevelCompleteEvent _singleton;

	public static GameRecordsData.LevelData curLevelData;

	private bool isWaitAni;

	public Animation completeFx;

	public string completeFxAnimName = string.Empty;

	private void Awake()
	{
		base.enabled = false;
		_singleton = this;
		isWaitAni = false;
	}

	private void Update()
	{
		if (isWaitAni && (completeFx == null || !completeFx.isPlaying))
		{
			isWaitAni = false;
			EndLevel();
		}
	}

	public static void Launch()
	{
		if (_singleton == null || _singleton.enabled)
		{
			return;
		}
		_singleton.enabled = true;
		PlayerController.instance.DisableController(disable: true);
		ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.grassGrowing);
		ScrollingCamera.instance.FreezeCamera(freezed: true);
		PlayerAnimation.instance.StopCurrent();
		if (_singleton.completeFx != null)
		{
			_singleton.isWaitAni = true;
			if (_singleton.completeFxAnimName != string.Empty)
			{
				_singleton.completeFx.Play(_singleton.completeFxAnimName);
			}
		}
		else
		{
			_singleton.EndLevel();
		}
	}

	public void EndLevel()
	{
		PlayerSoundList.StopAllSounds();
		if ((bool)GameSettings.gameSettings && (bool)GameSettings.gameSettings.lvlCompleteSound)
		{
			SoundEnvironment.Launch2DSound(GameSettings.gameSettings.lvlCompleteSound, 1f, 0f, -1f);
		}
		int levelIndex = GameSettings.GetLevelIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		int blockIndex = GameSettings.GetBlockIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		if (blockIndex < 0)
		{
			return;
		}
		GameRecordsData records = GameSettings.gameSettings.records;
		curLevelData = records.blockData[blockIndex].level[levelIndex];
		curLevelData.lockStatus = GameRecordsData.eLockStatus.unlock;
		int num = blockIndex;
		int num2 = levelIndex + 1;
		if (num2 >= records.blockData[num].level.Length)
		{
			num2 = 0;
			num++;
		}
		GameSettings.gameSettings.CalculateLevelCompleteData(curLevelData);
		int num3 = 0;
		for (int i = 0; i < records.blockData.Length; i++)
		{
			for (int j = 0; j < records.blockData[i].level.Length; j++)
			{
				num3 += records.blockData[i].level[j].stars;
			}
		}
		GameSettings.GetLevelCompleteData().allStarQuantity = num3;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < records.blockData.Length; i++)
		{
			if (records.blockData[i].isLock && records.blockData[i].starsToUnlock <= num3)
			{
				flag2 = true;
				records.blockData[i].isLock = false;
				if (records.blockData[i].level[0].lockStatus == GameRecordsData.eLockStatus.locked)
				{
					flag = true;
					records.blockData[i].level[0].lockStatus = GameRecordsData.eLockStatus.incomplete;
				}
			}
		}
		if (num < records.blockData.Length && !records.blockData[num].isLock && records.blockData[num].level[num2].lockStatus == GameRecordsData.eLockStatus.locked)
		{
			flag = true;
			records.blockData[num].level[num2].lockStatus = GameRecordsData.eLockStatus.incomplete;
		}
		if (flag2)
		{
			GameSettings.GetLevelCompleteData().isUnlockBlock = true;
		}
		if (flag)
		{
			GameSettings.GetLevelCompleteData().isUnlockLevel = true;
		}
		GameDataStorage.SaveGrassData();
		GameDataStorage.SaveRecords(records);
		GuiGameHud instance = GuiGameHud.instance;
		instance.SetGuiState(GuiGameHud.eGuiGameHudState.levelComplete);
		AnalyticsManager.LogEventWithParam2("completeLevel", "level", curLevelData.name, "time", StatisticScreen.SecsToTimeString(Mathf.FloorToInt(GameSettings.GetLevelCompleteData().timer)));
	}
}
