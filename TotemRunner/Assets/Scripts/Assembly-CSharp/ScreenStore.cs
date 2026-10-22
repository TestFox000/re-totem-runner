using UnityEngine;

public class ScreenStore : EventExecutor
{
	private static ScreenStore _singleton;

	public TextMesh[] buyProducts;

	public TextMesh buyText;

	public GameObject buyFullVersion;

	public TextMesh buyFullVerText;

	private bool buyMenuReady;

	private bool isWaitAsync;

	private int buyIndex;

	public static ScreenStore instance => _singleton;

	private void Awake()
	{
		_singleton = this;
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		base.enabled = false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (GuiGameHud.instance != null && GuiGameHud.instance.deathMsgHelp != null && GuiGameHud.instance.deathMsgHelp.gameObject.activeSelf)
		{
			GuiGameHud.instance.deathMsgHelp.SkipTutorAction();
		}
		if (buyMenuReady)
		{
			if (!isWaitAsync)
			{
				return;
			}
			string responseForBuy = InAppPurshase.GetResponseForBuy();
			if (!(responseForBuy != string.Empty))
			{
				return;
			}
			if (responseForBuy == "ok")
			{
				int num = 0;
				switch (buyIndex)
				{
				case 0:
					num = 5 * GuiGameHud.ghostCost;
					break;
				case 1:
					num = 15 * GuiGameHud.ghostCost;
					break;
				case 2:
					num = 30 * GuiGameHud.ghostCost;
					break;
				case 3:
					PlayerPrefs.SetInt("unlockfull", 1);
					break;
				}
				if (num > 0)
				{
					int crystalSummary = GameDataStorage.GetCrystalSummary();
					GameDataStorage.SetCrystalSummary(crystalSummary + num);
					GameStatistic.instance.PurshaseGems(num);
					GuiGameHud.UpdateCrystalProgress();
					if (!PlayerPrefs.HasKey("userHasBoughtSomething"))
					{
						PlayerPrefs.SetInt("userHasBoughtSomething", 1);
						AnalyticsManager.LogEvent("uniquePayingUser");
					}
					int value = PlayerPrefs.GetInt("buyGems", 0) + num / GuiGameHud.ghostCost;
					PlayerPrefs.SetInt("buyGems", value);
					AnalyticsManager.LogEventWithParam("purchaseMade", "IAP", "com.chillingo.totemrunner.f" + (buyIndex + 1));
				}
			}
			isWaitAsync = false;
			if (ChooseLevelGUI.IsFreeVersion())
			{
				if (buyFullVersion != null)
				{
					buyFullVersion.SetActiveRecursivelyCompat(state: true);
				}
			}
			else if (buyProducts != null)
			{
				for (int i = 0; i < buyProducts.Length; i++)
				{
					if (buyProducts[i] != null)
					{
						buyProducts[i].gameObject.SetActiveRecursivelyCompat(state: true);
					}
				}
			}
			if (buyText != null)
			{
				buyText.gameObject.SetActive(false);
			}
		}
		else
		{
			if (!InAppPurshase.IsPurchaseReady())
			{
				return;
			}
			buyMenuReady = true;
			string productPrice = InAppPurshase.GetProductPrice(0);
			if (productPrice == null || productPrice == string.Empty)
			{
				if (buyText != null)
				{
					buyText.text = "Unable connect to store";
				}
				return;
			}
			if (buyText != null)
			{
				buyText.gameObject.SetActive(false);
			}
			if (ChooseLevelGUI.IsFreeVersion())
			{
				if (buyFullVersion != null)
				{
					buyFullVersion.SetActiveRecursivelyCompat(state: true);
				}
				if (buyFullVerText != null)
				{
					buyFullVerText.text = ChooseLevelGUI.GetBuyProductText(3);
				}
			}
			else
			{
				if (buyProducts == null)
				{
					return;
				}
				for (int j = 0; j < buyProducts.Length; j++)
				{
					if (buyProducts[j] != null)
					{
						buyProducts[j].gameObject.SetActiveRecursivelyCompat(state: true);
						buyProducts[j].text = ChooseLevelGUI.GetBuyProductText(j);
					}
				}
			}
		}
	}

	public void StartShow()
	{
		base.enabled = true;
		if (GuiGameHud.instance.GetGuiState() == GuiGameHud.eGuiGameHudState.pause)
		{
			GuiGameHud.instance.pauseScreen.Enabled(val: false);
		}
		base.gameObject.SetActiveRecursivelyCompat(state: true);
		if (buyProducts != null)
		{
			TextMesh[] array = buyProducts;
			foreach (TextMesh textMesh in array)
			{
				textMesh.gameObject.SetActiveRecursivelyCompat(state: false);
			}
		}
		if (buyFullVersion != null)
		{
			buyFullVersion.SetActiveRecursivelyCompat(state: false);
		}
		if (buyText != null)
		{
			buyText.text = "Connecting to store";
		}
		buyMenuReady = false;
		InAppPurshase.StartPurchase(isSilent: false);
	}

	public void StopShow()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		base.enabled = false;
		if (GuiGameHud.instance.GetGuiState() == GuiGameHud.eGuiGameHudState.pause)
		{
			GuiGameHud.instance.pauseScreen.Enabled(val: true);
		}
	}

	public override void StartExecutor(string param)
	{
		base.StartExecutor(param);
		switch (param)
		{
		case "exit":
			StopShow();
			break;
		case "BayGhost_lo":
			if (buyMenuReady)
			{
				AddBuyedGhost(0);
			}
			break;
		case "BayGhost_hi":
			if (buyMenuReady)
			{
				AddBuyedGhost(2);
			}
			break;
		case "BayGhost_med":
			if (buyMenuReady)
			{
				AddBuyedGhost(1);
			}
			break;
		case "BuyFullVersion":
			if (buyMenuReady)
			{
				AddBuyedGhost(3);
			}
			break;
		}
	}

	private void AddBuyedGhost(int addQ)
	{
		switch (addQ)
		{
		case 0:
		case 1:
		case 2:
		case 3:
			if (!InAppPurshase.RequestForBuy(addQ))
			{
				break;
			}
			buyIndex = addQ;
			isWaitAsync = true;
			if (buyFullVersion != null)
			{
				buyFullVersion.SetActiveRecursivelyCompat(state: false);
			}
			if (buyProducts != null)
			{
				for (int i = 0; i < buyProducts.Length; i++)
				{
					if (buyProducts[i] != null)
					{
						buyProducts[i].gameObject.SetActiveRecursivelyCompat(state: false);
					}
				}
			}
			if (buyText != null)
			{
				buyText.text = "Purchase processing";
				buyText.gameObject.SetActive(true);
			}
			break;
		}
	}
}
