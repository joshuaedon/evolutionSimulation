using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent {
    public GameObject agentObj;
    public GameObject plumbob;
    public MeshRenderer MR;
    public Chunk chunk;
    public NeuralNetwork network;
    public int generation;
    public int ticksAlive;
    public float colour;
    int dir;
    public float hunger;

    public Agent(GameObject agentObj, Chunk chunk) {
        this.agentObj = agentObj;
        this.plumbob = agentObj.transform.GetChild(2).gameObject;
        this.plumbob.SetActive(false);
        this.MR = agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(new int[] {12, 5});
        this.generation = 1;
        this.ticksAlive = 0;
        this.colour = Random.Range(0f, 1f);
        changeColour(0f);

        this.dir = Random.Range(0, 4);
        // Rotate the agent's object to the correct direction
    	agentObj.transform.Rotate(0f, 90f - dir * 90f, 0f, Space.Self);
        this.hunger = 1;
    }

    public Agent(Chunk chunk, NeuralNetwork n, int generation, float colour) {
        this.agentObj = (GameObject)Transform.Instantiate(Resources.Load("Simulation/Agent"), GridController.GC.transform);
        this.plumbob = agentObj.transform.GetChild(2).gameObject;
        this.plumbob.SetActive(false);
        this.MR = agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(n);
        this.generation = generation;
        this.ticksAlive = 0;
        this.colour = colour;
		changeColour(this.network.mutate());

        this.dir = Random.Range(0, 4);
        // Rotate the agent's object to the correct direction
    	agentObj.transform.Rotate(0f, 90f - dir * 90f, 0f, Space.Self);
        this.hunger = 1;
    }

    public void moveObj() {
        agentObj.transform.position = new Vector3(chunk.xPos, GridController.GC.yScale * Mathf.Clamp(chunk.yPos + chunk.yOffset, 0f, 1f) + 0.5f, chunk.zPos);
    }

    public void loadInputs() {
        float[] inputs = new float[12];
        inputs[0] = Random.Range(0f, 1f);
        inputs[1] = this.hunger;
        inputs[2] = this.chunk.food;
        inputs[3] = this.chunk.isWater() ? 1 : 0;
        Chunk newChunk = getNewChunk();
        inputs[4] = (newChunk != null) ? newChunk.food : 0f;
        inputs[5] = (newChunk == null || newChunk.isWater()) ? 1 : 0;
        inputs[6] = (newChunk == null || newChunk.agent == null) ? 0 : 1;
        inputs[7 + highestOutput()] = 1f;
        network.loadInputs(inputs);
    }

    int highestOutput() {
    	float forwardsOut = network.returnOutput(0);
        float leftOut = network.returnOutput(1);
        float rightOut = network.returnOutput(2);
        float eatOut = network.returnOutput(3);
        float reproduceOut = network.returnOutput(4);
    	if(forwardsOut > Mathf.Max(Mathf.Max(Mathf.Max(leftOut, rightOut), eatOut), reproduceOut))
	        return 0;
	    else if(leftOut > Mathf.Max(Mathf.Max(rightOut, eatOut), reproduceOut))
	        return 1;
	    else if(rightOut > Mathf.Max(eatOut, reproduceOut))
	        return 2;
	    else if(eatOut > reproduceOut)
	    	return 3;
	    else if(hunger > 0 && reproduceOut > 0)
	    	return 4;
	    return -5;
    }

    public void act(bool isMenu) {
        if(!isMenu) {
        	// Loose hunger
        	this.hunger -= GridController.GC.hungerLoss + this.network.nodeCount * 0.00001f;
        	if(chunk.isWater()) {
        		network.mutateValue(0.2f);
        		changeColour(0.2f);
        		hunger -= 0.1f;
        	}

        	// Move forward, turn left, right or eat depending on the agents NN outputs
      		int output = highestOutput();
      		switch(output) {
      			case 0:
                	stepForward(isMenu);
      				break;
      			case 1:
                	turnLeft();
      				break;
      			case 2:
                	turnRight();
      				break;
      			case 3:
            		eat();
      				break;
      			case 4:
		            reproduce();
      				break;
      		}

            loadInputs();
        	if(this == SimulationManager.selectedAgent) {
        		if(chunk.isWater()) {
        			SimulationManager.AgentPanel.GetComponent<AgentPanelController>().resetNetwork();
    			} else if(SimulationManager.NNFlow)
        			network.setConnectionColours();
        	}

        	ticksAlive++;
        } else {
        	// If the agent is in the menu screen, randomely move forward, turn left or right
            float rand = Random.Range(0f, 1f);
            if(rand > 0.75f)
                turnLeft();
            else if(rand > 0.5f)
                turnRight();
            else
                stepForward(isMenu);
        }
    }

    void turnLeft() {
        this.dir = (dir + 1) % 4;
        agentObj.transform.Rotate(0f, -90, 0f, Space.Self);
    }

    void turnRight() {
        this.dir = (dir + 3) % 4;
        agentObj.transform.Rotate(0f, 90, 0f, Space.Self);
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
        GridController.GC.updateVertexColour(chunk.xPos, chunk.zPos);
    }
    
    public void reproduce() {
    	List<Chunk> chunks = new List<Chunk>();
    	for(int i = Mathf.Max(chunk.xPos-1, 0); i <= Mathf.Min(chunk.xPos+1, GridController.GC.gridArray.GetLength(0)-1); i++) {
    		for(int j = Mathf.Max(chunk.zPos-1, 0); j <= Mathf.Min(chunk.zPos+1, GridController.GC.gridArray.GetLength(1)-1); j++) {
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
    	// colour = (colour + Random.Range(-amount/3f, amount/3f) + 1000000f) % 1f;
    	colour = (this.network.countWeights()/50f + 1000000f) % 1f;
		MR.material.color = Color.HSVToRGB(colour, 1f, 1f);
    }

    Chunk getNewChunk() {
		int newCol = chunk.xPos;
        int newRow = chunk.zPos;
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
