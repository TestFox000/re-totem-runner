using System;
using UnityEngine;

public class AchievementSystem : MonoBehaviour
{
	public enum eAchievementID
	{
		ach_boarSpeedup = 0,
		ach_crystal250 = 1,
		ach_crystal500 = 2,
		ach_crystal1000 = 3,
		ach_tutorEnemyDestroy = 4,
		ach_deathFree = 5,
		ach_boarDeadlyJump1 = 6,
		ach_boarDeadlyJump3 = 7,
		ach_boarDeadlyJump10 = 8,
		ach_ghostPower3 = 9,
		ach_golemHunter = 10,
		ach_yardmen = 11,
		ach_brainExploder1 = 12,
		ach_block1Complete = 13,
		ach_block2Complete = 14,
		ach_block3Complete = 15,
		ach_block4Complete = 16,
		ach_masterRecordsmen = 17,
		ach_crystalFree = 18,
		ach_herbalist = 19,
		ach_stars5 = 20,
		ach_stars10 = 21,
		ach_stars15 = 22,
		ach_stars50 = 23,
		ach_golemSlayer = 24,
		enc_egg = 25,
		enc_star = 26,
		enc_golem = 27,
		enc_poacher = 28,
		enc_brain = 29,
		enc_crawler = 30,
		enc_flybomb = 31,
		enc_hornet = 32,
		enc_spider = 33,
		enc_titan = 34,
		enc_evilsoul = 35,
		enc_test12 = 36,
		enc_test13 = 37,
		enc_test14 = 38,
		enc_test15 = 39,
		enc_test16 = 40,
		enc_test17 = 41,
		enc_test18 = 42,
		enc_test19 = 43,
		enc_test20 = 44,
		ach_terminator = 45,
		ach_brainExploder3 = 46,
		ach_brainExploder10 = 47,
		ach_ghostPower10 = 48,
		ach_ghostPower25 = 49,
		ach_killPoacher3 = 50,
		ach_completeBoss = 51,
		quantity = 52
	}

	public enum eAchievementType
	{
		achievement = 0,
		encounter = 1
	}

	public enum eMsgState
	{
		invisible = 0,
		popup = 1,
		popdown = 2,
		visible = 3
	}

	[Serializable]
	public class AchievementData
	{
		public string name = string.Empty;

		public bool isReach;

		public eAchievementType achType;

		public string descr = "To reach this achievement#make something";

		public AchievementData(string id, eAchievementType typ)
		{
			name = id;
			isReach = false;
			achType = typ;
			descr = "To reach this achievement#make something";
		}
	}

	private eMsgState msgState;

	private eAchievementType curAchType;

	private Vector3 startPosition;

	public Vector3 popupDistance = Vector3.up * 100f;

	public float popupTimeout = 0.8f;

	private float popupSpeed;

	public float visibleTimeout = 2f;

	private float timeBound;

	private eAchievementID[] memorizeAchiev = new eAchievementID[16];

	private int memorizeAchievQ;

	public TextMesh refText;

	public Renderer refBackRenderer;

	public Renderer refCheckRenderer;

	public Texture2D backTextureAchievement;

	public Texture2D backTextureEncounter;

	public Texture2D backTextureEncounter2;

	private static AchievementSystem _singleton;

	private void Awake()
	{
		_singleton = this;
		startPosition = base.transform.position;
		if (popupTimeout > 0f)
		{
			popupSpeed = 1f / popupTimeout;
		}
		memorizeAchievQ = 0;
	}

	private void Start()
	{
		int num = Mathf.FloorToInt(GuiGameHud.instance.GetComponent<Camera>().ViewportToWorldPoint(Vector3.one).x - GuiGameHud.instance.GetComponent<Camera>().transform.position.x);
		startPosition.x += num - 512;
		base.transform.position = startPosition;
	}

	public static bool UtiliteIsReachAchievement(eAchievementID achId)
	{
		int num = (int)achId / 15;
		int num2 = (int)(achId - num * 15);
		int num3 = PlayerPrefs.GetInt("ach_" + num, 0);
		int num4 = 1;
		for (int i = 0; i < num2; i++)
		{
			num4 *= 2;
		}
		return (num3 & num4) != 0;
	}

	private void SaveAchievement(eAchievementID achId)
	{
		int num = (int)achId / 15;
		int num2 = (int)(achId - num * 15);
		int num3 = PlayerPrefs.GetInt("ach_" + num, 0);
		int num4 = 1;
		for (int i = 0; i < num2; i++)
		{
			num4 *= 2;
		}
		PlayerPrefs.SetInt(value: (!GameSettings.gameSettings.achievements[(int)achId].isReach) ? (num3 & ~num4) : (num3 | num4), key: "ach_" + num);
	}

