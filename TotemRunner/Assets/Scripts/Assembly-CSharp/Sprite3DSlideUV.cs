using UnityEngine;

public class Sprite3DSlideUV : Sprite3D
{
	public float uSpeed;

	public float vSpeed;

	public override void Update()
	{
		Vector2[] uv = mesh.uv;
		float num = uSpeed * Time.deltaTime;
		if (num != 0f)
		{
			float num2 = tileSize.x + 1f - num;
			if (uv[0].x > num2 || uv[1].x > num2 || uv[2].x > num2 || uv[3].x > num2)
			{
				num--;
			}
			else
			{
				num2 = 0f - tileSize.x - 1f - num;
				if (uv[0].x < num2 || uv[1].x < num2 || uv[2].x < num2 || uv[3].x < num2)
				{
					num++;
				}
			}
			uv[0].x += num;
			uv[1].x += num;
			uv[2].x += num;
			uv[3].x += num;
		}
		num = vSpeed * Time.deltaTime;
		if (num != 0f)
		{
			float num2 = tileSize.y + 1f - num;
			if (uv[0].y > num2 || uv[1].y > num2 || uv[2].y > num2 || uv[3].y > num2)
			{
				num--;
			}
			else
			{
				num2 = 0f - tileSize.y - 1f - num;
				if (uv[0].y < num2 || uv[1].y < num2 || uv[2].y < num2 || uv[3].y < num2)
				{
					num++;
				}
			}
			uv[0].y += num;
			uv[1].y += num;
			uv[2].y += num;
			uv[3].y += num;
		}
		mesh.uv = uv;
		base.Update();
	}
}
