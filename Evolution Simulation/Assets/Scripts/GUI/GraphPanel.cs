using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;

public class GraphPanel : MonoBehaviour {
	public static List<List<int>> population;
    public static List<List<AgentRecord>> agentRecords;

    GameObject GraphTimeSlider;
    GameObject GraphTimeText;
    GameObject MaxAgentsText;
    GameObject InnerGraphPanel;
    Dropdown YDropdown;
    Dropdown XDropdown;
    GameObject YRangeText;
    GameObject XRangeText;

    void Awake() {
        resetStats();

        GraphTimeSlider = transform.Find("GraphTimeSlider").gameObject;
		GraphTimeText = transform.Find("GraphTimeText").gameObject;
		InnerGraphPanel = transform.Find("InnerGraphPanel").gameObject;
		YDropdown = transform.Find("YDropdown").gameObject.GetComponent<Dropdown>();
		XDropdown = transform.Find("XDropdown").gameObject.GetComponent<Dropdown>();
		YRangeText = transform.Find("YRangeText").gameObject;
		XRangeText = transform.Find("XRangeText").gameObject;
    }

  	public static void resetStats() {
  		population = new List<List<int>>();
        population.Add(new List<int>());
        population[0].Add(0);

        agentRecords = new List<List<AgentRecord>>();
        agentRecords.Add(new List<AgentRecord>());
    }

  	public static void recordStats() {
    	int popCount = Grid.agents.Count;

    	// If the last list in population has a legth of 100
    	if(Grid.time % (10 * Mathf.Pow(2, population.Count - 1)) == 0 && population[population.Count - 1].Count >= 100) {
    		// Add a new population list, copy over every other population and set the max value
    		population.Add(new List<int>());
    		int max = 0;
    		for(int i = 1; i < population[population.Count - 2].Count; i += 2) {
    			int val = population[population.Count - 2][i];
    			max = Mathf.Max(max, val);
    			population[population.Count - 1].Add(val);
    		}
    		population[population.Count - 1].Insert(0, max);

    		// Add a new agent records list, copy over every other record and set the max values
    		agentRecords.Add(new List<AgentRecord>());
    		for(int i = 2; i < agentRecords[agentRecords.Count - 2].Count; i += 2) {
    			agentRecords[agentRecords.Count - 1].Add(agentRecords[agentRecords.Count - 2][i]);
    		}
    	}

    	for(int i = 0; i < population.Count; i++) {
    		if(Grid.time % (10 * Mathf.Pow(2, i)) == 0) {
    			population[i].Insert(1, popCount);
    			if(population[i][0] < popCount)
					population[i][0] = popCount;
				if(population[i].Count > 101) {
					if(population[i][population[i].Count - 1] >= population[i][0]) {
						int max = 0;
						for(int j = 1; j < population[i].Count - 1; j++)
							max = Mathf.Max(max, population[i][j]);
						population[i][0] = max;
					}
					population[i].RemoveAt(population[i].Count - 1);
				}
    		}
    	}

    	for(int i = 0; i < agentRecords.Count; i++) {
    		while(agentRecords[i].Count > 2 && Grid.time - agentRecords[i][agentRecords[i].Count - 1].deathTick > 1000 * Mathf.Pow(2, i)) {
				agentRecords[i].RemoveAt(agentRecords[i].Count - 1);
    		}
    	}
    }

