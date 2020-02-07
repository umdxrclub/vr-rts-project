using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Author: Paul Armstrong, XRClub

public class TerrainScript : MonoBehaviour {

	public Color mountainColor = new Color(82, 82, 82);
	public Color plainsColor = new Color(9, 82, 12);
	public Color resourceColor = new Color(219, 219, 36);
	public float scale = 1f;
	public int minMountains = 1;
	public int maxMountains = 3;
	public int minResources = 1;
	public int maxResources = 3;
	public int length = 25, width = 25;
	public int seed = 10;
    public GameObject resourcePrefab, mountainPrefab;
	public UnityEvent onComplete;

	[HideInInspector]
	public GameObject[] mountains;
	[HideInInspector]
	public GameObject[] resources;

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
		int numMountains = Random.Range(minMountains, maxMountains + 1);
		mountains = new GameObject[numMountains];
		for (int k = 0; k < numMountains; k++) {
			float angle = Random.value * 2f * Mathf.PI;
			float magnitude = (0.5f + Random.value/2f) * (Mathf.Min(length, width) / 2f);
			int i = Mathf.Clamp((int)Mathf.Round(magnitude * Mathf.Cos(angle) + length/2), 0, length - 1);
			int j = Mathf.Clamp((int)Mathf.Round(magnitude * Mathf.Sin(angle) + width/2), 0, width - 1);
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

			// Create a mountain object in the heirarchy
            GameObject newMountain = Instantiate(mountainPrefab);
			newMountain.transform.SetParent(transform);
            newMountain.transform.position = vertices[getVertIndex(i, j)];
			mountains[k] = newMountain;
		}

		// Generate resource locations
        int numResources = Random.Range(minResources, maxResources + 1);
		resources = new GameObject[numResources];
        for(int i = 0; i < numResources; i++) {

			float angle = Random.value * 2f * Mathf.PI;
			float magnitude = (0.2f + Random.value/2f) * (Mathf.Min(length, width) / 2f);
			int m = Mathf.Clamp((int)Mathf.Round(magnitude * Mathf.Cos(angle) + length/2), 0, length - 1);
			int n = Mathf.Clamp((int)Mathf.Round(magnitude * Mathf.Sin(angle) + width/2), 0, width - 1);

			// Create the actual resource object
            GameObject newResource = Instantiate(resourcePrefab);
			newResource.transform.SetParent(transform);
            newResource.transform.position = vertices[getVertIndex(m, n)];
			resources[i] = newResource;

			// Change the color of the nearby ground
			int radius = 2;
			for (int u = -radius; u <= radius; u++) {
				for (int v = -radius; v <= radius; v++) {
					if (isInBounds(m+u, n+v) && Vector2.Distance(new Vector2(u, 0), new Vector2(0, v)) < radius) {
						colors[getVertIndex(m+u, n+v)] = resourceColor;
					}
				}
			}
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

		// Move everything so that the origin is in the middle
		gameObject.transform.Translate(-scale * length / 2f, 0f, -scale * width / 2f);

		// Call the onComplete function
		onComplete.Invoke();

	}

	// Get the height of the terrain at an x and z in world space
	public float heightMap(float worldX, float worldZ) {

		float i = (worldX / scale) + length / 2f;
		float j = (worldZ / scale) + width / 2f;

		Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;

		return mesh.vertices[getVertIndex((int)Mathf.Round(i), (int)Mathf.Round(j))].y;	
	}

	// Helper function to get if coordinates are in bounds
	private bool isInBounds(int i, int j) {
		return (0 <= i && i < length && 0 <= j && j < width);
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
