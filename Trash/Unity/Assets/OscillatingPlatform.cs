using UnityEngine;
using System.Collections;

public class OscillatingPlatform : MonoBehaviour {

	Rigidbody2D rigid;

	void Start () {
		rigid = GetComponent<Rigidbody2D> ();
	}

	void Update () {
		rigid.velocity = 10*Mathf.Sin(Time.time * 2 * Mathf.PI / 5)*Vector2.up;
	}
}
