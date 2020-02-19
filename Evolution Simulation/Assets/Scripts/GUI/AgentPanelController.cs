using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;

public class AgentPanelController : MonoBehaviour {
    NeuralNetwork network;
    GameObject NodesPanel;
    GameObject ConnectionsPanel;
    bool isHighlight = false;

	public void OnEnable() {
		if(SimulationManager.selectedAgent != null) {
  			// Store the selected agent's neural network 
  			network = SimulationManager.selectedAgent.network;

  			// Store tha game objects of the node and connection panels
    		NodesPanel 	  	 = transform.Find("NodesPanel").gameObject;
    		ConnectionsPanel = transform.Find("ConnectionsPanel").gameObject;

    		// Find the max number of nodes in a single layer
    		int maxNodes = 0;
    		for(int i = 0; i < network.layers.Length; i++)
  			maxNodes = Mathf.Max(maxNodes, network.layers[i].nodes.Length);
  			// Get the panel width and height
  			float width = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x
  						+ transform.GetComponent<RectTransform>().sizeDelta.x
  						+ NodesPanel.GetComponent<RectTransform>().sizeDelta.x;
  			float height = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y
  						 + transform.GetComponent<RectTransform>().sizeDelta.y
  						 + NodesPanel.GetComponent<RectTransform>().sizeDelta.y
                         - 5;
  			// Calculate the horizontal spacing between the layers when spaced evenly
  			float horizontalSpacing = 0.5f * width / network.layers.Length;
  			// Calculate the maximum size the nodes can be while not overlapping
    		float nodeSize = height / maxNodes;
    		nodeSize = Mathf.Min(nodeSize, horizontalSpacing);

        // Store reference objects for the nodes and connections
    		GameObject referenceNode 	   = (GameObject)Instantiate(Resources.Load("GUI/NetworkNode"));
    		GameObject referenceConnection = (GameObject)Instantiate(Resources.Load("GUI/NetworkConnection"));
    		referenceNode.GetComponent<RectTransform>().sizeDelta = new Vector2(nodeSize, nodeSize);
        // Create all the nodes and connections of the selected agent's neural network
    		for(int l = 0; l < network.layers.Length; l++) {
            for(int n = 0; n < network.layers[l].nodes.Length; n++) {
        		    Node curNode = network.layers[l].nodes[n];
        				GameObject nodeObj = (GameObject)Instantiate(referenceNode, NodesPanel.transform);
        				nodeObj.transform.localPosition = new Vector2((2.0f * l + 1 - network.layers.Length) * horizontalSpacing, nodeSize * (network.layers[l].nodes.Length / 2.0f - n - 0.5f));
        				curNode.nodeObject = nodeObj;
        				for(int c = 0; c < curNode.nodes.Length; c++) {
        				    GameObject connectionObj = (GameObject)Instantiate(referenceConnection, ConnectionsPanel.transform);
          					UILineRenderer LineRenderer = connectionObj.GetComponent<UILineRenderer>();
          					Vector2 point1 = new Vector2(curNode.nodeObject.transform.localPosition.x, curNode.nodeObject.transform.localPosition.y);
          					Vector2 point2 = new Vector2(curNode.nodes[c].nodeObject.transform.localPosition.x, curNode.nodes[c].nodeObject.transform.localPosition.y);
                    List<Vector2> pointlist = new List<Vector2>(LineRenderer.Points);
    			          pointlist.Add(point1);
    			          pointlist.Add(point2);
    			          LineRenderer.Points = pointlist.ToArray();
    			          if(GridController.GC.transpNNConnections) {
    				            float opacity = Mathf.Abs(curNode.weights[c] * curNode.nodes[c].value / network.maxWeight);
    				            if(curNode.weights[c] < 0)
    				                LineRenderer.color = new Color(1.0f, 0.0f, 0.0f, opacity);
    				            else
    				                LineRenderer.color = new Color(0.0f, 1.0f, 0.0f, opacity);
    				        } else
    			            	LineRenderer.color = new Color(-curNode.weights[c], curNode.weights[c], 0, Mathf.Abs(curNode.weights[c]) / network.maxWeight);
        					  curNode.connectionObjects[c] = connectionObj;	
        				}
        		}
      		}
      		Destroy(referenceNode);
      		Destroy(referenceConnection);
  		}
  	}

    void Update() {
        // Highlight a node if the mouse is over it
        if(EventSystem.current.IsPointerOverGameObject()) {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> raycastResultsList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResultsList);

            bool highlighted = false;
            for(int i = 0; i < raycastResultsList.Count; i++) {
                if(raycastResultsList[i].gameObject.name == "NetworkNode(Clone)(Clone)") {
                    highlightNode(raycastResultsList[i].gameObject);
                    highlighted = true;
                }
            }
            if(!highlighted)
                unhighlight();
        } else if(isHighlight) {
            unhighlight();
        }
    }

    void highlightNode(GameObject node) {
        isHighlight = true;
        for(int l = 0; l < network.layers.Length; l++) {
            for(int n = 0; n < network.layers[l].nodes.Length; n++) {
                Node curNode = network.layers[l].nodes[n];
                for(int c = 0; c < curNode.nodes.Length; c++) {
                    if(curNode.nodeObject == node || curNode.nodes[c].nodeObject == node)
                        curNode.connectionObjects[c].SetActive(true);
                    else
                        curNode.connectionObjects[c].SetActive(false);
                }
            }
        }
    }

    void unhighlight() {
        isHighlight = false;
        for(int l = 0; l < network.layers.Length; l++) {
            for(int n = 0; n < network.layers[l].nodes.Length; n++) {
                Node curNode = network.layers[l].nodes[n];
                for(int c = 0; c < curNode.nodes.Length; c++) {
                    curNode.connectionObjects[c].SetActive(true);
                }
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

    void OnDisable() {
        if(NodesPanel != null) {
    		foreach(Transform child in NodesPanel.transform)
                GameObject.Destroy(child.gameObject);
    		foreach(Transform child in ConnectionsPanel.transform)
    			GameObject.Destroy(child.gameObject);
        }
    }
}
