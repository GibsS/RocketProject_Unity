using UnityEngine;
using System.Collections.Generic;

public class Test : MonoBehaviour {
	 	
	public EdgeCollider2D[] coll;
	
	void OnDrawGizmos() {
		test3 ();
	}

	void test1() {
		ContactSolver cs = new ContactSolver ();
		Vector2 O = transform.TransformPoint(coll [0].points [0] + coll[0].offset);
		Vector2 E = transform.TransformPoint(coll [0].points [1] + coll[0].offset);
		
		Vector2 K = new Vector2 (0.2f, 1);
		float R = 0.5f;
		
		List<float> ts = cs.getFirstContactPoint (O, E, K, R);
		
		Gizmos.DrawLine (O, E);
		Gizmos.DrawSphere (K, 0.1f);
		foreach(float t in ts)
			Gizmos.DrawSphere (O + t * (E - O), R);
	}

	void test2() {
		ContactSolver cs = new ContactSolver ();
		Vector2 O = transform.TransformPoint(coll [0].points [0] + coll[0].offset);
		Vector2 E = transform.TransformPoint(coll [0].points [1] + coll[0].offset);
		Vector2 A = transform.TransformPoint(coll [1].points [0] + coll[1].offset);
		Vector2 B = transform.TransformPoint(coll [1].points [1] + coll[1].offset);
		float R = 0.2f;
		
		Gizmos.DrawLine (O, E);
		Gizmos.DrawLine (A, B);
		
		foreach (float t in cs.getFirstContactLine(O, E, A, B, R))
			Gizmos.DrawSphere (O + t * (E - O), R);
	}

	void test3() {
		ContactSolver cs = new ContactSolver ();
		Vector2 O = transform.TransformPoint(coll [0].points [0] + coll[0].offset);
		Vector2 E = transform.TransformPoint(coll [0].points [1] + coll[0].offset);
		Vector2 A = transform.TransformPoint(coll [1].points [0] + coll[1].offset);
		Vector2 B = transform.TransformPoint(coll [1].points [1] + coll[1].offset);
		float R = 0.2f;
		
		Gizmos.DrawLine (O, E);
		Gizmos.DrawLine (A, B);

		MovementResult contact = cs.getFirstContact (O, E, A, B, R);
		if (contact.hasContact) {
			Gizmos.DrawSphere (contact.getContactPosition (), R);
			Gizmos.DrawSphere(contact.contactPoint, 0.1f);
		}
	}

	void test4() {
		Vector2 A = transform.TransformPoint(coll [0].points [0]);
		Vector2 B = transform.TransformPoint(coll [0].points [1]);
		Vector2 O = transform.TransformPoint(coll [0].points [0]);


	}
}
