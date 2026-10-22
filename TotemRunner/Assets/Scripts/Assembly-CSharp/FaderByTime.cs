using UnityEngine;

public class FaderByTime : BaseFader
{
	public float waitTimeOut;

	public float fadeTimeLength = 2f;

	public override void Update()
	{
		eState eState = curState;
		base.Update();
		switch (eState)
		{
		case eState.wait:
			curTime -= Time.deltaTime;
			if (curTime <= 0f)
			{
				SetState(eState.fadeIn);
			}
			break;
		case eState.fade:
			curTime -= Time.deltaTime;
			if (curTime <= 0f)
			{
				SetState(eState.fadeOut);
			}
			break;
		case eState.fadeIn:
			break;
		}
	}

	public override void SetState(eState state)
	{
		if (curState != state)
		{
			base.SetState(state);
			switch (state)
			{
			case eState.wait:
				curTime = waitTimeOut;
				break;
			case eState.fade:
				curTime = fadeTimeLength;
				break;
			case eState.fadeIn:
				break;
			}
		}
	}
}
