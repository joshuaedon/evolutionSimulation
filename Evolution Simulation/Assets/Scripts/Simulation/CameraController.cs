using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {
    public static float minPanSpeed = 20f;
    public static float panBorderThickness = 10f;
    public static float panLimit = 30f;

    public static float scrollSpeed = 20f;
    public static float minY = 10f;
    public static float maxY = 120f;

    public static float rotateSpeed = 80f;

    public static bool follow = false;

    void Update() {
        Vector3 pos = transform.position;
        Vector3 camDir = Camera.main.transform.forward;
    	if(follow && SimulationManager.selectedAgent != null) {
    		// Locks the camera's position to focus on the selected agent
        	float dist = 2 * pos.y * Mathf.Tan(Mathf.PI / 6.0f);
        	Vector3 agentPos = SimulationManager.selectedAgent.agentObj.transform.position;
    		pos = new Vector3(agentPos.x - camDir.x * dist, pos.y, agentPos.z - camDir.z * dist);
    		transform.position = pos;
    	} else {
    		// Tandlate the camera perpendicular to the y plane
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
        }

        // Zoom the camera in/out
        if(!EventSystem.current.IsPointerOverGameObject()) {
        	pos += Vector3.Normalize(camDir) * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * 100f * Time.deltaTime;
	        // Cap the camera's height
	        if(GridController.GC == null) {
	        	Debug.Log("hhhuihuhuihiuh");
	        }
	        pos.x = Mathf.Clamp(pos.x, -panLimit, GridController.GC.cols + panLimit);
	        pos.y = Mathf.Clamp(pos.y, minY, maxY);
	        pos.z = Mathf.Clamp(pos.z, -panLimit, GridController.GC.rows + panLimit);
    	}
        transform.position = pos;

        // Rotate the camera
        if(Input.GetKey("e"))
            transform.RotateAround(pos + transform.forward*2*pos.y*Mathf.Sin(Mathf.PI / 6f), Vector3.up, Time.deltaTime*rotateSpeed);
        if(Input.GetKey("q"))
            transform.RotateAround(pos + transform.forward*2*pos.y*Mathf.Sin(Mathf.PI / 6f), Vector3.up, -Time.deltaTime*rotateSpeed);
    }

    public void toggleFollow(bool b) {
    	follow = b;
    }
}