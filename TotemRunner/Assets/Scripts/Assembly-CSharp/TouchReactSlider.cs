using UnityEngine;

public class TouchReactSlider : MonoBehaviour, TouchEventListener
{
	public int zOrder;

	public Rect activeRect;

	public float snapStepH = 150f;

	public float minY;

	public float maxY = 600f;

	public Transform refTransform;

	public Transform refCam;

	private float curY;

	private float curSpeed;

	private Vector3 startPos = Vector3.zero;

	private TouchControllerForGUI touchController;

	private float startY;

	private float finishY;

	public GameObject upEnableObject;

	public GameObject downEnableObject;

	private void Awake()
	{
		touchController = TouchControllerForGUI.instance;
		touchController.AppendListener(this);
		if (refTransform == null)
		{
			refTransform = base.transform;
		}
		startPos = refTransform.localPosition;
		curSpeed = 0f;
	}

	private void Start()
	{
		curY = 0f;
		UpdatePosition();
	}

	private void Update()
	{
		if (curSpeed != 0f)
		{
			curY += curSpeed * Time.deltaTime;
			if ((curSpeed > 0f && curY >= finishY) || (curSpeed < 0f && curY <= finishY))
			{
				curY = finishY;
				curSpeed = 0f;
			}
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		refTransform.localPosition = startPos + new Vector3(0f, curY, 0f);
		if ((bool)upEnableObject)
		{
			upEnableObject.SetActive(curY > minY + 10f);
		}
		if ((bool)downEnableObject)
		{
			downEnableObject.SetActive(curY < maxY - 10f);
		}
	}

	public bool TouchStart(Vector2 pos)
	{
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		Vector2 vector = pos - (Vector2)base.transform.position;
		if (vector.x >= activeRect.xMin && vector.x <= activeRect.xMax && vector.y >= activeRect.yMin && vector.y <= activeRect.yMax)
		{
			startY = curY;
			return true;
		}
		return false;
	}

	public void TouchFinish(Vector2 posStart, Vector2 posEnd, float deltaTime)
	{
		curSpeed = 0f;
		if (!(snapStepH > 0f))
		{
			return;
		}
		int num = Mathf.FloorToInt(curY / snapStepH + 0.5f);
		if ((float)num * snapStepH < maxY)
		{
			finishY = (float)num * snapStepH;
		}
		else
		{
			finishY = maxY;
		}
		if (finishY != curY)
		{
			if (finishY > curY)
			{
				curSpeed = 200f;
			}
			else
			{
				curSpeed = -200f;
			}
		}
	}

	public void TouchMoving(Vector2 posDelta, float deltaTime)
	{
		curY = startY + posDelta.y;
		if (curY < minY)
		{
			curY = minY;
		}
		if (curY > maxY)
		{
			curY = maxY;
		}
		UpdatePosition();
	}

	public int GetZOrder()
	{
		return zOrder;
	}

	public bool TouchSlide(int slideNum, Vector2 deltaPos, float deltaTime)
	{
		return true;
	}

	public bool TouchClick(int clickNum)
	{
		return true;
	}

	public void VScroll(bool isUp)
	{
		if (!isUp && curY < maxY - 10f)
		{
			finishY = (float)(Mathf.FloorToInt((curY + 10f) / snapStepH) + 1) * snapStepH;
		}
		else if (isUp && curY > minY + 10f)
		{
			finishY = (float)(Mathf.FloorToInt((curY + 10f) / snapStepH) - 1) * snapStepH;
		}
		if (finishY < minY)
		{
			finishY = minY;
		}
		if (finishY > maxY)
		{
			finishY = maxY;
		}
		if (finishY > curY)
		{
			curSpeed = 800f;
		}
		else if (finishY < curY)
		{
			curSpeed = -800f;
		}
	}
}
