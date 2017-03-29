using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(CharacterController))]

/*
	Therese Henriksson
	IGME 202
	Final Project

	This is the parent class of the moving objects,
	it contains the methods to calculate all the steering forces,
	such as seek, flee, obstacle avoidance, coherence, alignment and separation

	It moves the characters, using terrain data to calculate the correct y position for
	when the height changes on the terrain.

 */

abstract public class VehicleMovement : MonoBehaviour {
	public Vector3 position;
	public Vector3 direction;
	public Vector3 velocity;
	public Vector3 acceleration;
	public float mass = 3f;
	public float maxSpeed;
	public float maxSpeedRef;
	public float personalBubble;
	public float arrivalDistance;
	public float arriveRadius = 10f;
	public bool lockYPos = true;

	private GameObject sceneManagerObj;
	public SceneManager sceneManager;
	public DebugRenderer debugRenderer;
	public float safeRadius = 2.5f;
	public Terrain terrain;
	private TerrainData terrainData;

	public float positionY;
	public float pathRadius = 10f;

	
	// Use this for initialization
	public virtual void Start () 
	{
		sceneManagerObj = GameObject.Find ("SceneManager");
		sceneManager = sceneManagerObj.GetComponent<SceneManager> ();
		debugRenderer = sceneManagerObj.GetComponent<DebugRenderer> ();

		terrainData = sceneManager.terrain.terrainData;
	}
	
	// Update is called once per frame
	void Update () {
		CalcSteeringForces ();
		UpdatePosition ();		
		SetTransform ();

	}

	public abstract void CalcSteeringForces ();

	/// <summary>
	/// Updates the position by using retrieved acceleration from forces
	/// to calculate a velocity, from the velocity we will get a direction
	/// </summary>
	private void UpdatePosition() 
	{
		position = gameObject.transform.position;
		// Step 1: add accel to vel * time
		velocity += acceleration * Time.deltaTime;

		// all characters except for bees, who are flying
		if (lockYPos) {
			position.y = GetHeightWorldCoords(terrainData, new Vector2(gameObject.transform.position.x, gameObject.transform.position.z)) + 0.2f;;
			velocity.y = 0f;
		}

		gameObject.transform.position = position;
		gameObject.GetComponent<CharacterController> ().Move (velocity * Time.deltaTime);
		// Step 3: derive a direction
		direction = velocity.normalized;

		// Step 4: Zero out acceletation
		acceleration = Vector3.zero;

	}

	/// <summary>
	/// Trying my best to not get weird rotations by setting x and z rot to 0
	/// is also setting the forward vector to the direction vector so that the character is facing the 
	/// right way
	/// </summary>
	private void SetTransform()
	{
		// Rotate this vehicle based on its forward vector
		gameObject.transform.rotation = Quaternion.Euler (0.0f, gameObject.transform.rotation.y, 0.0f);
		gameObject.transform.forward = direction;
	}

	// method written by FlyingOstriche 
	// http://answers.unity3d.com/questions/339191/how-can-i-get-the-y-position-of-a-spot-on-my-terra.html
	static float GetHeightWorldCoords(TerrainData terrainData,Vector2 point)
	{
		Vector3 scale=terrainData.heightmapScale;
		return (float)terrainData.GetHeight((int)(point.x/scale.x),(int)(point.y/scale.z));
	}

	/// <summary>
	/// Applies the force.
	/// </summary>
	/// <param name="force">Force.</param>
	public void ApplyForce(Vector3 force) 
	{
		acceleration += force / mass;
	}

	/// <summary>
	/// Seek the specified targetPos,
	/// calculates a steering force considering the positions of the two objects
	/// </summary>
	/// <param name="targetPos">Target position.</param>
	public Vector3 Seek(Vector3 targetPos)
	{
		// Step 1: Calculate desired velocity
		// Vector pointing from myself to my target
		Vector3 desiredVelocity = targetPos - position;
		
		// Step 2: Scale desired to max speed
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;
		
		// Step 3: Calculate the steering force for seeking
		// Steering = desired - current
		Vector3 steeringForce = desiredVelocity - velocity;
		
		// Step 4: Return steering force -> apply to acceleration
		return steeringForce;
	}

	/// <summary>
	/// Calculates arrival when getting close to target
	/// reduces speed depending on how close you are
	/// </summary>
	/// <param name="targetPos">Target position.</param>
	public Vector3 Arrival(Vector3 targetPos)
	{
		Vector3 desiredVelocity = targetPos - position;

		float mag = desiredVelocity.magnitude;

		// if within radius, reduce the speed depending on how close you are
		if (mag <= arriveRadius) {
			maxSpeed = maxSpeed / mag;

			// stop at a certain point
			if (mag <= 1f)
			{
				maxSpeed = 0f;
			}
		}
	
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;
		Vector3 steeringForce = desiredVelocity - velocity;

		// sets the maxSpeed back to original
		maxSpeed = maxSpeedRef;
		return steeringForce;
	}

