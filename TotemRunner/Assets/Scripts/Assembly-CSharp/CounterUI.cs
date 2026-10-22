using UnityEngine;

public class CounterUI : MonoBehaviour
{
	public int startCounter;

	private int curCounter;

	private int curShowingCounter;

	private int prevShowingCounter;

	public string prefixString = "Score: ";

	public string postfixString = string.Empty;

	public TextMesh refText;

	public float pulseTime = 0.7f;

	public float normalScale = 1f;

	public float pulsedScale = 1.2f;

	private float pulseTimeMul = 1.43f;

	private float scaleAddition;

	private float pulseTimeLeft = -1f;

	private int addVal;

	private int begVal;

	private Transform refTransform;

	private void Awake()
	{
		if (!refText)
		{
			refText = GetComponent<TextMesh>();
		}
		refTransform = base.transform;
		curCounter = startCounter;
		curShowingCounter = startCounter;
		pulseTimeMul = 1f / pulseTime;
		scaleAddition = pulsedScale - normalScale;
		refTransform.localScale = Vector3.one * normalScale;
		UpdateCounterView();
	}

	private void Update()
	{
		if (pulseTimeLeft >= 0f)
		{
			pulseTimeLeft -= Time.deltaTime;
			if (pulseTimeLeft <= 0f)
			{
				refTransform.localScale = Vector3.one * normalScale;
				pulseTimeLeft = -1f;
				curShowingCounter = curCounter;
			}
			else
			{
				float num = Mathf.Clamp01(pulseTimeLeft * pulseTimeMul);
				refTransform.localScale = Vector3.one * (normalScale + scaleAddition * num);
				curShowingCounter = begVal + Mathf.FloorToInt(num * (float)addVal);
			}
			if (prevShowingCounter != curShowingCounter)
			{
				prevShowingCounter = curShowingCounter;
				UpdateCounterView();
			}
		}
	}

	private void UpdateCounterView()
	{
		if ((bool)refText)
		{
			refText.text = prefixString + curShowingCounter + postfixString;
		}
	}

	public void SetCounter(int count)
	{
		if (curCounter != count)
		{
			curCounter = count;
			if (curCounter > curShowingCounter)
			{
				curShowingCounter++;
			}
			else if (curCounter < curShowingCounter)
			{
				curShowingCounter--;
			}
			addVal = curCounter - curShowingCounter;
			begVal = curShowingCounter;
			pulseTimeLeft = pulseTime;
		}
	}

	public void AddCounter(int addict)
	{
		SetCounter(curCounter + addict);
	}

	public int GetCounter()
	{
		return curCounter;
	}
}
