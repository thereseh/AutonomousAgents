using UnityEngine;
using System.Collections;

/*
		Therese Henriksson
		IGME 202
		Final Project

		This class swaps between cameras to get different perspective.
		The code was given to us by Erin Cascioli

		It also draws a GUI, which gives direction of the camera use,
		as well as shows details about which camera that's active
 	*/

public class Cameras : MonoBehaviour {
	public Camera[] cameras;

	private int currentCameraIndex;

	// Use this for initialization
	void Start () {

		currentCameraIndex = 0;

		for (int i = 1; i < cameras.Length; i++) 
		{
			cameras[i].gameObject.SetActive(false);
		}

		if (cameras.Length > 0) 
		{
			cameras[0].gameObject.SetActive(true);
		}
	
	}

	void OnGUI()
	{
		// Change the color of the box
		GUI.color = Color.black;
		
		// Increase font size
		GUI.skin.box.fontSize = 12;
		
		// Craw the text at (10, 10)
		GUI.Box ( new Rect(10, 10, 200, 25), "Press 'c' to change camera views");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.C)) 
		{
			currentCameraIndex++;

			if(currentCameraIndex < cameras.Length)
			{
				cameras[currentCameraIndex-1].gameObject.SetActive(false);
				cameras[currentCameraIndex].gameObject.SetActive(true);
			}
			else {
				cameras[currentCameraIndex-1].gameObject.SetActive(false);
				currentCameraIndex = 0;
				cameras[currentCameraIndex].gameObject.SetActive(true);
			}
		}
	
	}
}
