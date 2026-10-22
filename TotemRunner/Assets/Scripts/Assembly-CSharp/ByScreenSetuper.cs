using UnityEngine;

public class ByScreenSetuper : MonoBehaviour
{
	public enum eHorizontalCorrection
	{
		none = 0,
		left = 1,
		right = 2,
		byPos = 3
	}

	public Camera refCamera;

	public bool widthScaleCorrection;

	public eHorizontalCorrection horizontalCorrection;

	private void Awake()
	{
		if (refCamera == null)
		{
			refCamera = Camera.main;
		}
		if (!(refCamera != null))
		{
			return;
		}
		float num = refCamera.aspect * 0.75f;
		if (widthScaleCorrection)
		{
			Vector3 localScale = base.transform.localScale;
			localScale.x *= num;
			base.transform.localScale = localScale;
		}
		if (horizontalCorrection != eHorizontalCorrection.none && !Mathf.Approximately(num, 1f))
		{
			Vector3 position = base.transform.position;
			float num2 = Mathf.Floor(512f * (num - 1f));
			if (horizontalCorrection == eHorizontalCorrection.left || (horizontalCorrection == eHorizontalCorrection.byPos && position.x < 0f))
			{
				position.x -= num2;
			}
			else if (horizontalCorrection == eHorizontalCorrection.right || (horizontalCorrection == eHorizontalCorrection.byPos && position.x > 0f))
			{
				position.x += num2;
			}
			base.transform.position = position;
		}
	}
}
