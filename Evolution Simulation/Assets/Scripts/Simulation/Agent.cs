using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent {
    public GameObject agentObj;
    public Chunk chunk;
    public NeuralNetwork network;
    int dir;
    public bool dead;
    public float hunger;

    public Agent(GameObject agentObj, Chunk chunk) {
        this.agentObj = agentObj;
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(new int[] {1, 10, 4});

        this.dir = Random.Range(0, 4);
        this.dead = false;
        this.hunger = 1;
    }

    public void moveObj() {
        agentObj.transform.position = new Vector3(chunk.vertex.x, GridController.GC.yScale * chunk.vertex.y + 0.5f, chunk.vertex.z);
    }

    public void loadInputs() {
        float[] inputs = new float[1];
        inputs[0] = Random.Range(0f, 1f);
        network.loadInputs(inputs);
    }

    public void act(bool isMenu) {
        if(!isMenu) {
        	// Loose hunger
        	this.hunger -= GridController.GC.hungerLoss;

        	// Move forward, turn left, right or eat depending on the agents NN outputs
            float forwardsOut = network.returnOutput("Forwards", false);
            float leftOut = network.returnOutput("Left", false);
            float rightOut = network.returnOutput("Right", false);
            float eatOut = network.returnOutput("Eat", false);
            if(forwardsOut > Mathf.Max(Mathf.Max(leftOut, rightOut), eatOut))
                stepForward(isMenu);
            else if(leftOut > Mathf.Max(rightOut, eatOut))
                turnLeft();
            else if(rightOut > eatOut)
                turnRight();
            else
            	eat();

            loadInputs();
            if(this == SimulationManager.selectedAgent && SimulationManager.NNFlow)
            	network.setConnectionColours();
        } else {
        	// If the agent is in the menu screen, randomely move forward, turn left or right
            float rand = Random.Range(0f, 1f);
            if(rand > 2.0f/3.0f)
                turnLeft();
            else if(rand > 1.0f/3.0f)
                turnRight();
            else
                stepForward(isMenu);
        }
    }

    void turnLeft() {
        this.dir = (dir + 3) % 4;
    }

    void turnRight() {
        this.dir = (dir + 1) % 4;
    }

    void stepForward(bool isMenu) {
        int newCol = Mathf.RoundToInt(chunk.vertex.x);
        int newRow = Mathf.RoundToInt(chunk.vertex.z);
        switch(this.dir) {
            case 0: newCol++; break;
            case 1: newRow++; break;
            case 2: newCol--; break;
            case 3: newRow--; break;
        }
        if(newCol >= 0 && newRow >= 0 && newCol < GridController.GC.cols && newRow < GridController.GC.rows) {
            Chunk newChunk = GridController.GC.gridArray[newCol, newRow];
            if(newChunk.agent == null && (!isMenu || !newChunk.isWater() || this.chunk.isWater())) {
                // Step Forward
                this.chunk.agent = null;
                this.chunk = newChunk;
                newChunk.agent = this;
                moveObj();
            }
        }
    }

    void eat() {
    	float amount = Mathf.Min(GridController.GC.eatSpeed, Mathf.Min(1f - hunger, chunk.food));
    	hunger += amount;
    	chunk.food -= amount;
    	// Reset the color of the chunk's vertex
        GridController.GC.updateVertexColour(Mathf.RoundToInt(chunk.vertex.x), Mathf.RoundToInt(chunk.vertex.z));
    }
}
