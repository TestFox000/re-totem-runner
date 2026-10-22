using UnityEngine;

public class EffectControl : MonoBehaviour
{
	private class EffectControlInstance
	{
		public EffectControl ptr;

		public EffectControlInstance next;
	}

	public GameObject effectInstance;

	public float effectLifeTime = 1f;

	public Transform parentTransform;

	public Transform transformForPosition;

	private GameObject effectExample;

	private Sprite3DWithDrawLevel[] effectSprites;

	private float timeLeft = -1f;

	private static EffectControlInstance rootInstance;

	private void Awake()
	{
		EffectControlInstance effectControlInstance = new EffectControlInstance();
		effectControlInstance.ptr = this;
		effectControlInstance.next = rootInstance;
		rootInstance = effectControlInstance;
		timeLeft = -1f;
	}

	private void OnDestroy()
	{
		EffectControlInstance effectControlInstance = null;
		for (EffectControlInstance next = rootInstance; next != null; next = next.next)
		{
			if (next.ptr == this)
			{
				if (effectControlInstance != null)
				{
					effectControlInstance.next = next.next;
				}
				else
				{
					rootInstance = next.next;
				}
				break;
			}
			effectControlInstance = next;
		}
	}

	private void Update()
	{
		if (!(timeLeft >= 0f))
		{
			return;
		}
		if ((bool)effectExample && (bool)transformForPosition)
		{
			effectExample.transform.position = transformForPosition.position;
		}
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0f)
		{
			timeLeft = -1f;
			if ((bool)effectExample)
			{
				Object.Destroy(effectExample);
			}
		}
	}

	public void StartEffect()
	{
		if ((bool)effectExample)
		{
			Object.Destroy(effectExample);
		}
		effectExample = Object.Instantiate(effectInstance, base.transform.position, base.transform.rotation) as GameObject;
		effectExample.transform.parent = parentTransform;
		timeLeft = effectLifeTime;
		Component[] componentsInChildren = effectExample.GetComponentsInChildren<Sprite3DWithDrawLevel>();
		if (componentsInChildren != null)
		{
			effectSprites = new Sprite3DWithDrawLevel[componentsInChildren.Length];
			for (int i = 0; i < effectSprites.Length; i++)
			{
				effectSprites[i] = componentsInChildren[i] as Sprite3DWithDrawLevel;
			}
		}
		else
		{
			effectSprites = null;
		}
	}

	public static void DrawEffectsList()
	{
		for (EffectControlInstance next = rootInstance; next != null; next = next.next)
		{
			next.ptr.DrawEffect();
		}
	}

	private void DrawEffect()
	{
		if (timeLeft >= 0f && effectSprites != null)
		{
			for (int i = 0; i < effectSprites.Length; i++)
			{
				effectSprites[i].UpdateAfterCamera();
				effectSprites[i].RenderByOrder();
			}
		}
	}
}
