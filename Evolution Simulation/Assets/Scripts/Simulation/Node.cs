using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node {
    public float value;
    public Node[] nodes;
    public float[] weights;
    public GameObject nodeObject;
    public GameObject[] connectionObjects;
    public int layerNum;
    public int nodeNum;
    public int colour;

    public Node(Node[] prevNodes, int layerNum, int nodeNum) {
        this.nodes = new Node[prevNodes.Length];
        for(int i = 0; i < prevNodes.Length; i++)
            nodes[i] = prevNodes[i];
        this.weights = new float[prevNodes.Length];
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Random.Range(-1f, 1f);
        this.connectionObjects = new GameObject[prevNodes.Length];

        this.layerNum = layerNum;
        this.nodeNum = nodeNum;
        this.colour = 0;
    }

    // Used when copying a network
    public Node(int layerNum, int nodeNum) {
    	this.nodes = new Node[0];
        this.weights = new float[0];
        this.connectionObjects = new GameObject[0];

        this.layerNum = layerNum;
        this.nodeNum = nodeNum;
        this.colour = 0;
    }

    public void calculateValueReLU() {
        float sum = 0;
        for (int n = 0; n < nodes.Length; n++)
            sum += nodes[n].value*weights[n];
        // ACTIVATION FUNCTION
        sum = Mathf.Max(sum, 0);
        this.value = sum;
    }

    public void calculateValueSigmoid() {
        float sum = 0;
        for (int n = 0; n < nodes.Length; n++)
            sum += nodes[n].value*weights[n];
        // ACTIVATION FUNCTION
        sum = 1/(1 + Mathf.Exp(-sum));
        this.value = sum;
    }

    public void modifyConnection(Node node, bool addConnection) {
    	int connectionIndex = -1;
    	for(int n = 0; n < this.nodes.Length; n++) {
    		if(this.nodes[n] == node) {
    			connectionIndex = n;
    			break;
    		}
    	}

    	if(addConnection && connectionIndex == -1) {
    		List<Node> nodesList = new List<Node>(this.nodes);
    		nodesList.Add(node);
    		this.nodes = nodesList.ToArray();

    		List<float> weightsList = new List<float>(this.weights);
    		weightsList.Add(Random.Range(-1f, 1f));
    		this.weights = weightsList.ToArray();

	        this.connectionObjects = new GameObject[this.connectionObjects.Length + 1];
    	}

    	if(!addConnection && connectionIndex > -1) {
    		List<Node> nodesList = new List<Node>(this.nodes);
    		nodesList.RemoveAt(connectionIndex);
    		this.nodes = nodesList.ToArray();

    		List<float> weightsList = new List<float>(this.weights);
    		weightsList.RemoveAt(connectionIndex);
    		this.weights = weightsList.ToArray();

	        this.connectionObjects = new GameObject[this.connectionObjects.Length - 1];
    	}
    }

    public void combineConnections(Node otherNode) {
    	int connectionIndex = -1;
    	for(int n = 0; n < this.nodes.Length; n++) {
    		if(this.nodes[n] == otherNode) {
    			connectionIndex = n;
    			break;
    		}
    	}
    	// If this node has a connecton to the node, combine the connections
    	if(connectionIndex > -1) {
    		// For each of the other node's connections
    		for(int otherNodeN = 0; otherNodeN < otherNode.nodes.Length; otherNodeN++) {
    			// Check wheter this node already has a connection to it
    			int connectionIndex2 = -1;
		    	for(int n = 0; n < this.nodes.Length; n++) {
		    		if(this.nodes[n] == otherNode.nodes[otherNodeN]) {
		    			connectionIndex2 = n;
		    			break;
		    		}
		    	}
		    	float newWeight = Mathf.Min(otherNode.weights[otherNodeN] * this.weights[connectionIndex], otherNode.weights[otherNodeN] * Mathf.Abs(this.weights[connectionIndex]));
		    	if(connectionIndex2 > -1) {
		    		// If this node already has a connection to it, sum that weight with the new weight
		    		this.weights[connectionIndex2] += newWeight;
		    	} else {
		    		// If it does not, add the new connection
		    		List<Node> nodesList2 = new List<Node>(this.nodes);
		    		nodesList2.Add(otherNode.nodes[otherNodeN]);
		    		this.nodes = nodesList2.ToArray();

		    		List<float> weightsList2 = new List<float>(this.weights);
		    		weightsList2.Add(newWeight);
		    		this.weights = weightsList2.ToArray();

			        this.connectionObjects = new GameObject[this.connectionObjects.Length + 1];
		    	}
    		}

    		// Delete the connection to the other node
    		List<Node> nodesList = new List<Node>(this.nodes);
    		nodesList.RemoveAt(connectionIndex);
    		this.nodes = nodesList.ToArray();

    		List<float> weightsList = new List<float>(this.weights);
    		weightsList.RemoveAt(connectionIndex);
    		this.weights = weightsList.ToArray();

    		this.connectionObjects = new GameObject[this.connectionObjects.Length - 1];
    	}
    }

    public void display() {
        if(nodeObject != null) {
            float c = 0.5f + value / 2;
            if(colour == 0)
            	nodeObject.GetComponent<Image>().color = new Color(c, c, c);
            else if(colour == 1)
            	nodeObject.GetComponent<Image>().color = new Color(0.263f, c, 0.094f);
            else if(colour == 2)
            	nodeObject.GetComponent<Image>().color = new Color(0, 0.157f, c);
            else
            	nodeObject.GetComponent<Image>().color = new Color(c, 0.25f, 0.25f);
            nodeObject.transform.GetChild(0).GetComponent<Text>().text = value.ToString("n1");
        }
    }
}
