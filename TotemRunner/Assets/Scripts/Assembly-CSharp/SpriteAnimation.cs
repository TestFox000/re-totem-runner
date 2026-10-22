using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
	public enum eSpriteSequenceState
	{
		play = 0,
		pause = 1,
		stop = 2
	}

	public Sprite3D spriteComponent;

	public Sprite3DWithDrawLevel spriteComponentOrdered;

	private Material spriteMaterial;

	public Texture2D[] sequence;

	public int frameRate = 4;

	private int currentFrame;

	private float currentTime;

	private eSpriteSequenceState curState;

	private void Awake()
	{
		if ((bool)spriteComponent)
		{
			spriteMaterial = spriteComponent.material;
		}
		else if ((bool)spriteComponentOrdered)
		{
			spriteMaterial = spriteComponentOrdered.material;
		}
	}

	private void Update()
	{
		if (curState != eSpriteSequenceState.play)
		{
			return;
		}
		currentTime += (float)frameRate * Time.deltaTime;
		int num = Mathf.FloorToInt(currentTime);
		if (num != currentFrame)
		{
			if (num >= sequence.Length)
			{
				num = 0;
				currentTime = 0f;
			}
			ChangeFrame(num);
		}
	}

	public void StopSequence()
	{
		curState = eSpriteSequenceState.stop;
		currentFrame = 0;
		currentTime = 0f;
	}

	public void PauseSequence()
	{
		curState = eSpriteSequenceState.pause;
	}

	public void PlaySequence()
	{
		curState = eSpriteSequenceState.play;
	}

	private void ChangeFrame(int newFrame)
	{
		currentFrame = newFrame;
		spriteMaterial.mainTexture = sequence[newFrame];
	}
}