  	public void Update() {
  		if(InnerGraphPanel.activeInHierarchy) {
  			GraphTimeSlider.GetComponent<Slider>().maxValue = population.Count - 1;
  			int index = Mathf.RoundToInt(GraphTimeSlider.GetComponent<Slider>().value);
  			GraphTimeText.GetComponent<Text>().text = "Looking back " + 10 * Mathf.Pow(2, index) * (population[index].Count - 1) + " ticks";
  			clearGraph();

	  		float width = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x
	  					+ transform.GetComponent<RectTransform>().sizeDelta.x
	  					+ InnerGraphPanel.GetComponent<RectTransform>().sizeDelta.x;
			float height = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y
	  					 + transform.GetComponent<RectTransform>().sizeDelta.y
	  					 + InnerGraphPanel.GetComponent<RectTransform>().sizeDelta.y;

  			if(YDropdown.value == 0) {
	  			YRangeText.GetComponent<Text>().text = "0 - " + population[index][0];
	  			int lastRecord = Grid.time - (Grid.time % (10 * (int)Mathf.Pow(2, index))) + 10 * (int)Mathf.Pow(2, index);
	  			XRangeText.GetComponent<Text>().text = (lastRecord - 10 * Mathf.Pow(2, index) * (population[index].Count - 1)) + " - " + lastRecord; 

		        float hori = width / Mathf.Max(population[index].Count - 1, 1);
		        float vert = height / Mathf.Max(population[index][0], 1);
		        GameObject referenceConnection = (GameObject)Instantiate(Resources.Load("GUI/NetworkConnection"));
				for(int i = 1; i < population[index].Count - 1; i++) {
					GameObject connectionObj = (GameObject)Instantiate(referenceConnection, InnerGraphPanel.transform);
					UILineRenderer LineRenderer = connectionObj.GetComponent<UILineRenderer>();
					Vector2 point1 = new Vector2(width / 2f - (i - 1) * hori, population[index][i]   * vert - height / 2f);
					Vector2 point2 = new Vector2(width / 2f - i       * hori, population[index][i+1] * vert - height / 2f);
		    		List<Vector2> pointlist = new List<Vector2>(LineRenderer.Points);
		          	pointlist.Add(point1);
		          	pointlist.Add(point2);
		          	LineRenderer.Points = pointlist.ToArray();
				}
				Destroy(referenceConnection);
			} else {
				AgentRecord minRecord = new AgentRecord(true);
				AgentRecord maxRecord = new AgentRecord();
				for(int i = 0; i < agentRecords[index].Count; i++) {
					minRecord = minRecord.updateMinRecord(agentRecords[index][i]);
					maxRecord = maxRecord.updateMaxRecord(agentRecords[index][i]);
				}

				float yMin = minRecord.get(YDropdown.value-1);
				float xMin = minRecord.get(XDropdown.value);
				YRangeText.GetComponent<Text>().text = Mathf.Round(yMin * 100f) / 100f + " - " + Mathf.Round(maxRecord.get(YDropdown.value-1) * 100f) / 100f;
				XRangeText.GetComponent<Text>().text = Mathf.Round(xMin * 100f) / 100f + " - " + Mathf.Round(maxRecord.get(XDropdown.value) * 100f) / 100f;

				float hori = width / Mathf.Max(maxRecord.get(XDropdown.value) - xMin, 0.01f);
		        float vert = height / Mathf.Max(maxRecord.get(YDropdown.value-1) - yMin, 0.01f);
				GameObject referencePoint = (GameObject)Instantiate(Resources.Load("GUI/ScatterPoint"));
				for(int i = 1; i < agentRecords[index].Count; i++) {
					GameObject pointObj = (GameObject)Instantiate(referencePoint, InnerGraphPanel.transform);
					pointObj.transform.localPosition = new Vector2((agentRecords[index][i].get(XDropdown.value) - xMin) * hori   - width / 2f,
															  	   (agentRecords[index][i].get(YDropdown.value-1) - yMin) * vert - height / 2f);
					pointObj.GetComponent<Image>().color = Color.HSVToRGB(agentRecords[index][i].get(4), 1f, 1f);
				}
				Destroy(referencePoint);
			}
		}
  	}

  	public void YDropdownValueChanged() {
  		if(YDropdown.value == 0) {
  			XDropdown.value = 0;
  			XDropdown.interactable = false;
  		} else
  			XDropdown.interactable = true;
    }

  	void clearGraph() {
  		if(InnerGraphPanel != null) {
    		foreach(Transform child in InnerGraphPanel.transform)
                GameObject.Destroy(child.gameObject);
        }
  	}

    void OnDisable() {
        clearGraph();
    }
}
