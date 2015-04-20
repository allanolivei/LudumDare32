using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

	public static CameraController current;

	//config
	private float smoothGround = 2.0f;
	private float smoothAirRot = 0.5f;
	private float offsetGroundForward = 2.8f;
	private float offsetAirUp = 1.2f;

	//aux
	const float deg = 82.0f;
	const float distance = 13.1f;

	//utils	
	private float dynamicAirSmooth = 0.0f;
	private bool inAnimation = false;
	private bool animationComplete = false;

	//cache
	private PlayerController playerC;
	private Camera cam;
	private Transform playerT;
	private Transform myTrans;


	void Awake()
	{
		current = this;
	}

	void Start()
	{
		GameObject target = GameObject.FindGameObjectWithTag("Player");
		playerC = target.GetComponent<PlayerController>();
		playerT = target.GetComponent<Transform>();
		myTrans = transform;
		dynamicAirSmooth = 0.0f;
		cam = GetComponent<Camera>();
	}

	void LateUpdate()
	{
		if( inAnimation ) return;

		PlayerController.STATES state = (PlayerController.STATES)playerC.currentState;

		switch( state )
		{
		case PlayerController.STATES.GROUND:
			Quaternion targetR = Quaternion.LookRotation( Quaternion.AngleAxis(deg, playerT.right) * playerT.forward );
			myTrans.rotation = Quaternion.Lerp( myTrans.rotation, targetR, smoothGround * Time.deltaTime );

			Vector3 offset =  playerT.forward * offsetGroundForward;
			Vector3 targetP = playerT.position + offset - myTrans.rotation * Vector3.forward * distance;
			myTrans.position = Vector3.Lerp( myTrans.position, targetP, Time.deltaTime * smoothGround);
			dynamicAirSmooth = 0.0f;
			animationComplete = false;
			break;
		case PlayerController.STATES.FINISH_AIR:
			if( !animationComplete )
			{
				inAnimation = true;
				StartCoroutine("OrbitCamera");
			}
			break;
		case PlayerController.STATES.DEAD:
			break;
		default:
			if( Time.time - playerC.timeEnterState > 4.0f ) smoothAirRot = 10.0f;
			Quaternion tR = Quaternion.AngleAxis( playerC.sideOfBuilding*90.0f, Vector3.up ) * Quaternion.AngleAxis( 90.0f, Vector3.right );
			myTrans.rotation = Quaternion.Lerp( myTrans.rotation, tR, smoothAirRot * Time.deltaTime );
			Vector3 off =  tR * Vector3.up * offsetAirUp;
			Vector3 tP = playerT.position + off - myTrans.rotation * Vector3.forward * distance;
			dynamicAirSmooth = Mathf.Min(1.0f, dynamicAirSmooth + 0.34f * Time.deltaTime);
			myTrans.position = Vector3.Lerp(myTrans.position, tP, dynamicAirSmooth * dynamicAirSmooth);

			float targetFieldOfView = playerC.inSlowMotion ? 20.0f : Mathf.Lerp( 30, 40, playerC.inclination );
			cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFieldOfView, Time.deltaTime * 1.5f);
			break;
		}
	}

	private IEnumerator TranslateInFocus( Vector3 targetP )
	{
		float initTime = Time.unscaledTime;
		float duration = 3.0f;
		Vector3 initPosition = myTrans.position;

		while(true)
		{
			float percent = Mathf.Min(1, (Time.unscaledTime - initTime)/duration);
			myTrans.position = Vector3.Lerp( initPosition, targetP, percent );
			myTrans.LookAt( playerT.position );
			if( percent == 1 ) break;
			yield return null;
		}
	}

	private IEnumerator OrbitCamera()
	{
		inAnimation = true;
		float duration = 5.0f;
		float initTime = Time.unscaledTime;
		float endTSZero = 0.1f;
		float initTSOne = 0.8f;
		float initFieldOfView = cam.fieldOfView;
		float distance = (playerT.position - myTrans.position).magnitude;


		Quaternion tR = Quaternion.AngleAxis( playerC.sideOfBuilding*90.0f, Vector3.up ) * Quaternion.AngleAxis( 90.0f, Vector3.right );
		Vector3 off =  tR * Vector3.up * offsetAirUp;


		
		while( true )
		{
			//calculate duration(0-1)  
			float percent = Mathf.Min( 1, (Time.unscaledTime - initTime)/duration );

			//rotation animation
			//myTrans.rotation = Quaternion.AngleAxis( Mathf.Lerp(playerC.sideOfBuilding*90, playerC.sideOfBuilding*90 + 360 + 90 + 45, percent), Vector3.up ) * 
			//	Quaternion.AngleAxis( (percent < 0.5f ) ? Mathfx.Sinerp(90, 0, percent * 2) : Mathfx.Coserp(0, 60, (percent-0.5f) * 2), Vector3.right );
			float orbitAngle = Mathfx.Sinerp(playerC.sideOfBuilding*90, playerC.sideOfBuilding*90 + 180 + 45, percent);
			Quaternion forOrbit = Quaternion.AngleAxis( orbitAngle, Vector3.up );

			float heightAngle = (percent < 0.5f ) ? 
				Mathfx.Hermite(90, 35, LertEndModify(percent, 0.5f)) : 
				Mathfx.Hermite(35, 60, LerpInitModify(percent, 0.5f));
			Quaternion forHeight = Quaternion.AngleAxis( heightAngle, Vector3.right );

			myTrans.rotation = forOrbit * forHeight;

			//position animation
			GoToPosition( playerT.position + off - myTrans.forward * (distance + percent * 15) );

			//TimeScale Animation
			if( percent < endTSZero )
				Time.timeScale = Mathfx.Hermite( 1.0f, 0.0f, LertEndModify(percent, endTSZero) );
			else if( percent > initTSOne )
				Time.timeScale = Mathfx.Coserp( 0.0f, 1.0f, LerpInitModify(percent, initTSOne) );

			cam.fieldOfView = Mathfx.Hermite(initFieldOfView, 60, percent);

			//check finish animation
			if( percent == 1 ) break;
			yield return null;
		}
		
		animationComplete = true;
		
		inAnimation = false;
	}

	private float LertEndModify( float percent, float end )
	{
		return percent * (1/end);
	}

	private float LerpInitModify( float percent, float init)
	{
		return (percent - init) * (1/(1-init));
	}

	private void GoToPosition( Vector3 targetPosition )
	{
		Vector3 playerToTarget = (targetPosition-playerT.position);
		float paramDistance = playerToTarget.magnitude;
		Vector3 direction = playerToTarget/paramDistance;
		RaycastHit hit;
		if ( Physics.Raycast( playerT.position, direction, out hit, paramDistance, playerC.groundLayer ) )
		{
			targetPosition = hit.point;
		}
		else
		{
			float currentDistance = (myTrans.position - playerT.position).magnitude;
			targetPosition = playerT.position + direction * Mathf.Lerp( currentDistance, paramDistance, Time.unscaledDeltaTime * 1.3f );
		}
		myTrans.position = targetPosition;
	}
}
