using UnityEngine;

public class AnimationSampler : MonoBehaviour
{
	public string animationName;

	public float animationTime;

	private void Awake()
	{
		DoSampleAnimation();
	}

	private void DoSampleAnimation()
	{
		Animation animation = base.GetComponent<Animation>();
		animation.Play(animationName);
		animation[animationName].time = animationTime;
		animation.Sample();
		animation.Stop();
	}
}
