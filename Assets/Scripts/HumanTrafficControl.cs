using UnityEngine;
using System.Collections;

public class HumanTrafficControl : MonoBehaviour {

	public TrafficControl trafficControl;
	public int crossingDirection;
	public bool blocked = true;

	private Color debugBlockColor = new Color(1,0,0,0.5f);
	private Color debugUnBlockColor = new Color(0,1,0,0.5f);

	void Update()
	{
		float timeToFinish = trafficControl.changeInTime - Time.time;
		float deltaTime = Time.time - (trafficControl.changeInTime-trafficControl.duration);
		blocked = trafficControl.releasedDirection == crossingDirection || trafficControl.releasedDirection == -1
		|| timeToFinish  < 3.0f;// || deltaTime < 2.0f;
#if UNITY_EDITOR
		GetComponent<Renderer>().material.color = blocked ? debugBlockColor : debugUnBlockColor;
#endif
	}

}
