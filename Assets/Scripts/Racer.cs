using UnityEngine;
using System.Collections;


/*
	Therese Henriksson
		IGME 202
		Final Project
		
		This is the path follower class, my racer ladybird.

		This class starts off with a reference to the WP0 object,
		which contains a class called "path", it's through this we get information
		regarding being on the path or not. As the racers move along, the reference point changes.

		The racers are separating from each other, as well as avoiding obstacles along the road (such as regular obstacles,
		ants and spiders).
					
*/

public class Racer : VehicleMovement {
	
	private GameObject fleeFrom;
	public GameObject target;
	private int i = 0;
	
	public float seekingWeight = 2f;
	public float boundsWeight = 20f;
	public float avoidWeight = 3f;
	public float pathWeight = 10f;
	public float sepWeight;
	
	public float maxForce = 50f;
	
	public float numAhead = 1.5f;
	public float safeDistance = 10f;
	public float evadeDistance = 1f;
	public float futurePosAhead = 2f;
	public float mag;
	
	public Vector3 ultimateForce;
	public float offPath;
	
	
	private Vector3 drawLineForward;
	private Vector3 drawLineRight;
	private Vector3 evadeVector;
	private Vector3 futurePos;
	private Vector3 center = new Vector3(60f, 1f, 60f);

	// Use this for initialization
	public override void Start () 
	{
		base.Start();
		arrivalDistance = 10f;
		target = sceneManager.wayPoints [i];

	}
	
	public override void CalcSteeringForces()
	{
		ultimateForce = Vector3.zero;

		float closestTarget = Mathf.Infinity;

		// ===========================
		sceneManager.ants.RemoveAll (GameObject => GameObject == null);

		
		futurePos = gameObject.transform.position + (velocity.normalized * futurePosAhead);

		// =======================================================
		// 						PATH SEEKING!
		// =======================================================


		// Get the distance to future position
		Vector3 stayOnSegment = futurePos - target.transform.position;
		// the magnitude of this distance
		mag = stayOnSegment.magnitude;

		// Call method found in the path class
		// called from the current target, which is the first WP of each line segment
		// get the magnitude from future position and the distance to closest point
		offPath = target.GetComponent<Paths> ().OffPath (futurePos);

		// if this magnitude is larger than path radius, then you are heading off the paht
		// and need to seek the closest point
		// else, continue to seek the second WP of the line segment (to keep moving)
		if (offPath > pathRadius) {
			ultimateForce += Seek (target.GetComponent<Paths> ().ClosestPoint (futurePos)) * pathWeight;
		} else {
			ultimateForce += Seek (target.GetComponent<Paths> ().next.transform.position) * seekingWeight;
		}

		// if the magnitude of the future position is greater than the magnitude of the
		// line segment, then it's time to change the target to the second WP, which becomes
		// the first target of the next line segment
		if (mag > target.GetComponent<Paths> ().mag) {
			target = target.GetComponent<Paths>().next;
		}
	

		// =======================================================
		// 					OBSTACLE AVOIDANCE
		// =======================================================

			
		// calling the avoidance method for all obstacles
		foreach (GameObject obj in sceneManager.obstacles) {
			ultimateForce += AvoidObstacle(obj, safeDistance) * avoidWeight;
		}

		// ants
		if (sceneManager.ants.Count != 0) {
			foreach (GameObject obj in sceneManager.ants) {
				ultimateForce += AvoidObstacle (obj, safeDistance) * avoidWeight;
			}
		}

		// spiders
		if (sceneManager.ants.Count != 0) {
			foreach (GameObject obj in sceneManager.spiders) {
				ultimateForce += AvoidObstacle (obj, safeDistance) * avoidWeight;
			}
		}
		
		// =======================================================
		// 						SEPARATION
		// =======================================================

		// to make sure the cars don't keep running over each other
		for (int i = 0; i < sceneManager.racers.Count; i++)
		{
			// gets the distance to the other racer
			Vector3 distance = gameObject.transform.position - sceneManager.racers[i].transform.position;
			float mag2 = distance.magnitude;
			
			// if within personal bubble, add a separation force
			// unless it's "self"
			if (mag2 <= personalBubble && sceneManager.racers[i].name != gameObject.name)
			{
				ultimateForce += Separate(sceneManager.racers[i].transform.position) * sepWeight;
			}
		}

		// >>>>>>>>>>>>> APPLY FORCE <<<<<<<<<<<<<<<<<<<

		ultimateForce = Vector3.ClampMagnitude (ultimateForce, maxForce);
		ApplyForce (ultimateForce);
		

		// draw debug stuff
		if (sceneManager.draw) {

			drawLineForward = gameObject.transform.position;
			drawLineForward += (gameObject.transform.forward * 4f);
			
			drawLineRight = gameObject.transform.position;
			drawLineRight += (gameObject.transform.right * 4f);
			
			debugRenderer.DrawLine (gameObject.transform.position, drawLineRight, debugRenderer.Materials [1]);
			debugRenderer.DrawLine (gameObject.transform.position, drawLineForward, debugRenderer.Materials [2]);
		} 
		
	}
	

	// checks if they are going out of bounds
	private bool OutOfBounds()
	{
		Vector3 currPos = gameObject.transform.position;
		
		if (currPos.x <= 10f || currPos.x >= 180f || currPos.z <= 10f || currPos.z >= 180f) {
			return true;
		} else {
			return false;
		}
	}
}
