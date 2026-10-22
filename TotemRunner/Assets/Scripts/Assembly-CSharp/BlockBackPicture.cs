using UnityEngine;

public class BlockBackPicture : MonoBehaviour
{
	private void Awake()
	{
		if (Camera.main != null)
		{
			Camera main = Camera.main;
			Vector3 localScale = base.transform.localScale;
			Vector3 direction = new Vector3(main.pixelWidth / main.pixelHeight * 3f / 4f, 1f, 1f);
			Vector3 localScale2 = base.transform.InverseTransformDirection(direction);
			localScale2.x *= localScale.x;
			localScale2.y *= localScale.y;
			localScale2.z *= localScale.z;
			base.transform.localScale = localScale2;
		}
	}
}
