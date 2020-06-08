using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalculatedNode : Node {
	public float newValue;
    public List<Node> nodesFrom;
    public List<float> weights;
    public List<GameObject> connectionObjects;

    public CalculatedNode(int id) {
    	this.id = id;
        this.nodesFrom = new List<Node>();
        this.weights = new List<float>();
        this.connectionObjects = new List<GameObject>();
    }

    public void initiate(List<Node> nodes) {
        foreach(Node node in nodes) {
        	if(node != this && Random.Range(0f, 1f) <= SettingsPanel.startConnectionDensity) {
            	nodesFrom.Add(node);
            	weights.Add(Random.Range(-1f, 1f));
        	}
        }
    }

    public void inherit(List<Node> nodes, Node oldNode) {
        
    }

    public void calculateValueReLU() {
        float sum = 0;
        for (int n = 0; n < this.nodesFrom.Length; n++)
            sum += this.nodesFrom[n].value*weights[n];
        // ACTIVATION FUNCTION
        sum = Mathf.Max(sum, 0/*sum/10f*/);
        this.newValue = sum;
    }

    public void calculateValueSigmoid() {
        float sum = 0;
        for (int n = 0; n < this.nodesFrom.Length; n++)
            sum += this.nodesFrom[n].value*weights[n];
        // ACTIVATION FUNCTION
        sum = 1/(1 + Mathf.Exp(-sum));
        this.newValue = sum;
    }

    public void modifyConnection(Node node, bool addConnection) {
    	int connectionIndex = -1;
    	for(int n = 0; n < this.nodesFrom.Length; n++) {
    		if(this.nodesFrom[n] == node) {
    			connectionIndex = n;
    			break;
    		}
    	}

    	if(addConnection && connectionIndex == -1) {
    		List<Node> nodesList = new List<Node>(this.nodesFrom);
    		nodesList.Add(node);
    		this.nodesFrom = nodesList.ToArray();

    		List<float> weightsList = new List<float>(this.weights);
    		weightsList.Add(Random.Range(-1f, 1f));
    		this.weights = weightsList.ToArray();

	        this.connectionObjects = new GameObject[this.connectionObjects.Length + 1];
    	}

    	if(!addConnection && connectionIndex > -1) {
    		List<Node> nodesList = new List<Node>(this.nodesFrom);
    		nodesList.RemoveAt(connectionIndex);
    		this.nodesFrom = nodesList.ToArray();

    		List<float> weightsList = new List<float>(this.weights);
    		weightsList.RemoveAt(connectionIndex);
    		this.weights = weightsList.ToArray();

	        this.connectionObjects = new GameObject[this.connectionObjects.Length - 1];
    	}
    }

    public void combineConnections(Node otherNode) {
    	int connectionIndex = -1;
    	for(int n = 0; n < this.nodesFrom.Length; n++) {
    		if(this.nodesFrom[n] == otherNode) {
    			connectionIndex = n;
    			break;
    		}
    	}
    	// If this node has a connecton to the node, combine the connections
    	if(connectionIndex > -1) {
    		// For each of the other node's connections
    		for(int otherNodeN = 0; otherNodeN < otherNode.nodesFrom.Length; otherNodeN++) {
    			// Check wheter this node already has a connection to it
    			int connectionIndex2 = -1;
		    	for(int n = 0; n < this.nodesFrom.Length; n++) {
		    		if(this.nodesFrom[n] == otherNode.nodes[otherNodeN]) {
		    			connectionIndex2 = n;
		    			break;
		    		}
		    	}
		    	// float newWeight = Mathf.Min(otherNode.weights[otherNodeN] * this.weights[connectionIndex], otherNode.weights[otherNodeN] * Mathf.Abs(this.weights[connectionIndex]));
		    	float newWeight = (otherNode.weights[otherNodeN] < 0 && this.weights[connectionIndex] < 0) ? 0 : otherNode.weights[otherNodeN] * this.weights[connectionIndex];
		    	if(connectionIndex2 > -1) {
		    		// If this node already has a connection to it, sum that weight with the new weight
		    		this.weights[connectionIndex2] += newWeight;
		    	} else {
		    		// If it does not, add the new connection
		    		List<Node> nodesList2 = new List<Node>(this.nodesFrom);
		    		nodesList2.Add(otherNode.nodes[otherNodeN]);
		    		this.nodesFrom = nodesList2.ToArray();

		    		List<float> weightsList2 = new List<float>(this.weights);
		    		weightsList2.Add(newWeight);
		    		this.weights = weightsList2.ToArray();

			        this.connectionObjects = new GameObject[this.connectionObjects.Length + 1];
		    	}
    		}

    		// Delete the connection to the other node
    		List<Node> nodesList = new List<Node>(this.nodesFrom);
    		nodesList.RemoveAt(connectionIndex);
    		this.nodesFrom = nodesList.ToArray();

    		List<float> weightsList = new List<float>(this.weights);
    		weightsList.RemoveAt(connectionIndex);
    		this.weights = weightsList.ToArray();

    		this.connectionObjects = new GameObject[this.connectionObjects.Length - 1];
    	}
    }

    public override void display() {
        if(nodeObject != null) {
            float c = 0.5f + value / 2;
        	nodeObject.GetComponent<Image>().color = new Color(c, c, c);
            nodeObject.transform.GetChild(0).GetComponent<Text>().text = value.ToString("n1");
        }
    }
}