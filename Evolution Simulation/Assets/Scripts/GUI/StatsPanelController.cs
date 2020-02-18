using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanelController : MonoBehaviour {
	public Text text;

    void Update() {
        text.text = "FPS: " 		  + Mathf.Round(1f/Time.deltaTime) +
        			"\n"	  		  + GridController.GC.tickFrame +
        			"\nTime: " 		  + GridController.GC.time +
        			"\nAgent count: " + GridController.GC.agents.Count;
    }
}
