using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimulationManager : MonoBehaviour {
    public static Agent selectedAgent;
    float lastTicksPerSec;
    // Game Objects
	public GameObject StatsPanel;
	public GameObject AgentPanel;
	public GameObject TickSpeedText;

    void Start() {
        lastTicksPerSec = 2.0f;

        StatsPanel = GameObject.Find("StatsPanel");
        StatsPanel.SetActive(false);
        AgentPanel = GameObject.Find("AgentPanel");
        AgentPanel.SetActive(false);
        TickSpeedText = GameObject.Find("TickSpeedText");

        GridController.GC = GameObject.Find("Grid").GetComponent<GridController>();
        GridController.GC.createGrid();
        GridController.GC.spawnStartingAgents();
        GameObject.Find("Main Camera").transform.position = new Vector3((GridController.GC.cols - 1) / 2.0f, 60, (GridController.GC.rows - 1) / 2.0f - 35);
    }

    void Update() {
        // Toggle stats panel
        if(Input.GetKeyDown(KeyCode.F2))
            StatsPanel.SetActive(!StatsPanel.activeInHierarchy);
        if(Input.GetKeyDown(KeyCode.Space)) {
            Slider slider = GameObject.Find("TickSpeedSlider").GetComponent<Slider>();
            if(GridController.GC.ticksPerSec == 0.0f)
                slider.value = lastTicksPerSec;
            else {
                lastTicksPerSec = slider.value;
                slider.value = 0;
            }
        }


            

        // Select agent	
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // If an agent is already selected, set its colour back to white and destroy the old agent panel
            if(selectedAgent != null) {
                selectedAgent.agentObj.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                AgentPanel.SetActive(false);
                selectedAgent = null;
            }
            if(Physics.Raycast(ray, out hit) && hit.transform.name == "AgentBody") {
                // If the mouse is over a new agent, set its colour to red and set it as the selected agent, then create a new agent panel
                foreach(Agent a in GridController.GC.agents) {
                    if(a.agentObj.transform.GetChild(0) == hit.transform) {
                        a.agentObj.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                        selectedAgent = a;
                    }
                }
                AgentPanel.SetActive(true);// = (GameObject)Instantiate(Resources.Load("GUI/AgentPanel"), GameObject.Find("Canvas").transform);
            }
        }
    }

    public void adjustTickSpeed(float speed) {
        if(speed == 0)
            GridController.GC.ticksPerSec = 0;
        else
            GridController.GC.ticksPerSec = Mathf.Pow(10, speed-1);
        GridController.GC.framesPerTick = 1;
        TickSpeedText.GetComponent<Text>().text = Mathf.Round(GridController.GC.ticksPerSec * 100f) / 100f + " ticks/sec";
    }
}
