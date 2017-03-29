using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
	Therese Henriksson
	IGME 202
	Final Project

	This class is the scene manager, it keeps track of all the important details, such as the list of
	all the specific characters, obstacles, targets.

	The calculations for average position and direction is done and stored here.

	Collision detection is made here, as well as new spawns for targets (such as sugar cubes, honey and ants).

	Check for debug drawing is also done here (D key)

 */

public class SceneManager : MonoBehaviour {
	// lists
	public List<GameObject> wayPoints;
	public List<GameObject> flowers;
	public List<GameObject> bees;
	public List<GameObject> ants;
	public List<GameObject> racers;
	public List<GameObject> spiders;
	public List<GameObject> followSpider;
	public List<GameObject> obstacles;
	public List<GameObject> honey;
	public List<GameObject> sugar;

	// objects
	public GameObject futurePosCube;
	public GameObject tempPosCube;
	public GameObject tempPosCubeSpider;
	public GameObject honeyPrefab;
	public GameObject sugarPrefab;
	public GameObject antPreFab;
	public GameObject cameraObj;
	public GameObject leaderSpider;

	// diverse
	public Terrain terrain;
	private TerrainData terrainData;
	public DebugRenderer debugRenderer;
	private Vector3 tempDir;
	public Vector3 posBehindLeader;
	private float time;
	private int spawnTime = 4;
	public bool draw;

	
	// average positions and directions
	public Vector3 averagePositionAnt;
	public Vector3 averageDirectionAnt;
	public Vector3 averagePositionSpider;
	public Vector3 averageDirectionSpider;
	public Vector3 averagePositionRacer;
	public Vector3 averageDirectionRacer;


	// Use this for initialization
	void Start () {
		debugRenderer = gameObject.GetComponent<DebugRenderer> ();
		terrainData = terrain.GetComponent<TerrainCollider> ().terrainData;
		draw = false;
	}
	
	// Update is called once per frame
	void Update () {
		averagePositionAnt = Vector3.zero;
		averageDirectionAnt = Vector3.zero;
		averagePositionSpider = Vector3.zero;
		averagePositionRacer = Vector3.zero;
		averageDirectionRacer = Vector3.zero;
		averageDirectionSpider = Vector3.zero;
	
		honey.RemoveAll (GameObject => GameObject == null);
		ants.RemoveAll (GameObject => GameObject == null);
		sugar.RemoveAll (GameObject => GameObject == null);


		//======================================
		// 		AVG ANT POSITION & DIRECTION
		//======================================
		
		// adds all the average position and direction
		for (int i = 0; i < ants.Count; i++) {
			averagePositionAnt += ants[i].GetComponent<Ants>().position;
			averageDirectionAnt += ants[i].GetComponent<Ants>().direction;
		}
		
		// get the average of them
		averageDirectionAnt = averageDirectionAnt / ants.Count;
		averagePositionAnt = averagePositionAnt / ants.Count;
		
		//======================================
		// 		AVG SPIDER POSITION & DIRECTION
		//======================================

		for (int i = 0; i < followSpider.Count; i++) {
			averagePositionSpider += followSpider[i].GetComponent<FollowSpider>().position;
			averageDirectionSpider += followSpider[i].GetComponent<FollowSpider>().direction;
		}
		
		// get the average of them
		averageDirectionSpider = averageDirectionSpider / followSpider.Count;
		averagePositionSpider = averagePositionSpider / followSpider.Count;


		//======================================
		// 		AVG RACER POSITION
		//======================================

		for (int i = 0; i < racers.Count; i++) {
			averagePositionRacer += racers[i].GetComponent<Racer>().position;
			averageDirectionRacer += racers[i].GetComponent<Racer>().direction;

		}
		
		// get the average of them
		averagePositionRacer = averagePositionRacer / racers.Count;
		averageDirectionRacer = averageDirectionRacer / racers.Count;

		// check for collision
		CatchTarget ();

		// check if debugs should be drawn or not
		if (Input.GetKeyUp (KeyCode.D)) {
			draw = !draw;
		}

		// start timer to spawn ants
		if (ants.Count <= 9) {
			time += Time.deltaTime;
			if (time > spawnTime)
			{
				addAnts();
				time = 0f;
			}
		}

		// transform the position of the object which the smooth camera is following
		// behind the racers
		averagePositionRacer.y = averagePositionRacer.y + 2f;
		cameraObj.transform.position = averagePositionRacer;
		cameraObj.transform.forward = averageDirectionRacer;

		// cube to show avg pos of the ants, and the position behind the leader spider
		Destroy (tempPosCube);
		Destroy (tempPosCubeSpider);

		// draw debug objects
		if (draw) {
			tempDir = averagePositionAnt + (averageDirectionAnt * 2f);
			FuturePos ();
			if (ants.Count > 0)
			{
				debugRenderer.DrawLine (averagePositionAnt, tempDir, debugRenderer.Materials [0]);
			}
			debugRenderer.DrawLine (leaderSpider.transform.position, posBehindLeader, debugRenderer.Materials [0]);

		}

		// calculate the position behind the leader spider
		posBehindLeader = leaderSpider.GetComponent<VehicleMovement> ().velocity * -1;
		posBehindLeader.Normalize ();
		posBehindLeader *= 5f;
		posBehindLeader += leaderSpider.transform.position;
		posBehindLeader.y = GetHeightWorldCoords(terrainData, new Vector2(posBehindLeader.x, posBehindLeader.z)) + 1f;
	}

