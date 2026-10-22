using UnityEngine;

public class TouchControllerForGUI : MonoBehaviour
{
	public enum eTouchControlEvent
	{
		startTouch = 0,
		endTouch = 1,
		moveTouch = 2
	}

	private class ListenerNode
	{
		public TouchEventListener listener;

		public ListenerNode next;
	}

	private ListenerNode rootListener;

	private TouchEventListener[] reactListener = new TouchEventListener[10];

	private int nListenerReactionQuantity;

	private Vector3 startTouchCameraPos;

	private Vector2 startTouchPos;

	private Vector2 endTouchPos;

	private float deltaTouchTime;

	private Camera refCamera;

	private static TouchControllerForGUI _singleton;

	public static TouchControllerForGUI instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = Object.FindObjectOfType(typeof(TouchControllerForGUI)) as TouchControllerForGUI;
			}
			return _singleton;
		}
	}

	private void Awake()
	{
		_singleton = this;
		refCamera = base.GetComponent<Camera>();
	}

	private void Update()
	{
		if (Input.touchCount > 0)
		{
			switch (Input.GetTouch(0).phase)
			{
			case TouchPhase.Began:
				startTouchCameraPos = refCamera.transform.position;
				startTouchPos = TouchPosModification(Input.GetTouch(0).position);
				endTouchPos = startTouchPos;
				deltaTouchTime = 0f;
				ProcessTouchEvent(eTouchControlEvent.startTouch);
				break;
			case TouchPhase.Ended:
			case TouchPhase.Canceled:
				endTouchPos = TouchPosModification(Input.GetTouch(0).position);
				deltaTouchTime += Time.deltaTime;
				ProcessTouchEvent(eTouchControlEvent.endTouch);
				break;
			case TouchPhase.Moved:
				endTouchPos = TouchPosModification(Input.GetTouch(0).position);
				deltaTouchTime += Time.deltaTime;
				ProcessTouchEvent(eTouchControlEvent.moveTouch);
				break;
			case TouchPhase.Stationary:
				deltaTouchTime += Time.deltaTime;
				break;
			}
		}
	}

	private Vector2 TouchPosModification(Vector2 srcPos)
	{
		Vector3 position = refCamera.transform.position;
		refCamera.transform.position = startTouchCameraPos;
		Vector3 vector = refCamera.ScreenToWorldPoint(new Vector3(srcPos.x, srcPos.y, 100f));
		refCamera.transform.position = position;
		return new Vector2(vector.x, vector.y);
	}

	private void IncludeListenerToTouchReaction(TouchEventListener listener)
	{
		int zOrder = listener.GetZOrder();
		int i;
		for (i = 0; i < nListenerReactionQuantity && zOrder <= reactListener[i].GetZOrder(); i++)
		{
		}
		for (int j = i; j < nListenerReactionQuantity; j++)
		{
			reactListener[j + 1] = reactListener[j];
		}
		reactListener[i] = listener;
		nListenerReactionQuantity++;
	}

	private void ProcessTouchEvent(eTouchControlEvent touchEvent)
	{
		bool flag = true;
		switch (touchEvent)
		{
		case eTouchControlEvent.startTouch:
		{
			for (ListenerNode next = rootListener; next != null; next = next.next)
			{
				if (next.listener.TouchStart(startTouchPos))
				{
					IncludeListenerToTouchReaction(next.listener);
				}
			}
			break;
		}
		case eTouchControlEvent.endTouch:
		{
			Vector2 posDelta = endTouchPos - startTouchPos;
			float num = Mathf.Clamp(10f + 20f * deltaTouchTime, 10f, 30f);
			bool flag2 = posDelta.x < num && posDelta.x > 0f - num && posDelta.y < num && posDelta.y > 0f - num;
			flag = true;
			for (int i = 0; i < nListenerReactionQuantity; i++)
			{
				if (flag)
				{
					if (flag2)
					{
						if (reactListener[i].TouchClick(i))
						{
							flag = false;
						}
					}
					else if (reactListener[i].TouchSlide(i, posDelta, deltaTouchTime))
					{
						flag = false;
					}
				}
				reactListener[i].TouchFinish(startTouchPos, endTouchPos, deltaTouchTime);
			}
			nListenerReactionQuantity = 0;
			break;
		}
		case eTouchControlEvent.moveTouch:
		{
			Vector2 posDelta = endTouchPos - startTouchPos;
			for (int i = 0; i < nListenerReactionQuantity; i++)
			{
				reactListener[i].TouchMoving(posDelta, deltaTouchTime);
			}
			break;
		}
		}
	}

	public void AppendListener(TouchEventListener listener)
	{
		ListenerNode listenerNode = new ListenerNode();
		listenerNode.listener = listener;
		listenerNode.next = rootListener;
		rootListener = listenerNode;
	}

	private void ExcludeListener(TouchEventListener listener)
	{
		ListenerNode listenerNode = null;
		for (ListenerNode next = rootListener; next != null; next = next.next)
		{
			if (next.listener == listener)
			{
				if (listenerNode != null)
				{
					listenerNode.next = next.next;
				}
				else
				{
					rootListener = next.next;
				}
				next.next = null;
				break;
			}
			listenerNode = next;
		}
	}
}
