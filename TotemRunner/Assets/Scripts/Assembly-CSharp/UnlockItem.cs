public class UnlockItem : BonusItem
{
	public string artItemName = "art1";

	public override void OnSetItemState(eItemState state)
	{
		if (state == eItemState.taken)
		{
		}
	}
}
