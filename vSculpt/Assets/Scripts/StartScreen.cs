using UnityEngine;
using System.Collections;

public class StartScreen : MonoBehaviour {

	void Start()
	{
		GameObject.Find ("Tool").GetComponent<MouseInputManager> ().enabled = false;
	}

	public void OnStartButtonClicked()
	{
		transform.gameObject.SetActive (false);
		GameObject.Find ("Tool").GetComponent<MouseInputManager> ().enabled = true;
	}

	public void OnExitButtonClicked()
	{
		Application.Quit ();
	}
}
