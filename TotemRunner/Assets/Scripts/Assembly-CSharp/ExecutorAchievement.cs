public class ExecutorAchievement : EventExecutor
{
	public AchievementSystem.eAchievementID idAchiev;

	public override void StartExecutor()
	{
		AchievementSystem.Instance().ReachAchievement(idAchiev);
		base.StartExecutor();
	}
}
