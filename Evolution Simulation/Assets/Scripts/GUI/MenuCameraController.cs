using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraController : MonoBehaviour {
    void Update() {
        transform.RotateAround(transform.position + transform.forward*70, Vector3.up, Time.deltaTime*10);
    }
}
