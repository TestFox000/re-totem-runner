using UnityEngine;

public class SunLight : Sprite3D
{
	public Vector2 tile2Size = Vector2.one;

	public Vector2 uv1Speed = Vector2.zero;

	public Vector2 uv2Speed = Vector2.zero;

	public override void Update()
	{
		mesh.uv = SlideUV(mesh.uv, uv1Speed);
		mesh.uv2 = SlideUV(mesh.uv2, uv2Speed);
		base.Update();
	}

	public Vector2[] SlideUV(Vector2[] uvs, Vector2 speed)
	{
		float num = speed.x * Time.deltaTime;
		if (num != 0f)
		{
			float num2 = tileSize.x + 1f - num;
			if (uvs[0].x > num2 || uvs[1].x > num2 || uvs[2].x > num2 || uvs[3].x > num2)
			{
				num--;
			}
			else
			{
				num2 = 0f - tileSize.x - 1f - num;
				if (uvs[0].x < num2 || uvs[1].x < num2 || uvs[2].x < num2 || uvs[3].x < num2)
				{
					num++;
				}
			}
			uvs[0].x += num;
			uvs[1].x += num;
			uvs[2].x += num;
			uvs[3].x += num;
		}
		num = speed.y * Time.deltaTime;
		if (num != 0f)
		{
			float num2 = tileSize.y + 1f - num;
			if (uvs[0].y > num2 || uvs[1].y > num2 || uvs[2].y > num2 || uvs[3].y > num2)
			{
				num--;
			}
			else
			{
				num2 = 0f - tileSize.y - 1f - num;
				if (uvs[0].y < num2 || uvs[1].y < num2 || uvs[2].y < num2 || uvs[3].y < num2)
				{
					num++;
				}
			}
			uvs[0].y += num;
			uvs[1].y += num;
			uvs[2].y += num;
			uvs[3].y += num;
		}
		return uvs;
	}

	public override void InitSprite3D()
	{
		base.InitSprite3D();
		spriteTransform = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(spriteSize.x * base.transform.localScale.x, spriteSize.y * base.transform.localScale.y, base.transform.localScale.z));
	}

	public override void BuildMesh()
	{
		base.BuildMesh();
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, tile2Size.y),
			Vector2.zero,
			tile2Size,
			new Vector2(tile2Size.x, 0f)
		};
		mesh.uv2 = uv;
	}
}
