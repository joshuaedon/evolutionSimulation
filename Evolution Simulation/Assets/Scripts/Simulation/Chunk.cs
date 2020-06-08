using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public GameObject chunkObj;
    public GameObject vertexObj;
	public int col;
	public int row;
	public float elevation;
	public float elevationOffset;
    public Agent agent;
    public float food;

    public Chunk(GameObject chunkObj, int c, int r) {
    	this.chunkObj = chunkObj;
    	this.col = c;
		this.row = r;
    }

	public bool isWater() {
        return elevation + elevationOffset <= SettingsPanel.seaLevel;
    }

    public void updateColour() {
        if(isWater()) {
	      	float col = Mathf.Max(0.01f, 0.643f * (this.elevation - 0.1f) / (SettingsPanel.seaLevel - 0.1f));
        	this.chunkObj.GetComponent<SpriteRenderer>().color = new Color(col, col, 1f);
	    } else {
	      	float col = 0.3f + 0.7f * (this.elevation - SettingsPanel.seaLevel) / (1 - SettingsPanel.seaLevel);
	      	this.chunkObj.GetComponent<SpriteRenderer>().color = new Color(1f - col, 1f - col*0.608f, 1f - col);
	    }
    }
}
