using UnityEngine;

public class SkinSwitcherTrigger : MonoBehaviour
{
	public bool[] skinEnabled = new bool[5] { true, true, true, false, true };

	public EPlayerSkin startSkin;

	public bool manIsDying;

	private float disableTriggerTimeLeft;

	private void OnTriggerEnter(Collider other)
	{
		if (disableTriggerTimeLeft > 0f)
		{
			return;
		}
		disableTriggerTimeLeft = 1f;
		LevelAttributes instance = LevelAttributes.instance;
		if (instance.PlayerSkin != EPlayerSkin.Ghost)
		{
			for (int i = 0; i < skinEnabled.Length; i++)
			{
				instance.skinEnabled[i] = skinEnabled[i];
			}
			if (startSkin >= EPlayerSkin.Man && startSkin < EPlayerSkin.Quantity)
			{
				PlayerController.instance.ChangeSkin(startSkin);
			}
			if (manIsDying)
			{
				PlayerController.instance.SetPlayerState(EPlayerState.manDying);
				instance.isManDying = true;
			}
			else if (instance.isManDying)
			{
				instance.isManDying = false;
				PlayerController.instance.SetPlayerState(EPlayerState.manRun);
			}
		}
	}

	private void Update()
	{
		if (disableTriggerTimeLeft > 0f)
		{
			disableTriggerTimeLeft -= Time.deltaTime;
		}
	}
}
