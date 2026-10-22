using UnityEngine;

public class MaterialBlink : BaseColorBlend
{
	public float minPeriod = 0.5f;

	public float blinkPeriod = 0.5f;

	public float maxPeriod = 2f;

	public bool useRealTime;

	private float oldRealTime = -1f;

	private float timeFollow;

	private float timePeriod = 1f;

	private float blinkMul = 1f;

	public override void Awake()
	{
		base.Awake();
		timePeriod = minPeriod + blinkPeriod + maxPeriod + blinkPeriod;
		timeFollow = 0f;
		blinkMul = 1f / blinkPeriod;
		oldRealTime = -1f;
	}

	private void Update()
	{
		if (useRealTime)
		{
			if (oldRealTime >= 0f)
			{
				timeFollow += Time.realtimeSinceStartup - oldRealTime;
			}
			oldRealTime = Time.realtimeSinceStartup;
		}
		else
		{
			timeFollow += Time.deltaTime;
		}
		if (timeFollow >= timePeriod)
		{
			timeFollow -= timePeriod;
		}
		float num = timeFollow;
		if (num < minPeriod)
		{
			base.BlendFactor = 0f;
			return;
		}
		num -= minPeriod;
		if (num < blinkPeriod)
		{
			base.BlendFactor = num * blinkMul;
			return;
		}
		num -= blinkPeriod;
		if (num < maxPeriod)
		{
			base.BlendFactor = 1f;
			return;
		}
		num -= maxPeriod;
		base.BlendFactor = 1f - num * blinkMul;
	}

	public override void ResetToStart()
	{
		timeFollow = 0f;
		base.ResetToStart();
	}
}
