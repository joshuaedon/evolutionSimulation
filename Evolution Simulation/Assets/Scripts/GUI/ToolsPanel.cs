using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolsPanel : MonoBehaviour {
    public int godTool;
    List<Chunk> brushChunks;
    float brushSize;
    float lastTickSpeedSliderVal;
    // Game Objects
	public GameObject statsPanel;
    public GameObject settingsPanel;
    public static GameObject graphPanel;
	public static GameObject agentPanel;
	public GameObject agentButtons;
	public GameObject terrainButtons;
	public GameObject grassButtons;
	// Android support
	Vector3 clickMousePos;

    void Start() {
        godTool = 0;
        brushChunks = new List<Chunk>();
        brushSize = 5f;
        lastTickSpeedSliderVal = 2f;

        statsPanel = GameObject.Find("StatsPanel");
        statsPanel.SetActive(false);
        settingsPanel = GameObject.Find("SettingsPanel");
        settingsPanel.SetActive(false);
        graphPanel = GameObject.Find("GraphPanel");
        graphPanel.SetActive(false);
        agentPanel = GameObject.Find("AgentPanel");
        agentPanel.SetActive(false);
		agentButtons = GameObject.Find("AgentButtons");
        agentButtons.SetActive(false);
		terrainButtons = GameObject.Find("TerrainButtons");
        terrainButtons.SetActive(false);
		grassButtons = GameObject.Find("GrassButtons");
        grassButtons.SetActive(false);
    }

    void Update() {
        // Toggle stats panel
        if(Input.GetKeyDown(KeyCode.F2))
            statsPanel.SetActive(!statsPanel.activeInHierarchy);
        // Play/pause
        if(Input.GetKeyDown(KeyCode.Space)) {
            Slider TickSpeedSlider = GameObject.Find("TickSpeedSlider").GetComponent<Slider>();
            if(SettingsPanel.ticksPerSec == 0.0f)
                TickSpeedSlider.value = lastTickSpeedSliderVal;
            else {
                lastTickSpeedSliderVal = TickSpeedSlider.value;
                TickSpeedSlider.value = 0;
            }
        }

        // Save mouse position to tell whether a mouse up is a click or a drag
        if(Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
    		clickMousePos = Input.mousePosition;
        // Select agent	
        if(Input.GetMouseButtonUp(0) && !IsPointerOverUIObject() && clickMousePos == Input.mousePosition) {
            // If an agent is already selected, hide its highlight and the agent panel
            if(agentPanel.activeInHierarchy) {
                if(AgentPanel.selectedAgent != null)
                    AgentPanel.selectedAgent.transform.GetChild(4).gameObject.SetActive(false);
                agentPanel.SetActive(false);
            }

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
  
            if(hit.transform != null && (hit.transform.name == "Agent(Clone)" || hit.transform.name == "Agent")) {
                AgentPanel.selectedAgent = hit.transform.gameObject.GetComponent<Agent>();
                AgentPanel.selectedAgent.transform.GetChild(4).gameObject.SetActive(true);
                agentPanel.SetActive(true);
            }
        }

        if(godTool != 0 && !IsPointerOverUIObject()) {
        	// Display vertices
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Casts the ray and get the first game object hit
            Physics.Raycast(ray, out hit);
            GameObject referenceVertex = (GameObject)Instantiate(Resources.Load("Simulation/Vertex"));
            for(int c = 0; c < Grid.gridArray.GetLength(0); c++) {
                for(int r = 0; r < Grid.gridArray.GetLength(1); r++) {
                	Chunk chunk = Grid.gridArray[c, r];
                    if(Vector3.Distance(hit.point, new Vector3(chunk.col, 0, chunk.row)) < brushSize) {
                        if(Grid.gridArray[c, r].vertexObj == null) {
                            Grid.gridArray[c, r].vertexObj = (GameObject)Instantiate(referenceVertex, transform);
                            Grid.gridArray[c, r].vertexObj.transform.position = new Vector3(Grid.gridArray[c, r].col, 0, Grid.gridArray[c, r].row);
                            brushChunks.Add(Grid.gridArray[c, r]);
                        }
                    } else {
                        Destroy(Grid.gridArray[c, r].vertexObj);
                        brushChunks.Remove(Grid.gridArray[c, r]);
                    }
                }
            }
            Destroy(referenceVertex);

            // Change brush size
            if(Input.GetKey(KeyCode.LeftControl)) {
            	brushSize -= Input.GetAxis("Mouse ScrollWheel") * 20f * Time.deltaTime;
            	brushSize = Mathf.Min(brushSize, 50f);
            }

            // Deselect
            if(Input.GetMouseButtonDown(0)) {
            	godTool = 0;
            	for(int c = 0; c < Grid.gridArray.GetLength(0); c++) {
                	for(int r = 0; r < Grid.gridArray.GetLength(1); r++)
                        Destroy(Grid.gridArray[c, r].vertexObj);
                }
                brushChunks.Clear();
            }

            // Use
            if(Input.GetMouseButton(1)) {
            	switch(godTool) {
            		case 1: {
            			foreach(Chunk chunk in brushChunks) {
            				if(AgentPanel.selectedAgent == chunk.agent)
		                    	AgentPanel.selectedAgent = null;
		                    if(chunk.agent != null) {
				                Destroy(chunk.agent.gameObject);
				                Grid.agents.Remove(chunk.agent);
				                chunk.agent = null;
			            	}
            			}
            			break;
            		} case 2: {
            			if(brushChunks.Count == 0)
            				break;
            			Chunk chunk = brushChunks[Random.Range(0, brushChunks.Count)];
            			if(chunk.agent == null) {
	            			Agent agent = ((GameObject)Instantiate(Resources.Load("Simulation/Agent"), GameObject.Find("Agents").transform)).GetComponent<Agent>();
			                agent.initiate(chunk);
			                chunk.agent = agent;
			                Grid.agents.Add(agent);
			            }
            			break;
            		} case 3: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.elevationOffset = Mathf.Clamp(chunk.elevationOffset - (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.col, 0f, chunk.row))) * 0.025f, -1f, 1f);
            			Grid.updateGrid();
            			break;
            		} case 4: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.elevationOffset = Mathf.Clamp(chunk.elevationOffset + (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.col, 0f, chunk.row))) * 0.025f, -1f, 1f);
        				Grid.updateGrid();
            			break;
            		} case 5: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.food = Mathf.Clamp(chunk.food - (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.col, 0f, chunk.row))) * 0.1f, 0f, 1f);
        				Grid.updateGrid();
            			break;
            		} case 6: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.food = Mathf.Clamp(chunk.food + (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.col, 0f, chunk.row))) * 0.1f, 0f, 1f);
            			Grid.updateGrid();
            			break;
            		}
            	}
            }
        }
    }

    public void exitButton() {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Simulation");
    	UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void settingsButton() {
        settingsPanel.SetActive(!settingsPanel.activeInHierarchy);
        if(settingsPanel.activeInHierarchy) {
            statsPanel.SetActive(false);
            graphPanel.SetActive(false);
        }
    }

    public void graphButton() {
        graphPanel.SetActive(!graphPanel.activeInHierarchy);
        if(graphPanel.activeInHierarchy) {
            statsPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }
    }

    public void agentButton() {
        agentButtons.SetActive(!agentButtons.activeInHierarchy);
        if(agentButtons.activeInHierarchy) {
            terrainButtons.SetActive(false);
            grassButtons.SetActive(false);
        }
    }

	public void agentClearButton() {
        for(int i = Grid.agents.Count - 1; i >= 0; i--) {
            Destroy(Grid.agents[i].gameObject);
            Grid.agents[i].chunk.agent = null;
            Grid.agents.RemoveAt(i);
        }
        AgentPanel.selectedAgent = null;
        Grid.resetGridTime();
        GraphPanel.resetStats();
    }

    public void agentResetButton() {
        agentClearButton();
        Grid.spawnAgents(SettingsPanel.rows * SettingsPanel.cols / 50);
    }

    public void agentRemoveButton() {
        godTool = 1;
    }

    public void agentAddButton() {
        godTool = 2;
    }

    public void terrainButton() {
        terrainButtons.SetActive(!terrainButtons.activeInHierarchy);
        if(terrainButtons.activeInHierarchy) {
            agentButtons.SetActive(false);
            grassButtons.SetActive(false);
        }
    }

    public void terrainResetButton() {
    	for(int c = 0; c < SettingsPanel.cols; c++) {
            for(int r = 0; r < SettingsPanel.rows; r++) {
                Grid.gridArray[c, r].elevationOffset = 0f;
            }
        }
        Grid.updateGrid();
    }

    public void terrainLowerButton() {
        godTool = 3;
    }

    public void terrainRaiseButton() {
        godTool = 4;
    }

    public void grassButton() {
        grassButtons.SetActive(!grassButtons.activeInHierarchy);
        if(grassButtons.activeInHierarchy) {
            agentButtons.SetActive(false);
            terrainButtons.SetActive(false);
        }
    }

    public void grassClearButton() {
    	for(int c = 0; c < SettingsPanel.cols; c++) {
            for(int r = 0; r < SettingsPanel.rows; r++) {
                Grid.gridArray[c, r].food = 0f;
            }
        }
        Grid.updateGrid();
    }

    public void grassResetButton() {
    	for(int c = 0; c < SettingsPanel.cols; c++) {
            for(int r = 0; r < SettingsPanel.rows; r++) {
                Grid.gridArray[c, r].food = 0.25f;
            }
        }
        Grid.updateGrid();
    }

    public void grassRemoveButton() {
        godTool = 5;
    }

    public void grassAddButton() {
        godTool = 6;
    }

    private bool IsPointerOverUIObject() {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}