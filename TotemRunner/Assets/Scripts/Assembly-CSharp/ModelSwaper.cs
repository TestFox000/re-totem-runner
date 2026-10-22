using System;
using UnityEngine;

public class ModelSwaper : MonoBehaviour
{
	[Serializable]
	public class ModelDescribe
	{
		public string id;

		public GameObject skinObj;

		public EPlayerState playerState;

		public float offsetY;

		public float colliderRadius;

		public float colliderHeight;

		public Vector3 colliderCenter;

		public string colliderTiedBoneName = string.Empty;

		[NonSerialized]
		public Transform colliderTiedBone;
	}

	public ModelDescribe[] models;

	public int currentIndex;

	public PlayerController playerController;

	public CameraShake cameraShaker;

	private int animationEnumerateIndex = -1;

	private void Awake()
	{
		if (!playerController)
		{
			Debug.LogError("Model Swaper: Error - Not assigned property 'Player Controller'");
			playerController = UnityEngine.Object.FindObjectOfType(typeof(PlayerController)) as PlayerController;
		}
		for (int i = 0; i < models.Length; i++)
		{
			if (!models[i].skinObj)
			{
				continue;
			}
			models[i].skinObj.SetActiveRecursivelyCompat(i == currentIndex);
			if (models[i].colliderTiedBoneName != string.Empty)
			{
				models[i].colliderTiedBone = Utilites.FindBoneInTransform(models[i].skinObj.transform, models[i].colliderTiedBoneName);
				if (!models[i].colliderTiedBone)
				{
					Debug.LogError("Model Swaper: Error! Cant find bone name " + models[i].colliderTiedBoneName);
				}
			}
		}
		ChangeAttributes();
	}

	public Animation AnimationGetCurrent()
	{
		if (currentIndex >= 0 && currentIndex < models.Length && (bool)models[currentIndex].skinObj)
		{
			return models[currentIndex].skinObj.GetComponent<Animation>();
		}
		return null;
	}

	public Animation AnimationGetByModel(string id)
	{
		ModelDescribe[] array = models;
		foreach (ModelDescribe modelDescribe in array)
		{
			if (modelDescribe.id == id)
			{
				if ((bool)modelDescribe.skinObj)
				{
					return modelDescribe.skinObj.GetComponent<Animation>();
				}
				break;
			}
		}
		return null;
	}

	public void AnimationStartEnum()
	{
		animationEnumerateIndex = 0;
	}

	public void AnimationNextEnum()
	{
		if (++animationEnumerateIndex >= models.Length)
		{
			animationEnumerateIndex = -1;
		}
	}

	public Animation AnimationGetEnum()
	{
		if (animationEnumerateIndex < 0)
		{
			return null;
		}
		return models[animationEnumerateIndex].skinObj.GetComponent<Animation>();
	}

	public void DoSwitch(int idx)
	{
		if (currentIndex != idx && idx < models.Length)
		{
			if (currentIndex >= 0)
			{
				models[currentIndex].skinObj.SetActiveRecursivelyCompat(state: false);
				models[currentIndex].skinObj.GetComponent<Animation>().enabled = false;
			}
			models[idx].skinObj.SetActiveRecursivelyCompat(state: true);
			models[idx].skinObj.GetComponent<Animation>().enabled = true;
			currentIndex = idx;
			cameraShaker.LaunchShake(ECameraShakeType.changeSkin, 1f);
			ChangeAttributes();
		}
	}

	public void ChangeAttributes()
	{
		if (LevelAttributes.instance.PlayerSkin != (EPlayerSkin)currentIndex)
		{
			LevelAttributes.instance.PlayerSkin = (EPlayerSkin)currentIndex;
		}
		if (currentIndex == 1)
		{
			LevelAttributes.instance.SetVerticalOffset(base.transform.position.y);
		}
		else
		{
			LevelAttributes.instance.SetVerticalOffset(models[currentIndex].offsetY);
		}
		playerController.SetPlayerState(models[currentIndex].playerState);
		playerController.ChangeCapsule(models[currentIndex].colliderRadius, models[currentIndex].colliderHeight, models[currentIndex].colliderCenter, models[currentIndex].colliderTiedBone);
	}

	public void DoSwitch(string id)
	{
		for (int i = 0; i < models.Length; i++)
		{
			if (models[i].id == id)
			{
				DoSwitch(i);
				break;
			}
		}
	}

	public void CircleSwitch()
	{
		if (models.Length != 0)
		{
			int num = currentIndex + 1;
			if (num >= models.Length)
			{
				num = 0;
			}
			DoSwitch(num);
		}
	}
}
