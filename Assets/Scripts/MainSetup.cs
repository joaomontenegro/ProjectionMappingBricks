using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class MainSetup : MonoBehaviour {

	public string settingsFilePath;

	private Camera editorCamera;
	private Camera gameCamera;
	private Camera renderTextureCamera;
	
	private GameControl gameControl;
	private EditorControl editorControl;

	private int nVerticesX = 0;
	private int nVerticesY = 0;
	
	// Use this for initialization
	void Start () {
		// Get references to other scripts and objects
		gameControl = GameObject.Find ("Game").GetComponent<GameControl>();
		editorControl = GameObject.Find ("Editor").GetComponent<EditorControl>();

		// Initialize the settings file path
		settingsFilePath = Application.dataPath;
		if (Application.platform == RuntimePlatform.OSXPlayer) {
			settingsFilePath += "/../..";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer) {
			settingsFilePath += "/..";
		}
		settingsFilePath += "/settings.txt";

		// Init the cameras
		editorCamera = GameObject.Find("EditorCamera").GetComponent<Camera>();
		gameCamera = GameObject.Find("GameCamera").GetComponent<Camera>();
		renderTextureCamera = GameObject.Find("RenderTextureCamera").GetComponent<Camera>();
		InitCameras();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			SwitchCamera();
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			Save();
		}
		
		if (Input.GetKeyDown (KeyCode.L)) {
			Load();
		}

	}

	// Adjust the camera size to the aspect ratio, etc
	void InitCameras() {
		renderTextureCamera.aspect = 1;

		// Enable the game camera by default
		editorCamera.enabled = true;
		gameCamera.enabled = false;

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

	//**** IO ****//
	
	public void Save() {
		string text = "";

		text += "NBricks: " + gameControl.nBricksX + " " + gameControl.nBricksY + "\n";
		text += "BrickPosition: " + gameControl.brickPosition.x + " " + gameControl.brickPosition.y + "\n";
		text += "BrickSize: " + gameControl.brickSize.x + " " + gameControl.brickSize.y + "\n";
		text += "LightCycleSize: " + gameControl.lightCycleSize + "\n";
		text += "TrailSize: " + gameControl.trailSize + "\n";
		text += "Speed: " + gameControl.speed + "\n";
		text += "NVertices: " + editorControl.nVerticesX + " "+ editorControl.nVerticesY + "\n";
		text += "Vertices: " + editorControl.meshUtils.GetVerticesString(editorControl.mesh) + "\n";

		System.IO.File.WriteAllText (settingsFilePath, text);
	}
	

	public void Load() {
		FileInfo fileInfo = new FileInfo (settingsFilePath);
		StreamReader reader = fileInfo.OpenText();

		string line = reader.ReadLine();
		do {
			LoadLine(line);
			line = reader.ReadLine();
		} while (line != null);

		reader.Close ();

		editorControl.Init ();
		gameControl.Init ();
	}

	private bool LoadLine(string line) {

		string[] words = line.Split(' ');

		switch (words[0]) {
		case "NBricks:":
			return LoadNBricks(words);
		case "BrickPosition:":
			return LoadBrickPosition(words);
		case "BrickSize:":
			return LoadBrickSize(words);
		case "LightCycleSize:":
			return LoadLightCycleSize(words);
		case "TrailSize:":
			return LoadTrailSize(words);
		case "Speed:":
			return LoadSpeed(words);
		case "NVertices:":
			return LoadNVertices(words);
		case "Vertices:":
			return LoadVertices(words);
		}

		Debug.LogError("Unknown word:" + words[0]);
		return false;
	}

	private bool LoadNBricks(string[] words) {
		if (words.Length != 3) {
			Debug.LogError("LoadNBricks: wrong number of words:" + words.Length);
			return false;
		}
		int.TryParse(words [1], out gameControl.nBricksX);
		int.TryParse(words [2], out gameControl.nBricksY);
		return true;
	}

	private bool LoadBrickPosition(string[] words) {
		if (words.Length != 3) {
			Debug.LogError("LoadBrickPosition: wrong number of words:" + words.Length);
			return false;
		}
		float.TryParse(words [1], out gameControl.brickPosition.x);
		float.TryParse(words [2], out gameControl.brickPosition.y);
		return true;
	}

	private bool LoadBrickSize(string[] words) {
		if (words.Length != 3) {
			Debug.LogError("LoadBrickSize: wrong number of words:" + words.Length);
			return false;
		}
		float.TryParse(words [1], out gameControl.brickSize.x);
		float.TryParse(words [2], out gameControl.brickSize.y);
		return true;
	}

	private bool LoadLightCycleSize(string[] words) {
		if (words.Length != 2) {
			Debug.LogError("LoadLightCycleSize: wrong number of words:" + words.Length);
			return false;
		}
		float.TryParse(words [1], out gameControl.lightCycleSize);
		return true;
	}

	private bool LoadTrailSize(string[] words) {
		if (words.Length != 2) {
			Debug.LogError("LoadTrailSize: wrong number of words:" + words.Length);
			return false;
		}
		float.TryParse(words [1], out gameControl.trailSize);
		return true;
	}

	private bool LoadSpeed(string[] words) {
		if (words.Length != 2) {
			Debug.LogError("LoadSpeed: wrong number of words:" + words.Length);
			return false;
		}
		float.TryParse(words [1], out gameControl.speed);
		return true;
	}

	private bool LoadNVertices(string[] words) {
		if (words.Length != 3) {
			Debug.LogError("LoadNVertices: wrong number of words:" + words.Length);
			return false;
		}
		int.TryParse(words [1], out nVerticesX);
		int.TryParse(words [1], out nVerticesY);

		return true;
	}

	private bool LoadVertices(string[] words) {
		int nVertices = nVerticesX * nVerticesY;
		int nFloats = nVertices * 3 + 1;

		if (words.Length != nFloats) {
			Debug.LogError("LoadVertices: wrong number of words:" + words.Length + " expected:" + nFloats);
			return false;
		}

		Vector3[] vertices = new Vector3[nVertices];
		int f = 1;

		for (int v = 0; v < nVertices; v++) {
			float.TryParse(words [f++], out vertices[v].x);
			float.TryParse(words [f++], out vertices[v].y);
			float.TryParse(words [f++], out vertices[v].z);
		}

		editorControl.mesh.vertices = vertices;

		return true;
	}
}
