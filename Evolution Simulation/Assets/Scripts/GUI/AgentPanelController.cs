using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;
using System.Linq;

public class AgentPanelController : MonoBehaviour {
	string[] inputLabels = {"Random", "Hunger", "Health",
							"Stepped forward", "Turned left", "Turned right", "Ate", "Reproduced", "Attacked",
							"", "", "", "", "", "", "", "", "",
							"Constant"};
	string[] sensePositionLabels = {"Front Left", "Front", "Front Right", "Left", "Below", "Right", "Back Left", "Back", "Back Right"};
	string[] senseThingLabels = {"Food", "Water", "Agent"};
    string[] outputLabels = {"Forwards", "Left", "Right", "Eat", "Reproduce", "Attack"};
    bool isHighlight;
	Agent agent;
    NeuralNetwork network;
    GameObject NodesPanel;
    public static GameObject ConnectionsPanel;

	public void OnEnable() {
		if(SimulationManager.selectedAgent != null) {
			// senseChunks = new List<Chunk>();
			isHighlight = false;

  			// Store the selected agent and its neural network 
  			agent = SimulationManager.selectedAgent;
  			network = agent.network;

  			transform.Find("GenerationText").GetComponent<Text>().text = "Generation: " + agent.generation;
  			transform.Find("MutateAmountText").GetComponent<Text>().text = "Mutate amount: " + Mathf.Round(network.mutateAmount * 1000f) / 1000f;

  			// Store game objects for the nodes and connections panels
    		NodesPanel 	  	 = transform.Find("NodesPanel").gameObject;
    		ConnectionsPanel = transform.Find("ConnectionsPanel").gameObject;
    		
    		resetNetwork();
  		}
  	}

  	public void resetNetwork() {
        if(NodesPanel != null) {
    		foreach(Transform child in NodesPanel.transform)
                GameObject.Destroy(child.gameObject);
    		foreach(Transform child in ConnectionsPanel.transform)
    			GameObject.Destroy(child.gameObject);
        }
        // Find the max number of nodes in a single layer
		int maxNodes = 0;
		for(int i = 0; i < network.layers.Length; i++)
			maxNodes = Mathf.Max(maxNodes, network.layers[i].nodes.Length);
			// Get the panel width and height
			float width = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x
						+ transform.GetComponent<RectTransform>().sizeDelta.x
						+ NodesPanel.GetComponent<RectTransform>().sizeDelta.x
						- 50;
			float height = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y
						 + transform.GetComponent<RectTransform>().sizeDelta.y
						 + NodesPanel.GetComponent<RectTransform>().sizeDelta.y
                     - 5;
			// Calculate the horizontal spacing between the layers when spaced evenly
			float horizontalSpacing = 0.5f * width / network.layers.Length;
			// Calculate the maximum size the nodes can be while not overlapping
		float nodeSize = height / maxNodes;
		nodeSize = Mathf.Min(nodeSize, horizontalSpacing);

    	// Store reference objects for the nodes, connections and lebels
		GameObject referenceNode 	    = (GameObject)Instantiate(Resources.Load("GUI/NetworkNode"));
		GameObject referenceConnection  = (GameObject)Instantiate(Resources.Load("GUI/NetworkConnection"));
		GameObject referenceInputLabel  = (GameObject)Instantiate(Resources.Load("GUI/InputLabel"));
		GameObject referenceOutputLabel = (GameObject)Instantiate(Resources.Load("GUI/OutputLabel"));
		referenceNode.GetComponent<RectTransform>().sizeDelta = new Vector2(nodeSize, nodeSize);
   		// Create all the nodes and connections of the selected agent's neural network
		for(int l = 0; l < network.layers.Length; l++) {
        	for(int n = 0; n < network.layers[l].nodes.Length; n++) {
    		    Node curNode = network.layers[l].nodes[n];
				GameObject nodeObj = (GameObject)Instantiate(referenceNode, NodesPanel.transform);
				nodeObj.transform.localPosition = new Vector2((2.0f * l + 1 - network.layers.Length) * horizontalSpacing, nodeSize * (network.layers[l].nodes.Length / 2.0f - n - 0.5f));
				curNode.nodeObject = nodeObj;
				curNode.display();
				for(int c = 0; c < curNode.nodes.Length; c++) {
				    GameObject connectionObj = (GameObject)Instantiate(referenceConnection, ConnectionsPanel.transform);
  					UILineRenderer LineRenderer = connectionObj.GetComponent<UILineRenderer>();
  					Vector2 point1 = new Vector2(curNode.nodeObject.transform.localPosition.x, curNode.nodeObject.transform.localPosition.y);
  					Vector2 point2 = new Vector2(curNode.nodes[c].nodeObject.transform.localPosition.x, curNode.nodes[c].nodeObject.transform.localPosition.y);
            		List<Vector2> pointlist = new List<Vector2>(LineRenderer.Points);
		          	pointlist.Add(point1);
		          	pointlist.Add(point2);
		          	LineRenderer.Points = pointlist.ToArray();
					curNode.connectionObjects[c] = connectionObj;	
				}
    		}
  		}
  		// Create input and output labels
  		for(int n = 0; n < inputLabels.Length; n++) {
			GameObject labelObj = (GameObject)Instantiate(referenceInputLabel, NodesPanel.transform);
			labelObj.transform.localPosition = new Vector2((1 - network.layers.Length) * horizontalSpacing - nodeSize/2 - 47, nodeSize * (inputLabels.Length / 2.0f - n - 0.5f));
			if(n <= 8)
				labelObj.GetComponent<Text>().text = inputLabels[n];
			else if(n == inputLabels.Length-1)
				labelObj.GetComponent<Text>().text = inputLabels[n];
			else {
				labelObj.GetComponent<Text>().text = senseThingLabels[agent.senseThings[n-9]] + " " + sensePositionLabels[agent.sensePositions[n-9]];
				network.layers[0].nodes[n].colour = agent.senseThings[n-9]+1;
			}
			network.layers[0].nodes[n].display();
		}
		for(int n = 0; n < outputLabels.Length; n++) {
			GameObject labelObj = (GameObject)Instantiate(referenceOutputLabel, NodesPanel.transform);
			labelObj.transform.localPosition = new Vector2((2.0f * (network.layers.Length-1) + 1 - network.layers.Length) * horizontalSpacing + nodeSize/2 + 47, nodeSize * ((outputLabels.Length+1) / 2.0f - n - 0.5f));
			labelObj.GetComponent<Text>().text = outputLabels[n];
		}
  		Destroy(referenceNode);
  		Destroy(referenceConnection);
  		Destroy(referenceInputLabel);
  		Destroy(referenceOutputLabel);
  		network.setConnectionColours();
    }

