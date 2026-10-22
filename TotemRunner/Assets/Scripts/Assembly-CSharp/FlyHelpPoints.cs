using UnityEngine;

public class FlyHelpPoints : MonoBehaviour
{
	public Vector2[] points;

	private static FlyHelpPoints _singleton;

	public static FlyHelpPoints instance => _singleton;

	private void Awake()
	{
		base.enabled = false;
		_singleton = this;
	}

	public int GetNextPointIndex(float x)
	{
		if (points == null || points.Length == 0)
		{
			return -1;
		}
		int i = 0;
		int num = points.Length - 1;
		while (num > i + 1)
		{
			int num2 = (i + num) / 2;
			if (points[num2].x >= x)
			{
				num = num2;
			}
			else
			{
				i = num2;
			}
		}
		for (; i <= num; i++)
		{
			if (points[i].x >= x)
			{
				return i;
			}
		}
		return -1;
	}

	public Vector2 GetNextPoint(float x)
	{
		int nextPointIndex = GetNextPointIndex(x);
		if (nextPointIndex < 0)
		{
			return -1000f * Vector2.right;
		}
		return points[nextPointIndex];
	}

	public int NextPointIndex(float x)
	{
		if (!_singleton)
		{
			return -1;
		}
		return _singleton.GetNextPointIndex(x);
	}

	public static Vector2 GetPointByIndex(int idx)
	{
		if (_singleton != null && idx >= 0 && idx < _singleton.points.Length)
		{
			return _singleton.points[idx];
		}
		return -1000f * Vector2.right;
	}
}
