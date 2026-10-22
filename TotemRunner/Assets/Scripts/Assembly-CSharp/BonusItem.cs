using UnityEngine;

public class BonusItem : ObjectEnabledByDistance
{
	public enum eItemState
	{
		inactive = 0,
		fly = 1,
		taken = 2,
		undefine = -1
	}

	public float movementHorizontalSpeed;

	public Animation anim;

	public AudioClip takeSound;

	private eItemState curItemState = eItemState.undefine;

	public virtual void OnSetItemState(eItemState state)
	{
	}

	public override void ObjectAwake()
	{
		base.ObjectAwake();
		anim = GetComponentInChildren<Animation>();
		SetItemState(eItemState.inactive);
	}

	private void Update()
	{
		if (curItemState != eItemState.inactive)
		{
			eItemState eItemState2 = curItemState;
			if (eItemState2 == eItemState.fly)
			{
				Vector3 position = base.transform.position;
				position.x += movementHorizontalSpeed * Time.deltaTime;
				base.transform.position = position;
			}
		}
	}

	public void OnTriggerEnter(Collider col)
	{
		SetItemState(eItemState.taken);
	}

	public void SetItemState(eItemState state)
	{
		if (state == curItemState)
		{
			return;
		}
		switch (state)
		{
		case eItemState.inactive:
			anim.Stop();
			break;
		case eItemState.fly:
			anim.Play("idle");
			break;
		case eItemState.taken:
			anim.Play("take");
			if (takeSound != null)
			{
				SoundEnvironment.Launch3DSoundOnce(takeSound, base.transform.position);
			}
			break;
		}
		OnSetItemState(state);
		curItemState = state;
	}

	public override void ObjectEnable()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: true);
		SetItemState(eItemState.fly);
	}

	public override void ObjectDisable()
	{
		base.transform.position = startPosition;
		SetItemState(eItemState.inactive);
		base.ObjectDisable();
	}
}
