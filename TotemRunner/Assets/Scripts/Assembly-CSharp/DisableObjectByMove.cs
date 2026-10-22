using UnityEngine;

public class DisableObjectByMove : RestartObject
{
	private float baseXpos;

	private Transform locTransform;

	private void Awake()
	{
		locTransform = base.transform;
		baseXpos = locTransform.position.x;
	}

	private void Update()
	{
		float num = baseXpos - locTransform.position.x;
		if (num * num > 1f)
		{
			base.gameObject.SetActiveRecursivelyCompat(state: false);
			RestartObjectsList.IncludeRestartObject(this);
		}
	}

	public override bool RestartToX(float x)
	{
		if (baseXpos > x)
		{
			base.gameObject.SetActiveRecursivelyCompat(state: true);
		}
		return true;
	}
}
