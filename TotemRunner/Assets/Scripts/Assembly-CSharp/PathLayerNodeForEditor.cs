using UnityEngine;

public class PathLayerNodeForEditor : MonoBehaviour
{
	public ePathLayerID layerIndex;

	public static float layerXOffset = 100f;

	public Vector2 GetBegPoint()
	{
		Vector3 vector = base.transform.TransformPoint(new Vector3(0f - layerXOffset, 0f, 0f));
		return new Vector2(vector.x, vector.y);
	}

	public Vector2 GetEndPoint()
	{
		Vector3 vector = base.transform.TransformPoint(new Vector3(layerXOffset, 0f, 0f));
		return new Vector2(vector.x, vector.y);
	}
}
