using UnityEngine;
using System.Collections;

public abstract class MouseMonoBehaviour : MonoBehaviour {

	private Vector2 lastMousePos;
	private int dragButton = -1;
	private Vector2 dragStartPos;
	
	// Update is called once per frame
	public void Update () {
		Vector2 mousePos = ScreenToLocalPosition(Input.mousePosition);

		// Mouse button events
		for (int i = 0; i < 3; i++) {
			if (Input.GetMouseButtonDown (i)) {
				OnMousePress (i, mousePos);
			}
			
			if (Input.GetMouseButtonUp (i)) {
				OnMouseRelease (i, mousePos);
			}
		}
		
		// Mouse move events
		if (lastMousePos != mousePos) {
			OnMouseMove (lastMousePos, mousePos);
		}

		lastMousePos = mousePos;
	}

	public bool hasKeyModifiers() {
		return (Input.GetKey (KeyCode.LeftControl)
			|| Input.GetKey (KeyCode.RightControl)
			|| Input.GetKey (KeyCode.LeftShift)
			|| Input.GetKey (KeyCode.RightShift)
			|| Input.GetKey (KeyCode.RightAlt)
			|| Input.GetKey (KeyCode.LeftAlt));
	}

	private Vector2 ScreenToLocalPosition(Vector2 pos) {
		return Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.localPosition;
	}

	//**** Mouse Events ****//
	void OnMousePress(int button, Vector2 pos)
	{
		dragButton = button;
		dragStartPos = pos;
		
		OnDragStart (button, pos);
	}
	
	void OnMouseRelease(int button, Vector2 pos)
	{
		dragButton = -1;
		OnDragFinish (button, pos, dragStartPos);
	}
	
	void OnMouseMove(Vector2 fromPos, Vector2 toPos)
	{
		if (dragButton > -1) {
			OnDrag(dragButton, fromPos, toPos, dragStartPos);
		}
	}

	//**** Dragging Virtual Methods ****//
	public abstract void OnDragStart (int button, Vector2 pos);
	public abstract void OnDragFinish (int button, Vector2 pos, Vector2 dragStartPos);
	public abstract void OnDrag (int button, Vector2 fromPos, Vector2 toPos, Vector2 dragStartPos);
}
