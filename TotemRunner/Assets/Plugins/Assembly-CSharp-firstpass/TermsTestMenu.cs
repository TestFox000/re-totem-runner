using UnityEngine;

public class TermsTestMenu : MonoBehaviour
{
	private bool preCOPPA;

	private bool useCustomSkin;

	private bool canShowUI;

	public GameObject activateNextObject;

	private void Awake()
	{
		preCOPPA = false;
		useCustomSkin = false;
		canShowUI = false;
		Terms.registerForTermsNotification(Terms.AgeVerificationCallbackIdentifier.CRITERIAMET, "TermsTestMenu", "OnAgeVerificationCriteriaMet");
		Terms.registerForTermsNotification(Terms.AgeVerificationCallbackIdentifier.CRITERIANOTMET, "TermsTestMenu", "OnAgeVerificationCriteriaNotMet");
		Terms.registerForTermsNotification(Terms.AgeVerificationCallbackIdentifier.PENDINGDIALOGDISPLAY, "TermsTestMenu", "OnAgeVerificationPendingDialogDisplay");
	}

	private void Start()
	{
		Terms.initialiseTermsSession(preCOPPA, useCustomSkin, Terms.ComplianceLevel.FULLY_COMPLIANT);
	}

	public void ShowGui()
	{
		if (canShowUI)
		{
			Terms.showTermsUI();
		}
		else if (activateNextObject != null)
		{
			ActivateNextObject();
		}
	}

	private void OnAgeVerificationCriteriaMet(string message)
	{
		Debug.Log("[TermsTestMenu] OnAgeVerificationCriteriaMet message received");
		if (canShowUI && activateNextObject != null)
		{
			ActivateNextObject();
		}
		canShowUI = false;
	}

	private void OnAgeVerificationCriteriaNotMet(string message)
	{
		Debug.Log("[TermsTestMenu] OnAgeVerificationCriteriaNotMet message received");
		if (canShowUI && activateNextObject != null)
		{
			ActivateNextObject();
		}
		canShowUI = false;
	}

	private void OnAgeVerificationPendingDialogDisplay(string message)
	{
		Debug.Log("[TermsTestMenu] OnAgeVerificationPendingDialogDisplay message received");
		canShowUI = true;
	}

	private void ActivateNextObject()
	{
		// Preserve Unity 3's SetActiveRecursively behavior, including children
		// that were individually inactive before the splash sequence finishes.
		Transform[] children = activateNextObject.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < children.Length; i++)
		{
			children[i].gameObject.SetActive(true);
		}
	}
}
