using UnityEngine;
using System.Collections;

public class MeshUtils {

	public Vector2 selectionArea = new Vector2 (0.2f, 0.2f);
	public float selectionFalloff = 1.5f;

	// Initializes a mesh with a plane with nVerticesX * nVerticesY vertices. 
	public void InitPlane(Mesh mesh, int nVerticesX, int nVerticesY, Vector3[] vertices=null)
	{
		mesh.Clear ();

		int nVertices = nVerticesX * nVerticesY;
		int nTriangles = (nVerticesX - 1) * (nVerticesY - 1) * 6;

		bool initVertices = (vertices == null);

		if (initVertices) {
			vertices = new Vector3[nVertices];
		}
		
		Vector2[] uvs = new Vector2[nVertices];
		int[] triangles = new int[nTriangles];
		
		int i = 0;
		int iTriangles = 0;
		float incX = 1.0f / (nVerticesX - 1);
		float incY = 1.0f / (nVerticesY - 1);
		float xCoord = -0.5f;
		float yCoord = -0.5f;
		
		for(int y = 0; y < nVerticesY; y++) {
			
			for (int x = 0; x < nVerticesX; x++) {

				if (initVertices) {
					vertices[i] = new Vector3(xCoord, yCoord, 0);
				}
				uvs[i] = new Vector2(xCoord + 0.5f, yCoord + 0.5f);
				
				if (x < nVerticesX - 1 && y < nVerticesY - 1)
				{
					int v0 = i;
					int v1 = i + 1;
					int v2 = i + nVerticesX;
					int v3 = i + nVerticesX + 1;
					triangles[iTriangles++] = v0;
					triangles[iTriangles++] = v2;
					triangles[iTriangles++] = v3;
					triangles[iTriangles++] = v0;
					triangles[iTriangles++] = v3;
					triangles[iTriangles++] = v1;
				}
				
				xCoord += incX;
				i++;
			}
			
			yCoord += incY;
			xCoord = -0.5f;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
	}

	// Deforms the given mesh when a drag happens in a certain direction using
	// the given vertex weights.
	public void DeformMesh( Mesh mesh, Vector3 dragDirection, float[] vertexWeights) {
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] += dragDirection * vertexWeights[i];
		}
		
		mesh.vertices = vertices;
	}

	// Calculates the weights for each vertex of the given mesh using
	// their distance to a given point and the falloff
	public float[] GetVertexWeights(Mesh mesh, Vector2 point) {
		Vector3[] vertices = mesh.vertices;
		float[] vertexWeights = new float[vertices.Length];

		Vector2 relativePos;
		for (int i = 0; i < vertices.Length; i++) {
			// Mouse position to Vertex position in 2D
			// and Apply the selection area deformation
			relativePos.x = (vertices[i].x - point.x) / selectionArea.x;
			relativePos.y = (vertices[i].y - point.y) / selectionArea.y;
			
			// Weight values from 1 (on pos) to 0 (outside the area)
			vertexWeights[i] = Mathf.Pow(Mathf.Max(1.0f - relativePos.magnitude, 0), selectionFalloff);
		}

		return vertexWeights;
	}

	public void SetSelectionArea(Vector2 selectionArea) {
		this.selectionArea = selectionArea;
	}

	//**** IO ****//
	
	public string GetVerticesString(Mesh mesh) {
		Vector3[] vertices = mesh.vertices;
		string text = "";

		Vector3 vertex;
		for (int i = 0; i < vertices.Length; i++) {
			vertex = vertices[i];

			if (i > 0) {
				text += " ";
			}

			text += vertex.x + " " + vertex.y + " " + vertex.z;
		}
		
		return text;
	}
	
	public void SetVerticesFromString(Mesh mesh, string text) {
		string[] words = text.Split(" "[0]);
		Vector3[] vertices = new Vector3[words.Length];

		for (int i = 0; i < vertices.Length; i++) {
			vertices [i].x = float.Parse (words [i++]);
			vertices [i].y = float.Parse (words [i++]);
			vertices [i].z = float.Parse (words [i++]);
		}

		mesh.vertices = vertices;
	}
}
