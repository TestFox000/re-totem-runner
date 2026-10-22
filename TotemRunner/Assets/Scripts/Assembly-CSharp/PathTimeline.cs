using UnityEngine;

public class PathTimeline : MonoBehaviour
{
	private class PathBound
	{
		public float xMin;

		public float xMax;

		private float y;

		public PathBound(float left, float right, float y)
		{
			xMin = Mathf.Min(left, right);
			xMax = Mathf.Max(left, right);
			this.y = y;
		}

		public bool ClipByPlane(Plane plane)
		{
			float distanceToPoint = plane.GetDistanceToPoint(new Vector3(xMin, y, 0f));
			float distanceToPoint2 = plane.GetDistanceToPoint(new Vector3(xMax, y, 0f));
			float num = distanceToPoint2 - distanceToPoint;
			if (distanceToPoint <= 0f && distanceToPoint2 <= 0f)
			{
				return false;
			}
			float num2 = xMin;
			if (distanceToPoint < 0f)
			{
				xMin -= (xMax - xMin) * distanceToPoint / num;
			}
			if (distanceToPoint2 < 0f)
			{
				xMax += (num2 - xMax) * distanceToPoint2 / num;
			}
			return true;
		}
	}

	private class Transform2D
	{
		public Vector2 offset;

		private float cosA;

		private float sinA;

		private float scale;

		public Transform2D(Vector2 p1, Vector2 p2)
		{
			offset = -p1;
			Vector2 vector = p2 - p1;
			scale = 1f / vector.magnitude;
			cosA = vector.normalized.y;
			sinA = vector.normalized.x;
		}

		public Vector2 MulVector(Vector2 v)
		{
			Vector2 vector = v + offset;
			Vector2 vector2 = new Vector2(vector.x * cosA - vector.y * sinA, vector.x * sinA + vector.y * cosA);
			return scale * vector2;
		}
	}

	public Color commonColor = Color.green;

	public Color dangerColor = Color.red;

	public PlayerController player;

	public Camera cam;

	public float baseHeight;

	public float updateStep = 1f;

	public float leftBoundOffset = -100f;

	public float rightBoundOffset = 100f;

	public GameObject colliderContainer;

	public PathTimelineNode[] nodes;

	public PathLayerNode[] layer1Nodes;

	public PathLayerNode[] layer2Nodes;

	private int rightNodeIdx;

	private int leftNodeIdx;

	private float oldX;

	private static PathTimeline _singleton;

	private float lastQuerryX = -1000f;

	private float lastQuerryHeight;