	/// <summary>
	/// Separate one object from another
	/// </summary>
	/// <param name="targetPos">Target position.</param>
	public Vector3 Separate(Vector3 targetPos)
	{
		Vector3 desiredVelocity = position - targetPos;
		float mag = desiredVelocity.magnitude;
		
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;
		
		Vector3 steeringForce = desiredVelocity - velocity;
		
		return (steeringForce * (1/mag));
	}

	/// <summary>
	/// Put one object closer to another
	/// </summary>
	public Vector3 Cohersion(Vector3 direction)
	{
		Vector3 desiredVelocity = position - direction;

		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;
		
		Vector3 steeringForce = desiredVelocity - velocity;
		
		return steeringForce;
	}

	/// <summary>
	/// Align the objects
	/// </summary>
	public Vector3 Alignment(Vector3 direction)
	{
		Vector3 steeringForce = direction- direction;
		
		return steeringForce;
	}
	

	/// <summary>
	/// Flee the specified targetPos.
	/// calculates a steering force considering the positions of the two objects

	/// </summary>
	/// <param name="targetPos">Target position.</param>
	public Vector3 Flee(Vector3 targetPos)
	{
		Vector3 desiredVelocity = position - targetPos;
		
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;
		
		Vector3 steeringForce = desiredVelocity - velocity;
		
		return steeringForce;
	}

	/// <summary>
	/// Avoids the leader, checks of the leader is the to left or the right
	/// </summary>
	/// <returns>The avoidance.</returns>
	/// <param name="distance">Distance.</param>
	/// <param name="mag">Mag.</param>
	public Vector3 LeaderAvoidance(Vector3 distance, float mag)
	{
		float dotProduct = Vector3.Dot (distance, gameObject.transform.right);
		Vector3 desiredVelocity = Vector3.zero;
		Vector3 steeringForce = Vector3.zero;
		// it's to the left, so go right!
		if (dotProduct < 0) {
			desiredVelocity = gameObject.transform.right;
			desiredVelocity.Normalize ();
			desiredVelocity *= maxSpeed;
			steeringForce = (desiredVelocity - velocity) * (personalBubble / mag);
		}
		// it's to the right, so go left!
		if (dotProduct > 0) {
			desiredVelocity = gameObject.transform.right * -1;
			desiredVelocity.Normalize ();
			desiredVelocity *= maxSpeed;
			steeringForce = (desiredVelocity - velocity) * (personalBubble / mag);
		}
		return steeringForce;
	}

	/// <summary>
	/// Avoids the obstacle, which are trees placed around the world
	/// </summary>
	/// <returns>The obstacle.</returns>
	/// <param name="obst">Obst.</param>
	/// <param name="safeDistance">Safe distance.</param>
	public Vector3 AvoidObstacle(GameObject obst, float safeDistance)
	{
		// get the distance from the character and the object
		Vector3 vecToC = obst.transform.position - gameObject.transform.position;
			float magVecToC = vecToC.magnitude;
			float radiusObj = 0f;
		if (obst.tag == "Log") {
			radiusObj = obst.GetComponent<BoxCollider>().size.x;
		}
		else{
			radiusObj = obst.GetComponent<SphereCollider>().radius;
		}


		// if it's too far away, return zero
			if (magVecToC - (radiusObj + safeRadius) > safeDistance) {
			return Vector3.zero;

		// if it's behind the character, return zero
		} else if (Vector3.Dot (vecToC, gameObject.transform.forward) < 0) {
			return Vector3.zero;
		// in front but not in danger zone, return zero
		} else if ((radiusObj + safeRadius) < Vector3.Dot (vecToC, gameObject.transform.right)) {
			return Vector3.zero;

		// time to avoid!
		} else {
			float dotProduct = Vector3.Dot (vecToC, gameObject.transform.right);
			Vector3 desiredVelocity = Vector3.zero;
			Vector3 steeringForce = Vector3.zero;
			// it's to the left, so go right!
			if (dotProduct < 0) {
				desiredVelocity = gameObject.transform.right;
				desiredVelocity.Normalize ();
				desiredVelocity *= maxSpeed;
				steeringForce = (desiredVelocity - velocity) * (safeDistance / magVecToC);
			}
			// it's to the right, so go left!
			if (dotProduct > 0) {
				desiredVelocity = gameObject.transform.right * -1;
				desiredVelocity.Normalize ();
				desiredVelocity *= maxSpeed;
				steeringForce = (desiredVelocity - velocity) * (safeDistance / magVecToC);
			}
			return steeringForce;
		}

	}


}
