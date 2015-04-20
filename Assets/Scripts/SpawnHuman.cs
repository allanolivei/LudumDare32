using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnHuman : MonoBehaviour {

	public static Stack<HumanController> pooling = new Stack<HumanController>();
	
	public static void Recycle(HumanController human)
	{
		human.gameObject.SetActive(false);
		pooling.Push(human);
	}
	
	public static HumanController GetHuman()
	{
		if( pooling.Count > 0 ) return pooling.Pop();
		return default(HumanController);
	}
	
	public static void Clear()
	{
		while( pooling.Count > 0 ) 
		{
			HumanController human = pooling.Pop();
			if( human != null ) Destroy( human.gameObject );
		}
	}
	
	public HumanController[] humans;
	public float minDelay;
	public float maxDelay;
	
	void OnDestroy()
	{
		Clear ();
	}
	
	// Use this for initialization
	IEnumerator Start () {
		Transform myT = transform;
		int total = humans.Length;
		while( PlayerController.current.body.position.y > 50.0f)
		{
			HumanController c = GetHuman();
			if( c==null ) c = Instantiate<HumanController>( humans[Random.Range(0, total-1)] );
			Transform t = c.transform;
			t.position = myT.position + 
				(Quaternion.Euler(0,Random.Range(0,360),0) * Vector3.forward * myT.localScale.x*0.5f);
			c.gameObject.SetActive(true);
			yield return new WaitForSeconds( Random.Range( minDelay, maxDelay ) );
		}
	}
}
