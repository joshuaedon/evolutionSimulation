using UnityEngine.UI.Extensions;
using UnityEngine;

public class NeuralNetwork {
    // string[] inputLabels = {"Random", "Bot R", "Bot B", "Water R", "Water B", "Food R", "Food B", "Bot Col R", "Bot Col B", "Scent R", "Scent B", "Hunger"};
    // string[] outputLabels = {"Left", "Right", "Step", "Stay", "Dist R", "Dir R", "Range R", "Dist B", "Dir B", "Range B", "Breed/Kill", "Mutation", "Drop Scent"};
    string[] inputLabels = {"Random", "Hunger", "Food Below"};
    string[] outputLabels = {"Forwards", "Left", "Right", "Eat", "Reproduce"};
    public float mutateAmount;
    float weightDecay = 0.999f;
    // float[] prevOutputs;
    public Layer[] layers;
    public float maxWeight;
    
    public NeuralNetwork(int[] layerSizes) {
        // this.prevOutputs = new float[layerSizes[layerSizes.Length-1]];
        this.mutateAmount = 0.1f;
        this.layers = new Layer[layerSizes.Length];
        layers[0] = new Layer(new Node[0], layerSizes[0], 0);
        for (int i = 1; i < layers.Length; i++)
            layers[i] = new Layer(layers[i-1].nodes, layerSizes[i], i);
        setmaxWeight();
    }

    public NeuralNetwork(NeuralNetwork oldNetwork) {
    	this.mutateAmount = Mathf.Max(oldNetwork.mutateAmount * Random.Range(0.9f, 1f/0.9f), 0.005f);
    	this.layers = new Layer[oldNetwork.layers.Length];
    	for(int l = 0; l < oldNetwork.layers.Length; l++) {
			Layer oldL = oldNetwork.layers[l];
			Layer newL = new Layer();
			newL.nodes = new Node[oldL.nodes.Length];
			for(int n = 0; n < oldL.nodes.Length; n++) {
				Node oldN = oldL.nodes[n];
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
    }

    /*public NeuralNetwork(Layer[] a, Layer[] b, float aMutation, float bMutation) {
        this.prevOutputs = new float[a[a.Length-1].nodes.Length-1];
        this.layers = new Layer[a.Length];
        layers[0] = new Layer(0, a[0].nodes.Length-1);
        for (int i = 1; i < a.Length; i++) {
            if(i < b.Length)
                layers[i] = new Layer(a[i].nodes, b[i].nodes, aMutation, bMutation);
            else
                layers[i] = new Layer(a[i].nodes, aMutation);
        }
        setmaxWeight();
    }*/

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
    
    public void loadInputs(float[] inputs) {
        for (int i = 0; i < inputs.Length; i++) {
            layers[0].nodes[i].value = inputs[i];
        }
        calculate();
    }
    
    void calculate() {
        // for(int i = 0; i < prevOutputs.Length; i++)
        //     prevOutputs[i] = layers[layers.Length-1].nodes[i].value;

        for (int l = 1; l < layers.Length; l++) {
            for (int n = 0; n < layers[l].nodes.Length - 1; n++)
                layers[l].nodes[n].calculateValue();
        }
        for (int l = 0; l < layers.Length; l++) {
            for (int n = 0; n < layers[l].nodes.Length; n++)
                layers[l].nodes[n].display();
        }
    }
    
    public float returnOutput(string outputStr/*, bool prev*/) {
        int output = -1;
        for(int i = 0; i < outputLabels.Length; i++) {
            if(outputLabels[i].Equals(outputStr))
              output = i;
        }
        // if(prev)
        //     return prevOutputs[output];
        // else
            return layers[layers.Length-1].nodes[output].value;
    }

    /*void addLayer() {
        Layer[] newLayers = new Layer[layers.Length+1];
        newLayers[0] = layers[0];
        newLayers[1] = new Layer(layers[0].nodes.Length);
        for(int i = 1; i < layers.Length; i++)
            newLayers[i+1] = layers[i];
        this.layers = newLayers;
    }*/

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

    public float mutateValue(float a) {
		float sum = 0f;
    	int count = 0;
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
					sum += amount;
					count++;
					// Weight decay
					n.weights[c] *= weightDecay;
				}
			}
		}
		setmaxWeight();
		return sum / count;
    }

    public float mutate() {
		return mutateValue(mutateAmount);
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
