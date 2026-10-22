using System;
using UnityEngine;

public class DangerObject : MonoBehaviour, SortableByInt
{
	public enum eDangerObjectState
	{
		inactive = 0,
		wait = 1,
		attack = 2,
		destroy = 3,
		chase = 4,
		turnToPlayer = 5,
		attackAlternative = 6,
		hide = 7,
		prepare = 8,
		teleport = 9,
		maneuver = 10,
		run = 11,
		ambush = 12,
		unused = -1
	}

	public enum eDangerDeathType
	{
		usual = 0,
		alternative = 1
	}

	public enum eEnemyAttackStyle
	{
		slightAttack = 0,
		mightAttack = 1,
		nonBreakable = 2
	}

	public enum eDangerType
	{
		architecture = 0,
		obstacle = 1,
		movingObstacle = 2,
		brain = 3,
		bomb = 4,
		archer = 5,
		fatman = 6,
		spider = 7,
		barrier = 8,
		poacher = 9,
		unknown = 10
	}

	public enum eDangerMovementStyle
	{
		keepPlayerDistance = 0,
		constantSpeed = 1,
		standing = 2,
		keepPlayerDistanceFromDistance = 3
	}

	[Serializable]
	public class SoundDescriber
	{
		public AudioClip sound;

		public bool loop;

		public float delay;
	}

	[Serializable]
	public class SoundsList
	{
		public SoundDescriber wait;

		public SoundDescriber attack;

		public SoundDescriber destroy;

		public SoundDescriber chase;

		public SoundDescriber chase2;

		public SoundDescriber turnToPlayer;

		public SoundDescriber attackAlternative;

		public SoundDescriber hide;

		public SoundDescriber prepare;

		public SoundDescriber teleport;

		public SoundDescriber maneuver;

		public SoundDescriber run;

		public SoundDescriber ambush;
	}

	[Serializable]
	public class DangerObjectEvents
	{
		public ExecutorsTimeFollowList death;

		public ExecutorsTimeFollowList attack;

		public void Update()
		{
			death.Update();
			attack.Update();
		}
	}

	public SoundsList sounds;

	public bool isHardOnly;

	public int posGrid;

	public Transform refModelPrefab;

	protected Transform refModel;

	public Animation refAnim;

	public Transform refColliderTransform;

	protected SphereCollider refSphereCollider;

	protected ITargetController targetController;

	private bool isTemporaryDanger;

	[NonSerialized]
	public int updateFrame;

	public DangerObjectEvents events;

	protected eDangerObjectState dangerState;

	protected int dangerIndex = -1;

	public void SetTargetController(PlayerController pc)
	{
		targetController = pc;
	}

	public virtual int GetOrder()
	{
		return posGrid;
	}

	public int CalculateGrid()
	{
		return Mathf.CeilToInt(base.transform.position.x * DangerHelper.gridScale);
	}

	public void MoveToPos(Vector3 newPos)
	{
		base.transform.position = newPos;
		int num = CalculateGrid();
		if (num != posGrid)
		{
			posGrid = num;
			DangerHelper.OnMoveDanger(this);
		}
	}

	public eDangerObjectState GetDangerState()
	{
		return dangerState;
	}

	public virtual void DangerAwake()
	{
		if ((bool)refColliderTransform)
		{
			refSphereCollider = base.GetComponent<Collider>() as SphereCollider;
		}
	}

	public virtual void DangerUpdate()
	{
		events.Update();
	}

	public virtual void LateDangerUpdate()
	{
	}

	public int GetDangerIndex()
	{
		return dangerIndex;
	}

	public virtual eDangerType GetDangerType()
	{
		return eDangerType.unknown;
	}

	public virtual PlayerDeathType GetPlayerDeathType()
	{
		return PlayerDeathType.fall;
	}

	public virtual bool IsTough()
	{
		return false;
	}

	public virtual bool PlayerAttackAnimationDontPlay()
	{
		return false;
	}

