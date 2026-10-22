using UnityEngine;

public class AchievementCollector : MonoBehaviour
{
	private static AchievementSystem achSystem;

	private static AchievementCollector achCollector;

	private bool doCheckSpeedup = true;

	private bool doCheckTutorEnemy = true;

	private int doCheckBoarJumpAttack;

	private int doCheckPowerup;

	private int doCheckArcherDeath;

	private bool doCheckYardmen = true;

	private int doCheckBrainDeath;

	private int doCheckPoacherDeath;

	private int doCheckCrystalQuantity;

	private bool isBoarSpeedup;

	private float boarSpeedupStartX;

	private EPlayerState prevPlayerState;

	private void Awake()
	{
		achCollector = this;
		achSystem = AchievementSystem.Instance();
		if (!achSystem)
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		doCheckSpeedup = true;
		doCheckTutorEnemy = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 3;
		doCheckBoarJumpAttack = PlayerPrefs.GetInt("boarJumpAttack", 0);
		doCheckPowerup = PlayerPrefs.GetInt("powerupStarts", 0);
		doCheckArcherDeath = PlayerPrefs.GetInt("archerDeath", 0);
		doCheckYardmen = true;
		doCheckBrainDeath = PlayerPrefs.GetInt("brainDeath", 0);
		doCheckPoacherDeath = PlayerPrefs.GetInt("poacherDeath", 0);
		doCheckCrystalQuantity = PlayerPrefs.GetInt("achCrystalCount", 0);
	}

