using UnityEngine;
using System.Collections;

/*
	Therese Henriksson
		IGME 202
		Final Project
		
		This is the path class, which contains information such as the start pos (curr game object),
		the next WP (end point of line segment), the line segment vector, the unit vector of the line segment,
		and the magnitude of the line segment.

		There are two methods, one that returns the magnitude of the vector from the racer to the closest point,
		which is used to see of the racer is heading off path. Then there is a method that returns the closest point which to seek.

		Each WP has their own path class.
*/

public class Paths : MonoBehaviour {
	public GameObject next;
	public GameObject start;
	public Vector3 AB;
	public Vector3 unitAB;
	public float mag;
	private Vector3 distance;
	public float dotProduct;
	public Vector3 closestPoint;
	public Vector3 dist1;
	public Vector3 dist2;
	public Vector3 get;
	public GameObject sceneManager;


	// Use this for initialization
	void Start () {
		AB = next.transform.position - start.transform.position;
		unitAB = AB.normalized;
		mag = AB.magnitude;
		sceneManager = GameObject.Find ("SceneManager");
	}

	/// <summary>
	/// This method is called once per frame to check if the gameobject
	/// is off the path or not.
	/// It calculates what would be the closest point on the current segment line
	/// and then returns the magnitude of that vector
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="position">Position.</param>
	public float OffPath(Vector3 position)
	{
		// distance between the racer and the beginning WP of the path (the current game object)
		dist1 = position - gameObject.transform.position;

		// dot product to find point at the unit segment line from the vector that goes betweem the WP and racer
		dotProduct = Vector3.Dot (unitAB, dist1);

		// get the vector that goes from the beginning WP to the point we got from the dot product
		closestPoint = start.transform.position + (unitAB * dotProduct);

		// the distance between the racer and the closest point
		dist2 = position - closestPoint;
		// get the magnitude
		float m = dist2.magnitude;

		// return the magnitude
		return m;
	}

	/// <summary>
	/// Returns the closest point on the line if the racer is off path
	/// The math is explained in the method above
	/// </summary>
	/// <returns>The point.</returns>
	/// <param name="position">Position.</param>
	public Vector3 ClosestPoint(Vector3 position)
	{
		distance = position - gameObject.transform.position;
		dotProduct = Vector3.Dot (unitAB, distance);

		Vector3 closestPoint = start.transform.position + (unitAB * dotProduct);

		return closestPoint;

	}
	
	// Update is called once per frame
	void Update () {
	
		// draws the line between the points
		if (sceneManager.GetComponent<SceneManager> ().draw) {
			sceneManager.GetComponent<SceneManager> ().debugRenderer.DrawLine (gameObject.transform.position, next.transform.position, sceneManager.GetComponent<SceneManager> ().debugRenderer.Materials [0]);
		}
	}
}
