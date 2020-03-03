using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent {
    public GameObject agentObj;
    public MeshRenderer MR;
    public Chunk chunk;
    public NeuralNetwork network;
    public int generation;
    public float colour;
    int dir;
    public float hunger;

    public Agent(GameObject agentObj, Chunk chunk) {
        this.agentObj = agentObj;
        this.MR = agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(new int[] {7, 10, 5});
        this.generation = 1;
        this.colour = Random.Range(0f, 1f);
        changeColour(0f);

        this.dir = Random.Range(0, 4);
        // Rotate the agent's object to the correct direction
    	agentObj.transform.Rotate(0f, 90f - dir * 90f, 0f, Space.Self);
        this.hunger = 1;
    }

    public Agent(Chunk chunk, NeuralNetwork n, int generation, float colour) {
        this.agentObj = (GameObject)Transform.Instantiate(Resources.Load("Simulation/Agent"), GridController.GC.transform);
        this.MR = agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(n);
        this.generation = generation;
        this.colour = colour;
		changeColour(this.network.mutate());

        this.dir = Random.Range(0, 4);
        // Rotate the agent's object to the correct direction
    	agentObj.transform.Rotate(0f, 90f - dir * 90f, 0f, Space.Self);
        this.hunger = 1;
    }

    public void moveObj() {
        agentObj.transform.position = new Vector3(chunk.vertex.x, GridController.GC.yScale * chunk.vertex.y + 0.5f, chunk.vertex.z);
    }

    public void loadInputs() {
        float[] inputs = new float[7];
        inputs[0] = Random.Range(0f, 1f);
        inputs[1] = this.hunger;
        inputs[2] = this.chunk.food;
        inputs[3] = this.chunk.isWater() ? 1 : 0;
        Chunk newChunk = getNewChunk();
        inputs[4] = (newChunk != null) ? newChunk.food : 0f;
        inputs[5] = (newChunk == null || newChunk.isWater()) ? 1 : 0;
        inputs[6] = (newChunk == null || newChunk.agent == null) ? 0 : 1;
        network.loadInputs(inputs);
    }

    public void act(bool isMenu) {
        if(!isMenu) {
        	// Loose hunger
        	this.hunger -= GridController.GC.hungerLoss;
        	if(chunk.isWater()) {
        		network.mutateValue(1f);
        		changeColour(1f);
        		hunger -= 0.1f;
        	}

        	// Move forward, turn left, right or eat depending on the agents NN outputs
            float forwardsOut = network.returnOutput(0);
            float leftOut = network.returnOutput(1);
            float rightOut = network.returnOutput(2);
            float eatOut = network.returnOutput(3);
            float reproduceOut = network.returnOutput(4);
            if(forwardsOut > Mathf.Max(Mathf.Max(Mathf.Max(leftOut, rightOut), eatOut), reproduceOut))
                stepForward(isMenu);
            else if(leftOut > Mathf.Max(Mathf.Max(rightOut, eatOut), reproduceOut))
                turnLeft();
            else if(rightOut > Mathf.Max(eatOut, reproduceOut))
                turnRight();
            else if(eatOut > reproduceOut)
            	eat();
            else if(hunger > 0)
            	reproduce();

            loadInputs();
            if(this == SimulationManager.selectedAgent && (SimulationManager.NNFlow || chunk.isWater()))
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
        agentObj.transform.Rotate(0f, 90, 0f, Space.Self);
    }

    void turnRight() {
        this.dir = (dir + 1) % 4;
        agentObj.transform.Rotate(0f, -90, 0f, Space.Self);
    }

    void stepForward(bool isMenu) {
        Chunk newChunk = getNewChunk();
        if(newChunk != null && newChunk.agent == null && (!isMenu || !newChunk.isWater() || this.chunk.isWater())) {
            // Step Forward
            this.chunk.agent = null;
            this.chunk = newChunk;
            newChunk.agent = this;
            moveObj();
        }
    }

    void eat() {
    	float amount = Mathf.Min(GridController.GC.eatSpeed, Mathf.Min(1f - hunger, chunk.food));
    	hunger += amount;
    	chunk.food -= amount;
    	// Reset the color of the chunk's vertex
        GridController.GC.updateVertexColour(Mathf.RoundToInt(chunk.vertex.x), Mathf.RoundToInt(chunk.vertex.z));
    }
    
    public void reproduce() {
    	List<Chunk> chunks = new List<Chunk>();
    	int x = Mathf.RoundToInt(chunk.vertex.x);
    	int z = Mathf.RoundToInt(chunk.vertex.z);
    	for(int i = Mathf.Max(x-1, 0); i <= Mathf.Min(x+1, GridController.GC.gridArray.GetLength(0)-1); i++) {
    		for(int j = Mathf.Max(z-1, 0); j <= Mathf.Min(z+1, GridController.GC.gridArray.GetLength(1)-1); j++) {
    			Chunk c = GridController.GC.gridArray[i, j];
    			if(c.agent == null)
    				chunks.Add(c);
    		}
    	}
    	if(chunks.Count > 0) {
    		Chunk c = chunks[Random.Range(0, chunks.Count)];
    		Agent offspring = new Agent(c, network, generation+1, colour);
    		this.hunger /= 1f + Mathf.Min(GridController.GC.agents.Count / 200f, 1f);
    		offspring.hunger = this.hunger;
    		c.agent = offspring;
    		GridController.GC.agents.Add(offspring);
    	}
    }

    public void changeColour(float amount) {
    	colour = (colour + Random.Range(-amount/5f, amount/5f) + 1000000f) % 1f;
    	if(SimulationManager.selectedAgent != this)
    		display();
    }

    public void display() {
    	MR.material.color = Color.HSVToRGB(colour, 1f, 1f);
    }

    Chunk getNewChunk() {
		int newCol = Mathf.RoundToInt(chunk.vertex.x);
        int newRow = Mathf.RoundToInt(chunk.vertex.z);
        switch(this.dir) {
            case 0: newCol++; break;
            case 1: newRow++; break;
            case 2: newCol--; break;
            case 3: newRow--; break;
        }
        if(newCol >= 0 && newRow >= 0 && newCol < GridController.GC.cols && newRow < GridController.GC.rows) {
            return GridController.GC.gridArray[newCol, newRow];
        }
        return null;
    }
}
