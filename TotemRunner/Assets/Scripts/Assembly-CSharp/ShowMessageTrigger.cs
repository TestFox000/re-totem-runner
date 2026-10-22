using System;

public class ShowMessageTrigger : BaseTrigger
{
	[Serializable]
	public class StringData
	{
		public string strText = string.Empty;

		public float timeoffset;

		public float timeout = 3.4f;
	}

	public StringData[] strings;

	public GuiGameHud.eMessageType msgType;

	private float prevTime = -1f;

	private float nextTime;

	private static float outRangeTimer = 10000f;

	private void Start()
	{
		GameRecordsData.LevelData currentLevelData = GameSettings.gameSettings.GetCurrentLevelData();
		if (currentLevelData == null)
		{
			return;
		}
		for (int i = 0; i < strings.Length; i++)
		{
			if (strings[i].strText.IndexOf("$") >= 0)
			{
				strings[i].strText = strings[i].strText.Replace("$levelName", GameSettings.gameSettings.GetCurrentLevelData().name);
			}
		}
	}

	public override void ExecuteTrigger()
	{
		if (prevTime < 0f)
		{
			RestartObjectsList.IncludeRestartObject(this);
		}
		float timeoffset = outRangeTimer;
		for (int i = 0; i < strings.Length; i++)
		{
			if (strings[i].timeoffset > prevTime)
			{
				if (strings[i].timeoffset <= nextTime)
				{
					GuiGameHud.ShowMessageString(strings[i].strText, strings[i].timeout, i > 0, msgType);
				}
				else if (strings[i].timeoffset < timeoffset)
				{
					timeoffset = strings[i].timeoffset;
				}
			}
		}
		if (timeoffset < outRangeTimer)
		{
			prevTime = nextTime;
			nextTime = timeoffset;
			timeLeft = nextTime - prevTime;
		}
		else
		{
			prevTime = 0f;
		}
	}

	public override bool RestartToX(float x)
	{
		if (x < base.transform.position.x)
		{
			prevTime = -1f;
			nextTime = 0f;
		}
		return base.RestartToX(x);
	}
}
