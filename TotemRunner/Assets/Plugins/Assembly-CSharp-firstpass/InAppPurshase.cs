using UnityEngine;

public class InAppPurshase
{
	private static bool sessionStarted;

	private static bool purchaseRejected;

	public static void StartPurchase(bool isSilent)
	{
		sessionStarted = true;
		purchaseRejected = false;
		if (!isSilent)
		{
			Debug.LogWarning("[InAppPurshase] The original store service is unavailable in this local build.");
		}
	}

	public static void FinishPurchase()
	{
		sessionStarted = false;
		purchaseRejected = false;
	}

	public static bool IsPurchaseReady()
	{
		// Callers use this as 'catalog request finished', then check for an
		// empty price to show their existing 'Unable connect to store' message.
		// A permanently false result would leave that screen waiting forever.
		return sessionStarted;
	}

	public static string GetProductPrice(int index)
	{
		return string.Empty;
	}

	public static bool RequestForBuy(int id)
	{
		purchaseRejected = true;
		Debug.LogWarning("[InAppPurshase] Purchase rejected: no store service is configured.");
		return false;
	}

	public static string GetResponseForBuy()
	{
		return purchaseRejected ? "fail" : string.Empty;
	}
}
