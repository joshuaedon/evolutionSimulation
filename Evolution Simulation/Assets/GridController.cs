using UnityEngine;

public class GridController : MonoBehaviour {
    public static int cols = 50;
    public static int rows = 30;
    public float scale = 0.01f;
    GameObject[,] gridArray;

    void Start() {
        GameObject referenceChunk = (GameObject)Instantiate(Resources.Load("Chunk"));
        gridArray = new GameObject[cols, rows];

        for(int r = 0; r < rows; r++) {
            for(int c = 0; c < cols; c++) {
                GameObject chunk = (GameObject)Instantiate(referenceChunk, transform);
                chunk.transform.position = new Vector3(c, 0, r);

                float elevation = Mathf.PerlinNoise(c * 1f / cols, r * 1f / rows);
                chunk.transform.localScale = new Vector3(1f, elevation, 1f);

                float col = 0.3f + 0.7f * ((elevation - 0.5f) / (1f - 0.5f));
                chunk.GetComponent<Renderer>().material.color = new Color(1f - col, 1f - col * 0.6f, 1f - col);
                gridArray[c, r] = chunk;
            }
        }
        Destroy(referenceChunk);

        transform.position = new Vector3(-(cols - 1) / 2, 0, -(rows - 1) / 2);
    }
}
