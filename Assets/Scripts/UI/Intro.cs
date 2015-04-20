using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class Intro : MonoBehaviour {

	void Awake()
	{
		Time.timeScale = 0.0f;
	}

	void Update()
	{
		if( Input.GetKeyDown(KeyCode.Space) ) 
		{
			gameObject.SetActive(false);
			Camera.main.GetComponent<BlurOptimized>().enabled = false;
			Time.timeScale = 1.0f;
		}

		Input.ResetInputAxes();
	}

}
