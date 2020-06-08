using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
    public static float defaultPanSpeed = 2f;
    public static float panBorderThickness = 10f;
    public static float panLimit = 30f;

    public static float zoomSpeed = 5f;
    public static float minY = 10f;
    public static float maxY = 120f;

    public static bool follow = false;

    // Android support
    Vector3 touchPrev; 
    int touchCountPrev; 

    void Update() {
        Vector3 pos = transform.position;

    	if(follow && AgentPanel.selectedAgent != null) {
        	Vector3 agentPos = AgentPanel.selectedAgent.transform.position;
    		transform.position = new Vector3(agentPos.x, agentPos.y, transform.position.z);
    	} else {
    		// Pan camera with keyboard
	        float panSpeed = defaultPanSpeed * GetComponent<Camera>().orthographicSize;
	        if(Input.GetKey("w")/* || Input.mousePosition.y >= Screen.height - panBorderThickness*/)
	            pos.y += panSpeed * Time.deltaTime;
	        if(Input.GetKey("a")/* || Input.mousePosition.x <= panBorderThickness*/)
	            pos.x -= panSpeed * Time.deltaTime;
	        if(Input.GetKey("s")/* || Input.mousePosition.y <= panBorderThickness*/)
	            pos.y -= panSpeed * Time.deltaTime;
	        if(Input.GetKey("d")/* || Input.mousePosition.x >= Screen.width - panBorderThickness*/)
	            pos.x += panSpeed * Time.deltaTime;

           	// Android support
			if(Input.GetMouseButtonDown(0) && !IsPointerOverUIObject()) {
	       		// Set initial position of touch
	       		touchCountPrev = Input.touchCount;
				touchPrev = Input.mousePosition;
			}
			// Make sure position is different so clicking doesn't set following to null
			if(Input.GetMouseButton(0) && !IsPointerOverUIObject() && (touchPrev.x != Input.mousePosition.x) && (touchPrev.y != Input.mousePosition.y)) {
				if(touchCountPrev <= 1 && Input.touchCount <= 1)
					pos += new Vector3(touchPrev.x - Input.mousePosition.x, touchPrev.y - Input.mousePosition.y, 0) * GetComponent<Camera>().orthographicSize * 2 / Screen.height;
				// Update touch position so that the camera stops moving when the user's finger does
	       		touchCountPrev = Input.touchCount;
				touchPrev = Input.mousePosition;
			}
        }

        // Zoom the camera in/out
        if(!IsPointerOverUIObject() && !Input.GetKey(KeyCode.LeftControl))
        	GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * 100f * Time.deltaTime;
    	// Android support
		if(Input.touchCount == 2) {
			Vector2 touchZeroPrevPos = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
			Vector2 touchOnePrevPos  = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;

			float prevMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float curMag = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;

			GetComponent<Camera>().orthographicSize -= (curMag - prevMag) * zoomSpeed * Time.deltaTime;
		}

        // Cap the camera's position
        GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize, 1f, 100f);
        pos.x = Mathf.Clamp(pos.x, -(SettingsPanel.cols/2f + panLimit), SettingsPanel.cols/2f + panLimit);
        pos.y = Mathf.Clamp(pos.y, -(SettingsPanel.rows/2f + panLimit), SettingsPanel.rows/2f + panLimit);

        transform.position = pos;
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