using System;
using System.Collections;
using UnityEngine;

public class GrassGifManager : MonoBehaviour
{
	[Serializable]
	public class GrassGif
	{
		[Serializable]
		public class GrassFrame
		{
			public Vector2 uv;
		}

		public int materialNum;

		public Vector2 uvWidthHeight;

		public GrassFrame[] growFrame;

		public GrassFrame[] idleFrame;
	}

	[Serializable]
	public class HerbalSet
	{
		public string id = "usual";

		public float grassPoint = 1f;

		public int[] grassIndices = new int[3] { 0, 1, 2 };

		public Material butterflyMaterial;

		public int flowersMaterial;

		public bool isTaken;

		public bool isDisable;
	}

	public class GrassEntity
	{
		public float time;

		public int grass;

		public int firstVertex;

		public int numInGrow;

		public int frame;

		public Vector3 pos;

		public float angle;

		public Vector2 size;

		public float cutBL;

		public float cutBR;

		public float cutRight;

		public float grassPoint = 1f;
	}

	private class GrassGrowing
	{
		public bool isEmpty;

		public float timeMax;

		public float timeLeft;

		public float offsetY;

		public float frameTime;
	}

	private class SpriteCorners
	{
		public Vector2 lt;

		public Vector2 lb;

		public Vector2 rt;

		public Vector2 rb;
	}

	public FlowersManager flowers;

	public ButterflyManager butterfly;

	public Material[] materials;

	private Mesh mesh;

	private int makeIndex;

	private float grassCounter;

	private int grassCommonCounter;

	private float grassGrowingDelayLeft;

	public GrassGif[] grassList;

	public HerbalSet[] herbals = new HerbalSet[1]
	{
		new HerbalSet()
	};

	[NonSerialized]
	public int currentHerbal;

	public static int superGrassIndex = 6;

	public static int extraGrassIndex = 7;

	public Vector2 spriteSize = new Vector2(200f, 200f);

	public int grassFrameRate = 5;

	private float frameTiming;

	public float zOffsetStep = 1f;

	public float zOffsetMax = 5f;

	private float zCurOffset;

	public Transform target;

	public float grassStep = 80f;

	public float randomOffset;

	public Vector3 grassOffset = new Vector3(40f, -30f, 2f);

	public float grassDepth = 0.1f;

	public float grassGrowTimeMin = 0.8f;

	public float grassGrowTimeMax = 0.8f;

