using UnityEngine;

public class DangerBomb : DangerObject
{
	public enum eBombType
	{
		drop = 0,
		standing = 1
	}

	public enum eIdleType
	{
		idle = 0,
		idle2 = 1
	}

	private CapsuleCollider dangerCollider;

	private static float destroyTime = 0.5f;

	private static float leftBound = -1000f;

	public float lifeTime = 1f;

	private float leftTime = 1f;

	public PlayerDeathType playerDeathType = PlayerDeathType.impact;

	public float destroyDistance;

	public bool destroyByFlowering;

	public ECameraShakeType shakeType = ECameraShakeType.death;

	public eBombType bombType = eBombType.standing;

	public eIdleType idleType;

	public static void CreateBomb(GameObject bombPrefab, Vector3 pos, Quaternion rotate)
	{
		GameObject gameObject = Object.Instantiate(bombPrefab, pos, rotate) as GameObject;
		DangerBomb component = gameObject.GetComponent<DangerBomb>();
		component.refAnim = component.GetComponent<Animation>();
		component.dangerCollider = component.GetComponent<CapsuleCollider>();
		component.bombType = eBombType.drop;
		component.playerDeathType = PlayerDeathType.impact;
		component.OnPlayerEnter(PlayerController.instance);
		DangerHelper.IncludeTempDanger(component);
	}

	public override void DangerAwake()
	{
		leftTime = lifeTime;
		dangerCollider = GetComponent<CapsuleCollider>();
	}

	public override void DangerUpdate()
	{
		switch (dangerState)
		{
		case eDangerObjectState.attack:
			targetController.OnEnterToDangerZone(this, playerDeathType, eEnemyAttackStyle.nonBreakable);
			targetController.OnExitFromDangerZone(this);
			SetObjectState(eDangerObjectState.destroy);
			break;
		case eDangerObjectState.wait:
			if (destroyDistance != 0f || destroyByFlowering)
			{
				float num = base.transform.position.x - targetController.Position.x;
				if (destroyByFlowering)
				{
					num -= GrassGifManager.instance.flowers.offsetX + GrassGifManager.instance.flowers.rangeX - 130f;
				}
				if (num <= destroyDistance)
				{
					bool flag = destroyDistance != 0f;
					if (destroyByFlowering)
					{
						flag = GrassGifManager.instance.IsGrowing();
					}
					if (flag)
					{
						leftTime = -1f;
						SetObjectState(eDangerObjectState.destroy);
					}
				}
			}
			if (leftTime >= 0f)
			{
				leftTime -= Time.deltaTime;
				if (leftTime <= 0f)
				{
					SetObjectState(eDangerObjectState.destroy);
				}
			}
			break;
		}
		base.DangerUpdate();
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.bomb;
	}

	public override float GetDangerLeft()
	{
		if (bombType == eBombType.drop)
		{
			return base.transform.position.x - dangerCollider.radius;
		}
		return base.transform.position.x + leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		SetObjectState(eDangerObjectState.wait);
	}

	public override bool OnPlayerReenter()
	{
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.wait);
		bool flag = true;
		if (destroyDistance != 0f && base.transform.position.x - targetController.Position.x <= destroyDistance)
		{
			flag = false;
			SetObjectState(eDangerObjectState.destroy);
		}
		if (flag && leftTime > 0f)
		{
			float num = targetController.Position.x - GetDangerLeft();
			if (num > 0f)
			{
				Mathf.Clamp(leftTime - num / 400f, 0f, leftTime);
			}
			if (leftTime == 0f)
			{
				flag = false;
				SetObjectState(eDangerObjectState.destroy);
			}
		}
		return true;
	}

	private string GetIdleAnimationName()
	{
		string result = string.Empty;
		switch (idleType)
		{
		case eIdleType.idle:
			result = "idle";
			break;
		case eIdleType.idle2:
			result = "idle2";
			break;
		}
		return result;
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			refAnim.Play(GetIdleAnimationName());
			refAnim.Sample();
			refAnim.Stop();
			break;
		case eDangerObjectState.wait:
			refAnim.Play(GetIdleAnimationName());
			leftTime = lifeTime;
			break;
		case eDangerObjectState.attack:
			break;
		case eDangerObjectState.destroy:
			refAnim.Play("explosion");
			CameraShake.instance.LaunchShake(shakeType, 1f);
			if (bombType == eBombType.drop)
			{
				Object.Destroy(this, destroyTime);
			}
			break;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (dangerState == eDangerObjectState.wait)
		{
			if (targetController != null && targetController.HaveCollider(other))
			{
				SetObjectState(eDangerObjectState.attack);
			}
			else
			{
				Debug.LogWarning("Other collider enter!");
			}
		}
	}

	public override void OnResetDanger()
	{
		if (bombType == eBombType.drop)
		{
			Object.Destroy(this, 0f);
		}
		else
		{
			SetObjectState(eDangerObjectState.inactive);
		}
	}
}
