using UnityEngine;

public class CameraController : MonoBehaviour {

    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public float panLimit = 10f;

    public float scrollSpeed = 20f;
    public float minY = 10f;
    public float maxY = 120f;

    void Update() {
        Vector3 pos = transform.position;
        
        // Time.deltaTime = time ellapsed since last frame
        if(Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
            pos.z += panSpeed * Time.deltaTime;
        if(Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
            pos.z -= panSpeed * Time.deltaTime;
        if(Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
            pos.x += panSpeed * Time.deltaTime;
        if(Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
            pos.x -= panSpeed * Time.deltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        //pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;
        pos += Vector3.Normalize(Camera.main.transform.forward) * scroll * scrollSpeed * 100f * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, -(GridManager.cols/2 + panLimit), GridManager.cols/2 + panLimit);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -(GridManager.rows/2 + panLimit), GridManager.rows/2 + panLimit);
        transform.position = pos;
    }
}
