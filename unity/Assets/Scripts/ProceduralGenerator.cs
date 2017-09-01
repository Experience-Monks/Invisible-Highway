using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * A script to handle the procedural generation of the environment
 */

public class ProceduralGenerator : MonoBehaviour
{
	// the ground prefab that uses the shadowPlane material
	public GameObject groundPrefab;

	// the center of the plane in world space
	public Vector3 planeCenter;

	// defines max/min/range of the plane in world space x and z coordinates
	public Vector2 planeMax, planeMin, range;

	// the area of the environment plane
	private float playArea;

	// the floor height as determined by depth testing
	private float floorHeight;

	// a list to temporarily store potential instantiation positions and test criteria before actual instantiation
	private List<Vector3> randomInstantiationPos;

	// defines the padding around the plotted path points that the environment will extend to
	private const float PLANE_PADDING = 1.5f;


	void Start(){
		randomInstantiationPos = new List<Vector3> ();
	}

	// determines the environment plane size based on a bounding box around the user's plotted road points
	public void FindPlaneSize (List<Vector2> pathPoints)
	{
		// guarentees the values will be overwritten with values from path points
		planeMax = new Vector2 (-Mathf.Infinity, -Mathf.Infinity);
		planeMin = new Vector2 (Mathf.Infinity, Mathf.Infinity);

		foreach (Vector2 point in pathPoints) {
			planeMax = Vector2.Max (planeMax, point);
			planeMin = Vector2.Min (planeMin, point);
		}
	}

	// generates the environment plane at floor height based on the center point of the user's plotted points
	// plane is transparent but receives shadows cast on it
	public void GeneratePlane ()
	{
		range.x = planeMax.x - planeMin.x;
		range.y = planeMax.y - planeMin.y;

		// determine floor height from the PathCreationUIController singleton
		floorHeight = PathController.Instance.FloorHeight ();

		// create the plane center at the center of the bounding box defining the path points
		planeCenter = new Vector3 (range.x * 0.5f + planeMin.x, floorHeight, range.y * 0.5f + planeMin.y);
		GameObject groundPlane = Instantiate (groundPrefab, planeCenter, Quaternion.identity);

		// default plane is 10m x 10m so we divide by 10 and add the additional padding to the plane and playArea
		groundPlane.transform.localScale = new Vector3 (range.x / 10.0f * PLANE_PADDING, 0.1f, range.y / 10.0f * PLANE_PADDING);
		playArea = range.x * range.y * Mathf.Pow (PLANE_PADDING, 2.0f);
	}

	// randomly instantiates a given prefab for all position points that satisfy the given criteria passed as parameters
	public void RandomInstantiation (Transform parent, string name, int density, GameObject instantiationPrefab, float minDist, float maxDist, float perlinScale, float perlinThreshold)
	{
		// create an empty parent to hold all instantiated children
		GameObject bucket = new GameObject (name);
		bucket.transform.parent = parent;

		// generate potential points for instantiation of passed prefab
		InstantiationPositionGenerate (density, instantiationPrefab);

		// loop through potential instantiation positions and test whether it satisfies the distance criteria to the road
		for (int i = 0; i < randomInstantiationPos.Count - 1; i++) {
			if (DistanceCheck (randomInstantiationPos [i], minDist, maxDist)) {

				// if user has defined a perlinScale and perlinThreshold, use these values to clump the assets into groups
				// higher perlinScale means tighter clumps, lower perlinThreshold means larger individual clumps

				if (perlinThreshold == 0.0f || (Mathf.PerlinNoise (randomInstantiationPos [i].x * perlinScale, randomInstantiationPos [i].z * perlinScale) > perlinThreshold)) {
					GameObject tempGO = Instantiate (instantiationPrefab, randomInstantiationPos [i], Quaternion.Euler (0.0f, Random.Range (0.0f, 360.0f), 0.0f));
					tempGO.transform.parent = bucket.transform;
					// randomly offset the scale of each asset relative to its original size
					tempGO.transform.localScale.Scale (new Vector3 (Random.Range (0.9f, 1.1f), Random.Range (0.8f, 1.2f), Random.Range (0.9f, 1.1f)));
				}
			}
		}
		// clear the list of instantiation positions for the next prefabs
		randomInstantiationPos.Clear ();
	}

