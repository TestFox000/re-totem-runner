using System;
using UnityEngine;

public class ObjectEnabledByDistance : MonoBehaviour
{
	public float enabledDistance = 100f;

	[NonSerialized]
	public Vector3 startPosition;

	[NonSerialized]
	public ObjectEnabledByDistance next;

	public virtual void ObjectAwake()
	{
		startPosition = base.transform.position;
	}

	public virtual void ObjectEnable()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: true);
	}

	public virtual void ObjectDisable()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		next = null;
	}
}
