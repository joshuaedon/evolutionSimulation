using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent {
    public GameObject agentObj;

    public Agent(GameObject agentObj, int col, int row) {
        this.agentObj = agentObj;

        GridController gridController = GameObject.Find("Grid").GetComponent<GridController>();
        float elivation = (float)gridController.gridArray[col, row].elevation;
        agentObj.transform.position = new Vector3(col, gridController.yScale * elivation, row);
    }
}
