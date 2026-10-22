using UnityEngine;

public class TestTimer : MonoBehaviour
{
	private GUIText timeTextGUI;

	private static string strTestMessage = string.Empty;

	private void Awake()
	{
		timeTextGUI = GetComponent<GUIText>();
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		int num = Mathf.FloorToInt(realtimeSinceStartup) / 60;
		int num2 = Mathf.FloorToInt(realtimeSinceStartup - (float)num * 60f);
		string text = num + " min " + num2 + " sec, FPS: " + Mathf.FloorToInt(1f / Time.smoothDeltaTime);
		if (strTestMessage.Length > 0)
		{
			text = text + ", test: " + strTestMessage;
		}
		timeTextGUI.text = text;
	}

	public static void SetTestMessage(string str)
	{
		strTestMessage = str;
	}
}
