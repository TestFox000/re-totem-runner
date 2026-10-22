using System;
using UnityEngine;

public class LiftupMessage : MonoBehaviour
{
	[Serializable]
	public class LiftupMessageEntity
	{
		public TextMesh textMesh;

		[NonSerialized]
		public LiftupMessageEntity next;

		private Vector3 begPos = Vector3.zero;

		private Vector3 offset = 100f * Vector3.up;

		private float timeFollow = -1f;

		private float timeMul = 1f;

		public bool IsEmpty()
		{
			return timeFollow < 0f;
		}

		public void Start(string msg, Vector3 pos, Vector2 vOffset, float time)
		{
			begPos = pos;
			offset = new Vector3(vOffset.x, vOffset.y, 0f);
			timeFollow = 0f;
			timeMul = ((!(time > 0f)) ? 1000f : (1f / time));
			textMesh.gameObject.SetActiveRecursivelyCompat(state: true);
			GUtils.SetRendererAlpha(textMesh.GetComponent<Renderer>(), 1f);
			textMesh.text = msg;
			textMesh.transform.position = begPos;
			if ((bool)Camera.main)
			{
				AudioSource.PlayClipAtPoint(instance.sound, Camera.main.transform.position, instance.soundVolume);
			}
		}

		public void Update()
		{
			if (timeFollow >= 0f)
			{
				timeFollow += Time.deltaTime;
				float num = Mathf.Clamp01(timeFollow * timeMul);
				if (num >= 1f)
				{
					Stop();
					return;
				}
				GUtils.SetRendererAlpha(textMesh.GetComponent<Renderer>(), 1f - num);
				textMesh.transform.position = begPos + offset * num;
			}
		}

		public void Stop()
		{
			timeFollow = -1f;
			textMesh.gameObject.SetActive(false);
		}
	}

	public Vector2 defaultOffset = 100f * Vector2.up;

	public float defaultTime = 1f;

	public AudioClip sound;

	public float soundVolume = 1f;

	public LiftupMessageEntity[] list = new LiftupMessageEntity[5];

	private static LiftupMessage instance;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		LiftupMessageEntity[] array = list;
		foreach (LiftupMessageEntity liftupMessageEntity in array)
		{
			if (!liftupMessageEntity.IsEmpty())
			{
				liftupMessageEntity.Update();
			}
		}
	}

	private LiftupMessageEntity GetEmptyEntity()
	{
		LiftupMessageEntity[] array = list;
		foreach (LiftupMessageEntity liftupMessageEntity in array)
		{
			if (liftupMessageEntity.IsEmpty())
			{
				return liftupMessageEntity;
			}
		}
		return null;
	}

	public static void ShowMessage(string msg, Vector3 pos)
	{
		ShowMessage(msg, pos, instance.defaultOffset, instance.defaultTime);
	}

	private static void ShowMessage(string msg, Vector3 pos, Vector2 vOffset, float time)
	{
		LiftupMessageEntity emptyEntity = instance.GetEmptyEntity();
		if (emptyEntity != null)
		{
			emptyEntity.Start(msg, pos, vOffset, time);
		}
		else
		{
			Debug.LogWarning(Time.time + ": Overflow message pool by message - " + msg);
		}
	}
}
