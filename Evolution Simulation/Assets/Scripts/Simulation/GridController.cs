using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {
    OpenSimplexNoise osn;
    // Grid attributes
    [Range(0f, 10000f)]
    public float seed;
    [Range(0f, 20f)]
    public float time = 0;
    [Range(0, 200)]
    public static int cols = 100;
    [Range(0, 200)]
    public static int rows = 50;
    [Range(0f, 100f)]
    public float noiseScale = 10f;
    [Range(0f, 1f)]
    public static float seaLevel = 0.4f;
    [Range(1f, 10f)]
    public static float yScale = 3f;
    [Range(0, 50)]
    public int seaBorder = 10;
    [Range(0f, 10f)]
    public static float maxFood = 10;
    // Grid state
    public static Chunk[,] gridArray;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    public static List<Agent> agents;
    // User variables
    public static Agent selectedAgent;
    public bool showVertices = false;
    // Game objects
    public GameObject AgentPannel;

    void Start() {
        osn = new OpenSimplexNoise();

        seed = Random.Range(0, 10000);
        time = 0;

        gridArray = new Chunk[0, 0];
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        agents = new List<Agent>();

        selectedAgent = null;
        showVertices = false;

        AgentPannel.SetActive(false);

        updateGrid();

        // Spawn agents
        GameObject referenceAgent = (GameObject)Instantiate(Resources.Load("Simulation/Agent"));
        for(int i = 0; i < 20; i++) {
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

    void Update() {
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
                AgentPannel.SetActive(true);
            } else {
                selectedAgent = null;
                AgentPannel.SetActive(false);
            }
        }
        // Show vertices
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

        // Step bots
        for(int i = agents.Count - 1; i >= 0; i--)
            agents[i].act(i);
        // Load new agent inputs
        foreach(Agent a in agents)
            a.loadInputs();

        // time += 0.001f;
        // updateGrid();
    }

    private void OnValidate() {
        if(gridArray != null) {
            updateGrid();
        }
    }

    void updateGrid() {
        // Delete and create chunk objects if grid dimensions have been changed
        if(cols != gridArray.GetLength(0) || rows != gridArray.GetLength(1)) {
            for(int c = 0; c < gridArray.GetLength(0); c++) {
                for(int r = 0; r < gridArray.GetLength(1); r++) {
                    Destroy(gridArray[c, r].vertexObj);
                }
            }

            gridArray = new Chunk[cols, rows];
            vertices = new Vector3[cols * rows];

            GameObject referenceVertex = (GameObject)Instantiate(Resources.Load("Simulation/Vertex"));
            for(int c = 0; c < cols; c++) {
                for(int r = 0; r < rows; r++) {
                    GameObject vertexObj = (GameObject)Instantiate(referenceVertex, transform);
                    gridArray[c, r] = new Chunk(vertexObj, c, r);
                }
            }
            Destroy(referenceVertex);

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
        Color[] colours = new Color[cols * rows];
        // Set the positions and colours of vertices
        Color sand = new Color(0.941f, 0.953f, 0.741f);
        Color land = new Color(0.263f, 0.157f, 0.094f);
        for(int c = 0; c < cols; c++) {
            for(int r = 0; r < rows; r++) {
                float elevation = ((float)osn.Evaluate(c / noiseScale + seed, r / noiseScale + seed, time) + 1) / 2;

                int distFromBorder = Mathf.Min(c + 1, r + 1, cols - c, rows - r);
                if(distFromBorder < seaBorder)
                    elevation *= (float)distFromBorder / seaBorder;

                gridArray[c, r].setElevation(elevation, yScale);
                vertices[c*rows + r] = new Vector3(c, elevation * yScale, r);


                // float col = Mathf.Clamp(0.3f + 0.7f * ((elevation - seaLevel) / (1f - seaLevel)), 0, 1);
                Color col = Color.Lerp(sand, land, (1 + seaLevel) * elevation - seaLevel);
                colours[c*rows + r] = Color.Lerp(col, new Color(col.r, 1, col.b), gridArray[c, r].food / maxFood);//new Color(1f - col, 1f - col * 0.6f, 1f - col);
            }
        }
        // Set up water plane
        transform.Find("Water").transform.position = new Vector3(0, yScale * seaLevel, 0);
        transform.Find("Water").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        // Set up sea floor plane
        transform.Find("Sea Floor").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        // float seaFloorCol = Mathf.Clamp(0.3f - 0.7f * seaLevel / (1f - seaLevel), 0, 1);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.SetColor("_Color", sand/*new Color(1f - seaFloorCol, 1f - seaFloorCol * 0.6f, 1f - seaFloorCol)*/);
    
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colours;
        mesh.RecalculateNormals();

        MeshCollider meshc = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
    }
}