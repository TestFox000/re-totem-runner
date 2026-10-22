public class ExecutorLoadLevel : EventExecutor
{
	public string levelName = "MainMenu";

	public override void StartExecutor()
	{
		BaseCamera.GLaunchLoading(levelName);
		base.StartExecutor();
	}
}
