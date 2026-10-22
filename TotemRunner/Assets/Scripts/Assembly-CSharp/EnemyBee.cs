using UnityEngine;

public class EnemyBee : DangerObject
{
	public float leftBound = 850f;

	public float keepDistance = 500f;

	public float attackStartOffset = 1000f;

	public float attackMovementSpeed = 50f;

	public PlayerDeathType playerDeathType = PlayerDeathType.fall;

	public bool playerDontPlayAttack;

	public AudioClip attackSound;

	private float attackStartXPosition;

	private Vector3 startPoint = Vector3.zero;

	private bool makeAttackCheck;

	private float timeLeft = 3f;

	private Transform refTransform;

	public override void DangerAwake()
	{
		refTransform = base.transform;
		attackStartXPosition = refTransform.position.x + attackStartOffset;
		startPoint = refTransform.position;
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.inactive);
	}

	private void KeepDistance(float dist)
	{
		Vector3 position = refTransform.position;
		position.x = Mathf.Lerp(position.x, targetController.Position.x + dist, Time.deltaTime * 4f);
		if (!Mathf.Approximately(position.x, refTransform.position.x))
		{
			MoveToPos(position);
		}
	}

	private Vector3 GetAttackTarget()
	{
		Vector3 position = targetController.Position;
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Man:
			position.y += 160f;
			break;
		case EPlayerSkin.Eagle:
			position.y += 50f;
			break;
		case EPlayerSkin.Boar:
			position.y += 100f;
			break;
		}
		return position;
	}

	private void AttackTrack()
	{
		Vector3 position = refTransform.position;
		Vector3 attackTarget = GetAttackTarget();
		Vector3 vector = attackTarget - position;
		if (vector.x > 1f)
		{
			SetObjectState(eDangerObjectState.destroy);
			return;
		}
		position += vector.normalized * (attackMovementSpeed * Time.deltaTime);
		MoveToPos(position);
	}

	private void SampleAnimationToStart()
	{
		refAnim.Stop();
		refAnim.Play("idle");
		refAnim.Sample();
		refAnim.Stop();
	}

	public override void DangerUpdate()
	{
		switch (dangerState)
		{
		case eDangerObjectState.wait:
			KeepDistance(keepDistance);
			if (refTransform.position.x >= attackStartXPosition)
			{
				SetObjectState(eDangerObjectState.prepare);
			}
			break;
		case eDangerObjectState.prepare:
			KeepDistance(keepDistance);
			if (!refAnim.isPlaying)
			{
				SetObjectState(eDangerObjectState.chase);
			}
			break;
		case eDangerObjectState.chase:
			AttackTrack();
			if (makeAttackCheck)
			{
				if (!targetController.OnEnterToDangerZone(this, playerDeathType, eEnemyAttackStyle.slightAttack))
				{
					SetObjectState(eDangerObjectState.attack);
				}
				else
				{
					targetController.OnEnterToVulnerabilityZone(this);
				}
			}
			break;
		case eDangerObjectState.attack:
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				SetObjectState(eDangerObjectState.inactive);
			}
			break;
		case eDangerObjectState.destroy:
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				SetObjectState(eDangerObjectState.inactive);
			}
			break;
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
		return base.transform.position.x - leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		SetObjectState(eDangerObjectState.wait);
	}

	public override bool OnDangerWasAttack(bool isMightAttack)
	{
		SetObjectState(eDangerObjectState.destroy);
		return true;
	}

	public override PlayerDeathType GetPlayerDeathType()
	{
		return playerDeathType;
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			SampleAnimationToStart();
			if (targetController != null)
			{
				targetController.OnExitFromDangerZone(this);
			}
			refTransform.position = startPoint;
			break;
		case eDangerObjectState.wait:
			refAnim.Play("idle");
			break;
		case eDangerObjectState.prepare:
			refAnim.Play("prepare");
			if ((bool)attackSound)
			{
				SoundEnvironment.Launch3DSoundOnce(attackSound, base.transform.position);
			}
			break;
		case eDangerObjectState.chase:
			makeAttackCheck = false;
			refAnim.Play("aiming");
			AttackTrack();
			break;
		case eDangerObjectState.attack:
			refAnim.Play("attack");
			timeLeft = 3f;
			break;
		case eDangerObjectState.destroy:
			refAnim.Play("death");
			timeLeft = 3f;
			break;
		case eDangerObjectState.turnToPlayer:
		case eDangerObjectState.attackAlternative:
		case eDangerObjectState.hide:
			break;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (dangerState == eDangerObjectState.chase && !makeAttackCheck)
		{
			if (targetController != null && targetController.HaveCollider(other))
			{
				makeAttackCheck = true;
				return;
			}
			if (targetController == null)
			{
				targetController = other.GetComponent<PlayerController>();
				makeAttackCheck = true;
				return;
			}
			Debug.LogWarning(string.Concat("Other collider enter! ", other, "(", other.gameObject, ") instead ", targetController));
		}
	}

	public override void OnResetDanger()
	{
		SetObjectState(eDangerObjectState.inactive);
	}

	public override bool IsBoarVulnerability()
	{
		return true;
	}
}
