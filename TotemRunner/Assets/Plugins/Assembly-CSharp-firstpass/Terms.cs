using System.Collections.Generic;
using UnityEngine;

public class Terms
{
	public enum AgeVerificationCallbackIdentifier
	{
		CRITERIAMET = 0,
		CRITERIANOTMET = 1,
		PENDINGDIALOGDISPLAY = 2
	}

	public enum ComplianceLevel
	{
		FULLY_COMPLIANT = 0,
		AGE_SENSETIVE = 1,
		AGE_GATED = 2
	}

	private sealed class Notification
	{
		public string objectName;

		public string methodName;
	}

	private static readonly Dictionary<AgeVerificationCallbackIdentifier, Notification> notifications = new Dictionary<AgeVerificationCallbackIdentifier, Notification>();

	private static bool sessionStarted;

	public static void registerForTermsNotification(AgeVerificationCallbackIdentifier notificationId, string objectName, string methodName)
	{
		if (string.IsNullOrEmpty(objectName) || string.IsNullOrEmpty(methodName))
		{
			return;
		}
		notifications[notificationId] = new Notification { objectName = objectName, methodName = methodName };
	}

	public static void initialiseTermsSession(bool preCOPPA, bool useCustomSkin, ComplianceLevel complianceLevel)
	{
		sessionStarted = true;
		Debug.Log("[Terms] Local offline session: the original terms/age service is unavailable. No age verification or online consent is recorded.");
		// The original TermsTestMenu continues local gameplay on either result.
		// Do not fabricate CRITERIAMET or a pending online dialog.
		Notify(AgeVerificationCallbackIdentifier.CRITERIANOTMET, "unavailable:local_offline");
	}

	public static void closeTermsSession()
	{
		sessionStarted = false;
	}

	public static void showTermsUI()
	{
		if (sessionStarted)
		{
			Notify(AgeVerificationCallbackIdentifier.CRITERIANOTMET, "unavailable:local_offline");
		}
	}

	private static void Notify(AgeVerificationCallbackIdentifier identifier, string message)
	{
		Notification notification;
		if (!notifications.TryGetValue(identifier, out notification))
		{
			return;
		}
		GameObject target = GameObject.Find(notification.objectName);
		if (target != null)
		{
			target.SendMessage(notification.methodName, message, SendMessageOptions.DontRequireReceiver);
		}
	}
}
