using UnityEngine;
using System.Collections;

/*
	Therese Henriksson
		IGME 202
		Final Project
		
		This is a simple seek/arrival class, with bees seeking honey.

		The bees are seeking and arriving at honey that spawns on random flowers,
		they are the only characters that are not bound to the ground, as they are flying.

		They are avoiding obstacles that are not flowers, and they are separating from each other.
*/


public class Bee : VehicleMovement {

	public GameObject futurePosBall;
	public GameObject tempPosBall;
	private GameObject fleeFrom;

	public float seekingWeight = 2f;
	public float boundsWeight = 20f;
	public float avoidWeight = 3f;
	public float arrivalWeight;
	public float sepWeight;
	
	public float maxForce = 50f;
	
	public float numAhead = 1.5f;
	public float safeDistance = 10f;
	public float evadeDistance = 1f;
	public float futurePosAhead = 1f;
	public float mag;

	public Vector3 ultimateForce;


	private Vector3 drawLineForward;
	private Vector3 drawLineRight;
	private Vector3 evadeVector;
	private Vector3 futurePos;
	private Vector3 center = new Vector3(100f, 1f, 100f);
	private Vector3 target;

	// Use this for initialization
	public override void Start () 
	{
		base.Start();
		arrivalDistance = 10f;

		// doesn't need y position from terrain
		lockYPos = false;
	}
	
	public override void CalcSteeringForces()
	{
		ultimateForce = Vector3.zero;
		
		target = Vector3.zero;
		
		float closestTarget = Mathf.Infinity;

		// =======================================================
		// 					ARRIVING TO HONEY
		// =======================================================

		// clears out the list in case changes has been made just before this
		sceneManager.honey.RemoveAll (GameObject => GameObject == null);
		
		// if there are no targets, skip seeking
		if (sceneManager.honey.Count != 0) {
			for (int i = 0; i < sceneManager.honey.Count; i++) {
				// get distance from game object to honey
				Vector3 direction2 = sceneManager.honey [i].transform.position - gameObject.transform.position;
				float distTarget = direction2.magnitude;

				// if the distance is closer than prev honey, this is the current target
				if (distTarget < closestTarget) {
					closestTarget = distTarget;
					target = sceneManager.honey[i].transform.position;
				}
			}

			// Arrive to the closest honey
			ultimateForce += Arrival (target) * seekingWeight;
		}

		
		// =======================================================
		// 						SEPARATION
		// =======================================================

		for (int i = 0; i < sceneManager.bees.Count; i++)
		{
			// check distance to this bee
			Vector3 distance = gameObject.transform.position - sceneManager.bees[i].transform.position;
			// get the magnitude
			float mag = distance.magnitude;
			
			// if within personal bubble (and is not self), add a separation force
			if (mag <= personalBubble && sceneManager.bees[i].name != gameObject.name)
			{
				ultimateForce += Separate(sceneManager.bees[i].transform.position) * sepWeight;
			}
		}

		// =======================================================
		// 					OBSTACLE AVOIDANCE
		// =======================================================

		// calling the avoidance method for all obstacles
		// except for flowers, since we need to arrive for honey
		foreach (GameObject obj in sceneManager.obstacles) {
			if (obj.tag != "Rose" && obj.tag != "Flower")
			{
				ultimateForce += AvoidObstacle(obj, safeDistance) * avoidWeight;
			}
		}
		
		// =======================================================
		// 					OUT OF BOUNDS
		// =======================================================

		// to stay in scene, will rarely get called since they are
		// always arriving to honey
		if (OutOfBounds ()) {
			ultimateForce += Seek(center) * boundsWeight;
			
		}

		// >>>>>>>>>>>>> APPLY FORCE <<<<<<<<<<<<<<<<<<<

		ultimateForce = Vector3.ClampMagnitude (ultimateForce, maxForce);
		ApplyForce (ultimateForce);

		Destroy (tempPosBall);
		
		// draw debug stuff
		if (sceneManager.draw) {
			FuturePos ();
			
			drawLineForward = gameObject.transform.position;
			drawLineForward += (gameObject.transform.forward * 4f);
			
			drawLineRight = gameObject.transform.position;
			drawLineRight += (gameObject.transform.right * 4f);
			
			debugRenderer.DrawLine (gameObject.transform.position, drawLineRight, debugRenderer.Materials [1]);
			debugRenderer.DrawLine (gameObject.transform.position, drawLineForward, debugRenderer.Materials [2]);
		} 
		
	}

	/// <summary>
	/// Calculate the future position to place debug ball
	/// </summary>
	private void FuturePos()
	{
		futurePos = gameObject.transform.position + (velocity * futurePosAhead);
		tempPosBall = Instantiate (futurePosBall, futurePos, Quaternion.identity) as GameObject;
		tempPosBall.transform.parent = gameObject.transform;
		debugRenderer.DrawLine (gameObject.transform.position, futurePos, debugRenderer.Materials [0]);
	}


	
	// checks if they are going out of bounds
	private bool OutOfBounds()
	{
		Vector3 currPos = gameObject.transform.position;
		
		if (currPos.x <= 20f || currPos.x >= 180f || currPos.z <= 20f || currPos.z >= 180f) {
			return true;
		} else {
			return false;
		}
	}
}
