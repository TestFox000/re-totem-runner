using UnityEngine;

public class ScoreManager
{
	public enum eScorePart
	{
		grass = 0,
		berry = 1,
		seed = 2,
		addition = 3,
		gem = 4,
		death = 5,
		time = 6,
		allComplete = 7,
		none = -1
	}

	public static void UpdateScores()
	{
		if (!(GrassGifManager.instance == null))
		{
			GuiGameHud.SetScoreCounter(CalculateScore());
		}
	}

	public static int CalculateScore()
	{
		int num = 0;
		num += CalculatePartOfScore(eScorePart.gem);
		num += CalculatePartOfScore(eScorePart.addition);
		num += CalculatePartOfScore(eScorePart.seed);
		num += CalculatePartOfScore(eScorePart.berry);
		num -= GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.subScore);
		return Mathf.Max(0, num);
	}

	public static int CalculatePartOfScore(eScorePart partId)
	{
		int num = 0;
		switch (partId)
		{
		case eScorePart.grass:
			num = 0;
			break;
		case eScorePart.berry:
			if (GrassGifManager.instance.GetGrassCount() > 300)
			{
				num = (GrassGifManager.instance.GetGrassCount() - 300) * GameSettings.gameSettings.scoreItem.berryScore;
			}
			break;
		case eScorePart.addition:
			num = GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.addScore);
			break;
		case eScorePart.seed:
			num = GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.seed) * GameSettings.gameSettings.scoreItem.seedScore;
			break;
		case eScorePart.gem:
			num = GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.gem) * GameSettings.gameSettings.scoreItem.gemScore;
			break;
		case eScorePart.death:
			num = GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.deathCount) * 50;
			break;
		case eScorePart.time:
			num = Mathf.FloorToInt(GameSettings.GetLevelCompleteData().timer);
			if (num > 0)
			{
				num = 42000 / num;
			}
			break;
		case eScorePart.allComplete:
			num = CalculateScore();
			break;
		}
		return num;
	}

	public static int GetStarQuantity()
	{
		int num = CalculatePartOfScore(eScorePart.allComplete);
		int result = 0;
		if (LevelAttributes.instance != null && LevelAttributes.instance.scoreForStars != null)
		{
			int num2 = 0;
			if (LevelAttributes.instance.scoreForStars.Length > 0)
			{
				num2 = LevelAttributes.instance.scoreForStars[0];
			}
			int num3 = num2;
			if (LevelAttributes.instance.scoreForStars.Length > 1)
			{
				num3 = LevelAttributes.instance.scoreForStars[1];
			}
			int num4 = num3;
			if (LevelAttributes.instance.scoreForStars.Length > 2)
			{
				num4 = LevelAttributes.instance.scoreForStars[2];
			}
			if (num >= num4)
			{
				result = 3;
			}
			else if (num >= num3)
			{
				result = 2;
			}
			else if (num >= num2)
			{
				result = 1;
			}
		}
		return result;
	}
}
