using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour {

	CharacterCollider characterCollider;
	Rigidbody2D rigid;

	void Start () {
		characterCollider = GetComponent<CharacterCollider> ();
		rigid = GetComponent<Rigidbody2D> ();
	}

	void Update () {
		if (characterCollider.isInContact ()) {
			transform.parent = characterCollider.getFirstContactCollider ().transform;
			rigid.velocity = Vector2.zero;
		} else
			transform.parent = null;
	}
}

