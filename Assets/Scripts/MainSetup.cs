using UnityEngine;
using System.Collections;

public class MainSetup : MonoBehaviour {

	private Camera editorCamera;
	private Camera gameCamera;

	// Use this for initialization
	void Start () {
		// Get the cameras
		editorCamera = GameObject.Find("EditorCamera").GetComponent<Camera>();
		gameCamera = GameObject.Find("GameCamera").GetComponent<Camera>();

		// Enable the game camera by default
		editorCamera.enabled = true;
		gameCamera.enabled = false;

		InitCameraSize();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			SwitchCamera();
		}
	}

	// Adjust the camera size to the aspect ratio
	void InitCameraSize() {

		// Calculate the camera size according to the aspect ratio
		float aspectRatio = (float)Screen.width / (float)Screen.height;
		float cameraSize;
		if (aspectRatio > 1f) {
			cameraSize = 0.5f / aspectRatio;
		} else {
			cameraSize = 0.5f * aspectRatio;
		}

		// Adjust the Editor and Game camera size so that the
		// canvases cover the whole screen without any deformation.
		editorCamera.orthographicSize = cameraSize;
		gameCamera.orthographicSize = cameraSize;
	}

	// Switches between the game camera and the editor camera
	void SwitchCamera() {
		editorCamera.enabled = !editorCamera.enabled;
		gameCamera.enabled = !gameCamera.enabled;
	}

}
