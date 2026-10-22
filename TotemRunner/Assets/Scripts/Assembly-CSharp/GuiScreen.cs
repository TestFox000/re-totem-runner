using UnityEngine;

public class GuiScreen : MonoBehaviour
{
	protected bool isEnabled = true;

	protected Camera refCamera;

	private float disableTimeout = -1f;

	public virtual void UpdateInGUI()
	{
	}

	public virtual void StartShow(Camera refCam)
	{
		refCamera = refCam;
	}

	public virtual void StopShow()
	{
	}

	public void Enabled(bool val)
	{
		isEnabled = val;
	}

	public bool Enabled()
	{
		if (disableTimeout >= 0f && Time.timeSinceLevelLoad < disableTimeout)
		{
			return false;
		}
		return isEnabled;
	}

	public void SetDisableTimeout(float val)
	{
		disableTimeout = Time.timeSinceLevelLoad + val;
	}
}
