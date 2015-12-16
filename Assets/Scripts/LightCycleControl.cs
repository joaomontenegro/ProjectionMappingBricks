using UnityEngine;
using System.Collections;



public class LightCycleControl : MonoBehaviour {
	public Intersection nextIntersetion = new Intersection(0, 0);
	public Direction direction = Direction.Right;
	public float speed;
	public float keepDirectionProbability = 0.75f;
	public int trailCount = 10;

	private GameControl gameControl;
	private GameObject trail;
	private Queue trailQueue = new Queue();

	// Use this for initialization
	void Start () {
		Init ();
	}

	// Update is called once per frame
	void Update () {
		// Move
		UpdatePosition ();

		if (gameControl.IsAtIntersection(transform.localPosition, nextIntersetion, direction)) {
			UpdateDirection();
			nextIntersetion = gameControl.GetNextIntersection(nextIntersetion, direction);
		}
	}

	void Awake(){
		trail = transform.Find("Trail").gameObject;
	}

	public void Init() {
		gameControl = GameObject.Find("Game").GetComponent<GameControl>();
		int ix = Random.Range (0, gameControl.nBricksX * 2);
		int iy = Random.Range (0, gameControl.nBricksY);
		nextIntersetion =  new Intersection(ix, iy);
		PlaceAtIntersection();
		clearTrail();
	}

	public void SetColor(Color color) {
		Renderer renderer = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();
		renderer.material.color = color;

		Renderer trailRenderer = trail.transform.GetChild(0).GetComponent<Renderer>();
		trailRenderer.material.color = color;
	}

	public void clearTrail() {
		while (trailQueue.Count > 0) {
			GameObject.DestroyImmediate(trailQueue.Dequeue() as GameObject);
		}
	}

	/*********** Private **************/
	void UpdatePosition() {
		float stepSize = Time.deltaTime * speed;
		Vector3 localPosition = transform.localPosition;

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

		UpdateTrailScale(stepSize);
	}

	void UpdateTrailScale(float stepSize) {

		// Update the trail position
		Vector3 trailScale = trail.transform.localScale;
		trailScale.x += stepSize;
		trail.transform.localScale = trailScale;
	}

	void UpdateDirection() {
		Direction[] allowedDirections = gameControl.AllowedDirections (nextIntersetion, direction);
		
		// Check if the current direction is allowed (if it is, then it at position 0)
		int startIndex = 0;
		if(allowedDirections[0] == direction) {
			if (Random.value < keepDirectionProbability) {
				return;
			}
			
			if (allowedDirections.Length > 1) {
				// Ignore the current direction (at position 0)
				startIndex = 1;
			}
		}
		
		// Update the trail before applying the rotation
		UpdateTrailDirection();
		
		// Otherise randomly select an allowed direction
		direction = allowedDirections [Random.Range (startIndex, allowedDirections.Length)];
		
		switch (direction) {
		case Direction.Up:
			transform.eulerAngles = new Vector3(0, 0, 90);
			break;
		case Direction.Right:
			transform.eulerAngles = new Vector3(0, 0, 0);
			break;
		case Direction.Down:
			transform.eulerAngles = new Vector3(0, 0, -90);
			break;
		case Direction.Left:
			transform.eulerAngles = new Vector3(0, 0, 180);
			break;
		}
		
		PlaceAtIntersection();
	}

	void UpdateTrailDirection() {
		// Duplicate trail
		GameObject trailSection = GameObject.Instantiate(trail);
		trailSection.transform.localPosition = trail.transform.position;
		trailSection.transform.localRotation = trail.transform.rotation;
		trailSection.transform.localScale = trail.transform.lossyScale;

		trailQueue.Enqueue(trailSection);
		
		if (trailQueue.Count > trailCount) {
			GameObject toDiscard = trailQueue.Dequeue() as GameObject;
			GameObject.DestroyImmediate(toDiscard);
		}
		
		// Reset Trail scale
		Vector3 trailScale = trail.transform.localScale;
		trailScale.x = 0;
		trail.transform.localScale = trailScale;
		
	}

	void PlaceAtIntersection() {
		transform.localPosition = gameControl.GetPositionFromIndices (nextIntersetion);
	}
}
