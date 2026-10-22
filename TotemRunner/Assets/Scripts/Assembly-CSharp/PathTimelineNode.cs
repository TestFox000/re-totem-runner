using System;
using UnityEngine;

[Serializable]
public class PathTimelineNode
{
	public EPathNodeType eType;

	public Vector2[] keys;

	public float left;

	public float width;

	public int GetKeyFromX(float x)
	{
		int num = 0;
		int num2 = keys.Length - 1;
		if (num2 < num)
		{
			return -1;
		}
		while (num < num2 - 1)
		{
			int num3 = (num + num2) / 2;
			if (keys[num3].x > x)
			{
				num2 = num3;
			}
			else
			{
				num = num3;
			}
		}
		if (keys[num2].x <= x)
		{
			return num2;
		}
		return num;
	}

	public float GetHeight(float x)
	{
		if (keys.Length == 0)
		{
			return 0f;
		}
		float result = 0f;
		switch (eType)
		{
		case EPathNodeType.wall:
			result = keys[0].y;
			break;
		case EPathNodeType.pit:
			result = 0f - keys[0].y;
			break;
		case EPathNodeType.jaggy:
		{
			int keyFromX = GetKeyFromX(x);
			if (keyFromX < 0)
			{
				result = 0f;
				break;
			}
			if (keyFromX == keys.Length - 1)
			{
				result = keys[keyFromX].y;
				break;
			}
			Vector2 vector = keys[keyFromX];
			Vector2 vector2 = keys[keyFromX + 1];
			float num = vector2.x - vector.x;
			result = ((!(num < 0.1f)) ? (vector.y + (vector2.y - vector.y) * (x - vector.x) / num) : vector2.y);
			break;
		}
		}
		return result;
	}
}
