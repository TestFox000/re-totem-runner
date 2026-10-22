using System;
using UnityEngine;

public class LevelTaskManager : EventExecutor
{
	public enum eTaskId
	{
		gems = 0,
		seeds = 1,
		time = 2,
		kill = 3,
		berry = 4,
		none = -1
	}

	[Serializable]
	public class TaskTemplate
	{
		public string describe;

		public Texture2D image;
	}

	private class TasksDescribe
	{
		public eTaskId taskId = eTaskId.none;

		public bool precomplete;

		public int maxCount;

		public int curCount;
	}

	private static LevelTaskManager _singleton;

	public TaskTemplate[] tasks = new TaskTemplate[4];

	private TasksDescribe[] curTask;

	public Transform formTransform;

	public Material incompleteFontMaterial;

	private bool isStartForm = true;

	private bool isShowForm;

	private bool tapIsRealized;

	public static LevelTaskManager instance => _singleton;

	public bool IsShowForm => isShowForm;

	private void Awake()
	{
		_singleton = this;
		if (formTransform != null)
		{
			formTransform.gameObject.SetActiveRecursivelyCompat(state: false);
		}
	}

	private void Start()
	{
		curTask = new TasksDescribe[3];
		for (int i = 0; i < 3; i++)
		{
			curTask[i] = new TasksDescribe();
		}
	}

	private void Update()
	{
		if (!isShowForm)
		{
			return;
		}
		if (tapIsRealized)
		{
			if (Input.GetMouseButtonUp(0))
			{
				StartExecutor();
			}
		}
		else if (!Input.GetMouseButton(0))
		{
			tapIsRealized = true;
		}
	}

	private int GetMaxCounter(eTaskId taskId)
	{
		int result = 0;
		switch (taskId)
		{
		case eTaskId.gems:
			result = LevelAttributes.instance.crystalQuantity;
			break;
		case eTaskId.seeds:
			result = LevelAttributes.instance.seedQuantity;
			break;
		case eTaskId.time:
			result = LevelAttributes.instance.timeLength;
			break;
		case eTaskId.kill:
			result = LevelAttributes.instance.enemyQuantity;
			break;
		case eTaskId.berry:
			result = 10;
			break;
		default:
			Debug.Log(Time.time + ": Unknown task counter " + taskId);
			break;
		}
		return result;
	}

