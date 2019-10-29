using UnityEngine;

public class GridController : MonoBehaviour {
    public float seed;
    public int cols = 100;
    public int rows = 50;
    public float scale = 10f;
    public float seaLevel = 0.4f;
    public int seaBorder = 5;
    GameObject[,] gridArray;
    OpenSimplexNoise osn;

    void Start() {
        osn = new OpenSimplexNoise();
        seed = Random.Range(0, 10000);

        gridArray = new GameObject[cols, rows];
        GameObject referenceChunk = (GameObject)Instantiate(Resources.Load("Chunk"));
        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                GameObject chunk = (GameObject)Instantiate(referenceChunk, transform);
                gridArray[c, r] = chunk;
            }
        }
        updateGrid();
        Destroy(referenceChunk);

        transform.position = new Vector3(-(cols - 1) / 2, 0, -(rows - 1) / 2);

        transform.Find("Water").transform.position = new Vector3(0, seaLevel, 0);
        transform.Find("Water").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);

        transform.Find("Sea Floor").transform.localScale = new Vector3((cols + CameraController.panLimit + 1000) / 10, 1, (rows + CameraController.panLimit + 1000) / 10);
        float seaFloorCol = Mathf.Clamp(0.3f - 0.7f * seaLevel / (1f - seaLevel), 0, 1);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.color = new Color(1f - seaFloorCol, 1f - seaFloorCol * 0.6f, 1f - seaFloorCol);
    }

    private void OnValidate() {
        if(gridArray != null)
            updateGrid();
    }

    void updateGrid() {
        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                GameObject chunk = gridArray[c, r];

                float elevation = ((float)osn.Evaluate(c / scale, r / scale, seed) + 1) / 2;
                    
                int distFromBorder = Mathf.Min(c + 1, r + 1, cols - c, rows - r);
                if(distFromBorder < seaBorder)
                    elevation *= (float)distFromBorder / seaBorder;
                chunk.transform.position = new Vector3(c, elevation / 2, r);
                chunk.transform.localScale = new Vector3(1f, elevation, 1f);

                float col = Mathf.Clamp(0.3f + 0.7f * ((elevation - seaLevel) / (1f - seaLevel)), 0, 1);
                chunk.GetComponent<Renderer>().material.color = new Color(1f - col, 1f - col * 0.6f, 1f - col);
            }
        }
    }
}