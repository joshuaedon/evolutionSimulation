using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {
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
                //float elevation = Mathf.PerlinNoise(c*scale, r*scale);
                //chunk.transform.localScale = new Vector3(1f, elevation, 1f);
                /*if(elevation >= 0.5)
                    chunk.GetComponent<MeshRenderer>().material = ;*/
                gridArray[c, r] = chunk;
            }
        }
        Destroy(referenceChunk);

        transform.position = new Vector3(-(cols-1)/2, 0, -(rows-1)/2);
        transform.eulerAngles -= new Vector3(10, 10, 0);
        transform.position += new Vector3(0, 0, 32);
    }
}
