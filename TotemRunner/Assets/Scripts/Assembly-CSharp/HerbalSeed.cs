public class HerbalSeed : BonusItem
{
	public string herbariumName = "usual";

	private PlayerController refPlayer;

	public override void ObjectAwake()
	{
		base.ObjectAwake();
		refPlayer = PlayerController.instance;
	}

	public override void OnSetItemState(eItemState state)
	{
		if (state != eItemState.taken)
		{
			return;
		}
		GameStatistic.instance.Increment(GameStatistic.eStatisticItem.seed);
		GrassGifManager.AddHerbal(herbariumName);
		refPlayer.manBehaviour.grassLeft = PlayerController.instance.manBehaviour.grassTimeout;
		if (LevelAttributes.instance.isManDying)
		{
			LevelAttributes.instance.isManDying = false;
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.manDying)
			{
				PlayerController.instance.SetPlayerState(EPlayerState.manRun);
			}
		}
		if (TutorialProcessor.instance != null)
		{
			TutorialProcessor.instance.IsEnableGrass = true;
		}
	}
}
