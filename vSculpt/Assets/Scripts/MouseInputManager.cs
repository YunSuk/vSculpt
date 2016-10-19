using UnityEngine;
using System.Collections;

public class MouseInputManager : MonoBehaviour {

	public float toolSpeedX = 0.01f;
	public float toolSpeedY = 0.01f;
	public float toolSpeedZ = 1;

	public float cameraMoveSpeedX = 0.1f;
	public float cameraMoveSpeedY = 0.1f;
	public float cameraMoveSpeedZ = 1;

	public float cameraRotationSpeedX = 2;
	public float cameraRotationSpeedY = 2;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float mouseXMovement = Input.GetAxis ("Mouse X");
		float mouseYMovement = Input.GetAxis ("Mouse Y");
		float scrollMovement = Input.GetAxis ("Mouse ScrollWheel");

		if (Input.GetMouseButton (0) || Input.GetMouseButton (2) || Input.GetKey(KeyCode.LeftShift)) {
			Camera.main.transform.position += Camera.main.transform.rotation *
				new Vector3(-cameraMoveSpeedX * mouseXMovement, -cameraMoveSpeedY * mouseYMovement, cameraMoveSpeedZ * scrollMovement);
			return;
		}
		if (Input.GetMouseButton (1)) {
			//Quaternion rotX = Quaternion.AngleAxis (cameraRotationSpeedX * mouseXMovement, Vector3.up);
			//Quaternion rotY = Quaternion.AngleAxis (cameraRotationSpeedX * mouseYMovement, Camera.main.transform.right);
			Vector3 angles = Camera.main.transform.localEulerAngles;
			angles.x += cameraRotationSpeedY * mouseYMovement;
			angles.y += cameraRotationSpeedX * mouseXMovement;
			Camera.main.transform.localEulerAngles = angles; //= .y = //.rotation *= rotX * rotY;
			print(Camera.main.transform.up);
			return;
		}


		transform.position += Camera.main.transform.rotation *
			new Vector3(toolSpeedX * mouseXMovement, toolSpeedY * mouseYMovement, toolSpeedZ * scrollMovement);

	}
}
