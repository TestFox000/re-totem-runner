using UnityEngine;

public class DangerBarrier : DangerObject
{
	public float leftBound;

	public float vulnerabilityOffset;

	public PlayerDeathType playerDeathType = PlayerDeathType.fall;

	public bool isTough = true;

	public bool playerDontPlayAttack;

	public bool barrierIsNotDestroyed;

	public string alternativeDeathAnimation = string.Empty;

	private bool makePlayerEvent;

	public override void DangerAwake()
	{
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.inactive);
	}

	public override void DangerUpdate()
	{
		bool flag = false;
		switch (dangerState)
		{
		case eDangerObjectState.wait:
			if (!barrierIsNotDestroyed)
			{
				flag = true;
			}
			if (dangerState == eDangerObjectState.wait && DangerHelper.DistanceFromObject(targetController, base.transform.position.x + 1000f) <= 0f)
			{
				SetObjectState(eDangerObjectState.inactive);
			}
			break;
		case eDangerObjectState.attack:
			if (makePlayerEvent && targetController != null)
			{
				if (targetController.OnEnterToDangerZone(this, playerDeathType, IsTough() ? eEnemyAttackStyle.mightAttack : eEnemyAttackStyle.slightAttack))
				{
					SetObjectState(eDangerObjectState.wait);
					flag = true;
				}
				makePlayerEvent = false;
			}
			break;
		case eDangerObjectState.inactive:
			refAnim.Stop();
			break;
		}
		if (flag && targetController != null && targetController.Position.x < base.transform.position.x + 50f && (double)DangerHelper.DistanceFromObject(targetController, base.transform.position.x + leftBound - GameSettings.gameSettings.dangers.barrier.vulnerabilityDistance - vulnerabilityOffset) <= 0.0)
		{
			targetController.OnEnterToVulnerabilityZone(this);
		}
		base.DangerUpdate();
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.barrier;
	}

	public override bool PlayerAttackAnimationDontPlay()
	{
		return playerDontPlayAttack;
	}

	public override float GetDangerLeft()
	{
		return base.transform.position.x + leftBound - Mathf.Max(GameSettings.gameSettings.dangers.barrier.activateDistance, GameSettings.gameSettings.dangers.barrier.vulnerabilityDistance + vulnerabilityOffset);
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		SetObjectState(eDangerObjectState.wait);
	}

	public override bool OnDangerWasAttack(bool isMightAttack)
	{
		if (barrierIsNotDestroyed)
		{
			return false;
		}
		if (isTough && !isMightAttack)
		{
			return false;
		}
		SetObjectState(eDangerObjectState.destroy);
		return true;
	}

	public override PlayerDeathType GetPlayerDeathType()
	{
		return playerDeathType;
	}

	public override bool IsTough()
	{
		return isTough;
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			refAnim.Play("idle");
			refAnim.Sample();
			refAnim.Stop();
			if (targetController != null)
			{
				targetController.OnExitFromDangerZone(this);
			}
			break;
		case eDangerObjectState.wait:
			refAnim.Play("idle");
			break;
		case eDangerObjectState.attack:
			refAnim.Play("push");
			makePlayerEvent = true;
			break;
		case eDangerObjectState.destroy:
			if (!string.IsNullOrEmpty(alternativeDeathAnimation))
			{
				refAnim.Play(alternativeDeathAnimation);
			}
			else
			{
				refAnim.Play("destroy");
			}
			break;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (targetController != null && targetController.HaveCollider(other))
		{
			if (dangerState == eDangerObjectState.wait)
			{
				SetObjectState(eDangerObjectState.attack);
			}
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

	public override void OnResetDanger()
	{
		SetObjectState(eDangerObjectState.inactive);
	}
}
