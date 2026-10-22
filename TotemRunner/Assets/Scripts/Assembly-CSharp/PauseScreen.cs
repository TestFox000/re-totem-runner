using System;
using UnityEngine;

public class PauseScreen : GuiScreen
{
	[Serializable]
	public class PauseNodes
	{
		public Renderer backFillerRenderer;

		public Transform star1;

		public Transform star1Empty;

		public Transform star2;

		public Transform star2Empty;

		public Transform star3;

		public Transform star3Empty;

		public TextMesh levelLabel;

		public Transform buttonNextFern;

		public Transform buttonReplayFern;

		public Transform buttonMenuFern;
	}

	private StatisticScreen.eStatisticState state;

	public float fadeinTime = 1f;

	public Vector2 buttonsSize = 128f * Vector2.one;

	public Texture2D buttonNextImage;

	public Texture2D buttonReplayImage;

	public Texture2D buttonMenuImage;

	public Texture2D buttonNextImageSelected;

	public Texture2D buttonReplayImageSelected;

	public Texture2D buttonMenuImageSelected;

	public PauseNodes nodeList;

	public float fernScaleLength = 0.4f;

	public float nextFernScaleOffset = 0.4f;

	public float replayFernScaleOffset = 0.2f;

	public float menuFernScaleOffset;

	private Rect rectButtonNext = new Rect(896f, 736f, 128f, 32f);

	private Rect rectButtonReplay = new Rect(448f, 736f, 128f, 32f);

	private Rect rectButtonMenu = new Rect(0f, 736f, 128f, 32f);

	private float guiTimeFollow;

	private void Awake()
	{
		base.gameObject.SetActiveRecursivelyCompat(state: false);
		base.enabled = false;
	}

	private void Start()
	{
		RecalculateButtonsRectangle();
	}

