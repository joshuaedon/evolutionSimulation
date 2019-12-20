using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public GameObject chunkObj;
	public int col;
	public int row;
	public float elevation;
    public Agent agent;

    public Chunk(GameObject chunkObj, int col, int row) {
    		this.col = col;
    		this.row = row;
        this.chunkObj = chunkObj;
    }

	public bool isWater() {
        GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        return elevation <= gridController.seaLevel;
    }
}
