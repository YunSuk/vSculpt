using UnityEngine;
using System.Collections;

public class TriggerEvent : MonoBehaviour {
	void OnTriggerEnter(Collider other)
	{
		transform.gameObject.GetComponent<MeshRenderer> ().enabled = false;
	}
}