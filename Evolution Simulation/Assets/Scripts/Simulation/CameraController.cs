using UnityEngine;

public class CameraController : MonoBehaviour {

    public static float minPanSpeed = 20f;
    public static float panBorderThickness = 10f;
    public static float panLimit = 30f;

    public static float scrollSpeed = 20f;
    public static float minY = 10f;
    public static float maxY = 120f;

    public static float rotateSpeed = 80f;

    void Update() {
        Vector3 pos = transform.position;
        Vector3 camDir = Camera.main.transform.forward;
        Vector3 flatCamDir = new Vector3(camDir.x, 0, camDir.z);
        float panSpeed = minPanSpeed * Mathf.Sqrt(pos.y - minY + 1);

        //Time.deltaTime = time ellapsed since last frame
        if(Input.GetKey("w")/* || Input.mousePosition.y >= Screen.height - panBorderThickness*/)
            pos += flatCamDir * panSpeed * Time.deltaTime;
        flatCamDir = Quaternion.Euler(0, -90, 0) * flatCamDir;
        if(Input.GetKey("a")/* || Input.mousePosition.x <= panBorderThickness*/)
            pos += flatCamDir * panSpeed * Time.deltaTime;
        flatCamDir = Quaternion.Euler(0, -90, 0) * flatCamDir;
        if(Input.GetKey("s")/* || Input.mousePosition.y <= panBorderThickness*/)
            pos += flatCamDir * panSpeed * Time.deltaTime;
        flatCamDir = Quaternion.Euler(0, -90, 0) * flatCamDir;
        if(Input.GetKey("d")/* || Input.mousePosition.x >= Screen.width - panBorderThickness*/)
            pos += flatCamDir * panSpeed * Time.deltaTime;

        pos += Vector3.Normalize(camDir) * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * 100f * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, -panLimit, GridController.GC.cols + panLimit);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit, GridController.GC.rows + panLimit);
        transform.position = pos;

        if(Input.GetKey("e"))
            transform.RotateAround(pos + transform.forward*70, Vector3.up, Time.deltaTime*rotateSpeed);
        if(Input.GetKey("q"))
            transform.RotateAround(pos + transform.forward*70, Vector3.up, -Time.deltaTime*rotateSpeed);
    }
}