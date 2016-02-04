using UnityEngine;
using System.Collections;

public class OscillatingRotationPlatform : MonoBehaviour {

	Rigidbody2D rigid;
	
	void Start () {
		rigid = GetComponent<Rigidbody2D> ();
	}
	
	void Update () {
		rigid.angularVelocity = 2*Mathf.Sin(Time.time * 2 * Mathf.PI / 5);
	}
}
