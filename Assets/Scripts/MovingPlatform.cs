using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {

	Vector2 np;
	Vector2 nnp;

	void LateUpdate() {
		transform.position = np;
		np = nnp;
	}

	public void setNextPosition(Vector2 nextPosition) {
		this.nnp = nextPosition;
	}

	public Vector2 currentPosition() {
		return transform.position;
	}
	public Vector2 nextPosition() {
		return np;
	}
}
