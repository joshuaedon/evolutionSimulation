using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {
    public void setupSimulation() {
		UnityEngine.SceneManagement.SceneManager.LoadScene("Simulation");
    }

    public void exit() {
		Application.Quit();
    }
}
