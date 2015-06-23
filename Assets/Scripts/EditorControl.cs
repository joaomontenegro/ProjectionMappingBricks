using UnityEngine;
using System.Collections;

public class EditorControl : MouseMonoBehaviour {

	public int nVerticesX = 30;
	public int nVerticesY = 30;
	public Vector2 selectionArea = new Vector2 (0.2f, 0.2f);
	public float selectionSoftness = 1.5f;
	public string meshFilePath;

	private MeshUtils meshUtils;
	private MeshFilter meshFilter;
	private GameObject selectionQuad;
	float[] vertexWeights;

	// Use this for initialization
	void Start () {

		meshUtils = new MeshUtils ();
		meshFilter = GameObject.Find("EditorQuad").GetComponent<MeshFilter>();
		selectionQuad = GameObject.Find("SelectionQuad");

		meshUtils.InitPlane(meshFilter.mesh, nVerticesX, nVerticesY);

		meshFilePath = Application.dataPath;
		if (Application.platform == RuntimePlatform.OSXPlayer) {
			meshFilePath += "/../..";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer) {
			meshFilePath += "/..";
		}
		
		meshFilePath += "/mesh.txt";
	}
	
	// Update is called once per frame
	new void Update () {
		base.Update ();

		// Key events
		if (Input.GetKeyDown (KeyCode.S)) {
			meshUtils.SaveMesh(meshFilter.mesh, meshFilePath, nVerticesX, nVerticesY);
		}

		if (Input.GetKeyDown (KeyCode.L)) {
			meshUtils.LoadMesh(meshFilter.mesh, meshFilePath, nVerticesX, nVerticesY);
		}
	}


	//**** Dragging Events ****//
	public override void OnDragStart (int button, Vector2 pos) {
		if (button == 0) {
			vertexWeights = meshUtils.GetVertexWeights(meshFilter.mesh, pos);
		} else if (button == 1) {
			selectionQuad.transform.localPosition = new Vector3(pos.x, pos.y, -1);
			selectionQuad.transform.localScale = new Vector3(0, 0, 0);
			selectionQuad.GetComponent<Renderer>().enabled = true;
		}
	}
	
	public override void OnDragFinish(int button, Vector2 pos, Vector2 dragStartPos) {
		if (button == 1) {
			selectionQuad.GetComponent<Renderer>().enabled = false;
		}
	}
	
	public override void OnDrag(int button, Vector2 fromPos, Vector2 toPos, Vector2 dragStartPos) {
		if (button == 0) {
			Vector3 dragDirection = toPos - fromPos;
			dragDirection.z = 0;

			meshUtils.DeformMesh(meshFilter.mesh, dragDirection, vertexWeights);
			
		} else if (button == 1) {
			selectionArea = toPos - dragStartPos;
			
			if (selectionArea.x < 0f) {selectionArea.x *= -1;}
			if (selectionArea.y < 0f) {selectionArea.y *= -1;}
			
			if (selectionArea.x > 0f || selectionArea.y > 0f) {
				selectionQuad.transform.localScale = new Vector3(selectionArea.x * 2, selectionArea.y * 2, 1);
			}

			meshUtils.SetSelectionArea(selectionArea);
		}
	}
}
