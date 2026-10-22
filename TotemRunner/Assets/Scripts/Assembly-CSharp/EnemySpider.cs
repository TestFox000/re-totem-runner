using System;
using UnityEngine;

public class EnemySpider : DangerObject
{
	[Serializable]
	public class SpiderBehaviourParams
	{
		public float keepDistanceStart = 400f;

		public float beforeKeepDistanceSpeed = 100f;

		public float movementSpeed = 40f;

		public float walkBoundTime = 10000f;

		public float walkBoundDistance = 50000f;

		public float visibleTime = 1000f;

		public float visibleDistance = 50000f;

		public float bombDropFirst;

		public float bombDropStep = 1000f;

		public int bombQuantity = 5;

		public float sincAnimationOffset;

		public bool startAmbush;

		public eDangerMovementStyle walkMovementStyle;

		public eDangerMovementStyle backMovementStyle;

		public SlowMoParameters slowMo;
	}

	[Serializable]
	public class SpiderScaleEntity
	{
		public Transform refTransform;

		public float scaleFactor = 1f;

		public float scalePhaseBegin;

		public float scalePhaseLength = 0.5f;
	}

	public SpiderBehaviourParams spiderBehaviour;

	public PlayerDeathType playerDeath1Type = PlayerDeathType.impact;

	public PlayerDeathType playerDeath2Type = PlayerDeathType.impact;

	public SpiderScaleEntity[] scaleTransforms;

	public float scaleTransformDistance = 400f;

	public float scaleTransformOffset = 100f;

	private float scaleTransformBound;

	private Vector3 startPosition;

	private float timeLeft;

	private int bombLeft;

	private float livedTime;

	private float distanceFromTarget;

	private float dropBombBound;

	private GameObject vomitObject;

	public override void DangerAwake()
	{
		startPosition = base.transform.position;
		SamplingAnimation();
		vomitObject = base.transform.Find("Vomit").gameObject;
	}

	public override void LateDangerUpdate()
	{
		if (dangerState != eDangerObjectState.run || bombLeft <= 0)
		{
			return;
		}
		float num = scaleTransformBound - (base.transform.position.x + spiderBehaviour.sincAnimationOffset);
		if (num <= 0f)
		{
			scaleTransformBound += spiderBehaviour.bombDropStep;
		}
		else
		{
			if (!(num < scaleTransformDistance))
			{
				return;
			}
			num = 1f - num / scaleTransformDistance;
			SpiderScaleEntity[] array = scaleTransforms;
			foreach (SpiderScaleEntity spiderScaleEntity in array)
			{
				float num2 = num - spiderScaleEntity.scalePhaseBegin;
				if (!(num2 <= 0f) && !(num2 >= spiderScaleEntity.scalePhaseLength))
				{
					float f = num2 / spiderScaleEntity.scalePhaseLength * (float)Math.PI;
					float num3 = 1f + (spiderScaleEntity.scaleFactor - 1f) * Mathf.Sin(f);
					spiderScaleEntity.refTransform.localScale *= num3;
				}
			}
		}
	}

