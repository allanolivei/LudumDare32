using UnityEngine;
using System.Collections;

public class Teleportation : MonoBehaviour {

	public Teleportation otherTeleportation;


	public void Teleport( Transform target )
	{
		otherTeleportation.Exit(target);
	}

	public void Exit( Transform other)
	{
		GetComponent<Collider>().enabled = false;
		other.position = transform.position;
		Invoke("EnableTeleport", 1.0f);
	}

	private void EnableTeleport()
	{
		GetComponent<Collider>().enabled = true;
	}
}
