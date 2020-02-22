﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimulationManager : MonoBehaviour {
    public static Agent selectedAgent;
    float lastTickSpeedSliderVal;
    // Game Objects
	public GameObject StatsPanel;
    public GameObject SettingsPanel;
	public GameObject AgentPanel;

    void Start() {
        lastTickSpeedSliderVal = 2.0f;

        StatsPanel = GameObject.Find("StatsPanel");
        StatsPanel.SetActive(false);
        setDefaultValues();
        SettingsPanel = GameObject.Find("SettingsPanel");
        SettingsPanel.SetActive(false);
        AgentPanel = GameObject.Find("AgentPanel");
        AgentPanel.SetActive(false);

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

        // Select agent	
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // If an agent is already selected, set its colour back to white and destroy the old agent panel
            if(AgentPanel.activeInHierarchy) {
                AgentPanel.SetActive(false);
                if(selectedAgent != null) {
                    selectedAgent.agentObj.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                    selectedAgent = null;
                }
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

    public void adjustTickSpeed(float value) {
        if(value == 0)
            GridController.GC.ticksPerSec = 0;
        else
            GridController.GC.ticksPerSec = Mathf.Pow(10, value-1);
        // GridController.GC.framesPerTick = 1;
        GameObject.Find("TickSpeedText").GetComponent<Text>().text = Mathf.Round(GridController.GC.ticksPerSec * 100f) / 100f + " ticks/sec";
    }

    public void settingsButton() {
        SettingsPanel.SetActive(!SettingsPanel.activeInHierarchy);
        if(SettingsPanel.activeInHierarchy)
            StatsPanel.SetActive(false);
    }

    public void setDefaultValues() {
        // Time
        GameObject.Find("TerrainTimeStepSlider").GetComponent<Slider>().value = 1f;
        // Terrain
        GameObject.Find("ColumnsSlider").GetComponent<Slider>().value = 150;
        GameObject.Find("RowsSlider").GetComponent<Slider>().value = 75;
        GameObject.Find("NoiseScaleSlider").GetComponent<Slider>().value = 15f;
        GameObject.Find("SeaLevelSlider").GetComponent<Slider>().value = 0.45f;
        GameObject.Find("YScaleSlider").GetComponent<Slider>().value = 5f;
        GameObject.Find("SeaBorderSlider").GetComponent<Slider>().value = 10f;
        // Food
        GameObject.Find("GrassSpawnAmountSlider").GetComponent<Slider>().value = 1f;
        GameObject.Find("GrassSpawnTimeSlider").GetComponent<Slider>().value = 100;
        GameObject.Find("EatSpeedSlider").GetComponent<Slider>().value = 1f;
        GameObject.Find("HungerLossSlider").GetComponent<Slider>().value = 0.01f;
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
        GameObject.Find("ColumnsValue").GetComponent<Text>().text = " " + value + " columns";
    }

    public void adjustRows(float valueF) {
        int value = Mathf.RoundToInt(valueF);
        GridController.GC.rows = value;
        GridController.GC.createGrid();
        GameObject.Find("RowsValue").GetComponent<Text>().text = " " + value + " rows";
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
        GameObject.Find("GrassSpawnAmountValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " food";
    }

    public void adjustGrassSpawnTime(float value) {
        GridController.GC.grassSpawnTime = Mathf.RoundToInt(value);
        GameObject.Find("GrassSpawnTimeValue").GetComponent<Text>().text = " every " + value + " ticks";
    }

    public void adjustEatSpeed(float value) {
        GridController.GC.eatSpeed = value;
        GameObject.Find("EatSpeedValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " food/tick";
    }

    public void adjustHungerLoss(float value) {
        GridController.GC.hungerLoss = value;
        GameObject.Find("HungerLossValue").GetComponent<Text>().text = " " + Mathf.Round(value * 1000f) / 1000f + " food/tick";
    }
}
