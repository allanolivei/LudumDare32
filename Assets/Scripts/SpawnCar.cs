using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnCar : MonoBehaviour {

	public static Stack<CarController> pooling = new Stack<CarController>();

	public static void Recycle(CarController car)
	{
		car.gameObject.SetActive(false);
		pooling.Push(car);
	}

	public static CarController GetCar()
	{
		if( pooling.Count > 0 ) return pooling.Pop();
		return default(CarController);
	}

	public static void Clear()
	{
		while( pooling.Count > 0 ) 
		{
			CarController car = pooling.Pop();
			if( car != null ) Destroy( car.gameObject );
		}
	}

	public CarController[] cars;
	public float minSpeed;
	public float maxSpeed;
	public float minDelay;
	public float maxDelay;

	void OnDestroy()
	{
		Clear ();
	}

	// Use this for initialization
	IEnumerator Start () {
		Transform myT = transform;
		int total = cars.Length;
		while( PlayerController.current.body.position.y > 50.0f )
		{
			CarController c = GetCar();
			if( c==null ) c = Instantiate<CarController>( cars[Random.Range(0, total-1)] );
			Transform t = c.transform;
			t.position = myT.position;
			t.rotation = myT.rotation;
			c.gameObject.SetActive(true);
			c.speed = Random.Range(minSpeed, maxSpeed);
			c.acc = c.speed * 0.05f;
			yield return new WaitForSeconds( Random.Range( minDelay, maxDelay ) );
		}
	}
}
