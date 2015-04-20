using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour {

	public float speed;
	public float acc;
	public bool breakCar = false;
	public LayerMask collideMask;

	private Rigidbody body;
	private Transform myTrans;
	private float eyeDistance;
	public bool broken = false;
	public bool waitingTraffic = false;
	private TrafficControl currentTraffic;
	public bool waitingOtherCar = false;

	void Start()
	{
		body = GetComponent<Rigidbody>();
		myTrans = transform;
		eyeDistance = Random.Range(0.5f, 3.0f);
		broken = false;
		waitingTraffic = false;
	}

	void OnCollisionEnter( Collision other )
	{
		string tag = other.collider.tag;
		if( tag == "Car" || tag == "Player" /*|| tag == "Human"*/ )
			broken = true;
	}

	void OnTriggerEnter( Collider other )
	{
		if( other.tag == "TrafficControl" )
		{
			currentTraffic = other.GetComponent<TrafficControl>();
			float angle = Mathf.CeilToInt(myTrans.eulerAngles.y);
			int direction = (int)(angle/90.0f)%2;
			waitingTraffic = direction != currentTraffic.releasedDirection;
		}
	}

	void FixedUpdate()
	{ 
		if( waitingTraffic )
		{
			float angle = Mathf.CeilToInt(myTrans.eulerAngles.y);
			int direction = (int)(angle/90.0f)%2;
			waitingTraffic = direction != currentTraffic.releasedDirection;
		}

		// init velocity
		Vector3 vel = body.velocity;
		
		breakCar = waitingTraffic || broken;
		Vector3 center = body.position + myTrans.forward * 2.5f;
		if( Physics.Raycast( center, myTrans.forward, eyeDistance, collideMask ) ||
		    Physics.Raycast( center + myTrans.right, myTrans.forward, eyeDistance, collideMask ) || 
		    Physics.Raycast( center - myTrans.right, myTrans.forward, eyeDistance, collideMask ) )
		{
			breakCar = true;
			if( !waitingOtherCar )
			{
				speed = Mathf.Min(9.0f, speed - 0.5f);
				waitingOtherCar = true;
			}
		} else waitingOtherCar = false;


			
		// rotate inputs and limit velocity
		Vector3 input = breakCar ? Vector3.zero : myTrans.forward;
		// add acceleration
		input.x *= Mathf.Min(Mathf.Abs(vel.x) + acc, speed);
		input.z *= Mathf.Min(Mathf.Abs(vel.z) + acc, speed);
		
		// apply impulse
		Vector3 velChange = input - vel;
		Vector3 impulse = body.mass * velChange;
		impulse.y = 0;
		
		body.AddForceAtPosition(impulse, body.position, ForceMode.Impulse );
	}

	public void Kill()
	{
		this.enabled = false;
	}
}
