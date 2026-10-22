using UnityEngine;

public class EnemyPoacher : DangerObject
{
	public float leftBound = 800f;

	public float castStartOffset = 600f;

	public float attackStartOffset = 100f;

	public int tapSuccessCount = 16;

	public float vulnerabilityBound = 100f;

	public float playerDeathTimeOut = 0.8f;

	public GameObject castEffect;

	public ECameraShakeType camShakeType = ECameraShakeType.none;

	private Transform refTransform;

	private Transform refTargTransform;

	private ScreenQTE refQteScreen;

	private float timeLeft = 3f;

	private bool waitBoarForm;

	public override void DangerAwake()
	{
		refTransform = base.transform;
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.inactive);
	}

	private void EnableCastEffect(bool isEnable)
	{
		if ((bool)castEffect)
		{
			castEffect.SetActiveRecursivelyCompat(isEnable);
		}
	}

	private void SampleAnimationToStart()
	{
		refAnim.Stop();
		refAnim.Play("idle");
		refAnim.Sample();
		refAnim.Stop();
	}

	public override void OnExitFromState(eDangerObjectState state)
	{
		if (state == eDangerObjectState.prepare)
		{
			EnableCastEffect(isEnable: false);
		}
	}

	public override void DangerUpdate()
	{
		switch (dangerState)
		{
		case eDangerObjectState.wait:
			if (!refAnim.isPlaying)
			{
				refAnim.Play("cast");
			}
			if (refTransform.position.x - refTargTransform.position.x <= castStartOffset)
			{
				SetObjectState(eDangerObjectState.prepare);
			}
			break;
		case eDangerObjectState.prepare:
			if (!refAnim.isPlaying)
			{
				refAnim.Play("cast");
				EnableCastEffect(isEnable: true);
				if (camShakeType != ECameraShakeType.none)
				{
					CameraShake.instance.LaunchShake(camShakeType, 1f);
				}
			}
			if (GuiGameHud.instance.GetGuiState() == GuiGameHud.eGuiGameHudState.qte && refTransform.position.x - refQteScreen.GetPlayerPos().x <= attackStartOffset)
			{
				SetObjectState(eDangerObjectState.attack);
			}
			CheckVulnerability();
			break;
		case eDangerObjectState.attack:
			if (!refAnim.isPlaying)
			{
				SetObjectState(eDangerObjectState.inactive);
			}
			if (!(timeLeft >= 0f))
			{
				break;
			}
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				timeLeft = -1f;
				if (GuiGameHud.instance.GetGuiState() == GuiGameHud.eGuiGameHudState.qte)
				{
					refQteScreen.MakePlayerDeath();
				}
			}
			else
			{
				CheckVulnerability();
			}
			break;
		case eDangerObjectState.destroy:
			if (timeLeft >= 0f)
			{
				timeLeft -= Time.deltaTime;
				if (timeLeft <= 0f)
				{
					PlayerController.instance.DisableInput(isDisable: false);
					timeLeft = -1f;
				}
			}
			if (!refAnim.isPlaying)
			{
				float num = ((targetController == null) ? (-10000f) : (refTransform.position.x - targetController.Position.x));
				if (num < -1200f || num > 1200f)
				{
					SetObjectState(eDangerObjectState.inactive);
				}
			}
			break;
		case eDangerObjectState.inactive:
			PlayerController.instance.DisableInput(isDisable: false);
			break;
		}
		base.DangerUpdate();
	}

	private void CheckVulnerability()
	{
		if (targetController != null && PlayerController.instance.IsEnableControl())
		{
			if ((bool)refQteScreen && waitBoarForm && refQteScreen.GetSuccessFlag())
			{
				PlayerController.instance.ChangeSkin(EPlayerSkin.Boar);
				PlayerController.instance.SetPlayerState(EPlayerState.boarSpeedup);
				PlayerController.instance.DisableInput(isDisable: true);
				waitBoarForm = false;
			}
			if (refTransform.position.x - targetController.Position.x <= vulnerabilityBound)
			{
				targetController.OnEnterToVulnerabilityZone(this);
			}
		}
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.poacher;
	}

	public override float GetDangerLeft()
	{
		return base.transform.position.x - leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		refTargTransform = playerController.transform;
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
		case eDangerObjectState.inactive:
			SampleAnimationToStart();
			if (targetController != null)
			{
				targetController.OnExitFromDangerZone(this);
			}
			EnableCastEffect(isEnable: false);
			break;
		case eDangerObjectState.wait:
			refAnim.Play("prepare");
			break;
		case eDangerObjectState.prepare:
		{
			GuiGameHud instance = GuiGameHud.instance;
			if (instance != null)
			{
				waitBoarForm = true;
				refQteScreen = GuiGameHud.GetQteScreen();
				refQteScreen.SetQteTapCounter(tapSuccessCount - 1);
				if (LevelAttributes.instance.PlayerSkin != EPlayerSkin.Ghost)
				{
					instance.SetGuiState(GuiGameHud.eGuiGameHudState.qte);
				}
			}
			break;
		}
		case eDangerObjectState.attack:
			refAnim.Play("attack");
			timeLeft = playerDeathTimeOut;
			break;
		case eDangerObjectState.destroy:
			refAnim.Play("death");
			timeLeft = 0.7f;
			break;
		case eDangerObjectState.chase:
		case eDangerObjectState.turnToPlayer:
		case eDangerObjectState.attackAlternative:
		case eDangerObjectState.hide:
			break;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		SetObjectState(eDangerObjectState.destroy);
	}

	public override void OnResetDanger()
	{
		SetObjectState(eDangerObjectState.inactive);
	}
}
