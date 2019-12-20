using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent {
    public GameObject agentObj;
    public Chunk chunk;
    NeuralNetwork network;
    int dir;

    public Agent(GameObject agentObj, Chunk chunk) {
        this.agentObj = agentObj;
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(new int[] {1, 10, 4});

        this.dir = Random.Range(0, 4);
    }

    void moveObj() {
        GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        float elivation = (float)gridController.gridArray[chunk.col, chunk.row].elevation;
        agentObj.transform.position = new Vector3(chunk.col, gridController.yScale * elivation + 0.5f, chunk.row);
    }

    public void loadInputs() {
        float[] inputs = new float[1];
        inputs[0] = Random.Range(0f, 1f);
        network.loadInputs(inputs);
    }

    public void act(int a) {
        float leftOut = network.returnOutput("Left", false);
        float rightOut = network.returnOutput("Right", false);
        float stepOut = network.returnOutput("Step", false);
        float stayOut = network.returnOutput("Stay", false);
        if(leftOut > Mathf.Max(Mathf.Max(rightOut, stepOut), stayOut))
            turnLeft();
        else if(rightOut > Mathf.Max(stepOut, stayOut))
            turnRight();
        else if(stepOut > stayOut)
            stepForward();
        if(a == 0) {
            Debug.Log(leftOut + ", " + rightOut + ", " + stepOut + ", " + stayOut);
            if(leftOut > Mathf.Max(Mathf.Max(rightOut, stepOut), stayOut))
                Debug.Log("left");
            else if(rightOut > Mathf.Max(stepOut, stayOut))
                Debug.Log("right");
            else if(stepOut > stayOut)
                Debug.Log("step");
            else
                Debug.Log("stay");
        }
    }

    void turnLeft() {
        this.dir = (dir + 3) % 4;
    }

    void turnRight() {
        this.dir = (dir + 1) % 4;
    }

    void stepForward() {
        int newCol = chunk.col;
        int newRow = chunk.row;
        switch(this.dir) {
            case 0: newCol++; break;
            case 1: newRow++; break;
            case 2: newCol--; break;
            case 3: newRow--; break;
        }
        GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        if(newCol >= 0 && newRow >= 0 && newCol < gridController.cols && newRow < gridController.rows) {
            Chunk newChunk = gridController.gridArray[newCol, newRow];
            if(newChunk.agent == null) {
                // Step Forward
                this.chunk.agent = null;
                this.chunk = newChunk;
                newChunk.agent = this;
                moveObj();
            }
        }
    }
}