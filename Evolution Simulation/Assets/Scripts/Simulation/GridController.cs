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
    public bool underwaterFoodSpawn;
    //// Grid state
    public Chunk[,] gridArray;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    Color sand;
    Color land;
    public List<Agent> agents;
    
    void OnEnable() {
        isMenu = false;
        osn = new OpenSimplexNoise();

        startingAgents = true;
        // Time
        ticksPerSec = 0;
        time = 0;
        terrainTimeStep = 1f;
        terrainBias = 0;
        terrainUpdate = 250;
        // Terrain
        cols = 150;
        rows = 75;
        noiseScale = 15f;
        seaLevel = 0.45f;
        yScale = 5f;
        seaBorder = 10;
        // Food
        grassSpawnAmount = 45f;
        grassSpawnRate = 20;
        eatSpeed = 1f;
        hungerLoss = 0.005f;
        underwaterFoodSpawn = true;

        gridArray = new Chunk[0, 0];
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        sand = new Color(0.941f, 0.953f, 0.741f);
        land = new Color(0.263f, 0.157f, 0.094f);
        agents = new List<Agent>();
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

    void step() {
        // Debug.Log("1");
        if(time % grassSpawnRate == 0 && !isMenu)
            spawnGrass();
        // Debug.Log("2");

        // Step bots
        for(int i = agents.Count - 1; i >= 0; i--) {
            agents[i].act(isMenu);
            if(agents[i].hunger <= 0) {
                if(SimulationManager.selectedAgent == agents[i])
                    SimulationManager.selectedAgent = null;
                Destroy(agents[i].agentObj);
                agents[i].chunk.agent = null;
                agents.RemoveAt(i);
            }
        }

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
                if(c < cols && r < rows) {
                    newGridArray[c, r].agent = gridArray[c, r].agent;
                    newGridArray[c, r].food = gridArray[c, r].food;
                } else if(gridArray[c, r].agent != null) {
                    Destroy(gridArray[c, r].agent.agentObj);
                    agents.Remove(gridArray[c, r].agent);
                }
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

                gridArray[c, r].vertex.y = elevation;
                if(gridArray[c, r].vertexObj != null)
                    gridArray[c, r].setVertexPos(yScale);
                vertices[c*rows + r] = new Vector3(c, elevation * yScale, r);

                Color col = Color.Lerp(sand, land, (1 + seaLevel) * elevation - seaLevel);
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
        Color color = Color.Lerp(sand, land, (1 + seaLevel) * gridArray[c, r].vertex.y - seaLevel);
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
            } while((chunk.isWater() || chunk.agent != null) && tries < 100);
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
            if(chunk.food < 1 && (underwaterFoodSpawn || !chunk.isWater())) {
                float add = Mathf.Min(toAdd, 1f - chunk.food);
                toAdd -= add;
                chunk.food += add;

                // Reset the color of the chunk's vertex
                updateVertexColour(Mathf.RoundToInt(chunk.vertex.x), Mathf.RoundToInt(chunk.vertex.z));
            }
            tries++;
        }
        // if(toAdd > 0)
        //     Debug.Log(toAdd + " food could not be spawned");
    }
}