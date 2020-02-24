using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public GameObject vertexObj;
	public Vector3 vertex;
    public Agent agent;
    public float food;

    public Chunk(int c, int r) {
        this.vertex = new Vector3(c, 0, r);
        this.food = 0.25f;
    }

    public void setVertexPos(float yScale) {
        this.vertexObj.transform.position = new Vector3(vertex.x, vertex.y * yScale, vertex.z);
    }

	public bool isWater() {
        return vertex.y <= GridController.GC.seaLevel;
    }
}
