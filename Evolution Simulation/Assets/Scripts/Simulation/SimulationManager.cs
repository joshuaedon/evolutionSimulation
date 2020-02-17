using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour {
    public static Agent selectedAgent;
    // Game Objects
	public GameObject StatsPanel;
	public GameObject AgentPanel;
	public GameObject TickSpeedText;

    void Start() {
        StatsPanel = GameObject.Find("StatsPanel");
        StatsPanel.SetActive(true);
        AgentPanel = GameObject.Find("AgentPanel");
        AgentPanel.SetActive(false);
        TickSpeedText = GameObject.Find("TickSpeedText");

        GridController grid = GameObject.Find("Grid").GetComponent<GridController>();
        grid.createGrid();
        grid.spawnAgents(GridController.startingAgents);
    }

    void Update() {
        // Toggle stats panel
        if(Input.GetKeyDown(KeyCode.F2))
            StatsPanel.SetActive(!StatsPanel.activeInHierarchy);

        // Select agent	
        if(Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(selectedAgent != null)
                selectedAgent.agentObj.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            if(Physics.Raycast(ray, out hit) && hit.transform.name == "AgentBody") {
                foreach(Agent a in GridController.agents) {
                    if(a.agentObj.transform.GetChild(0) == hit.transform) {
                        a.agentObj.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                        selectedAgent = a;
                    }
                }
                AgentPanel.SetActive(true);
            } else {
                selectedAgent = null;
                AgentPanel.SetActive(false);
            }
        }
    }

    public void adjustTickSpeed(float speed) {
        Debug.Log(speed);
        if(speed == 0)
            GridController.ticksPerSec = 0;
        else
            GridController.ticksPerSec = Mathf.Pow(10, speed-1);
        GridController.framesPerTick = 1;
        TickSpeedText.GetComponent<Text>().text = Mathf.Round(GridController.ticksPerSec * 100f) / 100f + " ticks/sec";
    }
}
