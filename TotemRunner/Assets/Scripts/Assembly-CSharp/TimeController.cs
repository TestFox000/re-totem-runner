using System;
using UnityEngine;

public class TimeController : MonoBehaviour
{
	public enum eTimeMode
	{
		normal = 0,
		slowmo = 1
	}

	[Serializable]
	public class SlowMoEntity
	{
		public string id = "default";

		public float slowmoScale = 1f;

		public float timeFadeIn = 0.2f;

		public float timeFixed = 0.6f;

		public float timeFadeOut = 0.2f;

		public SlowMoEntity(string idName, float scale, float fadeIn, float fixedTime, float fadeOut)
		{
			id = idName;
			slowmoScale = scale;
			timeFadeIn = fadeIn;
			timeFixed = fixedTime;
			timeFadeOut = fadeOut;
		}
	}

	private eTimeMode currentMode;

	private static bool isTimePaused;

	private eSlowMoMode curSlowMoMode;

	private float slowmoTimeLeft = -1f;

	private float slowmoScale = 1f;

	private float timeFadeInMul = 5f;

	private float timeFixed = 0.6f;

	private float timeFadeOut = 0.2f;

	private float timeFadeOutMul = 5f;

	public float globalTimeScale = 1f;

	private float prevRealTime;

	private float realDeltaTime;

	private static TimeController _singleton;

	public SlowMoEntity[] slowmoList;

	public float RealDelta => realDeltaTime;

	public static TimeController instance => _singleton;

	public static float commonScale
	{
		get
		{
			return (!(_singleton == null)) ? _singleton.globalTimeScale : 1f;
		}
		set
		{
			if (_singleton != null)
			{
				_singleton.globalTimeScale = value;
				if (_singleton.currentMode == eTimeMode.normal)
				{
					_singleton.SetTimeScale(1f);
				}
			}
		}
	}

	private void Awake()
	{
		_singleton = this;
		isTimePaused = false;
		SetTimeScale(1f);
		prevRealTime = Time.realtimeSinceStartup;
	}

	private void SetTimeScale(float scale)
	{
		if (!isTimePaused)
		{
			Time.timeScale = scale * globalTimeScale;
		}
	}

	private void Update()
	{
		realDeltaTime = Time.realtimeSinceStartup - prevRealTime;
		prevRealTime = Time.realtimeSinceStartup;
		if (isTimePaused)
		{
			return;
		}
		eTimeMode eTimeMode2 = currentMode;
		if (eTimeMode2 != eTimeMode.slowmo)
		{
			return;
		}
		slowmoTimeLeft -= Time.deltaTime;
		if (slowmoTimeLeft <= 0f)
		{
			StopSlowMo(forced: true);
			return;
		}
		float num = slowmoTimeLeft;
		if (timeFadeOut > 0f && num <= timeFadeOut)
		{
			SetTimeScale(Mathf.Lerp(1f, slowmoScale, num * timeFadeOutMul));
			return;
		}
		num -= timeFadeOut + timeFixed;
		if (num <= 0f)
		{
			SetTimeScale(slowmoScale);
		}
		else
		{
			SetTimeScale(Mathf.Lerp(slowmoScale, 1f, num * timeFadeInMul));
		}
	}

	public static void StartPause(bool isPause)
	{
		isTimePaused = isPause;
		if (isPause)
		{
			Time.timeScale = 0f;
		}
		else if (_singleton == null)
		{
			Time.timeScale = 1f;
		}
		else
		{
			_singleton.SetTimeScale(1f);
		}
	}

	public static void StartSlowMo(string id)
	{
		SlowMoEntity[] array = _singleton.slowmoList;
		foreach (SlowMoEntity slowMoEntity in array)
		{
			if (slowMoEntity.id == id)
			{
				StartSlowMo(slowMoEntity.slowmoScale, slowMoEntity.timeFadeIn, slowMoEntity.timeFixed, slowMoEntity.timeFadeOut, eSlowMoMode.normal);
				return;
			}
		}
		Debug.LogError("Error! Cant start SlowMo (id=" + id + ") effect because his not founded in list");
	}

	public static void StartSlowMo(float scale, float fadeIn, float fixedTime, float fadeOut, eSlowMoMode slowMoMode)
	{
		_singleton.slowmoScale = scale;
		_singleton.timeFadeInMul = ((!(fadeIn > 0f)) ? 1f : (1f / fadeIn));
		_singleton.timeFixed = fixedTime;
		_singleton.timeFadeOut = fadeOut;
		_singleton.timeFadeOutMul = ((!(fadeOut > 0f)) ? 1f : (1f / fadeOut));
		_singleton.slowmoTimeLeft = fadeIn + fixedTime + fadeOut;
		_singleton.curSlowMoMode = slowMoMode;
		_singleton.currentMode = eTimeMode.slowmo;
	}

	public static void StopSlowMo(bool forced)
	{
		if (_singleton.curSlowMoMode != eSlowMoMode.nonBreakable || forced)
		{
			_singleton.currentMode = eTimeMode.normal;
			_singleton.SetTimeScale(1f);
		}
	}

	public static void ResetToInit()
	{
		isTimePaused = false;
		if (_singleton != null)
		{
			_singleton.currentMode = eTimeMode.normal;
			_singleton.SetTimeScale(1f);
		}
	}
}