	private void Update()
	{
		if (doCheckSpeedup)
		{
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.boarSpeedup)
			{
				if (!isBoarSpeedup)
				{
					isBoarSpeedup = true;
					boarSpeedupStartX = PlayerController.instance.transform.position.x;
				}
				else if ((double)(PlayerController.instance.transform.position.x - boarSpeedupStartX) >= 1000.0)
				{
					achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_boarSpeedup);
					doCheckSpeedup = false;
				}
			}
			else
			{
				isBoarSpeedup = false;
			}
		}
		if (doCheckTutorEnemy && GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.kill) >= LevelAttributes.instance.enemyQuantity)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_tutorEnemyDestroy);
			doCheckTutorEnemy = false;
		}
		if (doCheckBoarJumpAttack >= 0)
		{
			if (LevelAttributes.instance.GetPlayerState() == EPlayerState.boarAttack && (prevPlayerState == EPlayerState.boarFall || prevPlayerState == EPlayerState.boarGround))
			{
				doCheckBoarJumpAttack++;
				switch (doCheckBoarJumpAttack)
				{
				case 1:
					achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_boarDeadlyJump1);
					break;
				case 3:
					achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_boarDeadlyJump3);
					break;
				case 10:
					achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_boarDeadlyJump10);
					doCheckBoarJumpAttack = -1;
					break;
				}
				PlayerPrefs.SetInt("boarJumpAttack", doCheckBoarJumpAttack);
			}
			prevPlayerState = LevelAttributes.instance.GetPlayerState();
		}
		if (doCheckYardmen && GrassGifManager.instance.GetGrassCount() >= 50)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_yardmen);
			doCheckYardmen = false;
		}
	}

	public static void CompleteLevelCheck()
	{
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 16 && GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.deathCount) == 0)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_deathFree);
		}
		switch (GameSettings.GetBlockIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex))
		{
		case 1:
			if (!AchievementSystem.UtiliteIsReachAchievement(AchievementSystem.eAchievementID.ach_block1Complete) && GameSettings.gameSettings.records.blockData[1].level[0].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[1].level[1].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[1].level[2].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[1].level[3].lockStatus == GameRecordsData.eLockStatus.unlock)
			{
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_block1Complete);
			}
			break;
		case 2:
			if (!AchievementSystem.UtiliteIsReachAchievement(AchievementSystem.eAchievementID.ach_block2Complete) && GameSettings.gameSettings.records.blockData[2].level[0].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[2].level[1].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[2].level[2].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[2].level[3].lockStatus == GameRecordsData.eLockStatus.unlock)
			{
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_block2Complete);
			}
			break;
		case 3:
			if (!AchievementSystem.UtiliteIsReachAchievement(AchievementSystem.eAchievementID.ach_block3Complete) && GameSettings.gameSettings.records.blockData[3].level[0].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[3].level[1].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[3].level[2].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[3].level[3].lockStatus == GameRecordsData.eLockStatus.unlock)
			{
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_block3Complete);
			}
			break;
		case 4:
			if (!AchievementSystem.UtiliteIsReachAchievement(AchievementSystem.eAchievementID.ach_block4Complete) && GameSettings.gameSettings.records.blockData[4].level[0].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[4].level[1].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[4].level[2].lockStatus == GameRecordsData.eLockStatus.unlock && GameSettings.gameSettings.records.blockData[4].level[3].lockStatus == GameRecordsData.eLockStatus.unlock)
			{
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_block4Complete);
			}
			break;
		}
		if (GameSettings.GetBlockIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex) == 1 && GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.gem) == 0)
		{
			int num = PlayerPrefs.GetInt("notCrystalMask", 0);
			if (num != 15)
			{
				switch (GameSettings.GetLevelIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex))
				{
				case 0:
					num |= 1;
					break;
				case 1:
					num |= 2;
					break;
				case 2:
					num |= 4;
					break;
				case 3:
					num |= 8;
					break;
				}
				PlayerPrefs.SetInt("notCrystalMask", num);
				if (num == 15)
				{
					achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_crystalFree);
				}
			}
		}
		int allStarQuantity = GameSettings.GetLevelCompleteData().allStarQuantity;
		if (allStarQuantity >= 5)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_stars5);
		}
		if (allStarQuantity >= 10)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_stars10);
		}
		if (allStarQuantity >= 15)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_stars15);
		}
		if (allStarQuantity >= 50)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_stars50);
		}
	}

	public static void DangerDeath(DangerObject danger)
	{
		if (!achCollector)
		{
			return;
		}
		if (achCollector.doCheckArcherDeath < 100 && danger.GetDangerType() == DangerObject.eDangerType.archer)
		{
			achCollector.doCheckArcherDeath++;
			PlayerPrefs.SetInt("archerDeath", achCollector.doCheckArcherDeath);
			switch (achCollector.doCheckArcherDeath)
			{
			case 5:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_golemHunter);
				break;
			case 25:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_golemSlayer);
				break;
			case 100:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_terminator);
				break;
			}
		}
		if (achCollector.doCheckBrainDeath < 10 && danger.GetDangerType() == DangerObject.eDangerType.brain && LevelAttributes.instance.GetPlayerState() != EPlayerState.manDeath && LevelAttributes.instance.GetPlayerState() != EPlayerState.eagleDeath && LevelAttributes.instance.GetPlayerState() != EPlayerState.boarDeath)
		{
			achCollector.doCheckBrainDeath++;
			PlayerPrefs.SetInt("brainDeath", achCollector.doCheckBrainDeath);
			switch (achCollector.doCheckBrainDeath)
			{
			case 1:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_brainExploder1);
				break;
			case 3:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_brainExploder3);
				break;
			case 10:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_brainExploder10);
				break;
			}
		}
		if (achCollector.doCheckPoacherDeath < 3 && danger.GetDangerType() == DangerObject.eDangerType.poacher)
		{
			achCollector.doCheckPoacherDeath++;
			PlayerPrefs.SetInt("poacherDeath", achCollector.doCheckPoacherDeath);
			int num = achCollector.doCheckPoacherDeath;
			if (num == 3)
			{
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_killPoacher3);
			}
		}
	}

	public static void GhostPowerupStart()
	{
		if ((bool)achCollector && achCollector.doCheckPowerup < 25)
		{
			achCollector.doCheckPowerup++;
			PlayerPrefs.SetInt("powerupStarts", achCollector.doCheckPowerup);
			switch (achCollector.doCheckPowerup)
			{
			case 3:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_ghostPower3);
				break;
			case 10:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_ghostPower10);
				break;
			case 25:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_ghostPower25);
				break;
			}
		}
	}

	public static void AddGrassSeed(int seedIdx)
	{
		GrassGifManager instance = GrassGifManager.instance;
		if ((bool)instance && instance.herbals[GrassGifManager.superGrassIndex].isTaken)
		{
			achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_herbalist);
		}
	}

	public static void AddCrystal()
	{
		if (achCollector.doCheckCrystalQuantity < 1000)
		{
			achCollector.doCheckCrystalQuantity++;
			PlayerPrefs.SetInt("achCrystalCount", achCollector.doCheckCrystalQuantity);
			switch (achCollector.doCheckCrystalQuantity)
			{
			case 250:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_crystal250);
				break;
			case 500:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_crystal500);
				break;
			case 1000:
				achSystem.ReachAchievement(AchievementSystem.eAchievementID.ach_crystal1000);
				break;
			}
		}
	}
}
