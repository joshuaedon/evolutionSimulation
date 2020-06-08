using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {
	void Start() {
		SettingsPanel.cols = 150;
		SettingsPanel.rows = 150;
		// GridController.GC.createGrid();
		// for(int c = 0; c < 150; c++) {
  //           for(int r = 0; r < 150; r++) {
  //           	GridController.GC.gridArray[c, r].food = 1f;
  //           }
  //       }
		// GridController.GC.spawnStartingAgents();
		// GridController.GC.isMenu = true;
		// GridController.GC.ticksPerSec = 5.0f;
		// GameObject.Find("Main Camera").transform.position = new Vector3((GridController.GC.cols - 1) / 2.0f, 60, (GridController.GC.rows - 1) / 2.0f - 35);
	}

    public void play() {
    	UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MainMenu");
		UnityEngine.SceneManagement.SceneManager.LoadScene("Simulation");
    }

    public void exit() {
		Application.Quit();
    }
}
