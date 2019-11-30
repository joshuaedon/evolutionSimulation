using UnityEngine;

class NeuralNetwork {
  string[] inputLabels = {"Random", "Bot R", "Bot B", "Water R", "Water B", "Food R", "Food B", "Bot Col R", "Bot Col B", "Scent R", "Scent B", "Hunger"};
  string[] outputLabels = {"Left", "Right", "Step", "Stay", "Dist R", "Dir R", "Range R", "Dist B", "Dir B", "Range B", "Breed/Kill", "Mutation", "Drop Scent"};
  float[] prevOutputs;
  Layer[] layers;
  float maxConnection;
  
  NeuralNetwork(int[] layerSizes) {
    this.prevOutputs = new float[layerSizes[layerSizes.Length-1]];
    this.layers = new Layer[layerSizes.Length];
    layers[0] = new Layer(0, layerSizes[0]);
    for (int i = 1; i < layers.Length; i++)
      layers[i] = new Layer(layerSizes[i-1] + 1, layerSizes[i]);
    setMaxConnection();
  }

  NeuralNetwork(Layer[] a, Layer[] b, float aMutation, float bMutation) {
    this.prevOutputs = new float[a[a.Length-1].nodes.Length-1];
    this.layers = new Layer[a.Length];
    layers[0] = new Layer(0, a[0].nodes.Length-1);
    for (int i = 1; i < a.Length; i++) {
      if(i < b.Length)
        layers[i] = new Layer(a[i].nodes, b[i].nodes, aMutation, bMutation);
      else
        layers[i] = new Layer(a[i].nodes, aMutation);
    }
    setMaxConnection();
  }

  void setMaxConnection() {
    this.maxConnection = 0;
    for (int l = 1; l < layers.Length; l++) {
      Layer currentLayer = layers[l];
      for (int n = 0; n < currentLayer.nodes.Length - 1; n++) {
        Node currentNode = currentLayer.nodes[n];
        for (int c = 0; c < currentNode.connections.Length - 1; c++) {
          if(Mathf.Abs(currentNode.connections[c]) > this.maxConnection)
            this.maxConnection = Mathf.Abs(currentNode.connections[c]);
        }
      }
    }
  }
  
  void loadInputs(float[] inputs) {
    for (int i = 0; i < inputs.Length; i++) {
      layers[0].nodes[i].value = inputs[i];
    }
    calculate();
  }
  
  void calculate() {
    for(int i = 0; i < prevOutputs.Length; i++)
      prevOutputs[i] = layers[layers.Length-1].nodes[i].value;

    for (int l = 1; l < layers.Length; l++) {
      Layer currentLayer = layers[l];
      for (int n = 0; n < currentLayer.nodes.Length - 1; n++) {
        Node currentNode = currentLayer.nodes[n];
        float sum = 0;
        for (int c = 0; c < currentNode.connections.Length; c++)
          sum += currentNode.connections[c]*layers[l-1].nodes[c].value;
        //sum /= sqrt(currentNode.connections.length);
        // ^Suggested in https://www.youtube.com/watch?v=8bNIkfRJZpo but he makes connection weights smaller when assigned
        // - I think this will be better but may cause problems when mutations cause layer sizes to change
        // ACTIVATION FUNCTION
        sum = 1/(1 + Mathf.Exp(-sum));
        currentNode.value = sum;
      }
    }
  }
  
