using System;
using UnityEngine;

public class CutSceneController : MonoBehaviour
{
	[Serializable]
	public class CutSceneDescribe
	{
		[Serializable]
		public class OpacityTiming
		{
			public Renderer render;

			public float startTime;

			public float timeLength = 1f;

			public float startAlpha;

			public float endAlpha = 1f;
		}

		public Animation sceneAni;

		public OpacityTiming[] opacityTimings;

		public TextSceneDescribe sceneText;
	}

	[Serializable]
	public class TextSceneDescribe
	{
		[Serializable]
		public class TextString
		{
			public string str = string.Empty;

			public float startTime;

			public float timeLength = 3f;

			public float fadeinTime = 1f;

			public float fadeoutTime = 1f;

			public int renderIndex;

			public int size = 20;

			public Vector2 pos = Vector2.zero;

			public TextString()
			{
				timeLength = 3f;
				fadeinTime = 1f;
				fadeoutTime = 1f;
				size = 20;
			}
		}

		public TextMesh textMesh1;

		public TextMesh textMesh2;

		public TextMesh textMesh3;

		public TextMesh textMesh4;

		public TextString[] texts;
	}

	[Serializable]
	public class ProcessingMaterials
	{
		public Material mat;

		public float alphaBeg;

		public float alphaEnd = 1f;

		public float startTime;

		public float timeLength = 1f;
	}

	private static CutSceneController _singleton;

	private float timeFollow = -1f;

	private int curSceneIndex;

	public CutSceneDescribe[] cutScenes;

	private ProcessingMaterials[] processingMaterials = new ProcessingMaterials[10];

	private void Awake()
	{
		_singleton = this;
		for (int i = 0; i < processingMaterials.Length; i++)
		{
			processingMaterials[i] = new ProcessingMaterials();
		}
	}

	private void Update()
	{
		if (timeFollow < 0f)
		{
			return;
		}
		float num = timeFollow;
		timeFollow += Time.deltaTime;
		for (int i = 0; i < processingMaterials.Length; i++)
		{
			if ((bool)processingMaterials[i].mat)
			{
				EvaluateAlpha(processingMaterials[i]);
			}
		}
		if (cutScenes[curSceneIndex].opacityTimings != null)
		{
			for (int j = 0; j < cutScenes[curSceneIndex].opacityTimings.Length; j++)
			{
				float startTime = cutScenes[curSceneIndex].opacityTimings[j].startTime;
				if (startTime >= num && startTime < timeFollow)
				{
					AddProcessingMaterial(cutScenes[curSceneIndex].opacityTimings[j]);
				}
			}
		}
		if (cutScenes[curSceneIndex].sceneText.texts == null)
		{
			return;
		}
		for (int k = 0; k < cutScenes[curSceneIndex].sceneText.texts.Length; k++)
		{
			float startTime2 = cutScenes[curSceneIndex].sceneText.texts[k].startTime;
			if (startTime2 >= num && startTime2 < timeFollow)
			{
				AddProcessingString(cutScenes[curSceneIndex].sceneText.texts[k], isFadein: true);
			}
			startTime2 += cutScenes[curSceneIndex].sceneText.texts[k].timeLength;
			if (startTime2 >= num && startTime2 < timeFollow)
			{
				AddProcessingString(cutScenes[curSceneIndex].sceneText.texts[k], isFadein: false);
			}
		}
	}

	public void AddProcessingMaterial(CutSceneDescribe.OpacityTiming descr)
	{
		int i;
		for (i = 9; i < processingMaterials.Length && (bool)processingMaterials[i].mat; i++)
		{
		}
		if (i >= processingMaterials.Length)
		{
			Debug.LogWarning(Time.time + ": ProcessingMaterial list is overflow by scene timing " + timeFollow);
			return;
		}
		processingMaterials[i].mat = descr.render.material;
		processingMaterials[i].alphaBeg = descr.startAlpha;
		processingMaterials[i].alphaEnd = descr.endAlpha;
		processingMaterials[i].startTime = descr.startTime;
		processingMaterials[i].timeLength = descr.timeLength;
		EvaluateAlpha(processingMaterials[i]);
	}

