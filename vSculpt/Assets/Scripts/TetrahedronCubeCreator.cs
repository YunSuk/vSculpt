using UnityEngine;
using System.Collections;

public class TetrahedronCubeCreator : MonoBehaviour {

	public GameObject Cube;
	public float CubeSideLength = 0.01f;
	public int CubePerSide = 10;

	// Use this for initialization
	void Start () {

		float startX = CubeSideLength - CubeSideLength * CubePerSide/2;
		float startY = 0.525f;
		float startZ = 0.45f;
		for (int i = 0; i < CubePerSide; i++) {
			for (int j = 0; j < CubePerSide; j++) {
				for (int k = 0; k < CubePerSide; k++) {
					Instantiate (Cube, new Vector3 (startX + CubeSideLength * i, startY + CubeSideLength * j, startZ + CubeSideLength * k), new Quaternion ());
				}
			}
		}
	}
	

}
