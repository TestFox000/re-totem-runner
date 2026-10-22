using UnityEngine;

public class TestTriggerQTE : MonoBehaviour
{
	private bool isTriggered;

	private void Update()
	{
		if (isTriggered)
		{
			float num = PlayerController.instance.transform.position.x - base.transform.position.x;
			if (num > 1000f || num < 600f)
			{
				isTriggered = false;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isTriggered)
		{
			isTriggered = true;
			GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.qte);
		}
	}
}
