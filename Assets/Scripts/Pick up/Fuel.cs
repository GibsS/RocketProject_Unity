using UnityEngine;
using System.Collections;

public class Fuel : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Player") {
			RocketController rocketController = coll.gameObject.GetComponent<RocketController>();
			rocketController.addFuel(5);
			Destroy (this.gameObject);
		}
	}
}
