using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;
using UnityEngine;

public class NeuralNetwork {
    public float mutateAmount;
    float weightDecay = 0.999f;
    float mutateStrucProb = 0.01f;
    public Layer[] layers;
    public float maxWeight;
    public int nodeCount;
    
    // Used for starting agents
    public NeuralNetwork(int[] layerSizes) {
        this.mutateAmount = 0.1f;
        this.layers = new Layer[layerSizes.Length];
        layers[0] = new Layer(new Node[0], layerSizes[0], 0);
        for (int i = 1; i < layers.Length; i++)
            layers[i] = new Layer(layers[i-1].nodes, layerSizes[i], i);
        setmaxWeight();
        setNodeCount();
    }

    // Used when an agent reproduces
    public NeuralNetwork(NeuralNetwork oldNetwork) {
    	// Modify mutate amount
    	this.mutateAmount = Mathf.Max(oldNetwork.mutateAmount * Random.Range(0.9f, 1f/0.9f), 0.005f);
    	// Copy parent agent's network
    	this.layers = new Layer[oldNetwork.layers.Length];
    	for(int l = 0; l < oldNetwork.layers.Length; l++) {
			Layer oldL = oldNetwork.layers[l];
			// Create a new layer to copy to
			Layer newL = new Layer();
			newL.nodes = new Node[oldL.nodes.Length];
			for(int n = 0; n < oldL.nodes.Length; n++) {
				Node oldN = oldL.nodes[n];
				// Ceate a net node to copy to
				Node newN = new Node(l, n);
				newN.nodes = new Node[oldN.nodes.Length];
				newN.weights = new float[oldN.nodes.Length];
				newN.connectionObjects = new GameObject[oldN.nodes.Length];
				for(int c = 0; c < oldN.nodes.Length; c++) {
					Node toFind = oldN.nodes[c];
					newN.nodes[c] = this.layers[toFind.layerNum].nodes[toFind.nodeNum];

					newN.weights[c] = oldN.weights[c];
				}
				newL.nodes[n] = newN;
			}
			newL.nodes[newL.nodes.Length - 1].value = 1;
			this.layers[l] = newL;
		}
		setmaxWeight();
        setNodeCount();
    }

    void setmaxWeight() {
        this.maxWeight = 0;
        for (int l = 1; l < layers.Length; l++) {
            Layer currentLayer = layers[l];
            for (int n = 0; n < currentLayer.nodes.Length - 1; n++) {
                Node currentNode = currentLayer.nodes[n];
                for (int w = 0; w < currentNode.weights.Length; w++) {
                    if(Mathf.Abs(currentNode.weights[w]) > this.maxWeight)
                        this.maxWeight = Mathf.Abs(currentNode.weights[w]);
                }
            }
        }
    }

     public float countWeights() {
        float sum = 0;
        foreach(Layer l in this.layers) {
            foreach(Node n in l.nodes) {
                for(int w = 0; w < n.weights.Length; w++)
                    sum += n.weights[w];
            }
        }
        return sum;
    }

    void setNodeCount() {
    	nodeCount = 0;
    	foreach(Layer l in this.layers)
    		nodeCount += l.nodes.Length;
    }
    
    public void loadInputs(float[] inputs) {
        for (int i = 0; i < inputs.Length; i++) {
            layers[0].nodes[i].value = inputs[i];
        }
        calculate();
    }
    
    void calculate() {
        for(int l = 1; l < layers.Length-1; l++) {
            for(int n = 0; n < layers[l].nodes.Length - 1; n++)
                layers[l].nodes[n].calculateValueReLU();
        }
        // Calculate output layer with sigmoid
        for(int n = 0; n < layers[layers.Length-1].nodes.Length - 1; n++)
            layers[layers.Length-1].nodes[n].calculateValueSigmoid();
        // Update the display of each node
        for(int l = 0; l < layers.Length; l++) {
            for(int n = 0; n < layers[l].nodes.Length; n++)
                layers[l].nodes[n].display();
        }
    }
    
    public float returnOutput(int output) {
        return layers[layers.Length-1].nodes[output].value;
    }

    public void setConnectionColours() {
    	foreach(Layer l in layers) {
			foreach(Node n in l.nodes) {
				for(int c = 0; c < n.nodes.Length; c++) {
					UILineRenderer LineRenderer = n.connectionObjects[c].GetComponent<UILineRenderer>();
		          	if(SimulationManager.NNFlow) {
			            float opacity = Mathf.Abs(n.weights[c] * n.nodes[c].value / maxWeight);
			            if(n.weights[c] < 0)
			                LineRenderer.color = new Color(1.0f, 0.0f, 0.0f, opacity);
			            else
			                LineRenderer.color = new Color(0.0f, 1.0f, 0.0f, opacity);
			        } else
		            	LineRenderer.color = new Color(-n.weights[c], n.weights[c], 0, Mathf.Abs(n.weights[c]) / maxWeight);
				}
    		}
  		}
    }

    public void mutate() {
		mutateValue(mutateAmount);
    }

