using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub
// Date: March 2019

public class TerrainScript : MonoBehaviour {

	public Color mountainColor = new Color(82, 82, 82);
	public Color plainsColor = new Color(9, 82, 12);
	public float scale = 1f;
	public int length = 25, width = 25;
	public int seed = 10;
    public GameObject resourcePrefab;

	void Start() {

		Random.InitState(seed);
		Vector3[] vertices = new Vector3[length * width];
		Color[] colors = new Color[vertices.Length];
		int[] triangles = new int[6 * (length-1) * (width-1)];

		// Determine the basic vertices
		for (int i = 0; i < length; i++) {
			for (int j = 0; j < width; j++) {
				vertices[getVertIndex(i, j)] = new Vector3(i, Random.Range(0f, 0.3f), j)*scale;
			}
		}

		// Set every vertex color to plains for now
		for (int i = 0; i < colors.Length; i++) {
			colors[i] = plainsColor;
		}

		// Generate mountains
		int numMountains = Random.Range(0,4);
		for (int k = 0; k < numMountains; k++) {
			int i = Random.Range(0, length);
			int j = Random.Range(0, width);
			int radius = Random.Range(3, 10);
			int height = Random.Range(1, 5);
			for (int m = -radius; m <= radius; m++) {
				for (int n = -radius; n <= radius; n++) {

					float distance = Vector2.Distance(new Vector2(m, 0), new Vector2(0, n));
					if (distance <= radius + 1 && 0 <= i+m && i+m < length && 0 <= j+n && j+n < width) {

						// Raise close points based on their distance from the center of the mountain
						if (distance < radius) {
							vertices[getVertIndex(i+m, j+n)].y +=
							Mathf.Min(height*scale, Random.Range(0.75f, 1f)* height * scale * Mathf.Exp(-(distance / radius)));
						}

						// Give nearby points the mountain color
						if (distance <= radius+1) {
							colors[getVertIndex(i+m, j+n)] = mountainColor;
						}
					}
				}
			}
		}

        int numHotSpots = Random.Range(1,100);
        for(int i = 0; i < numHotSpots; i++)
        {
            int m = Random.Range(0, length);
            int n = Random.Range(0, width);
            GameObject newResource = Instantiate(resourcePrefab);
            newResource.transform.position = vertices[getVertIndex(m, n)];


        }

		// Define the triangles
		for (int i = 0; i < length - 1; i++) {
			for (int j = 0; j < length -1; j++) {
				
				// For each square, define 2 triangles
				triangles[getFirstTriangleIndex(i, j)+3] = getVertIndex(i, j);
				triangles[getFirstTriangleIndex(i, j)+4] = getVertIndex(i, j+1);
				triangles[getFirstTriangleIndex(i, j)+5] = getVertIndex(i+1, j+1);

				triangles[getFirstTriangleIndex(i, j)] = getVertIndex(i+1, j+1);
				triangles[getFirstTriangleIndex(i, j)+1] = getVertIndex(i+1, j);
				triangles[getFirstTriangleIndex(i, j)+2] = getVertIndex(i, j);
			}
		}

		// Create the new mesh
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
		gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	// Helper function to get the vertices index from i, j
	private int getVertIndex(int i, int j) {
		return (i*width + j);
	}

	// Helper function to get the triangle index from i, j
	private int getFirstTriangleIndex(int i, int j) {
		return 6*(i*(width-1) + j);
	}
}
