using UnityEngine;

public class ProgressOnButton : ProgressCircle
{
	public GuiGameHud.eButtonID buttonId;

	private GuiGameHud.ButtonParams refButtonParams;

	private int progressCounter;

	private TextMesh textMesh;

	public Material enableProgressMaterial;

	public Material disableProgressMaterial;

	public Material enableBackMaterial;

	public Material disableBackMaterial;

	public Material enableForeMaterial;

	public Material disableForeMaterial;

	public Animation effectAnimation;

	public AudioClip effectSound;

	private bool isDisableText;

	private GuiGameHud.eButtonState oldButtonState = GuiGameHud.eButtonState.inactive;

	private bool isStartup = true;

	public override void Start()
	{
		textMesh = GetComponent<TextMesh>();
		int counter = progressCounter;
		progressCounter = -1;
		isStartup = true;
		SetCounter(counter);
		base.Start();
	}

	protected override void RecalculateTransform()
	{
		refButtonParams = GuiGameHud.instance.GetButtonParams(buttonId);
		if (refButtonParams == null)
		{
			base.enabled = false;
		}
		else
		{
			progressTransform = refButtonParams.transformMatrix;
		}
	}

	private void ChangeHostButtonState(GuiGameHud.eButtonState state)
	{
		bool flag = state != GuiGameHud.eButtonState.inactive;
		oldButtonState = state;
		if ((bool)textMesh && isDisableText != flag)
		{
			isDisableText = flag;
			textMesh.GetComponent<Renderer>().enabled = !isDisableText;
		}
	}

	private void Update()
	{
		isStartup = false;
		if (refButtonParams != null && oldButtonState != refButtonParams.State())
		{
			ChangeHostButtonState(refButtonParams.State());
		}
	}

	public override void DoRender()
	{
		if (refButtonParams.State() != GuiGameHud.eButtonState.activated && refButtonParams.State() != GuiGameHud.eButtonState.hidden)
		{
			base.DoRender();
		}
	}

	private void PlayCounterEffect()
	{
		if ((bool)effectAnimation)
		{
			effectAnimation.transform.position = progressTransform.GetColumn(3);
			effectAnimation.Play();
		}
		if ((bool)effectSound)
		{
			SoundEnvironment.Launch3DSoundOnce(effectSound, Camera.main.transform.position + Vector3.right * 300f);
		}
	}

	public void SetCounter(int val)
	{
		if (progressCounter == val)
		{
			return;
		}
		if (!isStartup && val > progressCounter && progressCounter >= 0 && refButtonParams != null && refButtonParams.State() != GuiGameHud.eButtonState.hidden)
		{
			PlayCounterEffect();
		}
		progressCounter = val;
		if ((bool)textMesh)
		{
			textMesh.text = ((val <= 9) ? (string.Empty + val) : "9+");
		}
		if (progressCounter > 0)
		{
			if (enableProgressMaterial != null)
			{
				progressMaterial = enableProgressMaterial;
			}
			if (enableBackMaterial != null)
			{
				backMaterial = enableBackMaterial;
			}
			if (enableForeMaterial != null)
			{
				foreMaterial = enableForeMaterial;
			}
		}
		else
		{
			if (disableProgressMaterial != null)
			{
				progressMaterial = disableProgressMaterial;
			}
			if (disableBackMaterial != null)
			{
				backMaterial = disableBackMaterial;
			}
			if (disableForeMaterial != null)
			{
				foreMaterial = disableForeMaterial;
			}
		}
	}

	public void SetDisable(bool isDisable)
	{
		if (isDisable && (bool)effectAnimation && effectAnimation.isPlaying)
		{
			effectAnimation.Rewind();
			effectAnimation.Sample();
		}
		base.gameObject.SetActiveRecursivelyCompat(!isDisable);
	}
}
