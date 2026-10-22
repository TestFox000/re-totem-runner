using UnityEngine;

public class CloudObject : SpriteObject
{
	public float leftPosX;

	public float rightPosX = 1024f;

	public float randomRightAdd = 256f;

	public float startX = 0.5f;

	public Transform relationTransform;

	public float relationFactor = 1f;

	private int cicleNumber;

	private float currentTrackLength;

	public override void AwakeAfterCamera(Transform camTransform)
	{
		base.AwakeAfterCamera(camTransform);
		currentTrackLength = rightPosX - leftPosX + Random.Range(0f, randomRightAdd);
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		float num = startX + relationTransform.position.x * relationFactor;
		int num2 = Mathf.FloorToInt(num);
		if (num2 != cicleNumber)
		{
			cicleNumber = num2;
			currentTrackLength = rightPosX - leftPosX + Random.Range(0f, randomRightAdd);
		}
		num -= (float)num2;
		GUtils.SetTransformX(base.transform, relationTransform.position.x + leftPosX + num * currentTrackLength);
	}
}
