using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour {
    void Start() {
        GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        GameObject referenceAgent = (GameObject)Instantiate(Resources.Load("Agent"));
        for(int i = 0; i < 20; i++) {
            int x, z;
            int count = 0;
            do {
                x = Random.Range(0, gridController.cols);
                z = Random.Range(0, gridController.rows);
                count++;
            } while((gridController.gridArray[x, z].isWater() || gridController.gridArray[x, z].agent != null) && count < 1000);
            if(count < 1000) {
                GameObject agent = (GameObject)Instantiate(referenceAgent, transform);
                gridController.gridArray[x, z].agent = agent;
                float y = (float)gridController.gridArray[x, z].elevation;
                agent.transform.position = new Vector3(x, gridController.yScale * y, z);
            } else {
                Debug.Log("Agent could not be spawned");
            }
        }
        Destroy(referenceAgent);
    }

    void Update() {
        
    }
}
