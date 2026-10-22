using UnityEngine;

public class DangerObstacle : DangerObject
{
	public float leftBound;

	public float topBound;

	public float bottomBound;

	public eDangerType obstacleType = eDangerType.obstacle;

	public bool animationPlayAutomatically;

	public override eDangerType GetDangerType()
	{
		return obstacleType;
	}

	public override float GetDangerLeft()
	{
		return base.transform.position.x + leftBound - GameSettings.gameSettings.dangers.obstacle.vulnerabilityDistance;
	}

	public override void DoVisible(bool makeVisible)
	{
		base.DoVisible(makeVisible);
		if (refAnim != null && makeVisible && animationPlayAutomatically)
		{
			refAnim.Play();
		}
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		SetObjectState(eDangerObjectState.wait);
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		if (state == eDangerObjectState.attack)
		{
			targetController.OnEnterToDangerZone(this, (obstacleType != eDangerType.obstacle) ? PlayerDeathType.impact : PlayerDeathType.thorn, eEnemyAttackStyle.nonBreakable);
			targetController.OnExitFromDangerZone(this);
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (dangerState == eDangerObjectState.wait)
		{
			if (targetController != null && targetController.HaveCollider(other))
			{
				SetObjectState(eDangerObjectState.attack);
				return;
			}
			if (targetController == null)
			{
				targetController = other.GetComponent<PlayerController>();
				SetObjectState(eDangerObjectState.attack);
				return;
			}
			Debug.LogWarning(string.Concat("Other collider enter! ", other, "(", other.gameObject, ") instead ", targetController));
		}
	}

	public void OnTriggerStay(Collider other)
	{
		if (dangerState == eDangerObjectState.wait)
		{
			if (targetController != null && targetController.HaveCollider(other))
			{
				SetObjectState(eDangerObjectState.attack);
				return;
			}
			if (targetController == null)
			{
				targetController = other.GetComponent<PlayerController>();
				SetObjectState(eDangerObjectState.attack);
				return;
			}
			Debug.LogWarning(string.Concat("Other collider enter! ", other, "(", other.gameObject, ") instead ", targetController));
		}
	}

	public override void OnResetDanger()
	{
		SetObjectState(eDangerObjectState.inactive);
	}
}
