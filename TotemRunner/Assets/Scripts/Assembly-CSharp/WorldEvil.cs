using System;
using UnityEngine;

public class WorldEvil : MonoBehaviour
{
	public enum eControlId
	{
		none = 0,
		moveToPlayer = 1,
		pauseMove = 2,
		finalAway = 3,
		awayOneShot = 4,
		undefine = -1
	}

	[Serializable]
	public class EvilBehaviour
	{
		public float waitTimeout = 2f;

		public float maxWaitTimeout = 3f;

		public float timeoutGrowSpeed = 1f;

		public float moveTimeout = 5f;

		public float maxDist = 800f;

		public float minDist = 100f;

		public float flyHeight = 300f;

		public float fromPlayerHeight = 100f;

		public float movePauseTimeout = 0.5f;

		public float goAwayTimeout = 0.7f;

		public float goAwayStayTimeout = 1f;

		public float attackTimeout = 0.7f;

		public float prestartPausa = 3f;
	}

	[Serializable]
	public class AttackBehaviour
	{
		public float animTimeAdd;

		public float animTimeMul = 2f;

		public Vector3 attackPoint = Vector3.zero;
	}

	[Serializable]
	public class SoundsDescribe
	{
		public SoundEnvironment.SoundDescriber away = new SoundEnvironment.SoundDescriber();

		public SoundEnvironment.SoundDescriber attack = new SoundEnvironment.SoundDescriber();
	}

	private enum eEvilState
	{
		wait = 0,
		move = 1,
		attack = 2,
		away = 3,
		awayFinal = 4,
		awayOneShot = 5,
		inactive = 6,
		none = -1
	}

	private static WorldEvil _singleton;

	public Animation refAnim;

	public Renderer refRender;

	public GameObject refEvilObj;

	public ParticleSystem[] refParticles;

	private Transform refTransform;

	public PlayerDeathType playerDeathType = PlayerDeathType.impact;

	public EvilBehaviour behaviour = new EvilBehaviour();

	public AttackBehaviour attackBehaviour = new AttackBehaviour();

	public SoundsDescribe sounds = new SoundsDescribe();

	private float timeleft;

	private float maxTimeleft;

	private float mulTimeleft;

	private float playerDist;

	private float awayDist;

	private float movePauseTimeout;

	private eEvilState curState = eEvilState.none;

	private Vector3 attackBaseOffset = Vector3.zero;

	private bool enableEvil = true;

	public static WorldEvil instance => _singleton;

	private void Awake()
	{
		_singleton = this;
		enableEvil = true;
		SetState(eEvilState.inactive);
		timeleft = behaviour.prestartPausa + behaviour.waitTimeout;
		refTransform = base.transform;
	}

	private void Start()
	{
		MakeVisible(isVisible: false);
	}

	private void SetState(eEvilState state)
	{
		if (curState == state || (!enableEvil && state != eEvilState.awayFinal && state != eEvilState.inactive))
		{
			return;
		}
		switch (state)
		{
		case eEvilState.wait:
			maxTimeleft = behaviour.waitTimeout;
			timeleft = maxTimeleft;
			MakeVisible(isVisible: false);
			break;
		case eEvilState.move:
			maxTimeleft = behaviour.moveTimeout;
			mulTimeleft = 1f / maxTimeleft;
			movePauseTimeout = 0f;
			if (curState == eEvilState.away)
			{
				float num = Mathf.Clamp01((playerDist - behaviour.minDist) / (behaviour.maxDist - behaviour.minDist));
				timeleft = num * maxTimeleft;
				playerDist = Mathf.Lerp(behaviour.minDist, behaviour.maxDist, timeleft * mulTimeleft);
			}
			else
			{
				timeleft = maxTimeleft;
				playerDist = behaviour.maxDist;
			}
			MoveByPlayer();
			break;
		case eEvilState.attack:
			if (refAnim != null)
			{
				refAnim.Play("attack");
			}
			timeleft = behaviour.attackTimeout;
			attackBaseOffset = refTransform.position - PlayerController.instance.transform.position;
			if (sounds.attack.sound != null)
			{
				SoundEnvironment.Launch2DSound(sounds.attack.sound, sounds.attack.volume, 0f, -1f);
			}
			break;
		case eEvilState.away:
		case eEvilState.awayFinal:
			if (refAnim != null)
			{
				refAnim.Stop();
				refAnim.Play("away");
			}
			timeleft = behaviour.goAwayTimeout;
			mulTimeleft = 1f / timeleft;
			timeleft += behaviour.goAwayStayTimeout;
			awayDist = playerDist;
			if (sounds.away.sound != null && (double)playerDist < 900.0)
			{
				SoundEnvironment.Launch2DSound(sounds.away.sound, sounds.away.volume, 0f, -1f);
			}
			break;
		case eEvilState.inactive:
			MakeVisible(isVisible: false);
			break;
		}
		curState = state;
	}

	public void StartEvil()
	{
		if (enableEvil)
		{
			SetState(eEvilState.wait);
			timeleft = behaviour.prestartPausa + behaviour.waitTimeout;
		}
	}

	public void StopEvil()
	{
		SetState(eEvilState.inactive);
	}

