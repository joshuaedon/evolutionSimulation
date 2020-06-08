using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour {
	public Text text;

    void Update() {
        text.text = "FPS: " 		  + Mathf.Round(1f/Time.deltaTime) +
        			"\n"	  		  + Grid.tickFrameString +
        			"\nTime: " 		  + Grid.time +
        			"\nAgent count: " + Grid.agents.Count;
    }
}
