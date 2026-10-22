using System;
using UnityEngine;

public class EnemyTurnskin : DangerObject
{
	public enum eModelEntity
	{
		archer = 0,
		star = 1,
		teleportInArcher = 2,
		teleportInStar = 3,
		teleportOutArcher = 4,
		teleportOutStar = 5,
		commonDanger = 6,
		none = 7
	}

	[Serializable]
	public class ModelEntity
	{
		public string name;

		public GameObject obj;

		public float vulnerabilityDistance = 100f;

		public ModelEntity(string strID)
		{
			name = strID;
		}
	}

	public enum eTrackStyle
	{
		clamp = 0,
		loop = 1
	}

	public enum eTurnskinState
	{
		inactive = 0,
		archerWait = 1,
		archerAim = 2,
		archerShoot = 3,
		starWait = 4,
		starAttack = 5,
		teleportInArcher = 6,
		teleportInStar = 7,
		invisible = 8,
		teleportOutStar = 9,
		teleportOutArcher = 10,
		dangerVulnerable = 11,
		destroy = 12
	}

	public enum eMoveStyle
	{
		standing = 0,
		moveToNextPoint = 1,
		keepDistanceFromTarget = 2
	}

	[Serializable]
	public class TrackPoint
	{
		public string animationName = string.Empty;

		public eModelEntity modelId;

		public eTurnskinState state = eTurnskinState.archerAim;

		public Vector2 offset = Vector2.zero;

		public float distanceOut = -1f;

		public float timeOut = -1f;

		public bool animationOut;

		public Vector2 posOut = -Vector2.right;

		public eMoveStyle moveStyle;

		public float keepDistance = 400f;

		public float keepDistanceSpringiness = 2f;

		public bool syncAniByDist;

		public AudioClip sound;

		public float soundDelay;

		public float colliderRadius;

		public Transform colliderTied;
	}

	private class CurrentData
	{
		public int trackCurrent = -1;

		public Vector2 pos = Vector2.zero;

		public float timeLeft;

		public float distanceLeft;

		public bool waitAnimationEnd;

		public Vector2 posOut = -Vector2.right;

		public float timeMax;

		public eTurnskinState state;

		public eModelEntity currentModel = eModelEntity.none;

		public Animation ani;

		public string animationName = string.Empty;

		public AnimationState aniState;

		public eMoveStyle moveStyle;

		public float keepDistance;

		public bool syncAniByDist;

		public SphereCollider currentCollider;

		public Transform colliderTied;

		public bool skipCollider;
	}

	public float leftBound = 800f;

	public ModelEntity[] modelList = new ModelEntity[7]
	{
		new ModelEntity("Archer"),
		new ModelEntity("Star"),
		new ModelEntity("TeleportInArcher"),
		new ModelEntity("TeleportInStar"),
		new ModelEntity("TeleportOutArcher"),
		new ModelEntity("TeleportOutStar"),
		new ModelEntity("CommonDanger")
	};

	public eTrackStyle loopTrack;

	public TrackPoint[] trackPoints;

	public eDangerDeathType deathType;

	private CurrentData currentData = new CurrentData();

	private EnemyArcher.ArrowController arrow;

	private Transform jointArrow;

	private Transform jointBody;

	private Vector3 startPoint = Vector3.zero;

	private static string boneNameBody = "Archer/Root/Hips_Joint/Spine";

	private static string boneNameArrow = "Archer/Root/Hips_Joint/Spine/Root_Arm_n_Head/Arm_Left/Elbow_Left/Hand_Left/Bow/Arrow";

	private Transform locTransform;

	public override void DangerAwake()
	{
		locTransform = base.transform;
		startPoint = locTransform.position;
		jointBody = locTransform.Find(boneNameBody);
		jointArrow = locTransform.Find(boneNameArrow);
		if (jointBody == null && modelList[0].obj != null)
		{
			Debug.LogWarning("Warning! Cant find bone name " + boneNameBody + " into danger " + this);
		}
		if (jointArrow == null && modelList[0].obj != null)
		{
			Debug.LogWarning("Warning! Cant find bone name " + boneNameArrow + " into danger " + this);
		}
		dangerState = eDangerObjectState.unused;
		SetInternalState(eTurnskinState.inactive);
	}

