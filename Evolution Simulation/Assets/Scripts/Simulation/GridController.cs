using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {
    public static GridController GC;
    public bool isMenu;
    OpenSimplexNoise osn;
    //// Setup variables
    public bool startingAgents;
    //// Grid attributes
    // Time
    public float ticksPerSec;
    public float framesPerTick;
    public string tickFrame;
    public int time;
    public float terrainTimeStep;
    public float terrainBias;
    public float terrainUpdate;
    // Terrain
    public int cols;
    public int rows;
    public float noiseScale;
    public float seaLevel;
    public float yScale;
    public int seaBorder;
    // Food
    public int grassSpawnRate;
    public float grassSpawnAmount;
    public float eatSpeed;
    public float hungerLoss;
    public float nodeHungerLossPenalty;
    public bool underwaterFoodSpawn;
    // Health
    public float attackDamage;
    public float waterDamage;
    public float waterMutate;
    public bool seaAgents;
    //// Grid state
    public Chunk[,] gridArray;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    Color sand;
    Color land;
    public List<Agent> agents;
    //// Graph data
    public List<List<int>> population;
    public int agentRecordFrequency;
    public List<List<AgentRecord>> agentRecords;
    
    void OnEnable() {
    	Screen.fullScreen = false;

        isMenu = false;
        osn = new OpenSimplexNoise();

        startingAgents = true;
        // Time
    	ticksPerSec = 0;
        terrainTimeStep = 1f;
        terrainBias = 0;
        terrainUpdate = 250;
        // Terrain
        cols = 150;
        rows = 75;
        noiseScale = 25f;
        seaLevel = 0.45f;
        yScale = 5f;
        seaBorder = 10;
        // Food
        grassSpawnAmount = 45f;
        grassSpawnRate = 20;
        eatSpeed = 0.5f;
        hungerLoss = 0.004f;
        nodeHungerLossPenalty = 0.0000005f;
        underwaterFoodSpawn = false;
        // Health
        attackDamage = 0.5f;
        waterDamage = 0.1f;
        waterMutate = 0.2f;
        seaAgents = false;

        gridArray = new Chunk[0, 0];
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        sand = new Color(0.941f, 0.953f, 0.741f);
        land = new Color(0.263f, 0.157f, 0.094f);
        agents = new List<Agent>();

        resetGraphs();
    }

    public void resetGraphs() {
        terrainBias = terrainBias + time * terrainTimeStep * 0.0000001f * terrainUpdate;
        time = 0;

    	population = new List<List<int>>();
        population.Add(new List<int>());
        population[0].Add(0);

    	agentRecordFrequency = 25;
        agentRecords = new List<List<AgentRecord>>();
        agentRecords.Add(new List<AgentRecord>());
    }

    void Update() {
        // Step
        if(ticksPerSec != 0) {
            if(framesPerTick > 0) {
                framesPerTick--;
            } else {
                if((1/Time.deltaTime) < ticksPerSec) {
                    float ticksPerFrame = Mathf.Floor(ticksPerSec / Mathf.Max(2, (1/Time.deltaTime)));
                    for(int i = 0; i < ticksPerFrame; i++)
                        step();
                    tickFrame = "Ticks/frame: " + ticksPerFrame;
                } else {
                    step();
                    framesPerTick = Mathf.Floor(1 / (ticksPerSec * Time.deltaTime));
                    tickFrame = "Frames/tick: " + framesPerTick;
                }
            }
        }
    }

    public void step() {
        if(time % grassSpawnRate == 0 && !isMenu)
            spawnGrass();

        // Step bots
        for(int i = agents.Count - 1; i >= 0; i--) {
            agents[i].act(isMenu);
            if(agents[i].hunger <= 0 || agents[i].health <= 0) {
                if(SimulationManager.selectedAgent == agents[i])
                    SimulationManager.selectedAgent = null;
                Destroy(agents[i].agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material);
                Destroy(agents[i].agentObj.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material);
                Destroy(agents[i].agentObj);
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
						agents[i].landSea, agents[i].generation, agents[i].ticksAlive, (agents[i].network.countWeights()/25f + 1000000f) % 1f,
						senseFood, senseWater, senseAgent,
						senseFront, senseSide, senseBack,
						agents[i].network.nodeCount, agents[i].kills);

                	agentRecords[0].Insert(0, record);
                	for(int j = 1; j < agentRecords.Count; j++) {
                		if(Random.Range(0f, 1f) < 0.5f) {
                			agentRecords[j].Insert(0, record);
                		} else
                			break;
                	}
                }
                agents.RemoveAt(i);
            }
        }

        if(time % 10 == 0 && !isMenu)
        	recordStats();

        // Update grid
        if(time % Mathf.Ceil(terrainUpdate / terrainTimeStep) == 0)
            updateGrid();

        // Increment time
        time += 1;

        mesh.colors = colours;
    }

    public void createGrid() {
        // Reset chunk and vertex arrays
        Chunk[,] newGridArray = new Chunk[cols, rows];
        vertices = new Vector3[cols * rows];

        // Create new chunk objects
        for(int c = 0; c < cols; c++) {
            for(int r = 0; r < rows; r++) {
                newGridArray[c, r] = new Chunk(c, r);
            }
        }
        // Move over the agents and food from the previous gridArray
        for(int c = 0; c < gridArray.GetLength(0); c++) {
            for(int r = 0; r < gridArray.GetLength(1); r++) {
                if(c < cols && r < rows)
                    newGridArray[c, r].food = gridArray[c, r].food;
            }
        }
        // Remove agents outside of the grid
        for(int i = agents.Count - 1; i >= 0; i--) {
        	if(agents[i].chunk.xPos >= cols || agents[i].chunk.zPos >= rows) {
        		if(SimulationManager.selectedAgent == agents[i])
                    SimulationManager.selectedAgent = null;
	            Destroy(agents[i].agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material);
	            Destroy(agents[i].agentObj.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material);
                Destroy(agents[i].agentObj);
                agents[i].chunk.agent = null;
                agents.RemoveAt(i);
        	}
        }

        // Create mesh triangles
        triangles = new int[(cols-1) * (rows - 1) * 6];
        int v = 0;
        int t = 0;
        for(int c = 0; c < cols-1; c++) {
            for(int r = 0; r < rows - 1; r++) {
                triangles[t]     = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + rows;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + rows + 1;
                triangles[t + 5] = v + rows;

                v++;
                t += 6;
            }
            v++;
        }

        gridArray = newGridArray;

        updateGrid();
    }

    public void updateGrid() {
        // Set up water plane
        transform.Find("Water").transform.position = new Vector3(0, yScale * seaLevel, 0);
        transform.Find("Water").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);

        // Set up sea floor plane
        transform.Find("Sea Floor").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.SetColor("_Color", sand);

        // Set the positions and colours of vertices
        colours = new Color[cols * rows];
        for(int c = 0; c < cols; c++) {
            for(int r = 0; r < rows; r++) {
                float elevation = (
                                    (float)osn.Evaluate(c / noiseScale      , r / noiseScale      ,        terrainBias + time * terrainTimeStep * 0.0000001f * terrainUpdate    )     + 
                                    (float)osn.Evaluate(c / (noiseScale / 4), r / (noiseScale / 4), 1000 + terrainBias + time * terrainTimeStep * 0.0000001f * terrainUpdate * 2) / 8 + 
                                    1
                                  ) / 2;

                int distFromBorder = Mathf.Min(c + 1, r + 1, cols - c, rows - r);
                if(distFromBorder < seaBorder)
                    elevation *= -Mathf.Pow((float)distFromBorder / seaBorder - 1, 2) + 1;

                gridArray[c, r].yPos = elevation;
                if(gridArray[c, r].vertexObj != null)
                    gridArray[c, r].setVertexPos(yScale);
                vertices[c*rows + r] = new Vector3(c, Mathf.Clamp(elevation + gridArray[c, r].yOffset, 0.001f, 1f) * yScale, r);

                Color col = Color.Lerp(sand, land, (1 + seaLevel) * Mathf.Clamp(elevation + gridArray[c, r].yOffset, 0.001f, 1f) - seaLevel);
                colours[c*rows + r] = Color.Lerp(col, new Color(col.r, 1, col.b), gridArray[c, r].food);
            }
        }
        // Move any agent objects 
        foreach(Agent a in agents)
            a.moveObj();

        // Create mesh object
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colours;
        mesh.RecalculateNormals();
    }

    public void updateVertexColour(int c, int r) {
        Color color = Color.Lerp(sand, land, (1 + seaLevel) * Mathf.Clamp(gridArray[c, r].yPos + gridArray[c, r].yOffset, 0f, 1f) - seaLevel);
        colours[Mathf.RoundToInt(c*rows + r)] = Color.Lerp(color, new Color(color.r, 1, color.b), gridArray[c, r].food);
    }

    public void spawnStartingAgents() {
        if(startingAgents)
            spawnAgents(rows * cols / 50);
    }

    public void spawnAgents(int count) {
        GameObject referenceAgent = (GameObject)Instantiate(Resources.Load("Simulation/Agent"));
        for(int i = 0; i < count; i++) {
            Chunk chunk;
            int tries = 0;
            do {
                int col = Random.Range(0, cols);
                int row = Random.Range(0, rows);
                chunk = gridArray[col, row];
                tries++;
            } while(((chunk.isWater() && !seaAgents) || chunk.agent != null) && tries < 100);
            if(tries < 100) {
                GameObject agentObj = (GameObject)Instantiate(referenceAgent, transform);
                Agent agent = new Agent(agentObj, chunk);
                chunk.agent = agent;
                agents.Add(agent);
            } else {
                Debug.Log("Agent could not be spawned");
            }
        }
        Destroy(referenceAgent);
    }

    public void spawnGrass() {
        float toAdd = grassSpawnAmount * cols * rows / 10000f;

        int tries = 0;
        Chunk chunk;
        while(tries < 100 && toAdd > 0) {
            int col = Random.Range(0, cols);
            int row = Random.Range(0, rows);
            chunk = gridArray[col, row];
            if(chunk.food < 1) {
                float add = Mathf.Min(Mathf.Min(toAdd, 1f - chunk.food), 0.5f);
                toAdd -= add;
                if(underwaterFoodSpawn || !chunk.isWater()) {
                	chunk.food += add;
	                // Reset the color of the chunk's vertex
	                updateVertexColour(chunk.xPos, chunk.zPos);
                }
            }
            tries++;
        }
        // if(toAdd > 0)
        //     Debug.Log(toAdd + " food could not be spawned");
    }

    public void recordStats() {
  //   	string s = "[";
		// for(int i = 0; i < population.Count; i++)
		// 	s += population[i].Count + ", ";
		// s += "], [";
		// for(int i = 0; i < agentRecords.Count; i++)
		// 	s += agentRecords[i].Count + ", ";
		// s += "]";
		// Debug.Log(s);

    	int popCount = agents.Count;

    	// If the last list in population has a legth of 100
    	if(time % (10 * Mathf.Pow(2, population.Count - 1)) == 0 && population[population.Count - 1].Count >= 100) {
    		// Add a new population list, copy over every other population and set the max value
    		population.Add(new List<int>());
    		int max = 0;
    		for(int i = 1; i < population[population.Count - 2].Count; i += 2) {
    			int val = population[population.Count - 2][i];
    			max = Mathf.Max(max, val);
    			population[population.Count - 1].Add(val);
    		}
    		population[population.Count - 1].Insert(0, max);

    		// Add a new agent records list, copy over every other record and set the max values
    		agentRecords.Add(new List<AgentRecord>());
    		for(int i = 2; i < agentRecords[agentRecords.Count - 2].Count; i += 2) {
    			agentRecords[agentRecords.Count - 1].Add(agentRecords[agentRecords.Count - 2][i]);
    		}
    	}

    	for(int i = 0; i < population.Count; i++) {
    		if(time % (10 * Mathf.Pow(2, i)) == 0) {
    			population[i].Insert(1, popCount);
    			if(population[i][0] < popCount)
					population[i][0] = popCount;
				if(population[i].Count > 101) {
					if(population[i][population[i].Count - 1] >= population[i][0]) {
						int max = 0;
						for(int j = 1; j < population[i].Count - 1; j++)
							max = Mathf.Max(max, population[i][j]);
						population[i][0] = max;
					}
					population[i].RemoveAt(population[i].Count - 1);
				}
    		}
    	}

    	for(int i = 0; i < agentRecords.Count; i++) {
    		while(agentRecords[i].Count > 2 && time - agentRecords[i][agentRecords[i].Count - 1].deathTick > 1000 * Mathf.Pow(2, i)) {
				agentRecords[i].RemoveAt(agentRecords[i].Count - 1);
    		}
    	}
    }
}