    void Update() {
    	if(this.agent != null) {
	    	// Update the agent's hunger bar
	    	transform.Find("HungerBar").GetComponent<Slider>().value = agent.hunger;
	    	transform.Find("HealthBar").GetComponent<Slider>().value = agent.health;
	    	transform.Find("TicksAliveText").GetComponent<Text>().text = "Ticks alive: " + agent.ticksAlive;
	    	transform.Find("LandSeaText").GetComponent<Text>().text = "Land(1) / sea(0): " + (GridController.GC.seaAgents ? Mathf.Round(agent.landSea * 100f) / 100f : 1f);

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
	            if(isHighlight && !highlighted)
	                unhighlight();
	        } else if(isHighlight) {
	            unhighlight();
	        }
	    } else {
	    	transform.Find("HungerBar").GetComponent<Slider>().value = 0;
	    	transform.Find("HealthBar").GetComponent<Slider>().value = 0;
	    }
    }

    public void toggleNNFlow(bool b) {
    	SimulationManager.NNFlow = b;
    	network.setConnectionColours();
    }

    public void mutateAgent() {
    	network.mutateValue(0.1f);
    	agent.changeColour();
    	resetNetwork();
    }

    public void reproduceAgent() {
    	agent.reproduce();
    }

    void highlightNode(GameObject node) {
        isHighlight = true;
        Cursor.visible = false;
        for(int l = 0; l < network.layers.Length; l++) {
            for(int n = 0; n < network.layers[l].nodes.Length; n++) {
                Node curNode = network.layers[l].nodes[n];
                // if(curNode.nodeObject == node)
    			// 	Debug.Log(curNode.layerNum + ", " + curNode.nodeNum);
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
        Cursor.visible = true;
        for(int l = 0; l < network.layers.Length; l++) {
            for(int n = 0; n < network.layers[l].nodes.Length; n++) {
                Node curNode = network.layers[l].nodes[n];
                for(int c = 0; c < curNode.nodes.Length; c++) {
                    curNode.connectionObjects[c].SetActive(true);
                }
            }
        }
    }

    void OnDisable() {
        if(NodesPanel != null) {
    		foreach(Transform child in NodesPanel.transform)
                GameObject.Destroy(child.gameObject);
    		foreach(Transform child in ConnectionsPanel.transform)
    			GameObject.Destroy(child.gameObject);
        }
    }
}
