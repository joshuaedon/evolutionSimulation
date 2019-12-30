using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsManager : MonoBehaviour {
		public Text text;

    void Update() {
    		GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        text.text = "FPS: " + Mathf.Round(1f/Time.deltaTime) +
        						"\nAgent count: " + gridController.agents.Count;
    }
}
