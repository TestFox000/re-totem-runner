using UnityEngine;

/// <summary>
/// Preserves Unity 3.x recursive activation, including the active state of every child.
/// </summary>
public static class TotemRunnerLegacyGameObjectExtensions
{
	public static void SetActiveRecursivelyCompat(this GameObject target, bool state)
	{
		Transform[] hierarchy = target.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < hierarchy.Length; i++)
		{
			hierarchy[i].gameObject.SetActive(state);
		}
	}
}
