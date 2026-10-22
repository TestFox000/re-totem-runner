using System;
using UnityEngine;

[Serializable]
public class EnemySpiderParams
{
	public float leftBound = -800f;

	public float turnDistance = 350f;

	public float attackDistance = 200f;

	public float vulnerabilityDistance = 100f;

	public float disappearTime = 10f;

	public float attackDamageDelay = 0.2f;

	public float attackAltDamageDelay = 0.2f;

	public float animationFade = 0.2f;

	public GameObject bombPrefab;

	public Quaternion bombRotation = new Quaternion(0f, 1.57f, 0f, 1f);

	public float bombPosZ;

	public float bombOffsetX = -200f;

	public float turnTimeout = 0.75f;

	public float prepareHorizontalSpeed = 350f;
}