	private Vector3 CalculateCurrentPosition()
	{
		Vector3 position = locTransform.position;
		int nextTrackPoint = GetNextTrackPoint(currentData.trackCurrent);
		if (nextTrackPoint != currentData.trackCurrent)
		{
			float num = 0f;
			if (currentData.timeLeft >= 0f)
			{
				float num2 = Mathf.Clamp01(1f - currentData.timeLeft / currentData.timeMax);
				if (num2 > num)
				{
					num = num2;
				}
			}
			Vector2 offset = trackPoints[nextTrackPoint].offset;
			if (currentData.posOut.x >= 0f)
			{
				offset += currentData.posOut;
			}
			position.x = currentData.pos.x + offset.x * num;
			position.y = currentData.pos.y + offset.y * num;
		}
		return position;
	}

	private void DestroyArrow()
	{
		arrow = null;
		if (targetController != null)
		{
			targetController.LockSkin(eLockSkin.none);
		}
	}

	public override void DangerUpdate()
	{
		if (dangerState == eDangerObjectState.inactive)
		{
			return;
		}
		if (refColliderTransform != null && currentData.currentModel == eModelEntity.commonDanger && refSphereCollider != null)
		{
			refSphereCollider.center = base.transform.InverseTransformPoint(refColliderTransform.position);
		}
		if (currentData.currentCollider != null && currentData.colliderTied != null)
		{
			currentData.currentCollider.center = base.transform.InverseTransformPoint(currentData.colliderTied.transform.position);
		}
		if (currentData.skipCollider)
		{
			currentData.skipCollider = false;
		}
		if (arrow != null)
		{
			if (targetController.Position.x > base.transform.position.x)
			{
				DestroyArrow();
			}
			else if (currentData.state == eTurnskinState.archerShoot && arrow.GetArrowState() == EnemyArcher.ArrowController.eArrowState.fly && arrow.IsIntersectPlaneX(targetController.Position.x))
			{
				if (targetController.OnEnterToDangerZone(this, PlayerDeathType.impact, eEnemyAttackStyle.slightAttack))
				{
					arrow.MirrorDirection();
				}
				else
				{
					Transform transform = targetController.FindBone(EnemyArcher.boneNameTargetArrow);
					if (transform == null)
					{
						Debug.LogWarning("Warning! Cant find bone name " + EnemyArcher.boneNameTargetArrow + " into target model " + targetController);
					}
					arrow.TieToObject(transform, 1f);
				}
			}
		}
		TrackPoint trackPoint = trackPoints[currentData.trackCurrent];
		switch (currentData.moveStyle)
		{
		case eMoveStyle.moveToNextPoint:
			MoveToPos(CalculateCurrentPosition());
			break;
		case eMoveStyle.keepDistanceFromTarget:
		{
			currentData.keepDistance = Mathf.Lerp(currentData.keepDistance, trackPoint.keepDistance, Time.deltaTime * trackPoint.keepDistanceSpringiness);
			Vector2 vector = new Vector2(targetController.Position.x + currentData.keepDistance, currentData.pos.y);
			if (currentData.posOut.x > 0f && currentData.posOut.y != 0f)
			{
				vector.y += (vector.x - currentData.pos.x) / currentData.posOut.x * currentData.posOut.y;
			}
			MoveToPos(new Vector3(vector.x, vector.y, locTransform.position.z));
			break;
		}
		}
		if (currentData.syncAniByDist && currentData.posOut.x > 0f)
		{
			currentData.aniState.normalizedTime = Mathf.Clamp01((locTransform.position.x - currentData.pos.x) / currentData.posOut.x);
		}
		if (ConditionOfChangeTrackPoint())
		{
			ChangeTrackPoint(GetNextTrackPoint(currentData.trackCurrent));
		}
		bool flag = false;
		float num = 0f;
		switch (currentData.state)
		{
		case eTurnskinState.archerWait:
		case eTurnskinState.archerAim:
		case eTurnskinState.archerShoot:
			flag = true;
			num = GameSettings.gameSettings.dangers.archer.vulnerabilityDistance;
			break;
		case eTurnskinState.dangerVulnerable:
			flag = true;
			num = modelList[(int)currentData.currentModel].vulnerabilityDistance;
			break;
		}
		if (flag && locTransform.position.x - targetController.Position.x <= num)
		{
			targetController.OnEnterToVulnerabilityZone(this);
		}
		base.DangerUpdate();
	}

