public class ExecutorQteSuccess : EventExecutor
{
	public ScreenQTE screenQTE;

	public override void StartExecutor()
	{
		if ((bool)screenQTE && !screenQTE.IsDeath())
		{
			screenQTE.SetSuccess(isSuccess: true);
		}
		base.StartExecutor();
	}
}
