using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class HumanController : StateMachine {

	public static DestroyHuman[] destroyHumans;
	private float maxSpeed = 5.5f;
	private float minSpeed = 4.0f;

	private NavMeshAgent agent;
	private Rigidbody body;
	private Vector3 targetPoint;
	private Transform myTrans;
	private HumanTrafficControl traffic;
	private bool isInitialized = false;

	void Awake()
	{
		myTrans = transform;
		agent = GetComponent<NavMeshAgent>();
		body = GetComponent<Rigidbody>();

		agent.speed = Random.Range(minSpeed, maxSpeed);
	}

	void Start()
	{
		isInitialized = true;
		Init ();
	}

	void OnEnable()
	{
		if( isInitialized ) Init ();
	}

	void Init()
	{
		if( destroyHumans == null || destroyHumans[0] == null ) destroyHumans = FindObjectsOfType<DestroyHuman>();
		
		DestroyHuman dh;
		do {
			dh = destroyHumans[ Random.Range(0, destroyHumans.Length - 1) ];
		} while((dh.transform.position-myTrans.position).sqrMagnitude < 5000.0f);
		
		if( dh == null )
		{
			destroyHumans = null;
			return;
		}
		Transform target = dh.transform;
		targetPoint = target.position + 
			Quaternion.Euler( 0, Random.Range(0,360), 0 ) * Vector3.forward * target.localScale.x * 0.5f;
		
		ResumeAgent();
	}

	void OnTriggerEnter( Collider other )
	{
		CheckTraffic(other);
	}

	void OnTriggerStay( Collider other )
	{
		CheckTraffic(other);
	}

	private void CheckTraffic( Collider other )
	{
		if( other.tag == "HumanTrafficControl" && traffic == null )
		{
			if( Vector3.Dot(other.transform.forward, myTrans.forward) > 0.3f )
			{
				StopAgent ();
				traffic = other.GetComponent<HumanTrafficControl>();
				StopCoroutine("CheckHumanTrafficControl");
				StartCoroutine("CheckHumanTrafficControl");
			}
		}
	}

	private IEnumerator CheckHumanTrafficControl()
	{
		while( traffic.blocked ) yield return null;
		traffic = null;
		ResumeAgent ();
	}

	public void StopAgent()
	{
		if( agent.isOnNavMesh ) agent.Stop();
		body.velocity = Vector3.zero;
		body.useGravity = true;
	}

	public void ResumeAgent()
	{
		body.velocity = Vector3.zero;

		body.useGravity = false;
		body.freezeRotation = true;
		agent.enabled = true;

		body.MoveRotation(Quaternion.identity);

		if( agent.isOnNavMesh )
		{
			agent.Resume();
			if( !agent.hasPath )
				agent.SetDestination(targetPoint);
		}
	}

	void OnCollisionEnter( Collision other )
	{
		if( other.collider.tag == "Car" && !other.collider.GetComponent<CarController>().breakCar )
		{
			if( other.relativeVelocity.sqrMagnitude > 50.0f  )
				Kill ();
			else if( agent.isOnNavMesh )
			{
				agent.SetDestination(targetPoint);
				body.velocity = Vector3.zero;
			}
		}
		else
		{
			body.velocity = Vector3.zero;
		}
	}

	public void Kill()
	{
		agent.enabled = false;
		body.useGravity = true;
		body.freezeRotation = false;
	}
}
