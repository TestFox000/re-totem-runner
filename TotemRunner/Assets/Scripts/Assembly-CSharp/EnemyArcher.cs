using UnityEngine;

public class EnemyArcher : DangerObject
{
	private enum eArcherSubState
	{
		aiming = 0,
		shoot = 1
	}

	public class ArrowController
	{
		public enum eArrowState
		{
			inactive = 0,
			fly = 1,
			mirror = 2,
			tie = 3
		}

		private Vector2 pos;

		private Vector2 speed;

		private Quaternion rotation;

		private eArrowState state;

		private Transform parentTransform;

		private float timeLeft;

		public eArrowState GetArrowState()
		{
			return state;
		}

		public void StartFly(Vector3 begPos, Vector3 direction)
		{
			pos = begPos;
			speed = direction * GameSettings.gameSettings.dangers.archer.arrowPower;
			rotation.SetFromToRotation(Vector3.up, direction);
			timeLeft = 0.5f;
			state = eArrowState.fly;
		}

		public bool Update()
		{
			bool result = true;
			switch (state)
			{
			case eArrowState.inactive:
				result = false;
				break;
			case eArrowState.fly:
			case eArrowState.mirror:
				pos += speed * Time.deltaTime;
				Graphics.DrawMesh(GameSettings.gameSettings.dangers.archer.arrowMesh, pos, rotation, GameSettings.gameSettings.dangers.archer.arrowMaterial, 1);
				timeLeft -= Time.deltaTime;
				if (timeLeft <= 0f)
				{
					state = eArrowState.inactive;
				}
				break;
			case eArrowState.tie:
				if ((bool)parentTransform)
				{
					pos = parentTransform.position;
					rotation = parentTransform.rotation;
				}
				Graphics.DrawMesh(GameSettings.gameSettings.dangers.archer.arrowMesh, pos, rotation, GameSettings.gameSettings.dangers.archer.arrowMaterial, 1);
				timeLeft -= Time.deltaTime;
				if (timeLeft <= 0f)
				{
					state = eArrowState.inactive;
				}
				break;
			}
			return result;
		}

		public void Stop()
		{
			state = eArrowState.inactive;
		}

		public void TieToObject(Transform obj, float tiedTime)
		{
			parentTransform = obj;
			timeLeft = tiedTime;
			state = eArrowState.tie;
		}

		public void MirrorDirection()
		{
			speed = new Vector2(-1f, 1f).normalized * GameSettings.gameSettings.dangers.archer.arrowPower;
			rotation.SetFromToRotation(Vector3.up, speed);
			state = eArrowState.mirror;
		}

		public bool IsIntersectPlaneX(float x)
		{
			return pos.x <= x;
		}
	}

	private float timeLeft;

	private Transform jointArrow;

	private Transform jointBody;

	private float aimingTimeLeft;

	private static string boneNameBody = "Root/Hips_Joint/Spine";

	private static string boneNameArrow = "Root/Hips_Joint/Spine/Root_Arm_n_Head/Arm_Left/Elbow_Left/Hand_Left/Bow/Arrow";

	public static string boneNameTargetArrow = "Arrow_Joint";

	public eDangerDeathType deathType;

	private eArcherSubState subState;

	private ArrowController arrow;

