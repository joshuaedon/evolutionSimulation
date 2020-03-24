using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;

public class GraphPanelController : MonoBehaviour {
    GameObject GraphTimeSlider;
    GameObject GraphTimeText;
    GameObject MaxAgentsText;
    GameObject InnerGraphPanel;
    Dropdown YDropdown;
    Dropdown XDropdown;
    GameObject YRangeText;
    GameObject XRangeText;

	public void OnEnable() {
		GraphTimeSlider = transform.Find("GraphTimeSlider").gameObject;
		GraphTimeText = transform.Find("GraphTimeText").gameObject;
		InnerGraphPanel = transform.Find("InnerGraphPanel").gameObject;
		YDropdown = transform.Find("YDropdown").gameObject.GetComponent<Dropdown>();
		XDropdown = transform.Find("XDropdown").gameObject.GetComponent<Dropdown>();
		YRangeText = transform.Find("YRangeText").gameObject;
		XRangeText = transform.Find("XRangeText").gameObject;
  	}

  	public void Update() {
  		if(InnerGraphPanel.activeInHierarchy) {
  			GraphTimeSlider.GetComponent<Slider>().maxValue = GridController.GC.population.Count - 1;
  			int index = Mathf.RoundToInt(GraphTimeSlider.GetComponent<Slider>().value);
  			GraphTimeText.GetComponent<Text>().text = "Looking back " + 10 * Mathf.Pow(2, index) * (GridController.GC.population[index].Count - 1) + " ticks";
  			clearGraph();

	  		float width = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x
	  					+ transform.GetComponent<RectTransform>().sizeDelta.x
	  					+ InnerGraphPanel.GetComponent<RectTransform>().sizeDelta.x;
			float height = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y
	  					 + transform.GetComponent<RectTransform>().sizeDelta.y
	  					 + InnerGraphPanel.GetComponent<RectTransform>().sizeDelta.y;

  			if(YDropdown.value == 0) {
	  			YRangeText.GetComponent<Text>().text = "0 - " + GridController.GC.population[index][0];
	  			int lastRecord = GridController.GC.time - (GridController.GC.time % (10 * (int)Mathf.Pow(2, index))) + 10 * (int)Mathf.Pow(2, index);
	  			XRangeText.GetComponent<Text>().text = (lastRecord - 10 * Mathf.Pow(2, index) * (GridController.GC.population[index].Count - 1)) + " - " + lastRecord; 

		        float hori = width / Mathf.Max(GridController.GC.population[index].Count - 1, 1);
		        float vert = height / Mathf.Max(GridController.GC.population[index][0], 1);
		        GameObject referenceConnection = (GameObject)Instantiate(Resources.Load("GUI/NetworkConnection"));
				for(int i = 1; i < GridController.GC.population[index].Count - 1; i++) {
					GameObject connectionObj = (GameObject)Instantiate(referenceConnection, InnerGraphPanel.transform);
					UILineRenderer LineRenderer = connectionObj.GetComponent<UILineRenderer>();
					Vector2 point1 = new Vector2(width / 2f - (i - 1) * hori, GridController.GC.population[index][i]   * vert - height / 2f);
					Vector2 point2 = new Vector2(width / 2f - i       * hori, GridController.GC.population[index][i+1] * vert - height / 2f);
		    		List<Vector2> pointlist = new List<Vector2>(LineRenderer.Points);
		          	pointlist.Add(point1);
		          	pointlist.Add(point2);
		          	LineRenderer.Points = pointlist.ToArray();
				}
				Destroy(referenceConnection);
			} else {
				AgentRecord minRecord = new AgentRecord(true);
				AgentRecord maxRecord = new AgentRecord();
				for(int i = 0; i < GridController.GC.agentRecords[index].Count; i++) {
					minRecord = minRecord.updateMinRecord(GridController.GC.agentRecords[index][i]);
					maxRecord = maxRecord.updateMaxRecord(GridController.GC.agentRecords[index][i]);
				}

				float yMin = minRecord.get(YDropdown.value-1);
				float xMin = minRecord.get(XDropdown.value);
				YRangeText.GetComponent<Text>().text = Mathf.Round(yMin * 100f) / 100f + " - " + Mathf.Round(maxRecord.get(YDropdown.value-1) * 100f) / 100f;
				XRangeText.GetComponent<Text>().text = Mathf.Round(xMin * 100f) / 100f + " - " + Mathf.Round(maxRecord.get(XDropdown.value) * 100f) / 100f;

				float hori = width / Mathf.Max(maxRecord.get(XDropdown.value) - xMin, 0.01f);
		        float vert = height / Mathf.Max(maxRecord.get(YDropdown.value-1) - yMin, 0.01f);
				GameObject referencePoint = (GameObject)Instantiate(Resources.Load("GUI/ScatterPoint"));
				for(int i = 1; i < GridController.GC.agentRecords[index].Count; i++) {
					GameObject pointObj = (GameObject)Instantiate(referencePoint, InnerGraphPanel.transform);
					pointObj.transform.localPosition = new Vector2((GridController.GC.agentRecords[index][i].get(XDropdown.value) - xMin) * hori   - width / 2f,
															  	   (GridController.GC.agentRecords[index][i].get(YDropdown.value-1) - yMin) * vert - height / 2f);
					pointObj.GetComponent<Image>().color = Color.HSVToRGB(GridController.GC.agentRecords[index][i].get(4), 1f, 1f);
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
