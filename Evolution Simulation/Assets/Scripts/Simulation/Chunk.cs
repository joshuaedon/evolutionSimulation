using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public GameObject vertexObj;
	public int xPos;
	public int zPos;
	public float yPos;
	public float yOffset;
    public Agent agent;
    public float food;

    public Chunk(int c, int r) {
    	this.xPos = c;
		this.zPos = r;
		this.yPos = 0;
		this.yOffset = 0;
        this.food = 0.25f;
    }

    public void setVertexPos(float yScale) {
        this.vertexObj.transform.position = new Vector3(xPos, Mathf.Clamp(yPos + yOffset, 0f, 1f) * yScale, zPos);
    }

	public bool isWater() {
        return yPos + yOffset <= GridController.GC.seaLevel;
    }
}
