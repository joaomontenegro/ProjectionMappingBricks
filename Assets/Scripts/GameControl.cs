using UnityEngine;
using System.Collections;

public enum Direction{Up, Right, Down, Left};

public enum CameraAnimation{None, IntoPersp, IntoOrtho};

public class Intersection {
	public int ix;
	public int iy;
	
	public Intersection(int ix, int iy) {
		this.ix = ix;
		this.iy = iy;
	}
}

public class GameControl : MouseMonoBehaviour {

	public Vector3 brickPosition = new Vector3(-0.45f, -0.35f, 0f);
	public Vector3 brickSize = new Vector3(0.0615f, 0.0201f, 0.001f);
	public Vector3 brickScale = new Vector3(0.95f, 0.9f, 1f);
	public int nBricksX = 20;
	public int nBricksY = 37;

	public float brickPositionDragFactor = 0.1f;
	public float brickSizeDragFactor = 0.01f;

	public float trailSize = 0.005f;
	public float speed = 0.7f;
	public float cameraSpeed = 1f;

	public int nLightCycles = 2;

	private GameObject brick;
	private GameObject[] bricks = new GameObject[0];
	private Transform brickXform;

	private int nIntersectionsX;
	private int nIntersectionsY;

	private Material backgroundMaterial;
	private Material brickMaterial;

	private bool showBricks = false;
	private bool showBackground = false;

	private CameraAnimation cameraAnim = CameraAnimation.None;
	private Camera gameCamera;

	private GameObject[] lightCycles;
	
	// Use this for initialization
	void Start () {
		nIntersectionsX = nBricksX * 2 + 1;
		nIntersectionsY = nBricksY + 1;

		brick = GameObject.Find ("Brick");
		brickMaterial = brick.GetComponent<Renderer>().material;

		backgroundMaterial = GameObject.Find ("GameQuad").GetComponent<Renderer>().material;
		backgroundMaterial.color = new Color (0, 0, 0);

		// Light Cycle objects
		GameObject lightCycle = GameObject.Find ("LightCycle");
		LightCycleControl lightCycleControl = lightCycle.GetComponent<LightCycleControl> ();
		lightCycleControl.speed = speed;

		lightCycles = new GameObject[nLightCycles];
		lightCycles [0] = lightCycle;

		// Create the remainder instances of the light cycle
		for(int i = 1; i < nLightCycles; i++) {
			lightCycles[i] = GameObject.Instantiate(lightCycle);
		}

		InitLightCyclesModel();

		gameCamera = GameObject.Find ("RenderTextureCamera").GetComponent<Camera>();

		Init ();
	} 

	// Update is called once per frame
	new void Update () {
		base.Update ();

		ApplyKeyChanges ();
		AnimateCamera();
	}

	public void Init() {
		InitBricks ();

		for (int i = 0; i < nLightCycles; i++) {
			lightCycles [i].GetComponent<LightCycleControl>().Init ();
		}
	}

	//******* Game Board API *******//

	//TODO use a Position struct
	public Vector3 GetPositionFromIndices(Intersection indices, float z=0f) {
		float x = indices.ix * brickSize.x / 2 + brickPosition.x;
		float y = indices.iy * brickSize.y + brickPosition.y - brickSize.y / 2f;
		return new Vector3 (x, y, z);
	}

	//TODO
	public bool IsAtIntersection(Vector3 position, Intersection intersection, Direction direction) {
		Vector3 intersectionPosition = GetPositionFromIndices (intersection);
		bool result = false;
		switch (direction) {
		case Direction.Up:
			return position.y > intersectionPosition.y;
		case Direction.Right:
			return position.x > intersectionPosition.x;
		case Direction.Down:
			return position.y < intersectionPosition.y;
		case Direction.Left:
			return position.x < intersectionPosition.x;
		}

		return result;
	}


