using UnityEngine;

public class HttpBrowser
{
	private static void Imp_ShowBrowser(string httpUrl)
	{
		Application.OpenURL(httpUrl);
	}

	public static void OpenBrowser(string httpUrl)
	{
		Imp_ShowBrowser(httpUrl);
	}
}