  float returnOutput(string outputStr, bool prev) {
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

  void addLayer() {
    Layer[] newLayers = new Layer[layers.Length+1];
    newLayers[0] = layers[0];
    newLayers[1] = new Layer(layers[0].nodes.Length);
    for(int i = 1; i < layers.Length; i++)
      newLayers[i+1] = layers[i];
    this.layers = newLayers;
  }
  
  void printWeights() {
    for (int l = 1; l < layers.Length; l++) {
      Layer currentLayer = layers[l];
      Debug.Log("Layer " + l);
      for (int n = 0; n < currentLayer.nodes.Length - 1; n++) {
        Node currentNode = currentLayer.nodes[n];
        Debug.Log("[");
        for(int c = 0; c < currentNode.connections.Length; c++) {
          Debug.Log(string.Format("%10f, ", currentNode.connections[c]));
        }
        Debug.Log("]");
      }
    }
  }
  /*
  void display(float posX, float posY, float nWidth, float nHeight) {    
    noFill();//fill(255, 50);
    stroke(0, 100);
    strokeWeight(1);
    rect(posX, posY, nWidth, nHeight);

    float horizontalSpacing = nWidth/layers.length;
    float prevVerticalSpacing = 0;
    for (int l = 0; l < layers.length; l++) {
      Layer currentLayer = layers[l];
      float verticalSpacing = nHeight/(currentLayer.nodes.length + 1);
      for (int n = 0; n < currentLayer.nodes.length; n++) {
        Node currentNode = currentLayer.nodes[n];
        // Nodes
        fill(255/2.0 + currentNode.value*255/2.0);
        stroke(0);
        strokeWeight(1);
        // Active Outputs
        if(l == layers.length-1 && n < currentLayer.nodes.length-1) {
          String label = outputLabels[n];
          for(String s : moreThanNodes) {
            if(s.equals(label) && currentNode.value >= 0.5) {
              stroke(255);
              strokeWeight(2);
            }
          }
          for(String[] sArr : greatestNodes) {
            float maxVal = 0;
            boolean found = false;
            for(String s : sArr) {
              maxVal = max(maxVal, returnOutput(s, false));
              if(s.equals(label)) {
                found = true;
              }
            }
            if(found && currentNode.value == maxVal) {
              stroke(255);
              strokeWeight(2);
            }
          }
        }
        float circleSize = min(verticalSpacing, horizontalSpacing)/2;
        PVector nodePos = new PVector(posX + (l + 0.5)*horizontalSpacing, posY + (n + 1)*verticalSpacing);
        PVector mousePos = new PVector(mouseX, mouseY);
        if(nodePos.dist(mousePos) <= circleSize) {
          stroke(255);
          strokeWeight(3);
        }

        ellipse(nodePos.x, nodePos.y, circleSize, circleSize);
        textAlign(CENTER, CENTER);
        fill(0);
        textSize(circleSize/3);
        text(twosf(currentNode.value) + "", posX + (l + 0.5)*horizontalSpacing, posY + (n + 1)*verticalSpacing);
        // Connections
        for (int c = 0; c < currentNode.connections.length; c++) {
          if(toggleTransparentNNConnections.getState()) {
            float opacity = abs(255*currentNode.connections[c]*layers[l-1].nodes[c].value/maxConnection);
            if(currentNode.connections[c] < 0)
              stroke(255, 0, 0, opacity);
            else
              stroke(0, 255, 0, opacity);
            strokeWeight(5);
          } else {
            stroke(-currentNode.connections[c]*255, currentNode.connections[c]*255, 0, abs(currentNode.connections[c])*255/maxConnection);   // Max R/G when connection = -1 / 1
            strokeWeight(2);
          }
          line(posX + (l-0.5)*horizontalSpacing + min(prevVerticalSpacing, horizontalSpacing)/4, posY + (c + 1)*prevVerticalSpacing, posX + (l + 0.5)*horizontalSpacing - circleSize/2, posY + (n + 1)*verticalSpacing);
        }
      }
      prevVerticalSpacing = verticalSpacing;
    }
    
    //NODES
    for (int l = 0; l < layers.length; l++) {
      Layer currentLayer = layers[l];
      float verticalSpacing = nHeight/(currentLayer.nodes.length + 1);
      for (int n = 0; n < currentLayer.nodes.length; n++) {
        Node currentNode = currentLayer.nodes[n];
        
      }
    }
    
    // INPUT LABELS
    float verticalSpacing = nHeight/(layers[0].nodes.length + 1);
    for (int n = 0; n < layers[0].nodes.length-1; n++) {
      float circleSize = min(verticalSpacing/2, horizontalSpacing/2);
      textAlign(RIGHT, CENTER);
      fill(0);
      if (inputLabels != null)
        text(inputLabels[n], posX +  horizontalSpacing*0.5 - circleSize, posY + (n + 1)*verticalSpacing);
    }
    
    // OUTPUT LABELS
    verticalSpacing = nHeight/(layers[layers.length-1].nodes.length + 1);
    for (int n = 0; n < layers[layers.length-1].nodes.length-1; n++) {
      float circleSize = min(verticalSpacing/2, horizontalSpacing/2);
      textAlign(LEFT, CENTER);
      fill(0);
      if (outputLabels != null)
        text(outputLabels[n], posX + nWidth - horizontalSpacing*0.5 + circleSize, posY + (n + 1)*verticalSpacing);
    }
  }
  */
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
