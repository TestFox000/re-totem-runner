using System;
using UnityEngine;

public class MaterialTextureSwitcher : MonoBehaviour
{
	[Serializable]
	public class MaterialTextureSwitchDescribe
	{
		public enum eMaterialSwitchWorkType
		{
			always = 0,
			inCamera = 1
		}

		public Renderer meshRender;

		public Material material;

		public Texture2D[] textures;

		public eMaterialSwitchWorkType workType;

		public float leftVisibleBound = 800f;

		public float rightVisibleBound = 100f;

		[NonSerialized]
		public int curIndex;

		public void SwitchToNextTexture()
		{
			if (workType == eMaterialSwitchWorkType.inCamera && meshRender != null)
			{
				float x = meshRender.transform.position.x;
				if (x + rightVisibleBound < ScrollingCamera.leftCameraBound || x - leftVisibleBound > ScrollingCamera.rightCameraBound)
				{
					return;
				}
			}
			curIndex++;
			if (curIndex >= textures.Length)
			{
				curIndex = 0;
			}
			material.mainTexture = textures[curIndex];
		}
	}

	public float switchInterval = 0.1f;

	private float switchTime;

	public MaterialTextureSwitchDescribe[] switcherList;

	private void Awake()
	{
		MaterialTextureSwitchDescribe[] array = switcherList;
		foreach (MaterialTextureSwitchDescribe materialTextureSwitchDescribe in array)
		{
			materialTextureSwitchDescribe.curIndex = 0;
			if (materialTextureSwitchDescribe.meshRender != null)
			{
				materialTextureSwitchDescribe.material = materialTextureSwitchDescribe.meshRender.material;
			}
		}
		switchTime = 0f;
	}

	private void Update()
	{
		switchTime += Time.deltaTime;
		bool flag = false;
		if (switchTime >= switchInterval)
		{
			flag = true;
			switchTime -= switchInterval;
			if (switchTime >= switchInterval)
			{
				switchTime = 0f;
			}
		}
		if (flag)
		{
			MaterialTextureSwitchDescribe[] array = switcherList;
			foreach (MaterialTextureSwitchDescribe materialTextureSwitchDescribe in array)
			{
				materialTextureSwitchDescribe.SwitchToNextTexture();
			}
		}
	}
}
