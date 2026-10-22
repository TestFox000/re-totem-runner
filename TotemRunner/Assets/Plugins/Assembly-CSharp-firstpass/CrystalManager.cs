using UnityEngine;

public class CrystalManager : MonoBehaviour
{
	public enum eLeaderBoardID
	{
		common = 0
	}

	private static CrystalManager _instance;

	public static CrystalManager Instance
	{
		get
		{
			if (!_instance)
			{
				Debug.LogWarning(Time.time + ": Scene have`t object with CrystalManager script");
			}
			return _instance;
		}
	}

	private static void activateCrystalUI()
	{
	}

	private static void deactivateCrystalUI()
	{
	}

	private static bool isCrystalUIActive()
	{
		return false;
	}

	private static void closeCrystalSession()
	{
	}

	private static void Imp_ReportAchievement(string achId, float percent)
	{
	}

	private static void Imp_ResetAchievements()
	{
	}

	private static void Imp_ReportScore(long score)
	{
	}

	private static bool Imp_CheckAvailable()
	{
		return false;
	}

	private static bool Imp_LaunchGCUI(bool isLeftSide)
	{
		Debug.Log(Time.time + ": Launch GameCenter GUI");
		return false;
	}

	private static bool Imp_IsConnectingGC()
	{
		return false;
	}

	private static bool isLeftCrystalOrientation()
	{
		return Screen.orientation == ScreenOrientation.LandscapeLeft;
	}

	private void Awake()
	{
		_instance = this;
	}

	public static void ShowProfile(bool isShow)
	{
		if (isShow)
		{
			activateCrystalUI();
		}
		else
		{
			deactivateCrystalUI();
		}
	}

	public static bool IsCrystalUIActive()
	{
		return isCrystalUIActive();
	}

	public static void SetLeaderboardValue(eLeaderBoardID idLeadbrd, int val)
	{
		if (idLeadbrd == eLeaderBoardID.common)
		{
			Imp_ReportScore(val);
		}
		else
		{
			Debug.LogWarning(Time.time + ": Error! Unknown Leaderboard ID " + idLeadbrd);
		}
	}

	public static void ResetAchievements()
	{
		Imp_ResetAchievements();
	}

	public static void ReportAchievement(string achId, float percent)
	{
		Imp_ReportAchievement(achId, percent);
	}

	public static bool LaunchGCUI()
	{
		return Imp_LaunchGCUI(Screen.orientation == ScreenOrientation.LandscapeLeft);
	}

	public static bool IsConnectingGC()
	{
		return Imp_IsConnectingGC();
	}

	public static bool IsAvailableGC()
	{
		return Imp_CheckAvailable();
	}

	public static bool IsLeftCrystalOrientation()
	{
		return isLeftCrystalOrientation();
	}
}
