using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public GameObject vertexObj;
	public Vector3 vertex;
    public Agent agent;
    public float food;

    public Chunk(GameObject vertexObj, int c, int r) {
        this.vertexObj = vertexObj;
        vertexObj.SetActive(false);
        this.vertex = new Vector3(c, 0, r);
        this.food = 10;
    }

    public void setElevation(float elevation, float yScale) {
        this.vertexObj.transform.position = new Vector3(vertex.x, elevation * yScale, vertex.z);
        this.vertex.y = elevation;
    }

	public bool isWater() {
        return vertex.y <= GridController.seaLevel;
    }
}
