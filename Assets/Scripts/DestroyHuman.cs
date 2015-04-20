using UnityEngine;
using System.Collections;

public class DestroyHuman : MonoBehaviour {

	public new Transform transform;

	void Awake()
	{
		transform = GetComponent<Transform>();
	}

	void OnTriggerEnter( Collider other )
	{
		if( other.tag == "Human" && !PlayerController.current.jumpComplete )
		{
			SpawnHuman.Recycle( other.GetComponent<HumanController>() );
		}
	}

}
