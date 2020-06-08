using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
    // public GameObject highlight;
    public Chunk chunk;

    public NeuralNetwork network;
    // public float encodedInputs;
    public int generation;
    
    public int ticksAlive;
    public int kills;
    public int children;

    // public int[] sensePositions;
    // public int[] senseThings;

    int dir;

    public float hunger;
    public float health;

    void Awake() {
        // this.highlight = transform.GetChild(4).gameObject;
        // this.highlight.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(false);
        this.ticksAlive = 0;
        // this.sensePositions = new int[9];
        // this.senseThings = new int[9];

        this.dir = Random.Range(0, 4);
        // Rotate the agent's object to the correct direction
    	transform.Rotate(0f, 0f, dir * 90f, Space.Self);

        this.hunger = 1;
        this.health = 1;
    }

    public void initiate(Chunk chunk) {
        this.chunk = chunk;
        moveObj();

        this.generation = 1;
        this.network = new NeuralNetwork(10);
        setColour();

        // for(int i = 0; i < 9; i++)
        // 	sensePositions[i] = Random.Range(0, 9);
        // for(int i = 0; i < 9; i++)
        // 	senseThings[i] = Random.Range(0, 3);
    }

    public void inherit(Chunk chunk, int generation, NeuralNetwork n/*, int[] sensePositions, int[] senseThings*/) {
        this.chunk = chunk;
        moveObj();

        this.generation = generation;
        this.network = new NeuralNetwork(n);
        this.network.mutate();
		setColour();

		/*for(int i = 0; i < 9; i++) {
			if(Random.Range(0f, 1f) < this.network.mutateAmount / 5f)
        		this.sensePositions[i] = Random.Range(0, 9);
        	else
        		this.sensePositions[i] = sensePositions[i];
		}
        for(int i = 0; i < 9; i++) {
			if(Random.Range(0f, 1f) < this.network.mutateAmount / 5f)
        		this.senseThings[i] = Random.Range(0, 3);
        	else
        		this.senseThings[i] = senseThings[i];
		}*/
    }

    public void moveObj() {
		transform.position = new Vector3(chunk.col - 0.5f - SettingsPanel.cols/2f, chunk.row - 0.5f - SettingsPanel.rows/2f, 0);
    }

    public void loadInputs() {
        float[] inputs = new float[18];

        inputs[0] = Random.Range(0f, 1f);
        inputs[1] = this.hunger;
        inputs[2] = this.health;

        for(int i = 0; i < 6; i++)
        	inputs[i + 3] = 0.01f * network.returnOutput(i);
        inputs[3 + highestOutput()] += 0.99f;

        for(int i = 9; i < 18; i++)
        	inputs[i] = getThingValue(getChunk(sensePositions[i-9]), senseThings[i-9]);

        network.loadInputs(inputs);
    }

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
	    else if(reproduceOut > attackOut && canReproduce() && hunger > 0)
	    	return 4;
	    else if(attackOut > 0)
	    	return 5;
	    else
	    	return 0;
    }

    public void act(bool isMenu) {
        if(!isMenu) {
        	// Loose hunger and health
        	this.hunger -= SettingsPanel.hungerLoss + 0.0000001f * (this.network.nodeCount + network.layers.Length*20) * SettingsPanel.nodeHungerLossPenalty;

        	if(chunk.isWater()) {
        		if(SettingsPanel.waterMutate > 0) {
		    		network.mutateValue(SettingsPanel.waterMutate);
		    		setColour();
		    	}
	    		health -= SettingsPanel.waterDamage;
	    	}

        	transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().material.color = Color.white;
        	// Move forward, turn left, right, eat or attack depending on the agent's NN outputs
      		int output = highestOutput();
      		switch(output) {
      			case 0: stepForward(isMenu); break;
      			case 1: turnLeft(); break;
      			case 2: turnRight(); break;
      			case 3: eat(); break;
      			case 4: reproduce(); break;
      			case 5: attack(); break;
      		}

            loadInputs();
        	if(this == AgentPanel.selectedAgent) {
        		if(chunk.isWater()) {
        			ToolsPanel.agentPanel.GetComponent<AgentPanel>().resetNetwork();
    			} else if(AgentPanel.NNFlow)
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
        transform.Rotate(0f, 0f, 90f, Space.Self);
    }

    void turnRight() {
        this.dir = (dir + 3) % 4;
        transform.Rotate(0f, 0f, -90f, Space.Self);
    }

    void stepForward(bool isMenu) {
        Chunk newChunk = getChunk(1);
        if(newChunk != null && newChunk.agent == null && (!isMenu || !newChunk.isWater() || this.chunk.isWater())) {
            // Step Forward
            this.chunk.agent = null;
            this.chunk = newChunk;
            newChunk.agent = this;
            moveObj();
        }
    }

    void eat() {
    	float amount = Mathf.Min(SettingsPanel.eatSpeed, Mathf.Min(1f - hunger, chunk.food));
    	hunger += amount;
    	chunk.food -= amount;
    	// Reset the color of the chunk's vertex
        Grid.gridArray[chunk.col, chunk.row].updateColour();
    }
    
    public void reproduce() {
    	List<Chunk> chunks = new List<Chunk>();
    	for(int i = Mathf.Max(chunk.col-1, 0); i <= Mathf.Min(chunk.col+1, Grid.gridArray.GetLength(0)-1); i++) {
    		for(int j = Mathf.Max(chunk.row-1, 0); j <= Mathf.Min(chunk.row+1, Grid.gridArray.GetLength(1)-1); j++) {
    			Chunk c = Grid.gridArray[i, j];
    			if(c.agent == null)
    				chunks.Add(c);
    		}
    	}
    	if(chunks.Count > 0) {
    		Chunk chunk = chunks[Random.Range(0, chunks.Count)];
    		Agent offspring = ((GameObject)Instantiate(Resources.Load("Simulation/Agent"), GameObject.Find("Agents").transform)).GetComponent<Agent>();
            offspring.inherit(chunk, this.generation+1, this.network, this.sensePositions, this.senseThings);
    		this.hunger /= 1f + Mathf.Min(Grid.agents.Count / 200f, 1f);
    		offspring.hunger = this.hunger;
    		chunk.agent = offspring;
    		Grid.agents.Add(offspring);
    		this.children++;
    	}
    }

    void attack() {
    	// Change colour to black
    	transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().material.color = Color.black;
    	// If there is an agent infront, deal random damage
    	Chunk newChunk = getChunk(1);
        if(newChunk != null && newChunk.agent != null) {
            newChunk.agent.health -= 2f*Random.Range(0f, SettingsPanel.attackDamage);
            // If killed, eat as much as can, drop the rest on the ground under the attacked agent
            if(newChunk.agent.health <= 0) {
            	kills++;
            	float toAdd = newChunk.agent.hunger;
            	newChunk.agent.hunger = 0;
            	float add = Mathf.Min(toAdd, 1f - this.hunger);
            	this.hunger += add;
            	toAdd -= add;
            	newChunk.food = Mathf.Min(1f, newChunk.food + toAdd);
            }
        }
    }

    public void setColour() {
    	float colour = (this.network.countWeights()/25f + 1000000f) % 1f;
		transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().material.color = Color.HSVToRGB(colour, 1f, 1f);
    }

    Chunk getChunk(int position) {
		int newCol = chunk.col;
        int newRow = chunk.row;
        if(position == 0 || position == 1 || position == 2) {
	        switch(this.dir) {
	            case 0: newCol++; break;
	            case 1: newRow++; break;
	            case 2: newCol--; break;
	            case 3: newRow--; break;
	        }
    	}
    	if(position == 6 || position == 7 || position == 8) {
    		switch(this.dir) {
	            case 0: newCol--; break;
	            case 1: newRow--; break;
	            case 2: newCol++; break;
	            case 3: newRow++; break;
	        }
    	}
    	if(position == 0 || position == 3 || position == 6) {
    		switch(this.dir) {
	            case 0: newRow++; break;
	            case 1: newCol--; break;
	            case 2: newRow--; break;
	            case 3: newCol++; break;
	        }
    	}
    	if(position == 2 || position == 5 || position == 8) {
    		switch(this.dir) {
	            case 0: newRow--; break;
	            case 1: newCol++; break;
	            case 2: newRow++; break;
	            case 3: newCol--; break;
	        }
    	}
        if(newCol >= 0 && newRow >= 0 && newCol < SettingsPanel.cols && newRow < SettingsPanel.rows) {
            return Grid.gridArray[newCol, newRow];
        }
        return null;
    }

    float getThingValue(Chunk c, int thing) {
        switch(thing) {
        	case 0: return (c != null) ? c.food : 0f;
        	case 1: return (c == null || c.isWater()) ? 1 : 0;
        	case 2: return (c == null || c.agent == null) ? 0 : 1;
        }
        return 100f;
    }

    bool canReproduce() {
    	return !this.chunk.isWater();
    }
}

public struct AgentRecord {
	public int deathTick;
	public int generation;
	public int ticksAlive;
	public float colour;
	public int senseFood;
	public int senseWater;
	public int senseAgent;
	public int senseFront;
	public int senseSide;
	public int senseBack;
	public int nodes;
	public int kills;
	public int children;

	public AgentRecord(bool a) {
		this.deathTick = int.MaxValue;
    	this.generation = int.MaxValue;
    	this.ticksAlive = int.MaxValue;
    	this.colour = float.MaxValue;
    	this.senseFood = int.MaxValue;
    	this.senseWater = int.MaxValue;
    	this.senseAgent = int.MaxValue;
    	this.senseFront = int.MaxValue;
    	this.senseSide = int.MaxValue;
    	this.senseBack = int.MaxValue;
    	this.nodes = int.MaxValue;
    	this.kills = int.MaxValue;
    	this.children = int.MaxValue;
	}

	public AgentRecord(int deathTick,
					   int generation, int ticksAlive, float colour,
					   int senseFood, int senseWater, int senseAgent,
					   int senseFront, int senseSide, int senseBack,
					   int nodes, int kills, int children) {
		this.deathTick = deathTick;
    	this.generation = generation;
    	this.ticksAlive = ticksAlive;
    	this.colour = colour;
    	this.senseFood = senseFood;
    	this.senseWater = senseWater;
    	this.senseAgent = senseAgent;
    	this.senseFront = senseFront;
    	this.senseSide = senseSide;
    	this.senseBack = senseBack;
    	this.nodes = nodes;
    	this.kills = kills;
    	this.children = children;
	}

 	public AgentRecord updateMaxRecord(AgentRecord r) {
		this.deathTick = Mathf.Max(this.deathTick, r.deathTick);
		this.generation = Mathf.Max(this.generation, r.generation);
		this.ticksAlive = Mathf.Max(this.ticksAlive, r.ticksAlive);
		this.colour = Mathf.Max(this.colour, r.colour);
		this.senseFood = Mathf.Max(this.senseFood, r.senseFood);
		this.senseWater = Mathf.Max(this.senseWater, r.senseWater);
		this.senseAgent = Mathf.Max(this.senseAgent, r.senseAgent);
		this.senseFront = Mathf.Max(this.senseFront, r.senseFront);
		this.senseSide = Mathf.Max(this.senseSide, r.senseSide);
		this.senseBack = Mathf.Max(this.senseBack, r.senseBack);
		this.nodes = Mathf.Max(this.nodes, r.nodes);
		this.kills = Mathf.Max(this.kills, r.kills);
		this.children = Mathf.Max(this.children, r.children);
		return this;
    }

    public AgentRecord updateMinRecord(AgentRecord r) {
		this.deathTick = Mathf.Min(this.deathTick, r.deathTick);
		this.generation = Mathf.Min(this.generation, r.generation);
		this.ticksAlive = Mathf.Min(this.ticksAlive, r.ticksAlive);
		this.colour = Mathf.Min(this.colour, r.colour);
		this.senseFood = Mathf.Min(this.senseFood, r.senseFood);
		this.senseWater = Mathf.Min(this.senseWater, r.senseWater);
		this.senseAgent = Mathf.Min(this.senseAgent, r.senseAgent);
		this.senseFront = Mathf.Min(this.senseFront, r.senseFront);
		this.senseSide = Mathf.Min(this.senseSide, r.senseSide);
		this.senseBack = Mathf.Min(this.senseBack, r.senseBack);
		this.nodes = Mathf.Min(this.nodes, r.nodes);
		this.kills = Mathf.Min(this.kills, r.kills);
		this.children = Mathf.Min(this.children, r.children);
		return this;
    }

    public float get(int index) {
    	switch(index) {
	    	case 0: return deathTick;
	    	case 2: return generation;
	    	case 3: return ticksAlive;
	    	case 4: return colour;
	    	case 5: return senseFood;
	    	case 6: return senseWater;
	    	case 7: return senseAgent;
	    	case 8: return senseFront;
	    	case 9: return senseSide;
	    	case 10: return senseBack;
	    	case 11: return nodes;
	    	case 12: return kills;
	    	case 13: return children;
	    }
	    return 0;
    }

    public void print() {
    	Debug.Log(this.deathTick + ", " +
    		this.generation + ", " + this.ticksAlive + ", " + this.colour + ", " +
    		this.senseFood + ", " + this.senseWater + ", " + this.senseAgent + ", " +
    		this.senseFront + ", " + this.senseSide + ", " + this.senseBack + ", " +
    		this.nodes + ", " + this.kills + ", " + this.children);
    }
}
