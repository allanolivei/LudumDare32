using UnityEngine;
using System.Collections;

public class TrafficControl : MonoBehaviour {

	public int releasedDirection = 0;
	public float changeInTime = 0.0f;

	[HideInInspector, System.NonSerialized]
	public float duration = 5.0f;
	public float alertDuration = 2.8f;
	private int _released = 0;

	private Color debugOneDirection = new Color(1.0f,1.0f,0,0.5f);
	private Color debugZeroDirection = new Color(0,1.0f,1.0f,0.5f);

	IEnumerator Start()
	{
		while(true)
		{
			// Alert Duration - yellow
			releasedDirection = -1;//yellow
			yield return new WaitForSeconds( alertDuration );


			//Change direction
			_released = (_released+1)%2;

			releasedDirection = _released;//one or zer, red or green
			changeInTime = Time.time+duration+alertDuration;
#if UNITY_EDITOR
			GetComponent<Renderer>().material.color = releasedDirection == 1 ? debugOneDirection : debugZeroDirection;
#endif

			//duration
			yield return new WaitForSeconds( duration );
		}
	}
}