	public override void LateDangerUpdate()
	{
		if (currentData.state == eTurnskinState.archerAim)
		{
			Vector3 eulerAngles = Quaternion.FromToRotation(-Vector3.right, targetController.Position - locTransform.position).eulerAngles;
			jointBody.Rotate(eulerAngles, Space.World);
		}
		if (arrow != null && !arrow.Update())
		{
			DestroyArrow();
		}
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.archer;
	}

	public override float GetDangerLeft()
	{
		return startPoint.x - leftBound;
	}

	public override float GetDangerLeftInEditor()
	{
		return base.transform.position.x - leftBound;
	}

	public override bool OnPlayerReenter()
	{
		MoveToPos(startPoint);
		if (currentData.trackCurrent != 0)
		{
			ChangeTrackPoint(0);
		}
		currentData.ani.Stop();
		currentData.ani.Play(currentData.animationName);
		return true;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		ChangeTrackPoint(0);
	}

	public override void OnPlayerExit()
	{
		if (currentData.moveStyle == eMoveStyle.keepDistanceFromTarget)
		{
			currentData.moveStyle = eMoveStyle.moveToNextPoint;
			currentData.timeMax = currentData.aniState.length;
			currentData.timeLeft = currentData.aniState.length - currentData.aniState.time;
		}
	}

	private void ChangeTrackPoint(int nTrackPoint)
	{
		if (currentData.trackCurrent == nTrackPoint)
		{
			return;
		}
		if (currentData.posOut.x > 0f)
		{
			currentData.pos += currentData.posOut;
		}
		currentData.trackCurrent = nTrackPoint;
		TrackPoint trackPoint = trackPoints[nTrackPoint];
		SetInternalState(trackPoint.state);
		if (currentData.currentModel != trackPoint.modelId)
		{
			ChangeSkin(trackPoint.modelId);
		}
		if (currentData.animationName != trackPoint.animationName && !string.IsNullOrEmpty(trackPoint.animationName))
		{
			currentData.animationName = trackPoint.animationName;
			currentData.aniState = currentData.ani[trackPoint.animationName];
			if (currentData.ani.isPlaying)
			{
				currentData.aniState.time = 0f;
				currentData.ani.CrossFade(trackPoint.animationName, 0.2f);
			}
			else
			{
				currentData.ani.Play(trackPoint.animationName);
			}
		}
		currentData.distanceLeft = trackPoint.distanceOut;
		currentData.waitAnimationEnd = trackPoint.animationOut;
		currentData.timeMax = trackPoint.timeOut;
		currentData.syncAniByDist = trackPoint.syncAniByDist;
		if (trackPoint.animationOut)
		{
			AnimationState animationState = currentData.ani[trackPoint.animationName];
			if (animationState.length - animationState.time < currentData.timeMax)
			{
				currentData.timeMax = animationState.length - animationState.time;
			}
		}
		currentData.timeLeft = currentData.timeMax;
		currentData.posOut = trackPoint.posOut;
		if (nTrackPoint == 0)
		{
			currentData.pos = new Vector2(startPoint.x, startPoint.y);
		}
		currentData.pos += trackPoint.offset;
		MoveToPos(new Vector3(currentData.pos.x, currentData.pos.y, locTransform.position.z));
		if (trackPoint.moveStyle == eMoveStyle.keepDistanceFromTarget)
		{
			currentData.keepDistance = currentData.pos.x - targetController.Position.x;
		}
		currentData.moveStyle = trackPoint.moveStyle;
		if ((bool)trackPoint.sound)
		{
			SoundEnvironment.Launch3DSound(trackPoint.sound, 1f, base.transform.position, trackPoint.soundDelay, isLoop: false, null);
		}
		if (trackPoint.colliderRadius <= 0f)
		{
			if (currentData.currentCollider != null)
			{
				UnityEngine.Object.Destroy(currentData.currentCollider);
				currentData.currentCollider = null;
				currentData.colliderTied = null;
			}
			return;
		}
		currentData.skipCollider = true;
		if (currentData.currentCollider == null)
		{
			currentData.currentCollider = base.gameObject.AddComponent<SphereCollider>();
		}
		currentData.currentCollider.isTrigger = true;
		currentData.currentCollider.radius = trackPoint.colliderRadius;
		currentData.colliderTied = trackPoint.colliderTied;
		if ((bool)trackPoint.colliderTied)
		{
			currentData.currentCollider.center = base.transform.InverseTransformPoint(trackPoint.colliderTied.transform.position);
		}
		else
		{
			currentData.currentCollider.center = Vector3.zero;
		}
	}

