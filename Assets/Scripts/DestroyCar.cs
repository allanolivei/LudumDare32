using UnityEngine;
using System.Collections;

public class DestroyCar : MonoBehaviour {

	void OnTriggerEnter( Collider other )
	{
		if( other.tag == "Car" && !PlayerController.current.jumpComplete )
		{
			SpawnCar.Recycle( other.GetComponent<CarController>() );
		}
	}

}
