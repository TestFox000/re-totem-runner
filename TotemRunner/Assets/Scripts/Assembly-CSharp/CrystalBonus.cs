public class CrystalBonus : BonusItem
{
	public int scorePoints = 10;

	public override void ObjectAwake()
	{
		base.ObjectAwake();
		base.gameObject.SetActiveRecursivelyCompat(state: false);
	}

	public override void OnSetItemState(eItemState state)
	{
		if (state == eItemState.taken)
		{
			GameStatistic.instance.Increment(GameStatistic.eStatisticItem.gem);
			GuiGameHud.UpdateCrystalProgress();
			AchievementCollector.AddCrystal();
		}
	}
}
