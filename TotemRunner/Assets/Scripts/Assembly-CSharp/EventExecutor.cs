using UnityEngine;

public class EventExecutor : MonoBehaviour, IEventExecutor
{
	public EventExecutor nextExecutor;

	public virtual void StartExecutor()
	{
		if ((bool)nextExecutor)
		{
			nextExecutor.StartExecutor();
		}
	}

	public virtual void StartExecutor(string param)
	{
		StartExecutor();
	}
}
