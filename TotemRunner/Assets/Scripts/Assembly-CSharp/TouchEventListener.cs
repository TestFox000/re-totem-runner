using UnityEngine;

public interface TouchEventListener
{
	bool TouchStart(Vector2 pos);

	void TouchFinish(Vector2 posStart, Vector2 posEnd, float deltaTime);

	void TouchMoving(Vector2 posDelta, float deltaTime);

	int GetZOrder();

	bool TouchSlide(int slideNum, Vector2 deltaPos, float deltaTime);

	bool TouchClick(int clickNum);
}
