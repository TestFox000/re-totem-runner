using UnityEngine;

public class GameDataStorage : MonoBehaviour
{
	public static void SaveGameSettings()
	{
		PlayerPrefs.SetFloat("MusicVolume", GameSettings.sound.musicVolume);
		PlayerPrefs.SetFloat("SoundVolume", GameSettings.sound.soundVolume);
		PlayerPrefs.SetInt("InputStyle", (int)GameInput.inputStyle);
		PlayerPrefs.SetInt("HardMode", GameSettings.gameSettings.gameParam.hardMode ? 1 : 0);
	}

	public static void LoadGameSettings()
	{
		GameSettings.sound.musicVolume = PlayerPrefs.GetFloat("MusicVolume", GameSettings.sound.musicVolume);
		GameSettings.sound.soundVolume = PlayerPrefs.GetFloat("SoundVolume", GameSettings.sound.soundVolume);
		GameInput.inputStyle = (eInputStyle)PlayerPrefs.GetInt("InputStyle", (int)GameInput.inputStyle);
		GameSettings.gameSettings.gameParam.hardMode = PlayerPrefs.GetInt("HardMode", 0) != 0;
	}

	public static void SaveRecords(GameRecordsData records)
	{
		for (int i = 0; i < records.blockData.Length; i++)
		{
			GameRecordsData.BlockData blockData = records.blockData[i];
			string text = "block" + i;
			PlayerPrefs.SetInt(text + "_lock", blockData.isLock ? 1 : 0);
			for (int j = 0; j < blockData.level.Length; j++)
			{
				GameRecordsData.LevelData levelData = blockData.level[j];
				string text2 = "level" + i + "_" + j;
				PlayerPrefs.SetInt(text2 + "_lock", (int)levelData.lockStatus);
				PlayerPrefs.SetInt(text2 + "_stars", levelData.stars);
				PlayerPrefs.SetInt(text2 + "_record", levelData.record);
			}
		}
	}

	public static void LoadRecords(GameRecordsData records)
	{
		for (int i = 0; i < records.blockData.Length; i++)
		{
			GameRecordsData.BlockData blockData = records.blockData[i];
			string text = "block" + i;
			blockData.isLock = false;
			for (int j = 0; j < blockData.level.Length; j++)
			{
				GameRecordsData.LevelData levelData = blockData.level[j];
				string text2 = "level" + i + "_" + j;
				GameRecordsData.eLockStatus defaultValue2 = GameRecordsData.eLockStatus.locked;
				if (j == 0 && !blockData.isLock)
				{
					defaultValue2 = GameRecordsData.eLockStatus.incomplete;
				}
				levelData.lockStatus = (GameRecordsData.eLockStatus)PlayerPrefs.GetInt(text2 + "_lock", (int)defaultValue2);
				if (levelData.lockStatus == GameRecordsData.eLockStatus.locked)
				{
					levelData.lockStatus = GameRecordsData.eLockStatus.incomplete;
				}
				levelData.stars = PlayerPrefs.GetInt(text2 + "_stars", 0);
				levelData.record = PlayerPrefs.GetInt(text2 + "_record", 0);
			}
		}
	}

	public static void SaveGrassData()
	{
		int num = 1;
		int num2 = 0;
		GrassGifManager instance = GrassGifManager.instance;
		int num3 = 1;
		for (int i = 0; i < instance.herbals.Length; i++)
		{
			if (instance.herbals[i].isTaken)
			{
				num |= num3;
			}
			if (instance.herbals[i].isDisable)
			{
				num2 |= num3;
			}
			num3 *= 2;
		}
		PlayerPrefs.SetInt("HerbariumMask", num);
		PlayerPrefs.SetInt("HerbariumDisableMask", num2);
	}

	public static void LoadGrassData()
	{
		int herbariumMask = GetHerbariumMask();
		int herbariumDisableMask = GetHerbariumDisableMask();
		GrassGifManager instance = GrassGifManager.instance;
		int num = 1;
		for (int i = 0; i < instance.herbals.Length; i++)
		{
			if ((herbariumMask & num) != 0)
			{
				instance.herbals[i].isTaken = true;
			}
			instance.herbals[i].isDisable = (herbariumDisableMask & num) != 0;
			num *= 2;
		}
	}

	public static int GetHerbariumMask()
	{
		return PlayerPrefs.GetInt("HerbariumMask", 1);
	}

	public static int GetHerbariumDisableMask()
	{
		return PlayerPrefs.GetInt("HerbariumDisableMask", 0);
	}

	public static void SetHerbariumDisableMask(int mask)
	{
		PlayerPrefs.SetInt("HerbariumDisableMask", mask);
	}

	public static int GetOldHerbariumMask()
	{
		return PlayerPrefs.GetInt("OldHerbariumMask", 0);
	}

	public static void SetOldHerbariumMask(int mask)
	{
		PlayerPrefs.SetInt("OldHerbariumMask", mask);
	}

	public static int GetCrystalSummary()
	{
		return PlayerPrefs.GetInt("CrystalSummary", 0);
	}

	public static void SetCrystalSummary(int val)
	{
		PlayerPrefs.SetInt("CrystalSummary", val);
	}
}
