using UnityEngine;

public class GridController : MonoBehaviour {
    public static int cols = 50;
    public static int rows = 30;
    public float scale = 5f;
    public float seaLevel = 0.4f;
    public int seaBorder = 3;
    GameObject[,] gridArray;

    void Start() {
        GameObject referenceChunk = (GameObject)Instantiate(Resources.Load("Chunk"));
        gridArray = new GameObject[cols, rows];

        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                GameObject chunk = (GameObject)Instantiate(referenceChunk, transform);

                float elevation = Mathf.PerlinNoise((float)c / cols * scale, (float)r / rows * scale);
                chunk.transform.position = new Vector3(c, elevation / 2, r);
                chunk.transform.localScale = new Vector3(1f, elevation, 1f);

                float col = Mathf.Clamp(0.3f + 0.7f * ((elevation - seaLevel) / (1f - seaLevel)), 0, 1);
                chunk.GetComponent<Renderer>().material.color = new Color(1f - col, 1f - col * 0.6f, 1f - col);
                gridArray[c, r] = chunk;
            }
        }
        Destroy(referenceChunk);

        transform.Find("Water").transform.position = new Vector3(0, seaLevel, 0);
        float seaFloorCol = Mathf.Clamp(0.3f - 0.7f * seaLevel / (1f - seaLevel), 0, 1);
        transform.Find("Sea Floor").GetComponent<Renderer>().material.color = new Color(1f - seaFloorCol, 1f - seaFloorCol * 0.6f, 1f - seaFloorCol);

        transform.position = new Vector3(-(cols - 1) / 2, 0, -(rows - 1) / 2);
    }
}
