using System.Collections.Generic;
using UnityEngine;

/*********************************************************************/

// Created by Jam3 - http://www.jam3.com/

/*********************************************************************/

/*
 * The EnvironmentController takes care of creating the road and environment from the drawn path
 * as well as handling some interactions with elements like the Moose
 */

public class EnvironmentController : MonoBehaviour
{
	// prefabs
	public GameObject roadStartPrefab, roadMiddlePrefab, roadJointPrefab, roadEndPrefab;
	public List<GameObject> rockPrefab;
	public List<GameObject> roundTreePrefab;
	public GameObject pineTreePrefab;
	public GameObject pineFallPrefab;
	public GameObject mtnAPrefab;
	public GameObject mtnBPrefab;
	public GameObject mtnCPrefab;
	public GameObject mtnDPrefab;
	public List<GameObject> cloudPrefab;
	public GameObject balloonPrefab;
	public GameObject groundhogPrefab;
	public GameObject moosePrefab;
	public ProceduralGenerator proceduralGenerator;

	private bool mooseCreated;
	private bool groundhogCreated;

	// the length of the model of the road piece
	const float ROAD_PIECE_LENGTH = 0.25f;
	// the delay of the animation from one piece of road to the next, creating a ripple effect
	const float ROAD_ANIM_RIPPLE_DELAY = 0.02f;

	void Start()
	{
		// intializations
		mooseCreated = false;
		groundhogCreated = false;
	}

	// create and populate the environment
	public void GenerateEnvironment()
	{
		List<Vector2> pathPoints = PathController.Instance.GetPathPoints ();
		List<Vector2> notFirstPoint = pathPoints.GetRange (1, pathPoints.Count - 1);

		// create the road
		PopulatePathWithRoadSegments (notFirstPoint);

		// populate the rest of the environment
		PopulateEnvironment (notFirstPoint);
	}

	// create the road based on the points of the path the user drew
	// the road is split into segments, a segment is the road between 2 points (2 turns)
	// each segment is made up of pieces of road based 3 assets (start/end, straight, turn)
	private void PopulatePathWithRoadSegments(List<Vector2> pathPoints)
	{
		// create a parent for all the road pieces
		var roadContainer = new GameObject ("Road");
		int segmentCount = 0;

		// Get each road segment's starting point
		for (int pathPointIndex = 0; pathPointIndex < pathPoints.Count - 1; pathPointIndex++) {
			// create the segment and the pieces that it is made of
			GameObject segmentContainer = new GameObject ("Road Segment " + pathPointIndex);
			segmentContainer.transform.parent = roadContainer.transform;
			Vector2 segmentStartPos = pathPoints [pathPointIndex];
			Vector2 segmentEndPos = pathPoints [pathPointIndex + 1];
			float segmentDistance = Vector2.Distance (segmentStartPos, segmentEndPos) + ROAD_PIECE_LENGTH;
			int numRoadSegments = Mathf.CeilToInt (segmentDistance / ROAD_PIECE_LENGTH);
			float segmentLength = segmentDistance / numRoadSegments / ROAD_PIECE_LENGTH;

			for (int segmentIndex = 0; segmentIndex < numRoadSegments; segmentIndex++) {
				// if we are on the last piece of the segment and not in the last segment, skip
				// as the last piece overlaps the first piece of the next segment
				if (segmentIndex == numRoadSegments - 1 && pathPointIndex != pathPoints.Count - 2) {
					continue;
				}
				segmentCount++;

				// Get this segment's position on the floor by interpolating between the start and end positions
				Vector2 posOnFloor = Vector2.Lerp (segmentStartPos, segmentEndPos, ((float)segmentIndex / (numRoadSegments - 1)));

				// Convert that position to a 3D position by adding the floor height
				float floorHeight = PathController.Instance.FloorHeight ();
				Vector3 posInWorld = new Vector3 (posOnFloor.x, floorHeight, posOnFloor.y);

				// select the right assets depending on where on the segment we are
				GameObject segmentPrefab = SelectRoadPiece (pathPointIndex, pathPoints.Count, segmentIndex, numRoadSegments);

				Vector2 direction = segmentEndPos - segmentStartPos;
				Vector3 orientation = new Vector3 (direction.x, 0f, direction.y);

				// Create the segment
				GameObject segment = Instantiate (segmentPrefab, posInWorld, Quaternion.LookRotation (orientation, Vector3.up));
				segment.transform.parent = segmentContainer.transform;

				// scale the "straight" or "end" pieces to fit
				// don't scale the "turn" pieces as they become distorted
				if (segmentPrefab != roadJointPrefab)
					segment.transform.localScale = new Vector3 (segmentLength, 1f, 1f);

				// add a delay to the animation to create an animation effect
				AnimationRoad roadAnim = segment.GetComponent<AnimationRoad> ();
				if (roadAnim) roadAnim.delay = segmentCount * ROAD_ANIM_RIPPLE_DELAY;

				// road joints need to have their animation set to the correct frame to connect to the previous and next path points
				if (segmentPrefab == roadJointPrefab) {
					segment.transform.GetChild (0).localPosition = Vector3.zero;

					float jointAngle = calculateAngleCCW (segmentStartPos - pathPoints [pathPointIndex - 1], direction);
					float angleOffset = 62.5f;

					Animator anim = segment.GetComponent<Animator> ();
					float frameFreeze = (jointAngle - angleOffset) / (360.0f - 2.0f * angleOffset);

					// freeze on this frame so the road stops turning
					anim.speed = 0.0f;
					anim.Play ("roadSwing", 0, frameFreeze);
					segment.transform.Rotate (new Vector3 (0.0f, 180.0f + 90 - jointAngle / 2, 0.0f));

					// jointAngle > 180 = right turn, jointAngle < 180 = left turn
					float asymmetryOffset = jointAngle > 180f ? 0f : 0.01f;
					Vector3 translateDirection = jointAngle > 180f ? Vector3.left : Vector3.right;
					segment.transform.Translate (translateDirection * asymmetryOffset, Space.Self);
				}

				// the "end piece" needs to be rotated 180 to fit the road
				if (segmentIndex == numRoadSegments - 1 && pathPointIndex == pathPoints.Count - 2) {
					segment.transform.Rotate (Vector3.up * -270.0f);
				} else {
					segment.transform.Rotate (Vector3.up * -90.0f);
				}
			}
		}

        // now that the road has been instantiated, update its materials to do the Chroma Keying
        ARChromakeyHelper.Instance.UpdateMaterials ();
	}