	public override void DangerAwake()
	{
		jointBody = base.transform.Find(boneNameBody);
		jointArrow = base.transform.Find(boneNameArrow);
		if (jointBody == null)
		{
			Debug.LogWarning("Warning! Cant find bone name " + boneNameBody + " into archer model");
		}
		if (jointArrow == null)
		{
			Debug.LogWarning("Warning! Cant find bone name " + boneNameArrow + " into archer model");
		}
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.inactive);
	}

	public override void DangerUpdate()
	{
		if (dangerState == eDangerObjectState.inactive)
		{
			return;
		}
		if (arrow != null && dangerState == eDangerObjectState.attack && subState == eArcherSubState.shoot && arrow.GetArrowState() == ArrowController.eArrowState.fly && arrow.IsIntersectPlaneX(targetController.Position.x))
		{
			if (targetController.OnEnterToDangerZone(this, PlayerDeathType.impact, eEnemyAttackStyle.slightAttack))
			{
				arrow.MirrorDirection();
			}
			else
			{
				Transform transform = targetController.FindBone(boneNameTargetArrow);
				if (transform == null)
				{
					Debug.LogWarning("Warning! Cant find bone name " + boneNameTargetArrow + " into target model");
				}
				arrow.TieToObject(transform, 1f);
			}
		}
		bool flag = false;
		switch (dangerState)
		{
		case eDangerObjectState.wait:
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				SetObjectState(eDangerObjectState.attack);
			}
			else if (base.transform.position.x - targetController.Position.x <= GameSettings.gameSettings.dangers.archer.attackDistance)
			{
				SetObjectState(eDangerObjectState.attack);
			}
			flag = true;
			break;
		case eDangerObjectState.attack:
			if (subState == eArcherSubState.aiming)
			{
				timeLeft -= Time.deltaTime;
				if (timeLeft <= 0f)
				{
					SetSubState(eArcherSubState.shoot);
				}
			}
			flag = true;
			break;
		}
		if (flag && base.transform.position.x - targetController.Position.x <= GameSettings.gameSettings.dangers.archer.vulnerabilityDistance)
		{
			targetController.OnEnterToVulnerabilityZone(this);
		}
		base.DangerUpdate();
	}

	public override void LateDangerUpdate()
	{
		if (aimingTimeLeft > 0f)
		{
			aimingTimeLeft -= Time.deltaTime;
			if (aimingTimeLeft < GameSettings.gameSettings.dangers.archer.aimingTime)
			{
				Vector3 eulerAngles = Quaternion.FromToRotation(-Vector3.right, targetController.Position - base.transform.position).eulerAngles;
				jointBody.Rotate(eulerAngles, Space.World);
			}
		}
		if (arrow != null && !arrow.Update())
		{
			DestroyArrow();
		}
	}

	private void DestroyArrow()
	{
		arrow = null;
		if (targetController != null)
		{
			targetController.LockSkin(eLockSkin.none);
		}
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.archer;
	}

	public override float GetDangerLeft()
	{
		return base.transform.position.x + GameSettings.gameSettings.dangers.archer.leftBound;
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

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.wait:
			refAnim.Play("idle");
			timeLeft = GameSettings.gameSettings.dangers.archer.waitTime;
			aimingTimeLeft = GameSettings.gameSettings.dangers.archer.aimingTime + GameSettings.gameSettings.dangers.archer.aimingDelay;
			if (arrow == null)
			{
				arrow = new ArrowController();
			}
			arrow.TieToObject(jointArrow, 10f);
			break;
		case eDangerObjectState.attack:
			refAnim.CrossFade("attack", 0.2f);
			SetSubState(eArcherSubState.aiming);
			break;
		case eDangerObjectState.destroy:
			if (deathType == eDangerDeathType.alternative)
			{
				refAnim.CrossFade("deathAlt", 0.2f);
			}
			else
			{
				refAnim.CrossFade("death", 0.2f);
			}
			DestroyArrow();
			timeLeft = 3f;
			aimingTimeLeft = 0f;
			break;
		case eDangerObjectState.inactive:
			refAnim.Play("idle");
			refAnim.Sample();
			refAnim.Stop();
			DestroyArrow();
			aimingTimeLeft = 0f;
			break;
		}
	}

	public override void OnResetDanger()
	{
		SetObjectState(eDangerObjectState.inactive);
	}

	private void SetSubState(eArcherSubState newSubState)
	{
		switch (newSubState)
		{
		case eArcherSubState.aiming:
			timeLeft = GameSettings.gameSettings.dangers.archer.shootingDelay;
			break;
		case eArcherSubState.shoot:
		{
			Vector3 position = jointArrow.position;
			Vector3 position2 = targetController.Position;
			position2.y += 120f;
			position2 -= position;
			position2.Normalize();
			if (arrow == null)
			{
				arrow = new ArrowController();
			}
			arrow.StartFly(position, position2);
			if (targetController != null)
			{
				targetController.LockSkin(eLockSkin.boar);
			}
			break;
		}
		}
		subState = newSubState;
	}
}
