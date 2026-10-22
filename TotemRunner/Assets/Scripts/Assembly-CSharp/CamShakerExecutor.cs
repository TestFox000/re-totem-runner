public class CamShakerExecutor : EventExecutor
{
	public ECameraShakeType camShakeType = ECameraShakeType.death;

	public override void StartExecutor()
	{
		CameraShake.instance.LaunchShake(camShakeType, 1f);
		base.StartExecutor();
	}
}
