using UnityEngine;

public class BaseTrigger : RestartObject
{
	public float timeDelay;

	protected float timeLeft = -2f;

	public virtual void OnTriggerEnter(Collider other)
	{
		if (timeLeft < -1f)
		{
			TriggerActivation();
		}
	}

	public virtual void OnTriggerStay(Collider other)
	{
		if (timeLeft < -1f)
		{
			TriggerActivation();
		}
	}

	protected void TriggerActivation()
	{
		if (timeDelay > 0f)
		{
			timeLeft = timeDelay;
		}
		else
		{
			ExecuteTrigger();
		}
	}

	public virtual void ExecuteTrigger()
	{
	}

	public virtual void Update()
	{
		if (timeLeft >= 0f)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				timeLeft = -1f;
				ExecuteTrigger();
			}
		}
	}

	public override bool RestartToX(float x)
	{
		timeLeft = -2f;
		return true;
	}
}
