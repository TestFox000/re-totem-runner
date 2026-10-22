using UnityEngine;

public class ChitUtils : MonoBehaviour
{
	public static void ChitUnlockAllLevels(bool isComplete)
	{
		GameSettings.InitForEditor();
		if (GameSettings.gameSettings == null)
		{
			Debug.Log("Dont find GameSettings script into scene");
		}
		GameRecordsData records = GameSettings.gameSettings.records;
		GameDataStorage.LoadRecords(records);
		for (int i = 0; i < records.blockData.Length; i++)
		{
			records.blockData[i].isLock = false;
			for (int j = 0; j < records.blockData[i].level.Length; j++)
			{
				if (isComplete)
				{
					if (records.blockData[i].level[j].lockStatus == GameRecordsData.eLockStatus.locked || records.blockData[i].level[j].lockStatus == GameRecordsData.eLockStatus.incomplete)
					{
						records.blockData[i].level[j].lockStatus = GameRecordsData.eLockStatus.unlock;
					}
				}
				else if (records.blockData[i].level[j].lockStatus == GameRecordsData.eLockStatus.locked)
				{
					records.blockData[i].level[j].lockStatus = GameRecordsData.eLockStatus.incomplete;
				}
			}
		}
		GameDataStorage.SaveRecords(records);
	}
}
