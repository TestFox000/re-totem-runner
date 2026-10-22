using System;
using UnityEngine;

public class SoundEnvironment : MonoBehaviour
{
	[Serializable]
	public class SoundDescriber
	{
		public AudioClip sound;

		public float volume = 1f;

		public float delay;

		public SoundDescriber()
		{
			sound = null;
			volume = 1f;
			delay = 0f;
		}
	}

	[Serializable]
	public class SoundRepeater
	{
		public AudioClip sound;

		public float volume = 1f;

		public float timeStep = 0.2f;

		private float timeLeft = -1f;

		public void StartSound(AudioClip srcSound, float srcVolume, float srcTimeStep, float srcTimeDelay, Vector3 pos)
		{
			sound = srcSound;
			volume = srcVolume;
			timeStep = srcTimeStep;
			if (srcTimeDelay <= 0f)
			{
				Launch3DSound(sound, volume, pos, 0f, isLoop: false, null);
				timeLeft = timeStep;
			}
			else
			{
				timeLeft = srcTimeDelay;
			}
		}

		public void StopSound()
		{
			sound = null;
		}

		public void Update(Vector3 pos)
		{
			if (!sound)
			{
				return;
			}
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				timeLeft += timeStep;
				if (timeLeft < 0f)
				{
					timeLeft = timeStep;
				}
				Launch3DSound(sound, volume, pos, 0f, isLoop: false, null);
			}
		}
	}

	private Transform soundFolder;

	public float musicVolume = 1f;

	public AudioClip musicSound;

	private AudioSource sndMusic;

	public float environmentVolume = 1f;

	public AudioClip environmentSound;

	private AudioSource sndEnvironment;

	private AudioSource sndMusicAddition;

	private float timeOutMusicAddition;

	private float musicAdditionVolume = 1f;

	private float soundFadeoutTime = 0.8f;

	private float soundFadeinTimebound;

	private float soundFadeinTime = 1f;

	private Sound3DInstance rootSoundInstance;

	private static SoundEnvironment _singleton;

	public static SoundEnvironment instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = UnityEngine.Object.FindObjectOfType(typeof(SoundEnvironment)) as SoundEnvironment;
			}
			return _singleton;
		}
	}

	public static void PauseMusicAddition(bool isPause)
	{
	}

	private void Awake()
	{
		_singleton = this;
		soundFolder = base.transform.Find("Sounds");
		if (!soundFolder)
		{
			soundFolder = new GameObject("Sounds").transform;
			soundFolder.parent = base.transform;
		}
		timeOutMusicAddition = 0f;
	}

	private void Start()
	{
		if ((bool)base.GetComponent<AudioSource>())
		{
			base.GetComponent<AudioSource>().volume = GameSettings.sound.musicVolume;
		}
		LaunchMusic(musicSound);
		LaunchEnvironment(environmentSound);
	}

	private void Update()
	{
		if (timeOutMusicAddition > 0f)
		{
			float num = timeOutMusicAddition;
			timeOutMusicAddition -= Time.deltaTime;
			if (num > soundFadeinTimebound)
			{
				float num2 = timeOutMusicAddition - soundFadeinTimebound;
				if (num2 <= 0f)
				{
					sndMusicAddition.volume = musicAdditionVolume;
				}
				else
				{
					sndMusicAddition.volume = musicAdditionVolume * (1f - num2 / soundFadeinTime);
				}
			}
			if (timeOutMusicAddition < soundFadeoutTime)
			{
				if (timeOutMusicAddition <= 0f)
				{
					StopSoundObject(sndMusicAddition);
					sndMusicAddition = null;
					timeOutMusicAddition = 0f;
				}
				else
				{
					sndMusicAddition.volume = musicAdditionVolume * timeOutMusicAddition / soundFadeoutTime;
				}
			}
		}
		Sound3DInstance prevInst = null;
		for (Sound3DInstance next = rootSoundInstance; next != null; next = next.next)
		{
			if (next.source != null)
			{
				if (next.timeDelay > 0f)
				{
					next.timeDelay -= Time.deltaTime;
					if (!(next.timeDelay <= 0f))
					{
						continue;
					}
					next.timeDelay = 0f;
					next.source.Play();
					next.verifyPlaying = true;
				}
				else if (next.timeRestart > 0f)
				{
					next.timeRestart -= Time.deltaTime;
					if (next.timeRestart <= 0f)
					{
						next.timeRestart = 0f;
						next.source.Stop();
						next.source.Play();
						if (next.timeLeft >= 0f)
						{
							next.timeLeft = next.source.clip.length;
						}
					}
				}
				if (next.verifyPlaying)
				{
					if (next.timeLeft > 0f && !next.source.isPlaying)
					{
						next.source.Play();
					}
					next.verifyPlaying = false;
				}
				if (next.timeLeft >= 0f)
				{
					next.timeLeft -= Time.deltaTime;
					if (next.timeLeft <= 0f)
					{
						DestroySoundInstance(next, prevInst);
					}
				}
				else if (next.refCounter <= 0)
				{
					float x = next.source.transform.position.x;
					if (ScrollingCamera.leftCameraBound - x > 900f || x - ScrollingCamera.rightCameraBound > 1000f)
					{
						DestroySoundInstance(next, prevInst);
					}
				}
			}
			prevInst = next;
		}
	}

	public void StopSoundObject(AudioSource sndSrc)
	{
		if ((bool)sndSrc)
		{
			UnityEngine.Object.Destroy(sndSrc.gameObject);
		}
	}

	public void LaunchMusic(AudioClip musicSnd)
	{
		LaunchSoundObject("music", sndMusic, musicSnd, musicVolume * GameSettings.sound.musicVolume, 0f);
	}

	public void LaunchEnvironment(AudioClip envSnd)
	{
		LaunchSoundObject("environment", sndEnvironment, envSnd, environmentVolume * GameSettings.sound.soundVolume, 0f);
	}

	public void FadeoutMusicAddition(float fadeout)
	{
		if ((bool)sndMusicAddition)
		{
			timeOutMusicAddition = fadeout;
			soundFadeoutTime = fadeout;
			soundFadeinTime = 0f;
			soundFadeinTimebound = fadeout;
		}
	}

	public void LaunchMusicAddition(AudioClip musicAdditionSnd, float volume, float timeStart, float timeOut, float fadein, float fadeout)
	{
		if (!sndMusicAddition || !(sndMusicAddition.clip == musicAdditionSnd))
		{
			musicAdditionVolume = volume * GameSettings.sound.musicVolume;
			sndMusicAddition = LaunchSoundObject("music addition", sndMusicAddition, musicAdditionSnd, musicAdditionVolume, 0f);
			if ((bool)sndMusicAddition)
			{
				sndMusicAddition.time = timeStart;
			}
			timeOutMusicAddition = timeOut;
			soundFadeoutTime = fadeout;
			soundFadeinTime = fadein;
			soundFadeinTimebound = timeOut - fadein;
		}
	}

	public static void Launch3DSoundOnce(AudioClip sndClip, Vector3 pos)
	{
		pos.z = Camera.main.transform.position.z;
		AudioSource.PlayClipAtPoint(sndClip, pos, GameSettings.sound.soundVolume);
	}

	public static void Launch2DSound(AudioClip sndClip, float vol, float timeStart, float timeLength)
	{
		if (timeLength < 0f)
		{
			timeLength = sndClip.length - timeStart;
		}
		if (!(timeLength <= 0f))
		{
			Sound3DInstance instanceForSound = instance.GetInstanceForSound(sndClip);
			instanceForSound.refCounter++;
			AudioSource audioSource;
			if ((bool)instanceForSound.source)
			{
				audioSource = instanceForSound.source;
				audioSource.Stop();
			}
			else
			{
				GameObject gameObject = new GameObject(sndClip.name);
				gameObject.transform.parent = _singleton.soundFolder;
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.spatialBlend = 0f;
				audioSource.clip = sndClip;
				audioSource.playOnAwake = false;
				instanceForSound.source = audioSource;
			}
			audioSource.gameObject.SetActive(true);
			audioSource.gameObject.transform.position = Vector3.zero;
			audioSource.loop = false;
			audioSource.volume = vol * GameSettings.sound.soundVolume;
			audioSource.Play();
			instanceForSound.timeDelay = 0f;
			instanceForSound.timeLeft = timeLength;
		}
	}

	public static void Launch3DSound(AudioClip sndClip, float vol, Vector3 pos, float delaySec, bool isLoop, Transform parent)
	{
		Sound3DInstance instanceForSound = _singleton.GetInstanceForSound(sndClip);
		instanceForSound.refCounter++;
		AudioSource audioSource;
		if ((bool)instanceForSound.source)
		{
			audioSource = instanceForSound.source;
			if (delaySec <= 0f)
			{
				audioSource.Stop();
			}
			audioSource.transform.parent = ((!parent) ? _singleton.soundFolder : parent);
		}
		else
		{
			GameObject gameObject = new GameObject(sndClip.name);
			gameObject.transform.parent = ((!parent) ? _singleton.soundFolder : parent);
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.spatialBlend = 1f;
			audioSource.clip = sndClip;
			audioSource.minDistance = 350f;
			audioSource.maxDistance = 2000f;
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.playOnAwake = false;
			instanceForSound.source = audioSource;
			instanceForSound.timeDelay = delaySec;
		}
		pos.z = Camera.main.transform.position.z;
		audioSource.gameObject.transform.position = pos;
		audioSource.loop = isLoop;
		audioSource.volume = vol * GameSettings.sound.soundVolume;
		if (delaySec == 0f)
		{
			audioSource.gameObject.SetActive(true);
			audioSource.Play();
			instanceForSound.verifyPlaying = true;
			instanceForSound.timeRestart = 0f;
			instanceForSound.timeDelay = 0f;
		}
		else
		{
			instanceForSound.verifyPlaying = false;
			instanceForSound.timeRestart = Mathf.Clamp(delaySec - instanceForSound.timeDelay, 0f, delaySec);
		}
		instanceForSound.timeLeft = ((!isLoop) ? sndClip.length : (-1f));
	}

	public static void Stop3DSound(AudioClip sndClip, bool waitForMute)
	{
		Sound3DInstance prevInst = null;
		for (Sound3DInstance next = _singleton.rootSoundInstance; next != null; next = next.next)
		{
			if ((bool)next.source && next.source.clip == sndClip)
			{
				if (--next.refCounter <= 0 && !waitForMute)
				{
					DestroySoundInstance(next, prevInst);
				}
				break;
			}
			prevInst = next;
		}
	}

	public static void DestroySoundInstance(Sound3DInstance inst, Sound3DInstance prevInst)
	{
		inst.source.Stop();
		UnityEngine.Object.Destroy(inst.source.gameObject);
		inst.source = null;
	}

	private AudioSource LaunchSoundObject(string objName, AudioSource refSoundSource, AudioClip sndClip, float vol, float delaySec)
	{
		if (!refSoundSource)
		{
			GameObject gameObject = new GameObject(objName);
			gameObject.transform.parent = soundFolder;
			refSoundSource = gameObject.AddComponent<AudioSource>();
			refSoundSource.loop = true;
			refSoundSource.spatialBlend = 0f;
		}
		else
		{
			refSoundSource.Stop();
		}
		if (refSoundSource.clip != sndClip)
		{
			refSoundSource.clip = sndClip;
		}
		refSoundSource.volume = vol;
		if (delaySec > 0f)
		{
			int num = Mathf.FloorToInt(44100f * delaySec);
			refSoundSource.Play((uint)num);
		}
		else
		{
			refSoundSource.Play();
		}
		return refSoundSource;
	}

	public Sound3DInstance GetInstanceForSound(AudioClip snd)
	{
		Sound3DInstance sound3DInstance = null;
		Sound3DInstance sound3DInstance2 = null;
		for (Sound3DInstance next = rootSoundInstance; next != null; next = next.next)
		{
			if (next.source == null)
			{
				sound3DInstance2 = next;
			}
			else if (next.source.clip == snd)
			{
				return next;
			}
			sound3DInstance = next;
		}
		if (sound3DInstance2 == null)
		{
			sound3DInstance2 = new Sound3DInstance();
			sound3DInstance2.next = null;
			sound3DInstance2.source = null;
			if (sound3DInstance != null)
			{
				sound3DInstance.next = sound3DInstance2;
			}
			else
			{
				rootSoundInstance = sound3DInstance2;
			}
		}
		sound3DInstance2.refCounter = 0;
		return sound3DInstance2;
	}

	public static void StopAll3DSounds()
	{
		if (!(_singleton != null))
		{
			return;
		}
		for (Sound3DInstance next = _singleton.rootSoundInstance; next != null; next = next.next)
		{
			if (next.source != null)
			{
				next.refCounter = 0;
				DestroySoundInstance(next, null);
			}
		}
	}
}
