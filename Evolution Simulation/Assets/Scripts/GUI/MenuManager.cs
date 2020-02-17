using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {
	void Start() {
		GridController.cols = 150;
		GridController.rows = 150;
		GridController grid = GameObject.Find("Grid").GetComponent<GridController>();
		grid.createGrid();
		grid.spawnAgents(50);
		GridController.isMenu = true;
		GridController.ticksPerSec = 5.0f;
	}

    public void play() {
		UnityEngine.SceneManagement.SceneManager.LoadScene("Simulation");
    }

    public void exit() {
		Application.Quit();
    }
}
