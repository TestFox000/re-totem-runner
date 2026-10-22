using UnityEngine;

public class AchievementTrigger : MonoBehaviour
{
	public AchievementSystem.eAchievementID achievementId;

	private void OnTriggerEnter(Collider other)
	{
		AchievementSystem.Reach(achievementId);
	}
}
