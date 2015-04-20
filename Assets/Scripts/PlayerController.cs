using UnityEngine;
using System.Collections;

public struct Highscore
{
	public int humans;
	public int cars;
	public float selfDamage;
	public float humanDamageTotal;
	public float carDamageTotal;
	public float impact;

	public float score
	{
		get{ return impact + selfDamage + humanDamageTotal + carDamageTotal; }
	}
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : StateMachine {

	static public PlayerController current;

	public enum STATES
	{
		GROUND,
		JUMP,
		NORMAL_AIR,
		DEAD,
		FINISH_AIR
	};

	public float acc = 2.0f;
	private float airAcc = 10.0f;
	public float maxSpeed = 2.0f;
	public float jumpForce = 6.0f;
	public LayerMask groundLayer;

	[HideInInspector, System.NonSerialized]
	public Rigidbody body;
	[HideInInspector, System.NonSerialized]
	public float sideOfBuilding = 0;
	[HideInInspector, System.NonSerialized]
	public float inclination = 0;
	private bool _inSlowMotion = false;
	private float borderDistance;
	private Quaternion initRotation;
	public Highscore highScore;
	public float multiplyImpact = 1.0f;
	private bool _jumpComplete = false;

	void Awake()
	{
		current = this;
		body = GetComponent<Rigidbody>();
	}

	void Start()
	{
		borderDistance = new Vector3(9.5f,0,9.5f).magnitude;

		AddState( STATES.GROUND, null, StateGroundUpdate, null);
		AddState( STATES.JUMP, StateJumpEnter, StateJumpUpdate, null);
		AddState( STATES.NORMAL_AIR, null, StateAirUpdate, null);
		AddState( STATES.FINISH_AIR, StateFinishAirEnter, StateDeadUpdate, null);
		AddState( STATES.DEAD, StateDeadEnter, StateDeadUpdate, null);

		running = true;
		this.updateLocal = UpdateLocal.FixedUpdate;
		currentState = STATES.GROUND;
	}

	void OnTriggerEnter( Collider other )
	{
		if( other.tag == "SlowMotion" )
		{
			other.SendMessage("PickUp");
			if ( _inSlowMotion ) StopCoroutine( "SlowMotion" );
			StartCoroutine( "SlowMotion" );
		}
		else if( other.tag == "Double" )
		{
			other.SendMessage("PickUp");
			multiplyImpact *= 2.0f;
		}
		else if( other.tag == "Teleportation" )
		{
			other.GetComponent<Teleportation>().Teleport( transform );

			//register side of building
			Vector3 p = body.position;
			p.y = 0;
			p = p.normalized;
			float angle = Vector3.Angle( Vector3.forward, p ) * Mathf.Sign(Vector3.Dot(Vector3.right, p) );
			sideOfBuilding = Mathf.FloorToInt((angle+45)/90.0f);
		}
	}

	public bool jumpComplete{ get{ return _jumpComplete; } }

	public bool inSlowMotion{ get{ return _inSlowMotion; } }

	private IEnumerator SlowMotion()
	{
		_inSlowMotion = true;
		float initTimeScale = Time.timeScale;
		float initTime = Time.unscaledTime;
		float duration = 2.0f;
		while( true )
		{
			float percent = Mathf.Min(1, (Time.unscaledTime - initTime)/duration);
			//Time.timeScale = Mathfx.Hermite(initTimeScale, 0.4f, percent);
			if( percent == 1.0f ) break;
			yield return null;
		}

		//initTime = Time.unscaledTime;
		//while( Time.unscaledTime-initTime < 1.0f  ) yield return null;

		initTime = Time.unscaledTime;
		duration = 2.0f;
		while( true )
		{
			float percent = Mathf.Min(1, (Time.unscaledTime - initTime)/duration);
			//Time.timeScale = Mathfx.Hermite(0.2f, 1.0f, percent);
			if( percent == 1.0f ) break;
			yield return null;
		}

		_inSlowMotion = false;
	}

	void OnCollisionEnter( Collision col )
	{
		if( _jumpComplete ) return;

		float impact = col.relativeVelocity.magnitude * multiplyImpact;
		if( impact > 10.0f ) 
		{
			Vector3 explosionPoint = col.contacts[0].point;
			float explosionRadius = 5.0f + impact * 0.1f;
			float explosionForce = impact * 20.0f;
			highScore.impact += impact;
			highScore.selfDamage += explosionForce * 0.1f;
			foreach( Collider c in Physics.OverlapSphere(explosionPoint, explosionRadius) )
			{
				if( c.name != this.name && c.attachedRigidbody )
				{
					if( c.tag == "Human" )
					{
						highScore.humans += 1;
						highScore.humanDamageTotal += Mathf.Abs((Mathf.Min (1, explosionRadius - Vector3.Distance( c.bounds.center, body.position )) * impact) * 1.0f);
						c.GetComponent<HumanController>().Kill();
					}
					else if ( c.tag == "Car" )
					{
						highScore.cars += 1;
						highScore.carDamageTotal += Mathf.Abs((Mathf.Min(1,explosionRadius - Vector3.Distance( c.bounds.center, body.position )) * impact) * 0.5f);
						c.GetComponent<CarController>().Kill();
					}
					//c.SendMessage("Kill", SendMessageOptions.DontRequireReceiver);
					c.attachedRigidbody.AddExplosionForce( explosionForce, explosionPoint, explosionRadius ); 
				}
			}
			currentState = STATES.DEAD;
		}

		if( transform.position.y < 4.0f )
			Invoke("FinishJump", 2.0f);
	}

	private void FinishJump()
	{
		_jumpComplete = true;
	}


	private void StateGroundUpdate ()
	{
		//register side of building
		Vector3 p = body.position;
		p.y = 0;
		p = p.normalized;
		float angle = Vector3.Angle( Vector3.forward, p ) * Mathf.Sign(Vector3.Dot(Vector3.right, p) );
		sideOfBuilding = Mathf.FloorToInt((angle+45)/90.0f);

		// get inputs
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		//jumping
		if( v > 0 || Input.GetButton("Jump") ) 
		{
			currentState = STATES.JUMP;
			return;
		}
		else v = 0;
		
		// init velocity
		Vector3 vel = body.velocity;

		// input relative camera
		//Vector3 calc = (camT.position - body.position);
		//calc.y = 0;
		Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);
		
		// rotate inputs and limit velocity
		Vector3 input = rot * new Vector3(h, 0, v).normalized;

		// check border
		if( !Physics.Raycast( body.position + input * 0.5f, Vector3.down, 10.0f, groundLayer ) )
			RotateInBorder( input );
		
		// add acceleration
		input.x *= Mathf.Min(Mathf.Abs(vel.x) + acc, maxSpeed);
		input.z *= Mathf.Min(Mathf.Abs(vel.z) + acc, maxSpeed);
		
		// apply impulse
		Vector3 velChange = input - vel;
		Vector3 impulse = body.mass * velChange;
		impulse.y = 0;
		
		body.AddForceAtPosition(impulse, body.position, ForceMode.Impulse );
	}

