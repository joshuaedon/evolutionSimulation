using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour {
    void Start() {
        GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        GameObject referenceAgent = (GameObject)Instantiate(Resources.Load("Chunk"));
        for(int i = 0; i < 20; i++) {
            int x, z;
            int count = 0;
            do {
                x = Random.Range(0, gridController.cols);
                z = Random.Range(0, gridController.rows);
                count++;
            } while(gridController.gridArray[x, z].agent != null && count < 1000);
            if(gridController.gridArray[x, z].agent == null) {
                GameObject agent = (GameObject)Instantiate(referenceAgent, transform);
                gridController.gridArray[x, z].agent = agent;
                agent.transform.position = new Vector3(x, 30, z);
            }
        }
        Destroy(referenceAgent);
    }

    void Update() {
        
    }
}
