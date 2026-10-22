using System;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
	[Serializable]
	public class ShakeWave
	{
		public float length;

		public float amplitude;

		public float period;
	}

	[Serializable]
	public class Shake
	{
		public string id;

		public ShakeWave[] waves;

		public Shake(string name, float length, float amp, float period)
		{
			id = name;
			waves = new ShakeWave[1];
			waves[0] = new ShakeWave();
			waves[0].length = length;
			waves[0].amplitude = amp;
			waves[0].period = period;
		}
	}

	public Shake[] shakes = new Shake[6]
	{
		new Shake("UsualShake", 0.25f, 0.006f, 0.125f),
		new Shake("boarAttack", 0.5f, 0.01f, 0.25f),
		new Shake("ground", 0.3f, 0.03f, 0.15f),
		new Shake("boarSpeedupStep", 0.1f, 0.005f, 0.05f),
		new Shake("death", 0.5f, 0.01f, 0.25f),
		new Shake("boarStep", 0.1f, 0.005f, 0.05f)
	};

	private ECameraShakeType curShake = ECameraShakeType.none;

	private float curTime;

	private float curShakeScale = 1f;

	private Quaternion baseCameraAngle;

	private static CameraShake _singleton;

	public static CameraShake instance => _singleton;

	private void Awake()
	{
		_singleton = this;
		baseCameraAngle = base.transform.rotation;
		curShakeScale = 1f;
		Shake[] array = shakes;
		foreach (Shake shake in array)
		{
			ShakeWave[] waves = shake.waves;
			foreach (ShakeWave shakeWave in waves)
			{
				if (shakeWave.period <= 0f)
				{
					shakeWave.period = 0.1f;
				}
			}
		}
	}

	private void Update()
	{
		if (curShake == ECameraShakeType.none)
		{
			return;
		}
		curTime += Time.deltaTime;
		bool flag = true;
		Quaternion identity = Quaternion.identity;
		ShakeWave[] waves = shakes[(int)curShake].waves;
		foreach (ShakeWave shakeWave in waves)
		{
			if (curTime <= shakeWave.length)
			{
				flag = false;
				float num = 1f;
				float num2 = curTime / shakeWave.period;
				num2 -= (float)Mathf.FloorToInt(num2);
				if (num2 > 0.5f)
				{
					num = -1f;
					num2 -= 0.5f;
				}
				num2 = 8f * (num2 - 2f * num2 * num2);
				identity.x += shakeWave.amplitude * num2 * num * curShakeScale;
			}
		}
		if (flag)
		{
			StopShake();
		}
		else
		{
			base.transform.rotation = baseCameraAngle * identity;
		}
	}

	private void StopShake()
	{
		curShake = ECameraShakeType.none;
		base.transform.rotation = baseCameraAngle;
	}

	public void LaunchShake(ECameraShakeType shakeType, float shakeScale)
	{
		curShakeScale = shakeScale;
		if (shakeType >= ECameraShakeType.usualShake && (int)shakeType < shakes.Length)
		{
			curShake = shakeType;
			curTime = 0f;
		}
	}
}
