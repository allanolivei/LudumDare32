using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIResult : MonoBehaviour {

	public Text content;
	public Text score;
	public GameObject container;

	void Update () {
		if( container.activeInHierarchy ) 
		{
			if( Input.GetKeyDown(KeyCode.Space) )
				Application.LoadLevel(Application.loadedLevel);
		}
		else if( PlayerController.current.jumpComplete )
		{
			PrintValue();
			container.SetActive(true);
		}
	}

	void PrintValue()
	{
		Highscore h = PlayerController.current.highScore;
		string result = "Impact: <color=\"white\">"+Mathf.CeilToInt(h.impact)+"</color>\n"+
			"Human: <color=\"white\">"+h.humans+"</color>\n"+
				"Human Damage Total: <color=\"white\">"+Mathf.CeilToInt(h.humanDamageTotal)+"</color>\n"+
				"Car: <color=\"white\">"+h.cars+"</color>\n"+
				"Car Damage Total: <color=\"white\">"+Mathf.CeilToInt(h.carDamageTotal)+"</color>\n"+
				"Self Damage: <color=\"white\">"+Mathf.CeilToInt(h.selfDamage)+"</color>\n";

		content.text = result;
		score.text = "Score Total: <color=\"red\">"+Mathf.CeilToInt(h.score)+"</color>";
	}
}
