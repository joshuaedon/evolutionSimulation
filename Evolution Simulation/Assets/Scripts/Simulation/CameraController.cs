using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
    public static float minPanSpeed = 20f;
    public static float panBorderThickness = 10f;
    public static float panLimit = 30f;

    public static float scrollSpeed = 20f;
    public static float minY = 10f;
    public static float maxY = 120f;

    public static float rotateSpeed = 80f;

    public static bool follow = false;

    // Android support
    Vector3 touchPrev; 
    int touchCountPrev; 

    void Update() {
        Vector3 pos = transform.position;
        Vector3 camDir = Camera.main.transform.forward;
    	if(follow && SimulationManager.selectedAgent != null) {
    		// Locks the camera's position to focus on the selected agent
        	float dist = 2 * pos.y * Mathf.Tan(Mathf.PI / 6.0f);
        	Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        	// forward.RotateAround(Vector3.zero, Vector3.up, 90);
        	Vector3 agentPos = SimulationManager.selectedAgent.agentObj.transform.position + Quaternion.AngleAxis(90, Vector3.up)*forward*10;
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

           	// Android support
			if(Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
				touchPrev = Input.mousePosition;
			if(Input.GetMouseButton(0) && !IsPointerOverUIObject()) {
				if(touchCountPrev <= 1 && Input.touchCount <= 1)
					pos += new Vector3((touchPrev.x - Input.mousePosition.x) / Screen.width, 0, (touchPrev.y - Input.mousePosition.y) / Screen.height) * 100f;
				// Update touch position so that the camera stops moving when the user's finger does
	       		touchCountPrev = Input.touchCount;
				touchPrev = Input.mousePosition;
			}
        }

        // Zoom the camera in/out
        if(!IsPointerOverUIObject() && !Input.GetKey(KeyCode.LeftControl))
        	pos += Vector3.Normalize(camDir) * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * 100f * Time.deltaTime;
    	// Android support
		if(Input.touchCount == 2) {
			Vector2 touchZeroPrevPos = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
			Vector2 touchOnePrevPos  = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;

			float prevMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float curMag = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;

			pos += Vector3.Normalize(camDir) * (curMag - prevMag) * scrollSpeed * Time.deltaTime;
		}
        // Cap the camera's height
        pos.x = Mathf.Clamp(pos.x, -panLimit, GridController.GC.cols + panLimit);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit, GridController.GC.rows + panLimit);
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

    private bool IsPointerOverUIObject() {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}