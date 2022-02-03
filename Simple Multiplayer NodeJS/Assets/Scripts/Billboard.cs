using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	public Camera cam;

	void Update () {
		transform.LookAt(cam.transform);
		//transform.LookAt (Camera.main.transform);
	}
}
