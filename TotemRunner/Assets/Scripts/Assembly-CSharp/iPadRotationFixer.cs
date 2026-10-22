using UnityEngine;

public class iPadRotationFixer : MonoBehaviour
{
	private void Start()
	{
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.autorotateToLandscapeLeft = false;
		Screen.autorotateToLandscapeRight = false;
	}

	private void Update()
	{
		if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft && Screen.orientation != ScreenOrientation.LandscapeLeft)
		{
			Screen.orientation = ScreenOrientation.LandscapeLeft;
		}
		if (Input.deviceOrientation == DeviceOrientation.LandscapeRight && Screen.orientation != ScreenOrientation.LandscapeRight)
		{
			Screen.orientation = ScreenOrientation.LandscapeRight;
		}
	}
}
