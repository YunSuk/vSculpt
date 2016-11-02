﻿using UnityEngine;
using System.Collections;

public class PauseScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Deactivate the Pause Screen Components
		transform.GetChild (0).gameObject.SetActive (false);
	}
		
	public void OnStopRecordingButtonClicked()
	{
		transform.GetChild (0).gameObject.SetActive (true);
		print ("Stop Recording Button Clicked!");
		GameObject.Find ("Tool").GetComponent<MouseInputManager> ().enabled = false;
	}

	public void OnResumeButtonClicked()
	{
		transform.GetChild (0).gameObject.SetActive (false);
		GameObject.Find ("Tool").GetComponent<MouseInputManager> ().enabled = true;
	}

	public void OnExportModelButtonClicked()
	{
		// TODO: Do Export Model
	}

	public void OnExitButtonClicked()
	{
		Application.Quit ();
	}
}