	private void Update()
	{
		if (state == StatisticScreen.eStatisticState.disable)
		{
			return;
		}
		switch (state)
		{
		case StatisticScreen.eStatisticState.disable:
			break;
		case StatisticScreen.eStatisticState.fadein:
			guiTimeFollow += GameSettings.realDeltaTime;
			if ((bool)nodeList.backFillerRenderer)
			{
				if (fadeinTime > 0f)
				{
					nodeList.backFillerRenderer.material.color = new Color(1f, 1f, 1f, Mathf.Clamp01(guiTimeFollow / fadeinTime));
				}
				else
				{
					nodeList.backFillerRenderer.material.color = Color.white;
				}
			}
			if (guiTimeFollow >= fadeinTime)
			{
				ChangeState(StatisticScreen.eStatisticState.showForm);
			}
			break;
		case StatisticScreen.eStatisticState.showForm:
			ChangeState(StatisticScreen.eStatisticState.waitInput);
			break;
		case StatisticScreen.eStatisticState.waitInput:
			if (guiTimeFollow <= nextFernScaleOffset + fernScaleLength)
			{
				guiTimeFollow += GameSettings.realDeltaTime;
				float num = CalculateFernSizeByScaleTime(Mathf.Clamp01((guiTimeFollow - nextFernScaleOffset) / fernScaleLength));
				nodeList.buttonNextFern.localScale = new Vector3(num, num, 1f);
				num = CalculateFernSizeByScaleTime(Mathf.Clamp01((guiTimeFollow - replayFernScaleOffset) / fernScaleLength));
				nodeList.buttonReplayFern.localScale = new Vector3(num, num, 1f);
				num = CalculateFernSizeByScaleTime(Mathf.Clamp01((guiTimeFollow - menuFernScaleOffset) / fernScaleLength));
				nodeList.buttonMenuFern.localScale = new Vector3(0f - num, num, 1f);
			}
			if (GameInput.IsBackPressed() || GameInput.IsMenuPressed())
			{
				ChangeState(StatisticScreen.eStatisticState.disable);
				GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.game);
			}
			break;
		case StatisticScreen.eStatisticState.loading:
			break;
		case StatisticScreen.eStatisticState.finalizeLevel:
		case StatisticScreen.eStatisticState.calculateScore:
		case StatisticScreen.eStatisticState.showSummaryScore:
		case StatisticScreen.eStatisticState.showStamps:
			break;
		}
	}

	private int CalculateFernSizeByScaleTime(float timeRel)
	{
		int num = 128;
		int num2 = 320;
		int num3 = 256;
		if (timeRel <= 0f)
		{
			return num;
		}
		if (timeRel >= 1f)
		{
			return num3;
		}
		if (timeRel < 0.5f)
		{
			return num + Mathf.RoundToInt((float)(num2 - num) * 2f * timeRel);
		}
		return num3 + Mathf.FloorToInt((float)(num2 - num3) * 2f * (1f - timeRel));
	}

	public override void UpdateInGUI()
	{
		if (isEnabled && state != StatisticScreen.eStatisticState.loading && state == StatisticScreen.eStatisticState.waitInput)
		{
			GUIStyle gUIStyle = new GUIStyle();
			gUIStyle.normal.background = buttonNextImage;
			gUIStyle.active.background = buttonNextImageSelected;
			if (GUI.Button(rectButtonNext, string.Empty, gUIStyle))
			{
				ChangeState(StatisticScreen.eStatisticState.disable);
				GuiGameHud.instance.SetGuiState(GuiGameHud.eGuiGameHudState.game);
			}
			gUIStyle.normal.background = buttonReplayImage;
			gUIStyle.active.background = buttonReplayImageSelected;
			if (GUI.Button(rectButtonReplay, string.Empty, gUIStyle))
			{
				ChangeState(StatisticScreen.eStatisticState.loading);
				BaseCamera.GLaunchLoading(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
			}
			gUIStyle.normal.background = buttonMenuImage;
			gUIStyle.active.background = buttonMenuImageSelected;
			if (GUI.Button(rectButtonMenu, string.Empty, gUIStyle))
			{
				ChangeState(StatisticScreen.eStatisticState.loading);
				BaseCamera.GLaunchLoading(1);
			}
		}
	}

	public override void StartShow(Camera cam)
	{
		base.StartShow(cam);
		TimeController.StartPause(isPause: true);
		PlayerController.instance.DisableController(disable: true);
		ChangeState(StatisticScreen.eStatisticState.fadein);
	}

	public override void StopShow()
	{
		ChangeState(StatisticScreen.eStatisticState.disable);
	}

	private void RecalculateButtonsRectangle()
	{
		Vector2 vector = buttonsSize;
		if (refCamera.pixelHeight != 768f)
		{
			vector.y *= refCamera.pixelHeight / 768f;
			vector.x *= refCamera.pixelHeight / 768f;
		}
		Vector3 vector2 = refCamera.WorldToScreenPoint(nodeList.buttonNextFern.position);
		Vector3 vector3 = refCamera.WorldToScreenPoint(nodeList.buttonReplayFern.position);
		Vector3 vector4 = refCamera.WorldToScreenPoint(nodeList.buttonMenuFern.position);
		vector2.y = refCamera.pixelHeight - vector2.y;
		vector3.y = refCamera.pixelHeight - vector3.y;
		vector4.y = refCamera.pixelHeight - vector4.y;
		rectButtonNext = new Rect(vector2.x - 0.5f * vector.x, vector2.y - 0.5f * vector.y, vector.x, vector.y);
		rectButtonReplay = new Rect(vector3.x - 0.5f * vector.x, vector3.y - 0.5f * vector.y, vector.x, vector.y);
		rectButtonMenu = new Rect(vector4.x - 0.5f * vector.x, vector4.y - 0.5f * vector.y, vector.x, vector.y);
	}

	private void ChangeState(StatisticScreen.eStatisticState newState)
	{
		if (state == newState)
		{
			return;
		}
		switch (newState)
		{
		case StatisticScreen.eStatisticState.disable:
			base.gameObject.SetActiveRecursivelyCompat(state: false);
			base.enabled = false;
			break;
		case StatisticScreen.eStatisticState.fadein:
			base.enabled = true;
			base.gameObject.SetActive(true);
			if ((bool)nodeList.backFillerRenderer)
			{
				nodeList.backFillerRenderer.gameObject.SetActive(true);
				nodeList.backFillerRenderer.material.color = new Color(1f, 1f, 1f, 0f);
			}
			guiTimeFollow = 0f;
			break;
		case StatisticScreen.eStatisticState.showForm:
		{
			base.gameObject.SetActiveRecursivelyCompat(state: true);
			nodeList.buttonNextFern.gameObject.SetActive(false);
			nodeList.buttonReplayFern.gameObject.SetActive(false);
			nodeList.buttonMenuFern.gameObject.SetActive(false);
			int grassCount = GrassGifManager.instance.GetGrassCount();
			if (grassCount < 100)
			{
				nodeList.star1.gameObject.SetActive(false);
			}
			else
			{
				nodeList.star1Empty.gameObject.SetActive(false);
			}
			if (grassCount < 200)
			{
				nodeList.star2.gameObject.SetActive(false);
			}
			else
			{
				nodeList.star2Empty.gameObject.SetActive(false);
			}
			if (grassCount < 300)
			{
				nodeList.star3.gameObject.SetActive(false);
			}
			else
			{
				nodeList.star3Empty.gameObject.SetActive(false);
			}
			nodeList.levelLabel.text = GameSettings.GetLevelCompleteData().levelName;
			break;
		}
		case StatisticScreen.eStatisticState.waitInput:
		{
			guiTimeFollow = 0f;
			nodeList.buttonNextFern.gameObject.SetActive(true);
			nodeList.buttonReplayFern.gameObject.SetActive(true);
			nodeList.buttonMenuFern.gameObject.SetActive(true);
			float num = CalculateFernSizeByScaleTime(0f);
			nodeList.buttonNextFern.localScale = new Vector3(num, num, 1f);
			nodeList.buttonReplayFern.localScale = new Vector3(num, num, 1f);
			nodeList.buttonMenuFern.localScale = new Vector3(0f - num, num, 1f);
			break;
		}
		}
		state = newState;
	}
}
