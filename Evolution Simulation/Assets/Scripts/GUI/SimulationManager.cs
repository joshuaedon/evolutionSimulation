using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimulationManager : MonoBehaviour {
    public static Agent selectedAgent;
    public static bool NNFlow;
    public int godTool;
    List<Chunk> brushChunks;
    float brushSize;
    float lastTickSpeedSliderVal;
    // Game Objects
	public GameObject StatsPanel;
    public GameObject SettingsPanel;
    public static GameObject GraphPanel;
	public static GameObject AgentPanel;
	public GameObject StepButton;
	public GameObject AgentButtons;
	public GameObject TerrainButtons;
	public GameObject GrassButtons;
	// Android support
	Vector3 clickMousePos;

    void Start() {
		NNFlow = true;
        godTool = 0;
        brushChunks = new List<Chunk>();
        brushSize = 5f;
        lastTickSpeedSliderVal = 2f;

        StatsPanel = GameObject.Find("StatsPanel");
        StatsPanel.SetActive(false);
        setDefaultValues();
        SettingsPanel = GameObject.Find("SettingsPanel");
        SettingsPanel.SetActive(false);
        GraphPanel = GameObject.Find("GraphPanel");
        GraphPanel.SetActive(false);
        AgentPanel = GameObject.Find("AgentPanel");
        AgentPanel.SetActive(false);
        StepButton = GameObject.Find("StepButton");
		AgentButtons = GameObject.Find("AgentButtons");
        AgentButtons.SetActive(false);
		TerrainButtons = GameObject.Find("TerrainButtons");
        TerrainButtons.SetActive(false);
		GrassButtons = GameObject.Find("GrassButtons");
        GrassButtons.SetActive(false);

        GridController.GC = GameObject.Find("Grid").GetComponent<GridController>();
        GridController.GC.createGrid();
        GridController.GC.spawnStartingAgents();
        GameObject.Find("Main Camera").transform.position = new Vector3((GridController.GC.cols - 1) / 2.0f, 60, (GridController.GC.rows - 1) / 2.0f - 35);
    }

    void Update() {
        // Toggle stats panel
        if(Input.GetKeyDown(KeyCode.F2))
            StatsPanel.SetActive(!StatsPanel.activeInHierarchy);
        // Play/pause
        if(Input.GetKeyDown(KeyCode.Space)) {
            Slider TickSpeedSlider = GameObject.Find("TickSpeedSlider").GetComponent<Slider>();
            if(GridController.GC.ticksPerSec == 0.0f)
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // If an agent is already selected, set its colour back to white and destroy the old agent panel
            if(AgentPanel.activeInHierarchy) {
                AgentPanel.SetActive(false);
                if(selectedAgent != null) {
                    selectedAgent.plumbob.SetActive(false);;
                    selectedAgent = null;
                }
            }
            if(Physics.Raycast(ray, out hit) && hit.transform.name == "AgentBody") {
                // If the mouse is over a new agent, set its colour to red and set it as the selected agent, then create a new agent panel
                foreach(Agent a in GridController.GC.agents) {
                    if(a.agentObj.transform.GetChild(0) == hit.transform || a.agentObj.transform.GetChild(1) == hit.transform) {
                        selectedAgent = a;
                        a.plumbob.SetActive(true);
                    }
                }
                AgentPanel.SetActive(true);// = (GameObject)Instantiate(Resources.Load("GUI/AgentPanel"), GameObject.Find("Canvas").transform);
            }
        }

        if(godTool != 0 && !IsPointerOverUIObject()) {
        	// Display vertices
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Casts the ray and get the first game object hit
            Physics.Raycast(ray, out hit);
            GameObject referenceVertex = (GameObject)Instantiate(Resources.Load("Simulation/Vertex"));
            for(int c = 0; c < GridController.GC.gridArray.GetLength(0); c++) {
                for(int r = 0; r < GridController.GC.gridArray.GetLength(1); r++) {
                	Chunk chunk = GridController.GC.gridArray[c, r];
                    if(Vector3.Distance(hit.point, new Vector3(chunk.xPos, 0f, chunk.zPos)) < brushSize) {
                        if(GridController.GC.gridArray[c, r].vertexObj == null) {
                            GridController.GC.gridArray[c, r].vertexObj = (GameObject)Instantiate(referenceVertex, transform);
                            GridController.GC.gridArray[c, r].setVertexPos(GridController.GC.yScale);
                            brushChunks.Add(GridController.GC.gridArray[c, r]);
                        }
                    } else {
                        Destroy(GridController.GC.gridArray[c, r].vertexObj);
                        brushChunks.Remove(GridController.GC.gridArray[c, r]);
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
            	for(int c = 0; c < GridController.GC.gridArray.GetLength(0); c++) {
                	for(int r = 0; r < GridController.GC.gridArray.GetLength(1); r++)
                        Destroy(GridController.GC.gridArray[c, r].vertexObj);
                }
                brushChunks.Clear();
            }

            // Use
            if(Input.GetMouseButton(1)) {
            	switch(godTool) {
            		case 1: {
            			foreach(Chunk chunk in brushChunks) {
            				if(SimulationManager.selectedAgent == chunk.agent)
		                    	SimulationManager.selectedAgent = null;
		                    if(chunk.agent != null) {
				                Destroy(chunk.agent.agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material);
				                Destroy(chunk.agent.agentObj.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material);
				                Destroy(chunk.agent.agentObj);
				                GridController.GC.agents.Remove(chunk.agent);
				                chunk.agent = null;
			            	}
            			}
            			break;
            		} case 2: {
            			if(brushChunks.Count == 0)
            				break;
            			Chunk chunk = brushChunks[Random.Range(0, brushChunks.Count)];
            			if(chunk.agent == null) {
	            			GameObject agentObj = (GameObject)Instantiate(Resources.Load("Simulation/Agent"), transform);
			                Agent agent = new Agent(agentObj, chunk);
			                chunk.agent = agent;
			                GridController.GC.agents.Add(agent);
			            }
            			break;
            		} case 3: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.yOffset = Mathf.Clamp(chunk.yOffset - (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.xPos, 0f, chunk.zPos))) * 0.025f, -1f, 1f);
            			GridController.GC.updateGrid();
            			break;
            		} case 4: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.yOffset = Mathf.Clamp(chunk.yOffset + (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.xPos, 0f, chunk.zPos))) * 0.025f, -1f, 1f);
        				GridController.GC.updateGrid();
            			break;
            		} case 5: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.food = Mathf.Clamp(chunk.food - (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.xPos, 0f, chunk.zPos))) * 0.1f, 0f, 1f);
        				GridController.GC.updateGrid();
            			break;
            		} case 6: {
            			foreach(Chunk chunk in brushChunks)
            				chunk.food = Mathf.Clamp(chunk.food + (brushSize - Vector3.Distance(hit.point, new Vector3(chunk.xPos, 0f, chunk.zPos))) * 0.1f, 0f, 1f);
            			GridController.GC.updateGrid();
            			break;
            		}
            	}
            }
        }


    }

    public void adjustTickSpeed(float value) {
        if(value == 0) {
            GridController.GC.ticksPerSec = 0;
            StepButton.GetComponent<Button>().interactable = true;
        } else {
            GridController.GC.ticksPerSec = Mathf.Pow(10, value-1);
            StepButton.GetComponent<Button>().interactable = false;
        }
        GridController.GC.framesPerTick = 1;
        GameObject.Find("TickSpeedText").GetComponent<Text>().text = Mathf.Round(GridController.GC.ticksPerSec * 100f) / 100f + " ticks/sec";
    }

    public void exitButton() {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Simulation");
    	UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void settingsButton() {
        SettingsPanel.SetActive(!SettingsPanel.activeInHierarchy);
        if(SettingsPanel.activeInHierarchy) {
            StatsPanel.SetActive(false);
            GraphPanel.SetActive(false);
        }
    }

    public void graphButton() {
        GraphPanel.SetActive(!GraphPanel.activeInHierarchy);
        if(GraphPanel.activeInHierarchy) {
            StatsPanel.SetActive(false);
            SettingsPanel.SetActive(false);
        }
    }

    public void agentButton() {
        AgentButtons.SetActive(!AgentButtons.activeInHierarchy);
        if(AgentButtons.activeInHierarchy) {
            TerrainButtons.SetActive(false);
            GrassButtons.SetActive(false);
        }
    }

	public void agentClearButton() {
        for(int i = GridController.GC.agents.Count - 1; i >= 0; i--) {
            if(SimulationManager.selectedAgent == GridController.GC.agents[i])
                SimulationManager.selectedAgent = null;
            Destroy(GridController.GC.agents[i].agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material);
            Destroy(GridController.GC.agents[i].agentObj.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material);
            Destroy(GridController.GC.agents[i].agentObj);
            GridController.GC.agents[i].chunk.agent = null;
            GridController.GC.agents.RemoveAt(i);
        }

        GridController.GC.resetGraphs();
    }

    public void agentResetButton() {
        agentClearButton();
        GridController.GC.spawnStartingAgents();
    }

    public void agentRemoveButton() {
        godTool = 1;
    }

    public void agentAddButton() {
        godTool = 2;
    }

    public void terrainButton() {
        TerrainButtons.SetActive(!TerrainButtons.activeInHierarchy);
        if(TerrainButtons.activeInHierarchy) {
            AgentButtons.SetActive(false);
            GrassButtons.SetActive(false);
        }
    }

    public void terrainResetButton() {
    	for(int c = 0; c < GridController.GC.cols; c++) {
            for(int r = 0; r < GridController.GC.rows; r++) {
                GridController.GC.gridArray[c, r].yOffset = 0f;
            }
        }
        GridController.GC.updateGrid();
    }

    public void terrainLowerButton() {
        godTool = 3;
    }

    public void terrainRaiseButton() {
        godTool = 4;
    }

    public void grassButton() {
        GrassButtons.SetActive(!GrassButtons.activeInHierarchy);
        if(GrassButtons.activeInHierarchy) {
            AgentButtons.SetActive(false);
            TerrainButtons.SetActive(false);
        }
    }

    public void grassClearButton() {
    	for(int c = 0; c < GridController.GC.cols; c++) {
            for(int r = 0; r < GridController.GC.rows; r++) {
                GridController.GC.gridArray[c, r].food = 0f;
            }
        }
        GridController.GC.updateGrid();
    }

    public void grassResetButton() {
    	for(int c = 0; c < GridController.GC.cols; c++) {
            for(int r = 0; r < GridController.GC.rows; r++) {
                GridController.GC.gridArray[c, r].food = 0.25f;
            }
        }
        GridController.GC.updateGrid();
    }

    public void grassRemoveButton() {
        godTool = 5;
    }

    public void grassAddButton() {
        godTool = 6;
    }

    public void setDefaultValues() {
        // Time
        GameObject.Find("TerrainTimeStepSlider").GetComponent<Slider>().value = 1f;
        // Terrain
        GameObject.Find("ColumnsSlider").GetComponent<Slider>().value = 150;
        GameObject.Find("RowsSlider").GetComponent<Slider>().value = 75;
        GameObject.Find("NoiseScaleSlider").GetComponent<Slider>().value = 25f;
        GameObject.Find("SeaLevelSlider").GetComponent<Slider>().value = 0.45f;
        GameObject.Find("YScaleSlider").GetComponent<Slider>().value = 5f;
        GameObject.Find("SeaBorderSlider").GetComponent<Slider>().value = 10f;
        // Food
        GameObject.Find("GrassSpawnAmountSlider").GetComponent<Slider>().value = 70f;
        GameObject.Find("GrassSpawnRateSlider").GetComponent<Slider>().value = 20;
        GameObject.Find("EatSpeedSlider").GetComponent<Slider>().value = 0.5f;
        GameObject.Find("HungerLossSlider").GetComponent<Slider>().value = 0.004f;
        GameObject.Find("NodeHungerLossPenaltySlider").GetComponent<Slider>().value = 2f;

        GameObject.Find("WaterDamageSlider").GetComponent<Slider>().value = 0.1f;
        GameObject.Find("WaterMutateSlider").GetComponent<Slider>().value = 0.2f;
        GameObject.Find("AttackDamageSlider").GetComponent<Slider>().value = 1f;
    }

    public void adjustTerrainTimeStep(float value) {
        GridController.GC.terrainBias = GridController.GC.terrainBias + (GridController.GC.terrainTimeStep - Mathf.Pow(10, value-1)) * (GridController.GC.time * 0.0000001f * GridController.GC.terrainUpdate);
        if(value == 0)
            GridController.GC.terrainTimeStep = 0;
        else
            GridController.GC.terrainTimeStep = Mathf.Pow(10, value-1);
        GameObject.Find("TerrainTimeStepValue").GetComponent<Text>().text = " " + Mathf.Round(GridController.GC.terrainTimeStep * 100f) / 100f;
    }

    public void adjustCols(float valueF) {
        int value = Mathf.RoundToInt(valueF);
        GridController.GC.cols = value;
        GridController.GC.createGrid();
        GameObject.Find("ColumnsValue").GetComponent<Text>().text = " " + value + " points";
    }

    public void adjustRows(float valueF) {
        int value = Mathf.RoundToInt(valueF);
        GridController.GC.rows = value;
        GridController.GC.createGrid();
        GameObject.Find("RowsValue").GetComponent<Text>().text = " " + value + " points";
    }

    public void adjustNoiseScale(float value) {
        GridController.GC.noiseScale = value;
        GridController.GC.updateGrid();
        GameObject.Find("NoiseScaleValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f;
    }

    public void adjustSeaLevel(float value) {
        GridController.GC.seaLevel = value;
        GridController.GC.updateGrid();
        GameObject.Find("SeaLevelValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) + "%";
    }

    public void adjustYScale(float value) {
        GridController.GC.yScale = value;
        GridController.GC.updateGrid();
        GameObject.Find("YScaleValue").GetComponent<Text>().text = " " + Mathf.Round(value * 4f) + "%";
    }

    public void adjustSeaBorder(float valueF) {
        int value = Mathf.RoundToInt(valueF);
        GridController.GC.seaBorder = value;
        GridController.GC.updateGrid();
        GameObject.Find("SeaBorderValue").GetComponent<Text>().text = " " + value;
    }


    public void adjustGrassSpawnAmount(float value) {
        GridController.GC.grassSpawnAmount = value;
        GameObject.Find("GrassSpawnAmountValue").GetComponent<Text>().text = " " + Mathf.Round(value * GridController.GC.cols * GridController.GC.rows / 100f) / 100f + " food";
    }

    public void adjustGrassSpawnRate(float value) {
        GridController.GC.grassSpawnRate = Mathf.RoundToInt(value);
        GameObject.Find("GrassSpawnRateValue").GetComponent<Text>().text = " every " + value + " ticks";
    }

    public void adjustEatSpeed(float value) {
        GridController.GC.eatSpeed = value;
        GameObject.Find("EatSpeedValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " food/tick";
    }

    public void adjustHungerLoss(float value) {
        GridController.GC.hungerLoss = value;
        GameObject.Find("HungerLossValue").GetComponent<Text>().text = " " + Mathf.Round(value * 1000f) / 1000f + " food/tick";
    }

    public void adjustNodeHungerLossPenalty(float value) {
        GridController.GC.nodeHungerLossPenalty = value;
        GameObject.Find("NodeHungerLossPenaltyValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " * 10^-7 food/tick";
    }

    public void toggleUnderwaterFoodSpawn(bool b) {
    	GridController.GC.underwaterFoodSpawn = b;
    }


    public void adjustAttackDamage(float value) {
        GridController.GC.attackDamage = value;
        GameObject.Find("AttackDamageValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " health/tick";
    }

    public void adjustWaterDamage(float value) {
        GridController.GC.waterDamage = value;
        GameObject.Find("WaterDamageValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " health/tick";
    }

    public void adjustWaterMutate(float value) {
        GridController.GC.waterMutate = value;
        GameObject.Find("WaterMutateValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f;
    }

    public void toggleSeaAgents(bool b) {
    	GridController.GC.seaAgents = b;
    }

    private bool IsPointerOverUIObject() {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}