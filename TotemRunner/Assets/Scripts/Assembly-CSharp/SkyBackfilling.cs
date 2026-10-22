using UnityEngine;

public class SkyBackfilling : SpriteObject
{
	public override void AwakeAfterCamera(Transform camTransform)
	{
		Camera camera = BaseCamera.instance.GetComponent<Camera>();
		Vector3 one = Vector3.one;
		one.z = base.transform.position.z - (camera.transform.position.z + camera.nearClipPlane);
		Vector3 vector = camera.ViewportToWorldPoint(one);
		float num = vector.x * 2.5f;
		float num2 = vector.y * 2.5f;
		rect = new Rect((0f - num) * 0.5f, (0f - num2) * 0.5f, num, num2);
		base.AwakeAfterCamera(camTransform);
	}
}