	// calculate counter clockwise angles
	private float calculateAngleCCW(Vector2 vecFrom, Vector2 vecTo)
	{
		Vector3 vecIn = new Vector3 (vecFrom.x, 0.0f, vecFrom.y);
		Vector3 vecOut = new Vector3 (vecTo.x, 0.0f, vecTo.y);
		float angle = Vector3.Angle (vecIn * -1.0f, vecOut);
		Vector3 crossProduct = Vector3.Cross (vecIn, vecOut);
		if (crossProduct.y > 0) {
			angle = 360 - angle;
		}
		return angle;
	}

	// select and return the right road piece prefab
	private GameObject SelectRoadPiece(int pathPointIndex, int pathPointsCount, int segmentIndex, int segmentCount)
	{
		// The very first segment in the path must be a "start" piece
		if (pathPointIndex == 0 && segmentIndex == 0) {
			return roadStartPrefab;
		}

		// Every segment must start with a "joint" piece
		// TODO: unless it's the only piece...
		if (segmentIndex == 0) {
			return roadJointPrefab;
		}

		// the very last segment in the path must be an "end" piece
		// we subtract 2 here because pathPointIndex represents the point at the beginning of a road
		if (segmentIndex == segmentCount - 1 && pathPointIndex == pathPointsCount - 2) {
			return roadEndPrefab;
		}

		// All other segments must be "middle" pieces
		return roadMiddlePrefab;
	}

