using System;

[Serializable]
public class EnemyBrainParams
{
	public float leftBound = -1000f;

	public float horizontalWaitSpeed = 150f;

	public float chaseDistance = -250f;

	public float catchDistance = 110f;

	public float damageRadius = 1000f;

	public float verticalOffset = 70f;

	public float verticalSpringiness = 2f;

	public float verticalSpringinessInManeuver = 1f;

	public float horizontalRelativeSpeed = 6.5f;

	public string colliderTieBone = "root/Hips_Joint";

	public float otherBrainPushRadius = 100f;

	public float teleportInDelay = 0.2f;

	public float teleportOutDelay = 0.6f;

	public float teleportModelFadeInTime;

	public float teleportModelFadeOutTime = 1.5f;

	public float teleportModelFadeTime = 0.3f;

	public float teleportTimeLength = 2f;
}
