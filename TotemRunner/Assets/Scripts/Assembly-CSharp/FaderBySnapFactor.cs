public class FaderBySnapFactor : BaseFader
{
	public TouchSlidingCamera refCam;

	public int snapIndex;

	public BaseColorBlend refColorBlend;

	public override void Update()
	{
		base.Update();
		if (!refCam)
		{
			return;
		}
		bool flag = false;
		if (refCam.GetSnapIndex() == snapIndex)
		{
			float snapFactor = refCam.GetSnapFactor();
			if (snapFactor < 1f && snapFactor > -1f)
			{
				flag = true;
			}
		}
		if (flag && curState != eState.fade)
		{
			SetState(eState.fadeIn);
		}
		else if (!flag && curState != eState.wait && curState != eState.none)
		{
			SetState(eState.fadeOut);
		}
	}

	public override void SetState(eState state)
	{
		if ((bool)refColorBlend)
		{
			if (state == eState.fade)
			{
				refColorBlend.enabled = true;
			}
			else
			{
				refColorBlend.enabled = false;
				refColorBlend.ResetToStart();
			}
		}
		base.SetState(state);
	}
}
