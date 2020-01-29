using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
	public Vector3 vertex;
    public Agent agent;

    public Chunk(Vector3 vertex) {
		this.vertex = vertex;
    }

	public bool isWater() {
        GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        return vertex.y <= gridController.seaLevel;
    }
}
