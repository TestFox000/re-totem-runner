using System;
using UnityEngine;

[Serializable]
public class SoundDescribe
{
	public string id;

	public AudioClip sound;

	[NonSerialized]
	public AudioSource soundSource;

	public float delayInSec;

	public float soundVolume = 1f;

	public bool loop = true;
}
