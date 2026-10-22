using UnityEngine;

public class BaseFader : MonoBehaviour
{
	public enum eState
	{
		none = 0,
		wait = 1,
		fadeIn = 2,
		fade = 3,
		fadeOut = 4
	}

	public float fadeInTime = 0.3f;

	public float fadeOutTime = 0.3f;

	public bool correctAspect = true;

	protected float curTime = -1f;

	protected eState curState;

	private void Awake()
	{
		if (correctAspect && base.GetComponent<Renderer>() != null && GetComponent<TextMesh>() == null)
		{
			Vector3 position = base.transform.position;
			position.x = 0f;
			position.y = 0f;
			base.transform.position = position;
			GUtils.SetLocalScaleX(base.transform, Camera.main.aspect * 768f);
		}
	}

	private void Start()
	{
		if (base.GetComponent<Renderer>() != null)
		{
			SetState(eState.wait);
		}
	}

	public virtual void Update()
	{
		switch (curState)
		{
		case eState.fadeIn:
			curTime -= Time.deltaTime;
			if (fadeInTime > 0f && ValidateMaterial())
			{
				GUtils.SetRendererAlpha(base.GetComponent<Renderer>(), Mathf.Clamp01(1f - curTime / fadeInTime));
			}
			if (curTime <= 0f)
			{
				SetState(eState.fade);
			}
			break;
		case eState.fadeOut:
			curTime -= Time.deltaTime;
			if (fadeOutTime > 0f && ValidateMaterial())
			{
				GUtils.SetRendererAlpha(base.GetComponent<Renderer>(), Mathf.Clamp01(curTime / fadeOutTime));
			}
			if (curTime <= 0f)
			{
				SetState(eState.none);
			}
			break;
		case eState.fade:
			break;
		}
	}

	private bool ValidateMaterial()
	{
		return base.GetComponent<Renderer>() != null && base.GetComponent<Renderer>().material != null && base.GetComponent<Renderer>().material.HasProperty("_Color");
	}

	public virtual void SetState(eState state)
	{
		if (curState == state)
		{
			return;
		}
		if ((bool)base.GetComponent<Renderer>())
		{
			switch (state)
			{
			case eState.wait:
				if (ValidateMaterial())
				{
					GUtils.SetRendererAlpha(base.GetComponent<Renderer>(), 0f);
				}
				base.GetComponent<Renderer>().enabled = false;
				break;
			case eState.fadeIn:
				base.GetComponent<Renderer>().enabled = true;
				if (ValidateMaterial())
				{
					GUtils.SetRendererAlpha(base.GetComponent<Renderer>(), 0f);
				}
				curTime = fadeInTime;
				break;
			case eState.fade:
				base.GetComponent<Renderer>().enabled = true;
				if (ValidateMaterial())
				{
					GUtils.SetRendererAlpha(base.GetComponent<Renderer>(), 1f);
				}
				break;
			case eState.fadeOut:
				base.GetComponent<Renderer>().enabled = true;
				if (ValidateMaterial())
				{
					GUtils.SetRendererAlpha(base.GetComponent<Renderer>(), 1f);
				}
				curTime = fadeOutTime;
				break;
			case eState.none:
				base.GetComponent<Renderer>().enabled = false;
				break;
			}
		}
		curState = state;
	}
}
