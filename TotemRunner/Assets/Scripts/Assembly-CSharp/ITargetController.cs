using UnityEngine;

public interface ITargetController
{
	Vector3 Position { get; }

	Rect GetCollisionRect();

	bool OnEnterToDangerZone(DangerObject objDanger, PlayerDeathType playerDeathType, DangerObject.eEnemyAttackStyle attackStyle);

	void OnEnterToVulnerabilityZone(DangerObject objDanger);

	void OnExitFromDangerZone(DangerObject objDanger);

	bool HaveCollider(Collider col);

	Transform FindBone(string boneName);

	void LockSkin(eLockSkin lockType);
}
