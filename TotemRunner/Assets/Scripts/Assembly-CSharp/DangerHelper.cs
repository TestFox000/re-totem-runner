using UnityEngine;

public class DangerHelper : MonoBehaviour
{
	public DangerObject[] dangerList;

	private int lastEnteredDanger;

	public static Transform tempDangersFolder;

	public static float gridScale = 0.01f;

	public static float visibleLeftBound = 1000f;

	public static float visibleRightBound = 1500f;

	private OrderListLoop visibleList;

	private int currentGrid;

	private int nextVisibleDangerIndex;

	public int scoreForKill = 10;

	private static DangerHelper _singleton;

	public static DangerHelper instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = Object.FindObjectOfType(typeof(DangerHelper)) as DangerHelper;
			}
			return _singleton;
		}
	}

	public static void IncludeTempDanger(DangerObject danger)
	{
		danger.transform.parent = tempDangersFolder;
		danger.DoVisible(makeVisible: true);
		_singleton.visibleList.InsertNod(danger);
	}

	public static void DeleteAllTempDangers()
	{
		foreach (Transform item in tempDangersFolder)
		{
			Object.Destroy(item.gameObject);
		}
	}

	public static DangerObject GetNearestDangerObject(Transform srcTransform, float maxDist)
	{
		if (_singleton == null || _singleton.dangerList == null || _singleton.dangerList.Length == 0)
		{
			return null;
		}
		float x = srcTransform.position.x;
		int num = 0;
		int num2 = 0;
		int num3 = _singleton.dangerList.Length - 1;
		while (num2 < num3 - 1)
		{
			num = (num2 + num3) / 2;
			float x2 = _singleton.dangerList[num].transform.position.x;
			if (x2 < x)
			{
				num2 = num;
			}
			else
			{
				num3 = num;
			}
		}
		for (num = num2; num < _singleton.dangerList.Length; num++)
		{
			float x3 = _singleton.dangerList[num].transform.position.x;
			if (!(x3 < x))
			{
				if (x3 - x > maxDist)
				{
					break;
				}
				if (_singleton.dangerList[num].GetDangerType() >= DangerObject.eDangerType.archer)
				{
					return _singleton.dangerList[num];
				}
			}
		}
		return null;
	}

	public static void ResetToX(float x)
	{
		if (_singleton == null || !_singleton.enabled || _singleton.dangerList == null)
		{
			return;
		}
		DeleteAllTempDangers();
		int num = _singleton.FindFirstDangerFromX(x);
		if (num < 0)
		{
			return;
		}
		int num2 = Mathf.FloorToInt((ScrollingCamera.leftCameraBound - visibleLeftBound) * gridScale);
		_singleton.visibleList.StartEnum();
		while (_singleton.visibleList.IsHaveEnum())
		{
			DangerObject dangerObject = _singleton.visibleList.GetEnum() as DangerObject;
			dangerObject.DoVisible(makeVisible: false);
			_singleton.visibleList.NextEnum();
		}
		_singleton.visibleList.EndEnum();
		_singleton.visibleList.ClearList();
		int i = _singleton.FindFirstDangerFromX(ScrollingCamera.leftCameraBound - visibleLeftBound);
		if (i < 0)
		{
			i = _singleton.dangerList.Length;
		}
		int num3 = _singleton.FindFirstDangerFromX(ScrollingCamera.rightCameraBound + visibleRightBound);
		if (num3 < 0)
		{
			num3 = _singleton.dangerList.Length;
		}
		for (; i < num3; i++)
		{
			DangerObject dangerObject = _singleton.dangerList[i];
			if (GameSettings.gameSettings.gameParam.hardMode || !dangerObject.isHardOnly)
			{
				dangerObject.DoVisible(makeVisible: true);
				_singleton.visibleList.InsertNod(dangerObject);
				if (i < num && dangerObject.transform.position.x > x)
				{
					dangerObject.SetTargetController(PlayerController.instance);
					dangerObject.OnPlayerReenter();
				}
			}
		}
		_singleton.nextVisibleDangerIndex = num3;
		_singleton.currentGrid = num2;
		while (_singleton.lastEnteredDanger > num)
		{
			_singleton.lastEnteredDanger--;
			if (GameSettings.gameSettings.gameParam.hardMode || !_singleton.dangerList[_singleton.lastEnteredDanger].isHardOnly)
			{
				_singleton.dangerList[_singleton.lastEnteredDanger].OnResetDanger();
			}
		}
	}

	public static float DistanceFromObject(ITargetController target, float x)
	{
		return x - target.GetCollisionRect().xMax;
	}

	public static bool CheckObstacleCollision(float top, float bottom, ITargetController target)
	{
		Rect collisionRect = target.GetCollisionRect();
		return collisionRect.yMin < top && collisionRect.yMax > bottom;
	}

	public static bool CheckColliderCollision(Collider checkCollider, ITargetController target)
	{
		Rect collisionRect = target.GetCollisionRect();
		float width = collisionRect.width;
		Ray ray = new Ray(new Vector3(collisionRect.xMin, collisionRect.yMin, checkCollider.transform.position.z), Vector3.right);
		if (checkCollider.Raycast(ray, out var hitInfo, width))
		{
			return true;
		}
		ray.origin = new Vector3(collisionRect.xMin, collisionRect.yMax, checkCollider.transform.position.z);
		if (checkCollider.Raycast(ray, out hitInfo, width))
		{
			return true;
		}
		return false;
	}

	private void Awake()
	{
		_singleton = this;
		tempDangersFolder = base.transform.Find("TempDangers");
		NonReleaseVersionPrepare();
		visibleList = new OrderListLoop(100);
		currentGrid = -1000;
		nextVisibleDangerIndex = 0;
		if (dangerList != null)
		{
			DangerObject[] array = dangerList;
			foreach (DangerObject dangerObject in array)
			{
				dangerObject.DangerAwake();
			}
		}
	}

	private void NonReleaseVersionPrepare()
	{
	}

	public void StartFirstRespawn(float x)
	{
		lastEnteredDanger = FindFirstDangerFromX(x);
		if (lastEnteredDanger < 0)
		{
			lastEnteredDanger = dangerList.Length;
		}
	}

	private void LateUpdate()
	{
		if (!(Time.deltaTime <= 0f))
		{
			visibleList.StartEnum();
			while (visibleList.IsHaveEnum())
			{
				DangerObject dangerObject = visibleList.GetEnum() as DangerObject;
				dangerObject.LateDangerUpdate();
				visibleList.NextEnum();
			}
			visibleList.EndEnum();
		}
	}

	private void Update()
	{
		if (Time.deltaTime <= 0f || dangerList == null)
		{
			return;
		}
		int num = Mathf.FloorToInt((ScrollingCamera.leftCameraBound - visibleLeftBound) * gridScale);
		if (currentGrid != num)
		{
			currentGrid = num;
			while (visibleList.GetListQuantity() > 0)
			{
				SortableByInt firstSortable = visibleList.GetFirstSortable();
				if (firstSortable.GetOrder() >= currentGrid)
				{
					break;
				}
				visibleList.ShiftNod();
				DangerObject dangerObject = firstSortable as DangerObject;
				dangerObject.DoVisible(makeVisible: false);
			}
		}
		int frameCount = Time.frameCount;
		visibleList.StartEnum();
		while (visibleList.IsHaveEnum())
		{
			DangerObject dangerObject = visibleList.GetEnum() as DangerObject;
			if (!dangerObject)
			{
				Debug.LogWarning(Time.time + ": VisibleList enum is not DangeObject " + visibleList.GetEnum());
				visibleList.DebugTrace();
			}
			if (dangerObject.updateFrame != frameCount)
			{
				dangerObject.updateFrame = frameCount;
				dangerObject.DangerUpdate();
			}
			visibleList.NextEnum();
		}
		visibleList.EndEnum();
		while (nextVisibleDangerIndex < dangerList.Length && dangerList[nextVisibleDangerIndex].GetDangerLeft() - ScrollingCamera.rightCameraBound < visibleRightBound)
		{
			if (GameSettings.gameSettings.gameParam.hardMode || !dangerList[nextVisibleDangerIndex].isHardOnly)
			{
				dangerList[nextVisibleDangerIndex].DoVisible(makeVisible: true);
				visibleList.InsertNod(dangerList[nextVisibleDangerIndex]);
			}
			nextVisibleDangerIndex++;
		}
		float x = PlayerController.instance.transform.position.x;
		while (lastEnteredDanger < nextVisibleDangerIndex && !(dangerList[lastEnteredDanger].GetDangerLeft() - x > 0f))
		{
			if (GameSettings.gameSettings.gameParam.hardMode || !dangerList[lastEnteredDanger].isHardOnly)
			{
				dangerList[lastEnteredDanger].DangerEnterPlayer(PlayerController.instance, lastEnteredDanger);
			}
			lastEnteredDanger++;
		}
	}

	public static void OnPlayerDeath()
	{
		if (_singleton == null || _singleton.visibleList == null)
		{
			return;
		}
		_singleton.visibleList.StartEnum();
		while (_singleton.visibleList.IsHaveEnum())
		{
			DangerObject dangerObject = _singleton.visibleList.GetEnum() as DangerObject;
			if (dangerObject.GetDangerState() != DangerObject.eDangerObjectState.inactive)
			{
				dangerObject.OnPlayerExit();
			}
			_singleton.visibleList.NextEnum();
		}
		_singleton.visibleList.EndEnum();
	}

	private int FindFirstDangerFromX(float x)
	{
		int num = 0;
		int num2 = dangerList.Length - 1;
		while (num < num2 - 1)
		{
			int num3 = (num + num2) / 2;
			float dangerLeft = dangerList[num3].GetDangerLeft();
			if (dangerLeft < x)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		if (num < dangerList.Length && dangerList[num].GetDangerLeft() >= x)
		{
			return num;
		}
		if (num2 > 0 && dangerList[num2].GetDangerLeft() >= x)
		{
			return num2;
		}
		return -1;
	}

	public static float GetDistanceToNextDanger(DangerObject objDanger, float x)
	{
		DangerObject.eDangerType dangerType = objDanger.GetDangerType();
		if (dangerType == DangerObject.eDangerType.brain || dangerType == DangerObject.eDangerType.spider)
		{
			return 10000f;
		}
		int num = _singleton.FindFirstDangerFromX(objDanger.GetDangerLeft());
		if (num < 0)
		{
			return 10000f;
		}
		while (num > 0 && !(_singleton.dangerList[num] == objDanger))
		{
			num--;
		}
		int num2 = 0;
		DangerObject.eDangerType dangerType2 = objDanger.GetDangerType();
		num++;
		while (num2 < 10 && num < _singleton.dangerList.Length)
		{
			num2++;
			if (_singleton.dangerList[num].GetDangerType() == dangerType2)
			{
				break;
			}
			num++;
		}
		if (num >= _singleton.dangerList.Length)
		{
			return 10000f;
		}
		return _singleton.dangerList[num].transform.position.x - objDanger.transform.position.x;
	}

	public static void OnMoveDanger(DangerObject objDanger)
	{
		_singleton.visibleList.OnChangeOrder(objDanger);
	}
}
