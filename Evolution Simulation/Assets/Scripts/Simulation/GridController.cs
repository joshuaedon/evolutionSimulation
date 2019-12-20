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
    List<Agent> agents;
    OpenSimplexNoise osn;

    void Start() {
        osn = new OpenSimplexNoise();
        seed = Random.Range(0, 10000);
        gridArray = new Chunk[0, 0];
        agents = new List<Agent>();

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
        if(cols != gridArray.GetLength(0) || rows != gridArray.GetLength(1)) {
            for(int c = 0; c < gridArray.GetLength(0); c++) {
                for(int r = 0; r < gridArray.GetLength(1); r++) {
                    Destroy(gridArray[c, r].chunkObj);
                }
            }
            for(int i = agents.Count - 1; i >= 0; i--) {
                if(agents[i].chunk.col > cols || agents[i].chunk.row > rows) {
                    Destroy(agents[i].agentObj);
                    agents.RemoveAt(i);
                }
            }
            
            gridArray = new Chunk[cols, rows];

            GameObject referenceChunk = (GameObject)Instantiate(Resources.Load("Simulation/Chunk"));
            for(int c = 0; c < cols; c++) {
                for(int r = 0; r < rows; r++) {
                    GameObject chunk = (GameObject)Instantiate(referenceChunk, transform);
                    gridArray[c, r] = new Chunk(chunk, c, r);
                }
            }
            Destroy(referenceChunk);
        }


        // Create chunk objects and set their size and position
        for(int c = 0; c < cols; c++) {
            for(int r = 0; r < rows; r++) {
                GameObject chunk = gridArray[c, r].chunkObj;

                float elevation = ((float)osn.Evaluate(c / noiseScale + seed, r / noiseScale + seed, time) + 1) / 2;
                gridArray[c, r].elevation = elevation;

                int distFromBorder = Mathf.Min(c + 1, r + 1, cols - c, rows - r);
                if(distFromBorder < seaBorder)
                    elevation *= (float)distFromBorder / seaBorder;

                chunk.transform.position = new Vector3(c, yScale * elevation / 2, r);
                chunk.transform.localScale = new Vector3(1f, yScale * elevation, 1f);

                float col = Mathf.Clamp(0.3f + 0.7f * ((elevation - seaLevel) / (1f - seaLevel)), 0, 1);
                chunk.GetComponent<Renderer>().material.color = new Color(1f - col, 1f - col * 0.6f, 1f - col);
            }
        }
        // Set up water plane
        transform.Find("Water").transform.position = new Vector3(0, yScale * seaLevel, 0);
        transform.Find("Water").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        // Set up sea floor plane
        transform.Find("Sea Floor").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        float seaFloorCol = Mathf.Clamp(0.3f - 0.7f * seaLevel / (1f - seaLevel), 0, 1);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.color = new Color(1f - seaFloorCol, 1f - seaFloorCol * 0.6f, 1f - seaFloorCol);
    }

    void createGrid() {

    }
}