	private void StateJumpEnter()
	{
		body.AddForce( (transform.forward + transform.up).normalized * jumpForce, ForceMode.Impulse );

		inclination = 0.0f;

		Quaternion targetR = Quaternion.LookRotation(Vector3.down, transform.forward);
		StartCoroutine( RotateCharacter(targetR, 0.7f, 0.1f) );
	}

	private void StateJumpUpdate ()
	{
		currentState = STATES.NORMAL_AIR;
	}

	private void StateAirUpdate ()
	{
		// get inputs
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		if( Input.GetButton("Jump") )
		{
			inclination = Mathf.Min(1.0f, inclination + 1.3f * Time.deltaTime);
		}
		else
		{
			inclination = Mathf.Max(0.0f, inclination - 1.3f * Time.deltaTime);
		}
		body.AddForce(Vector3.up * (0.5f-inclination) * 10.0f, ForceMode.Acceleration);
		body.MoveRotation( Quaternion.AngleAxis( sideOfBuilding*90.0f, Vector3.up ) * 
		                   Quaternion.AngleAxis( 90.0f + 80.0f * (inclination*inclination), Vector3.right ) );

		// init velocity
		Vector3 vel = body.velocity;
		
		// input relative camera
		Quaternion rot = Quaternion.AngleAxis( sideOfBuilding*90.0f, Vector3.up );//Quaternion.LookRotation(transform.up, Vector3.up);
		
		// rotate inputs and limit velocity
		Vector3 input = rot * new Vector3(h, 0, v).normalized;
		
		// add acceleration
		input.x *= Mathf.Min(Mathf.Abs(vel.x) + airAcc, maxSpeed * 5);
		input.z *= Mathf.Min(Mathf.Abs(vel.z) + airAcc, maxSpeed * 5);
		
		// apply impulse
		Vector3 velChange = input - vel;
		Vector3 impulse = body.mass * velChange;
		if( !inSlowMotion )
			impulse.y = 0;
		
		body.AddForceAtPosition(impulse, body.position, ForceMode.Acceleration );

		RaycastHit hit;
		if( Physics.Raycast(body.position - Vector3.up * 1.0f, -Vector3.up, out hit, 14.0f ) && !hit.collider.isTrigger )
		{
			currentState = STATES.FINISH_AIR;
		}
	}