	public override void DangerUpdate()
	{
		if (dangerState == eDangerObjectState.inactive)
		{
			return;
		}
		switch (dangerState)
		{
		case eDangerObjectState.run:
		{
			float num = spiderBehaviour.movementSpeed * Time.deltaTime;
			switch (spiderBehaviour.walkMovementStyle)
			{
			case eDangerMovementStyle.keepPlayerDistance:
				distanceFromTarget -= num;
				MoveToPos(new Vector3(targetController.Position.x + distanceFromTarget, base.transform.position.y, base.transform.position.z));
				break;
			case eDangerMovementStyle.constantSpeed:
				MoveToPos(new Vector3(base.transform.position.x + num, base.transform.position.y, base.transform.position.z));
				break;
			case eDangerMovementStyle.keepPlayerDistanceFromDistance:
				if (distanceFromTarget > spiderBehaviour.keepDistanceStart)
				{
					MoveToPos(new Vector3(base.transform.position.x + spiderBehaviour.beforeKeepDistanceSpeed * Time.deltaTime, base.transform.position.y, base.transform.position.z));
					break;
				}
				distanceFromTarget -= num;
				MoveToPos(new Vector3(targetController.Position.x + distanceFromTarget, base.transform.position.y, base.transform.position.z));
				break;
			}
			livedTime += Time.deltaTime;
			if (bombLeft > 0 && base.transform.position.x + GameSettings.gameSettings.dangers.spider.bombOffsetX >= dropBombBound)
			{
				Vector3 pos = default;
				pos.x = dropBombBound;
				pos.y = PathTimeline.instance.GetHeightByX(dropBombBound);
				pos.z = GameSettings.gameSettings.dangers.spider.bombPosZ;
				DangerObject.CreateTempInstance(GameSettings.gameSettings.dangers.spider.bombPrefab, pos, Quaternion.identity);
				dropBombBound += spiderBehaviour.bombDropStep;
				bombLeft--;
			}
			break;
		}
		case eDangerObjectState.turnToPlayer:
			MoveToPos(base.transform.position + new Vector3(GameSettings.gameSettings.dangers.spider.prepareHorizontalSpeed * Time.deltaTime, 0f, 0f));
			break;
		case eDangerObjectState.hide:
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				SetObjectState(eDangerObjectState.inactive);
			}
			break;
		case eDangerObjectState.prepare:
			switch (spiderBehaviour.backMovementStyle)
			{
			case eDangerMovementStyle.keepPlayerDistance:
				MoveToPos(base.transform.position + new Vector3(GameSettings.gameSettings.dangers.spider.prepareHorizontalSpeed * Time.deltaTime, 0f, 0f));
				break;
			case eDangerMovementStyle.constantSpeed:
			{
				float num = spiderBehaviour.movementSpeed * Time.deltaTime;
				MoveToPos(new Vector3(base.transform.position.x + num, base.transform.position.y, base.transform.position.z));
				break;
			}
			}
			break;
		}
		distanceFromTarget = base.transform.position.x - targetController.Position.x;
		switch (dangerState)
		{
		case eDangerObjectState.attack:
		case eDangerObjectState.turnToPlayer:
		case eDangerObjectState.attackAlternative:
		case eDangerObjectState.prepare:
		case eDangerObjectState.run:
			if (distanceFromTarget <= GameSettings.gameSettings.dangers.spider.vulnerabilityDistance)
			{
				targetController.OnEnterToVulnerabilityZone(this);
			}
			break;
		}
		switch (dangerState)
		{
		case eDangerObjectState.run:
		{
			if (distanceFromTarget <= GameSettings.gameSettings.dangers.spider.turnDistance)
			{
				SetObjectState(eDangerObjectState.turnToPlayer);
				break;
			}
			if (livedTime >= spiderBehaviour.walkBoundTime)
			{
				SetObjectState(eDangerObjectState.turnToPlayer);
				break;
			}
			if (livedTime >= spiderBehaviour.visibleTime)
			{
				SetObjectState(eDangerObjectState.hide);
				break;
			}
			float num2 = base.transform.position.x - startPosition.x;
			if (num2 > spiderBehaviour.walkBoundDistance)
			{
				SetObjectState(eDangerObjectState.turnToPlayer);
			}
			else if (num2 > spiderBehaviour.visibleDistance)
			{
				SetObjectState(eDangerObjectState.hide);
			}
			break;
		}
		case eDangerObjectState.turnToPlayer:
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				SetObjectState(eDangerObjectState.prepare);
			}
			break;
		case eDangerObjectState.prepare:
		case eDangerObjectState.ambush:
			if (distanceFromTarget <= GameSettings.gameSettings.dangers.spider.attackDistance)
			{
				switch (LevelAttributes.instance.PlayerSkin)
				{
				case EPlayerSkin.Man:
				case EPlayerSkin.Boar:
					SetObjectState(eDangerObjectState.attack);
					break;
				default:
					SetObjectState(eDangerObjectState.attackAlternative);
					break;
				}
			}
			break;
		case eDangerObjectState.attack:
		case eDangerObjectState.attackAlternative:
			if (timeLeft > 0f)
			{
				timeLeft -= Time.deltaTime;
			}
			if (timeLeft <= 0f)
			{
				targetController.OnEnterToDangerZone(this, (dangerState != eDangerObjectState.attack) ? playerDeath2Type : playerDeath1Type, eEnemyAttackStyle.slightAttack);
			}
			break;
		}
		base.DangerUpdate();
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.spider;
	}

	public override float GetDangerLeft()
	{
		return startPosition.x + GameSettings.gameSettings.dangers.spider.leftBound;
	}

	public override float GetDangerLeftInEditor()
	{
		return base.transform.position.x + GameSettings.gameSettings.dangers.spider.leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		if (spiderBehaviour.startAmbush)
		{
			SetObjectState(eDangerObjectState.ambush);
		}
		else
		{
			SetObjectState(eDangerObjectState.run);
		}
	}

	public override void OnPlayerExit()
	{
		if ((dangerState == eDangerObjectState.run || dangerState == eDangerObjectState.hide) && !spiderBehaviour.startAmbush)
		{
			SetObjectState(eDangerObjectState.turnToPlayer);
		}
	}

	public override bool OnDangerWasAttack(bool isMightAttack)
	{
		SetObjectState(eDangerObjectState.destroy);
		return true;
	}

	public override void OnExitFromState(eDangerObjectState state)
	{
		if (state == eDangerObjectState.inactive)
		{
		}
	}

	private void SamplingAnimation()
	{
		if (spiderBehaviour.startAmbush)
		{
			refAnim.Play("ambush");
		}
		else
		{
			refAnim.Play("idle");
		}
		refAnim.Sample();
		refAnim.Stop();
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			SamplingAnimation();
			MoveToPos(startPosition);
			break;
		case eDangerObjectState.run:
			refAnim.Play("walk");
			livedTime = 0f;
			bombLeft = spiderBehaviour.bombQuantity;
			dropBombBound = base.transform.position.x + spiderBehaviour.bombDropFirst;
			scaleTransformBound = dropBombBound + scaleTransformOffset;
			distanceFromTarget = base.transform.position.x - targetController.Position.x;
			break;
		case eDangerObjectState.ambush:
			refAnim.Play("ambush");
			livedTime = 0f;
			bombLeft = 0;
			distanceFromTarget = base.transform.position.x - targetController.Position.x;
			break;
		case eDangerObjectState.hide:
			refAnim.CrossFade("hide", GameSettings.gameSettings.dangers.spider.animationFade);
			timeLeft = GameSettings.gameSettings.dangers.spider.disappearTime;
			break;
		case eDangerObjectState.attack:
			refAnim.CrossFade("attack1", GameSettings.gameSettings.dangers.spider.animationFade);
			timeLeft = GameSettings.gameSettings.dangers.spider.attackDamageDelay;
			break;
		case eDangerObjectState.destroy:
			refAnim.CrossFade("death", GameSettings.gameSettings.dangers.spider.animationFade);
			break;
		case eDangerObjectState.turnToPlayer:
			refAnim.CrossFade("turn", GameSettings.gameSettings.dangers.spider.animationFade);
			timeLeft = GameSettings.gameSettings.dangers.spider.turnTimeout;
			if (spiderBehaviour.slowMo.scale != 1f)
			{
				TimeController.StartSlowMo(spiderBehaviour.slowMo.scale, spiderBehaviour.slowMo.timeFadeIn, spiderBehaviour.slowMo.timeFixed, spiderBehaviour.slowMo.timeFadeOut, eSlowMoMode.normal);
			}
			break;
		case eDangerObjectState.attackAlternative:
			refAnim.CrossFade("attack2", GameSettings.gameSettings.dangers.spider.animationFade);
			timeLeft = GameSettings.gameSettings.dangers.spider.attackAltDamageDelay;
			if ((bool)vomitObject)
			{
				vomitObject.SetActiveRecursivelyCompat(state: true);
				vomitObject.GetComponent<Animation>().Play();
			}
			break;
		case eDangerObjectState.prepare:
			refAnim.CrossFade("prepare", GameSettings.gameSettings.dangers.spider.animationFade);
			break;
		case eDangerObjectState.wait:
		case eDangerObjectState.chase:
		case eDangerObjectState.teleport:
		case eDangerObjectState.maneuver:
			break;
		}
	}

	public override void OnResetDanger()
	{
		if ((bool)vomitObject)
		{
			vomitObject.SetActiveRecursivelyCompat(state: false);
		}
		SetObjectState(eDangerObjectState.inactive);
	}
}
