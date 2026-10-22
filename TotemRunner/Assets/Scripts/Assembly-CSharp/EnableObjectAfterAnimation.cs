using UnityEngine;

public class EnableObjectAfterAnimation : MonoBehaviour
{
	public Animation refAnim;

	public float animTime = -1f;

	public GameObject[] enableObjects;

	public GameObject[] disableObjects;

	private bool isWaitAnim;

	private void Awake()
	{
		isWaitAnim = false;
	}

	private void OnEnable()
	{
		if (refAnim != null)
		{
			isWaitAnim = true;
		}
	}

	private void Update()
	{
		if (!isWaitAnim)
		{
			return;
		}
		bool flag = !refAnim.isPlaying;
		if (!flag && animTime >= 0f && refAnim.clip != null && refAnim[refAnim.clip.name].time >= animTime)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		isWaitAnim = false;
		if (enableObjects != null)
		{
			for (int i = 0; i < enableObjects.Length; i++)
			{
				if ((bool)enableObjects[i])
				{
					enableObjects[i].SetActiveRecursivelyCompat(state: true);
				}
			}
		}
		if (disableObjects == null)
		{
			return;
		}
		for (int i = 0; i < disableObjects.Length; i++)
		{
			if ((bool)disableObjects[i])
			{
				disableObjects[i].SetActiveRecursivelyCompat(state: false);
			}
		}
	}
}