	public AnimationCurve growAngleAnimated = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, -1.4f));

	public AnimationCurve growSizeAnimated = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.3f));

	public AnimationCurve growOffsetYAnimated = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, -100f));

	public float minGrassY = -80f;

	public float newGrassTimeOut = 10f;

	public Color dropParticlesColorCommon = new Color(0f, 1f, 0f, 0.4f);

	public Color dropParticlesColorExtra = new Color(1f, 0f, 1f, 0.4f);

	public Sprite3DWithDrawLevel manHandGlowSprite;

	public Material manHandGlowMaterialCommon;

	public Material manHandGlowMaterialExtra;

	private bool growingIsActive;

	private float lastGrassPos = -1000f;

	private float currentGrassPos;

	private PathTimeline earthPath;

	private ArrayList grasses = new ArrayList();

	public AudioClip extraGrassSound;

	public float extraGrassSoundVolume = 1f;

	private ArrayList growList = new ArrayList();

	private bool[] materialUsed;

	private Vector3[] updateVertices;

	private Vector2[] updateUVs;

	private int[] updateTriangles;

	private int updateSubmesh;

	private Bounds levelBounds;

	private int fixedHerbariumIndex = -1;

	private float fixedHerbariumTimeLeft;

	private static GrassGifManager _singleton;

	private static int badGrassCount;

	public static int g_nGrassLayer;

	public static GrassGifManager instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = UnityEngine.Object.FindObjectOfType(typeof(GrassGifManager)) as GrassGifManager;
			}
			return _singleton;
		}
	}

	public static void TurnOnBadGrass()
	{
		badGrassCount++;
		SetCurrentHerbal("bad");
	}

	public static void TurnOffBadGrass()
	{
		badGrassCount--;
		if (badGrassCount <= 0)
		{
			SetCurrentHerbal(0);
			ChooseRandomHerbal();
		}
	}

	public static void AddHerbal(string herbariumName)
	{
		GrassGifManager grassGifManager = instance;
		for (int i = 0; i < grassGifManager.herbals.Length; i++)
		{
			if (grassGifManager.herbals[i].id == herbariumName)
			{
				grassGifManager.herbals[i].isTaken = true;
				grassGifManager.fixedHerbariumIndex = i;
				grassGifManager.fixedHerbariumTimeLeft = grassGifManager.newGrassTimeOut;
				if (grassGifManager.currentHerbal != 1 && grassGifManager.grassCounter < 300f)
				{
					SetCurrentHerbal(i);
				}
				AchievementCollector.AddGrassSeed(i);
				break;
			}
		}
	}

	public void DeleteHerbal(int herbalNum)
	{
		GrassGifManager grassGifManager = instance;
		grassGifManager.herbals[herbalNum].isTaken = false;
		if (grassGifManager.fixedHerbariumIndex == herbalNum)
		{
			grassGifManager.fixedHerbariumIndex = -1;
		}
	}

	public static void ChooseRandomHerbal()
	{
		GrassGifManager grassGifManager = instance;
		if (grassGifManager.currentHerbal == 1)
		{
			return;
		}
		if (grassGifManager.grassCounter >= 300f)
		{
			if (grassGifManager.currentHerbal != extraGrassIndex)
			{
				SetCurrentHerbal(extraGrassIndex);
			}
			return;
		}
		if (grassGifManager.fixedHerbariumTimeLeft > 0f && grassGifManager.fixedHerbariumIndex >= 0 && !grassGifManager.herbals[grassGifManager.fixedHerbariumIndex].isDisable)
		{
			SetCurrentHerbal(grassGifManager.fixedHerbariumIndex);
			return;
		}
		int num = 0;
		for (int i = 0; i < grassGifManager.herbals.Length; i++)
		{
			if (grassGifManager.herbals[i].isTaken && !grassGifManager.herbals[i].isDisable)
			{
				num++;
			}
		}
		int num2 = UnityEngine.Random.Range(0, num);
		num = 0;
		for (int j = 0; j < grassGifManager.herbals.Length; j++)
		{
			if (grassGifManager.herbals[j].isTaken && !grassGifManager.herbals[j].isDisable)
			{
				if (num == num2)
				{
					SetCurrentHerbal(j);
					return;
				}
				num++;
			}
		}
		SetCurrentHerbal(0);
	}

	public int GetHerbalIndexByName(string id)
	{
		for (int i = 0; i < herbals.Length; i++)
		{
			if (herbals[i].id == id)
			{
				return i;
			}
		}
		return -1;
	}

	private static void SetCurrentHerbal(string id)
	{
		GrassGifManager grassGifManager = instance;
		for (int i = 0; i < grassGifManager.herbals.Length; i++)
		{
			if (grassGifManager.herbals[i].id == id)
			{
				SetCurrentHerbal(i);
				break;
			}
		}
	}

	private static void SetCurrentHerbal(int herbalIndex)
	{
		GrassGifManager grassGifManager = instance;
		if (herbalIndex == extraGrassIndex && grassGifManager.currentHerbal != extraGrassIndex)
		{
			SoundEnvironment.Launch2DSound(grassGifManager.extraGrassSound, grassGifManager.extraGrassSoundVolume, 0f, -1f);
		}
		grassGifManager.currentHerbal = herbalIndex;
		grassGifManager.butterfly.SetMaterial(grassGifManager.herbals[herbalIndex].butterflyMaterial);
		grassGifManager.flowers.SetUsedFlowersMaterial(grassGifManager.herbals[herbalIndex].flowersMaterial);
	}

	private void Awake()
	{
		_singleton = this;
		LevelAttributes levelAttributes = LevelAttributes.instance;
		if (levelAttributes == null)
		{
			levelAttributes = UnityEngine.Object.FindObjectOfType(typeof(LevelAttributes)) as LevelAttributes;
		}
		g_nGrassLayer = base.gameObject.layer;
		badGrassCount = 0;
		earthPath = PathTimeline.instance;
		GameDataStorage.LoadGrassData();
		superGrassIndex = GetHerbalIndexByName("super");
		extraGrassIndex = GetHerbalIndexByName("extra");
		if (grassStep <= 0f)
		{
			grassStep = 0.1f;
		}
		if (grassFrameRate <= 0)
		{
			grassFrameRate = 1;
		}
		frameTiming = 1f / (float)grassFrameRate;
		grasses = new ArrayList();
		growList = new ArrayList();
		if (materials.Length == 0)
		{
			materials = new Material[1]
			{
				new Material(Shader.Find("Unlit/Transparent Cutout"))
			};
		}
		materialUsed = new bool[materials.Length];
		for (int i = 0; i < materials.Length; i++)
		{
			materialUsed[i] = false;
		}
		mesh = new Mesh();
		mesh.vertices = new Vector3[1] { Vector3.zero };
		mesh.uv = new Vector2[1] { Vector2.zero };
		mesh.subMeshCount = materials.Length;
		for (int j = 0; j < materials.Length; j++)
		{
			mesh.SetTriangles(new int[3], j);
		}
		float xMin = levelAttributes.bounds.xMin;
		float xMax = levelAttributes.bounds.xMax;
		levelBounds = new Bounds(new Vector3((xMin + xMax) * 0.5f, -100f, 0f), new Vector3(xMax - xMin, 1000f, 1f));
		mesh.bounds = levelBounds;
		flowers.InitFlowers(levelBounds);
		butterfly.InitButterfly(levelBounds);
	}

	private void Start()
	{
		PlayerController.instance.SetParticleColor(dropParticlesColorCommon);
		if ((bool)manHandGlowSprite)
		{
			manHandGlowSprite.material = manHandGlowMaterialCommon;
		}
	}

	private void Update()
	{
		currentGrassPos = CalculateCurrentPos();
		if (growingIsActive)
		{
			grassGrowingDelayLeft -= Time.deltaTime;
			if (CanGrowing())
			{
				MakeGrass();
				GuiGameHud.instance.SetGrassCounters(Mathf.FloorToInt(grassCounter), grassCommonCounter);
			}
		}
		else
		{
			grassGrowingDelayLeft = 0.2f;
		}
		float x = target.position.x;
		float heightByX = earthPath.GetHeightByX(x);
		flowers.UpdateFlowers(x, growingIsActive);
		butterfly.UpdateButterfly(x, heightByX, growingIsActive);
		if (grasses.Count > 0)
		{
			UpdateGrass();
		}
		if (updateVertices != null)
		{
			mesh.vertices = updateVertices;
		}
		if (updateUVs != null)
		{
			mesh.uv = updateUVs;
		}
		if (updateTriangles != null)
		{
			mesh.SetTriangles(updateTriangles, updateSubmesh);
		}
		if (updateVertices != null)
		{
			mesh.RecalculateBounds();
		}
		updateVertices = null;
		updateUVs = null;
		updateTriangles = null;
		for (int i = 0; i < materials.Length; i++)
		{
			if (materialUsed[i])
			{
				Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, materials[i], g_nGrassLayer, null, i);
			}
		}
		flowers.DrawFlowers();
	}

	public void DrawGrassEnvironment()
	{
		butterfly.DrawButterflys();
	}

	public void ResetToX(float x)
	{
		if (grasses == null)
		{
			return;
		}
		float num = CalculateCurrentPos();
		int num2;
		for (num2 = grasses.Count - 1; num2 >= 0; num2--)
		{
			GrassEntity grassEntity = (GrassEntity)grasses[num2];
			if (grassEntity.pos.x <= num)
			{
				break;
			}
			if (grassEntity.numInGrow >= 0)
			{
				((GrassGrowing)growList[grassEntity.numInGrow]).isEmpty = true;
			}
			grasses.RemoveAt(num2);
		}
		if (num2 < 0)
		{
			updateVertices = null;
			updateUVs = null;
			updateTriangles = null;
			mesh = new Mesh();
			mesh.vertices = new Vector3[1] { Vector3.zero };
			mesh.uv = new Vector2[1] { Vector2.zero };
			mesh.subMeshCount = materials.Length;
			float xMin = LevelAttributes.instance.bounds.xMin;
			float xMax = LevelAttributes.instance.bounds.xMax;
			mesh.bounds = new Bounds(new Vector3((xMin + xMax) * 0.5f, 0f, 0f), new Vector3(xMax - xMin, 0f, 0f));
			lastGrassPos = -1000f;
			grassCounter = 0f;
		}
		else
		{
			int[] array = new int[materials.Length];
			for (int i = 0; i < grasses.Count; i++)
			{
				int materialNum = grassList[((GrassEntity)grasses[i]).grass].materialNum;
				array[materialNum]++;
			}
			for (int i = 0; i < array.Length; i++)
			{
				int[] triangles = mesh.GetTriangles(i);
				if (array[i] < triangles.Length)
				{
					int num3 = array[i] * 6;
					int[] array2 = new int[num3];
					for (int j = 0; j < num3; j++)
					{
						array2[j] = triangles[j];
					}
					mesh.SetTriangles(array2, i);
				}
			}
			updateTriangles = null;
			updateVertices = new Vector3[((GrassEntity)grasses[num2]).firstVertex + 4];
			updateUVs = new Vector2[((GrassEntity)grasses[num2]).firstVertex + 4];
			for (int i = updateVertices.Length - 1; i >= 0; i--)
			{
				ref Vector3 reference = ref updateVertices[i];
				reference = mesh.vertices[i];
				ref Vector2 reference2 = ref updateUVs[i];
				reference2 = mesh.uv[i];
			}
			mesh.vertices = updateVertices;
			mesh.uv = updateUVs;
			lastGrassPos = ((GrassEntity)grasses[num2]).pos.x;
			grassCounter = 0f;
			float grassPointScale = LevelAttributes.instance.grassPointScale;
			for (int i = 0; i < grasses.Count; i++)
			{
				if (grassCounter < 300f)
				{
					grassCounter += ((GrassEntity)grasses[i]).grassPoint * grassPointScale;
				}
				else
				{
					grassCounter += ((GrassEntity)grasses[i]).grassPoint;
				}
			}
		}
		GuiGameHud.instance.SetGrassCounters(Mathf.FloorToInt(grassCounter), grassCommonCounter);
		flowers.ResetToX(x);
		butterfly.ResetToX(x);
		if (grassCounter >= 300f)
		{
			PlayerController.instance.SetParticleColor(dropParticlesColorExtra);
			if ((bool)manHandGlowSprite)
			{
				manHandGlowSprite.material = manHandGlowMaterialExtra;
			}
		}
		else
		{
			PlayerController.instance.SetParticleColor(dropParticlesColorCommon);
			if ((bool)manHandGlowSprite)
			{
				manHandGlowSprite.material = manHandGlowMaterialCommon;
			}
		}
	}

	private bool CanGrowing()
	{
		if (currentGrassPos - lastGrassPos < grassStep)
		{
			return false;
		}
		if ((double)grassGrowingDelayLeft >= 0.0)
		{
			return false;
		}
		float heightByX = earthPath.GetHeightByX(currentGrassPos + spriteSize.x);
		if (heightByX < minGrassY)
		{
			return false;
		}
		if (Mathf.Abs(earthPath.GetHeightByX(currentGrassPos) - heightByX) > spriteSize.y * 0.5f)
		{
			return false;
		}
		if (earthPath.GetLayerHeight(ePathLayerID.disableGrass, currentGrassPos) > -5000f)
		{
			return false;
		}
		return true;
	}

	private float CalculateCurrentPos()
	{
		if (!target)
		{
			return grassOffset.x;
		}
		float num = target.position.x + grassOffset.x;
		return grassStep * Mathf.Floor(num / grassStep);
	}

	private void MakeGrass()
	{
		if (grassList.Length > 0)
		{
			int num = UnityEngine.Random.Range(0, 2);
			if (num == makeIndex)
			{
				num++;
			}
			makeIndex = num;
			int idx = herbals[currentHerbal].grassIndices[makeIndex];
			MakeGrass(idx, currentGrassPos + UnityEngine.Random.Range(0f - randomOffset, randomOffset), herbals[currentHerbal].grassPoint);
			lastGrassPos = currentGrassPos;
		}
	}

	private void MakeGrass(int idx, float x, float grassPoint)
	{
		if (WorldEvil.instance != null)
		{
			WorldEvil.instance.MakeGrowing();
		}
		int vertexCount = mesh.vertexCount;
		updateSubmesh = grassList[idx].materialNum;
		materialUsed[updateSubmesh] = true;
		GrassEntity grassEntity = new GrassEntity();
		grassEntity.grassPoint = grassPoint;
		grassEntity.time = -1f;
		grassEntity.grass = idx;
		grassEntity.frame = 0;
		grassEntity.pos.x = x;
		grassEntity.pos.z = grassOffset.z + zCurOffset;
		zCurOffset += zOffsetStep;
		if (zCurOffset > zOffsetMax)
		{
			zCurOffset = 0f;
		}
		grassEntity.angle = growAngleAnimated.Evaluate(0f);
		grassEntity.size = spriteSize * growSizeAnimated.Evaluate(0f);
		grassEntity.pos.y = earthPath.GetHeightByX(x);
		float num = Mathf.Min(earthPath.GetHeightByX(x + grassEntity.size.x * 0.5f), earthPath.GetHeightByX(x - grassEntity.size.x * 0.5f));
		if (num > grassEntity.pos.y - 150f)
		{
			grassEntity.pos.y = num;
		}
		grassEntity.pos.y += grassOffset.y;
		grassEntity.numInGrow = GetGrowInstance();
		grassEntity.firstVertex = vertexCount;
		GrassGrowing grassGrowing = (GrassGrowing)growList[grassEntity.numInGrow];
		grassGrowing.timeMax = UnityEngine.Random.Range(grassGrowTimeMin, grassGrowTimeMax);
		grassGrowing.timeLeft = grassGrowing.timeMax;
		grassGrowing.offsetY = (growSizeAnimated.Evaluate(0f) - growSizeAnimated.Evaluate(1f)) * spriteSize.y;
		if (grassList[idx].growFrame.Length > 1)
		{
			grassGrowing.frameTime = grassGrowing.timeMax / (float)grassList[idx].growFrame.Length;
		}
		else
		{
			grassGrowing.frameTime = -1f;
		}
		grassEntity.time = grassGrowing.frameTime;
		grassEntity.cutBL = 0f;
		grassEntity.cutBR = 0f;
		grassEntity.cutRight = 0f;
		ArrayList arrayList = new ArrayList(mesh.vertices);
		arrayList.Add(Vector3.zero);
		arrayList.Add(Vector3.zero);
		arrayList.Add(Vector3.zero);
		arrayList.Add(Vector3.zero);
		ArrayList arrayList2 = new ArrayList(mesh.uv);
		arrayList2.Add(Vector2.zero);
		arrayList2.Add(Vector2.zero);
		arrayList2.Add(Vector2.zero);
		arrayList2.Add(Vector2.zero);
		updateVertices = arrayList.ToArray(typeof(Vector3)) as Vector3[];
		updateUVs = arrayList2.ToArray(typeof(Vector2)) as Vector2[];
		if (grassEntity.time < 0f)
		{
			UpdateUVs(grassEntity);
		}
		int[] triangles = mesh.GetTriangles(updateSubmesh);
		int num2 = 0;
		if (triangles != null)
		{
			num2 = triangles.Length;
		}
		int[] array = new int[num2 + 6];
		for (int i = 0; i < num2; i++)
		{
			array[i] = triangles[i];
		}
		array[num2] = 0 + vertexCount;
		array[num2 + 1] = 2 + vertexCount;
		array[num2 + 2] = 1 + vertexCount;
		array[num2 + 3] = 1 + vertexCount;
		array[num2 + 4] = 2 + vertexCount;
		array[num2 + 5] = 3 + vertexCount;
		updateTriangles = array;
		grasses.Add(grassEntity);
		int num3 = Mathf.FloorToInt(grassCounter);
		if (grassCounter < 300f)
		{
			grassCounter += grassPoint * LevelAttributes.instance.grassPointScale;
		}
		else
		{
			grassCounter += grassPoint;
		}
		if (num3 < 300 && grassCounter >= 300f)
		{
			ChooseRandomHerbal();
			PlayerController.instance.SetParticleColor(dropParticlesColorExtra);
			if ((bool)manHandGlowSprite)
			{
				manHandGlowSprite.material = manHandGlowMaterialExtra;
			}
		}
		ScoreManager.UpdateScores();
	}

	private int CalculateStarsQuantity()
	{
		int num = Mathf.FloorToInt(grassCounter);
		return num / 100;
	}

	private int GetGrowInstance()
	{
		int count = growList.Count;
		for (int i = 0; i < count; i++)
		{
			GrassGrowing grassGrowing = growList[i] as GrassGrowing;
			if (grassGrowing.isEmpty)
			{
				grassGrowing.isEmpty = false;
				return i;
			}
		}
		growList.Add(new GrassGrowing());
		((GrassGrowing)growList[count]).isEmpty = false;
		return count;
	}

	private bool UpdateVertices(GrassEntity grass)
	{
		int firstVertex = grass.firstVertex;
		SpriteCorners spriteCorners;
		if (grass.numInGrow < 0)
		{
			spriteCorners = CalculateCorners(grass.pos, grass.size, 0f, grass.angle);
		}
		else
		{
			GrassGrowing grassGrowing = (GrassGrowing)growList[grass.numInGrow];
			float time = grassGrowing.timeLeft / grassGrowing.timeMax;
			float angle = growAngleAnimated.Evaluate(time);
			Vector2 size = spriteSize * growSizeAnimated.Evaluate(time);
			float yOffset = growOffsetYAnimated.Evaluate(time);
			spriteCorners = CalculateCorners(grass.pos, size, yOffset, angle);
		}
		bool result = CutSpriteByPath(grass, spriteCorners);
		if (updateVertices == null)
		{
			updateVertices = mesh.vertices;
		}
		ref Vector3 reference = ref updateVertices[firstVertex++];
		reference = new Vector3(spriteCorners.lt.x, spriteCorners.lt.y, grass.pos.z);
		ref Vector3 reference2 = ref updateVertices[firstVertex++];
		reference2 = new Vector3(spriteCorners.lb.x, spriteCorners.lb.y, grass.pos.z);
		ref Vector3 reference3 = ref updateVertices[firstVertex++];
		reference3 = new Vector3(spriteCorners.rt.x, spriteCorners.rt.y, grass.pos.z);
		ref Vector3 reference4 = ref updateVertices[firstVertex++];
		reference4 = new Vector3(spriteCorners.rb.x, spriteCorners.rb.y, grass.pos.z);
		return result;
	}

	private bool CutSpriteByPath(GrassEntity grass, SpriteCorners corners)
	{
		float cutBL = grass.cutBL;
		float cutBR = grass.cutBR;
		float cutRight = grass.cutRight;
		if (earthPath.GetHeightByX(corners.lt.x) > corners.lt.y)
		{
			grass.cutRight = 1f;
		}
		else
		{
			grass.cutRight = Mathf.Clamp01(earthPath.LineIntersect(corners.rt, corners.lt));
		}
		if (grass.cutRight > 0f)
		{
			corners.rb += (corners.lb - corners.rb) * grass.cutRight;
			corners.rt += (corners.lt - corners.rt) * grass.cutRight;
		}
		grass.cutBL = Mathf.Clamp01(earthPath.LineIntersect(corners.lb, corners.lt) - grassDepth);
		grass.cutBR = Mathf.Clamp01(earthPath.LineIntersect(corners.rb, corners.rt) - grassDepth);
		if (grass.cutBL > 0f)
		{
			corners.lb += (corners.lt - corners.lb) * grass.cutBL;
		}
		if (grass.cutBR > 0f)
		{
			corners.rb += (corners.rt - corners.rb) * grass.cutBR;
		}
		return !Mathf.Approximately(cutBL, grass.cutBL) || !Mathf.Approximately(cutBR, grass.cutBR) || !Mathf.Approximately(cutRight, grass.cutRight);
	}

	private void UpdateUVs(GrassEntity grass)
	{
		GrassGif grassGif = grassList[grass.grass];
		int firstVertex = grass.firstVertex;
		float x;
		float y;
		if (grass.numInGrow < 0)
		{
			x = grassGif.idleFrame[grass.frame].uv.x;
			y = grassGif.idleFrame[grass.frame].uv.y;
		}
		else
		{
			x = grassGif.growFrame[grass.frame].uv.x;
			y = grassGif.growFrame[grass.frame].uv.y;
		}
		float num = x + grassGif.uvWidthHeight.x;
		float y2 = y + grassGif.uvWidthHeight.y;
		float y3 = ((!(grass.cutBL > 0f)) ? y : (y + grassGif.uvWidthHeight.y * grass.cutBL));
		float y4 = ((!(grass.cutBR > 0f)) ? y : (y + grassGif.uvWidthHeight.y * grass.cutBR));
		if (grass.cutRight > 0f)
		{
			num -= grassGif.uvWidthHeight.x * grass.cutRight;
		}
		if (updateUVs == null)
		{
			updateUVs = mesh.uv;
		}
		ref Vector2 reference = ref updateUVs[firstVertex++];
		reference = new Vector2(x, y2);
		ref Vector2 reference2 = ref updateUVs[firstVertex++];
		reference2 = new Vector2(x, y3);
		ref Vector2 reference3 = ref updateUVs[firstVertex++];
		reference3 = new Vector2(num, y2);
		ref Vector2 reference4 = ref updateUVs[firstVertex++];
		reference4 = new Vector2(num, y4);
	}

	private SpriteCorners CalculateCorners(Vector2 pos, Vector2 size, float yOffset, float angle)
	{
		Vector2 vector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		Vector2 vector2 = new Vector2(0f - vector.y, vector.x);
		vector *= size.x;
		vector2 *= size.y;
		Vector2 vector3 = pos;
		vector3.y += yOffset;
		SpriteCorners spriteCorners = new SpriteCorners();
		spriteCorners.lt = vector3 + vector2;
		spriteCorners.lb = vector3;
		spriteCorners.rt = vector3 + vector + vector2;
		spriteCorners.rb = vector3 + vector;
		return spriteCorners;
	}

	private void GrassVisibleInclude(int idx)
	{
	}

	private void GrassVisibleExclude(int idx)
	{
	}

	public void UpdateGrass()
	{
		foreach (GrassEntity grass in grasses)
		{
			bool flag = false;
			if (grass.numInGrow >= 0)
			{
				GrassGrowing grassGrowing = (GrassGrowing)growList[grass.numInGrow];
				grassGrowing.timeLeft -= Time.deltaTime;
				if (grassGrowing.timeLeft > 0f)
				{
					flag = UpdateVertices(grass);
				}
				else
				{
					grassGrowing.isEmpty = true;
					grass.numInGrow = -1;
					grass.frame = 0;
					if (grassList[grass.grass].idleFrame.Length > 1)
					{
						grass.time = frameTiming;
					}
					else
					{
						grass.time = -1f;
					}
					flag = true;
				}
			}
			if (grass.time >= 0f)
			{
				grass.time -= Time.deltaTime;
				if (grass.time < 0f)
				{
					while (grass.time < 0f)
					{
						grass.frame++;
						if (grass.frame >= grassList[grass.grass].idleFrame.Length)
						{
							grass.frame = 0;
						}
						grass.time += frameTiming;
					}
					flag = true;
				}
			}
			if (flag)
			{
				UpdateUVs(grass);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		float xMin = LevelAttributes.instance.bounds.xMin;
		float xMax = LevelAttributes.instance.bounds.xMax;
		Gizmos.DrawLine(new Vector3(xMin, minGrassY, 0f), new Vector3(xMax, minGrassY, 0f));
	}

	public void StartGrowing()
	{
		growingIsActive = true;
		ChooseRandomHerbal();
	}

	public void StopGrowing()
	{
		growingIsActive = false;
	}

	public bool IsGrowing()
	{
		return growingIsActive;
	}

	public int GetGrassCount()
	{
		return Mathf.FloorToInt(grassCounter);
	}
}
