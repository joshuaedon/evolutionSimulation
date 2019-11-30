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
    public OpenSimplexNoise osn;

    void Start() {
        osn = new OpenSimplexNoise();
        seed = Random.Range(0, 10000);

        gridArray = new Chunk[cols, rows];
        GameObject referenceChunk = (GameObject)Instantiate(Resources.Load("Chunk"));
        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                GameObject chunk = (GameObject)Instantiate(referenceChunk, transform);
                gridArray[c, r] = new Chunk(chunk, c, r);
            }
        }
        updateGrid();
        Destroy(referenceChunk);



        GameObject referenceAgent = (GameObject)Instantiate(Resources.Load("Agent"));
        for(int i = 0; i < 20; i++) {
            int col, row;
            int count = 0;
            do {
                col = Random.Range(0, cols);
                row = Random.Range(0, rows);
                count++;
            } while((gridArray[col, row].isWater() || gridArray[col, row].agent != null) && count < 1000);
            if(count < 1000) {
                GameObject agentObj = (GameObject)Instantiate(referenceAgent, transform);
                Agent agent = new Agent(agentObj, col, row);
                gridArray[col, row].agent = agent;
            } else {
                Debug.Log("Agent could not be spawned");
            }
        }
        Destroy(referenceAgent);
    }

    private void OnValidate() {
        if(gridArray != null)
            updateGrid();
    }

    void updateGrid() {
        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                GameObject chunk = gridArray[c, r].chunkObj;

                float elevation = ((float)osn.Evaluate(c / noiseScale + seed, r / noiseScale + seed, time) + 1) / 2;

                int distFromBorder = Mathf.Min(c + 1, r + 1, cols - c, rows - r);
                if(distFromBorder < seaBorder)
                    elevation *= (float)distFromBorder / seaBorder;

                chunk.transform.position = new Vector3(c, yScale * elevation / 2, r);
                chunk.transform.localScale = new Vector3(1f, yScale * elevation, 1f);

                float col = Mathf.Clamp(0.3f + 0.7f * ((elevation - seaLevel) / (1f - seaLevel)), 0, 1);
                chunk.GetComponent<Renderer>().material.color = new Color(1f - col, 1f - col * 0.6f, 1f - col);

                gridArray[c, r].elevation = elevation;
            }
        }

        transform.Find("Water").transform.position = new Vector3(0, yScale * seaLevel, 0);
        transform.Find("Water").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);

        transform.Find("Sea Floor").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        float seaFloorCol = Mathf.Clamp(0.3f - 0.7f * seaLevel / (1f - seaLevel), 0, 1);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.color = new Color(1f - seaFloorCol, 1f - seaFloorCol * 0.6f, 1f - seaFloorCol);
    }
}