using System;
using UnityEngine;

public class TouchReactButton : MonoBehaviour, TouchEventListener
{
	public enum eTouchEventType
	{
		none = 0,
		slideMenuToRight = 1,
		slideMenuToLeft = 2,
		launchLevel = 3,
		touchButton = 4
	}

	[Serializable]
	public class TouchEventDescribe
	{
		public eTouchEventType eventType;

		public string eventValueStr;
	}

	public enum eReactButtonState
	{
		enable = 0,
		active = 1,
		disable = 2,
		hidden = 3
	}

	public int zOrder;

	public Material activeMaterial;

	public Material inactiveMaterial;

	public TouchEventDescribe clickEvent = new TouchEventDescribe();

	public TouchEventDescribe slideEvent = new TouchEventDescribe();

	public eReactButtonState buttonState;

	public bool skipParenting;

	private TouchControllerForGUI touchController;

	private Rect buttonRect;

	private Vector2 startTouchPos;

	public Vector2 zoneSize = Vector2.zero;

	private void Start()
	{
		touchController = TouchControllerForGUI.instance;
		touchController.AppendListener(this);
		Camera main = Camera.main;
		Vector3 position = main.transform.position;
		GUtils.SetTransformX(main.transform, 0f);
		MeshFilter component = GetComponent<MeshFilter>();
		Vector3 position2;
		Vector3 position3;
		if (component != null)
		{
			position2 = component.sharedMesh.bounds.min;
			position3 = component.sharedMesh.bounds.max;
		}
		else
		{
			position2 = -Vector3.one;
			position3 = Vector3.one;
		}
		if (zoneSize.x > 0f || zoneSize.y > 0f)
		{
			position2.x *= zoneSize.x;
			position2.y *= zoneSize.y;
			position3.x *= zoneSize.x;
			position3.y *= zoneSize.y;
		}
		position2 = base.transform.TransformPoint(position2);
		position3 = base.transform.TransformPoint(position3);
		if (!skipParenting)
		{
			position2 -= base.transform.position;
			position3 -= base.transform.position;
		}
		if (position2.x > position3.x)
		{
			float x = position2.x;
			position2.x = position3.x;
			position3.x = x;
		}
		if (position2.y < position3.y)
		{
			float x = position2.y;
			position2.y = position3.y;
			position3.y = x;
		}
		buttonRect = new Rect(position2.x, position3.y, position3.x - position2.x, position2.y - position3.y);
		main.transform.position = position;
	}

	private eReactButtonState GetButtonState()
	{
		return buttonState;
	}

	public void SetButtonState(eReactButtonState state)
	{
		buttonState = state;
		if (buttonState == eReactButtonState.hidden)
		{
			if (base.GetComponent<Renderer>() != null && base.GetComponent<Renderer>().enabled)
			{
				base.GetComponent<Renderer>().enabled = false;
			}
		}
		else if (base.GetComponent<Renderer>() != null && !base.GetComponent<Renderer>().enabled)
		{
			base.GetComponent<Renderer>().enabled = true;
		}
		if (buttonState == eReactButtonState.active)
		{
			if (activeMaterial != null)
			{
				base.GetComponent<Renderer>().material = activeMaterial;
			}
		}
		else if (inactiveMaterial != null)
		{
			base.GetComponent<Renderer>().material = inactiveMaterial;
		}
	}

	private void ExecuteClickEvent()
	{
		switch (clickEvent.eventType)
		{
		case eTouchEventType.slideMenuToRight:
			if (ChooseLevelGUI.instance.GetGUIState() == ChooseLevelGUI.eStateGUI.common)
			{
				ChooseLevelGUI.instance.LaunchBlockScrolling(0.1f, 1f, sideIsRight: true);
			}
			break;
		case eTouchEventType.slideMenuToLeft:
			if (ChooseLevelGUI.instance.GetGUIState() == ChooseLevelGUI.eStateGUI.common)
			{
				ChooseLevelGUI.instance.LaunchBlockScrolling(0.1f, 1f, sideIsRight: false);
			}
			break;
		case eTouchEventType.launchLevel:
			if (ChooseLevelGUI.instance.GetGUIState() == ChooseLevelGUI.eStateGUI.common)
			{
				ChooseLevelGUI.instance.LaunchLoading(clickEvent.eventValueStr, 2);
			}
			break;
		case eTouchEventType.touchButton:
			ChooseLevelGUI.instance.TouchButton(clickEvent.eventValueStr);
			break;
		}
	}

	public bool TouchStart(Vector2 pos)
	{
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		if (buttonState == eReactButtonState.disable || buttonState == eReactButtonState.hidden)
		{
			return false;
		}
		if (skipParenting)
		{
			pos.x -= Camera.main.transform.position.x;
		}
		Vector2 vector = pos;
		if (!skipParenting)
		{
			vector -= (Vector2)base.transform.position;
		}
		if (vector.x >= buttonRect.xMin && vector.x <= buttonRect.xMax && vector.y >= buttonRect.yMin && vector.y <= buttonRect.yMax)
		{
			startTouchPos = pos;
			SetButtonState(eReactButtonState.active);
			return true;
		}
		return false;
	}

	public void TouchFinish(Vector2 posStart, Vector2 posEnd, float deltaTime)
	{
		if (buttonState == eReactButtonState.active)
		{
			SetButtonState(eReactButtonState.enable);
		}
	}

	public void TouchMoving(Vector2 posDelta, float deltaTime)
	{
		if (buttonState == eReactButtonState.active)
		{
			Vector2 vector = startTouchPos + posDelta;
			Vector2 vector2 = vector;
			if (!skipParenting)
			{
				vector2 -= (Vector2)base.transform.position;
			}
			if (vector2.x < buttonRect.xMin || vector2.x > buttonRect.xMax || vector2.y < buttonRect.yMin || vector2.y > buttonRect.yMax)
			{
				SetButtonState(eReactButtonState.enable);
			}
		}
	}

	public int GetZOrder()
	{
		return zOrder;
	}

	public bool TouchSlide(int slideNum, Vector2 deltaPos, float deltaTime)
	{
		if (buttonState != eReactButtonState.active)
		{
			return false;
		}
		if (slideNum == 0 && Mathf.Abs(deltaPos.x) < 100f && Mathf.Abs(deltaPos.y) < 200f && clickEvent.eventType != eTouchEventType.none)
		{
			ExecuteClickEvent();
			return true;
		}
		return false;
	}

	public bool TouchClick(int clickNum)
	{
		if (buttonState != eReactButtonState.active)
		{
			return false;
		}
		if (clickEvent.eventType != eTouchEventType.none)
		{
			ExecuteClickEvent();
			return true;
		}
		return false;
	}
}
