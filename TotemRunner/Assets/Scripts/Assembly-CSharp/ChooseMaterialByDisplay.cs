using UnityEngine;

public class ChooseMaterialByDisplay : MonoBehaviour
{
	public Camera refCamera;

	public TextAsset[] textures = new TextAsset[4];

	private void Awake()
	{
		Renderer component = GetComponent<Renderer>();
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.RGB24, mipChain: false);
		switch (PluginUtils.GetDisplayType())
		{
		case 1:
			texture2D.LoadImage(textures[0].bytes);
			refCamera.orthographicSize = 320f;
			break;
		case 2:
			texture2D.LoadImage(textures[1].bytes);
			refCamera.orthographicSize = 320f;
			break;
		case 3:
			texture2D.LoadImage(textures[2].bytes);
			refCamera.orthographicSize = 384f;
			break;
		case 4:
			texture2D.LoadImage(textures[3].bytes);
			refCamera.orthographicSize = 384f;
			break;
		case 5:
			texture2D.LoadImage(textures[0].bytes);
			refCamera.orthographicSize = 320f;
			break;
		case 6:
			texture2D.LoadImage(textures[1].bytes);
			refCamera.orthographicSize = 320f;
			break;
		}
		texture2D.filterMode = FilterMode.Bilinear;
		component.material.mainTexture = texture2D;
	}
}
