using UnityEngine;
using System.Collections;

/*
	Therese Henriksson
	IGME 202
	Final Project

	This is the class for the followers, which in this case are smaller spiders.

	They are constantly arriving at a point behind the leader (who is guiding them along the scene using wandering).
	Unless they are close enough to targets (ants) then they will pursue and arrive instead. The followers are
	slighly faster than the leader. 

	If the followers are getting in the way of the leader, they will avoid it.

 */


public class FollowSpider : VehicleMovement {
	
	public GameObject leader;
	public GameObject targetObj;

	private Vector3 ultimateForce;
	public float sepWeight;
	public float cohersionWeight;
	public float seekingWeight;
	public float alignWeight;
	public float avoidWeight;
	public float leaderAvoid;
	public float arrivalWeight;
	public float boundsWeight = 5f;
	public float pursueWeight;

	public float futurePosAhead = 1.5f;
	public bool chasing = false;

	public float pursueRadius;
	public float distTarget;
	public float maxForce;
	public float numAhead;
	public float safeDistance = 10f;
	
	public GameObject target;
	public Vector3 center = new Vector3 (60f, 0.5f, 60f);
	private Vector3 drawLineForward;
	private Vector3 drawLineRight;
	private Vector3 evadeVector;
	private Vector3 futurePos;
	private Vector3 tempPos;
	private Vector3 tempDir;
	public float magLeader;
	// Use this for initialization
	public override void Start () 
	{
		base.Start();
		positionY = 0.2f;
		arrivalDistance = 5f;
		leader = GameObject.Find ("LeaderSpider");
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

					// if close enough, chase!
					if (distTarget < pursueRadius)
					{
						target = sceneManager.ants [i];
						chasing = true;
					}
				}
			}
			// pursue and arrive
			if (target != null) {
				ultimateForce += Arrival (target.transform.position + (target.GetComponent<VehicleMovement> ().velocity * futurePosAhead)) * pursueRadius;
			}
		}

		// =======================================================
		// 			LEADER FOLLOWING #1 POS BEHIND - ARRIVAl
		// =======================================================

		if (!chasing) {
			// the distance behind leader is calculated in scene manager
			ultimateForce += Arrival (sceneManager.posBehindLeader) * arrivalWeight;
		}

		// =======================================================
		// 				LEADER FOLLOWING #2 AVOIDANCE
		// =======================================================

		// get the distance to the leader
		Vector3 distToLeader = (leader.transform.position + leader.GetComponent<VehicleMovement> ().velocity * futurePosAhead) - gameObject.transform.position;
		magLeader = distToLeader.magnitude;

		// if wihin the personal bubble of leader, avoid it
		if (magLeader <= personalBubble) {
			ultimateForce += LeaderAvoidance(distToLeader, magLeader) * leaderAvoid;
			ultimateForce += Flee(distToLeader) * leaderAvoid;

		}
	
		// =======================================================
		// 						SEPARATION
		// =======================================================

		for (int i = 0; i < sceneManager.followSpider.Count; i++)
		{
			// gets the distance another spider
			Vector3 distance = gameObject.transform.position - sceneManager.followSpider[i].transform.position;
			float mag = distance.magnitude;
			
			// if within personal bubble, add a separation force
			// unless "self"
			if (mag <= personalBubble && sceneManager.followSpider[i].name != gameObject.name)
			{
				ultimateForce += Separate(sceneManager.followSpider[i].transform.position) * sepWeight;
			}
		}

		// =======================================================
		// 				OBSTACLE AVOIDANCE
		// =======================================================
		// avoid obstacles
		foreach (GameObject obj in sceneManager.obstacles) {
			ultimateForce += AvoidObstacle(obj, safeDistance) * avoidWeight;
		}


		
		// =======================================================
		// 						COHERSION
		// =======================================================

		// so they are always trying to get close to each other
		ultimateForce += Seek (sceneManager.averagePositionSpider) * cohersionWeight;


		
		// =======================================================
		// 						ALIGNMENT
		// =======================================================
		ultimateForce += Alignment (sceneManager.averageDirectionSpider) * alignWeight;



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

	
	// checks if they are going out of bounds
	private bool OutOfBounds()
	{
		Vector3 currPos = gameObject.transform.position;
		
		if (currPos.x <= 23f || currPos.x >= 180f || currPos.z <= 23f || currPos.z >= 155f) {
			return true;
		} else {
			return false;
		}
	}
}
