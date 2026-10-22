using UnityEngine;

public class TutorialProcessor : MonoBehaviour
{
	public int beginGrassQuantity = 10;

	public TutorialMessage beginGrassMessage;

	private int nGrassCount;

	public int beginCrystalQuantity = 3;

	public TutorialMessage beginCrystalMessage;

	private int nCrystalCount;

	public float enableButtonDistance_SkinBoar;

	public float enableButtonDistance_SkinEagle;

	public float enableButtonDistance_GrowGrass;

	public float enableButtonDistance_PowerUp;

	private float curDistance;

	private bool enableGrass;

	private TutorialMessage nextTimeTutor;

	private int nextTimeTutorCountdown = -1;

	private static TutorialProcessor _singleton;

	public static TutorialProcessor instance => _singleton;

	public bool IsEnableGrass
	{
		get
		{
			return enableGrass;
		}
		set
		{
			enableGrass = value;
		}
	}

	private void Awake()
	{
		_singleton = this;
		curDistance = -10000f;
		nGrassCount = ((!(beginGrassMessage != null)) ? (-1) : 0);
		nCrystalCount = ((!(beginCrystalMessage != null)) ? (-1) : 0);
	}

	private void Start()
	{
		GameInput.instance.HideControl(eGameInputButton.growing, hideFlag: true);
		GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: true);
		GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: true);
		GameInput.instance.HideControl(eGameInputButton.powerUp, hideFlag: true);
	}

	public void SetNextTimeTutor(TutorialMessage tutMsg, int framedelay)
	{
		nextTimeTutorCountdown = framedelay;
		nextTimeTutor = tutMsg;
	}

	private void Update()
	{
		if (nextTimeTutorCountdown >= 0)
		{
			nextTimeTutorCountdown--;
			if (nextTimeTutorCountdown <= 0)
			{
				nextTimeTutorCountdown = -1;
				if (nextTimeTutor != null)
				{
					nextTimeTutor.ShowMessage();
				}
			}
		}
		if (nGrassCount >= 0 && nGrassCount < beginGrassQuantity)
		{
			nGrassCount = GrassGifManager.instance.GetGrassCount();
			if (nGrassCount >= beginGrassQuantity)
			{
				nGrassCount = -1;
				LaunchTutorialEvent(beginGrassMessage);
			}
		}
		if (nCrystalCount >= 0 && nCrystalCount < beginCrystalQuantity)
		{
			nCrystalCount = GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.gem);
			if (nCrystalCount >= beginCrystalQuantity)
			{
				nCrystalCount = -1;
				LaunchTutorialEvent(beginCrystalMessage);
			}
		}
		float x = PlayerController.instance.transform.position.x;
		if (curDistance < enableButtonDistance_SkinBoar && x >= enableButtonDistance_SkinBoar)
		{
			GameInput.instance.HideControl(eGameInputButton.skinBoar, hideFlag: false);
		}
		if (curDistance < enableButtonDistance_SkinEagle && x >= enableButtonDistance_SkinEagle)
		{
			GameInput.instance.HideControl(eGameInputButton.skinEagle, hideFlag: false);
		}
		if (curDistance < enableButtonDistance_GrowGrass && x >= enableButtonDistance_GrowGrass)
		{
			GameInput.instance.HideControl(eGameInputButton.growing, hideFlag: false);
		}
		if (curDistance < enableButtonDistance_PowerUp && x >= enableButtonDistance_PowerUp)
		{
			GameInput.instance.HideControl(eGameInputButton.powerUp, hideFlag: false);
		}
		curDistance = x;
	}

	public void LaunchTutorialEvent(TutorialMessage tutorMsg)
	{
		if ((bool)tutorMsg)
		{
			tutorMsg.ShowMessage();
		}
	}
}
