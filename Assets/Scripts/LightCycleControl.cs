using UnityEngine;
using System.Collections;

public class LightCycleControl : MonoBehaviour {

	public Intercection nextIntercetion = new Intercection(0, 0);
	public Direction direction = Direction.Right;
	public float speed;

	private GameControl gameControl;

	// Use this for initialization
	void Start () {
		gameControl = GameObject.Find("Game").GetComponent<GameControl>();
		Init ();
	}
	
	// Update is called once per frame
	void Update () {

		// Move
		step ();

		if (gameControl.isAtIntersection(transform.localPosition, nextIntercetion, direction)) {
			PlaceAtIntersection(); // TODO: only when direction changes?
			UpdateDirection();
			nextIntercetion = gameControl.GetNextIntercection(nextIntercetion, direction);
		}
	}

	public void Init() {
		nextIntercetion.ix = nextIntercetion.iy = 10;
		PlaceAtIntersection();
	}

	//TODO: make abstract?
	public void UpdateDirection() {
		Direction[] allowedDirections = gameControl.AllowedDirections (nextIntercetion, direction);
		if (allowedDirections.Length == 0) {
			return;
		} else if (allowedDirections.Length == 1) {
			direction = allowedDirections [0];
		} else {
			direction = allowedDirections [Random.Range (0, allowedDirections.Length)];
		}		
	}

	/*********** Private **************/

	private void step() {
		Vector3 localPosition = transform.localPosition;
		float stepSize = Time.deltaTime * speed;
		switch (direction) {
		case Direction.Up:
			localPosition.y += stepSize;
			break;
		case Direction.Right:
			localPosition.x += stepSize;
			break;
		case Direction.Down:
			localPosition.y -= stepSize;
			break;
		case Direction.Left:
			localPosition.x -= stepSize;
			break;
		}

		transform.localPosition = localPosition;
	}

	private void PlaceAtIntersection() {
		transform.localPosition = gameControl.GetPositionFromIndices (nextIntercetion);
	}
}
