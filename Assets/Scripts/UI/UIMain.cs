using UnityEngine;
using System.Collections;

public class UIMain : MonoBehaviour {

	public void ResetLevel()
	{
		Application.LoadLevel(Application.loadedLevel);
	}
}
