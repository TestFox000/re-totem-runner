using UnityEngine;

public class ItemCounter : MonoBehaviour
{
	public enum RoundType
	{
		none = 0,
		ghostCost = 1
	}

	private int oldCount;

	public CounterUI counter;

	public GameStatistic.eStatisticItem itemId;

	public RoundType round;

	private void Awake()
	{
		if (counter == null)
		{
			counter = GetComponent<CounterUI>();
		}
	}

	private void Update()
	{
		int num = oldCount;
		RoundType roundType = round;
		num = ((roundType != RoundType.ghostCost) ? GameStatistic.instance.ItemCount(itemId) : (GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.gemSummary) % GuiGameHud.ghostCost));
		if (num != oldCount)
		{
			oldCount = num;
			if (counter != null)
			{
				counter.SetCounter(num);
			}
		}
	}
}
