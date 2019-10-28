using UnityEngine;

public class CameraController : MonoBehaviour {

    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public float panLimit = 10f;

    public float scrollSpeed = 20f;
    public float minY = 10f;
    public float maxY = 120f;

    public float rotateSpeed = 20f;

    void Update() {
        Vector3 pos = transform.position;
        Vector3 camDir = Camera.main.transform.forward;
        Vector3 flatCamDir = new Vector3(camDir.x, 0, camDir.z);

        //Time.deltaTime = time ellapsed since last frame
        if(Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
            pos += flatCamDir * panSpeed * Time.deltaTime;
        flatCamDir = Quaternion.Euler(0, -90, 0) * flatCamDir;
        if(Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
            pos += flatCamDir * panSpeed * Time.deltaTime;
        flatCamDir = Quaternion.Euler(0, -90, 0) * flatCamDir;
        if(Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
            pos += flatCamDir * panSpeed * Time.deltaTime;
        flatCamDir = Quaternion.Euler(0, -90, 0) * flatCamDir;
        if(Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
            pos += flatCamDir * panSpeed * Time.deltaTime;

        pos += Vector3.Normalize(camDir) * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * 100f * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, -(GridController.cols/2 + panLimit), GridController.cols/2 + panLimit);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -(GridController.rows/2 + panLimit), GridController.rows/2 + panLimit);
        transform.position = pos;

        if(Input.GetKey("e") || Input.mousePosition.x <= panBorderThickness)
            transform.RotateAround(pos, Vector3.up, Time.deltaTime*rotateSpeed * 2f);
        if(Input.GetKey("q") || Input.mousePosition.x <= panBorderThickness)
            transform.RotateAround(pos, Vector3.up, -Time.deltaTime*rotateSpeed * 2f);
    }
}