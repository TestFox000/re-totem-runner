using UnityEngine;

public class TutorialMessageChoose : TutorialMessage
{
	public MeshRenderer refOtherRender;

	public Texture2D backOtherTexture;

	public TextAsset[] backOtherList;

	private int backOtherFrame;

	private bool chooseComplete;

	public override void ShowMessage()
	{
		base.ShowMessage();
		chooseComplete = false;
		if (refOtherRender != null)
		{
			backOtherFrame = 0;
			if (backOtherList != null && backOtherList.Length > 0)
			{
				GUtils.LoadPictImage(refOtherRender, backOtherList[0], ref backOtherTexture);
				backOtherFrame = 0;
			}
			refOtherRender.gameObject.SetActiveRecursivelyCompat(state: true);
		}
		GuiGameHud.instance.scoreCounter.gameObject.SetActive(false);
	}

	public override void HideMessage()
	{
		base.HideMessage();
		if (refOtherRender != null)
		{
			refOtherRender.gameObject.SetActiveRecursivelyCompat(state: false);
		}
		GuiGameHud.instance.scoreCounter.gameObject.SetActive(true);
	}

	protected override void NextAnimFrame()
	{
		base.NextAnimFrame();
		backOtherFrame++;
		if (backOtherFrame >= backOtherList.Length)
		{
			backOtherFrame = 0;
		}
		GUtils.LoadPictImage(refOtherRender, backOtherList[backOtherFrame], ref backOtherTexture);
	}

	public override void Update()
	{
		base.Update();
		if (isUpdating && Input.GetMouseButtonDown(0))
		{
			if (IsInsideRender(refMessageRender, Input.mousePosition))
			{
				chooseComplete = true;
				ChooseControl(isButton: true);
			}
			if (IsInsideRender(refOtherRender, Input.mousePosition))
			{
				chooseComplete = true;
				ChooseControl(isButton: false);
			}
		}
	}

	private void ChooseControl(bool isButton)
	{
		GameInput.inputStyle = (isButton ? eInputStyle.eisButton : eInputStyle.eisAcceleration);
		GameDataStorage.SaveGameSettings();
	}

	private bool IsInsideRender(MeshRenderer mr, Vector2 scrPos)
	{
		if (mr == null)
		{
			return false;
		}
		MeshFilter component = mr.GetComponent<MeshFilter>();
		if (component == null)
		{
			return false;
		}
		if (component.mesh == null)
		{
			return false;
		}
		Camera camera = GuiGameHud.instance.GetComponent<Camera>();
		Ray ray = camera.ScreenPointToRay(scrPos);
		ray.origin = component.transform.InverseTransformPoint(ray.origin);
		ray.direction = component.transform.InverseTransformDirection(ray.direction);
		return component.mesh.bounds.IntersectRay(ray);
	}

	protected override bool KeyIsPressed()
	{
		return chooseComplete;
	}
}
