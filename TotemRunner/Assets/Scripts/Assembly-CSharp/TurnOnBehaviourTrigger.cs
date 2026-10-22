using UnityEngine;
using System;

public class TurnOnBehaviourTrigger : BaseTrigger
{
	[Serializable]
	public class BehaviourEntity
	{
		public TriggeredBehaviour behaviour;

		public float timeDelayToNext;
	}

	public BehaviourEntity[] list;

	private float timeFollowPrev = -1f;

	private float timeFollow = -1f;

	public override void ExecuteTrigger()
	{
		if (timeFollow < 0f)
		{
			timeFollow = 0f;
			RestartObjectsList.IncludeRestartObject(this);
		}
		float num = 10000f;
		BehaviourEntity[] array = list;
		foreach (BehaviourEntity behaviourEntity in array)
		{
			if (behaviourEntity.timeDelayToNext <= timeFollow)
			{
				if (behaviourEntity.timeDelayToNext > timeFollowPrev)
				{
					ExecuteEntity(behaviourEntity);
				}
			}
			else if (behaviourEntity.timeDelayToNext < num)
			{
				num = behaviourEntity.timeDelayToNext;
			}
		}
		if (num < 10000f)
		{
			timeLeft = num - timeFollow;
			timeFollowPrev = timeFollow;
			timeFollow = num;
		}
	}

	private void ExecuteEntity(BehaviourEntity entity)
	{
		entity.behaviour.StartExecute();
	}

	public override void Update()
	{
		base.Update();
	}

	public override bool RestartToX(float x)
	{
		base.RestartToX(x);
		timeFollow = -1f;
		timeFollowPrev = -1f;
		BehaviourEntity[] array = list;
		foreach (BehaviourEntity behaviourEntity in array)
		{
			if (behaviourEntity.behaviour.transform.position.x >= x)
			{
				behaviourEntity.behaviour.RestartToX(x);
				if ((bool)behaviourEntity.behaviour.GetComponent<Animation>())
				{
					behaviourEntity.behaviour.GetComponent<Animation>().Stop();
				}
			}
		}
		return true;
	}
}
