using UnityEngine;

public class TestInput : MonoBehaviour
{
	public float timeScale = 0.3f;

	public float fastTimeScale = 5f;

	public TextMesh dbgStr;

	private bool isShowDbg;

	private float debugMsgTimeout = -1f;

	private string debugMsgStr = string.Empty;

	private static TestInput _singleton;

	public static TestInput instance => _singleton;

	private void Awake()
	{
		_singleton = this;
		isShowDbg = PlayerPrefs.GetInt("dbgShowDbgText", 0) != 0;
		if (dbgStr != null)
		{
			dbgStr.gameObject.SetActiveRecursivelyCompat(isShowDbg);
		}
	}

	public void SetDebugMessage(string msgStr, float showTimeout)
	{
		debugMsgTimeout = showTimeout;
		debugMsgStr = msgStr;
	}

	private void Update()
	{
		if (!isShowDbg || !(dbgStr != null))
		{
			return;
		}
		string text = string.Empty;
		if (debugMsgTimeout >= 0f)
		{
			debugMsgTimeout -= Time.deltaTime;
			if (debugMsgTimeout <= 0f)
			{
				debugMsgTimeout = -1f;
			}
			else
			{
				text = debugMsgStr + ", ";
			}
		}
		dbgStr.text = text + "Fps: " + Mathf.FloorToInt(1f / Time.smoothDeltaTime);
	}
}
