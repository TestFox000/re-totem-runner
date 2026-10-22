using System;
using UnityEngine;

public class LevelAttributes : MonoBehaviour
{
	[Serializable]
	public class LvlTaskDescribe
	{
		public LevelTaskManager.eTaskId id = LevelTaskManager.eTaskId.none;

		public int counter = -1;

		public LvlTaskDescribe(LevelTaskManager.eTaskId taskId, int taskCounter)
		{
			id = taskId;
			counter = taskCounter;
		}
	}

	public Rect bounds;

	public int crystalQuantity;

	public int enemyQuantity;

	public int seedQuantity;

	public int timeLength = 150;

	public LvlTaskDescribe[] taskId = new LvlTaskDescribe[3]
	{
		new LvlTaskDescribe(LevelTaskManager.eTaskId.gems, -1),
		new LvlTaskDescribe(LevelTaskManager.eTaskId.time, -1),
		new LvlTaskDescribe(LevelTaskManager.eTaskId.seeds, -1)
	};

	public bool[] skinEnabled = new bool[5] { true, true, true, false, true };

	public float grassPointScale = 1f;

	public bool isManDying;

	public int[] scoreForStars = new int[3] { 100, 300, 800 };

	private EPlayerSkin playerSkin;

	private EPlayerState playerState;

	private Color sceneViewDisplayColor = new Color(0.2f, 0.74f, 0.27f, 0.5f);

	private float verticalOffset;

	public static Mesh spriteMesh;

	private static LevelAttributes _singleton;

	public EPlayerSkin PlayerSkin
	{
		get
		{
			return playerSkin;
		}
		set
		{
			playerSkin = value;
		}
	}

	public bool IsFlyForm => playerSkin == EPlayerSkin.Eagle || playerSkin == EPlayerSkin.Star || playerSkin == EPlayerSkin.Ghost;

	public static LevelAttributes instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = UnityEngine.Object.FindObjectOfType(typeof(LevelAttributes)) as LevelAttributes;
			}
			return _singleton;
		}
	}

	public static bool IsInstalled => _singleton != null;

	public void SetPlayerState(EPlayerState newState)
	{
		playerState = newState;
	}

	public EPlayerState GetPlayerState()
	{
		return playerState;
	}

	public void SetVerticalOffset(float newOffset)
	{
		verticalOffset = newOffset;
	}

	public float GetVerticalOffset()
	{
		return verticalOffset;
	}

	private void Awake()
	{
		_singleton = this;
		spriteMesh = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f)
		};
		spriteMesh.vertices = vertices;
		Vector2[] uv = new Vector2[4]
		{
			Vector2.up,
			Vector2.zero,
			Vector2.one,
			Vector2.right
		};
		spriteMesh.uv = uv;
		int[] triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		spriteMesh.triangles = triangles;
		spriteMesh.RecalculateBounds();
	}

	private void OnDisable()
	{
		_singleton = null;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = sceneViewDisplayColor;
		Vector3 vector = new Vector3(bounds.xMin, bounds.yMax, 0f);
		Vector3 vector2 = new Vector3(bounds.xMin, bounds.yMin, 0f);
		Vector3 vector3 = new Vector3(bounds.xMax, bounds.yMax, 0f);
		Vector3 vector4 = new Vector3(bounds.xMax, bounds.yMin, 0f);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector4);
		Gizmos.DrawLine(vector4, vector3);
		Gizmos.DrawLine(vector3, vector);
	}

	public static Bounds GetBounds()
	{
		Rect rect = instance.bounds;
		Vector3 center = new Vector3((rect.xMin + rect.xMax) * 0.5f, (rect.yMin + rect.yMax) * 0.5f, 0f);
		Vector3 size = new Vector3(rect.width, rect.height, 1f);
		return new Bounds(center, size);
	}

	public EPlayerSkin GetDefaultSkin()
	{
		if (PlayerSkin == EPlayerSkin.Star)
		{
			return EPlayerSkin.Star;
		}
		for (int i = 0; i < skinEnabled.Length; i++)
		{
			if (skinEnabled[i])
			{
				return (EPlayerSkin)i;
			}
		}
		return EPlayerSkin.Man;
	}
}
