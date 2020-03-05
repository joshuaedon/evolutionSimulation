using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent {
    public GameObject agentObj;
    public GameObject plumbob;
    public Chunk chunk;
    public NeuralNetwork network;
    public int generation;
    public int ticksAlive;
    // public float lookDir;
    // public float lookRange;
    // public float lookDist;
    int dir;
    public float hunger;
    public float health;

    public Agent(GameObject agentObj, Chunk chunk) {
        this.agentObj = agentObj;
        this.plumbob = agentObj.transform.GetChild(2).gameObject;
        this.plumbob.SetActive(false);
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(new int[] {13, 6});
        this.generation = 1;
        this.ticksAlive = 0;
  //       this.lookDir = Random.Range(0f, 1f);
		// this.lookRange = Random.Range(0f, 1f);
		// this.lookDist = Random.Range(0f, 1f);
        changeColour();

        this.dir = Random.Range(0, 4);
        // Rotate the agent's object to the correct direction
    	agentObj.transform.Rotate(0f, 90f - dir * 90f, 0f, Space.Self);
        this.hunger = 1;
        this.health = 1;
    }

    public Agent(Chunk chunk, NeuralNetwork n, int generation/*, float lookDir, float lookRange, float lookDist*/) {
        this.agentObj = (GameObject)Transform.Instantiate(Resources.Load("Simulation/Agent"), GridController.GC.transform);
        this.plumbob = agentObj.transform.GetChild(2).gameObject;
        this.plumbob.SetActive(false);
        this.chunk = chunk;
        moveObj();

        this.network = new NeuralNetwork(n);
        this.generation = generation;
        this.ticksAlive = 0;
        this.network.mutate();
  //       this.lookDir = lookDir;
		// this.lookRange = lookRange;
		// this.lookDist = lookDist;
		changeColour();

        this.dir = Random.Range(0, 4);
        // Rotate the agent's object to the correct direction
    	agentObj.transform.Rotate(0f, 90f - dir * 90f, 0f, Space.Self);
        this.hunger = 1;
        this.health = 1;
    }

    public void moveObj() {
        agentObj.transform.position = new Vector3(chunk.xPos, GridController.GC.yScale * Mathf.Clamp(chunk.yPos + chunk.yOffset, 0f, 1f) + 0.5f, chunk.zPos);
    }

    public void loadInputs() {
        float[] inputs = new float[13];
        inputs[0] = Random.Range(0f, 1f);
        inputs[1] = this.hunger;
        inputs[2] = this.chunk.food;
        inputs[3] = this.chunk.isWater() ? 1 : 0;
        Chunk newChunk = getNewChunk();
        inputs[4] = (newChunk != null) ? newChunk.food : 0f;
        inputs[5] = (newChunk == null || newChunk.isWater()) ? 1 : 0;
        inputs[6] = (newChunk == null || newChunk.agent == null) ? 0 : 1;
        /*Chunk[] rightChunks = getChunks(1);
        inputs[7] = amountFood(rightChunks);
        inputs[8] = amountWater(rightChunks);
        inputs[9] = amountAgent(rightChunks);
        Chunk[] leftChunks = getChunks(-1);
        inputs[10] = amountFood(leftChunks);
        inputs[11] = amountWater(leftChunks);
        inputs[12] = amountAgent(leftChunks);*/
        inputs[7 + highestOutput()] = 1f;
        network.loadInputs(inputs);
    }

    /*float amountFood(Chunk[] chunks) {
    	if(chunks.Length == 0)
    		return 0;
    	float sum = 0;
    	foreach(Chunk c in chunks) {
    		sum += c.food;
    	}
    	return sum / chunks.Length;
    }
    float amountWater(Chunk[] chunks) {
    	if(chunks.Length == 0)
    		return 0;
    	float sum = 0;
    	foreach(Chunk c in chunks) {
    		if(c.isWater())
    			sum += 1f;
    	}
    	return sum / chunks.Length;
    }
    float amountAgent(Chunk[] chunks) {
    	if(chunks.Length == 0)
    		return 0;
    	float sum = 0;
    	foreach(Chunk c in chunks) {
    		if(c.agent != null)
    			sum += 1f;
    	}
    	return sum / chunks.Length;
    }*/

    int highestOutput() {
    	float forwardsOut  = network.returnOutput(0);
        float leftOut      = network.returnOutput(1);
        float rightOut     = network.returnOutput(2);
        float eatOut       = network.returnOutput(3);
        float reproduceOut = network.returnOutput(4);
        float attackOut    = network.returnOutput(5);
    	if(forwardsOut > Mathf.Max(Mathf.Max(Mathf.Max(Mathf.Max(leftOut, rightOut), eatOut), reproduceOut), attackOut))
	        return 0;
	    else if(leftOut > Mathf.Max(Mathf.Max(Mathf.Max(rightOut, eatOut), reproduceOut), attackOut))
	        return 1;
	    else if(rightOut > Mathf.Max(Mathf.Max(eatOut, reproduceOut), attackOut))
	        return 2;
	    else if(eatOut > Mathf.Max(reproduceOut, attackOut))
	    	return 3;
	    else if(reproduceOut > attackOut && hunger > 0 && !chunk.isWater())
	    	return 4;
	    else
	    	return 5;
    }

    public void act(bool isMenu) {
        if(!isMenu) {
        	// Loose hunger
        	this.hunger -= GridController.GC.hungerLoss + this.network.nodeCount * 0.0000005f;
        	if(chunk.isWater()) {
        		network.mutateValue(0.2f);
        		changeColour();
        		health -= 0.1f;
        	}

        	agentObj.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
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
      			case 5:
		            attack();
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
    		Agent offspring = new Agent(c, network, generation+1/*,
    									Mathf.Clamp(lookDir   + Random.Range(-0.01f, 0.01f), 0f, 1f),
    									Mathf.Clamp(lookRange + Random.Range(-0.01f, 0.01f), 0f, 1f),
    									Mathf.Clamp(lookDist  + Random.Range(-0.01f, 0.01f), 0f, 1f)
    								   */);
    		this.hunger /= 1f + Mathf.Min(GridController.GC.agents.Count / 200f, 1f);
    		offspring.hunger = this.hunger;
    		c.agent = offspring;
    		GridController.GC.agents.Add(offspring);
    	}
    }

    void attack() {
    	// Change colour to black
    	agentObj.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material.color = Color.black;
    	// If there is an agent infront, deal random damage
    	Chunk newChunk = getNewChunk();
        if(newChunk != null && newChunk.agent != null) {
            newChunk.agent.health -= Random.Range(0f, 0.5f);
            // If killed, eat as much as can, drop the rest on the ground under the attacked agent
            if(newChunk.agent.health <= 0) {
            	float toAdd = newChunk.agent.hunger;
            	newChunk.agent.hunger = 0;
            	float add = Mathf.Min(toAdd, 1f - this.hunger);
            	this.hunger += add;
            	toAdd -= add;
            	newChunk.food = Mathf.Min(1f, newChunk.food + toAdd);
            }
        }
    }

     public void changeColour() {
    	float colour = (this.network.countWeights()/50f + 1000000f) % 1f;
		agentObj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(colour, 1f, 1f);
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

    /*public Chunk[] getChunks(int positive) {
    	float direction = (-positive * lookDir * Mathf.PI + (dir * Mathf.PI / 2f)) % (2f * Mathf.PI);
    	float range = lookRange * Mathf.PI;
    	float distance = lookDist * 5f + 1f;

    	List<Chunk> chunks = new List<Chunk>();
    	for(int c = -Mathf.FloorToInt(distance); c <= distance; c++) {
    		for(int r = -Mathf.FloorToInt(distance); r <= distance; r++) {
    			if(this.chunk.xPos + c >= 0 && this.chunk.zPos + r >= 0 && this.chunk.xPos + c < GridController.GC.cols && this.chunk.zPos + r < GridController.GC.rows) {
    				if((c != 0 || r != 0) && checkChunk(direction, range, distance, c, r))
    					chunks.Add(GridController.GC.gridArray[this.chunk.xPos + c, this.chunk.zPos + r]);
    			}
			}
    	}

    	return chunks.ToArray();
    }

    bool checkChunk(float direction, float range, float distance, int c, int r) {
    	if(Mathf.Pow(c*c + r*r, 0.5f) > distance)
    		return false;

    	float difference = Mathf.Abs(Mathf.Atan2(r, c) - direction);
    	if(difference > range && difference < Mathf.PI * 2 - range)
    		return false;

    	return true;
    }*/
}