	public void AddProcessingString(TextSceneDescribe.TextString descr, bool isFadein)
	{
		CutSceneDescribe.OpacityTiming opacityTiming = new CutSceneDescribe.OpacityTiming();
		TextMesh textMesh = null;
		switch (descr.renderIndex)
		{
		case 0:
			textMesh = cutScenes[curSceneIndex].sceneText.textMesh1;
			break;
		case 1:
			textMesh = cutScenes[curSceneIndex].sceneText.textMesh2;
			break;
		case 2:
			textMesh = cutScenes[curSceneIndex].sceneText.textMesh3;
			break;
		case 3:
			textMesh = cutScenes[curSceneIndex].sceneText.textMesh4;
			break;
		}
		opacityTiming.render = textMesh.GetComponent<Renderer>();
		if (isFadein)
		{
			textMesh.text = descr.str;
			textMesh.characterSize = descr.size;
			textMesh.transform.position = new Vector3(descr.pos.x + textMesh.transform.parent.position.x, descr.pos.y + textMesh.transform.parent.position.y, textMesh.transform.position.z);
			opacityTiming.startAlpha = 0f;
			opacityTiming.endAlpha = 1f;
			opacityTiming.startTime = descr.startTime;
			opacityTiming.timeLength = descr.fadeinTime;
		}
		else
		{
			opacityTiming.startAlpha = 1f;
			opacityTiming.endAlpha = 0f;
			opacityTiming.startTime = descr.startTime + descr.timeLength;
			opacityTiming.timeLength = descr.fadeoutTime;
		}
		AddProcessingMaterial(opacityTiming);
	}

	private void EvaluateAlpha(ProcessingMaterials processing)
	{
		float num = (timeFollow - processing.startTime) / processing.timeLength;
		if (num < 0f)
		{
			num = 0f;
		}
		else if (num >= 1f)
		{
			Color color = processing.mat.color;
			color.a = processing.alphaEnd;
			processing.mat.color = color;
			processing.mat = null;
		}
		else
		{
			Color color2 = processing.mat.color;
			color2.a = Mathf.Lerp(processing.alphaBeg, processing.alphaEnd, num);
			processing.mat.color = color2;
		}
	}

	public static void StartCutScene()
	{
		if (_singleton.cutScenes != null && _singleton.cutScenes.Length > 0)
		{
			_singleton.timeFollow = 0f;
			_singleton.curSceneIndex = 0;
			_singleton.cutScenes[_singleton.curSceneIndex].sceneAni.Play();
		}
		IngameVideoPlayer.PlayMovie("intro");
	}

	public static void StopCutScene()
	{
		if (_singleton.cutScenes != null && _singleton.curSceneIndex < _singleton.cutScenes.Length)
		{
			_singleton.cutScenes[_singleton.curSceneIndex].sceneAni.Stop();
		}
		_singleton.timeFollow = -1f;
		for (int i = 0; i < _singleton.processingMaterials.Length; i++)
		{
			_singleton.processingMaterials[i].mat = null;
		}
		IngameVideoPlayer.StopMovie();
	}

	public static bool IsFinishedCutScene()
	{
		if (_singleton.timeFollow >= 0f)
		{
			if (_singleton.curSceneIndex < 0 || _singleton.curSceneIndex >= _singleton.cutScenes.Length)
			{
				return true;
			}
			if (_singleton.cutScenes[_singleton.curSceneIndex].sceneAni == null)
			{
				return true;
			}
			if (!_singleton.cutScenes[_singleton.curSceneIndex].sceneAni.isPlaying)
			{
				return true;
			}
		}
		if (!IngameVideoPlayer.IsPlaying())
		{
			return true;
		}
		return false;
	}
}