	//TODO
	public Direction[] AllowedDirections(Intersection intersection, Direction currentDirection) {
		Direction[] allowedDirections = new Direction[0];

		//TODO: better corners
		if (intersection.ix == 0) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Right;

		} else if (intersection.ix == nIntersectionsX - 1) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Left;

		} else if (intersection.iy == 0) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Up;
			
		} else if (intersection.iy == nIntersectionsY - 1) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Down;

		} else if (currentDirection == Direction.Up || currentDirection == Direction.Down) {
			allowedDirections = new Direction[2];
			allowedDirections [0] = Direction.Right;
			allowedDirections [1] = Direction.Left;

		} else {
			if ((intersection.ix % 2 == 0 && intersection.iy % 2 == 0) 
			    	|| (intersection.ix % 2 == 1 && intersection.iy % 2 == 1)) {
				allowedDirections = new Direction[2];
				allowedDirections [0] = currentDirection;
				allowedDirections [1] = Direction.Down;


			} else if ((intersection.ix % 2 == 0 && intersection.iy % 2 == 1) 
			        || (intersection.ix % 2 == 1 && intersection.iy % 2 == 0)) {
				allowedDirections = new Direction[2];
				allowedDirections [0] = currentDirection;
				allowedDirections [1] = Direction.Up;


			}
		}

		return allowedDirections;
	}

	//TODO
	public Intersection GetNextIntersection(Intersection intersetion, Direction direction) {
		Intersection nextIntersection = new Intersection(intersetion.ix, intersetion.iy);

		switch (direction) {
		case Direction.Up:
			nextIntersection.iy++;
			break;
		case Direction.Right:
			nextIntersection.ix++;
			break;
		case Direction.Down:
			nextIntersection.iy--;
			break;
		case Direction.Left:
			nextIntersection.ix--;
			break;
		}

		return nextIntersection;
	}

	//**** Light Cycles ****//

	void InitLightCyclesModel() {

		Color[] colors = {new Color(0, 0.3f, 1), new Color(1, 0, 0), new Color(1, 0.75f, 0)};

		for (int i = 0; i < lightCycles.Length; i++) {
            lightCycles[i].GetComponent<LightCycleControl>().SetColor(colors[i % colors.Length]);
            lightCycles[i].GetComponent<LightCycleControl>().SetModelIndex(i % 4);
        }
	}

	//**** Bricks and Background ****//
	
	void InitBricks() {
		for (int i = 0; i < bricks.Length; i++) {
			GameObject.DestroyImmediate(bricks[i]);
		}

		bricks = new GameObject[nBricksX * nBricksY];
		
		for (int i = 0; i < bricks.Length; i++) {
			bricks[i] = GameObject.Instantiate(brick);
			bricks[i].GetComponent<Renderer>().enabled = showBricks;
		}

		UpdateBricks ();
	}
	
	void UpdateBricks() {
		Vector3 scaledBrickSize = brickSize;
		scaledBrickSize.x *= brickScale.x;
		scaledBrickSize.y *= brickScale.y;
		
		int i = 0;
		Vector3 pos = brickPosition;
		for (int y = 0; y < nBricksY; y++) {
			for (int x = 0; x < nBricksX; x++) {
				GameObject brickInst = bricks[i++];
				brickInst.GetComponent<Transform>().localPosition = pos;
				brickInst.GetComponent<Transform>().localScale = scaledBrickSize;
				pos.x += brickSize.x;
			}
			pos.y += brickSize.y;
			pos.x = brickPosition.x;
			
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
		
		if (showBackground) {
			backgroundMaterial.color = new Color (0.5f, 0.5f, 0.5f);
			brickMaterial.color = new Color(0, 0, 0);
		} else {
			backgroundMaterial.color = new Color (0, 0, 0);
			brickMaterial.color = new Color(0.75f, 0, 0);
		}
	}

	void CycleCamera() {

		if (gameCamera.orthographic) {
			cameraAnim = CameraAnimation.IntoPersp;
			gameCamera.orthographic = false;
		} else {
			cameraAnim = CameraAnimation.IntoOrtho;
		}
	}

	void AnimateCamera() {
		if (cameraAnim == CameraAnimation.None) {
			return;
		}

		Vector3 targetTranslate = new Vector3(0, 0, -2); // Ortho position
		if (cameraAnim == CameraAnimation.IntoPersp) {
			targetTranslate = new Vector3(0, -0.5f, -0.5f); // Persp position
		}

		if (Vector3.Distance(gameCamera.transform.position, targetTranslate) < 0.001f) {
			gameCamera.transform.localPosition = targetTranslate;
			if (cameraAnim == CameraAnimation.IntoOrtho) {
				gameCamera.orthographic = true;
			}
			cameraAnim = CameraAnimation.None;
		}

		gameCamera.transform.position = Vector3.MoveTowards(gameCamera.transform.position, targetTranslate, Time.deltaTime * cameraSpeed);
		gameCamera.transform.LookAt(new Vector3(0, 0, 0));

		float distance = gameCamera.transform.position.magnitude;
		gameCamera.fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan(0.5f / distance);
	}

	//**** Dragging and Key Events ****//	

	bool GetControlModifier () {
		return (Input.GetKey (KeyCode.LeftControl)
		        || Input.GetKey (KeyCode.RightControl));
	}
	
	bool GetShiftModifier () {
		return (Input.GetKey (KeyCode.LeftShift)
		        || Input.GetKey (KeyCode.RightShift));
	}

	public override void OnDragStart (int button, Vector2 pos) {}

	public override void OnDragFinish(int button, Vector2 pos, Vector2 dragStartPos) {}

	public override void OnDrag(int button, Vector2 fromPos, Vector2 toPos, Vector2 dragStartPos) {
		Vector2 delta = toPos - fromPos;
		bool update = false;
		bool controlModifier = GetControlModifier ();
		bool shiftModifier = GetShiftModifier ();

		if (controlModifier) {
			brickPosition.x += delta.x * brickPositionDragFactor;
			brickPosition.y += delta.y * brickPositionDragFactor;
			update = true;
		} else if (shiftModifier) {
			brickSize.x += delta.x * brickSizeDragFactor;
			brickSize.y += delta.y * brickSizeDragFactor;
			update = true;
		}

		if (update) {
			UpdateBricks ();
		}
	}

	void ApplyKeyChanges() {
		bool update = false;
		bool controlModifier = GetControlModifier ();
		bool shiftModifier = GetShiftModifier ();

		if (Input.GetKeyDown(KeyCode.B)) {
			CycleBackground();
		} else if (Input.GetKeyDown(KeyCode.C)) {
			CycleCamera();
		}

		if (Input.GetKey(KeyCode.LeftArrow)) {
			if (controlModifier) {
				brickPosition.x -= 0.001f * brickPositionDragFactor;
				update = true;
			} else if (shiftModifier) {
				brickSize.x -= 0.001f * brickSizeDragFactor;
				update = true;
			}

		} else if (Input.GetKey(KeyCode.RightArrow)) {
			if (controlModifier) {
				brickPosition.x += 0.001f * brickPositionDragFactor;
				update = true;
			} else if (shiftModifier) {
				brickSize.x += 0.001f * brickSizeDragFactor;
				update = true;
			}

		} else if (Input.GetKey(KeyCode.DownArrow)) {
			if (controlModifier) {
				brickPosition.y -= 0.001f * brickPositionDragFactor;
				update = true;
			} else if (shiftModifier) {
				brickSize.y -= 0.001f * brickSizeDragFactor;
				update = true;
			}

		} else if (Input.GetKey(KeyCode.UpArrow)) {
			if (controlModifier) {
				brickPosition.y += 0.001f * brickPositionDragFactor;
				update = true;
			} else if (shiftModifier) {
				brickSize.y += 0.001f * brickSizeDragFactor;
				update = true;
			}
		}

		if (update) {
			UpdateBricks ();
		}
	}
}