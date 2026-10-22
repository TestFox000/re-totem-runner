using UnityEngine;

public class ExecutorEnableByProfile : EventExecutor
{
	public string profileName;

	public GameObject refButton;

	private void Awake()
	{
		DisableThisObject(PlayerPrefs.GetInt(profileName, 0) != 0);
	}

	public void DisableThisObject(bool isDisable)
	{
		if (isDisable)
		{
			base.gameObject.SetActiveRecursivelyCompat(state: false);
			if (refButton != null)
			{
				refButton.SetActiveRecursivelyCompat(state: true);
			}
		}
		else
		{
			base.gameObject.SetActiveRecursivelyCompat(state: true);
			if (refButton != null)
			{
				refButton.SetActiveRecursivelyCompat(state: false);
			}
		}
	}

	public override void StartExecutor()
	{
		PlayerPrefs.SetInt(profileName, 1);
		DisableThisObject(base.gameObject.activeSelf);
		base.StartExecutor();
	}
}
