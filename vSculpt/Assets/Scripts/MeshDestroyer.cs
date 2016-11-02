using UnityEngine;
using System.Collections;

public class MeshDestroyer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		foreach (Transform child in transform) {
			GameObject childGO = child.gameObject;
			if (childGO.GetComponent<MeshCollider> () == null) {
				MeshCollider mc = childGO.AddComponent<MeshCollider> ();
				mc.sharedMesh = childGO.GetComponent<MeshFilter> ().sharedMesh;
				mc.convex = true;
				mc.isTrigger = true;

				childGO.AddComponent<TriggerEvent> ();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
