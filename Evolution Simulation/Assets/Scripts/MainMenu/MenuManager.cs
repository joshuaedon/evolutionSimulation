﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {
    public void play() {
    		UnityEngine.SceneManagement.SceneManager.LoadScene("Simulation");
    }
}