	public virtual void DoVisible(bool makeVisible)
	{
		base.gameObject.SetActiveRecursivelyCompat(makeVisible);
		if ((bool)refAnim)
		{
			refAnim.enabled = makeVisible;
		}
		if (isTemporaryDanger && !makeVisible)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void SetObjectState(eDangerObjectState state)
	{
		if (dangerState == state)
		{
			return;
		}
		SoundStop(dangerState, waitForMute: false);
		OnExitFromState(dangerState);
		OnEnterToState(state);
		SoundPlay(state);
		dangerState = state;
		switch (state)
		{
		case eDangerObjectState.destroy:
			events.death.StartExecutes();
			GameStatistic.instance.Increment(GameStatistic.eStatisticItem.kill);
			AchievementCollector.DangerDeath(this);
			if ((bool)WorldEvil.instance)
			{
				WorldEvil.instance.MovePause();
			}
			PlayerController.instance.boarBehaviour.speedupPauseLeft = PlayerController.instance.boarBehaviour.speedupPauseTimeout;
			break;
		case eDangerObjectState.attack:
			events.attack.StartExecutes();
			break;
		}
	}

	public void SoundPlay(eDangerObjectState state)
	{
		switch (state)
		{
		case eDangerObjectState.wait:
			if ((bool)sounds.wait.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.wait.sound, 1f, base.transform.position, sounds.wait.delay, sounds.wait.loop, (!sounds.wait.loop) ? null : base.transform);
			}
			break;
		case eDangerObjectState.attack:
			if ((bool)sounds.attack.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.attack.sound, 1f, base.transform.position, sounds.attack.delay, sounds.attack.loop, null);
			}
			break;
		case eDangerObjectState.destroy:
			if ((bool)sounds.destroy.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.destroy.sound, 1f, base.transform.position, sounds.destroy.delay, sounds.destroy.loop, null);
			}
			break;
		case eDangerObjectState.chase:
			if ((bool)sounds.chase.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.chase.sound, 1f, base.transform.position, sounds.chase.delay, sounds.chase.loop, base.transform);
			}
			if ((bool)sounds.chase2.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.chase2.sound, 1f, base.transform.position, sounds.chase2.delay, sounds.chase2.loop, null);
			}
			break;
		case eDangerObjectState.turnToPlayer:
			if ((bool)sounds.turnToPlayer.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.turnToPlayer.sound, 1f, base.transform.position, sounds.turnToPlayer.delay, sounds.turnToPlayer.loop, null);
			}
			break;
		case eDangerObjectState.attackAlternative:
			if ((bool)sounds.attackAlternative.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.attackAlternative.sound, 1f, base.transform.position, sounds.attackAlternative.delay, sounds.attackAlternative.loop, null);
			}
			break;
		case eDangerObjectState.hide:
			if ((bool)sounds.hide.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.hide.sound, 1f, base.transform.position, sounds.hide.delay, sounds.hide.loop, null);
			}
			break;
		case eDangerObjectState.prepare:
			if ((bool)sounds.prepare.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.prepare.sound, 1f, base.transform.position, sounds.prepare.delay, sounds.prepare.loop, null);
			}
			break;
		case eDangerObjectState.teleport:
			if ((bool)sounds.teleport.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.teleport.sound, 1f, base.transform.position, sounds.teleport.delay, sounds.teleport.loop, null);
			}
			break;
		case eDangerObjectState.maneuver:
			if ((bool)sounds.maneuver.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.maneuver.sound, 1f, base.transform.position, sounds.maneuver.delay, sounds.maneuver.loop, null);
			}
			break;
		case eDangerObjectState.run:
			if ((bool)sounds.run.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.run.sound, 1f, base.transform.position, sounds.run.delay, sounds.run.loop, null);
			}
			break;
		case eDangerObjectState.ambush:
			if ((bool)sounds.ambush.sound)
			{
				SoundEnvironment.Launch3DSound(sounds.ambush.sound, 1f, base.transform.position, sounds.ambush.delay, sounds.ambush.loop, null);
			}
			break;
		}
	}

	public void SoundStop(eDangerObjectState state, bool waitForMute)
	{
		switch (state)
		{
		case eDangerObjectState.wait:
			if ((bool)sounds.wait.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.wait.sound, waitForMute);
			}
			break;
		case eDangerObjectState.attack:
			if ((bool)sounds.attack.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.attack.sound, waitForMute);
			}
			break;
		case eDangerObjectState.destroy:
			if ((bool)sounds.destroy.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.destroy.sound, waitForMute);
			}
			break;
		case eDangerObjectState.chase:
			if ((bool)sounds.chase.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.chase.sound, waitForMute);
			}
			break;
		case eDangerObjectState.turnToPlayer:
			if ((bool)sounds.turnToPlayer.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.turnToPlayer.sound, waitForMute);
			}
			break;
		case eDangerObjectState.attackAlternative:
			if ((bool)sounds.attackAlternative.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.attackAlternative.sound, waitForMute);
			}
			break;
		case eDangerObjectState.hide:
			if ((bool)sounds.hide.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.hide.sound, waitForMute);
			}
			break;
		case eDangerObjectState.prepare:
			if ((bool)sounds.prepare.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.prepare.sound, waitForMute);
			}
			break;
		case eDangerObjectState.teleport:
			break;
		case eDangerObjectState.maneuver:
			if ((bool)sounds.maneuver.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.maneuver.sound, waitForMute);
			}
			break;
		case eDangerObjectState.run:
			if ((bool)sounds.run.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.run.sound, waitForMute);
			}
			break;
		case eDangerObjectState.ambush:
			if ((bool)sounds.ambush.sound)
			{
				SoundEnvironment.Stop3DSound(sounds.ambush.sound, waitForMute);
			}
			break;
		}
	}

	public virtual void OnExitFromState(eDangerObjectState state)
	{
	}

	public virtual void OnEnterToState(eDangerObjectState state)
	{
	}

	public virtual float GetDangerLeft()
	{
		return base.transform.position.x;
	}

	public virtual float GetDangerLeftInEditor()
	{
		return GetDangerLeft();
	}

	public virtual bool OnPlayerReenter()
	{
		return false;
	}

	public virtual void DangerEnterPlayer(PlayerController playerController, int dangerNum)
	{
		dangerIndex = dangerNum;
		OnPlayerEnter(playerController);
	}

	public virtual void OnPlayerEnter(PlayerController playerController)
	{
		playerController.OnEnterToDangerZone(this, PlayerDeathType.fall, eEnemyAttackStyle.nonBreakable);
	}

	public virtual void OnPlayerExit()
	{
	}

	public virtual void OnResetDanger()
	{
	}

	public virtual bool OnDangerWasAttack(bool isMightAttack)
	{
		return false;
	}

	public virtual bool IsBoarVulnerability()
	{
		return false;
	}

	public static void CreateTempInstance(GameObject dangerPrefab, Vector3 pos, Quaternion rotate)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(dangerPrefab, pos, rotate) as GameObject;
		DangerObject component = gameObject.GetComponent<DangerObject>();
		if ((bool)component.GetComponent<Animation>())
		{
			component.refAnim = component.GetComponent<Animation>();
		}
		else
		{
			component.refAnim = component.GetComponentInChildren<Animation>();
		}
		component.posGrid = component.CalculateGrid();
		component.isTemporaryDanger = true;
		component.DangerAwake();
		component.OnPlayerEnter(PlayerController.instance);
		DangerHelper.IncludeTempDanger(component);
	}
}
