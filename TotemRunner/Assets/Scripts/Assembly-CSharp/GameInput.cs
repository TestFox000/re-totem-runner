using UnityEngine;

public class GameInput : MonoBehaviour
{
	private enum eInputState
	{
		inactive = 0,
		activated = 1,
		active = 2,
		inactivated = 3,
		hide = 4
	}

	public static eInputStyle inputStyle;

	private eInputState[] inputStates = new eInputState[6];

	private float[] inputValues = new float[6];

	private static GameInput _singleton;

	public static GameInput instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = Object.FindObjectOfType(typeof(GameInput)) as GameInput;
			}
			return _singleton;
		}
	}

	private void Awake()
	{
		_singleton = this;
		if (inputStyle == eInputStyle.eisUndefined)
		{
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
			{
				inputStyle = eInputStyle.eisKeyboard;
			}
			else
			{
				inputStyle = eInputStyle.eisAcceleration;
			}
		}
	}

	public void HideControl(eGameInputButton id, bool hideFlag)
	{
		inputStates[(int)id] = (hideFlag ? eInputState.hide : eInputState.inactive);
		GuiGameHud.instance.HideButton(id, hideFlag);
	}

	public static bool IsHiddenButton(eGameInputButton id)
	{
		return instance.inputStates[(int)id] == eInputState.hide;
	}

	public void DeactivateButton(eGameInputButton id)
	{
		eInputState eInputState2 = inputStates[(int)id];
		if (eInputState2 == eInputState.activated || eInputState2 == eInputState.active)
		{
			inputStates[(int)id] = eInputState.inactivated;
			inputValues[(int)id] = 0f;
		}
	}

	private void ActivateButton(eGameInputButton id)
	{
		switch (inputStates[(int)id])
		{
		case eInputState.inactive:
		case eInputState.inactivated:
			inputStates[(int)id] = eInputState.activated;
			inputValues[(int)id] = 0f;
			break;
		case eInputState.activated:
		case eInputState.active:
			break;
		}
	}

	private void LateUpdate()
	{
		eInputStyle eInputStyle2 = inputStyle;
		if (eInputStyle2 == eInputStyle.eisKeyboard)
		{
			for (int i = 0; i < inputStates.Length; i++)
			{
				if (inputStates[i] == eInputState.activated)
				{
					inputStates[i] = eInputState.active;
				}
				else if (inputStates[i] == eInputState.inactivated)
				{
					inputStates[i] = eInputState.inactive;
				}
			}
			if (Input.GetButton("Fire2"))
			{
				ActivateButton(eGameInputButton.skinEagle);
			}
			else
			{
				DeactivateButton(eGameInputButton.skinEagle);
			}
			if (Input.GetButton("Fire3"))
			{
				ActivateButton(eGameInputButton.skinBoar);
			}
			else
			{
				DeactivateButton(eGameInputButton.skinBoar);
			}
			inputStates[0] = (Input.GetButton("Vertical") ? eInputState.active : eInputState.inactive);
			inputValues[0] = Input.GetAxis("Vertical");
			if (inputStates[5] != eInputState.hide)
			{
				inputStates[5] = (GuiGameHud.GetButtonState(eGameInputButton.powerUp) ? eInputState.active : eInputState.inactive);
			}
			return;
		}
		for (int i = 0; i < inputStates.Length; i++)
		{
			if (inputStates[i] != eInputState.hide)
			{
				inputStates[i] = (GuiGameHud.GetButtonState((eGameInputButton)i) ? eInputState.active : eInputState.inactive);
			}
		}
		inputValues[0] = GuiGameHud.GetJoystickValue(eGameInputButton.flyVertical);
	}

	public static bool GetButton(eGameInputButton id)
	{
		bool result = false;
		eInputState eInputState2 = _singleton.inputStates[(int)id];
		if (eInputState2 == eInputState.activated || eInputState2 == eInputState.active)
		{
			result = true;
		}
		return result;
	}

	public static float GetButtonValue(eGameInputButton id)
	{
		return _singleton.inputValues[(int)id];
	}

	public static bool IsBackPressed()
	{
		return PluginUtils.GetDeviceKeyDown() == PluginUtils.eDeviceKeyDown.keyBack;
	}

	public static bool IsMenuPressed()
	{
		return PluginUtils.GetDeviceKeyDown() == PluginUtils.eDeviceKeyDown.keyMenu;
	}
}
