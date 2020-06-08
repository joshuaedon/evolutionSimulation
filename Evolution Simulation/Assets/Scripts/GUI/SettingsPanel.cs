using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsPanel : MonoBehaviour {
    // Time
    public static float ticksPerSec;
    public static float terrainTimeStep;
    // Terrain
    public static int cols;
    public static int rows;
    public static float noiseScale;
    public static float seaLevel;
    public static int seaBorder;
    public static bool waterAgentSpawn; ////////////////// Implement
    // Food
    public static int grassSpawnRate;
    public static float grassSpawnAmount;
    public static float eatSpeed;
    public static float hungerLoss;
    public static float nodeHungerLossPenalty;//////// Move down
    public static bool waterFoodSpawn;
    // Health
    public static float attackDamage;
    public static float waterDamage;
    public static float waterMutate;
    // Neural Network
    public static float nodeSize;
    public static float weightDecay;
    public static bool independentMutateValues; // Should this be split into chance and weight mutates????
    ////////////////// Modify mutateValue amount?
    public static float addRemoveNodeChance; // chance of node being removed (choose random node), chance of node being added (place with no connections)
    public static float addInputOutputNodeChance;
    ////////////////// Mutate between node types?
    public static bool mergeConnections;
    public static float addRemoveConnectionChance; // for two nodes being considered, chance of connection being added if none or removed if there
    public static int addRemoveConnectionType; // Per network/node/nodePair
    public static float modifyConnection;
    public static float startConnectionDensity;

    void Awake() {
        // Time
    	ticksPerSec = 0;
        terrainTimeStep = 1f;
        // Terrain
        cols = 150;
        rows = 75;
        noiseScale = 25f;
        seaLevel = 0.45f;
        seaBorder = 10;
        waterAgentSpawn = false;
        // Food
        grassSpawnAmount = 70f;
        grassSpawnRate = 20;
        eatSpeed = 0.5f;
        hungerLoss = 0.004f;
        nodeHungerLossPenalty = 2f;
        waterFoodSpawn = false;
        // Health
        attackDamage = 1f;
        waterDamage = 0.1f;
        waterMutate = 0.2f;
        // Neural Network
		nodeSize = 0.1f;
	    weightDecay = 0.001f;
	    independentMutateValues = false;
	    addRemoveNodeChance = 0.05f;
	    addInputOutputNodeChance = 0.2f;
	    mergeConnections = false;
	    addRemoveConnectionChance = 0.2f;
	    addRemoveConnectionType = 1; // Per 0-network, 1-node, 2-nodePair
	    modifyConnection = 1f;
	    startConnectionDensity = 0.2f;

        setDefaultValues();/////////////////////////// Redundancy????
    }

    public void setDefaultValues() {
        // Time
        GameObject.Find("TerrainTimeStepSlider").GetComponent<Slider>().value = 1f;
        // Terrain
        GameObject.Find("ColumnsSlider").GetComponent<Slider>().value = 150;
        GameObject.Find("RowsSlider").GetComponent<Slider>().value = 75;
        GameObject.Find("NoiseScaleSlider").GetComponent<Slider>().value = 25f;
        GameObject.Find("SeaLevelSlider").GetComponent<Slider>().value = 0.45f;
        GameObject.Find("SeaBorderSlider").GetComponent<Slider>().value = 10f;
        ////////////////////////////////waterAgentSpawn
        // Food
        GameObject.Find("GrassSpawnAmountSlider").GetComponent<Slider>().value = 70f;
        GameObject.Find("GrassSpawnRateSlider").GetComponent<Slider>().value = 20;
        GameObject.Find("EatSpeedSlider").GetComponent<Slider>().value = 0.5f;
        GameObject.Find("HungerLossSlider").GetComponent<Slider>().value = 0.004f;
        GameObject.Find("NodeHungerLossPenaltySlider").GetComponent<Slider>().value = 2f;
		//Health
        GameObject.Find("WaterDamageSlider").GetComponent<Slider>().value = 0.1f;
        GameObject.Find("WaterMutateSlider").GetComponent<Slider>().value = 0.2f;
        GameObject.Find("AttackDamageSlider").GetComponent<Slider>().value = 1f;
    }


    public void adjustTickSpeed(float value) {
        if(value == 0) {
            ticksPerSec = 0;
            GameObject.Find("StepButton").GetComponent<Button>().interactable = true;/////// Check if working
        } else {
            ticksPerSec = Mathf.Pow(10, value-1);
            GameObject.Find("StepButton").GetComponent<Button>().interactable = false;
        }
        Grid.frameCounter = 1;
        GameObject.Find("TickSpeedText").GetComponent<Text>().text = Mathf.Round(ticksPerSec * 100f) / 100f + " ticks/sec";
    }


    public void adjustTerrainTimeStep(float value) {
        Grid.terrainBias = Grid.terrainBias + (terrainTimeStep - Mathf.Pow(10, value-1)) * (Grid.time * 0.0000001f * Grid.terrainUpdate);
        if(value == 0)
            terrainTimeStep = 0;
        else
            terrainTimeStep = Mathf.Pow(10, value-1);
        GameObject.Find("TerrainTimeStepValue").GetComponent<Text>().text = " " + Mathf.Round(terrainTimeStep * 100f) / 100f;
    }

    public void adjustCols(float valueF) {
        int value = Mathf.RoundToInt(valueF);
        cols = value;
        Grid.createGrid();
        GameObject.Find("ColumnsValue").GetComponent<Text>().text = " " + value + " points";
    }

    public void adjustRows(float valueF) {
        int value = Mathf.RoundToInt(valueF);
        rows = value;
        Grid.createGrid();
        GameObject.Find("RowsValue").GetComponent<Text>().text = " " + value + " points";
    }

    public void adjustNoiseScale(float value) {
        noiseScale = value;
        Grid.updateGrid();
        GameObject.Find("NoiseScaleValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f;
    }

    public void adjustSeaLevel(float value) {
        seaLevel = value;
        Grid.updateGrid();
        GameObject.Find("SeaLevelValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) + "%";
    }

    public void adjustSeaBorder(float valueF) {
        int value = Mathf.RoundToInt(valueF);
        seaBorder = value;
        Grid.updateGrid();
        GameObject.Find("SeaBorderValue").GetComponent<Text>().text = " " + value;
    }


    public void adjustGrassSpawnAmount(float value) {
        grassSpawnAmount = value;
        GameObject.Find("GrassSpawnAmountValue").GetComponent<Text>().text = " " + Mathf.Round(value * cols * rows / 100f) / 100f + " food";
    }

    public void adjustGrassSpawnRate(float value) {
        grassSpawnRate = Mathf.RoundToInt(value);
        GameObject.Find("GrassSpawnRateValue").GetComponent<Text>().text = " every " + value + " ticks";
    }

    public void adjustEatSpeed(float value) {
        eatSpeed = value;
        GameObject.Find("EatSpeedValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " food/tick";
    }

    public void adjustHungerLoss(float value) {
        hungerLoss = value;
        GameObject.Find("HungerLossValue").GetComponent<Text>().text = " " + Mathf.Round(value * 1000f) / 1000f + " food/tick";
    }

    public void adjustNodeHungerLossPenalty(float value) {
        nodeHungerLossPenalty = value;
        GameObject.Find("NodeHungerLossPenaltyValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " * 10^-7 food/tick";
    }

    public void toggleWaterFoodSpawn(bool b) {
    	waterFoodSpawn = b;
    }


    public void adjustAttackDamage(float value) {
        attackDamage = value;
        GameObject.Find("AttackDamageValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " health/tick";
    }

    public void adjustWaterDamage(float value) {
        waterDamage = value;
        GameObject.Find("WaterDamageValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f + " health/tick";
    }

    public void adjustWaterMutate(float value) {
        waterMutate = value;
        GameObject.Find("WaterMutateValue").GetComponent<Text>().text = " " + Mathf.Round(value * 100f) / 100f;
    }
}