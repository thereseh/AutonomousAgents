using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
	Therese Henriksson
		IGME 202
		Final Project
		
		This is the flocker class, which is a number of ants.

		They are normally arriving at sugar cube targets, moving together with separation, cohersion and
		alignment.

		If spiders are getting close, they will evade, eventually flee, and ignore arriving at targets meanwhile.

		They are also avoiding obstacles, staying within bounds.
					
*/


public class Ants : VehicleMovement {
	
	private Vector3 ultimateForce;
	public float sepWeight;
	public float cohersionWeight;
	public float seekingWeight;
	public float alignWeight;
	public float avoidWeight;
	public float boundsWeight = 5f;
	public float fleeWeight = 13f;
	public float evadeWeight;
	 
	public float evadeDistance;
	public float evadeRadius;
	public float fleeRadius = 20f;
	public float maxForce;
	public float numAhead;
	public float safeDistance = 10f;
	private bool fleeing = false;
	
	private GameObject fleeFrom;
	private Vector3 target;
	public Vector3 center = new Vector3 (60f, 0.5f, 60f);
	private Vector3 drawLineForward;
	private Vector3 drawLineRight;
	private Vector3 evadeVector;
	private Vector3 futurePos;
	private Vector3 tempPos;
	private Vector3 tempDir;
	// Use this for initialization
	public override void Start () 
	{
		base.Start();
	}
	
	public override void CalcSteeringForces()
	{
		ultimateForce = Vector3.zero;
		float closestEnemy = Mathf.Infinity;
		target = Vector3.zero;
		float closestTarget = Mathf.Infinity;
		
		// ===============================

		// clear the list in case something has changed recently
		sceneManager.sugar.RemoveAll (GameObject => GameObject == null);
		sceneManager.ants.RemoveAll (GameObject => GameObject == null);

		// =======================================================
		// 						FLEEING
		// =======================================================

		fleeing = false;
		for (int i = 0; i < sceneManager.spiders.Count; i++)
		{
			// get distance to spider
			Vector3 direction2 = sceneManager.spiders[i].transform.position - gameObject.transform.position;
			float distEnemy = direction2.magnitude;
			fleeFrom = sceneManager.spiders[i];

			// if the distance is smaller than "safe radius"
			if (distEnemy <= fleeRadius) {
				ultimateForce += Flee (fleeFrom.transform.position) * fleeWeight;
				// if not too close, just evade it
			} else if (distEnemy <= evadeRadius){
				evadeVector = fleeFrom.transform.position + (fleeFrom.GetComponent<VehicleMovement>().velocity * evadeDistance);
				ultimateForce += Flee (evadeVector) * evadeWeight;
				fleeing = true;
			}
		}

		// =======================================================
		// 					ARRIVING TO TARGET
		// =======================================================
		
		
		// if there are no targets, skip seeking
		if (sceneManager.sugar.Count != 0 && !fleeing) {
			for (int i = 0; i < sceneManager.sugar.Count; i++) {
				// get the distance to the sugar
				Vector3 direction2 = sceneManager.sugar [i].transform.position - gameObject.transform.position;
				float distTarget = direction2.magnitude;
				
				// if this sugar is the closest one, set to target
				if (distTarget < closestTarget) {
					closestTarget = distTarget;
					target = sceneManager.sugar[i].transform.position;
				}
			}
			ultimateForce += Arrival (target) * seekingWeight;
		}


		// =======================================================
		// 						SEPARATION
		// =======================================================

		for (int i = 0; i < sceneManager.ants.Count; i++)
		{
			// gets the distance to the other ant
			Vector3 distance = gameObject.transform.position - sceneManager.ants[i].transform.position;
			float mag = distance.magnitude;
			
			// if within personal bubble, add a separation force
			// unless "self"
			if (mag <= personalBubble && sceneManager.ants[i].name != gameObject.name)
			{
				ultimateForce += Separate(sceneManager.ants[i].transform.position) * sepWeight;
			}
		}

		// =======================================================
		// 					OBSTACLE AVOIDANCE
		// =======================================================

		// avoid obstacles (trees)!
		foreach (GameObject obj in sceneManager.obstacles) {
			ultimateForce += AvoidObstacle(obj, safeDistance) * avoidWeight;
		}
		foreach (GameObject obj in sceneManager.racers) {
			ultimateForce += AvoidObstacle(obj, safeDistance) * avoidWeight;
		}
		
		// =======================================================
		// 						COHERSION
		// =======================================================

		// so they are always trying to get close to each other
		// seek the average position of the ants
		ultimateForce += Seek (sceneManager.averagePositionAnt) * cohersionWeight;




		// =======================================================
		// 						ALIGMENT
		// =======================================================

		// Align along the average direction
		ultimateForce += Alignment (sceneManager.averageDirectionAnt) * alignWeight;



		// =======================================================
		// 					STAY WITHIN BOUNDS
		// =======================================================


		// so they stay within bounds
		if (OutOfBounds ()) {
			ultimateForce += Seek(center) * boundsWeight;
		}

		// >>>>>>>>>>>>> APPLY FORCE <<<<<<<<<<<<<<<<<<<
		ultimateForce = Vector3.ClampMagnitude (ultimateForce, maxForce);
		ApplyForce (ultimateForce);


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
		
		if (currPos.x <= 23f || currPos.x >= 180f || currPos.z <= 23f || currPos.z >= 180f) {
			return true;
		} else {
			return false;
		}
	}
}
