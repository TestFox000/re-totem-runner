using UnityEngine;

public class SeedChecker : MonoBehaviour
{
	public enum eSeedType
	{
		poppy = 0,
		lilac = 1,
		tulip = 2,
		fungus = 3
	}

	public int blockNum;

	public int levelNum;

	public eSeedType seedType;

	private void Start()
	{
		if (!IsSeedVisible())
		{
			base.gameObject.SetActiveRecursivelyCompat(state: false);
		}
	}

	private bool IsSeedVisible()
	{
		if (GameSettings.gameSettings == null)
		{
			return false;
		}
		if (blockNum < 0 || blockNum >= GameSettings.gameSettings.records.blockData.Length)
		{
			return false;
		}
		if (GameSettings.gameSettings.records.blockData[blockNum].isLock)
		{
			return false;
		}
		if (levelNum < 0 || levelNum >= GameSettings.gameSettings.records.blockData[blockNum].level.Length)
		{
			return false;
		}
		if (GameSettings.gameSettings.records.blockData[blockNum].level[levelNum].lockStatus != GameRecordsData.eLockStatus.unlock)
		{
			return false;
		}
		int herbariumMask = GameDataStorage.GetHerbariumMask();
		bool flag = true;
		switch (seedType)
		{
		case eSeedType.poppy:
			flag = (herbariumMask & 4) != 0;
			break;
		case eSeedType.lilac:
			flag = (herbariumMask & 8) != 0;
			break;
		case eSeedType.tulip:
			flag = (herbariumMask & 0x10) != 0;
			break;
		case eSeedType.fungus:
			flag = (herbariumMask & 0x20) != 0;
			break;
		}
		return !flag;
	}
}
