using UnityEngine;

public class NeuralNetwork {
    // string[] inputLabels = {"Random", "Bot R", "Bot B", "Water R", "Water B", "Food R", "Food B", "Bot Col R", "Bot Col B", "Scent R", "Scent B", "Hunger"};
    // string[] outputLabels = {"Left", "Right", "Step", "Stay", "Dist R", "Dir R", "Range R", "Dist B", "Dir B", "Range B", "Breed/Kill", "Mutation", "Drop Scent"};
    string[] inputLabels = {"Random"};
    string[] outputLabels = {"Forwards", "Left", "Right", "Eat"};
    float[] prevOutputs;
    public Layer[] layers;
    public float maxWeight;
    
    public NeuralNetwork(int[] layerSizes) {
        this.prevOutputs = new float[layerSizes[layerSizes.Length-1]];
        this.layers = new Layer[layerSizes.Length];
        layers[0] = new Layer(new Node[0], layerSizes[0]);
        for (int i = 1; i < layers.Length; i++)
            layers[i] = new Layer(layers[i-1].nodes, layerSizes[i]);
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
        for(int i = 0; i < prevOutputs.Length; i++)
            prevOutputs[i] = layers[layers.Length-1].nodes[i].value;

        for (int l = 1; l < layers.Length; l++) {
            for (int n = 0; n < layers[l].nodes.Length - 1; n++)
                layers[l].nodes[n].calculateValue();
        }
        for (int l = 0; l < layers.Length; l++) {
            for (int n = 0; n < layers[l].nodes.Length; n++)
                layers[l].nodes[n].display();
        }
    }
    
    public float returnOutput(string outputStr, bool prev) {
        int output = -1;
        for(int i = 0; i < outputLabels.Length; i++) {
            if(outputLabels[i].Equals(outputStr))
              output = i;
        }
        if(prev)
            return prevOutputs[output];
        else
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
    /*  
    NeuralNetwork copy() {
      int[] layerSizes = new int[this.layers.length];
      for (int i = 0; i < layerSizes.length; i++)
        layerSizes[i] = this.layers[i].nodes.length - 1;
      
      NeuralNetwork newNetwork = new NeuralNetwork(layerSizes);
      
      for (int l = 0; l < newNetwork.layers.length; l++) {
        Layer currentLayer = newNetwork.layers[l];
        for (int n = 0; n < currentLayer.nodes.length; n++)
          currentLayer.nodes[n].connections = this.layers[l].nodes[n].connections.clone();
      }
      
      return newNetwork;
    }

    void mutate(float amount) {
      for (Layer l : layers) {
        for (Node n : l.nodes) {
          for (int c = 0; c < n.connections.length; c++)
            n.connections[c] += random(-amount, amount);
        }
      }
      for (int l = 0; l < this.layers.length; l++) {
        Layer currentLayer = this.layers[l];
        for (int n = 0; n < currentLayer.nodes.length; n++) {
          Node currentNode = currentLayer.nodes[n];
          for (int c = 0; c < currentNode.connections.length; c++)
            currentNode.connections[c] += random(-amount, amount);
        }
      }
    }
    */
}