	public static PathTimeline instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = Object.FindObjectOfType(typeof(PathTimeline)) as PathTimeline;
				if (_singleton == null)
				{
					Debug.LogError("There needs to be one active PathTimeline script on a GameObject in your scene.");
				}
			}
			return _singleton;
		}
	}

	private void Start()
	{
		Restart();
	}

	private void Restart()
	{
		leftNodeIdx = -1;
		rightNodeIdx = -1;
	}

	public void ResetToX(float x)
	{
		leftNodeIdx = FindNodeFromX(x + leftBoundOffset, 0, nodes.Length - 1);
		if (leftNodeIdx > 0)
		{
			leftNodeIdx--;
		}
		rightNodeIdx = FindNodeFromX(x + rightBoundOffset, 0, nodes.Length - 1);
	}

	private void Update()
	{
		UpdateActiveZone(cam.transform.position.x);
	}

	private void Awake()
	{
		_singleton = this;
	}

	private void UpdateActiveZone(float curX)
	{
		if (curX - oldX < updateStep && curX - oldX > 0f - updateStep)
		{
			return;
		}
		float num = curX + leftBoundOffset;
		float num2 = curX + rightBoundOffset;
		if (curX - oldX > 0f)
		{
			for (int i = rightNodeIdx + 1; i < nodes.Length && !(nodes[i].left >= num2); i++)
			{
				rightNodeIdx = i;
			}
			for (int i = leftNodeIdx; i < rightNodeIdx; i++)
			{
				if (i >= 0)
				{
					if (nodes[i].left + nodes[i].width >= num)
					{
						break;
					}
					leftNodeIdx = i;
				}
			}
		}
		oldX = curX;
	}

	public float GetCurX()
	{
		return oldX;
	}

	public float GetHeightByXRange(float x, int nFirst, int nLast)
	{
		float num = baseHeight;
		if (nLast > nFirst && nodes.Length > 0)
		{
			int num2 = nFirst;
			int num3 = nLast;
			while (num2 < num3 - 1)
			{
				int num4 = (num2 + num3) / 2;
				if (x < nodes[num4].left)
				{
					num3 = num4;
				}
				else
				{
					num2 = num4;
				}
			}
			int num5 = -1;
			if (x >= nodes[num3].left)
			{
				num5 = num3;
			}
			else if (num2 >= 0)
			{
				num5 = num2;
			}
			else if (num3 > 0)
			{
				num5 = num3 - 1;
			}
			if (num5 >= 0)
			{
				PathTimelineNode pathTimelineNode = nodes[num5];
				if (x <= pathTimelineNode.left + pathTimelineNode.width)
				{
					num = baseHeight + pathTimelineNode.GetHeight(x - pathTimelineNode.left);
				}
				else if (pathTimelineNode.keys.Length > 0)
				{
					num = baseHeight + pathTimelineNode.keys[pathTimelineNode.keys.Length - 1].y;
				}
				if (num5 > 0)
				{
					pathTimelineNode = nodes[num5 - 1];
					if (x <= pathTimelineNode.left + pathTimelineNode.width)
					{
						float num6 = baseHeight + pathTimelineNode.GetHeight(x - pathTimelineNode.left);
						if (num6 > num)
						{
							num = num6;
						}
					}
				}
			}
		}
		return num;
	}

	public float GetHeightByX(float x)
	{
		if (x == lastQuerryX)
		{
			return lastQuerryHeight;
		}
		lastQuerryX = x;
		lastQuerryHeight = GetHeightByXRange(x, leftNodeIdx, rightNodeIdx);
		return lastQuerryHeight;
	}

	public float GetHeightByXWithMinHeight(float x, float minY)
	{
		float heightByX = GetHeightByX(x);
		if (heightByX > minY)
		{
			return heightByX;
		}
		int i = FindNodeFromX(x, 0, nodes.Length - 1);
		if (i < 0)
		{
			i = 0;
		}
		for (; i < nodes.Length - 1 && nodes[i].left < x; i++)
		{
		}
		if (nodes[i].left >= x && nodes[i].keys != null)
		{
			for (int j = 0; j < nodes[i].keys.Length; j++)
			{
				if (nodes[i].keys[j].y > minY)
				{
					return nodes[i].keys[j].y;
				}
			}
		}
		return 100f;
	}

	private int FindNodeFromX(float x, int firstNode, int lastNode)
	{
		int num = firstNode;
		int num2 = lastNode;
		if (num2 < num)
		{
			return -1;
		}
		while (num < num2 - 1)
		{
			int num3 = (num + num2) / 2;
			if (nodes[num3].left > x)
			{
				num2 = num3;
			}
			else
			{
				num = num3;
			}
		}
		while (num2 >= num)
		{
			if (nodes[num2].left <= x)
			{
				return num2;
			}
			num2--;
		}
		return -1;
	}

	public float LineIntersect(Vector2 p1, Vector2 p2)
	{
		float x = Mathf.Min(p1.x, p2.x);
		float x2 = Mathf.Max(p1.x, p2.x);
		int num = FindNodeFromX(x, 0, nodes.Length - 1);
		int num2 = FindNodeFromX(x2, 0, nodes.Length - 1);
		if (num2 < 0)
		{
			return 0f;
		}
		if (num < 0)
		{
			num = 0;
		}
		Transform2D transform2D = new Transform2D(p1, p2);
		transform2D.offset.y += baseHeight;
		float x3 = transform2D.offset.x;
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		int i;
		for (i = num; i <= num2; i++)
		{
			transform2D.offset.x = x3 + nodes[i].left;
			int j;
			for (j = 0; j < nodes[i].keys.Length - 1; j++)
			{
				vector = transform2D.MulVector(nodes[i].keys[j]);
				vector2 = transform2D.MulVector(nodes[i].keys[j + 1]);
				if (vector.x * vector2.x < 0f)
				{
					break;
				}
			}
			if (j < nodes[i].keys.Length - 1)
			{
				break;
			}
		}
		if (i > num2)
		{
			return 0f;
		}
		float num3 = vector2.x - vector.x;
		if (num3 < 0.001f && num3 > -0.001f)
		{
			return vector.y;
		}
		return vector.y - vector.x / num3 * (vector2.y - vector.y);
	}

	public float GetLayerHeight(ePathLayerID layerNum, float x)
	{
		PathLayerNode[] array = null;
		switch (layerNum)
		{
		case ePathLayerID.maxFlyHight:
			array = layer1Nodes;
			break;
		case ePathLayerID.disableGrass:
			array = layer2Nodes;
			break;
		}
		if (array == null || array.Length == 0)
		{
			return -10000f;
		}
		int num = 0;
		int num2 = array.Length - 1;
		int num3;
		while (num < num2 - 1)
		{
			num3 = (num + num2) / 2;
			if (array[num3].begPos.x < x)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		for (num3 = num; num3 <= num2 && (!(x >= array[num3].begPos.x) || !(x <= array[num3].endPos.x)); num3++)
		{
		}
		if (num3 > num2)
		{
			return -10000f;
		}
		float num4 = x - array[num3].begPos.x;
		float num5 = array[num3].endPos.x - array[num3].begPos.x;
		if (num5 < 0.1f)
		{
			return array[num3].begPos.y;
		}
		num5 = (array[num3].endPos.y - array[num3].begPos.y) / num5;
		return array[num3].begPos.y + num5 * num4;
	}
}
