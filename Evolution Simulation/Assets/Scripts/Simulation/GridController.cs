using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour {
    OpenSimplexNoise osn;
    //// Grid attributes
    [Range(0, 4)]
    public float ticksPerSec = 0;
    public float framesPerTick;
    public static string tickFrame;
    [Range(0, 10000)]
    public static int time = 0;
    // []
    public float terrainTimeStep = 0.0000001f;
    // []
    public int terrainTimeUpdate = 250;
    // Terrain
    [Range(0, 200)]
    public static int cols = 150;
    [Range(0, 200)]
    public static int rows = 75;
    [Range(1f, 100f)]
    public float noiseScale = 15f;
    [Range(0f, 1f)]
    public static float seaLevel = 0.45f;
    [Range(1f, 10f)]
    public static float yScale = 3f;
    [Range(0, 50)]
    public int seaBorder = 10;
    // Food
    [Range(0f, 10f)]
    public float maxFood = 10f;
    // []
    public float foodSpread = 0.05f;
    // [] Agents
    public int startingAgents = 50;
    //// Grid state
    public static Chunk[,] gridArray;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Color[] colours;
    public static List<Agent> agents;
    //// User variables
    public static Agent selectedAgent;
    public bool showVertices = false;
    //// Game objects
    public GameObject StatsPanel;
    public GameObject AgentPanel;
    public GameObject TickSpeedText;

    void Start() {
        osn = new OpenSimplexNoise();

        time = 0;

        gridArray = new Chunk[0, 0];
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        agents = new List<Agent>();

        selectedAgent = null;
        showVertices = false;

        StatsPanel = GameObject.Find("StatsPanel");
        StatsPanel.SetActive(true);
        AgentPanel = GameObject.Find("AgentPanel");
        AgentPanel.SetActive(false);
        TickSpeedText = GameObject.Find("TickSpeedText");

        createGrid();

        // Spawn agents
        GameObject referenceAgent = (GameObject)Instantiate(Resources.Load("Simulation/Agent"));
        for(int i = 0; i < startingAgents; i++) {
            Chunk chunk;
            int count = 0;
            do {
                int col = Random.Range(0, cols);
                int row = Random.Range(0, rows);
                chunk = gridArray[col, row];
                count++;
            } while((chunk.isWater() || chunk.agent != null) && count < 1000);
            if(count < 1000) {
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

    void step() {
        // Step bots
        for(int i = agents.Count - 1; i >= 0; i--)
            agents[i].act(i);

        // Load new agent inputs
        foreach(Agent a in agents)
            a.loadInputs();

        // Update grid
        if(time % Mathf.Ceil(terrainTimeUpdate / (terrainTimeStep * 10000000)) == 0) {
            Debug.Log(time);
            updateGrid();
        }

        // Increment time
        time += 1;
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
                foreach(Agent a in agents) {
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

        // Display vertices
        if(showVertices && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Casts the ray and get the first game object hit
            Physics.Raycast(ray, out hit);

            for(int c = 0; c < gridArray.GetLength(0); c++) {
                for(int r = 0; r < gridArray.GetLength(1); r++) {
                    if(Vector3.Distance(hit.point, gridArray[c, r].vertex) < 5)
                        gridArray[c, r].vertexObj.SetActive(true);
                    else
                        gridArray[c, r].vertexObj.SetActive(false);
                }
            }
        }

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

    public void adjustTickSpeed(float speed) {
        if(speed == 0)
            this.ticksPerSec = 0;
        else
            this.ticksPerSec = Mathf.Pow(10, speed-1);
        framesPerTick = 1;
        TickSpeedText.GetComponent<Text>().text = Mathf.Round(ticksPerSec * 100f) / 100f + " ticks/sec";
    }

    private void OnValidate() {
        if(gridArray != null) {
            createGrid();
        }
    }

    void updateGrid() {
        // Set the positions and colours of vertices
        colours = new Color[cols * rows];
        Color sand = new Color(0.941f, 0.953f, 0.741f);
        Color land = new Color(0.263f, 0.157f, 0.094f);
        for(int c = 0; c < cols; c++) {
            for(int r = 0; r < rows; r++) {
                float elevation = (
                                    (float)osn.Evaluate(c / noiseScale, r / noiseScale, time * terrainTimeStep * terrainTimeUpdate) + 
                                    (float)osn.Evaluate(c / (noiseScale / 4), r / (noiseScale / 4), 1000 + time * terrainTimeStep * terrainTimeUpdate / 2) / 8 + 
                                    1
                                  ) / 2;

                int distFromBorder = Mathf.Min(c + 1, r + 1, cols - c, rows - r);
                if(distFromBorder < seaBorder)
                    elevation *= -Mathf.Pow((float)distFromBorder / seaBorder - 1, 2) + 1;

                gridArray[c, r].setElevation(elevation, yScale);
                vertices[c*rows + r] = new Vector3(c, elevation * yScale, r);

                Color col = Color.Lerp(sand, land, (1 + seaLevel) * elevation - seaLevel);
                colours[c*rows + r] = Color.Lerp(col, new Color(col.r, 1, col.b), gridArray[c, r].food / maxFood);
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

    void createGrid() {
        // Set up water plane
        transform.Find("Water").transform.position = new Vector3(0, yScale * seaLevel, 0);
        transform.Find("Water").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);

        // Set up sea floor plane
        transform.Find("Sea Floor").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.SetColor("_Color", new Color(0.941f, 0.953f, 0.741f));

        // Delete and create grid objects if grid dimensions have been changed
        if(cols != gridArray.GetLength(0) || rows != gridArray.GetLength(1)) {
            // Destroy old vertex objects
            for(int c = 0; c < gridArray.GetLength(0); c++) {
                for(int r = 0; r < gridArray.GetLength(1); r++) {
                    Destroy(gridArray[c, r].vertexObj);
                }
            }

            // Reset chunk and vertex arrays
            gridArray = new Chunk[cols, rows];
            vertices = new Vector3[cols * rows];

            // Create new vertex objects
            GameObject referenceVertex = (GameObject)Instantiate(Resources.Load("Simulation/Vertex"));
            for(int c = 0; c < cols; c++) {
                for(int r = 0; r < rows; r++) {
                    GameObject vertexObj = (GameObject)Instantiate(referenceVertex, transform);
                    gridArray[c, r] = new Chunk(vertexObj, c, r);
                }
            }
            Destroy(referenceVertex);

            // Create mesh triangles
            triangles = new int[(cols-1) * (rows - 1) * 6];
            int v = 0;
            int t = 0;
            for(int c = 0; c < cols-1; c++) {
                for(int r = 0; r < rows-1; r++) {
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
        }

        updateGrid();
    }
}