	private int GetNextTrackPoint(int nTrackPoint)
	{
		int num = nTrackPoint + 1;
		if (num >= trackPoints.Length)
		{
			num = ((loopTrack != eTrackStyle.loop) ? (trackPoints.Length - 1) : 0);
		}
		return num;
	}

	private bool ConditionOfChangeTrackPoint()
	{
		if (currentData.timeLeft >= 0f)
		{
			currentData.timeLeft -= Time.deltaTime;
			if (currentData.timeLeft <= 0f)
			{
				return true;
			}
		}
		if (currentData.distanceLeft != -1f)
		{
			float num = locTransform.position.x - targetController.Position.x;
			if (num <= currentData.distanceLeft)
			{
				return true;
			}
		}
		if (currentData.waitAnimationEnd && !currentData.ani.IsPlaying(currentData.animationName))
		{
			return true;
		}
		if (currentData.posOut.x >= 0f && locTransform.position.x - currentData.pos.x >= currentData.posOut.x)
		{
			MoveToPos(new Vector3(currentData.pos.x, currentData.pos.y, locTransform.position.z));
			return true;
		}
		return false;
	}

	private void ChangeSkin(eModelEntity newSkin)
	{
		if (newSkin == currentData.currentModel)
		{
			return;
		}
		if (currentData.currentModel != eModelEntity.none)
		{
			modelList[(int)currentData.currentModel].obj.SetActiveRecursivelyCompat(state: false);
			modelList[(int)currentData.currentModel].obj.GetComponent<Animation>().enabled = false;
			if (currentData.currentModel == eModelEntity.archer)
			{
				DestroyArrow();
			}
		}
		currentData.currentModel = newSkin;
		if (newSkin == eModelEntity.none)
		{
			currentData.ani = null;
			currentData.animationName = string.Empty;
			currentData.aniState = null;
			return;
		}
		modelList[(int)newSkin].obj.SetActiveRecursivelyCompat(state: true);
		currentData.ani = modelList[(int)newSkin].obj.GetComponent<Animation>();
		currentData.ani.enabled = true;
		currentData.ani.Stop();
		currentData.animationName = string.Empty;
		currentData.aniState = null;
	}

	public override bool OnDangerWasAttack(bool isMightAttack)
	{
		SetInternalState(eTurnskinState.destroy);
		return true;
	}