	// overloaded method for randomly sampling from a List of prefabs instead of a single prefab environment asset
	public void RandomInstantiation (Transform parent, string name, int density, List<GameObject> instantiationPrefabs, float minDist, float maxDist, float perlinScale, float perlinThreshold)
	{
		// create an empty parent to hold all instantiated children
		GameObject bucket = new GameObject (name);
		bucket.transform.parent = parent;

		GameObject instantiationPref = instantiationPrefabs [0];
		// generate potential points for instantiation of passed prefab
		InstantiationPositionGenerate (density, instantiationPref);

		// loop through potential instantiation positions and test whether it satisfies the distance criteria to the road
		for (int i = 0; i < randomInstantiationPos.Count; i++) {
			if (DistanceCheck (randomInstantiationPos [i], minDist, maxDist)) {

				// if user has defined a perlinScale and perlinThreshold, use these values to clump the assets into groups
				// higher perlinScale means tighter clumps, lower perlinThreshold means larger individual clumps

				if (perlinThreshold == 0.0f || (Mathf.PerlinNoise (randomInstantiationPos [i].x * perlinScale, randomInstantiationPos [i].z * perlinScale) > perlinThreshold)) {
					// sample the prefab at random from the List of passed prefabs
					GameObject tempGO = Instantiate (instantiationPrefabs [Random.Range (0, instantiationPrefabs.Count)], randomInstantiationPos [i], Quaternion.Euler (0.0f, Random.Range (0.0f, 360.0f), 0.0f));
					tempGO.transform.parent = bucket.transform;
					// randomly offset the scale of each asset relative to its original size
					tempGO.transform.localScale.Scale (new Vector3 (Random.Range (0.8f, 1.2f), Random.Range (0.7f, 1.3f), Random.Range (0.8f, 1.2f)));
				}
			}
		}
		// clear the list of instantiation positions for the next prefabs
		randomInstantiationPos.Clear ();
	}
		
	// creates a list of position points that are within the environment plane based on the passed prefab density
	private void InstantiationPositionGenerate (int density, GameObject instantiationPrefab)
	{
		// the number of objects is a function of the defined density of the given prefab and the play area
		// scales the number of objects required for different sizes of play area to keep consistent look of environment
		int numObjects = (int)(density * playArea); 

		// populates points based on envioronment plane
		for (int i = 0; i < numObjects; i++) {

			float posX = Random.Range (planeMin.x - 0.5f * range.x, planeMax.x + 0.5f * range.x);
			float posY = floorHeight;
			float posZ = Random.Range (planeMin.y - 0.5f * range.y, planeMax.y + 0.5f * range.y);

			// for clouds, we want the position to be in the air instead of on the ground
			if (instantiationPrefab.tag == "FloatingSpawn") {
				posY += Random.Range (0.75f, 1.0f);
			}
			randomInstantiationPos.Add (new Vector3 (posX, posY, posZ));
		}
	}

	// checks the shortest distance from prefab point to finite line segments defined by path points and returns true if distance check passed
	private bool DistanceCheck (Vector3 instantiatePos, float minDist, float maxDist)
	{
		// initally set to fail distance check test
		bool inRange = false;

		List<Vector2> points = PathController.Instance.GetPathPoints();

		// check proximity criteria for all individual road segments
		for (int i = 0; i < points.Count - 1; i++) {
			
			Vector2 startPoint = points [i];
			Vector2 endPoint = points [i + 1];
			Vector2 prefabPoint = new Vector2 (instantiatePos.x, instantiatePos.z);

			float distance;
			float lengthSquared = Mathf.Pow (Vector2.Distance (startPoint, endPoint), 2.0f);

			// in case there are overlapping points
			if (lengthSquared == 0.0f) {
				distance = Vector2.Distance (prefabPoint, startPoint);
			}

			// limited to [0,1] to only consider line segment and not line
			float parametric = Mathf.Max (0, Mathf.Min (1, Vector2.Dot (prefabPoint - startPoint, endPoint - startPoint) / lengthSquared));

			// projection of pointPrefab back onto the line defined by path points
			Vector2 projection = startPoint + parametric * (endPoint - startPoint);

			distance = Vector2.Distance (prefabPoint, projection);

			// the distance cannot be lower than minDist to ANY point but only needs to be less than maxDist to ONE SINGLE point
			if (distance < minDist) {
				return false;
			}
			if (distance < maxDist) {
				inRange = true;
			}
		}
		return inRange;
	}
}
