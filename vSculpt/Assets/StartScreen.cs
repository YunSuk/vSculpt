using UnityEngine;
using System.Collections;

public class StartScreen : MonoBehaviour {

	public void OnStartButtonClicked()
	{
		transform.gameObject.SetActive (false);
	}

	public void OnExitButtonClicked()
	{
		Application.Quit ();
	}
}
