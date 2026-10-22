public class ExecutorLaunchStore : EventExecutor
{
	public override void StartExecutor()
	{
		base.StartExecutor();
		if (ScreenStore.instance != null && !ScreenStore.instance.enabled)
		{
			ScreenStore.instance.StartShow();
		}
	}
}