	// draws the cubes for avg ant position and pos behind leader
	private void FuturePos()
	{
		if (ants.Count > 1) {
			tempPosCube = Instantiate (futurePosCube, averagePositionAnt, Quaternion.identity) as GameObject;
		}

		tempPosCubeSpider = Instantiate (futurePosCube, posBehindLeader, Quaternion.identity) as GameObject;

	}


	/// <summary>
	/// Check for collisions with ttargets
	/// </summary>
	private void CatchTarget()
	{
		// ==================================
		//	Ants catching sugar cubes
		// ==================================
		for(int i = 0; i < ants.Count; i++)
		{
			Vector3 charPos = ants[i].transform.position;
			for(int j = 0; j < sugar.Count; j++)
			{
				Vector3 targetPos = sugar[j].transform.position;
				
				Vector3 distance = targetPos - charPos;
				float mag = distance.magnitude;

				if (mag <= 3f)
				{
					Destroy(sugar[j]);
					addSugar();
					break;
				}
			}
		}

		// ==================================
		//	Bees catching honey
		// ==================================
		
		for(int i = 0; i < bees.Count; i++)
		{
			Vector3 charPos = bees[i].transform.position;
			for(int j = 0; j < honey.Count; j++)
			{
				Vector3 targetPos = honey[j].transform.position;
				
				Vector3 distance = targetPos - charPos;
				float mag = distance.magnitude;

				if (mag <= 3f)
				{
					Destroy(honey[j]);
					addHoney();
					break;
				}
			}
		}


		// ==================================
		//	Spiders catching ants
		// ==================================
		for(int i = 0; i < spiders.Count; i++)
		{
			Vector3 charPos = spiders[i].transform.position;
			for(int j = 0; j < ants.Count; j++)
			{
				Vector3 targetPos = ants[j].transform.position;
				
				Vector3 distance = targetPos - charPos;
				float mag = distance.magnitude;

				if (mag <= 2f)
				{
					Destroy(ants[j]);
					break;
				}
			}
		}
	}


	/// <summary>
	/// Spawns new sugar cubes at a random position, which is not overlapping with obstacles
	/// </summary>
	private void addSugar()
	{
		Vector3 spawnPoint = GetRandomPoint();

		GameObject copy = Instantiate (sugarPrefab, spawnPoint, Quaternion.identity) as GameObject;
		sugar.Add (copy);
	}

	/// <summary>
	/// Spawns a new ant around a certain area close to the log
	/// </summary>
	private void addAnts()
	{

		Vector3 randomPos = new Vector3 (Random.Range (16f, 18f), 0.5f, Random.Range (120f, 122f));
		
		randomPos.y = GetHeightWorldCoords(terrainData, new Vector2(randomPos.x, randomPos.z)) + 0.3f;

		GameObject copy = Instantiate (antPreFab, randomPos, Quaternion.identity) as GameObject;
		ants.Add (copy);
	}

	/// <summary>
	/// Finds a random point that is not overlapping with obstacle
	/// </summary>
	/// <returns>The random point.</returns>
	private Vector3 GetRandomPoint()
	{
		int n = 0;
		Vector3 randomPoint = Vector3.zero;
		// keep looking for a random point until it's one that's not taken
		// or too close
		// all current obstacles
		while (n < obstacles.Count) {
			// get a spawnpoint
			randomPoint = new Vector3 (Random.Range (35f, 170f), 0f, Random.Range (30f, 170f));
			
			// check if there is already a flower at this point
			for(int i = 0; i < obstacles.Count; i++) {
				// get the position of the flower
				Vector3 currPos = obstacles[i].transform.position;
				// check if they are overlapping or not
				if ((randomPoint.x >= currPos.x + 10f) || (randomPoint.x <= currPos.x - 10f) &&
				    (randomPoint.z >= currPos.z + 10f) || (randomPoint.z <= currPos.z - 10f))
				{
					n++;
				}
			}
			randomPoint.y = GetHeightWorldCoords(terrainData, new Vector2(randomPoint.x, randomPoint.z)) + 0.3f;
		}

		return randomPoint;
	}


	// method written by FlyingOstriche 
	// http://answers.unity3d.com/questions/339191/how-can-i-get-the-y-position-of-a-spot-on-my-terra.html
	static float GetHeightWorldCoords(TerrainData terrainData,Vector2 point)
	{
		Vector3 scale=terrainData.heightmapScale;
		return (float)terrainData.GetHeight((int)(point.x/scale.x),(int)(point.y/scale.z));
	}

	/// <summary>
	/// Spawns honey on top of a random flower
	/// </summary>
	private void addHoney()
	{
		// gets a random flower from the list
		GameObject randomFlower = flowers [Random.Range (0, flowers.Count)];
		Vector3 randomPos = randomFlower.transform.position;
		// y pos depends on the flower, the white kind has a gameobject on top, the
		// honey is spawned at this position
		if (randomFlower.tag == "Rose") {
			randomPos.y = 8.6f;
		} else {
			Vector3 topFlower = randomFlower.transform.GetChild (1).transform.position;
			randomPos = topFlower;
		}
		
		GameObject copy = Instantiate (honeyPrefab, randomPos, Quaternion.identity) as GameObject;
		honey.Add (copy);
	}
}
