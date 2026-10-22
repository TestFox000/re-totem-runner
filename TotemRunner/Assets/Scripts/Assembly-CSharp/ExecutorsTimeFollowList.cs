using System;
using UnityEngine;

[Serializable]
public class ExecutorsTimeFollowList
{
	[Serializable]
	public class ExecutionNode
	{
		public float timeStart;

		public EventExecutor executor;

		[NonSerialized]
		public bool executed;
	}

	public ExecutionNode[] executes;

	private float timeFollow = -1f;

	private int nCurExecute;

	public ExecutorsTimeFollowList()
	{
		timeFollow = -1f;
		nCurExecute = 0;
	}

	public void StartExecutes()
	{
		if (timeFollow < 0f)
		{
			timeFollow = 0f;
			nCurExecute = 0;
		}
	}

	public void Update()
	{
		if (!(timeFollow < 0f))
		{
			timeFollow += Time.deltaTime;
			while (nCurExecute < executes.Length && executes[nCurExecute].timeStart < timeFollow)
			{
				executes[nCurExecute++].executor.StartExecutor();
			}
			if (nCurExecute >= executes.Length)
			{
				timeFollow = -1f;
			}
		}
	}
}
