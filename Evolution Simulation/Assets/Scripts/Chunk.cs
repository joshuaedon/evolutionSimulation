using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
		public int col;
		public int row;
    public GameObject chunk;
    public GameObject agent;

    public Chunk(GameObject chunk, int col, int row) {
    		this.col = col;
    		this.row = row;
        this.chunk = chunk;
    }

}
