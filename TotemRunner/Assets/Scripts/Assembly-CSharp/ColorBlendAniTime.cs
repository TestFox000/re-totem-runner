using UnityEngine;

public class ColorBlendAniTime : BaseColorBlend
{
	public float preTime = 0.1f;

	public float postTime = 0.1f;

	public float blinkPeriod = 0.5f;

	private float blinkMul;

	private Animation refAnimation;

	private AnimationState refAniState;

	public override void Awake()
	{
		base.Awake();
		if (refAnimation == null)
		{
			refAnimation = base.GetComponent<Animation>();
		}
		if (blinkPeriod > 0f)
		{
			blinkMul = 1f / blinkPeriod;
		}
	}

	private void Update()
	{
		if (!refAnimation)
		{
			return;
		}
		if ((bool)refAniState && !refAniState.enabled)
		{
			refAniState = null;
		}
		if (refAniState == null)
		{
			foreach (AnimationState item in refAnimation)
			{
				if (item.enabled)
				{
					refAniState = item;
					break;
				}
			}
		}
		if (!(refAniState != null))
		{
			return;
		}
		if (blinkPeriod > 0f)
		{
			float num = refAniState.time * blinkMul;
			int num2 = Mathf.FloorToInt(num);
			num -= (float)num2;
			if ((num2 & 1) != 0)
			{
				base.BlendFactor = 1f - num;
			}
			else
			{
				base.BlendFactor = num;
			}
		}
		if ((bool)mat)
		{
			if (refAniState.time < preTime)
			{
				GUtils.SetMaterialAlpha(mat, Mathf.Lerp(0f, mat.color.a, refAniState.time / preTime));
			}
			else if (postTime > 0f && refAniState.time > refAniState.length - postTime)
			{
				GUtils.SetMaterialAlpha(mat, Mathf.Lerp(mat.color.a, 0f, (refAniState.time - refAniState.length + postTime) / postTime));
			}
		}
	}
}
