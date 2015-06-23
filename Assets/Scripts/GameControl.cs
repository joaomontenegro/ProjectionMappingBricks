using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	public Vector3 brickPosition = new Vector3(-0.5f, -0.5f, 0f);
	public Vector3 brickSize = new Vector3(0.1f, 0.05f, 0.001f);
	public Vector3 brickScale = new Vector3(0.95f, 0.9f, 1f);
	public int nBricksX = 11;
	public int nBricksY = 20;
	public Vector2 velocity = new Vector2 (1f, 0f);

	private GameObject brick;
	private GameObject background;
	private GameObject lightCycle;
	private GameObject trail;

	private float halfHeight;
	private float initialY;
	private Transform brickXform;
	private GameObject[] bricks;
	private bool showBricks = false;
	private bool showBackground = false;
	private bool goingRight = true;

	// Use this for initialization
	void Start () {
		brick = GameObject.Find ("Brick");
		background = GameObject.Find ("GameQuad");
		lightCycle = GameObject.Find ("LightCycle");
		trail = GameObject.Find ("LightCycleTrail");

		halfHeight = ((float)Screen.height / (float)Screen.width) / 2f;
		initialY = brickPosition.y - brickSize.y / 2f;

		InitBackground ();
		InitLightCycle ();
	} 

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.B)) {
			CycleBackground();
		}

		UpdateLightCycle ();
	}

	void InitLightCycle() {
		lightCycle.transform.localPosition = new Vector3(-0.5f, initialY, -1f);
		lightCycle.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
		lightCycle.GetComponent<Rigidbody2D> ().velocity = velocity;
	}

	void UpdateLightCycle() {
		Vector3 lightCyclePos = lightCycle.transform.localPosition;
		bool changePos = false;

		if (lightCyclePos.x > 0.5f) {
			lightCycle.GetComponent<Rigidbody2D> ().velocity = -velocity;
			goingRight = false;
			lightCyclePos.y += brickSize.y;
			changePos = true;
		} else if (lightCyclePos.x < -0.5f) {
			lightCycle.GetComponent<Rigidbody2D> ().velocity = velocity;
			goingRight = true;
			lightCyclePos.y += brickSize.y;
			changePos = true;
		}

		if (lightCyclePos.y > halfHeight) {
			lightCyclePos.y = initialY;
			changePos = true;
		}

		if (changePos) {
			lightCycle.transform.localPosition = lightCyclePos;
		}

		Vector2 trailPos = new Vector3(0f, lightCyclePos.y, -1f);
		Vector2 trailScale = new Vector3(0.01f, 0.01f, 0.01f);

		if (goingRight) {
			trailPos.x = (-0.5f + lightCyclePos.x) / 2f;
			trailScale.x = (0.5f + trailPos.x) * 2;
		} else {
			trailPos.x = (0.5f + lightCyclePos.x) / 2f;
			trailScale.x = (0.5f - trailPos.x) * 2;
		}

		trail.transform.localPosition = trailPos;
		trail.transform.localScale = trailScale;
	}

	void InitBackground() {
		background.GetComponent<Renderer>().enabled = showBackground;

		//TODO: improve bricks scaling according to brick size
		Vector3 scaledBrickSize = brickSize;
		scaledBrickSize.x *= brickScale.x;
		scaledBrickSize.y *= brickScale.y;
		brick.transform.localScale = scaledBrickSize;

		int i = 0;
		bricks = new GameObject[nBricksX * nBricksY];
		Vector3 pos = brickPosition;

		for (int y = 0; y < nBricksY; y++) {
			for (int x = 0; x < nBricksX; x++) {
				GameObject brickInst = GameObject.Instantiate(brick);
				brickInst.GetComponent<Transform>().localPosition = pos;
				brickInst.GetComponent<Renderer>().enabled = showBricks;

				pos.x += brickSize.x;
				bricks[i++] = brickInst;
			}
			pos.y += brickSize.y;
			pos.x = -0.5f;

			if (y % 2 == 0) {
				pos.x += brickSize.x / 2;
			}
		}
	}

	void CycleBackground() {
		if (!showBricks) {
			showBricks = true;
		} else if (!showBackground) {
			showBackground = true;
		} else {
			showBricks = false;
			showBackground = false;
		}

		for (int i = 0; i < bricks.Length; i++) {
			bricks[i].GetComponent<Renderer>().enabled = showBricks;
		}

		background.GetComponent<Renderer>().enabled = showBackground;
	}
}