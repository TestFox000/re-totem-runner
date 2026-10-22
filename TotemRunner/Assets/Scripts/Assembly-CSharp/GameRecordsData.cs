using System;

[Serializable]
public class GameRecordsData
{
	public enum eLockStatus
	{
		locked = 0,
		incomplete = 1,
		unlock = 2,
		unused = 3
	}

	[Serializable]
	public class LevelData
	{
		public string name = "noname";

		public int masterRecord;

		[NonSerialized]
		public eLockStatus lockStatus;

		[NonSerialized]
		public int stars;

		[NonSerialized]
		public int record;
	}

	[Serializable]
	public class BlockData
	{
		public string blockName = "noname";

		public int starsToUnlock;

		[NonSerialized]
		public bool isLock = true;

		public LevelData[] level = new LevelData[4];
	}

	public BlockData[] blockData = new BlockData[6];
}
