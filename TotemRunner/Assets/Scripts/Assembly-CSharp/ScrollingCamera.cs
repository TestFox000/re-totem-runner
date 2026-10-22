using System;
using UnityEngine;

public class ScrollingCamera : BaseCamera
{
	[Serializable]
	public class CameraZoomingParams
	{
		public string id;

		public float scale = 1f;

		public float time;

		public float delay;

		public float springiness = 4f;

		public Vector2 offset = new Vector2(-340f, 200f);

		public bool isRelativeOffsetX = true;

		public bool lockPosY = true;

		public float offsetBlendingSpeed;
	}

	[Serializable]
	public class CameraZooming
	{
		public CameraZoomingParams[] templates;

		[NonSerialized]
		public ECameraZoomingType zoomCurrent;

		private float timeLeft;

		private float defaultSpringiness;

		private float scale;

		private float curOffsetX;

		public void Init(float springiness)
		{
			defaultSpringiness = springiness;
			zoomCurrent = ECameraZoomingType.none;
			scale = 1f;
			timeLeft = 0f;
			curOffsetX = 0f;
		}

		public void Update()
		{
			if (zoomCurrent != ECameraZoomingType.none && timeLeft > 0f)
			{
				timeLeft -= TimeController.instance.RealDelta;
				if (timeLeft <= 0f)
				{
					SetCurrent(ECameraZoomingType.none);
				}
			}
		}

		public void SetCurrent(ECameraZoomingType type)
		{
			if (type != zoomCurrent)
			{
				zoomCurrent = type;
				if (type != ECameraZoomingType.none)
				{
					scale = templates[(int)type].scale;
					timeLeft = templates[(int)type].time;
				}
				else
				{
					scale = 1f;
				}
			}
		}

		public float GetScale()
		{
			return (zoomCurrent != ECameraZoomingType.none) ? scale : 1f;
		}

		public float GetSpringiness()
		{
			return (zoomCurrent != ECameraZoomingType.none) ? templates[(int)zoomCurrent].springiness : defaultSpringiness;
		}

		public bool IsZooming()
		{
			return zoomCurrent != ECameraZoomingType.none;
		}

		public Vector3 ModifyGoalPosition(Vector3 pos, float dist, float lockHeight)
		{
			if (zoomCurrent != ECameraZoomingType.none)
			{
				float num = scale - 1f;
				float num2 = curOffsetX;
				if (templates[(int)zoomCurrent].isRelativeOffsetX)
				{
					curOffsetX = templates[(int)zoomCurrent].offset.x * num;
				}
				else
				{
					curOffsetX = templates[(int)zoomCurrent].offset.x;
				}
				if (templates[(int)zoomCurrent].offsetBlendingSpeed > 0f && !Mathf.Approximately(num2, curOffsetX))
				{
					curOffsetX = Mathf.MoveTowards(num2, curOffsetX, templates[(int)zoomCurrent].offsetBlendingSpeed * Time.deltaTime);
				}
				pos.x += curOffsetX;
				if (templates[(int)zoomCurrent].lockPosY)
				{
					pos.y = lockHeight;
				}
				pos.y += templates[(int)zoomCurrent].offset.y * num;
				pos.z -= num * dist;
			}
			else
			{
				pos.y = lockHeight;
			}
			return pos;
		}
	}

	public ObjectEnabledByDistance[] listOfObjectsForVisibleActivation2;

	public Transform target;

	public float distance = 800f;

	public float springiness = 4f;

	[NonSerialized]
	public float zSpringinessMultiply = 1f;

	public Vector2 offset = new Vector2(340f, 220f);

	public float xFreedomMax;

	public float camHeight = 160f;

	public float cameraSpeed = 7000f;

	public float cameraSpeedupSpeed = 7000f;

	public float cameraZoomOffsetX;

	public CameraZooming zooming = new CameraZooming();

	private int lastActivatedObject = -1;

	private ObjectEnabledByDistance rootActivatedObject;

	private Rect levelBounds;

	private bool cameraFreezed;

	public static float leftCameraBound;

	public static float rightCameraBound = 100f;

	public static float fieldOfViewMultiply = 1f;

	private static ScrollingCamera _singleton;

	public new static ScrollingCamera instance => _singleton;

	public override void Awake()
	{
		_singleton = this;
		base.Awake();
		cameraFreezed = false;
		if (LevelAttributes.instance != null)
		{
			levelBounds = LevelAttributes.instance.bounds;
		}
		else
		{
			levelBounds = new Rect(-800f, 600f, 20000f, 1000f);
		}
		zooming.Init(springiness);
		base.transform.position = GetGoalPosition();
		ObjectEnabledByDistance[] array = listOfObjectsForVisibleActivation2;
		foreach (ObjectEnabledByDistance objectEnabledByDistance in array)
		{
			objectEnabledByDistance.ObjectAwake();
		}
		fieldOfViewMultiply = Mathf.Tan(base.GetComponent<Camera>().fieldOfView * ((float)Math.PI / 180f));
	}

	public void Start()
	{
		levelBounds = LevelAttributes.instance.bounds;
	}

	public void TeleportToTarget()
	{
		base.transform.position = GetGoalPosition();
	}

