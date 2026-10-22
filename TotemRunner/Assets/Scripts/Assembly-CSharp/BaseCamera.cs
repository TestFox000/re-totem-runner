using UnityEngine;

public class BaseCamera : MonoBehaviour
{
	public enum ECameraZoomingType
	{
		death = 0,
		grassGrowing = 1,
		boarSpeedup = 2,
		boarPowerAttack = 3,
		titanInsidingZoom = 4,
		ghostStart = 5,
		ghostFly = 6,
		qteZooming = 7,
		startZooming = 8,
		none = 9
	}

	public AfterCameraUpdated[] afterUpdatedObjects;

	public Texture2D loadingTexture;

	public Texture2D loadingLabel;

	public Rect loadingLabelRect = new Rect(0f, 0f, 256f, 64f);

	protected int launchLoading = -1;

	protected string loadingLevelName = string.Empty;

	protected int loadingLevelIndex = -1;

	private static BaseCamera _singleton;

	public static BaseCamera instance => _singleton;

	public virtual void Awake()
	{
		_singleton = this;
		AfterCameraUpdated[] array = afterUpdatedObjects;
		foreach (AfterCameraUpdated afterCameraUpdated in array)
		{
			afterCameraUpdated.AwakeAfterCamera(base.transform);
		}
	}

	public virtual void LateUpdate()
	{
		AfterCameraUpdated[] array = afterUpdatedObjects;
		foreach (AfterCameraUpdated afterCameraUpdated in array)
		{
			afterCameraUpdated.UpdateAfterCamera();
		}
		if (launchLoading >= 0 && --launchLoading == 0)
		{
			TimeController.ResetToInit();
			if (loadingLevelIndex < 0)
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene(loadingLevelName);
			}
			else
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene(loadingLevelIndex);
			}
		}
	}

	public virtual void OnPostRender()
	{
		AfterCameraUpdated afterCameraUpdated = null;
		AfterCameraUpdated[] array = afterUpdatedObjects;
		foreach (AfterCameraUpdated afterCameraUpdated2 in array)
		{
			while (afterCameraUpdated != null && afterCameraUpdated.GetSecondOrder() <= afterCameraUpdated2.order)
			{
				afterCameraUpdated.RenderByOrderSecond();
				afterCameraUpdated = afterCameraUpdated.nextSecondOrderedObject;
			}
			afterCameraUpdated2.RenderByOrder();
			if (afterCameraUpdated2.GetSecondOrder() < 0)
			{
				continue;
			}
			if (afterCameraUpdated == null)
			{
				afterCameraUpdated = afterCameraUpdated2;
				afterCameraUpdated.nextSecondOrderedObject = null;
				continue;
			}
			if (afterCameraUpdated.GetSecondOrder() > afterCameraUpdated2.GetSecondOrder())
			{
				afterCameraUpdated2.nextSecondOrderedObject = afterCameraUpdated;
				afterCameraUpdated = afterCameraUpdated2;
				continue;
			}
			AfterCameraUpdated afterCameraUpdated3 = afterCameraUpdated;
			while (afterCameraUpdated3 != null)
			{
				if (afterCameraUpdated3.nextSecondOrderedObject == null || afterCameraUpdated3.nextSecondOrderedObject.GetSecondOrder() > afterCameraUpdated2.GetSecondOrder())
				{
					afterCameraUpdated2.nextSecondOrderedObject = afterCameraUpdated3.nextSecondOrderedObject;
					afterCameraUpdated3.nextSecondOrderedObject = afterCameraUpdated2;
					break;
				}
				afterCameraUpdated3 = afterCameraUpdated3.nextSecondOrderedObject;
			}
		}
		while (afterCameraUpdated != null)
		{
			afterCameraUpdated.RenderByOrderSecond();
			afterCameraUpdated = afterCameraUpdated.nextSecondOrderedObject;
		}
	}

	public virtual void OnGUI()
	{
		if (launchLoading >= 0)
		{
			float pixelWidth = base.GetComponent<Camera>().pixelWidth;
			float pixelHeight = base.GetComponent<Camera>().pixelHeight;
			if ((bool)loadingTexture)
			{
				GUI.DrawTexture(new Rect(0f, 0f, pixelWidth, pixelHeight), loadingTexture, ScaleMode.StretchToFill, alphaBlend: false);
			}
			if ((bool)loadingLabel)
			{
				GUI.DrawTexture(new Rect((pixelWidth - loadingLabelRect.width) * 0.5f + loadingLabelRect.x, (pixelHeight - loadingLabelRect.height) * 0.5f - loadingLabelRect.y, loadingLabelRect.width, loadingLabelRect.height), loadingLabel, ScaleMode.StretchToFill, alphaBlend: false);
			}
		}
	}

	public void LaunchLoading(string lvlName)
	{
		launchLoading = 4;
		loadingLevelName = lvlName;
		loadingLevelIndex = -1;
	}

	public bool IsLoading()
	{
		return launchLoading >= 0;
	}

	public static void GLaunchLoading(string lvlName)
	{
		if (_singleton != null)
		{
			_singleton.LaunchLoading(lvlName);
		}
	}

	public static void GLaunchLoading(int lvlIndex)
	{
		if (_singleton != null)
		{
			_singleton.loadingLevelIndex = lvlIndex;
			_singleton.launchLoading = 4;
		}
	}
}