	private void StateFinishAirEnter()
	{
	}

	private void StateDeadEnter()
	{
		body.freezeRotation = false;
	}

	private void StateDeadUpdate()
	{

	}

	private void RotateInBorder( Vector3 inputDirection )
	{
		Vector3 pos = body.position;
		pos.y = 0;
		Vector3 targetP = pos.normalized * borderDistance + Vector3.up * body.position.y;
		Quaternion targetR = Quaternion.LookRotation(inputDirection, Vector3.up);
		StartCoroutine( TranslateAndRotate(targetP, targetR) );
	}

	private IEnumerator TranslateAndRotate( Vector3 targetP, Quaternion targetR )
	{
		running = false;
		
		float initTime = Time.time;
		Quaternion initRotation = body.rotation;
		Vector3 initPosition = body.position;
		float duration = 1.0f;
		
		while( true )
		{
			float percent = Mathf.Min(1, (Time.time - initTime)/ duration);
			body.MoveRotation( Quaternion.Slerp( initRotation, targetR, percent ) );
			body.MovePosition( Vector3.Lerp( initPosition, targetP, percent ) );
			if( percent == 1 ) break;
			yield return null;
		}
		
		running = true;
	}
	
	private IEnumerator RotateCharacter( Quaternion targetR, float duration=1.0f, float delay=0.0f )
	{
		running = false;

		yield return new WaitForSeconds(delay);

		float initTime = Time.time;
		Quaternion initRotation = body.rotation;

		while( true )
		{
			float percent = Mathf.Min(1, (Time.time - initTime)/ duration);
			body.MoveRotation( Quaternion.Slerp( initRotation, targetR, percent ) );
			if( percent == 1 ) break;
			yield return null;
		}

		running = true;
	}

	private IEnumerator TranslateCharacter( Vector3 targetP )
	{
		running = false;
		
		float initTime = Time.time;
		Vector3 initPosition = body.position;
		float duration = 1.0f;
		
		while( true )
		{
			float percent = Mathf.Min(1, (Time.time - initTime)/ duration);
			body.MovePosition( Vector3.Lerp( initPosition, targetP, percent ) );
			if( percent == 1 ) break;
			yield return null;
		}
		
		running = true;
	}

	void OnDrawGizmos()
	{
	}

}
