using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    public static bool isMenu;
    private static OpenSimplexNoise osn;
    //// Grid state
    public static Chunk[,] gridArray;
    public static List<Agent> agents;
    // Time
    public static int time;
    public static float frameCounter;
    public static string tickFrameString;

    public static float terrainBias;
    public static float terrainUpdate;

    private int agentRecordFrequency;
    
    void Start() {
    	Screen.fullScreen = false;

        isMenu = false;
        osn = new OpenSimplexNoise();

        gridArray = new Chunk[0, 0];
        agents = new List<Agent>();

        time = 0;

        terrainBias = 0;
        terrainUpdate = 250;

    	agentRecordFrequency = 25;

    	createGrid();
    	spawnAgents(SettingsPanel.rows * SettingsPanel.cols / 50);
    }

    public static void resetGridTime() {
        time = 0;
        terrainBias = terrainBias + time * SettingsPanel.terrainTimeStep * 0.0000001f * terrainUpdate;
    }

    void Update() {
        if(SettingsPanel.ticksPerSec != 0) {
            if(frameCounter > 0) {
                frameCounter--;
            } else {
                if((1/Time.deltaTime) < SettingsPanel.ticksPerSec) {
                    float ticksPerFrame = Mathf.Floor(SettingsPanel.ticksPerSec / Mathf.Max(2, (1/Time.deltaTime)));
                    for(int i = 0; i < ticksPerFrame; i++)
                        step();
                    tickFrameString = "Ticks/frame: " + ticksPerFrame;
                } else {
                    step();
                    frameCounter = Mathf.Floor(1 / (SettingsPanel.ticksPerSec * Time.deltaTime));
                    tickFrameString = "Frames/tick: " + frameCounter;
                }
            }
        }
    }

    public void step() {
        if(time % SettingsPanel.grassSpawnRate == 0 && !isMenu)
            spawnGrass();

        // Step bots
        for(int i = agents.Count - 1; i >= 0; i--) {
            agents[i].act(isMenu);
            if(agents[i].hunger <= 0 || agents[i].health <= 0) {
                if(AgentPanel.selectedAgent == agents[i])
                    AgentPanel.selectedAgent = null;
                Destroy(agents[i].gameObject);
                agents[i].chunk.agent = null;
                // Save a record of the agent's attributes before deleting it
                if(Random.Range(0f, 1f) < 1f/agentRecordFrequency) {
                	int senseFood, senseWater, senseAgent, senseFront, senseSide, senseBack;
                	senseFood = senseWater = senseAgent = senseFront = senseSide = senseBack = 0;
                	for(int j = 0; j < agents[i].senseThings.Length; j++) {
                		switch(agents[i].senseThings[j]) {
                			case 0: senseFood++; break;
                			case 1: senseWater++; break;
                			case 2: senseAgent++; break;
                		}
                	}
                	for(int j = 0; j < agents[i].sensePositions.Length; j++) {
                		switch(agents[i].sensePositions[j]) {
                			case 0: case 1: case 2: senseFront++; break;
                			case 3: case 4: case 5: senseSide++; break;
                			case 6: case 7: case 8: senseBack++; break;
                		}
                	}
                	AgentRecord record = new AgentRecord(
                		time,
						agents[i].generation, agents[i].ticksAlive, (agents[i].network.countWeights()/25f + 1000000f) % 1f,
						senseFood, senseWater, senseAgent,
						senseFront, senseSide, senseBack,
						agents[i].network.nodeCount, agents[i].kills, agents[i].children);

                	// Insert record in most recent list, and have a recursive 50% chance of adding it to every more distant list
                	GraphPanel.agentRecords[0].Insert(0, record);
                	for(int j = 1; j < GraphPanel.agentRecords.Count; j++) {
                		if(Random.Range(0f, 1f) < 0.5f) {
                			GraphPanel.agentRecords[j].Insert(0, record);
                		} else
                			break;
                	}
                }
                agents.RemoveAt(i);
            }
        }

        if(time % 10 == 0 && !isMenu)
        	GraphPanel.recordStats();

        // Update grid
        if(time % Mathf.Ceil(terrainUpdate / SettingsPanel.terrainTimeStep) == 0)
            updateGrid();

        // Increment time
        time += 1;
    }

    public static void createGrid() {
        // Reset chunk array
        Chunk[,] newGridArray = new Chunk[SettingsPanel.cols, SettingsPanel.rows];

        // Destroy old chunk objects
        GameObject chunksContainer = GameObject.Find("Chunks");
        foreach(Transform child in chunksContainer.transform)
            GameObject.Destroy(child.gameObject);
        // Create new chunk objects
        GameObject referenceChunk = (GameObject)Instantiate(Resources.Load("Simulation/Chunk"));
        for(int c = 0; c < SettingsPanel.cols; c++) {
            for(int r = 0; r < SettingsPanel.rows; r++) {
            	GameObject chunkObj = (GameObject)Instantiate(referenceChunk, chunksContainer.transform);
    			chunkObj.transform.position = new Vector3(c - 0.5f - SettingsPanel.cols/2f, r - 0.5f - SettingsPanel.rows/2f, 0);
                newGridArray[c, r] = new Chunk(chunkObj, c, r);
            }
        }
        Destroy(referenceChunk);

        // Move over the agents and food from the previous gridArray
        for(int c = 0; c < gridArray.GetLength(0); c++) {
            for(int r = 0; r < gridArray.GetLength(1); r++) {
                if(c < SettingsPanel.cols && r < SettingsPanel.rows)
                    newGridArray[c, r].food = gridArray[c, r].food;
            }
        }
        // Remove agents outside of the grid
        for(int i = agents.Count - 1; i >= 0; i--) {
        	if(agents[i].chunk.col >= SettingsPanel.cols || agents[i].chunk.row >= SettingsPanel.rows) {
        		if(AgentPanel.selectedAgent == agents[i])
                    AgentPanel.selectedAgent = null;
                Destroy(agents[i].gameObject);
                agents[i].chunk.agent = null;
                agents.RemoveAt(i);
        	}
        }

        gridArray = newGridArray;
        updateGrid();
    }

    public static void updateGrid() {
        // Set the positions and colours of chunk
        for(int c = 0; c < SettingsPanel.cols; c++) {
            for(int r = 0; r < SettingsPanel.rows; r++) {
                float elevation = (
                                    (float)osn.Evaluate(c / SettingsPanel.noiseScale      , r / SettingsPanel.noiseScale      ,        terrainBias + time * SettingsPanel.terrainTimeStep * 0.0000001f * terrainUpdate    )     + 
                                    (float)osn.Evaluate(c / (SettingsPanel.noiseScale / 4), r / (SettingsPanel.noiseScale / 4), 1000 + terrainBias + time * SettingsPanel.terrainTimeStep * 0.0000001f * terrainUpdate * 2) / 8 + 
                                    1
                                  ) / 2;

                int distFromBorder = Mathf.Min(c + 1, r + 1, SettingsPanel.cols - c, SettingsPanel.rows - r);
                if(distFromBorder < SettingsPanel.seaBorder)
                    elevation *= -Mathf.Pow((float)distFromBorder / SettingsPanel.seaBorder - 1, 2) + 1;

                gridArray[c, r].elevation = elevation;
                gridArray[c, r].updateColour();
            }
        }
        // Move any agent objects 
        foreach(Agent a in agents)
            a.moveObj();
    }

    // public static void spawnStartingAgents() {
    //     spawnAgents(rows * cols / 50);
    // }

    public static void spawnAgents(int count) {///Change to spawn agent (how are agents spawned with the tool)
        GameObject referenceAgent = (GameObject)Instantiate(Resources.Load("Simulation/Agent"));
        GameObject agentsContainer = GameObject.Find("Agents");
        for(int i = 0; i < count; i++) {
            Chunk chunk;
            int tries = 0;
            do {
                int col = Random.Range(0, SettingsPanel.cols);
                int row = Random.Range(0, SettingsPanel.rows);
                chunk = gridArray[col, row];
                tries++;
            } while(((chunk.isWater() && !SettingsPanel.waterAgentSpawn) || chunk.agent != null) && tries < 100);
            if(tries < 100) {
                Agent agent = ((GameObject)Instantiate(Resources.Load("Simulation/Agent"), agentsContainer.transform)).GetComponent<Agent>();
                agent.initiate(chunk);
                chunk.agent = agent;
                agents.Add(agent);
            } else {
                Debug.Log("Agent could not be spawned");
            }
        }
        Destroy(referenceAgent);
    }

    public void spawnGrass() {
        float toAdd = SettingsPanel.grassSpawnAmount * SettingsPanel.cols * SettingsPanel.rows / 10000f;

        int tries = 0;
        Chunk chunk;
        while(tries < 100 && toAdd > 0) {
            int col = Random.Range(0, SettingsPanel.cols);
            int row = Random.Range(0, SettingsPanel.rows);
            chunk = gridArray[col, row];
            if(chunk.food < 1) {
                float add = Mathf.Min(Mathf.Min(toAdd, 1f - chunk.food), 0.5f);
                toAdd -= add;
                if(SettingsPanel.waterFoodSpawn || !chunk.isWater()) {
                	chunk.food += add;
	                // Reset the color of the chunk's vertex
	                gridArray[col, row].updateColour();
                }
            }
            tries++;
        }
        // if(toAdd > 0)
        //     Debug.Log(toAdd + " food could not be spawned");
    }
}