	public override void OnEnterToState(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.destroy:
			DestroyArrow();
			if (deathType == eDangerDeathType.alternative)
			{
				currentData.ani.CrossFade("deathAlt", 0.2f);
			}
			else
			{
				currentData.ani.CrossFade("death", 0.2f);
			}
			currentData.timeLeft = 3f;
			currentData.distanceLeft = -1f;
			currentData.waitAnimationEnd = false;
			currentData.posOut.x = -1f;
			currentData.moveStyle = eMoveStyle.standing;
			break;
		case eDangerObjectState.inactive:
			DestroyArrow();
			SampleDanger();
			ChangeSkin(eModelEntity.none);
			currentData.trackCurrent = -1;
			break;
		case eDangerObjectState.wait:
		case eDangerObjectState.attack:
			break;
		}
	}

	private void SampleDanger()
	{
		ChangeSkin(trackPoints[0].modelId);
		eModelEntity modelId = trackPoints[0].modelId;
		if (modelList[(int)modelId].obj != null)
		{
			Animation animation = modelList[(int)modelId].obj.GetComponent<Animation>();
			if (animation != null)
			{
				animation.Play(trackPoints[0].animationName);
				animation.Sample();
				animation.Stop();
			}
		}
	}

	public override void DoVisible(bool makeVisible)
	{
		if (makeVisible)
		{
			base.gameObject.SetActiveRecursivelyCompat(state: true);
			eModelEntity modelId = trackPoints[0].modelId;
			for (int i = 0; i < modelList.Length; i++)
			{
				if (i == (int)modelId)
				{
					if ((bool)modelList[i].obj)
					{
						modelList[i].obj.GetComponent<Animation>().enabled = true;
					}
				}
				else if ((bool)modelList[i].obj)
				{
					modelList[i].obj.SetActiveRecursivelyCompat(state: false);
					modelList[i].obj.GetComponent<Animation>().enabled = false;
				}
			}
			return;
		}
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		for (int i = 0; i < modelList.Length; i++)
		{
			if ((bool)modelList[i].obj)
			{
				modelList[i].obj.GetComponent<Animation>().enabled = false;
			}
		}
	}

	public override void OnResetDanger()
	{
		locTransform.position = startPoint;
		SetInternalState(eTurnskinState.inactive);
	}

	private void SetInternalState(eTurnskinState newSubState)
	{
		switch (newSubState)
		{
		case eTurnskinState.inactive:
			SetObjectState(eDangerObjectState.inactive);
			if (currentData.currentCollider != null)
			{
				UnityEngine.Object.Destroy(currentData.currentCollider);
				currentData.currentCollider = null;
				currentData.colliderTied = null;
			}
			break;
		case eTurnskinState.archerWait:
			if (dangerState != eDangerObjectState.wait)
			{
				SetObjectState(eDangerObjectState.wait);
			}
			if (arrow == null)
			{
				arrow = new EnemyArcher.ArrowController();
			}
			arrow.TieToObject(jointArrow, 10f);
			break;
		case eTurnskinState.archerAim:
			if (dangerState != eDangerObjectState.attack)
			{
				SetObjectState(eDangerObjectState.attack);
			}
			if (arrow == null)
			{
				arrow = new EnemyArcher.ArrowController();
			}
			arrow.TieToObject(jointArrow, 5f);
			break;
		case eTurnskinState.archerShoot:
		{
			if (dangerState != eDangerObjectState.attack)
			{
				SetObjectState(eDangerObjectState.attack);
			}
			if (arrow == null)
			{
				arrow = new EnemyArcher.ArrowController();
			}
			Vector3 position = jointArrow.position;
			Vector3 position2 = targetController.Position;
			position2.y += 120f;
			position2 -= position;
			position2.Normalize();
			arrow.StartFly(position, position2);
			if (targetController != null)
			{
				targetController.LockSkin(eLockSkin.boar);
			}
			break;
		}
		case eTurnskinState.starWait:
			SetObjectState(eDangerObjectState.wait);
			break;
		case eTurnskinState.starAttack:
			SetObjectState(eDangerObjectState.attack);
			break;
		case eTurnskinState.teleportInArcher:
			SetObjectState(eDangerObjectState.teleport);
			break;
		case eTurnskinState.teleportInStar:
			SetObjectState(eDangerObjectState.teleport);
			break;
		case eTurnskinState.invisible:
			SetObjectState(eDangerObjectState.wait);
			break;
		case eTurnskinState.teleportOutArcher:
			SetObjectState(eDangerObjectState.teleport);
			break;
		case eTurnskinState.teleportOutStar:
			SetObjectState(eDangerObjectState.teleport);
			break;
		case eTurnskinState.destroy:
			SetObjectState(eDangerObjectState.destroy);
			break;
		}
		currentData.state = newSubState;
	}

	public void OnTriggerEnter(Collider other)
	{
		if ((currentData.state != eTurnskinState.starWait && currentData.state != eTurnskinState.starAttack) || currentData.skipCollider)
		{
			return;
		}
		if (targetController != null && targetController.HaveCollider(other))
		{
			targetController.OnEnterToDangerZone(this, PlayerDeathType.thorn, eEnemyAttackStyle.nonBreakable);
			targetController.OnExitFromDangerZone(this);
			currentData.timeLeft = -1f;
			currentData.distanceLeft = -1f;
			currentData.waitAnimationEnd = false;
			currentData.posOut.x = -1f;
			currentData.moveStyle = eMoveStyle.standing;
			if (currentData.ani["attack"] != null)
			{
				currentData.ani.CrossFade("attack");
			}
		}
		else
		{
			Debug.LogWarning(string.Concat("Other collider ", other, " entered instead target ", targetController));
		}
	}
}
