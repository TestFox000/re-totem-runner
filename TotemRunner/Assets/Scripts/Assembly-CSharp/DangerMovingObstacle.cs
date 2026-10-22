using UnityEngine;

public class DangerMovingObstacle : DangerObject
{
	public enum eMoveMode
	{
		none = 0,
		bowl = 1,
		jump = 2
	}

	public float movingSpeed;

	public float leftBound = 1400f;

	public float startAnimationBound = 750f;

	public float deactivateBound = 200f;

	public Transform colliderBone;

	public ECameraShakeType prepareCamShakeType = ECameraShakeType.death;

	public float prepareCamShakeDelay = 0.2f;

	public float prepareCamShakeStep = 0.5f;

	private float timeLeft;

	public AudioClip camShakeSound;

	public bool camShakeSoundIsLoop;

	private bool camShakeSoundIsPlayed;

	public bool soundKillByPlayerDeath;

	public float camShakeSoundDelay = 0.1f;

	public float camShakeMaxVolume = 1f;

	public float camShakeMinVolume = 0.2f;

	public float camShakeMinSoundDistance = 200f;

	public float camShakeMaxSoundDistance = 500f;

	private Vector3 oldObjectPosition;

	private SphereCollider dangerCollider;

	public eMoveMode movingMode;

	public override void DangerAwake()
	{
		dangerCollider = GetComponent<SphereCollider>();
		oldObjectPosition = base.transform.position;
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.inactive);
	}

	public override void DangerUpdate()
	{
		float num = 0f;
		if (dangerState != eDangerObjectState.inactive && timeLeft >= 0f)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				timeLeft = prepareCamShakeStep;
				PlayShakeEffect();
			}
		}
		switch (dangerState)
		{
		case eDangerObjectState.prepare:
			if (base.transform.position.x - targetController.Position.x <= startAnimationBound)
			{
				SetObjectState(eDangerObjectState.wait);
			}
			break;
		case eDangerObjectState.wait:
		case eDangerObjectState.attack:
			MoveToPos(base.transform.position + new Vector3(movingSpeed * Time.deltaTime, 0f, 0f));
			if ((bool)dangerCollider)
			{
				dangerCollider.center = colliderBone.position - base.transform.position;
				num = dangerCollider.center.x + dangerCollider.radius;
			}
			num += base.transform.position.x + deactivateBound;
			if (DangerHelper.DistanceFromObject(targetController, num) < 0f)
			{
				SetObjectState(eDangerObjectState.inactive);
			}
			break;
		}
		switch (dangerState)
		{
		case eDangerObjectState.wait:
			if (DangerHelper.DistanceFromObject(targetController, base.GetComponent<Collider>().bounds.center.x) <= 0f && (bool)base.GetComponent<Collider>() && DangerHelper.CheckColliderCollision(base.GetComponent<Collider>(), targetController))
			{
				SetObjectState(eDangerObjectState.attack);
			}
			break;
		}
		base.DangerUpdate();
	}

	public override eDangerType GetDangerType()
	{
		return eDangerType.movingObstacle;
	}

	public override float GetDangerLeft()
	{
		return oldObjectPosition.x - leftBound;
	}

	public override float GetDangerLeftInEditor()
	{
		return base.transform.position.x - leftBound;
	}

	public override void OnPlayerEnter(PlayerController playerController)
	{
		targetController = playerController;
		dangerState = eDangerObjectState.unused;
		SetObjectState(eDangerObjectState.prepare);
	}

	private void PlayShakeEffect()
	{
		CameraShake.instance.LaunchShake(prepareCamShakeType, 1f);
		if (!(camShakeSound != null))
		{
			return;
		}
		if (camShakeSoundIsLoop)
		{
			if (!camShakeSoundIsPlayed)
			{
				SoundEnvironment.Launch3DSound(camShakeSound, camShakeMaxVolume, base.transform.position, camShakeSoundDelay, isLoop: true, null);
				camShakeSoundIsPlayed = true;
			}
		}
		else
		{
			float num = Mathf.Abs(base.transform.position.x - targetController.Position.x);
			float num2 = 1f - (num - camShakeMinSoundDistance) / (camShakeMaxSoundDistance - camShakeMinSoundDistance);
			float vol = Mathf.Clamp(camShakeMinVolume + num2 * (camShakeMaxVolume - camShakeMinVolume), camShakeMinVolume, camShakeMaxVolume);
			SoundEnvironment.Launch3DSound(camShakeSound, vol, base.transform.position, camShakeSoundDelay, isLoop: false, null);
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
			if (camShakeSoundIsPlayed && camShakeSoundIsLoop)
			{
				SoundEnvironment.Stop3DSound(camShakeSound, waitForMute: true);
			}
			camShakeSoundIsPlayed = false;
			break;
		case eDangerObjectState.prepare:
			refAnim.Stop();
			refAnim.Play("idle");
			MoveToPos(oldObjectPosition);
			dangerCollider.center = colliderBone.position - base.transform.position;
			if (prepareCamShakeDelay <= 0f)
			{
				PlayShakeEffect();
				timeLeft = prepareCamShakeStep;
			}
			else
			{
				timeLeft = prepareCamShakeDelay;
			}
			break;
		case eDangerObjectState.wait:
			switch (movingMode)
			{
			case eMoveMode.bowl:
				refAnim.Play("bowl");
				break;
			case eMoveMode.jump:
				refAnim.Play("jump");
				break;
			default:
				refAnim.Play("idle");
				break;
			}
			break;
		case eDangerObjectState.attack:
			targetController.OnEnterToDangerZone(this, PlayerDeathType.impact, eEnemyAttackStyle.nonBreakable);
			targetController.OnExitFromDangerZone(this);
			if (soundKillByPlayerDeath)
			{
				if (camShakeSoundIsPlayed && camShakeSoundIsLoop)
				{
					SoundEnvironment.Stop3DSound(camShakeSound, waitForMute: true);
				}
				camShakeSoundIsPlayed = false;
			}
			break;
		}
	}

	public void OnTriggerEnter(Collider other)
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

	public override void OnResetDanger()
	{
		if ((bool)dangerCollider)
		{
			SetObjectState(eDangerObjectState.inactive);
			MoveToPos(oldObjectPosition);
			dangerCollider.center = colliderBone.position - base.transform.position;
		}
	}
}
