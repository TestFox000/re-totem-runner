using UnityEngine;

public class Utilites : MonoBehaviour
{
	public static Transform FindBoneInTransform(Transform baseTransform, string boneName)
	{
		foreach (Transform item in baseTransform)
		{
			if (boneName == item.name)
			{
				return item;
			}
			Transform transform2 = FindBoneInTransform(item, boneName);
			if ((bool)transform2)
			{
				return transform2;
			}
		}
		return null;
	}

	public static void UnlockBonusContent(string contentId)
	{
		GuiGameHud.instance.UnlockContent();
	}

	public static Vector2 GetRectCenter(Rect srcRect)
	{
		Vector2 result = default;
		result.x = (srcRect.xMin + srcRect.xMax) * 0.5f;
		result.y = (srcRect.yMin + srcRect.yMax) * 0.5f;
		return result;
	}

	public static void AnimSyncCrossFade(Animation ani, string prevAni, string nextAni, float[] syncPoints)
	{
		AnimationState animationState = ani[prevAni];
		AnimationState animationState2 = ani[nextAni];
		float num = 0.3f;
		float num2 = 0f;
		float num3 = animationState.time;
		if (num3 > animationState.length)
		{
			num3 -= (float)Mathf.FloorToInt(num3 / animationState.length) * animationState.length;
		}
		for (int i = 0; i < syncPoints.Length; i++)
		{
			if (syncPoints[i] >= num3)
			{
				num = syncPoints[i] - num3;
				num2 = 0f - num;
				break;
			}
		}
		if (num < 0.1f)
		{
			num = 0.1f;
		}
		for (; num2 < 0f; num2 += animationState2.length)
		{
		}
		ani.CrossFade(nextAni, num);
		animationState2.time = num2;
	}
}
