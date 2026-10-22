using UnityEngine;

public class PluginUtils
{
	public enum eDeviceKeyDown
	{
		none = 0,
		keyMenu = 1,
		keyBack = 2
	}

	private static eDeviceKeyDown lastUpdateKey;

	private static int lastUpdateFrame = -1;

	private static int Imp_GetDisplayType()
	{
		// The original art has 4:3, 3:2 and 16:9 variants. Pick the nearest
		// family instead of treating every modern widescreen phone as a 4:3 iPad.
		int shortSide = Mathf.Max(1, Mathf.Min(Screen.width, Screen.height));
		float aspect = (float)Mathf.Max(Screen.width, Screen.height) / shortSide;
		float distance43 = Mathf.Abs(aspect - 4f / 3f);
		float distance32 = Mathf.Abs(aspect - 3f / 2f);
		float distance169 = Mathf.Abs(aspect - 16f / 9f);
		if (distance169 <= distance32 && distance169 <= distance43)
		{
			return shortSide > 320 ? 6 : 5;
		}
		if (distance32 <= distance43)
		{
			return shortSide > 320 ? 2 : 1;
		}
		return shortSide > 768 ? 4 : 3;
	}

	private static bool Imp_IsChits()
	{
		return false;
	}

	private static bool Imp_IsShowFacebook()
	{
		return false;
	}

	public static int Imp_GetLastKeyDown()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			return (int)eDeviceKeyDown.keyBack;
		}
		return Input.GetKeyDown(KeyCode.Menu) ? (int)eDeviceKeyDown.keyMenu : (int)eDeviceKeyDown.none;
	}

	public static void Imp_quitFromApp()
	{
		Application.Quit();
	}

	public static int GetDisplayType()
	{
		return Imp_GetDisplayType();
	}

	public static bool IsChits()
	{
		return Imp_IsChits();
	}

	public static bool IsShowFacebook()
	{
		return Imp_IsShowFacebook();
	}

	public static eDeviceKeyDown GetDeviceKeyDown()
	{
		if (Time.frameCount != lastUpdateFrame)
		{
			lastUpdateFrame = Time.frameCount;
			switch (Imp_GetLastKeyDown())
			{
			case 0:
				lastUpdateKey = eDeviceKeyDown.none;
				break;
			case 1:
				lastUpdateKey = eDeviceKeyDown.keyMenu;
				break;
			case 2:
				lastUpdateKey = eDeviceKeyDown.keyBack;
				break;
			default:
				lastUpdateKey = eDeviceKeyDown.none;
				break;
			}
		}
		return lastUpdateKey;
	}

	public static void QuitFromApp()
	{
		Imp_quitFromApp();
	}
}
