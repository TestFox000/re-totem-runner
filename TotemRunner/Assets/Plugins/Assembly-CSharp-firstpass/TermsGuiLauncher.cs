using UnityEngine;

public class TermsGuiLauncher : MonoBehaviour
{
	public TermsTestMenu termsMenu;

	private void Start()
	{
		if (termsMenu != null)
		{
			termsMenu.ShowGui();
		}
	}

	private void Update()
	{
	}
}
