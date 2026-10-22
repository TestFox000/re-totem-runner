using UnityEngine;

public class PlayerSoundList : MonoBehaviour
{
	public Transform soundFolder;

	public SoundDescribe[] soundList;

	public static PlayerSoundList instance;

	private Transform locTransform;

	public static string GetSoundNameFromId(ePlayerSoundID id)
	{
		string result = "unknown";
		switch (id)
		{
		case ePlayerSoundID.grassGrow:
			result = "grass drop";
			break;
		case ePlayerSoundID.manStep:
			result = "man step";
			break;
		case ePlayerSoundID.manStep2:
			result = "man step 2";
			break;
		case ePlayerSoundID.boarStep:
			result = "boar step";
			break;
		case ePlayerSoundID.boarStep2:
			result = "boar step 2";
			break;
		case ePlayerSoundID.boarBreath:
			result = "boar breath";
			break;
		case ePlayerSoundID.eagleWings:
			result = "eagle wings";
			break;
		case ePlayerSoundID.boarAttack:
			result = "boar attack";
			break;
		case ePlayerSoundID.skinSwitch:
			result = "skin switch";
			break;
		case ePlayerSoundID.skinSwitchToMan:
			result = "skin switch man";
			break;
		case ePlayerSoundID.skinSwitchToEagle:
			result = "skin switch eagle";
			break;
		case ePlayerSoundID.skinSwitchToBoar:
			result = "skin switch boar";
			break;
		case ePlayerSoundID.ground:
			result = "ground";
			break;
		case ePlayerSoundID.manPoison:
			result = "man poison";
			break;
		case ePlayerSoundID.vasyaStart:
			result = "vasya start";
			break;
		case ePlayerSoundID.vasyaFly:
			result = "vasya fly";
			break;
		case ePlayerSoundID.manDeath:
			result = "man death";
			break;
		case ePlayerSoundID.eagleDeath:
			result = "eagle death";
			break;
		case ePlayerSoundID.boarDeath:
			result = "boar death";
			break;
		}
		return result;
	}

	public static ePlayerSoundID GetSoundIdFromName(string sndName)
	{
		return sndName switch
		{
			"grass drop" => ePlayerSoundID.grassGrow, 
			"man step" => ePlayerSoundID.manStep, 
			"man step 2" => ePlayerSoundID.manStep2, 
			"boar step" => ePlayerSoundID.boarStep, 
			"boar step 2" => ePlayerSoundID.boarStep2, 
			"boar breath" => ePlayerSoundID.boarBreath, 
			"eagle wings" => ePlayerSoundID.eagleWings, 
			"boar attack" => ePlayerSoundID.boarAttack, 
			"skin switch" => ePlayerSoundID.skinSwitch, 
			"skin switch man" => ePlayerSoundID.skinSwitchToMan, 
			"skin switch eagle" => ePlayerSoundID.skinSwitchToEagle, 
			"skin switch boar" => ePlayerSoundID.skinSwitchToBoar, 
			"ground" => ePlayerSoundID.ground, 
			"man poison" => ePlayerSoundID.manPoison, 
			"vasya start" => ePlayerSoundID.vasyaStart, 
			"vasya fly" => ePlayerSoundID.vasyaFly, 
			"man death" => ePlayerSoundID.manDeath, 
			"eagle death" => ePlayerSoundID.eagleDeath, 
			"boar death" => ePlayerSoundID.boarDeath, 
			_ => ePlayerSoundID.none, 
		};
	}

	private void Awake()
	{
		instance = this;
		locTransform = base.transform;
		// Unity 3.5 allowed active audio children under this inactive empty
		// container. Modern Unity inherits inactivity from the parent instead.
		if (soundFolder != null)
		{
			soundFolder.gameObject.SetActive(true);
		}
	}

	public void Start3DSoundOnce(ePlayerSoundID id, Vector3 pos)
	{
		AudioSource.PlayClipAtPoint(soundList[(int)id].sound, pos, soundList[(int)id].soundVolume * GameSettings.sound.soundVolume);
	}

	public void PauseSound(ePlayerSoundID id, float scaleVolume, bool isPause)
	{
		if (id < ePlayerSoundID.grassGrow || (int)id >= soundList.Length)
		{
			Debug.LogWarning(string.Concat("Sound ", id, " is not contained into player list"));
			return;
		}
		if (!soundList[(int)id].soundSource)
		{
			GameObject gameObject = new GameObject(soundList[(int)id].id);
			gameObject.transform.parent = soundFolder;
			gameObject.transform.position = locTransform.position;
			soundList[(int)id].soundSource = gameObject.AddComponent<AudioSource>();
			soundList[(int)id].soundSource.clip = soundList[(int)id].sound;
			soundList[(int)id].soundSource.loop = soundList[(int)id].loop;
			soundList[(int)id].soundSource.maxDistance = 2000f;
			soundList[(int)id].soundSource.minDistance = 800f;
			soundList[(int)id].soundSource.dopplerLevel = 0f;
		}
		if ((bool)soundList[(int)id].soundSource)
		{
			soundList[(int)id].soundSource.volume = soundList[(int)id].soundVolume * GameSettings.sound.soundVolume * scaleVolume;
			if (isPause)
			{
				soundList[(int)id].soundSource.Pause();
			}
			else
			{
				soundList[(int)id].soundSource.Play();
			}
		}
	}

	public void StartSound(ePlayerSoundID id, float scaleVolume)
	{
		if (id < ePlayerSoundID.grassGrow || (int)id >= soundList.Length)
		{
			Debug.LogWarning(string.Concat("Sound ", id, " is not contained into player list"));
			return;
		}
		if (soundList[(int)id].soundSource == null)
		{
			GameObject gameObject = new GameObject(soundList[(int)id].id);
			gameObject.transform.parent = soundFolder;
			gameObject.transform.position = locTransform.position;
			soundList[(int)id].soundSource = gameObject.AddComponent<AudioSource>();
			soundList[(int)id].soundSource.clip = soundList[(int)id].sound;
			soundList[(int)id].soundSource.loop = soundList[(int)id].loop;
			soundList[(int)id].soundSource.maxDistance = 2000f;
			soundList[(int)id].soundSource.minDistance = 800f;
			soundList[(int)id].soundSource.dopplerLevel = 0f;
		}
		else
		{
			soundList[(int)id].soundSource.Stop();
		}
		soundList[(int)id].soundSource.volume = soundList[(int)id].soundVolume * GameSettings.sound.soundVolume * scaleVolume;
		soundList[(int)id].soundSource.Play((ulong)Mathf.FloorToInt(soundList[(int)id].delayInSec * 44100f));
	}

	public void StopSound(ePlayerSoundID id)
	{
		if (id >= ePlayerSoundID.grassGrow && (int)id < soundList.Length && (bool)soundList[(int)id].soundSource)
		{
			soundList[(int)id].soundSource.Stop();
			Object.Destroy(soundList[(int)id].soundSource.gameObject);
			soundList[(int)id].soundSource = null;
		}
	}

	public static void StopAllSounds()
	{
		if ((bool)instance)
		{
			for (int i = 0; i < 19; i++)
			{
				instance.StopSound((ePlayerSoundID)i);
			}
		}
	}
}
