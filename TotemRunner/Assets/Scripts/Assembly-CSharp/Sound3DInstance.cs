using System;
using UnityEngine;

[Serializable]
public class Sound3DInstance
{
	public Sound3DInstance next;

	public AudioSource source;

	public float timeLeft;

	public float timeDelay;

	public float timeRestart;

	public bool verifyPlaying;

	public int refCounter;
}