	private bool CheckComplete(int taskIndex)
	{
		if (curTask == null)
		{
			return false;
		}
		bool result = false;
		switch (curTask[taskIndex].taskId)
		{
		case eTaskId.gems:
			if (GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.gem) >= curTask[taskIndex].maxCount)
			{
				result = true;
			}
			break;
		case eTaskId.seeds:
			if (GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.seed) >= curTask[taskIndex].maxCount)
			{
				result = true;
			}
			break;
		case eTaskId.time:
			if (Mathf.FloorToInt(GameSettings.GetLevelCompleteData().timer) <= curTask[taskIndex].maxCount)
			{
				result = true;
			}
			break;
		case eTaskId.kill:
			if (GameStatistic.instance.ItemCount(GameStatistic.eStatisticItem.kill) >= curTask[taskIndex].maxCount)
			{
				result = true;
			}
			break;
		case eTaskId.berry:
			if (GrassGifManager.instance.GetGrassCount() - 300 >= curTask[taskIndex].maxCount)
			{
				result = true;
			}
			break;
		default:
			Debug.Log(Time.time + ": Unknown task counter " + taskIndex);
			break;
		}
		return result;
	}

	private void LoadTaskSettings()
	{
		int levelIndex = GameSettings.GetLevelIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		int blockIndex = GameSettings.GetBlockIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		string key = "task" + blockIndex + "_" + levelIndex;
		int num = PlayerPrefs.GetInt(key, 0);
		int num2 = 1;
		for (int i = 0; i < 3; i++)
		{
			curTask[i].taskId = LevelAttributes.instance.taskId[i].id;
			curTask[i].precomplete = (num & num2) != 0;
			if (LevelAttributes.instance.taskId[i].counter > 0)
			{
				curTask[i].maxCount = LevelAttributes.instance.taskId[i].counter;
			}
			else
			{
				curTask[i].maxCount = GetMaxCounter(curTask[i].taskId);
			}
			curTask[i].curCount = 0;
			num2 *= 2;
		}
	}

	private string GetTaskDescribe(eTaskId taskId, int taskCounter)
	{
		return tasks[(int)taskId].describe.Replace("#", "\n").Replace("/n", string.Empty + taskCounter).Replace("/t", StatisticScreen.SecsToTimeString(taskCounter));
	}

	private void PrepareShow()
	{
		formTransform.gameObject.SetActiveRecursivelyCompat(state: true);
		GuiGameHud.instance.HideButton(eGameInputButton.powerUp, hideFlag: true);
	}

	private void ShowTaskForm()
	{
		if (formTransform == null)
		{
			return;
		}
		PrepareShow();
		for (int i = 0; i < 3; i++)
		{
			int taskId = (int)curTask[i].taskId;
			Transform transform = formTransform.Find("task" + (i + 1));
			if (transform == null)
			{
				continue;
			}
			transform.GetComponent<TextMesh>().text = GetTaskDescribe(curTask[i].taskId, curTask[i].maxCount);
			Transform transform2 = transform.Find("image");
			if (transform2 != null)
			{
				transform2.GetComponent<Renderer>().material.mainTexture = tasks[taskId].image;
			}
			if (curTask[i].precomplete)
			{
				transform.GetComponent<Renderer>().enabled = false;
				continue;
			}
			transform.GetComponent<Renderer>().enabled = true;
			Transform transform3 = transform.Find("complete");
			if (transform3 != null)
			{
				transform3.gameObject.SetActiveRecursivelyCompat(state: false);
			}
		}
	}

	private void ShowCompleteForm()
	{
		if (formTransform == null)
		{
			return;
		}
		PrepareShow();
		for (int i = 0; i < 3; i++)
		{
			Transform transform = formTransform.Find("task" + (i + 1));
			if (transform == null)
			{
				continue;
			}
			if (CheckComplete(i))
			{
				transform.GetComponent<Renderer>().enabled = false;
				continue;
			}
			Transform transform2 = transform.Find("complete");
			transform.GetComponent<Renderer>().enabled = false;
			if (curTask[i].precomplete)
			{
				transform2.GetComponent<TextMesh>().text = "complete early";
				continue;
			}
			transform2.GetComponent<TextMesh>().text = "incomplete";
			Transform transform3 = transform2.Find("image");
			transform3.gameObject.SetActiveRecursivelyCompat(state: false);
			transform2.GetComponent<Renderer>().material = incompleteFontMaterial;
		}
	}

	private void ShowEnd()
	{
		if (formTransform != null)
		{
			formTransform.gameObject.SetActiveRecursivelyCompat(state: false);
		}
		TimeController.StartPause(isPause: false);
		WorldEvil.instance.gameObject.SetActiveRecursivelyCompat(state: true);
		ScrollingCamera.instance.FreezeCamera(freezed: false);
		isShowForm = false;
		if (isStartForm)
		{
			PlayerController.instance.LaunchPlayer();
			GuiGameHud.instance.HideButton(eGameInputButton.powerUp, hideFlag: false);
			GuiGameHud.UpdateCrystalProgress();
		}
		else
		{
			StatisticScreen.instance.LaunchAfterTask();
		}
	}

	public override void StartExecutor()
	{
		if (isShowForm)
		{
			base.StartExecutor();
			ShowEnd();
		}
	}

	public void ShowTasks()
	{
		isShowForm = true;
		isStartForm = true;
		tapIsRealized = false;
		ScrollingCamera.instance.FreezeCamera(freezed: true);
		WorldEvil.instance.gameObject.SetActiveRecursivelyCompat(state: false);
		TimeController.StartPause(isPause: true);
		LoadTaskSettings();
		ShowTaskForm();
	}

	public void ShowCompleteResult()
	{
		isShowForm = true;
		isStartForm = false;
		tapIsRealized = false;
		ScrollingCamera.instance.FreezeCamera(freezed: true);
		WorldEvil.instance.gameObject.SetActiveRecursivelyCompat(state: false);
		TimeController.StartPause(isPause: true);
		ShowCompleteForm();
	}
}
