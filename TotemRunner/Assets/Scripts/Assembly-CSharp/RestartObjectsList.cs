using UnityEngine;

public class RestartObjectsList : MonoBehaviour
{
	private RestartObject rootRestartObject;

	private RestartObject lastRestartObject;

	public static RestartObjectsList instance;

	private void Awake()
	{
		instance = this;
		rootRestartObject = null;
		lastRestartObject = null;
	}

	public static void IncludeRestartObject(RestartObject obj)
	{
		if (!obj.next && obj != instance.lastRestartObject)
		{
			if ((bool)instance.rootRestartObject)
			{
				obj.SetNext(instance.rootRestartObject);
			}
			else
			{
				instance.lastRestartObject = obj;
			}
			instance.rootRestartObject = obj;
		}
		else
		{
			Debug.LogWarning(string.Concat(Time.time, ": Restart objects ", obj, obj.transform.position, " already into restart list"));
		}
	}

	public static void ResetToX(float x)
	{
		RestartObject restartObject = null;
		RestartObject next = instance.rootRestartObject;
		while (next != null)
		{
			if (next.RestartToX(x))
			{
				if ((bool)restartObject)
				{
					restartObject.next = next.next;
					next.next = null;
					next = restartObject.next;
				}
				else
				{
					instance.rootRestartObject = next.next;
					next.next = null;
					next = instance.rootRestartObject;
				}
			}
			else
			{
				restartObject = next;
				next = next.GetNext();
			}
		}
		instance.lastRestartObject = restartObject;
	}
}
