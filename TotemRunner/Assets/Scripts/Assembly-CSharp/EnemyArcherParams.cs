using System;
using UnityEngine;

[Serializable]
public class EnemyArcherParams
{
	public float leftBound = -620f;

	public float shootingDelay = 0.1f;

	public float waitTime = 1f;

	public float attackDistance = 300f;

	public float aimingDelay = 0.5f;

	public float aimingTime = 5f;

	public float vulnerabilityDistance = 200f;

	public float arrowPower = 4000f;

	public Mesh arrowMesh;

	public Material arrowMaterial;

	public string jointBodyName = "Root/Hips_Joint/Spine";

	public string jointArrowName = "Root/Hips_Joint/Spine/Arm_Right/Elbow_Right/Hand_Right/Bow/Arrow";
}
