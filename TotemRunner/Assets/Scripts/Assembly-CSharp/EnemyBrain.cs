using System;
using UnityEngine;

public class EnemyBrain : DangerObject
{
	[Serializable]
	public class TeleportDescribe
	{
		public float turnOnDistance = 1000f;

		public float teleportXOffset = 600f;

		public float teleportYPosition = 500f;
	}

	private float chaseDistance;

	private Vector3 startPosition;

	private Transform colliderTieBone;

	private SphereCollider sphereCollider;

	private bool isOnBadGrass;

	private GameObject teleportInObject;

	private GameObject teleportOutObject;

	private GameObject modelObject;

	private float timeTeleport;

	private bool teleportInActive;

	private bool teleportOutActive;

	private float timeLeft = -1f;

	public PlayerDeathType deathStandart = PlayerDeathType.impact;

	public PlayerDeathType deathAlternative = PlayerDeathType.impact;

	private static float timeoutDeath = 2f;

	public TeleportDescribe[] teleports;

	private int currentTeleport = -1;

	public override void DangerAwake()
	{
		startPosition = base.transform.position;
		sphereCollider = base.GetComponent<Collider>() as SphereCollider;
		Transform transform = base.transform.Find("TeleportIn");
		teleportInObject = ((!(transform != null)) ? null : transform.gameObject);
		transform = base.transform.Find("TeleportOut");
		teleportOutObject = ((!(transform != null)) ? null : transform.gameObject);
		modelObject = base.transform.Find("Brain_Monster").gameObject;
		colliderTieBone = refAnim.transform.Find(GameSettings.gameSettings.dangers.brain.colliderTieBone);
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.inactive);
		isOnBadGrass = false;
	}

	public override void DangerUpdate()
	{
		if (dangerState == eDangerObjectState.inactive)
		{
			return;
		}
		EnemyBrainParams brain = GameSettings.gameSettings.dangers.brain;
		if (timeLeft >= 0f)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				SetObjectState(eDangerObjectState.inactive);
				return;
			}
		}
		float num = base.transform.position.x - startPosition.x;
		switch (dangerState)
		{
		case eDangerObjectState.wait:
			MoveToPos(base.transform.position + new Vector3(brain.horizontalWaitSpeed * Time.deltaTime, 0f, 0f));
			break;
		case eDangerObjectState.maneuver:
			MoveToPos(new Vector3(base.transform.position.x + brain.horizontalWaitSpeed * Time.deltaTime, Mathf.Lerp(base.transform.position.y, targetController.Position.y + brain.verticalOffset, Time.deltaTime * brain.verticalSpringinessInManeuver), base.transform.position.z));
			break;
		case eDangerObjectState.chase:
		case eDangerObjectState.teleport:
			chaseDistance -= Time.deltaTime * brain.horizontalRelativeSpeed;
			MoveToPos(new Vector3(targetController.Position.x - chaseDistance, Mathf.Lerp(base.transform.position.y, targetController.Position.y + brain.verticalOffset, Time.deltaTime * brain.verticalSpringiness), base.transform.position.z));
			break;
		}
		sphereCollider.center = base.transform.InverseTransformPoint(colliderTieBone.position);
		float num2 = base.transform.position.x - startPosition.x;
		if (dangerState != eDangerObjectState.teleport && teleports.Length > 0)
		{
			for (currentTeleport = 0; currentTeleport < teleports.Length; currentTeleport++)
			{
				if (num < teleports[currentTeleport].turnOnDistance && num2 >= teleports[currentTeleport].turnOnDistance)
				{
					SetObjectState(eDangerObjectState.teleport);
					break;
				}
			}
		}
		switch (dangerState)
		{
		case eDangerObjectState.wait:
		case eDangerObjectState.chase:
		case eDangerObjectState.maneuver:
		{
			Collider[] array = Physics.OverlapSphere(colliderTieBone.position, sphereCollider.radius);
			if (array.Length <= 0)
			{
				break;
			}
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (collider == sphereCollider || targetController.HaveCollider(collider))
				{
					continue;
				}
				DangerObject component = collider.gameObject.GetComponent<DangerObject>();
				if (component == null)
				{
					continue;
				}
				if (component.GetDangerType() == eDangerType.brain)
				{
					Vector3 vector = base.transform.position - component.transform.position;
					if (vector.magnitude < brain.otherBrainPushRadius)
					{
						vector = vector.normalized * ((vector.magnitude - brain.otherBrainPushRadius) * 0.5f);
						MoveToPos(base.transform.position - vector);
						component.MoveToPos(component.transform.position + vector);
					}
				}
				else
				{
					SetObjectState(eDangerObjectState.destroy);
				}
			}
			break;
		}
		}
		float num3 = DangerHelper.DistanceFromObject(targetController, base.transform.position.x);
		switch (dangerState)
		{
		case eDangerObjectState.wait:
		case eDangerObjectState.maneuver:
			if (num3 <= brain.chaseDistance)
			{
				SetObjectState(eDangerObjectState.chase);
			}
			break;
		case eDangerObjectState.chase:
		{
			Vector2 vector2 = new Vector2(chaseDistance, base.transform.position.y - targetController.Position.y);
			float magnitude = vector2.magnitude;
			if (magnitude <= brain.catchDistance)
			{
				StartAttack();
			}
			break;
		}
		case eDangerObjectState.attack:
			targetController.OnEnterToDangerZone(this, deathStandart, eEnemyAttackStyle.nonBreakable);
			break;
		case eDangerObjectState.attackAlternative:
			targetController.OnEnterToDangerZone(this, deathAlternative, eEnemyAttackStyle.nonBreakable);
			break;
		case eDangerObjectState.teleport:
		{
			float num4 = timeTeleport;
			timeTeleport += Time.deltaTime;
			if (timeTeleport >= brain.teleportInDelay && !teleportInActive)
			{
				teleportInActive = true;
				teleportInObject.SetActiveRecursivelyCompat(state: true);
				teleportInObject.GetComponent<Animation>().Stop();
				teleportInObject.GetComponent<Animation>().Play("action");
			}
			if (timeTeleport >= brain.teleportOutDelay)
			{
				if (!teleportOutActive)
				{
					teleportOutActive = true;
					teleportOutObject.SetActiveRecursivelyCompat(state: true);
					teleportOutObject.GetComponent<Animation>().Stop();
					teleportOutObject.GetComponent<Animation>().Play("action");
				}
				teleportOutObject.transform.position = new Vector3(base.transform.position.x + teleports[currentTeleport].teleportXOffset, teleports[currentTeleport].teleportYPosition, startPosition.z);
			}
			if (timeTeleport >= brain.teleportModelFadeOutTime)
			{
				if (num4 < brain.teleportModelFadeOutTime)
				{
					refAnim.Play("maneuver");
				}
				modelObject.transform.position = teleportOutObject.transform.position;
			}
			if (timeTeleport > brain.teleportModelFadeInTime)
			{
				float num5 = 1f;
				num5 = ((timeTeleport > brain.teleportModelFadeOutTime) ? Mathf.Clamp01((timeTeleport - brain.teleportModelFadeOutTime) / brain.teleportModelFadeTime) : ((!(timeTeleport < brain.teleportModelFadeInTime + brain.teleportModelFadeTime)) ? 0f : Mathf.Clamp01(1f - (timeTeleport - brain.teleportModelFadeOutTime) / brain.teleportModelFadeTime)));
				modelObject.transform.localScale = new Vector3(num5, num5, num5);
			}
			if (timeTeleport >= brain.teleportTimeLength)
			{
				teleportInObject.SetActiveRecursivelyCompat(state: false);
				teleportOutObject.SetActiveRecursivelyCompat(state: false);
				MoveToPos(teleportOutObject.transform.position);
				modelObject.transform.position = base.transform.position;
				modelObject.transform.localScale = Vector3.one;
				SetObjectState(eDangerObjectState.maneuver);
			}
			break;
		}
		}
		eDangerObjectState stateSnapshot = dangerState;
		if (stateSnapshot == eDangerObjectState.chase && ScrollingCamera.instance.zooming.zoomCurrent != BaseCamera.ECameraZoomingType.grassGrowing)
		{
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.grassGrowing);
		}
		base.DangerUpdate();
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.brain;
	}

	public override float GetDangerLeft()
	{
		return startPosition.x + GameSettings.gameSettings.dangers.brain.leftBound;
	}

	public override bool OnPlayerReenter()
	{
		if (targetController == null)
		{
			return false;
		}
		refAnim.Stop();
		if (targetController.Position.x < startPosition.x)
		{
			MoveToPos(startPosition);
			if (targetController.Position.x >= GetDangerLeft())
			{
				SetObjectState(eDangerObjectState.wait);
			}
		}
		else
		{
			if (dangerState == eDangerObjectState.inactive && targetController.Position.x > base.transform.position.x)
			{
				return false;
			}
			MoveToPos(targetController.Position + new Vector3(GameSettings.gameSettings.dangers.brain.chaseDistance, 0f, 0f));
			SetObjectState(eDangerObjectState.chase);
		}
		return true;
	}

	public override float GetDangerLeftInEditor()
	{
		GameSettings.InitForEditor();
		return base.transform.position.x + GameSettings.gameSettings.dangers.brain.leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		SetObjectState(eDangerObjectState.wait);
	}

	public override void OnExitFromState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.chase:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			if (isOnBadGrass)
			{
				GrassGifManager.TurnOffBadGrass();
				isOnBadGrass = false;
			}
			break;
		case eDangerObjectState.teleport:
			modelObject.transform.localScale = Vector3.one;
			break;
		}
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.inactive:
			refAnim.Play("idle");
			refAnim.Sample();
			refAnim.Stop();
			timeLeft = -1f;
			break;
		case eDangerObjectState.wait:
			refAnim.Play("idle");
			break;
		case eDangerObjectState.chase:
			chaseDistance = targetController.Position.x - base.transform.position.x;
			refAnim.CrossFade("fly");
			if (!isOnBadGrass)
			{
				GrassGifManager.TurnOnBadGrass();
				isOnBadGrass = true;
			}
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.grassGrowing);
			break;
		case eDangerObjectState.attack:
			refAnim.Play("death");
			timeLeft = timeoutDeath;
			break;
		case eDangerObjectState.attackAlternative:
			refAnim.CrossFade("attackAlt");
			CameraShake.instance.LaunchShake(ECameraShakeType.death, 1f);
			timeLeft = timeoutDeath;
			break;
		case eDangerObjectState.destroy:
			refAnim.Play("death");
			CameraShake.instance.LaunchShake(ECameraShakeType.death, 1f);
			timeLeft = timeoutDeath;
			break;
		case eDangerObjectState.teleport:
			refAnim.CrossFade("fly");
			chaseDistance = targetController.Position.x - base.transform.position.x;
			timeTeleport = 0f;
			teleportInActive = false;
			teleportOutActive = false;
			break;
		case eDangerObjectState.maneuver:
			break;
		case eDangerObjectState.turnToPlayer:
		case eDangerObjectState.hide:
		case eDangerObjectState.prepare:
			break;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		switch (dangerState)
		{
		case eDangerObjectState.inactive:
		case eDangerObjectState.attack:
		case eDangerObjectState.destroy:
		case eDangerObjectState.attackAlternative:
		case eDangerObjectState.teleport:
			return;
		}
		if (targetController != null && targetController.HaveCollider(other))
		{
			StartAttack();
		}
		else if (targetController == null)
		{
			targetController = other.GetComponent<PlayerController>();
			if (targetController != null)
			{
				StartAttack();
				return;
			}
			Debug.LogWarning(string.Concat("Other collider enter! ", other, "(", other.gameObject, ") instead ", targetController));
		}
	}

	private void StartAttack()
	{
		if (dangerState == eDangerObjectState.maneuver)
		{
			SetObjectState(eDangerObjectState.attackAlternative);
		}
		else
		{
			SetObjectState(eDangerObjectState.attack);
		}
	}

	public override void OnResetDanger()
	{
		base.transform.position = startPosition;
		SetObjectState(eDangerObjectState.inactive);
	}

	public override void DoVisible(bool makeVisible)
	{
		base.DoVisible(makeVisible);
		if (makeVisible)
		{
			if ((bool)teleportInObject)
			{
				teleportInObject.SetActiveRecursivelyCompat(state: false);
			}
			if ((bool)teleportOutObject)
			{
				teleportOutObject.SetActiveRecursivelyCompat(state: false);
			}
		}
		if ((bool)modelObject)
		{
			modelObject.GetComponent<Animation>().enabled = makeVisible;
		}
		if ((bool)teleportInObject)
		{
			teleportInObject.GetComponent<Animation>().enabled = makeVisible;
		}
		if ((bool)teleportOutObject)
		{
			teleportOutObject.GetComponent<Animation>().enabled = makeVisible;
		}
		if (!makeVisible && isOnBadGrass)
		{
			GrassGifManager.TurnOffBadGrass();
			isOnBadGrass = false;
		}
	}
}
