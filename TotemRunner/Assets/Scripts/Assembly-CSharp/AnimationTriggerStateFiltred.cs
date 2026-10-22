using UnityEngine;

public class AnimationTriggerStateFiltred : AnimationTrigger
{
	private bool triggerInactive = true;

	public EPlayerState[] stateFilterList;

	public override void OnTriggerEnter(Collider other)
	{
		if (triggerInactive && CheckFilter())
		{
			TriggerActivation();
			triggerInactive = false;
		}
	}

	public override void OnTriggerStay(Collider other)
	{
		if (triggerInactive && CheckFilter())
		{
			TriggerActivation();
			triggerInactive = false;
		}
	}

	private bool CheckFilter()
	{
		if (!PlayerController.instance.IsEnableControl())
		{
			return false;
		}
		EPlayerState playerState = LevelAttributes.instance.GetPlayerState();
		EPlayerState[] array = stateFilterList;
		foreach (EPlayerState ePlayerState in array)
		{
			if (playerState == ePlayerState)
			{
				return true;
			}
		}
		return false;
	}

	public override bool RestartToX(float x)
	{
		bool result = base.RestartToX(x);
		triggerInactive = true;
		return result;
	}
}
