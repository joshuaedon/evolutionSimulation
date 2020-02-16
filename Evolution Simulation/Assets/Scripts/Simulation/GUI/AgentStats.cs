using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentStats : MonoBehaviour {
		void OnEnable() {
				if(GridController.selectedAgent != null) {
	    		NeuralNetwork n = GridController.selectedAgent.network;

	    		int maxNodes = 0;
	    		for(int i = 0; i < n.layers.Length; i++)
	    				maxNodes = Mathf.Max(maxNodes, n.layers[i].nodes.Length);
	    		float nodeSize = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y/maxNodes;
	    		nodeSize = Mathf.Min(nodeSize, (transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x - 400)/(2*n.layers.Length));

	    		GameObject referenceLayer = (GameObject)Instantiate(Resources.Load("Simulation/GUI/NetworkLayer"));
	    		GameObject referenceNode = (GameObject)Instantiate(Resources.Load("Simulation/GUI/NetworkNode"));
	    		referenceNode.GetComponent<RectTransform>().sizeDelta = new Vector2(nodeSize, nodeSize);
	    		for(int i = 0; i < n.layers.Length; i++) {
	        		GameObject layerObj = (GameObject)Instantiate(referenceLayer, transform);
	        		for(int j = 0; j < n.layers[i].nodes.Length; j++) {
	        				GameObject nodeObj = (GameObject)Instantiate(referenceNode, layerObj.transform);
	        		}
	      	}
	      }

      	

		    /*float prevVerticalSpacing = 0;
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
		    }*/
    }

    void OnDisable() {
    		foreach(Transform child in transform)
    				GameObject.Destroy(child.gameObject);
    }
}
