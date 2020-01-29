using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {
    [Range(0f, 10000f)]
    public float seed;
    [Range(0f, 20f)]
    public float time = 0;
    [Range(0, 200)]
    public int cols = 100;
    [Range(0, 200)]
    public int rows = 50;
    [Range(0f, 100f)]
    public float noiseScale = 10f;
    [Range(0f, 1f)]
    public float seaLevel = 0.4f;
    [Range(1f, 10f)]
    public float yScale = 3f;
    [Range(0, 50)]
    public int seaBorder = 10;
    public Chunk[,] gridArray;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    public List<Agent> agents;
    public Agent selectedAgent;
    public GameObject AgentPannel;
    OpenSimplexNoise osn;

    void Start() {
        osn = new OpenSimplexNoise();
        seed = Random.Range(0, 10000);
        gridArray = new Chunk[0, 0];
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        agents = new List<Agent>();
        AgentPannel.SetActive(false);

        updateGrid();

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
            gridArray = new Chunk[cols, rows];
            vertices = new Vector3[cols * rows];

            for(int c = 0; c < cols; c++) {
                for(int r = 0; r < rows; r++) {
                    Vector3 vertex = new Vector3(c, 0, r);
                    vertices[c*rows + r] = vertex;
                    gridArray[c, r] = new Chunk(vertex);
                }
            }

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
        // Set the size and position of chunk objects
        for(int c = 0; c < cols; c++) {
            for(int r = 0; r < rows; r++) {
                float elevation = ((float)osn.Evaluate(c / noiseScale + seed, r / noiseScale + seed, time) + 1) / 2;

                int distFromBorder = Mathf.Min(c + 1, r + 1, cols - c, rows - r);
                if(distFromBorder < seaBorder)
                    elevation *= (float)distFromBorder / seaBorder;

                gridArray[c, r].vertex.y = elevation;
                vertices[c*rows + r].y = elevation * yScale;

                // chunk.transform.position = new Vector3(c, yScale * elevation / 2, r);
                // chunk.transform.localScale = new Vector3(1f, yScale * elevation, 1f);

                float col = Mathf.Clamp(0.3f + 0.7f * ((elevation - seaLevel) / (1f - seaLevel)), 0, 1);
                // chunk.GetComponent<Renderer>().material.SetColor("_Color", new Color(1f - col, 1f - col * 0.6f, 1f - col));
            }
        }
        // Set up water plane
        transform.Find("Water").transform.position = new Vector3(0, yScale * seaLevel, 0);
        transform.Find("Water").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        // Set up sea floor plane
        transform.Find("Sea Floor").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        float seaFloorCol = Mathf.Clamp(0.3f - 0.7f * seaLevel / (1f - seaLevel), 0, 1);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.SetColor("_Color", new Color(1f - seaFloorCol, 1f - seaFloorCol * 0.6f, 1f - seaFloorCol));
    
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}