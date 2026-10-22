using UnityEngine;

public class TouchButtonSquare : MonoBehaviour
{
	public Vector2 squarePos = Vector2.zero;

	public Vector2 squareSize = Vector2.zero;

	public float maxTouchOffset = 50f;

	public EventExecutor executor;

	public string executorParam;

	public Camera refCamera;

	private Vector2 startTouchPos = Vector2.zero;

	private bool isTouching;

	private bool isSkiped;

	public Renderer refRender;

	public Texture2D texturePressed;

	public Texture2D textureUnpressed;

	public bool IsTouched => isTouching && !isSkiped;

	private void Update()
	{
		if (!isTouching)
		{
			if (GetTouchState())
			{
				StartTouching();
			}
		}
		else if (!GetTouchState())
		{
			FinishTouching();
		}
		else if (!isSkiped)
		{
			Vector2 touchPos = GetTouchPos();
			if (!IsPointInside(touchPos))
			{
				isSkiped = true;
				ChangePressedStatus();
			}
		}
	}

	private void ChangePressedStatus()
	{
		if (!(refRender != null))
		{
			return;
		}
		if (isTouching && !isSkiped)
		{
			if (refRender.material.mainTexture != texturePressed)
			{
				refRender.material.mainTexture = texturePressed;
			}
		}
		else if (refRender.material.mainTexture != textureUnpressed)
		{
			refRender.material.mainTexture = textureUnpressed;
		}
	}

	private Vector2 GetTouchPos()
	{
		Vector2 result = Vector2.zero;
		if (Input.touchCount > 0)
		{
			result = Input.touches[0].position;
		}
		return result;
	}

	private bool GetTouchState()
	{
		bool result = false;
		if (Input.touchCount > 0 && Input.touches[0].phase != TouchPhase.Ended && Input.touches[0].phase != TouchPhase.Canceled)
		{
			result = true;
		}
		return result;
	}

	public virtual bool IsPointInside(Vector2 pnt)
	{
		if (refCamera == null)
		{
			return false;
		}
		Vector3 vector = refCamera.ScreenToWorldPoint(new Vector3(pnt.x, pnt.y, base.transform.position.z - refCamera.transform.position.z));
		float num = base.transform.position.x + squarePos.x;
		float num2 = base.transform.position.y + squarePos.y;
		float num3 = squareSize.x * 0.5f;
		float num4 = squareSize.y * 0.5f;
		if (vector.x >= num - num3 && vector.x <= num + num3 && vector.y <= num2 + num4 && vector.y >= num2 - num4)
		{
			return true;
		}
		return false;
	}

	private void StartTouching()
	{
		startTouchPos = GetTouchPos();
		isTouching = true;
		isSkiped = !IsPointInside(startTouchPos);
		ChangePressedStatus();
	}

	private void FinishTouching()
	{
		if (executor != null && !isSkiped)
		{
			if (string.IsNullOrEmpty(executorParam))
			{
				executor.StartExecutor();
			}
			else
			{
				executor.StartExecutor(executorParam);
			}
		}
		isTouching = false;
		ChangePressedStatus();
	}
}
