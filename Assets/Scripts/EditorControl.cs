using UnityEngine;
using System.Collections;

public class EditorControl : MouseMonoBehaviour {

	public int nVerticesX = 30;
	public int nVerticesY = 30;
	public Vector2 selectionArea = new Vector2 (0.2f, 0.2f);
	public float selectionSoftness = 1.5f;
	public Mesh mesh;

	public MeshUtils meshUtils;

	private GameObject selectionQuad;
	float[] vertexWeights;

	// Use this for initialization
	void Start () {
		GameObject editorQuad = GameObject.Find ("EditorQuad");

		meshUtils = new MeshUtils ();
		mesh = editorQuad.GetComponent<MeshFilter>().mesh;

		selectionQuad = GameObject.Find("SelectionQuad");

		// Initalize the size of the render texture
		RenderTexture rt = editorQuad.GetComponent<Renderer> ().material.mainTexture as RenderTexture;
		int resolution = (int)Mathf.Max (Screen.width, Screen.height) * 2;
		rt.width = resolution;
		rt.height = resolution;

		Init ();
	}

	// Update is called once per frame
	new void Update () {
		base.Update ();
	}

	// If vertices==null then initialize the vertices with their original positions.
	public void Init(Vector3[] vertices = null) {
		meshUtils.InitPlane(mesh, nVerticesX, nVerticesY, vertices);
	}

	public void SetVertices(Vector3[] vertices) {
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
	}

	//**** Dragging Events ****//
	public override void OnDragStart (int button, Vector2 pos) {
		if (hasKeyModifiers ()) {
			return;
		}

		if (button == 0) {
			vertexWeights = meshUtils.GetVertexWeights(mesh, pos);
		} else if (button == 1) {
			selectionQuad.transform.localPosition = new Vector3(pos.x, pos.y, -1);
			selectionQuad.transform.localScale = new Vector3(0, 0, 0);
			selectionQuad.GetComponent<Renderer>().enabled = true;
		}
	}
	
	public override void OnDragFinish(int button, Vector2 pos, Vector2 dragStartPos) {
		if (hasKeyModifiers ()) {
			return;
		}

		if (button == 1) {
			selectionQuad.GetComponent<Renderer>().enabled = false;
		}
	}
	
	public override void OnDrag(int button, Vector2 fromPos, Vector2 toPos, Vector2 dragStartPos) {
		if (hasKeyModifiers ()) {
			return;
		}

		if (button == 0) {
			Vector3 dragDirection = toPos - fromPos;
			dragDirection.z = 0;

			meshUtils.DeformMesh(mesh, dragDirection, vertexWeights);
			
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
