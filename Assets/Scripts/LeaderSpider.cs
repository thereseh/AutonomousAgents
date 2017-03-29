using UnityEngine;
using System.Collections;

/*
	Therese Henriksson
	IGME 202
	Final Project

	This is the class for the leader, which in this environment is a spider.
	The leader is moving along using wandering, unless close enough to targets (ants),
	then it will pursue and arrive instead.

	The leader will also avoid obstacles, stay within bounds.

 */

public class LeaderSpider : VehicleMovement {
	private Vector3 ultimateForce;
	public float seekingWeight;
	public float wanderWeight;
	public float avoidWeight;
	public float boundsWeight;
	public float futurePosAhead;
	public float pursueWeight;

	public float pursueRadius;
	public bool chasing = false;
	public float maxForce;
	public float numAhead;
	public float radiusWander;
	public float safeDistance;

	public float distTarget;
	public GameObject target;
	public Vector3 center = new Vector3 (100f, 1f, 100f);
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
		
		float closestTarget = Mathf.Infinity;

		// ===============================
		sceneManager.ants.RemoveAll (GameObject => GameObject == null);
		
		// =======================================================
		// 					ARRIVING TO TARGET
		// =======================================================
		
		chasing = false;
		// if there are no targets, skip seeking
		if (sceneManager.ants.Count != 0) {
			for (int i = 0; i < sceneManager.ants.Count; i++) {
				Vector3 direction2 = sceneManager.ants [i].transform.position - sceneManager.averagePositionSpider;
				float distTarget = direction2.magnitude;
				if (distTarget < closestTarget) {
					closestTarget = distTarget;

					// if close enough, chase this target
					if (distTarget < pursueRadius)
					{
						target = sceneManager.ants [i];
						chasing = true;
					}
				}
			}
			// pursue future position and arrive there
			if (target != null) {
				ultimateForce += Arrival (target.transform.position + (target.GetComponent<VehicleMovement> ().velocity * futurePosAhead)) * pursueRadius;
			}
		}

		// =======================================================
		// 						WANDERING
		// =======================================================

		// if not chasing, wander instead
		if (!chasing) {
			ultimateForce += Seek (CalcWander ());
		}
		
		// =======================================================
		// 					OBSTACLE AVOIDANCE
		// =======================================================
		// avoid obstacles (trees)!
		foreach (GameObject obj in sceneManager.obstacles) {
			ultimateForce += AvoidObstacle(obj, safeDistance) * avoidWeight;
		}

		// =======================================================
		// 					OUT OF BOUNDS
		// =======================================================
		// so they stay within bounds
		if (OutOfBounds ()) {
			ultimateForce += Seek(center) * boundsWeight;
			
		}
		
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
	

	/// <summary>
	/// Calculates a new position to wander too (Seeking the vector that it returns)
	/// </summary>
	/// <returns>The wander.</returns>
	private Vector3 CalcWander()
	{
		Vector3 distAhead = gameObject.transform.position + (velocity * numAhead);
		
		float angle = Random.Range (0, 359);
		angle = ((angle * Mathf.PI) / 180);
		float x = (Mathf.Cos (angle) * radiusWander);
		float z = (Mathf.Sin (angle) * radiusWander);
		
		
		Vector3 v1 = new Vector3 (x, 0f, z);
		return (v1 + distAhead);
	}
	
	
	// checks if they are going out of bounds
	private bool OutOfBounds()
	{
		Vector3 currPos = gameObject.transform.position;
		
		if (currPos.x <= 26f || currPos.x >= 180f || currPos.z <= 23f || currPos.z >= 155f) {
			return true;
		} else {
			return false;
		}
	}
}
