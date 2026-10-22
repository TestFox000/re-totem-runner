using UnityEngine;

public class GameStatistic : MonoBehaviour
{
	public enum eStatisticItem
	{
		gem = 0,
		kill = 1,
		addScore = 2,
		subScore = 3,
		deathCount = 4,
		seed = 5,
		gemSummary = 6,
		itemCount = 7,
		none = -1
	}

	private class SavedData
	{
		public int gem;

		public int kill;

		public int addScore;

		public int subScore;

		public int gemSummary;

		public int seed;
	}

	private static GameStatistic _singleton;

	private int[] itemCounter = new int[7];

	private SavedData save = new SavedData();

	public static GameStatistic instance => _singleton;

	private void Awake()
	{
		_singleton = this;
		LoadOnStart();
	}

	private void Start()
	{
		ScoreManager.UpdateScores();
	}

	private void LoadOnStart()
	{
		for (int i = 0; i < 7; i++)
		{
			itemCounter[i] = 0;
		}
		itemCounter[6] = GameDataStorage.GetCrystalSummary();
	}

	public int ItemCount(eStatisticItem itemId)
	{
		return itemCounter[(int)itemId];
	}

	public void TakeItem(eStatisticItem item, int count)
	{
		itemCounter[(int)item] -= count;
	}

	public void AddItem(eStatisticItem item, int count)
	{
		itemCounter[(int)item] += count;
		ScoreManager.UpdateScores();
	}

	public void Increment(eStatisticItem item)
	{
		itemCounter[(int)item]++;
		if (item == eStatisticItem.gem)
		{
			itemCounter[6]++;
		}
		ScoreManager.UpdateScores();
	}

	public void SaveOnCheckpoint()
	{
		save.gem = itemCounter[0];
		save.kill = itemCounter[1];
		save.addScore = itemCounter[2];
		save.subScore = itemCounter[3];
		save.gemSummary = itemCounter[6];
		save.seed = itemCounter[5];
	}

	public void LoadFromCheckpoint()
	{
		itemCounter[0] = save.gem;
		itemCounter[1] = save.kill;
		itemCounter[2] = save.addScore;
		itemCounter[3] = save.subScore;
		itemCounter[6] = save.gemSummary;
		itemCounter[5] = save.seed;
		ScoreManager.UpdateScores();
	}

	public void SaveInStorage()
	{
		GameDataStorage.SetCrystalSummary(itemCounter[6]);
	}

	public void PurshaseGems(int gemsCount)
	{
		AddItem(eStatisticItem.gemSummary, gemsCount);
		save.gemSummary += gemsCount;
	}
}