	public static AchievementSystem Instance()
	{
		if (!_singleton)
		{
			_singleton = UnityEngine.Object.FindObjectOfType(typeof(AchievementSystem)) as AchievementSystem;
		}
		return _singleton;
	}

	public void State(eMsgState state)
	{
		if (state == msgState)
		{
			return;
		}
		if (msgState == eMsgState.invisible)
		{
			base.gameObject.SetActiveRecursivelyCompat(state: true);
			if ((bool)refCheckRenderer)
			{
				refCheckRenderer.gameObject.SetActive(false);
			}
		}
		switch (state)
		{
		case eMsgState.invisible:
			if (memorizeAchievQ == 0)
			{
				base.gameObject.SetActiveRecursivelyCompat(state: false);
			}
			base.transform.position = startPosition;
			break;
		case eMsgState.popup:
			timeBound = Time.realtimeSinceStartup + popupTimeout;
			break;
		case eMsgState.popdown:
			timeBound = Time.realtimeSinceStartup + popupTimeout;
			break;
		case eMsgState.visible:
			timeBound = Time.realtimeSinceStartup + visibleTimeout;
			base.transform.position = startPosition + popupDistance;
			if (curAchType == eAchievementType.encounter && (bool)refBackRenderer)
			{
				refBackRenderer.material.mainTexture = backTextureEncounter2;
			}
			if ((bool)refCheckRenderer && curAchType == eAchievementType.achievement)
			{
				refCheckRenderer.gameObject.SetActive(true);
			}
			break;
		}
		msgState = state;
	}

	private void Update()
	{
		switch (msgState)
		{
		case eMsgState.invisible:
			if (memorizeAchievQ > 0)
			{
				memorizeAchievQ--;
				MakeShowAchievement(memorizeAchiev[memorizeAchievQ]);
			}
			break;
		case eMsgState.popup:
		{
			if (Time.realtimeSinceStartup >= timeBound)
			{
				State(eMsgState.visible);
				break;
			}
			float num = 1f - popupSpeed * (timeBound - Time.realtimeSinceStartup);
			base.transform.position = startPosition + popupDistance * num;
			break;
		}
		case eMsgState.popdown:
		{
			if (Time.realtimeSinceStartup >= timeBound)
			{
				State(eMsgState.invisible);
				break;
			}
			float num = popupSpeed * (timeBound - Time.realtimeSinceStartup);
			base.transform.position = startPosition + popupDistance * num;
			break;
		}
		case eMsgState.visible:
			if (Time.realtimeSinceStartup >= timeBound)
			{
				State(eMsgState.popdown);
			}
			break;
		}
	}

	public string GetGCAchId(eAchievementID achId)
	{
		if (achId < eAchievementID.ach_golemHunter)
		{
			return "ach_0" + (int)achId;
		}
		return "ach_" + (int)achId;
	}

	public void ReachAchievement(eAchievementID achId)
	{
		if (!GameSettings.gameSettings.achievements[(int)achId].isReach)
		{
			GameSettings.gameSettings.achievements[(int)achId].isReach = true;
			SaveAchievement(achId);
			if (GameSettings.gameSettings.achievements[(int)achId].achType == eAchievementType.achievement)
			{
				CrystalManager.ReportAchievement(GetGCAchId(achId), 100f);
			}
			if (msgState == eMsgState.invisible)
			{
				MakeShowAchievement(achId);
			}
			else
			{
				MemorizeAchievement(achId);
			}
		}
	}

	public static void Reach(eAchievementID achId)
	{
		if (_singleton != null)
		{
			_singleton.ReachAchievement(achId);
		}
	}

	private void MakeShowAchievement(eAchievementID achId)
	{
		if ((bool)refText)
		{
			refText.text = GameSettings.gameSettings.achievements[(int)achId].name;
		}
		if ((bool)refBackRenderer)
		{
			curAchType = GameSettings.gameSettings.achievements[(int)achId].achType;
			switch (curAchType)
			{
			case eAchievementType.achievement:
				refBackRenderer.material.mainTexture = backTextureAchievement;
				refText.GetComponent<Renderer>().material.color = Color.green;
				break;
			case eAchievementType.encounter:
				refBackRenderer.material.mainTexture = backTextureEncounter;
				refText.GetComponent<Renderer>().material.color = Color.red;
				break;
			}
		}
		State(eMsgState.popup);
	}

	private void MemorizeAchievement(eAchievementID achId)
	{
		if (memorizeAchievQ < memorizeAchiev.Length)
		{
			memorizeAchiev[memorizeAchievQ] = achId;
			memorizeAchievQ++;
		}
		else
		{
			Debug.LogWarning("Warning! Achievement pool is overflow");
		}
	}
}
