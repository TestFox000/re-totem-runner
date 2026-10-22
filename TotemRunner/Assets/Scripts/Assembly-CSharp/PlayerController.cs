using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, ITargetController
{
	[Serializable]
	public class StepCamShaker
	{
		public float timeLeft;

		public int stepIndex;

		public float[] shakeTime;

		public ECameraShakeType camShakeType;

		public CameraShake camShaker;

		public void Init(CameraShake shaker, float[] aShakeTime, ECameraShakeType shakeType)
		{
			timeLeft = aShakeTime[0];
			stepIndex = 0;
			shakeTime = aShakeTime;
			camShakeType = shakeType;
			camShaker = shaker;
		}

		public void Update()
		{
			timeLeft -= Time.deltaTime;
			bool flag = false;
			while (timeLeft < 0f)
			{
				stepIndex++;
				if (stepIndex >= shakeTime.Length)
				{
					stepIndex = 0;
				}
				else
				{
					flag = true;
				}
				timeLeft += shakeTime[stepIndex];
			}
			if (flag)
			{
				camShaker.LaunchShake(camShakeType, 1f);
			}
		}
	}

	[Serializable]
	public class SoundEventListener
	{
		public enum eEventState
		{
			none = 0,
			once = 1,
			loop = 2,
			chain = 3
		}

		private ePlayerSoundID soundId = ePlayerSoundID.none;

		private AnimationState curAniState;

		private float prevTime = -1f;

		private eEventState eventState;

		private float[] nextBorders = new float[10];

		private float[] volumeScales = new float[10];

		private ePlayerSoundID[] nextSoundId = new ePlayerSoundID[10];

		private int curBorderIndex;

		private int maxBorderIndex = 1;

		public void StartEvent(float _timeBorder, ePlayerSoundID sndId, eEventState state)
		{
			soundId = sndId;
			curAniState = PlayerAnimation.instance.GetCurentAnimationState();
			prevTime = -1f;
			eventState = state;
			nextBorders[0] = _timeBorder;
			volumeScales[0] = 1f;
			nextSoundId[0] = ePlayerSoundID.none;
			curBorderIndex = 0;
			maxBorderIndex = 1;
		}

		public void AddNextBorder(float nextBorder, float volumeScale, ePlayerSoundID sndId)
		{
			if (maxBorderIndex < nextBorders.Length - 1)
			{
				volumeScales[maxBorderIndex] = volumeScale;
				nextSoundId[maxBorderIndex] = sndId;
				nextBorders[maxBorderIndex++] = nextBorder;
			}
		}

		public void StopEvent()
		{
			soundId = ePlayerSoundID.none;
			eventState = eEventState.none;
		}

		public void Update()
		{
			if (eventState == eEventState.none)
			{
				return;
			}
			float num = nextBorders[curBorderIndex];
			if (prevTime < num && curAniState.time >= num)
			{
				ePlayerSoundID id = ((nextSoundId[curBorderIndex] == ePlayerSoundID.none) ? soundId : nextSoundId[curBorderIndex]);
				PlayerSoundList.instance.StartSound(id, volumeScales[curBorderIndex]);
				switch (eventState)
				{
				case eEventState.once:
					StopEvent();
					break;
				case eEventState.chain:
					curBorderIndex++;
					if (curBorderIndex >= maxBorderIndex)
					{
						StopEvent();
					}
					break;
				case eEventState.loop:
					curBorderIndex++;
					if (curBorderIndex >= maxBorderIndex)
					{
						curBorderIndex = 0;
					}
					break;
				}
			}
			prevTime = curAniState.time;
		}
	}

	[Serializable]
	public class CommonBehaviour
	{
		[NonSerialized]
		public float horizontalSpeedCur = 400f;

		[NonSerialized]
		public float horizontalSpeedMax = 400f;

		public float horizontalSpeedAcceleration = 800f;

		[NonSerialized]
		public float horizontalAcceleration = 800f;

		public float horizontalSpeedBase = 400f;

		public float minMovementStep = 0.1f;

		public float maxStepHeight = 50f;

		public float slowmoManSkinScale = 0.5f;

		public float slowmoBoarAttackScale = 0.1f;

		[NonSerialized]
		public Transform colliderTiedBone;

		[NonSerialized]
		public Vector3 colliderTiedPoint = Vector3.zero;

		public EffectControl effectOfSkinSwitch;

		[NonSerialized]
		public float vSpeed;

		public int deathSubScore = 50;
	}

	[Serializable]
	public class EagleBehaviour
	{
		public float verticalSpeed = 1500f;

		public float minSpeedForChangeState = 100f;

		public float minFlyHeight = -10f;

		public float maxFlyHeight = 450f;
	}

	[Serializable]
	public class StarBehaviour
	{
		public float verticalSpeed = 1500f;

		public float horizontalSpeed = 150f;

		[NonSerialized]
		public float vMovement;

		public float vericalInertion = 0.65f;
	}

	[Serializable]
	public class GhostBehaviour
	{
		public float verticalSpeed = 1500f;

		public float horizontalSpeed = 1200f;

		public float horizontalSpeedForStart = 100f;

		public float horizontalSpeedAcceleration = 100f;

		[NonSerialized]
		public float vMovement;

		public float flyHeight = 140f;

		[NonSerialized]
		public int nextFlyHelperPoint = -1;

		[NonSerialized]
		public float scoreTimeLeft;

		public float grassScoreTimeout = 0.1f;

		public int grassScore = 6;
	}

	[Serializable]
	public class ManBehaviour
	{
		public float fallheight = 30f;

		public float gravity = 2000f;

		public float maxFallSpeed = 1000f;

		public float actionCameraFreedomX;

		public float horizontalSpeed = 400f;

		public ParticleEmitter growParticleSystem;

		public Sprite3DWithDrawLevel glowSprite1;

		public Sprite3DWithDrawLevel glowSprite2;

		public Material glowSprite1Material;

		[NonSerialized]
		public float dyingTimeLeft;

		public float dyingTimeout = 2f;

		public Material dyingSpriteMaterial;

		public Color dyingBegColor;

		public Color dyingEndColor;

		[NonSerialized]
		public float grassLeft;

		public float grassTimeout = 5f;

		[NonSerialized]
		public float scoreTimeLeft;

		public float grassScoreTimeout = 0.2f;

		public int grassScore = 5;
	}

	[Serializable]
	public class BoarBehaviour
	{
		public float gravity = 5000f;

		public float maxFallSpeed = 2000f;

		public float fallheight = 30f;

		public float frontOffset = 60f;

		[NonSerialized]
		public Quaternion baseRotation;

		[NonSerialized]
		public float currentAngle;

		public float dangerPreviewDistance = 500f;

		[NonSerialized]
		public DangerObject previewDangerObject;

		public float speedupCameraFreedomX = 100f;

		public float horizontalSpeed = 600f;

		public float speedupHorizontalSpeed = 1000f;

		public float prepareHorzSpeed = 600f;

		public float attackHorzSpeed = 600f;

		public float[] speedupStepShakeTime = new float[2] { 0.1f, 0.3f };

		public float[] stepShakeTime = new float[2] { 0.1f, 0.3f };

		[NonSerialized]
		public bool isPowerAttack;

		public float noPlayPowerAttackDistance = 600f;

		public float noPlayAttackDistance = 350f;

		public Transform attackTrailRoot;

		[NonSerialized]
		public float boarBonusTimeLeft = 1f;

		public float boarBonusTimeDelay = 1f;

		public int boarSpeedupScore = 5;

		public int boarAttackScore = 10;

		public float boarAttackBonusTimeDelay = 0.6f;

		public float boarAttackBonusTimeInterval = 0.2f;

		[NonSerialized]
		public float speedupTimeleft;

		public float speedupTimeout = 2f;

		[NonSerialized]
		public float speedupPauseLeft;

		public float speedupPauseTimeout = 0.5f;

		public AudioClip slowMoAttackSound;

		public float slowMoAttackSndVol = 1f;
	}

	[Serializable]
	public class Timing
	{
		public float deathTimeout = 2f;

		[NonSerialized]
		public float manGroundingTime;

		[NonSerialized]
		public float boarAttackTime;

		[NonSerialized]
		public float boarGroundingTime;

		public float boarAttackMoment = 0.3f;

		public float slowmoManSkin = 0.3f;

		public float slowmoBoarAttack = 0.1f;

		public float slowmoBoarAttackFinishing = 0.05f;
	}

	public class DeferedActions
	{
		public enum EDeferedAction
		{
			manGroundFinished = 0,
			boarGroundFinished = 1,
			boarAttackFinished = 2,
			boarAttackMoment = 3,
			actionQuantity = 4
		}

		public float camZoomingScale;

		public float camZoomingTime;

		public float camZoomingSpringinessK;

		private float[] deferedActions = new float[4];

		public DeferedActions(PlayerController controller)
		{
		}

		public void AddAction(float time, EDeferedAction act)
		{
			deferedActions[(int)act] = time;
		}

		public int GetActionQuantity()
		{
			return deferedActions.Length;
		}

		public bool ActionTime(EDeferedAction act)
		{
			deferedActions[(int)act] -= Time.deltaTime;
			return deferedActions[(int)act] <= 0f;
		}

		public void ForceFinishAction(EDeferedAction act)
		{
			deferedActions[(int)act] = 0f;
		}

		public bool IsProcessed(EDeferedAction act)
		{
			return deferedActions[(int)act] > 0f;
		}
	}

	public Transform spawnPoint;

	private Transform lastSpawnPoint;

	public CameraShake cameraShaker;

	private PlayerDeathType currentDeathType = PlayerDeathType.fall;

	private float lastDeathX = -10000f;

	private int deathCounter;

	private int deathCounterLimit = 3;

	private StepCamShaker stepShaker = new StepCamShaker();

	private SoundEventListener soundEvent = new SoundEventListener();

	public CommonBehaviour commonBehaviour;

	public EagleBehaviour eagleBehaviour;

	public StarBehaviour starBehaviour;

	public GhostBehaviour ghostBehaviour;

	public ManBehaviour manBehaviour;

	public BoarBehaviour boarBehaviour;

	public Timing timing;

	public GrassGifManager grassManager;

	private CharacterController controller;

	private ModelSwaper modelSwaper;

	private PlayerAnimation playerAnimation;

	private CollisionFlags collisionFlags;

	private int disableController;

	private bool isDeathProcess;

	private bool isEnableInput = true;

	private int lastGetPathFrame = -1;

	private float lastGetPathValue;

	private eLockSkin lockSkin;

	private DeferedActions deferedAction;

	private float firstSpawnTime = 4f;

	private int firstSpawnState;

	private Texture storedTex1;

	private Texture storedTex2;

	public Texture2D whiteTex;

	public GameObject startLevelFx;

	public AudioClip startLevelSound;

	public GameObject endLevelFx;

	public GameObject blackScreen;

	private static PlayerController _singleton;

	public Vector3 Position => base.transform.position;

	public static PlayerController instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = UnityEngine.Object.FindObjectOfType(typeof(PlayerController)) as PlayerController;
			}
			return _singleton;
		}
	}

	public bool HaveCollider(Collider col)
	{
		return col.gameObject == base.gameObject;
	}

	public float GetSpawnPointX()
	{
		return lastSpawnPoint.position.x;
	}

	public void EnterToCheckPoint(Transform checkPoint)
	{
		lastSpawnPoint = checkPoint;
		if (LevelAttributes.instance.PlayerSkin == EPlayerSkin.Ghost)
		{
			ChangeSkin(EPlayerSkin.Man);
		}
	}

	public static bool IsFirstSpawn()
	{
		if (_singleton != null)
		{
			return _singleton.firstSpawnTime >= 0f;
		}
		return false;
	}

	public bool IsEnableControl()
	{
		return disableController == 0;
	}

	public void DisableInput(bool isDisable)
	{
		isEnableInput = !isDisable;
	}

	public void DisableController(bool disable)
	{
		if (!isDeathProcess)
		{
			if (disable)
			{
				disableController = -1;
			}
			else
			{
				disableController = 0;
			}
		}
	}

	public void LockSkin(eLockSkin lockType)
	{
		lockSkin = lockType;
	}

	private void Awake()
	{
		_singleton = this;
		firstSpawnTime = 4f;
		firstSpawnState = 0;
		if ((bool)startLevelFx)
		{
			startLevelFx.SetActiveRecursivelyCompat(state: false);
		}
		if ((bool)endLevelFx)
		{
			endLevelFx.SetActiveRecursivelyCompat(state: false);
		}
		if ((bool)blackScreen)
		{
			blackScreen.SetActiveRecursivelyCompat(state: false);
		}
		controller = GetComponent<CharacterController>();
		playerAnimation = GetComponent<PlayerAnimation>();
		modelSwaper = GetComponent<ModelSwaper>();
		deferedAction = new DeferedActions(this);
		lastSpawnPoint = spawnPoint;
		commonBehaviour.horizontalSpeedCur = manBehaviour.horizontalSpeed;
		commonBehaviour.horizontalSpeedMax = manBehaviour.horizontalSpeed;
		commonBehaviour.horizontalAcceleration = commonBehaviour.horizontalSpeedAcceleration;
		boarBehaviour.baseRotation = base.transform.rotation;
		boarBehaviour.previewDangerObject = null;
		if (!playerAnimation || !modelSwaper)
		{
			disableController = -1;
			Debug.LogError("There needs to be active scripts in your scene: PlayerAnimation, ModelSwaper, LevelAttributes, PathTimeline");
		}
		Animation animation = modelSwaper.AnimationGetByModel("man");
		if ((bool)animation)
		{
			timing.manGroundingTime = animation["ground"].length;
		}
		else
		{
			timing.manGroundingTime = 0f;
		}
		animation = modelSwaper.AnimationGetByModel("boar");
		if ((bool)animation)
		{
			timing.boarAttackTime = animation["boarAttack"].length;
			timing.boarGroundingTime = animation["boarGround"].length;
		}
		else
		{
			timing.boarAttackTime = 0f;
			timing.boarGroundingTime = 0f;
		}
		commonBehaviour.vSpeed = 0f;
		EnableGrassDropEffect(isEnable: false);
		disableController = -1;
	}

	public void SetParticleColor(Color newColor)
	{
		if ((bool)manBehaviour.growParticleSystem)
		{
			ParticleAnimator component = manBehaviour.growParticleSystem.gameObject.GetComponent<ParticleAnimator>();
			if ((bool)component)
			{
				component.colorAnimation = new Color[5] { newColor, newColor, newColor, newColor, newColor };
			}
		}
	}

	public void EnableGrassDropEffect(bool isEnable)
	{
		manBehaviour.growParticleSystem.enabled = isEnable;
		manBehaviour.growParticleSystem.gameObject.SetActive(isEnable);
		manBehaviour.glowSprite1.enabled = isEnable;
		manBehaviour.glowSprite2.enabled = isEnable;
	}

	public void EnableDyingEffect(bool isEnable)
	{
		manBehaviour.glowSprite1.enabled = isEnable;
		if (isEnable)
		{
			manBehaviour.glowSprite1.material = manBehaviour.dyingSpriteMaterial;
			manBehaviour.glowSprite1.material.color = manBehaviour.dyingBegColor;
		}
		else
		{
			manBehaviour.glowSprite1.material = manBehaviour.glowSprite1Material;
		}
	}

	private void Start()
	{
		manBehaviour.grassLeft = 1f;
		if (spawnPoint.position.x > 0f)
		{
			DangerHelper.instance.StartFirstRespawn(spawnPoint.position.x);
		}
		LaunchPlayer();
	}

	public void LaunchPlayer()
	{
		disableController = 5;
		firstSpawnTime = 4f;
		firstSpawnState = 0;
		if ((bool)blackScreen)
		{
			blackScreen.SetActiveRecursivelyCompat(state: true);
		}
		Spawn();
	}

	public void UpdateDefered()
	{
		for (int i = 0; i < deferedAction.GetActionQuantity(); i++)
		{
			if (deferedAction.IsProcessed((DeferedActions.EDeferedAction)i) && deferedAction.ActionTime((DeferedActions.EDeferedAction)i))
			{
				FinishDeferedAction((DeferedActions.EDeferedAction)i);
			}
		}
	}

	public void FinishDeferedAction(DeferedActions.EDeferedAction act)
	{
		switch (act)
		{
		case DeferedActions.EDeferedAction.manGroundFinished:
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.manGround || LevelAttributes.instance.GetPlayerState() == EPlayerState.manGroundAction)
			{
				SetPlayerState(EPlayerState.manRun);
			}
			break;
		case DeferedActions.EDeferedAction.boarGroundFinished:
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.boarGround)
			{
				SetPlayerState(EPlayerState.boarRun);
			}
			break;
		case DeferedActions.EDeferedAction.boarAttackFinished:
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.boarAttack)
			{
				SetPlayerState(EPlayerState.boarRun);
			}
			break;
		case DeferedActions.EDeferedAction.boarAttackMoment:
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.boarAttack && boarBehaviour.isPowerAttack)
			{
				TimeController.StartSlowMo(commonBehaviour.slowmoBoarAttackScale, 0f, timing.slowmoBoarAttack - timing.slowmoBoarAttackFinishing, timing.slowmoBoarAttackFinishing, eSlowMoMode.normal);
				if (boarBehaviour.slowMoAttackSound != null)
				{
					SoundEnvironment.Launch2DSound(boarBehaviour.slowMoAttackSound, boarBehaviour.slowMoAttackSndVol, 0f, -1f);
				}
			}
			break;
		}
		deferedAction.ForceFinishAction(act);
	}

	public bool ChangeSkin(EPlayerSkin newSkin)
	{
		EPlayerSkin playerSkin = LevelAttributes.instance.PlayerSkin;
		if (!LevelAttributes.instance.skinEnabled[(int)newSkin])
		{
			if (newSkin != EPlayerSkin.Man || playerSkin != EPlayerSkin.Eagle || !LevelAttributes.instance.skinEnabled[2])
			{
				return false;
			}
			newSkin = EPlayerSkin.Boar;
		}
		switch (playerSkin)
		{
		case EPlayerSkin.Boar:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			commonBehaviour.horizontalSpeedMax = commonBehaviour.horizontalSpeedBase;
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.boarAttack)
			{
				playerAnimation.SampleToFisrtFrame();
			}
			break;
		case EPlayerSkin.Star:
			commonBehaviour.horizontalSpeedMax = commonBehaviour.horizontalSpeedBase;
			break;
		case EPlayerSkin.Man:
			commonBehaviour.horizontalSpeedMax = commonBehaviour.horizontalSpeedBase;
			break;
		case EPlayerSkin.Ghost:
			commonBehaviour.horizontalSpeedMax = commonBehaviour.horizontalSpeedBase;
			StartGrowing(isStart: false);
			break;
		}
		modelSwaper.DoSwitch((int)newSkin);
		switch (newSkin)
		{
		case EPlayerSkin.Boar:
			commonBehaviour.horizontalSpeedMax = boarBehaviour.horizontalSpeed;
			boarBehaviour.currentAngle = 0f;
			boarBehaviour.attackTrailRoot.localScale = Vector3.zero;
			boarBehaviour.speedupTimeleft = boarBehaviour.speedupTimeout;
			boarBehaviour.speedupPauseLeft = 0f;
			break;
		case EPlayerSkin.Eagle:
			commonBehaviour.vSpeed = 0f;
			break;
		case EPlayerSkin.Man:
			commonBehaviour.horizontalSpeedMax = manBehaviour.horizontalSpeed;
			break;
		case EPlayerSkin.Star:
			commonBehaviour.vSpeed = 0f;
			commonBehaviour.horizontalSpeedMax = starBehaviour.horizontalSpeed;
			break;
		case EPlayerSkin.Ghost:
			LevelAttributes.instance.SetVerticalOffset(base.transform.position.y);
			commonBehaviour.vSpeed = 0f;
			commonBehaviour.horizontalSpeedMax = ghostBehaviour.horizontalSpeedForStart;
			StartGrowing(isStart: true);
			break;
		}
		if (newSkin == EPlayerSkin.Man && playerSkin == EPlayerSkin.Eagle)
		{
			TimeController.StartSlowMo(commonBehaviour.slowmoManSkinScale, 0f, timing.slowmoManSkin, 0f, eSlowMoMode.normal);
		}
		else
		{
			TimeController.StopSlowMo(forced: false);
		}
		if (playerSkin != newSkin)
		{
			commonBehaviour.effectOfSkinSwitch.StartEffect();
			PlaySwitchSkinSound(newSkin);
		}
		return true;
	}

	public void PlaySwitchSkinSound(EPlayerSkin playerSkin)
	{
		switch (playerSkin)
		{
		case EPlayerSkin.Man:
			PlayerSoundList.instance.StartSound(ePlayerSoundID.skinSwitchToMan, 1f);
			break;
		case EPlayerSkin.Eagle:
			PlayerSoundList.instance.StartSound(ePlayerSoundID.skinSwitchToEagle, 1f);
			break;
		case EPlayerSkin.Boar:
			PlayerSoundList.instance.StartSound(ePlayerSoundID.skinSwitchToBoar, 1f);
			break;
		case EPlayerSkin.Star:
			break;
		}
	}

	public bool PlayerAnimationIsPlaying()
	{
		Animation animation = modelSwaper.AnimationGetCurrent();
		if ((bool)animation)
		{
			return animation.isPlaying;
		}
		return false;
	}

	public void SetManWhite(bool isWhite)
	{
		if (LevelAttributes.instance.PlayerSkin == EPlayerSkin.Ghost)
		{
			return;
		}
		Animation animation = modelSwaper.AnimationGetCurrent();
		if (!animation)
		{
			return;
		}
		Renderer componentInChildren = animation.gameObject.GetComponentInChildren<Renderer>();
		if (!componentInChildren)
		{
			return;
		}
		if (componentInChildren.materials != null && componentInChildren.materials.Length > 0)
		{
			if (isWhite)
			{
				storedTex1 = componentInChildren.materials[0].mainTexture;
				componentInChildren.materials[0].mainTexture = whiteTex;
			}
			else
			{
				componentInChildren.materials[0].mainTexture = storedTex1;
			}
		}
		else
		{
			storedTex1 = null;
		}
		if (componentInChildren.materials != null && componentInChildren.materials.Length > 1)
		{
			if (isWhite)
			{
				storedTex2 = componentInChildren.materials[1].mainTexture;
				componentInChildren.materials[1].mainTexture = whiteTex;
			}
			else
			{
				componentInChildren.materials[1].mainTexture = storedTex2;
			}
		}
		else
		{
			storedTex2 = null;
		}
	}

	private void GrassLeftUpdate()
	{
		manBehaviour.grassLeft = 1f;
	}

	private bool CanSeedling()
	{
		if (TutorialProcessor.instance != null && !TutorialProcessor.instance.IsEnableGrass)
		{
			return false;
		}
		return manBehaviour.grassLeft > 0f && !LevelAttributes.instance.isManDying;
	}

	private void Update()
	{
		UpdateDefered();
		soundEvent.Update();
		if ((bool)commonBehaviour.colliderTiedBone)
		{
			controller.center = base.transform.InverseTransformPoint(commonBehaviour.colliderTiedBone.TransformPoint(commonBehaviour.colliderTiedPoint));
		}
		if (!Mathf.Approximately(commonBehaviour.horizontalSpeedCur, commonBehaviour.horizontalSpeedMax))
		{
			commonBehaviour.horizontalSpeedCur = Mathf.Lerp(commonBehaviour.horizontalSpeedCur, commonBehaviour.horizontalSpeedMax, Time.deltaTime * commonBehaviour.horizontalAcceleration);
		}
		if (disableController > 0)
		{
			disableController--;
		}
		if (disableController != 0)
		{
			return;
		}
		if (firstSpawnTime >= 0f)
		{
			firstSpawnTime -= Time.deltaTime;
			switch (firstSpawnState)
			{
			case 0:
				if (firstSpawnTime <= 3.3f)
				{
					if ((bool)startLevelFx)
					{
						startLevelFx.SetActiveRecursivelyCompat(state: true);
					}
					if ((bool)startLevelSound)
					{
						AudioSource.PlayClipAtPoint(startLevelSound, Camera.main.transform.position, 1f);
					}
					if ((bool)blackScreen)
					{
						blackScreen.SetActiveRecursivelyCompat(state: false);
					}
					firstSpawnState = 1;
				}
				break;
			case 1:
				if (firstSpawnTime <= 3.25f)
				{
					if ((bool)blackScreen)
					{
						blackScreen.SetActiveRecursivelyCompat(state: true);
					}
					firstSpawnState = 2;
				}
				break;
			case 2:
				if (firstSpawnTime <= 3.2f)
				{
					if ((bool)blackScreen)
					{
						blackScreen.SetActiveRecursivelyCompat(state: false);
					}
					firstSpawnState = 3;
				}
				break;
			case 3:
				if (firstSpawnTime <= 3.12f)
				{
					if ((bool)blackScreen)
					{
						blackScreen.SetActiveRecursivelyCompat(state: true);
					}
					firstSpawnState = 4;
				}
				break;
			case 4:
				if (firstSpawnTime <= 3.05f)
				{
					if ((bool)blackScreen)
					{
						blackScreen.SetActiveRecursivelyCompat(state: false);
					}
					firstSpawnState = 5;
				}
				break;
			case 5:
				if (firstSpawnTime <= 3f)
				{
					PlayerHide(isHide: false);
					Vector3 position2 = base.transform.position;
					position2.y = PathTimeline.instance.GetHeightByX(base.transform.position.x) + LevelAttributes.instance.GetVerticalOffset();
					base.transform.position = position2;
					SetManWhite(isWhite: true);
					playerAnimation.SwitchAnimation("firstSpawn", noBlend: true, sincTime: false);
					firstSpawnState = 6;
				}
				break;
			case 6:
				if (firstSpawnTime <= 2.8f)
				{
					SetManWhite(isWhite: false);
					firstSpawnState = 7;
				}
				break;
			case 7:
				if (firstSpawnTime <= 2.6f)
				{
					cameraShaker.LaunchShake(ECameraShakeType.boarSpeedupStep, 1f);
					firstSpawnState = 8;
				}
				break;
			case 8:
			{
				if (PlayerAnimationIsPlaying())
				{
					break;
				}
				firstSpawnState = 8;
				firstSpawnTime = -1f;
				playerAnimation.SwitchAnimation("run", noBlend: true, sincTime: false);
				ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
				Animation animation = modelSwaper.AnimationGetCurrent();
				if ((bool)animation)
				{
					Renderer componentInChildren = animation.gameObject.GetComponentInChildren<Renderer>();
					if ((bool)componentInChildren)
					{
						Vector3 position = base.transform.position;
						position.x = componentInChildren.transform.position.x;
						base.transform.position = position;
					}
				}
				break;
			}
			}
			if (firstSpawnTime >= 0f)
			{
				return;
			}
			firstSpawnTime = -1f;
			if ((bool)startLevelFx)
			{
				startLevelFx.SetActiveRecursivelyCompat(state: false);
			}
		}
		if (isEnableInput)
		{
			switch (LevelAttributes.instance.PlayerSkin)
			{
			case EPlayerSkin.Man:
				if (GameInput.GetButton(eGameInputButton.skinEagle))
				{
					ChangeSkin(EPlayerSkin.Eagle);
				}
				else if (GameInput.GetButton(eGameInputButton.skinBoar))
				{
					ChangeSkin(EPlayerSkin.Boar);
				}
				break;
			case EPlayerSkin.Eagle:
				if (!GameInput.GetButton(eGameInputButton.skinEagle))
				{
					if (GameInput.GetButton(eGameInputButton.skinBoar))
					{
						ChangeSkin(EPlayerSkin.Boar);
					}
					else
					{
						ChangeSkin(EPlayerSkin.Man);
					}
				}
				break;
			case EPlayerSkin.Boar:
				if (!GameInput.GetButton(eGameInputButton.skinBoar))
				{
					if (GameInput.GetButton(eGameInputButton.skinEagle))
					{
						ChangeSkin(EPlayerSkin.Eagle);
					}
					else if (lockSkin != eLockSkin.boar)
					{
						ChangeSkin(EPlayerSkin.Man);
					}
				}
				break;
			}
		}
		Quaternion rotation = boarBehaviour.baseRotation;
		float num = 0f;
		float num2 = commonBehaviour.horizontalSpeedCur * Time.deltaTime;
		float num3 = GetPathY() + LevelAttributes.instance.GetVerticalOffset();
		bool flag = true;
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Man:
			switch (LevelAttributes.instance.GetPlayerState())
			{
			case EPlayerState.manFall:
			{
				commonBehaviour.vSpeed += manBehaviour.gravity * Time.deltaTime;
				if (commonBehaviour.vSpeed > manBehaviour.maxFallSpeed)
				{
					commonBehaviour.vSpeed = manBehaviour.maxFallSpeed;
				}
				float num8 = base.transform.position.y - commonBehaviour.vSpeed * Time.deltaTime;
				if (num8 <= num3)
				{
					SetPlayerState(EPlayerState.manGround);
				}
				else
				{
					num3 = num8;
				}
				flag = false;
				if (WorldEvil.instance != null)
				{
					WorldEvil.instance.MovePause();
				}
				break;
			}
			case EPlayerState.manGround:
				if (CanSeedling())
				{
					SetPlayerState(EPlayerState.manGroundAction);
				}
				break;
			case EPlayerState.manGroundAction:
				GrassLeftUpdate();
				if (!CanSeedling())
				{
					SetPlayerState(EPlayerState.manGround);
				}
				break;
			case EPlayerState.manRun:
				if (CanSeedling())
				{
					SetPlayerState(EPlayerState.manAction);
				}
				break;
			case EPlayerState.manDying:
				manBehaviour.dyingTimeLeft -= Time.deltaTime;
				if (manBehaviour.dyingTimeLeft <= 0f)
				{
					PlayerDeath(PlayerDeathType.weak);
				}
				else
				{
					manBehaviour.glowSprite1.material.color = Color.Lerp(manBehaviour.dyingEndColor, manBehaviour.dyingBegColor, manBehaviour.dyingTimeLeft / manBehaviour.dyingTimeout);
				}
				if (WorldEvil.instance != null)
				{
					WorldEvil.instance.MovePause();
				}
				break;
			case EPlayerState.manAction:
				GrassLeftUpdate();
				if (!CanSeedling())
				{
					SetPlayerState(EPlayerState.manRun);
				}
				manBehaviour.scoreTimeLeft -= Time.deltaTime;
				if (manBehaviour.scoreTimeLeft <= 0f)
				{
					manBehaviour.scoreTimeLeft = manBehaviour.grassScoreTimeout;
				}
				break;
			case EPlayerState.manDeath:
				flag = false;
				break;
			}
			if (flag)
			{
				float num8 = base.transform.position.y - num3;
				if (num8 > manBehaviour.fallheight && LevelAttributes.instance.GetPlayerState() != EPlayerState.manFall)
				{
					SetPlayerState(EPlayerState.manFall);
					num3 = base.transform.position.y;
				}
			}
			break;
		case EPlayerSkin.Eagle:
		{
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.eagleDeath)
			{
				break;
			}
			num3 = LevelAttributes.instance.GetVerticalOffset();
			float num8;
			if (isEnableInput)
			{
				switch (GameInput.inputStyle)
				{
				case eInputStyle.eisButton:
				case eInputStyle.eisJoystick:
				case eInputStyle.eisKeyboard:
					num = ((!GameInput.GetButton(eGameInputButton.flyVertical)) ? 0f : (GameInput.GetButtonValue(eGameInputButton.flyVertical) * 0.4f));
					if (num != 0f)
					{
						num3 += num * eagleBehaviour.verticalSpeed * Time.deltaTime;
					}
					break;
				case eInputStyle.eisAcceleration:
					num3 = GuiGameHud.instance.minFlyHeight + (GameInput.GetButtonValue(eGameInputButton.flyVertical) + 1f) * 0.5f * (eagleBehaviour.maxFlyHeight - GuiGameHud.instance.minFlyHeight);
					num3 = Mathf.Lerp(base.transform.position.y, num3, Time.deltaTime * 2f);
					num8 = num3 - base.transform.position.y;
					if (num8 > commonBehaviour.maxStepHeight)
					{
						num3 = base.transform.position.y + commonBehaviour.maxStepHeight - 0.1f;
					}
					break;
				case eInputStyle.eisAcceleration2:
					num3 += GameInput.GetButtonValue(eGameInputButton.flyVertical) * eagleBehaviour.verticalSpeed * 0.4f * Time.deltaTime;
					break;
				}
			}
			float pathY = GetPathY();
			if (pathY < -200f && WorldEvil.instance != null)
			{
				WorldEvil.instance.MovePause();
			}
			if (num3 - pathY < eagleBehaviour.minFlyHeight)
			{
				num3 = pathY + eagleBehaviour.minFlyHeight;
			}
			if (num3 > eagleBehaviour.maxFlyHeight)
			{
				num3 = eagleBehaviour.maxFlyHeight;
			}
			float layerHeight = PathTimeline.instance.GetLayerHeight(ePathLayerID.maxFlyHight, base.transform.position.x + num2);
			if (layerHeight > -5000f)
			{
				layerHeight -= controller.center.y;
				if (num3 > layerHeight)
				{
					num3 = layerHeight;
				}
			}
			LevelAttributes.instance.SetVerticalOffset(num3);
			EPlayerState ePlayerState = EPlayerState.eagleFly;
			num8 = (num3 - base.transform.position.y) / Time.deltaTime;
			float minSpeedForChangeState = eagleBehaviour.minSpeedForChangeState;
			float num13 = 0f - eagleBehaviour.minSpeedForChangeState;
			if (num8 > minSpeedForChangeState)
			{
				ePlayerState = EPlayerState.eagleUp;
			}
			else if (num8 < num13)
			{
				ePlayerState = EPlayerState.eagleDown;
			}
			if (ePlayerState != LevelAttributes.instance.GetPlayerState())
			{
				SetPlayerState(ePlayerState);
			}
			break;
		}
		case EPlayerSkin.Boar:
			switch (LevelAttributes.instance.GetPlayerState())
			{
			case EPlayerState.boarFall:
			{
				commonBehaviour.vSpeed += boarBehaviour.gravity * Time.deltaTime;
				if (commonBehaviour.vSpeed > boarBehaviour.maxFallSpeed)
				{
					commonBehaviour.vSpeed = boarBehaviour.maxFallSpeed;
				}
				float num8 = base.transform.position.y - commonBehaviour.vSpeed * Time.deltaTime;
				if (num8 <= num3)
				{
					SetPlayerState(EPlayerState.boarGround);
					Vector3 position3 = base.transform.position;
					position3.y = num3;
					base.transform.position = position3;
				}
				else
				{
					num3 = num8;
				}
				flag = false;
				break;
			}
			case EPlayerState.boarGround:
				if (isEnableInput && boarBehaviour.speedupTimeleft > 0f)
				{
					SetPlayerState(EPlayerState.boarSpeedup);
				}
				break;
			case EPlayerState.boarRun:
				if (!boarBehaviour.previewDangerObject)
				{
					DangerObject nearestDangerObject = DangerHelper.GetNearestDangerObject(base.transform, boarBehaviour.dangerPreviewDistance);
					if ((bool)nearestDangerObject && LevelAttributes.instance.GetPlayerState() == EPlayerState.boarRun)
					{
						boarBehaviour.previewDangerObject = nearestDangerObject;
						SetPlayerState(EPlayerState.boarPrepare);
					}
				}
				if (isEnableInput && boarBehaviour.speedupTimeleft > 0f)
				{
					SetPlayerState(EPlayerState.boarSpeedup);
				}
				stepShaker.Update();
				break;
			case EPlayerState.boarPrepare:
				if (isEnableInput && boarBehaviour.speedupTimeleft > 0f)
				{
					SetPlayerState(EPlayerState.boarSpeedup);
				}
				else if ((bool)boarBehaviour.previewDangerObject && (double)(boarBehaviour.previewDangerObject.transform.position.x - base.transform.position.x) < 0.0)
				{
					boarBehaviour.previewDangerObject = DangerHelper.GetNearestDangerObject(base.transform, boarBehaviour.dangerPreviewDistance);
					if (!boarBehaviour.previewDangerObject)
					{
						SetPlayerState(EPlayerState.boarRun);
					}
				}
				stepShaker.Update();
				break;
			case EPlayerState.boarAttack:
				if (boarBehaviour.isPowerAttack)
				{
					boarBehaviour.boarBonusTimeLeft -= Time.deltaTime;
					if (boarBehaviour.boarBonusTimeLeft <= 0f)
					{
						boarBehaviour.boarBonusTimeLeft = boarBehaviour.boarAttackBonusTimeInterval;
					}
				}
				break;
			case EPlayerState.boarSpeedup:
				boarBehaviour.boarBonusTimeLeft -= Time.deltaTime;
				if (boarBehaviour.boarBonusTimeLeft <= 0f)
				{
					boarBehaviour.boarBonusTimeLeft = boarBehaviour.boarBonusTimeDelay;
				}
				if (boarBehaviour.speedupTimeleft > 0f)
				{
					if (boarBehaviour.speedupPauseLeft > 0f)
					{
						boarBehaviour.speedupPauseLeft -= Time.deltaTime;
						if (boarBehaviour.speedupPauseLeft <= 0f)
						{
							boarBehaviour.speedupPauseLeft = 0f;
						}
					}
					else
					{
						boarBehaviour.speedupTimeleft -= Time.deltaTime;
						if (boarBehaviour.speedupTimeleft <= 0f)
						{
							boarBehaviour.speedupTimeleft = 0f;
						}
					}
				}
				if (isEnableInput && boarBehaviour.speedupTimeleft <= 0f)
				{
					SetPlayerState(EPlayerState.boarRun);
				}
				stepShaker.Update();
				break;
			case EPlayerState.boarDeath:
				flag = false;
				break;
			}
			if (flag)
			{
				Vector3 position4 = base.transform.position;
				position4.x += num2;
				Vector3 vector = base.transform.TransformDirection(Vector3.forward * boarBehaviour.frontOffset);
				Vector3 vector2 = position4 - vector;
				position4 += vector;
				float num9 = PathTimeline.instance.GetHeightByX(position4.x) + LevelAttributes.instance.GetVerticalOffset();
				float num10 = PathTimeline.instance.GetHeightByX(vector2.x) + LevelAttributes.instance.GetVerticalOffset();
				float num11 = 300f;
				float num12 = 0f;
				if (position4.y - num9 > 30f)
				{
					num12 -= num11;
				}
				if (vector2.y - num10 > 30f)
				{
					num12 += num11;
				}
				if (num12 != 0f)
				{
					boarBehaviour.currentAngle += num12 * Time.deltaTime;
				}
				else
				{
					float f = Mathf.Clamp(num9 - num10, -60f, 60f) / (2f * boarBehaviour.frontOffset);
					boarBehaviour.currentAngle = 57.29578f * Mathf.Atan(f);
				}
				float num8 = base.transform.position.y - num3;
				if (num8 > boarBehaviour.fallheight && LevelAttributes.instance.GetPlayerState() != EPlayerState.boarFall)
				{
					SetPlayerState(EPlayerState.boarFall);
					num3 = base.transform.position.y;
				}
			}
			rotation = Quaternion.Euler(0f, 0f, boarBehaviour.currentAngle) * boarBehaviour.baseRotation;
			break;
		case EPlayerSkin.Star:
		{
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.starDeath)
			{
				break;
			}
			num3 = LevelAttributes.instance.GetVerticalOffset();
			if (isEnableInput)
			{
				switch (GameInput.inputStyle)
				{
				case eInputStyle.eisButton:
				case eInputStyle.eisJoystick:
				case eInputStyle.eisKeyboard:
					num = ((!GameInput.GetButton(eGameInputButton.flyVertical)) ? 0f : (GameInput.GetButtonValue(eGameInputButton.flyVertical) * 0.4f));
					if (num < 0f)
					{
						starBehaviour.vMovement -= Time.deltaTime * starBehaviour.vericalInertion;
						if (starBehaviour.vMovement < num)
						{
							starBehaviour.vMovement = num;
						}
					}
					else if (num > 0f)
					{
						starBehaviour.vMovement += Time.deltaTime * starBehaviour.vericalInertion;
						if (starBehaviour.vMovement > num)
						{
							starBehaviour.vMovement = num;
						}
					}
					else if (starBehaviour.vMovement > 0f)
					{
						starBehaviour.vMovement -= Time.deltaTime * starBehaviour.vericalInertion;
						if (starBehaviour.vMovement < 0f)
						{
							starBehaviour.vMovement = 0f;
						}
					}
					else if (starBehaviour.vMovement < 0f)
					{
						starBehaviour.vMovement += Time.deltaTime * starBehaviour.vericalInertion;
						if (starBehaviour.vMovement > 0f)
						{
							starBehaviour.vMovement = 0f;
						}
					}
					if (starBehaviour.vMovement != 0f)
					{
						num3 += starBehaviour.vMovement * starBehaviour.verticalSpeed * Time.deltaTime;
					}
					break;
				case eInputStyle.eisAcceleration:
				{
					num3 = GuiGameHud.instance.minFlyHeight + (GameInput.GetButtonValue(eGameInputButton.flyVertical) + 1f) * 0.5f * (eagleBehaviour.maxFlyHeight - GuiGameHud.instance.minFlyHeight);
					num3 = Mathf.Lerp(base.transform.position.y, num3, Time.deltaTime * 2f);
					float num8 = num3 - base.transform.position.y;
					if (num8 > commonBehaviour.maxStepHeight)
					{
						num3 = base.transform.position.y + commonBehaviour.maxStepHeight - 0.1f;
					}
					break;
				}
				case eInputStyle.eisAcceleration2:
					num3 += GameInput.GetButtonValue(eGameInputButton.flyVertical) * eagleBehaviour.verticalSpeed * 0.4f * Time.deltaTime;
					break;
				}
			}
			if (num3 - GetPathY() < eagleBehaviour.minFlyHeight)
			{
				num3 = GetPathY() + eagleBehaviour.minFlyHeight;
			}
			if (num3 > eagleBehaviour.maxFlyHeight)
			{
				num3 = eagleBehaviour.maxFlyHeight;
			}
			float layerHeight2 = PathTimeline.instance.GetLayerHeight(ePathLayerID.maxFlyHight, base.transform.position.x + num2);
			if (layerHeight2 > -5000f)
			{
				layerHeight2 -= controller.center.y;
				if (num3 > layerHeight2)
				{
					num3 = layerHeight2;
				}
			}
			LevelAttributes.instance.SetVerticalOffset(num3);
			break;
		}
		case EPlayerSkin.Ghost:
		{
			num3 = LevelAttributes.instance.GetVerticalOffset();
			float num4 = base.transform.position.x + commonBehaviour.horizontalSpeedCur * Time.deltaTime;
			float num5 = -1000f;
			if (ghostBehaviour.nextFlyHelperPoint >= 0)
			{
				Vector2 pointByIndex = FlyHelpPoints.GetPointByIndex(ghostBehaviour.nextFlyHelperPoint);
				float num6 = pointByIndex.x - num4;
				if (num6 <= 0f)
				{
					ghostBehaviour.nextFlyHelperPoint = FlyHelpPoints.instance.NextPointIndex(num4 + 1f);
				}
				else if (num6 < 300f)
				{
					num5 = pointByIndex.y;
				}
			}
			float num7 = GetPathY();
			if (num7 < -400f)
			{
				num7 = ((!(num5 > -400f)) ? PathTimeline.instance.GetHeightByXWithMinHeight(num4, -400f) : (num5 - ghostBehaviour.flyHeight));
			}
			num3 = Mathf.Lerp(num3, num7 + ghostBehaviour.flyHeight, Time.deltaTime * 4f);
			LevelAttributes.instance.SetVerticalOffset(num3);
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.ghostStart)
			{
				AnimationState curentAnimationState = playerAnimation.GetCurentAnimationState();
				if (!curentAnimationState || !curentAnimationState.enabled)
				{
					SetPlayerState(EPlayerState.ghostFly);
				}
			}
			ghostBehaviour.scoreTimeLeft -= Time.deltaTime;
			if (ghostBehaviour.scoreTimeLeft <= 0f)
			{
				ghostBehaviour.scoreTimeLeft = ghostBehaviour.grassScoreTimeout;
			}
			break;
		}
		}
		num = num3 - base.transform.position.y;
		if (num > 0f - commonBehaviour.minMovementStep && num < commonBehaviour.minMovementStep)
		{
			num = 0f;
		}
		bool flag2 = false;
		if (num > commonBehaviour.maxStepHeight)
		{
			flag2 = true;
		}
		if (num3 < GameSettings.gameSettings.level.killZoneY)
		{
			flag2 = true;
		}
		if (flag2)
		{
			flag2 = PlayerDeath(PlayerDeathType.fall);
		}
		if (!flag2)
		{
			Vector3 motion = new Vector3(num2, num, 0f);
			LegacyConcaveTriggerBridge.BeforePlayerMove(controller);
			collisionFlags = controller.Move(motion);
			Vector3 position5 = base.transform.position;
			position5.z = 0f;
			base.transform.rotation = rotation;
			base.transform.position = position5;
			LegacyConcaveTriggerBridge.AfterPlayerMove(controller);
		}
	}

	public void Spawn()
	{
		GameSettings.LoadRestoreData();
		if (firstSpawnTime >= 0f)
		{
			PlayerHide(isHide: true);
		}
		else
		{
			PlayerHide(isHide: false);
		}
		float num = commonBehaviour.horizontalSpeedBase;
		EPlayerSkin defaultSkin = LevelAttributes.instance.GetDefaultSkin();
		switch (defaultSkin)
		{
		case EPlayerSkin.Man:
			num = manBehaviour.horizontalSpeed;
			break;
		case EPlayerSkin.Eagle:
			num = commonBehaviour.horizontalSpeedBase;
			break;
		case EPlayerSkin.Boar:
			num = boarBehaviour.horizontalSpeed;
			boarBehaviour.currentAngle = 0f;
			boarBehaviour.attackTrailRoot.localScale = Vector3.zero;
			break;
		case EPlayerSkin.Star:
			num = starBehaviour.horizontalSpeed;
			starBehaviour.vMovement = 0f;
			break;
		}
		EPlayerState playerState = modelSwaper.models[(int)defaultSkin].playerState;
		if (LevelAttributes.instance.PlayerSkin != defaultSkin)
		{
			modelSwaper.DoSwitch((int)defaultSkin);
		}
		if (LevelAttributes.instance.GetPlayerState() != playerState)
		{
			SetPlayerState(playerState);
		}
		if (lastSpawnPoint != spawnPoint)
		{
			CheckPoint component = lastSpawnPoint.gameObject.GetComponent<CheckPoint>();
			if ((bool)component)
			{
				component.OnRespawn();
			}
		}
		if (firstSpawnTime < 0f)
		{
			commonBehaviour.effectOfSkinSwitch.StartEffect();
		}
		commonBehaviour.horizontalSpeedCur = num;
		commonBehaviour.horizontalSpeedMax = num;
		commonBehaviour.vSpeed = 0f;
		EnableGrassDropEffect(isEnable: false);
		float x = lastSpawnPoint.position.x;
		ScrollingCamera.instance.ResetToX(x);
		PathTimeline.instance.ResetToX(x);
		TeleportToPosition(lastSpawnPoint.position);
		grassManager.ResetToX(x);
		DangerHelper.ResetToX(x);
		RestartObjectsList.ResetToX(x);
		LevelMeshCombiner.ResetAllToX();
		lastGetPathFrame = Time.frameCount - 1;
		if (LevelAttributes.instance.PlayerSkin == EPlayerSkin.Man || LevelAttributes.instance.PlayerSkin == EPlayerSkin.Boar)
		{
			GUtils.SetTransformY(base.transform, GetPathY() + LevelAttributes.instance.GetVerticalOffset());
		}
		ScrollingCamera.instance.LockToTarget(base.transform);
		if (firstSpawnTime >= 0f)
		{
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.startZooming);
			if ((bool)blackScreen)
			{
				blackScreen.transform.position = Vector3.forward;
			}
		}
		if (WorldEvil.instance != null)
		{
			WorldEvil.instance.StartEvil();
		}
		if (GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.deathCount) > 0)
		{
			int a = ScoreManager.CalculateScore();
			a = Mathf.Min(a, commonBehaviour.deathSubScore);
			if (a > 0)
			{
				LiftupMessage.ShowMessage("-" + a + " sp", new Vector3(base.transform.position.x + 10f, base.transform.position.y + 120f, 0f));
				GameStatistic.instance.AddItem(GameStatistic.eStatisticItem.subScore, a);
			}
		}
	}

	private float GetPathY()
	{
		if (lastGetPathFrame == Time.frameCount)
		{
			return lastGetPathValue;
		}
		lastGetPathFrame = Time.frameCount;
		lastGetPathValue = PathTimeline.instance.GetHeightByX(base.transform.position.x + commonBehaviour.horizontalSpeedCur * Time.deltaTime);
		return lastGetPathValue;
	}

	private void StartDeath()
	{
		if (disableController >= 0)
		{
			playerAnimation.SwitchAnimation(GetDeathAnimationNameFromDeathType(), noBlend: false, sincTime: false);
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.death);
			cameraShaker.LaunchShake(ECameraShakeType.death, 1f);
			ScrollingCamera.instance.LockToTarget(null);
			DangerHelper.OnPlayerDeath();
			TimeController.StopSlowMo(forced: true);
			TimeController.StartSlowMo(0.2f, 0f, 0.24f, 0.1f, eSlowMoMode.normal);
			GameStatistic.instance.Increment(GameStatistic.eStatisticItem.deathCount);
			disableController = -1;
			isDeathProcess = true;
			AnalyticsManager.LogEventWithParam("deathLevel", "level", GameSettings.gameSettings.GetCurrentLevelData().name);
			StartCoroutine(DeathProcedure());
		}
	}

	private IEnumerator DeathProcedure()
	{
		yield return new WaitForSeconds(timing.deathTimeout * 0.5f);
		float deathDiffPos = lastDeathX - base.transform.position.x;
		if (deathDiffPos > -1000f && deathDiffPos < 1000f)
		{
			bool isUseHelpMsg = false;
			switch (LevelAttributes.instance.PlayerSkin)
			{
			case EPlayerSkin.Man:
				isUseHelpMsg = true;
				break;
			case EPlayerSkin.Boar:
				isUseHelpMsg = true;
				break;
			case EPlayerSkin.Eagle:
				isUseHelpMsg = true;
				break;
			}
			if (isUseHelpMsg)
			{
				deathCounter++;
				if (deathCounter >= deathCounterLimit)
				{
					deathCounter = 0;
					GuiGameHud.ShowDeathHelp();
				}
			}
		}
		else
		{
			deathCounter = 1;
		}
		lastDeathX = base.transform.position.x;
		yield return new WaitForSeconds(timing.deathTimeout * 0.5f);
		Spawn();
		disableController = 5;
		isDeathProcess = false;
	}

	public Rect GetCollisionRect()
	{
		Rect result = LevelAttributes.instance.PlayerSkin switch
		{
			EPlayerSkin.Man => new Rect(base.transform.position.x - 20f, base.transform.position.y, 80f, 180f), 
			EPlayerSkin.Eagle => new Rect(base.transform.position.x - 30f, base.transform.position.y + 30f, 100f, 90f), 
			EPlayerSkin.Boar => new Rect(base.transform.position.x - 60f, base.transform.position.y, 160f, 90f), 
			EPlayerSkin.Star => new Rect(base.transform.position.x - 20f, base.transform.position.y + 20f, 40f, 40f), 
			EPlayerSkin.Ghost => new Rect(base.transform.position.x - 100f, base.transform.position.y + 100f, 200f, 200f), 
			_ => new Rect(base.transform.position.x - 10f, base.transform.position.y + 10f, 20f, 20f), 
		};
		return result;
	}

	public bool PlayerDeath(PlayerDeathType playerDeathType)
	{
		currentDeathType = playerDeathType;
		bool result = true;
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Man:
			if (LevelAttributes.instance.GetPlayerState() != EPlayerState.manDeath)
			{
				SetPlayerState(EPlayerState.manDeath);
			}
			break;
		case EPlayerSkin.Boar:
			if (LevelAttributes.instance.GetPlayerState() != EPlayerState.boarDeath)
			{
				SetPlayerState(EPlayerState.boarDeath);
			}
			break;
		case EPlayerSkin.Eagle:
			if (LevelAttributes.instance.GetPlayerState() != EPlayerState.eagleDeath)
			{
				SetPlayerState(EPlayerState.eagleDeath);
			}
			break;
		case EPlayerSkin.Star:
			if (LevelAttributes.instance.GetPlayerState() != EPlayerState.starDeath)
			{
				SetPlayerState(EPlayerState.starDeath);
			}
			break;
		case EPlayerSkin.Ghost:
			result = false;
			break;
		}
		return result;
	}

	public bool OnEnterToDangerZone(DangerObject objDanger, PlayerDeathType playerDeathType, DangerObject.eEnemyAttackStyle attackStyle)
	{
		if (disableController != 0)
		{
			return true;
		}
		bool flag = true;
		EPlayerState playerState = LevelAttributes.instance.GetPlayerState();
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Man:
			if (objDanger.GetDangerType() != DangerObject.eDangerType.architecture)
			{
				flag = false;
			}
			break;
		case EPlayerSkin.Eagle:
			flag = false;
			break;
		case EPlayerSkin.Boar:
			if (objDanger.GetDangerType() == DangerObject.eDangerType.architecture)
			{
				break;
			}
			switch (attackStyle)
			{
			case DangerObject.eEnemyAttackStyle.nonBreakable:
				flag = false;
				break;
			case DangerObject.eEnemyAttackStyle.mightAttack:
				if (playerState != EPlayerState.boarSpeedup && playerState != EPlayerState.boarAttack)
				{
					flag = false;
				}
				break;
			}
			break;
		case EPlayerSkin.Star:
			flag = false;
			break;
		}
		if (!flag)
		{
			PlayerDeath(playerDeathType);
		}
		return flag;
	}

	public void OnExitFromDangerZone(DangerObject objDanger)
	{
		if (LevelAttributes.instance.GetPlayerState() == EPlayerState.boarPrepare && boarBehaviour.previewDangerObject == objDanger)
		{
			SetPlayerState(EPlayerState.boarRun);
		}
	}

	public void OnEnterToVulnerabilityZone(DangerObject objDanger)
	{
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Boar:
		{
			EPlayerState playerState = LevelAttributes.instance.GetPlayerState();
			if (playerState == EPlayerState.boarRun || playerState == EPlayerState.boarPrepare || playerState == EPlayerState.boarSpeedup || playerState == EPlayerState.boarGround)
			{
				if (objDanger.OnDangerWasAttack(playerState == EPlayerState.boarSpeedup))
				{
					float distanceToNextDanger = DangerHelper.GetDistanceToNextDanger(objDanger, base.transform.position.x);
					if (distanceToNextDanger > ((playerState == EPlayerState.boarSpeedup) ? boarBehaviour.noPlayPowerAttackDistance : boarBehaviour.noPlayAttackDistance) && !objDanger.PlayerAttackAnimationDontPlay())
					{
						SetPlayerState(EPlayerState.boarAttack);
					}
				}
			}
			else if (objDanger.IsBoarVulnerability())
			{
				objDanger.OnDangerWasAttack(isMightAttack: false);
			}
			break;
		}
		case EPlayerSkin.Ghost:
			objDanger.OnDangerWasAttack(isMightAttack: true);
			break;
		case EPlayerSkin.Star:
			break;
		}
	}

	public float GetSpeed()
	{
		return commonBehaviour.horizontalSpeedCur;
	}

	public Vector3 GetVelocity()
	{
		return new Vector3(commonBehaviour.horizontalSpeedCur, 0f, 0f);
	}

	public bool IsTouchingCeiling()
	{
		return (collisionFlags & CollisionFlags.Above) != 0;
	}

	public void OnExitFromState(EPlayerState state)
	{
		switch (state)
		{
		case EPlayerState.manFall:
			commonBehaviour.vSpeed = 0f;
			break;
		case EPlayerState.manGround:
			break;
		case EPlayerState.manGroundAction:
			StartGrowing(isStart: false);
			break;
		case EPlayerState.manDying:
			EnableDyingEffect(isEnable: false);
			PlayerSoundList.instance.StopSound(ePlayerSoundID.manPoison);
			break;
		case EPlayerState.boarFall:
			commonBehaviour.vSpeed = 0f;
			break;
		case EPlayerState.manAction:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			ScrollingCamera.instance.xFreedomMax = 0f;
			StartGrowing(isStart: false);
			break;
		case EPlayerState.manDeath:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			if (currentDeathType == PlayerDeathType.weak)
			{
				EnableDyingEffect(isEnable: false);
			}
			break;
		case EPlayerState.eagleDeath:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			break;
		case EPlayerState.boarPrepare:
			boarBehaviour.previewDangerObject = null;
			break;
		case EPlayerState.boarSpeedup:
			ScrollingCamera.instance.xFreedomMax = 0f;
			commonBehaviour.horizontalSpeedMax = commonBehaviour.horizontalSpeedBase;
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			break;
		case EPlayerState.boarDeath:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			break;
		case EPlayerState.starDeath:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			break;
		case EPlayerState.ghostStart:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			break;
		case EPlayerState.ghostFly:
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.none);
			commonBehaviour.horizontalAcceleration = commonBehaviour.horizontalSpeedAcceleration;
			PlayerSoundList.instance.StopSound(ePlayerSoundID.vasyaFly);
			break;
		case EPlayerState.manThorn:
		case EPlayerState.eagleFly:
		case EPlayerState.eagleDown:
		case EPlayerState.eagleUp:
		case EPlayerState.boarRun:
		case EPlayerState.boarAttack:
		case EPlayerState.boarGround:
		case EPlayerState.starFly:
			break;
		}
	}

	public void StartGrowing(bool isStart)
	{
		if (isStart)
		{
			grassManager.StartGrowing();
		}
		else
		{
			grassManager.StopGrowing();
		}
		EnableGrassDropEffect(isStart && LevelAttributes.instance.PlayerSkin == EPlayerSkin.Man);
		if (isStart && LevelAttributes.instance.PlayerSkin == EPlayerSkin.Man)
		{
			PlayerSoundList.instance.PauseSound(ePlayerSoundID.grassGrow, 1f, isPause: false);
		}
		else
		{
			PlayerSoundList.instance.PauseSound(ePlayerSoundID.grassGrow, 1f, isPause: true);
		}
	}

	public void SetPlayerState(EPlayerState state)
	{
		EPlayerState ePlayerState = state;
		if ((ePlayerState == EPlayerState.manRun || ePlayerState == EPlayerState.manAction) && LevelAttributes.instance.isManDying)
		{
			state = EPlayerState.manDying;
		}
		EPlayerState playerState = LevelAttributes.instance.GetPlayerState();
		if (playerState != state)
		{
			OnExitFromState(playerState);
		}
		switch (state)
		{
		case EPlayerState.manRun:
			LevelAttributes.instance.SetPlayerState(EPlayerState.manRun);
			playerAnimation.SwitchAnimation("run", noBlend: false, playerState == EPlayerState.manAction);
			soundEvent.StartEvent(0.23f, ePlayerSoundID.manStep, SoundEventListener.eEventState.loop);
			soundEvent.AddNextBorder(0.53f, 1f, ePlayerSoundID.manStep2);
			break;
		case EPlayerState.manAction:
			LevelAttributes.instance.SetPlayerState(EPlayerState.manAction);
			playerAnimation.SwitchAnimation("action", noBlend: false, playerState == EPlayerState.manRun);
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.grassGrowing);
			ScrollingCamera.instance.xFreedomMax = manBehaviour.actionCameraFreedomX;
			StartGrowing(isStart: true);
			manBehaviour.scoreTimeLeft = manBehaviour.grassScoreTimeout;
			break;
		case EPlayerState.manDying:
			LevelAttributes.instance.SetPlayerState(EPlayerState.manDying);
			playerAnimation.SwitchAnimation("dying", noBlend: false, sincTime: false);
			PlayerSoundList.instance.StartSound(ePlayerSoundID.manPoison, 1f);
			manBehaviour.dyingTimeLeft = manBehaviour.dyingTimeout;
			EnableDyingEffect(isEnable: true);
			break;
		case EPlayerState.eagleFly:
			LevelAttributes.instance.SetPlayerState(EPlayerState.eagleFly);
			playerAnimation.SwitchAnimation("EagleFly", noBlend: false, sincTime: false);
			break;
		case EPlayerState.eagleDown:
			LevelAttributes.instance.SetPlayerState(EPlayerState.eagleDown);
			playerAnimation.SwitchAnimation("EagleDown", noBlend: false, sincTime: false);
			soundEvent.StartEvent(0.1f, ePlayerSoundID.eagleWings, SoundEventListener.eEventState.once);
			break;
		case EPlayerState.eagleUp:
			LevelAttributes.instance.SetPlayerState(EPlayerState.eagleUp);
			playerAnimation.SwitchAnimation("EagleUp", noBlend: false, sincTime: false);
			soundEvent.StartEvent(0.1f, ePlayerSoundID.eagleWings, SoundEventListener.eEventState.chain);
			soundEvent.AddNextBorder(1f, 0.8f, ePlayerSoundID.none);
			soundEvent.AddNextBorder(1.8f, 0.6f, ePlayerSoundID.none);
			break;
		case EPlayerState.eagleDeath:
			LevelAttributes.instance.SetPlayerState(EPlayerState.eagleDeath);
			StartDeath();
			PlayerSoundList.instance.StartSound(ePlayerSoundID.eagleDeath, 1f);
			break;
		case EPlayerState.manDeath:
			LevelAttributes.instance.SetPlayerState(EPlayerState.manDeath);
			if (currentDeathType == PlayerDeathType.weak)
			{
				EnableDyingEffect(isEnable: true);
				manBehaviour.glowSprite1.material.color = manBehaviour.dyingEndColor;
			}
			StartDeath();
			PlayerSoundList.instance.StartSound(ePlayerSoundID.manDeath, 1f);
			break;
		case EPlayerState.manFall:
			LevelAttributes.instance.SetPlayerState(EPlayerState.manFall);
			playerAnimation.SwitchAnimation("fall", noBlend: false, sincTime: false);
			break;
		case EPlayerState.manGround:
			PlayerSoundList.instance.StartSound(ePlayerSoundID.ground, 1f);
			LevelAttributes.instance.SetPlayerState(EPlayerState.manGround);
			playerAnimation.SwitchAnimation("ground", noBlend: false, playerState == EPlayerState.manGroundAction);
			if (playerState == EPlayerState.manFall)
			{
				cameraShaker.LaunchShake(ECameraShakeType.ground, 1f);
				deferedAction.AddAction(timing.manGroundingTime, DeferedActions.EDeferedAction.manGroundFinished);
			}
			break;
		case EPlayerState.manGroundAction:
			LevelAttributes.instance.SetPlayerState(EPlayerState.manGroundAction);
			playerAnimation.SwitchAnimation("manGroundAction", noBlend: false, playerState == EPlayerState.manGround);
			StartGrowing(isStart: true);
			break;
		case EPlayerState.boarRun:
			LevelAttributes.instance.SetPlayerState(EPlayerState.boarRun);
			playerAnimation.SwitchAnimation("boarRun", noBlend: false, playerState == EPlayerState.boarSpeedup);
			commonBehaviour.horizontalSpeedMax = boarBehaviour.horizontalSpeed;
			stepShaker.Init(cameraShaker, boarBehaviour.stepShakeTime, ECameraShakeType.boarStep);
			soundEvent.StartEvent(0.21f, ePlayerSoundID.boarStep2, SoundEventListener.eEventState.loop);
			soundEvent.AddNextBorder(0.25f, 1f, ePlayerSoundID.boarBreath);
			soundEvent.AddNextBorder(0.42f, 1f, ePlayerSoundID.boarStep);
			break;
		case EPlayerState.boarPrepare:
			LevelAttributes.instance.SetPlayerState(EPlayerState.boarPrepare);
			playerAnimation.SwitchAnimation("boarPrepare", noBlend: false, playerState == EPlayerState.boarRun);
			commonBehaviour.horizontalSpeedMax = boarBehaviour.prepareHorzSpeed;
			stepShaker.Init(cameraShaker, boarBehaviour.stepShakeTime, ECameraShakeType.boarStep);
			soundEvent.StartEvent(0.21f, ePlayerSoundID.boarStep2, SoundEventListener.eEventState.loop);
			soundEvent.AddNextBorder(0.25f, 1f, ePlayerSoundID.boarBreath);
			soundEvent.AddNextBorder(0.42f, 1f, ePlayerSoundID.boarStep);
			break;
		case EPlayerState.boarSpeedup:
			LevelAttributes.instance.SetPlayerState(EPlayerState.boarSpeedup);
			playerAnimation.SwitchAnimation("boarSpeedup", noBlend: false, sincTime: false);
			ScrollingCamera.instance.xFreedomMax = boarBehaviour.speedupCameraFreedomX;
			commonBehaviour.horizontalSpeedMax = boarBehaviour.speedupHorizontalSpeed;
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.boarSpeedup);
			stepShaker.Init(cameraShaker, boarBehaviour.speedupStepShakeTime, ECameraShakeType.boarSpeedupStep);
			soundEvent.StartEvent(0.17f, ePlayerSoundID.boarStep2, SoundEventListener.eEventState.loop);
			soundEvent.AddNextBorder(0.2f, 1f, ePlayerSoundID.boarBreath);
			soundEvent.AddNextBorder(0.29f, 1f, ePlayerSoundID.boarStep);
			boarBehaviour.boarBonusTimeLeft = boarBehaviour.boarBonusTimeDelay;
			break;
		case EPlayerState.boarAttack:
			LevelAttributes.instance.SetPlayerState(EPlayerState.boarAttack);
			boarBehaviour.isPowerAttack = playerState == EPlayerState.boarSpeedup;
			playerAnimation.SwitchAnimation("boarAttack", noBlend: false, sincTime: false);
			if (boarBehaviour.isPowerAttack)
			{
				PlayerSoundList.instance.StartSound(ePlayerSoundID.boarAttack, 1f);
			}
			deferedAction.AddAction(timing.boarAttackMoment, DeferedActions.EDeferedAction.boarAttackMoment);
			deferedAction.AddAction(timing.boarAttackTime, DeferedActions.EDeferedAction.boarAttackFinished);
			commonBehaviour.horizontalSpeedMax = boarBehaviour.attackHorzSpeed;
			if (boarBehaviour.isPowerAttack)
			{
				ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.boarPowerAttack);
			}
			soundEvent.StartEvent(0.1f, ePlayerSoundID.boarStep, SoundEventListener.eEventState.loop);
			boarBehaviour.boarBonusTimeLeft = boarBehaviour.boarAttackBonusTimeDelay;
			break;
		case EPlayerState.boarFall:
			LevelAttributes.instance.SetPlayerState(EPlayerState.boarFall);
			playerAnimation.SwitchAnimation("boarFall", noBlend: false, sincTime: false);
			break;
		case EPlayerState.boarGround:
			LevelAttributes.instance.SetPlayerState(EPlayerState.boarGround);
			playerAnimation.SwitchAnimation("boarGround", noBlend: false, sincTime: false);
			cameraShaker.LaunchShake(ECameraShakeType.ground, 1f);
			deferedAction.AddAction(timing.boarGroundingTime, DeferedActions.EDeferedAction.boarGroundFinished);
			break;
		case EPlayerState.boarDeath:
			LevelAttributes.instance.SetPlayerState(EPlayerState.boarDeath);
			StartDeath();
			PlayerSoundList.instance.StartSound(ePlayerSoundID.boarDeath, 1f);
			break;
		case EPlayerState.starFly:
			LevelAttributes.instance.SetPlayerState(EPlayerState.starFly);
			playerAnimation.SwitchAnimation("starFly", noBlend: false, sincTime: false);
			break;
		case EPlayerState.starDeath:
			LevelAttributes.instance.SetPlayerState(EPlayerState.starDeath);
			StartDeath();
			break;
		case EPlayerState.ghostStart:
			LevelAttributes.instance.SetPlayerState(EPlayerState.ghostStart);
			playerAnimation.SwitchAnimation("ghostStart", noBlend: false, sincTime: false);
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.ghostStart);
			AchievementCollector.GhostPowerupStart();
			PlayerSoundList.instance.StartSound(ePlayerSoundID.vasyaStart, 1f);
			ghostBehaviour.scoreTimeLeft = ghostBehaviour.grassScoreTimeout;
			break;
		case EPlayerState.ghostFly:
			cameraShaker.LaunchShake(ECameraShakeType.death, 1f);
			LevelAttributes.instance.SetPlayerState(EPlayerState.ghostFly);
			playerAnimation.SwitchAnimation("ghostFly", noBlend: false, sincTime: false);
			commonBehaviour.horizontalSpeedMax = ghostBehaviour.horizontalSpeed;
			commonBehaviour.horizontalAcceleration = ghostBehaviour.horizontalSpeedAcceleration;
			ScrollingCamera.instance.SetZoom(BaseCamera.ECameraZoomingType.ghostFly);
			ghostBehaviour.nextFlyHelperPoint = ((!(FlyHelpPoints.instance != null)) ? (-1) : FlyHelpPoints.instance.NextPointIndex(base.transform.position.x));
			PlayerSoundList.instance.StartSound(ePlayerSoundID.vasyaFly, 1f);
			break;
		default:
			Debug.LogError("SetPlayerState: Unknown Player State " + state);
			break;
		}
	}

	public void Reset()
	{
		base.gameObject.tag = "Player";
	}

	public void ChangeCapsule(float rad, float colHeight, Vector3 colCenter, Transform colTie)
	{
		controller.radius = rad;
		controller.height = colHeight;
		controller.center = colCenter;
		commonBehaviour.colliderTiedBone = colTie;
		commonBehaviour.colliderTiedPoint = colCenter;
	}

	public Transform FindBone(string boneName)
	{
		Transform transform = base.gameObject.transform;
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Man:
			transform = transform.Find("Man");
			break;
		case EPlayerSkin.Eagle:
			transform = transform.Find("Eagle");
			break;
		case EPlayerSkin.Boar:
			transform = transform.Find("Boar");
			break;
		}
		return Utilites.FindBoneInTransform(transform, boneName);
	}

	private string GetDeathAnimationNameFromDeathType()
	{
		string result = string.Empty;
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Man:
			switch (currentDeathType)
			{
			case PlayerDeathType.thorn:
				result = "manDeathThorn";
				break;
			case PlayerDeathType.fall:
				result = "death";
				break;
			case PlayerDeathType.impact:
				result = "manDeathImpact";
				break;
			case PlayerDeathType.weak:
				result = "manWeakDeath";
				break;
			}
			break;
		case EPlayerSkin.Boar:
			switch (currentDeathType)
			{
			case PlayerDeathType.thorn:
				result = "boarDeathThorn";
				break;
			case PlayerDeathType.fall:
				result = "boarDeathFall";
				break;
			case PlayerDeathType.impact:
				result = "boarDeathImpact";
				break;
			case PlayerDeathType.weak:
				result = "boarDeathThorn";
				break;
			}
			break;
		case EPlayerSkin.Eagle:
			switch (currentDeathType)
			{
			case PlayerDeathType.thorn:
				result = "EagleDeathThorn";
				break;
			case PlayerDeathType.fall:
				result = "EagleDeathThorn";
				break;
			case PlayerDeathType.impact:
				result = "EagleDeath";
				break;
			case PlayerDeathType.weak:
				result = "EagleDeathThorn";
				break;
			}
			break;
		case EPlayerSkin.Star:
			result = "starDeath";
			break;
		}
		return result;
	}

	public void TeleportToPosition(Vector3 pos)
	{
		base.transform.position = pos;
		switch (LevelAttributes.instance.PlayerSkin)
		{
		case EPlayerSkin.Eagle:
		case EPlayerSkin.Star:
		{
			commonBehaviour.vSpeed = 0f;
			float num = pos.y;
			float heightByX = PathTimeline.instance.GetHeightByX(pos.x);
			if (num - heightByX < eagleBehaviour.minFlyHeight)
			{
				num = heightByX + eagleBehaviour.minFlyHeight;
			}
			if (num > eagleBehaviour.maxFlyHeight)
			{
				num = eagleBehaviour.maxFlyHeight;
			}
			float layerHeight = PathTimeline.instance.GetLayerHeight(ePathLayerID.maxFlyHight, pos.x);
			if (layerHeight > -5000f)
			{
				layerHeight -= controller.center.y;
				if (num > layerHeight)
				{
					num = layerHeight;
				}
			}
			LevelAttributes.instance.SetVerticalOffset(num);
			break;
		}
		case EPlayerSkin.Boar:
			break;
		}
	}

	public void PlayerHide(bool isHide)
	{
		Animation animation = modelSwaper.AnimationGetCurrent();
		if ((bool)animation)
		{
			animation.gameObject.SetActiveRecursivelyCompat(!isHide);
		}
	}
}
