using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsManager : MonoBehaviour {
	public Text text;

    void Update() {
        text.text = "FPS: " 		  + Mathf.Round(1f/Time.deltaTime) +
        			"\n"	  		  + GridController.tickFrame +
        			"\nTime: " 		  + GridController.time +
        			"\nAgent count: " + GridController.agents.Count;
    }
}