    // Mutate each weight in the network by a certain value with a 90% probability
    // There is a 10% chance the value will double recursively
    public void mutateValue(float a) {
		foreach(Layer l in layers) {
			foreach(Node n in l.nodes) {
				for(int c = 0; c < n.weights.Length; c++) {
					bool done = false;
					float amount = a;
					while(!done) {
						if(Random.Range(0f, 1f) > 0.9f)
							amount *= 2;
						else
							done = true;
					}
					n.weights[c] += Random.Range(-amount, amount);
					// Weight decay
					n.weights[c] *= weightDecay;
				}
			}
		}
		mutateStructure();
		setmaxWeight();
    }

    void mutateStructure() {
    	// Add layer
		if(Random.Range(0f, 1f) < mutateStrucProb) {
			int curL = Random.Range(1, layers.Length);
			// Add new layer at index curL
			List<Layer> layerList = new List<Layer>(this.layers);
			layerList.Insert(curL, new Layer());
			this.layers = layerList.ToArray();
			// Update all the stored layer numbers
			updateLayerNumbers(curL);
		}

		// Remove layer
		if(Random.Range(0f, 1f) < mutateStrucProb && this.layers.Length > 2) {
			int curL = Random.Range(1, layers.Length-1);

			foreach(Node curN in this.layers[curL].nodes) {
				// Combine the nodes connections with any nodes ahead in the network which have a connection to the node
				for (int l = curL+1; l < layers.Length; l++) {
            		for (int n = 0; n < layers[l].nodes.Length - 1; n++)
            			layers[l].nodes[n].combineConnections(curN);
            	}
            }

			// Remove the layer at index curL
			List<Layer> layerList = new List<Layer>(this.layers);
			layerList.RemoveAt(curL);
			this.layers = layerList.ToArray();
			// Update all the stored layer numbers
			updateLayerNumbers(curL);
		}

		// Add nodes
		for (int curL = 1; curL < layers.Length-1; curL++) {
			if(Random.Range(0f, 1f) < mutateStrucProb) {
				// Add node at the top of layer
				List<Node> nodeList = new List<Node>(this.layers[curL].nodes);
				nodeList.Insert(0, new Node(curL, 0));
				this.layers[curL].nodes = nodeList.ToArray();
				// Update node numbers
				updateNodeNumbers(curL);
			}
		}

		// Remove nodes
		for (int curL = 1; curL < layers.Length-1; curL++) {
			if(Random.Range(0f, 1f) < mutateStrucProb && this.layers[curL].nodes.Length > 1) {
				int curN = Random.Range(0, this.layers[curL].nodes.Length-1);
				List<Node> nodeList = new List<Node>(this.layers[curL].nodes);

				// Combine the nodes connections with any nodes ahead in the network which have a connection to the node
				for (int l = curL+1; l < layers.Length; l++) {
            		for (int n = 0; n < layers[l].nodes.Length - 1; n++)
            			layers[l].nodes[n].combineConnections(nodeList[curN]);
            	}

				// Remove the node
				nodeList.RemoveAt(curN);
				this.layers[curL].nodes = nodeList.ToArray();
				// Update node numbers
				updateNodeNumbers(curL);
			}
		}

		// Add/remove connections
    	for (int curL = 1; curL < layers.Length; curL++) {
    		// For each node (appart from the bias node) in each layer
            for (int curN = 0; curN < layers[curL].nodes.Length - 1; curN++) {
            	// Set probability to add a layer over remove to 0.5 for the layer behind the current
            	float probAdd = 2f/3f;
            	// For each node in each layer before the current
            	for(int prevL = curL-1; prevL >= 0; prevL--) {
					for(int prevN = 0; prevN < layers[prevL].nodes.Length; prevN++) {
						// Have a 1/200 chance of modifying the node
						if(Random.Range(0f, 1f) < mutateStrucProb)
							layers[curL].nodes[curN].modifyConnection(layers[prevL].nodes[prevN], Random.Range(0f, 1f) < probAdd);
					}
					// Divide the probability to add by 2 for each layer back
					probAdd *= 0.75f;
				}
			}
		}

        setNodeCount();
    }

    void updateLayerNumbers(int startingLayer) {
    	for(int l = startingLayer; l < this.layers.Length; l++) {
			foreach(Node n in this.layers[l].nodes)
				n.layerNum = l;
		}
    }

    void updateNodeNumbers(int layerNum) {
    	for(int n = 0; n < this.layers[layerNum].nodes.Length; n++) {
			this.layers[layerNum].nodes[n].nodeNum = n;
		}
    }

    public void printWeights() {
        string s = "";
        for (int l = 1; l < layers.Length; l++) {
            Layer currentLayer = layers[l];
            s += "\nLayer " + l;
            for (int n = 0; n < currentLayer.nodes.Length - 1; n++) {
                Node currentNode = currentLayer.nodes[n];
                s += "[";
                for(int w = 0; w < currentNode.weights.Length; w++) {
                    s += currentNode.weights[w] + ", ";
                }
                s += "]";
            }
        }
        Debug.Log(s);
    }
}
