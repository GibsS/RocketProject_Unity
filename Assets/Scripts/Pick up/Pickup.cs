using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Player") {
			RocketController rocketController = coll.gameObject.GetComponent<RocketController>();
			if(!rocketController.isMaxLevel()) {
				rocketController.addRocket();
				Destroy (this.gameObject);
			}
		}
	}
}
