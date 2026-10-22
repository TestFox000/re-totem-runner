using UnityEngine;

public class Rain : AfterCameraUpdated
{
	public class DropEntity
	{
		public DropEntity next;

		public int frame;

		public Vector2 pos;

		public float xOffset;
	}

	public Material dropMaterial;

	public Vector2 dropHalfSize = new Vector2(3f, 48f);

	public float zPlane = 1000f;

	public float dropBornY = 400f;

	public float dropBornYRand = 100f;

	public float dropKillY = -350f;

	public Vector2 dropSpeed = new Vector2(-1000f, -1600f);

	public float leftBornBound = -150f;

	public float rightBornBound = 700f;

	public float bornTimeDelay = 0.1f;

	public int bornQuantity = 5;

	public float camVelocityRelation = 0.03f;

	public float xOffsetMin = 40f;

	public float xOffsetMax = 50f;

	public Vector2[] dropSpeedArray = new Vector2[8]
	{
		new Vector2(-700f, -300f),
		new Vector2(-500f, -200f),
		new Vector2(-300f, -150f),
		new Vector2(-150f, -250f),
		new Vector2(-100f, -300f),
		new Vector2(-50f, -300f),
		new Vector2(0f, -300f),
		new Vector2(50f, -300f)
	};

	private float bornTimeLeft;

	private Mesh dropMesh;

	private DropEntity rootDrop;

	private int dropQuantity;

	private Transform camTransform;

	public override void AwakeAfterCamera(Transform relationCamera)
	{
		dropMesh = new Mesh();
		camTransform = relationCamera;
		dropQuantity = 0;
		bornTimeLeft = 0f;
		rootDrop = null;
	}

	private void UpdateDropMesh()
	{
		dropMesh.Clear();
		if (dropQuantity > 0)
		{
			Vector3[] array = new Vector3[dropQuantity * 4];
			Vector2[] array2 = new Vector2[dropQuantity * 4];
			int[] array3 = new int[dropQuantity * 6];
			float num = Mathf.Clamp(camTransform.GetComponent<Camera>().velocity.x, -200f, 1000f) * camVelocityRelation;
			int num2 = 0;
			int num3 = 0;
			for (DropEntity next = rootDrop; next != null; next = next.next)
			{
				float num4 = next.pos.x - dropHalfSize.x;
				float num5 = next.pos.x + dropHalfSize.x;
				float y = next.pos.y + dropHalfSize.y;
				ref Vector3 reference = ref array[num2];
				reference = new Vector3(num4, y, 0f);
				ref Vector3 reference2 = ref array[num2 + 1];
				reference2 = new Vector3(num5, y, 0f);
				y = next.pos.y - dropHalfSize.y;
				num4 -= num + next.xOffset;
				num5 -= num + next.xOffset;
				ref Vector3 reference3 = ref array[num2 + 2];
				reference3 = new Vector3(num4, y, 0f);
				ref Vector3 reference4 = ref array[num2 + 3];
				reference4 = new Vector3(num5, y, 0f);
				float num6 = ((next.frame != 0) ? 0.5f : 0f);
				float x = num6 + 0.5f;
				ref Vector2 reference5 = ref array2[num2];
				reference5 = new Vector2(num6, 1f);
				ref Vector2 reference6 = ref array2[num2 + 1];
				reference6 = new Vector2(x, 1f);
				ref Vector2 reference7 = ref array2[num2 + 2];
				reference7 = new Vector2(num6, 0f);
				ref Vector2 reference8 = ref array2[num2 + 3];
				reference8 = new Vector2(x, 0f);
				array3[num3] = num2;
				array3[num3 + 1] = num2 + 1;
				array3[num3 + 2] = num2 + 2;
				array3[num3 + 3] = num2 + 2;
				array3[num3 + 4] = num2 + 1;
				array3[num3 + 5] = num2 + 3;
				num2 += 4;
				num3 += 6;
			}
			dropMesh.vertices = array;
			dropMesh.uv = array2;
			dropMesh.triangles = array3;
		}
		dropMesh.RecalculateBounds();
	}

	public override void UpdateAfterCamera()
	{
		DropEntity dropEntity = null;
		int num = ((dropSpeedArray.Length <= 0) ? (-1) : 0);
		for (DropEntity next = rootDrop; next != null; next = next.next)
		{
			if (num >= 0)
			{
				if (++num >= dropSpeedArray.Length)
				{
					num = 0;
				}
				next.pos += Time.deltaTime * dropSpeedArray[num];
			}
			else
			{
				next.pos += Time.deltaTime * dropSpeed;
			}
			if (next.pos.y <= dropKillY)
			{
				if (dropEntity != null)
				{
					dropEntity.next = next.next;
				}
				else
				{
					rootDrop = next.next;
				}
				dropQuantity--;
			}
			else
			{
				dropEntity = next;
			}
		}
		bornTimeLeft -= Time.deltaTime;
		if (bornTimeLeft <= 0f)
		{
			for (int i = 0; i < bornQuantity; i++)
			{
				float x = Random.Range(leftBornBound, rightBornBound);
				DropEntity dropEntity2 = new DropEntity();
				dropEntity2.frame = Random.Range(0, 2);
				dropEntity2.pos = new Vector2(x, dropBornY + Random.Range(0f, dropBornYRand));
				dropEntity2.xOffset = Random.Range(xOffsetMin, xOffsetMax);
				dropEntity2.next = rootDrop;
				rootDrop = dropEntity2;
				dropQuantity++;
			}
			bornTimeLeft = bornTimeDelay;
		}
		UpdateDropMesh();
	}

	public override void RenderByOrder()
	{
		if (dropQuantity > 0)
		{
			dropMaterial.SetPass(0);
			Graphics.DrawMeshNow(dropMesh, new Vector3(camTransform.position.x, camTransform.position.y, camTransform.position.z + zPlane), Quaternion.identity);
		}
	}
}
