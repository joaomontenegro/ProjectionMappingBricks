using UnityEngine;
using System.Collections;

public enum Direction{Up, Right, Down, Left};

public class Intercection {
	public int ix;
	public int iy;
	
	public Intercection(int ix, int iy) {
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

	public float lightCycleSize = 0.01f;
	public float trailSize = 0.005f;
	public float speed = 0.7f;

	private GameObject brick;
	private GameObject[] bricks = new GameObject[0];
	private Transform brickXform;
	
	private Material backgroundMaterial;

	private bool showBricks = false;
	private bool showBackground = false;

	// Use this for initialization
	void Start () {
		brick = GameObject.Find ("Brick");
		backgroundMaterial = GameObject.Find ("GameQuad").GetComponent<Renderer>().material;
		backgroundMaterial.color = new Color (0, 0, 0);

		// Light Cycle objects
		GameObject lightCycle = GameObject.Find ("LightCycle");
		lightCycle.transform.localScale = new Vector3 (lightCycleSize, lightCycleSize, lightCycleSize);
		LightCycleControl lightCycleControl = lightCycle.GetComponent<LightCycleControl> ();
		lightCycleControl.speed = speed;

		Init ();
	} 

	// Update is called once per frame
	new void Update () {
		base.Update ();

		ApplyKeyChanges ();
	}

	public void Init() {
		InitBricks ();

		//TODO
		GameObject lightCycle = GameObject.Find ("LightCycle");
		lightCycle.GetComponent<LightCycleControl> ().Init ();
	}

	//******* Game Board API *******//

	//TODO use a Position struct
	public Vector3 GetPositionFromIndices(Intercection indices, float z=0f) {
		float x = indices.ix * brickSize.x / 2 + brickPosition.x;
		float y = indices.iy * brickSize.y + brickPosition.y - brickSize.y / 2f;
		return new Vector3 (x, y, z);
	}

	//TODO
	public bool isAtIntersection(Vector3 position, Intercection intercection, Direction direction) {
		Vector3 intercectionPosition = GetPositionFromIndices (intercection);
		switch (direction) {
		case Direction.Up:
			return position.y > intercectionPosition.y;
		case Direction.Right:
			return position.x > intercectionPosition.x;
		case Direction.Down:
			return position.y < intercectionPosition.y;
		case Direction.Left:
			return position.x < intercectionPosition.x;
		}

		return false;
	}

	//TODO
	public Direction[] AllowedDirections(Intercection intercection, Direction currentDirection) {
		Direction[] allowedDirections = new Direction[0];

		//TODO: better corners
		if (intercection.ix == 0) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Right;

		} else if (intercection.ix == nBricksX * 2 - 1) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Left;

		} else if (intercection.iy == 0) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Up;
			
		} else if (intercection.iy == nBricksY - 1) {
			allowedDirections = new Direction[1];
			allowedDirections [0] = Direction.Down;

		} else if (currentDirection == Direction.Up || currentDirection == Direction.Down) {
			allowedDirections = new Direction[2];
			allowedDirections [0] = Direction.Right;
			allowedDirections [1] = Direction.Left;

		} else {
			if ((intercection.ix % 2 == 0 && intercection.iy % 2 == 0) 
			    	|| (intercection.ix % 2 == 1 && intercection.iy % 2 == 1)) {
				allowedDirections = new Direction[2];
				allowedDirections [0] = Direction.Down;
				allowedDirections [1] = currentDirection;

			} else if ((intercection.ix % 2 == 0 && intercection.iy % 2 == 1) 
			        || (intercection.ix % 2 == 1 && intercection.iy % 2 == 0)) {
				allowedDirections = new Direction[2];
				allowedDirections [0] = Direction.Up;
				allowedDirections [1] = currentDirection;

			}
		}

		return allowedDirections;
	}

	//TODO
	public Intercection GetNextIntercection(Intercection intercetion, Direction direction) {
		Intercection nextIntercection = new Intercection(intercetion.ix, intercetion.iy);

		switch (direction) {
		case Direction.Up:
			nextIntercection.iy++;
			break;
		case Direction.Right:
			nextIntercection.ix++;
			break;
		case Direction.Down:
			nextIntercection.iy--;
			break;
		case Direction.Left:
			nextIntercection.ix--;
			break;
		}

		return nextIntercection;
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
			backgroundMaterial.color = new Color (1, 1, 1);
		} else {
			backgroundMaterial.color = new Color (0, 0, 0);
		}
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