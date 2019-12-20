using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsManager : MonoBehaviour {
		public Text text;

    void Update() {
    		GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        text.text = "Agent count: " + gridController.agents.Count;
    }
}
