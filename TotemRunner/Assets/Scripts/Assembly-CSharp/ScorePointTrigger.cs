using UnityEngine;

public class ScorePointTrigger : MonoBehaviour
{
	public int score;

	public Vector3 showPointOffset = Vector3.zero;

	private void OnTriggerEnter(Collider other)
	{
		GameStatistic.instance.AddItem(GameStatistic.eStatisticItem.addScore, score);
		LiftupMessage.ShowMessage("+" + score + " sp", base.transform.TransformPoint(showPointOffset));
	}
}
