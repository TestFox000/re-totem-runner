using UnityEngine;

public class LevelLauncher : MonoBehaviour
{
	public string loadingLevelName = "MainMenu";

	private int frameCounter = 3;

	private void Start()
	{
		frameCounter = 3;
	}

	private void Update()
	{
		if (frameCounter >= 0)
		{
			frameCounter--;
			if (frameCounter < 0)
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene(loadingLevelName);
			}
		}
	}
}