	// populate the environment
	private void PopulateEnvironment(List<Vector2> pathPoints)
	{
		proceduralGenerator.FindPlaneSize (pathPoints);

		proceduralGenerator.GeneratePlane ();

		// instantiate the environment objects
		// params: numObjects	instPrefab	minD	maxD	perlinScale		perlinClump
		Transform container = new GameObject ("Environment Container").transform;
		proceduralGenerator.RandomInstantiation (container, "Rocks", 100, rockPrefab, 0.15f, 0.4f, 0.0f, 0.0f);
		proceduralGenerator.RandomInstantiation (container, "Round Trees", 20, roundTreePrefab, 0.3f, 0.5f, 0.0f, 0.0f);
		proceduralGenerator.RandomInstantiation (container, "Pine Fall Trees", 50, pineFallPrefab, 0.3f, 0.6f, 0.0f, 0.0f);
		proceduralGenerator.RandomInstantiation (container, "Pine Trees", 350, pineTreePrefab, 0.45f, 0.9f, 3.0f, 0.55f);
		proceduralGenerator.RandomInstantiation (container, "Mountain A", 5, mtnAPrefab, 0.9f, 1.0f, 0.0f, 0.0f);
		proceduralGenerator.RandomInstantiation (container, "Mountain B", 5, mtnBPrefab, 0.9f, 1.0f, 0.0f, 0.0f);
		proceduralGenerator.RandomInstantiation (container, "Mountain C", 5, mtnCPrefab, 1.1f, 1.3f, 0.0f, 0.0f);
		proceduralGenerator.RandomInstantiation (container, "Mountain D", 2, mtnDPrefab, 1.2f, 1.4f, 0.0f, 0.0f);
		proceduralGenerator.RandomInstantiation (container, "Clouds", 20, cloudPrefab, 0.1f, 1.5f, 0.8f, 0.7f);

		GameObject balloon = Instantiate (balloonPrefab, new Vector3 (0.0f, pathPoints [0].y + 1.0f, 0.0f), Quaternion.identity);
		balloon.GetComponent<MoveBalloon> ().SetOffsetAndRange (proceduralGenerator.planeCenter, proceduralGenerator.range);

		PlaceMooseInteraction (pathPoints);
		PlaceGroundhogInteraction (pathPoints);
	}

	// moose interaction
	private void PlaceMooseInteraction(List<Vector2> pathPoints)
	{
		const float minimumRoadDistance = 1.25f;
		const float distOffsetAlongRoad = 1.2f;
		const float distOffsetPerpRoad = 0.6f;

		float floorHeight = PathController.Instance.FloorHeight ();

		if (mooseCreated) {
			return;
		}

		for (int i = 1; i < pathPoints.Count - 1; i++) {
			float distance = Vector2.Distance (pathPoints [i + 1], pathPoints [i]);
			if (distance > minimumRoadDistance) {
				// place the moose at this segment
				mooseCreated = true;

				Vector3 startPoint = new Vector3 (pathPoints [i].x, floorHeight, pathPoints [i].y);
				Vector3 endPoint = new Vector3 (pathPoints [i + 1].x, floorHeight, pathPoints [i + 1].y);
				GameObject moose = Instantiate (moosePrefab, startPoint, Quaternion.identity);
				Vector3 orientation = (endPoint - startPoint).normalized;
				moose.transform.Translate (orientation * distOffsetAlongRoad, Space.World);

				Vector3 crashLocation = moose.transform.position;
				Vector3 perpendicular = Vector3.Cross (orientation, Vector3.up).normalized;
				moose.transform.Translate (perpendicular * -1.0f * distOffsetPerpRoad, Space.World);
				moose.transform.rotation = Quaternion.LookRotation (crashLocation - moose.transform.position, Vector3.up);

				return;
			}
		}
	}

	// groundhog interaction
	private void PlaceGroundhogInteraction(List<Vector2> pathPoints)
	{
		const float minimumRoadDistance = 1.1f;
		const float distOffsetAlongRoad = 1.0f;
		const float distOffsetPerpRoad = 0.7f;

		float floorHeight = PathController.Instance.FloorHeight ();

		if (groundhogCreated) {
			return;
		}

		for (int i = 3; i < pathPoints.Count - 1; i++) {
			float distance = Vector2.Distance (pathPoints [i + 1], pathPoints [i]);
			if (distance > minimumRoadDistance) {
				// place the groundhog
				groundhogCreated = true;

				Vector3 startPoint = new Vector3 (pathPoints [i].x, floorHeight, pathPoints [i].y);
				Vector3 endPoint = new Vector3 (pathPoints [i + 1].x, floorHeight, pathPoints [i + 1].y);
				GameObject groundhog = Instantiate (groundhogPrefab, startPoint, Quaternion.identity);

				Vector3 orientation = (endPoint - startPoint).normalized;
				groundhog.transform.Translate (orientation * distOffsetAlongRoad, Space.World);
				Vector3 perpendicular = Vector3.Cross (orientation, Vector3.up).normalized;
				groundhog.transform.Translate (perpendicular * distOffsetPerpRoad, Space.World);

				return;
			}
		}
	}
}