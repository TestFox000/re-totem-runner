using UnityEngine;

public class SocialNetPoster
{
	private static int postingStatus = -1;

	private static bool Imp_FBLaunchPost()
	{
		// Status 1 grants gems in ChooseLevelGUI. Report the existing failure
		// status (2) instead: this build has no Facebook connection.
		postingStatus = 2;
		Debug.LogWarning("[SocialNetPoster] Facebook posting is unavailable; nothing was posted.");
		return false;
	}

	private static int Imp_FBPostingStatus()
	{
		int result = postingStatus;
		postingStatus = -1;
		return result;
	}

	public static bool LaunchFacebookPost()
	{
		return Imp_FBLaunchPost();
	}

	public static int GetPostingStatus()
	{
		return Imp_FBPostingStatus();
	}
}
