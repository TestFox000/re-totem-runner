using UnityEngine;

public class ExecutorTapProgress : EventExecutor
{
	public int needCount;

	private int curCount;

	public EventExecutor countReachExecutor;

	public BaseProgress progressGui;

	public bool reverseProgress;

	public float countDecreaseTime;

	private float countDecreaseLeft = -1f;

	private void Update()
	{
		if (curCount <= 0 || !(countDecreaseLeft >= 0f))
		{
			return;
		}
		countDecreaseLeft -= Time.deltaTime;
		if (countDecreaseLeft <= 0f)
		{
			curCount--;
			UpdateProgress();
			if ((float)curCount > 0f)
			{
				countDecreaseLeft = countDecreaseTime;
			}
		}
	}

	private void UpdateProgress()
	{
		if ((bool)progressGui)
		{
			float num = (float)curCount / ((float)needCount + 1f);
			if (reverseProgress)
			{
				num = 1f - num;
			}
			progressGui.SetProgress(num);
		}
	}

	public override void StartExecutor()
	{
		if (countDecreaseTime > 0f)
		{
			countDecreaseLeft = countDecreaseTime;
		}
		curCount++;
		if (curCount > needCount && (bool)countReachExecutor)
		{
			countReachExecutor.StartExecutor();
		}
		UpdateProgress();
	}

	public void ResetCounter()
	{
		curCount = 0;
		countDecreaseLeft = -1f;
		UpdateProgress();
	}
}
