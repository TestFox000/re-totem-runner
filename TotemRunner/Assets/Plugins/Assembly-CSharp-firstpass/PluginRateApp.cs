using UnityEngine;

public class PluginRateApp
{
	public static void ShowRatePromt()
	{
		Debug.LogWarning("[PluginRateApp] Rating is unavailable for this local build; no rating was submitted.");
	}

	public static bool IsRateProcess()
	{
		return false;
	}
}