	private void Update()
	{
		switch (curState)
		{
		case eEvilState.attack:
			MoveByPlayer();
			if (timeleft >= 0f)
			{
				timeleft -= Time.deltaTime;
				if (timeleft <= 0f)
				{
					timeleft = -1f;
					PlayerController.instance.PlayerDeath(playerDeathType);
				}
			}
			break;
		case eEvilState.wait:
			timeleft -= Time.deltaTime;
			if (timeleft <= 0f)
			{
				MakeVisible(isVisible: true);
				SetState(eEvilState.move);
			}
			else
			{
				playerDist = behaviour.maxDist;
				MoveByPlayer();
			}
			break;
		case eEvilState.move:
			if (refAnim != null && !refAnim.isPlaying)
			{
				refAnim.Play("move");
			}
			if (timeleft > 0f)
			{
				if (movePauseTimeout <= 0f)
				{
					timeleft -= Time.deltaTime;
				}
				else
				{
					movePauseTimeout -= Time.deltaTime;
					if (movePauseTimeout <= 0f)
					{
						movePauseTimeout = 0f;
					}
				}
				if (timeleft <= 0f)
				{
					timeleft = 0f;
				}
			}
			if (timeleft > 0f)
			{
				playerDist = Mathf.Lerp(behaviour.minDist, behaviour.maxDist, timeleft * mulTimeleft);
				MoveByPlayer();
				break;
			}
			playerDist = behaviour.minDist;
			if (refAnim == null || !refAnim.IsPlaying("away"))
			{
				SetState(eEvilState.attack);
			}
			break;
		case eEvilState.away:
		case eEvilState.awayOneShot:
			timeleft -= Time.deltaTime;
			if (timeleft <= 0f)
			{
				SetState(eEvilState.wait);
				break;
			}
			if (curState != eEvilState.awayOneShot && !GrassGifManager.instance.IsGrowing())
			{
				SetState(eEvilState.move);
				break;
			}
			playerDist = Mathf.Lerp(behaviour.maxDist, awayDist, Mathf.Clamp01(timeleft * mulTimeleft));
			MoveByPlayer();
			break;
		case eEvilState.awayFinal:
			timeleft -= Time.deltaTime;
			if (timeleft <= 0f)
			{
				SetState(eEvilState.inactive);
				break;
			}
			playerDist = Mathf.Lerp(behaviour.maxDist, awayDist, Mathf.Clamp01(timeleft * mulTimeleft));
			MoveByPlayer();
			break;
		}
	}

	private void MoveByPlayer()
	{
		Vector3 position = refTransform.position;
		Vector3 position2 = PlayerController.instance.transform.position;
		position2.z = position.z;
		if (curState == eEvilState.attack)
		{
			float num = 0f;
			if (refAnim != null)
			{
				num = Mathf.Clamp01((refAnim["attack"].normalizedTime - attackBehaviour.animTimeAdd) * attackBehaviour.animTimeMul);
				Vector3 vector = position2 + attackBaseOffset;
				Vector3 to = position2 + attackBehaviour.attackPoint;
				position = Vector3.Lerp(vector, to, num);
			}
		}
		else
		{
			position.x = position2.x + playerDist;
			if (curState != eEvilState.wait)
			{
				float heightByX = PathTimeline.instance.GetHeightByX(position.x);
				heightByX = ((!(heightByX < -200f)) ? (heightByX + behaviour.flyHeight) : behaviour.flyHeight);
				float num2 = position2.y + behaviour.fromPlayerHeight;
				if (num2 > heightByX)
				{
					heightByX = num2;
				}
				position.y = Mathf.Lerp(position.y, heightByX, Time.deltaTime * 2f);
			}
		}
		refTransform.position = position;
	}

	public void MovePause()
	{
		if (movePauseTimeout < behaviour.movePauseTimeout)
		{
			movePauseTimeout = behaviour.movePauseTimeout;
		}
	}

	public void MakeGrowing()
	{
		switch (curState)
		{
		case eEvilState.wait:
			if (maxTimeleft < behaviour.maxWaitTimeout)
			{
				maxTimeleft = Mathf.Clamp(maxTimeleft + Time.deltaTime * behaviour.timeoutGrowSpeed, behaviour.waitTimeout, behaviour.maxWaitTimeout);
			}
			timeleft = maxTimeleft;
			break;
		case eEvilState.move:
		case eEvilState.attack:
			SetState(eEvilState.away);
			break;
		}
	}

	private void MakeVisible(bool isVisible)
	{
		if (isVisible)
		{
			playerDist = behaviour.maxDist;
			MoveByPlayer();
			if (refRender != null)
			{
				refRender.enabled = true;
			}
			if (refEvilObj != null)
			{
				refEvilObj.SetActiveRecursivelyCompat(state: true);
			}
			if (refParticles != null)
			{
				ParticleSystem[] array = refParticles;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Clear();
					particleSystem.gameObject.SetActiveRecursivelyCompat(state: false);
					particleSystem.gameObject.SetActiveRecursivelyCompat(state: true);
					particleSystem.time = 0f;
					particleSystem.Play();
				}
			}
			return;
		}
		if (refAnim != null)
		{
			refAnim.Stop();
		}
		if (refRender != null)
		{
			refRender.enabled = false;
		}
		if (refEvilObj != null)
		{
			refEvilObj.SetActiveRecursivelyCompat(state: false);
		}
		if (refParticles != null)
		{
			ParticleSystem[] array2 = refParticles;
			foreach (ParticleSystem particleSystem2 in array2)
			{
				particleSystem2.Stop();
			}
		}
	}

	public void ExternControl(eControlId controlId, float fParam)
	{
		switch (controlId)
		{
		case eControlId.moveToPlayer:
			SetState(eEvilState.move);
			timeleft = fParam;
			break;
		case eControlId.pauseMove:
			MovePause();
			movePauseTimeout = fParam;
			break;
		case eControlId.finalAway:
			enableEvil = false;
			SetState(eEvilState.awayFinal);
			break;
		case eControlId.awayOneShot:
			SetState(eEvilState.awayOneShot);
			break;
		}
	}
}