	public override void LateUpdate()
	{
		zooming.Update();
		Vector3 goalPosition = GetGoalPosition();
		float t = Time.deltaTime * zooming.GetSpringiness();
		base.transform.position = Vector3.Lerp(base.transform.position, goalPosition, t);
		rightCameraBound = base.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1f, 0f, 0f - base.transform.position.z)).x;
		for (int i = lastActivatedObject + 1; i < listOfObjectsForVisibleActivation2.Length; i++)
		{
			float num = rightCameraBound + listOfObjectsForVisibleActivation2[i].enabledDistance;
			if (listOfObjectsForVisibleActivation2[i].transform.position.x <= num)
			{
				AddObjectToActivatedList(i);
				lastActivatedObject = i;
				listOfObjectsForVisibleActivation2[i].ObjectEnable();
			}
		}
		leftCameraBound = base.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0f, 0f, 0f - base.transform.position.z)).x;
		ObjectEnabledByDistance objectEnabledByDistance = null;
		float num2 = leftCameraBound - 500f;
		ObjectEnabledByDistance objectEnabledByDistance2 = rootActivatedObject;
		while (objectEnabledByDistance2 != null)
		{
			ObjectEnabledByDistance next = objectEnabledByDistance2.next;
			float num3 = objectEnabledByDistance2.transform.position.x + fieldOfViewMultiply * objectEnabledByDistance2.transform.position.z;
			if (num3 < num2)
			{
				if ((bool)objectEnabledByDistance)
				{
					objectEnabledByDistance.next = objectEnabledByDistance2.next;
				}
				else
				{
					rootActivatedObject = objectEnabledByDistance2.next;
				}
				objectEnabledByDistance2.ObjectDisable();
			}
			else
			{
				objectEnabledByDistance = objectEnabledByDistance2;
			}
			objectEnabledByDistance2 = next;
		}
		base.LateUpdate();
	}

	public override void OnPostRender()
	{
		if (!(Camera.current != base.GetComponent<Camera>()))
		{
			base.OnPostRender();
			if ((bool)GrassGifManager.instance)
			{
				GrassGifManager.instance.DrawGrassEnvironment();
			}
			EffectControl.DrawEffectsList();
		}
	}

	private void AddObjectToActivatedList(int objectIndex)
	{
		ObjectEnabledByDistance objectEnabledByDistance = listOfObjectsForVisibleActivation2[objectIndex];
		if ((bool)objectEnabledByDistance.next)
		{
			Debug.Log(string.Concat(Time.time, ": Object ", objectEnabledByDistance, " already activated"));
			return;
		}
		ObjectEnabledByDistance next = rootActivatedObject;
		while ((bool)next)
		{
			if (next == objectEnabledByDistance)
			{
				return;
			}
			next = next.next;
		}
		objectEnabledByDistance.next = rootActivatedObject;
		rootActivatedObject = objectEnabledByDistance;
	}

	private Vector3 GetGoalPosition()
	{
		if (cameraFreezed)
		{
			return base.transform.position;
		}
		Vector3 vector;
		if (target == null)
		{
			vector = base.transform.position;
			vector.x += Time.deltaTime * cameraSpeed;
		}
		else
		{
			if (target.position.x >= levelBounds.xMax + 150f)
			{
				LevelCompleteEvent.Launch();
				return base.transform.position;
			}
			vector = target.position + new Vector3(offset.x, offset.y, 0f - distance);
			vector = zooming.ModifyGoalPosition(vector, distance, camHeight);
			vector.x = Mathf.Clamp(base.transform.position.x + Time.deltaTime * cameraSpeedupSpeed, vector.x - xFreedomMax, vector.x);
		}
		Vector3 zero = Vector3.zero;
		Vector3 position = base.transform.position;
		base.transform.position = vector;
		Vector3 vector2 = ((!(target == null)) ? base.GetComponent<Camera>().WorldToViewportPoint(target.position) : base.GetComponent<Camera>().WorldToViewportPoint(vector + new Vector3(0f - offset.x, 0f - offset.y, distance)));
		Vector3 vector3 = base.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1f, 1f, vector2.z));
		zero.x = Mathf.Min(levelBounds.xMax - vector3.x, 0f);
		vector += zero;
		Vector3 vector4 = base.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0f, 0f, vector2.z));
		zero.x = Mathf.Max(levelBounds.xMin - vector4.x, 0f);
		vector += zero;
		base.transform.position = position;
		return vector;
	}

	public void ResetToX(float x)
	{
		Vector3 position = base.transform.position;
		position.x = x + offset.x;
		base.transform.position = position;
		rightCameraBound = base.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1f, 0f, 0f - base.transform.position.z)).x;
		leftCameraBound = base.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0f, 0f, 0f - base.transform.position.z)).x;
		while (lastActivatedObject >= 0 && !(listOfObjectsForVisibleActivation2[lastActivatedObject].startPosition.x < x))
		{
			lastActivatedObject--;
		}
		ObjectEnabledByDistance objectEnabledByDistance = null;
		ObjectEnabledByDistance objectEnabledByDistance2 = rootActivatedObject;
		while (objectEnabledByDistance2 != null)
		{
			ObjectEnabledByDistance next = objectEnabledByDistance2.next;
			if (objectEnabledByDistance2.startPosition.x >= x)
			{
				if ((bool)objectEnabledByDistance)
				{
					objectEnabledByDistance.next = objectEnabledByDistance2.next;
				}
				else
				{
					rootActivatedObject = objectEnabledByDistance2.next;
				}
				objectEnabledByDistance2.ObjectDisable();
				objectEnabledByDistance2.transform.position = objectEnabledByDistance2.startPosition;
			}
			else
			{
				objectEnabledByDistance = objectEnabledByDistance2;
			}
			objectEnabledByDistance2 = next;
		}
	}

	public void FreezeCamera(bool freezed)
	{
		cameraFreezed = freezed;
	}

	public void LockToTarget(Transform newTarget)
	{
		target = newTarget;
	}

	public void SetZoom(ECameraZoomingType type)
	{
		zooming.SetCurrent(type);
	}

	public float SetOffsetX(float x)
	{
		float x2 = offset.x;
		offset.x = x;
		return x2;
	}
}
