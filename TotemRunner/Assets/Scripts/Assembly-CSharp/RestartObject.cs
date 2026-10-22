using System;
using UnityEngine;

public class RestartObject : MonoBehaviour
{
	[NonSerialized]
	public RestartObject next;

	public void SetNext(RestartObject val)
	{
		next = val;
	}

	public RestartObject GetNext()
	{
		return next;
	}

	public virtual bool RestartToX(float x)
	{
		return true;
	}
}
