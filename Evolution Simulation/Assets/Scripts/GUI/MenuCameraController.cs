using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraController : MonoBehaviour {
    void Start() {
        transform.position = new Vector3((GridController.cols - 1) / 2.0f, 60, (GridController.rows - 1) / 2.0f - 35);
    }

    void Update() {
        transform.RotateAround(transform.position + transform.forward*70, Vector3.up, Time.deltaTime*10);
    }
}
