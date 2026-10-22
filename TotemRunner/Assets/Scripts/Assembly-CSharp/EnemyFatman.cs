using UnityEngine;

public class EnemyFatman : DangerObject
{
	private float damageDistance = 80f;

	private Transform jointDamageBone;

	private Transform jointBody;

	public override void DangerAwake()
	{
		jointBody = base.transform.Find("Bugai/bones/bip_pelvis/bip_spine_0");
		jointDamageBone = base.transform.Find("Bugai/bones/bip_pelvis/bip_spine_0/bip_spine_1/bip_collar_L/bip_upperArm_L/bip_lowerArm_L/bip_hand_L");
	}

	public override void DangerUpdate()
	{
		float num = 0f;
		if (targetController != null)
		{
			num = base.transform.position.x - targetController.Position.x;
		}
		if (dangerState == eDangerObjectState.wait && num <= GameSettings.gameSettings.dangers.fatman.attackDistance)
		{
			SetObjectState(eDangerObjectState.attack);
		}
		eDangerObjectState stateSnapshot = dangerState;
		if (stateSnapshot == eDangerObjectState.wait || stateSnapshot == eDangerObjectState.attack)
		{
			if (num <= GameSettings.gameSettings.dangers.fatman.vulnerabilityDistance)
			{
				targetController.OnEnterToVulnerabilityZone(this);
			}
			damageDistance = base.transform.position.x - jointDamageBone.position.x;
			if (num <= damageDistance)
			{
				targetController.OnEnterToDangerZone(this, PlayerDeathType.impact, eEnemyAttackStyle.mightAttack);
			}
		}
		base.DangerUpdate();
	}

	public override void LateDangerUpdate()
	{
		if (dangerState == eDangerObjectState.attack)
		{
			Vector3 eulerAngles = Quaternion.FromToRotation(-Vector3.right, targetController.Position - base.transform.position).eulerAngles;
			jointBody.Rotate(eulerAngles, Space.World);
		}
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.fatman;
	}

	public override float GetDangerLeft()
	{
		return base.transform.position.x + GameSettings.gameSettings.dangers.fatman.leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		SetObjectState(eDangerObjectState.wait);
	}

	public override bool OnDangerWasAttack(bool isMightAttack)
	{
		if (isMightAttack)
		{
			SetObjectState(eDangerObjectState.destroy);
			return true;
		}
		return false;
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			refAnim.Play("idle");
			refAnim.Sample();
			refAnim.Stop();
			break;
		case eDangerObjectState.wait:
			refAnim.Play("wait");
			break;
		case eDangerObjectState.attack:
			refAnim.Play("attack");
			break;
		case eDangerObjectState.destroy:
			refAnim.CrossFade("death", 0.2f);
			break;
		}
	}

	public override void OnResetDanger()
	{
		SetObjectState(eDangerObjectState.inactive);
	}

	public override bool IsTough()
	{
		return true;
	}
}
