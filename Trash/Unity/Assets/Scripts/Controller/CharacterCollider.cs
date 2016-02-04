using UnityEngine;
using System.Collections;

public class CharacterCollider : MonoBehaviour {

	Collider2D charCollider;
	Rigidbody2D body;

	public Collider2D[] contactColliders;
	public Vector2[] contactPoints; // In relative space
	public Vector2[] contactRightTangent; // In relative space

	public LayerMask terrainLayer;

	void Start() {
		contactColliders = new Collider2D[2];
		contactPoints = new Vector2[2];
		contactRightTangent = new Vector2[2];

		body = GetComponent<Rigidbody2D> ();
		charCollider = GetComponent<CircleCollider2D> ();
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (contactColliders [0] == null) {
			Vector2 direction = transform.TransformVector(transform.InverseTransformPoint (coll.contacts [0].point)).normalized;
			float length =  Vector2.Distance(transform.position, coll.contacts[0].point) + 0.1f;

			RaycastHit2D hit1 = Physics2D.Raycast(transform.position, 
			                                     direction,
			                                     length,
			                                     terrainLayer);
			RaycastHit2D hit2 = Physics2D.Raycast (transform.position,
			                                      Quaternion.AngleAxis(1, new Vector3(0, 0, 1))*direction,
			                                      length,
			                                      terrainLayer);

			contactColliders [0] 	= coll.collider;
			contactRightTangent[0] 	= transform.InverseTransformVector(hit2.point - hit1.point).normalized;	
			contactPoints[0] 		= transform.InverseTransformPoint(hit1.point);
		} else if (coll.collider != contactColliders[0] 
		           && contactColliders [1] == null) {
			Vector2 direction = transform.TransformVector(transform.InverseTransformPoint (coll.contacts [0].point)).normalized;
			float length =  Vector2.Distance(transform.position, coll.contacts[0].point) + 0.1f;
			
			RaycastHit2D hit1 = Physics2D.Raycast(transform.position, 
			                                      direction,
			                                      length,
			                                      terrainLayer);
			RaycastHit2D hit2 = Physics2D.Raycast (transform.position,
			                                       Quaternion.AngleAxis(1, new Vector3(0, 0, 1))*direction,
			                                       length,
			                                       terrainLayer);
			
			contactColliders [1] = coll.collider;
			contactRightTangent[1] = transform.InverseTransformVector(hit2.point - hit1.point).normalized;
			contactPoints[1] = transform.InverseTransformPoint(hit1.point);
		} 
	}
	 void OnCollisionStay2D(Collision2D coll) {

	}
	void OnCollisionExit2D(Collision2D coll) {
		if (contactColliders [0] == coll.collider) {
			if(contactColliders[1] == null) {
				contactColliders[0] = null;
			} else {
				contactColliders[0] = contactColliders[1];
				contactPoints[0] = contactPoints[1];
				contactRightTangent[0] = contactRightTangent[1];
				contactColliders[1] = null;
			}
		} else if (contactColliders [1] == coll.collider) {
			contactColliders[1] = null;
		} else {
			throw new UnityException("contact lost when no contact detected");
		}
	}

	void OnDrawGizmos() {
		if (contactColliders != null && contactColliders.Length > 0) {
			if (isInContact ()) {
				Gizmos.color = Color.red;
				Gizmos.DrawRay (getGlobalFirstContactPoint (), getMainLeftTangent ());
				Gizmos.color = Color.green;
				Gizmos.DrawRay (getGlobalFirstContactPoint (), getMainRightTangent ());
				Gizmos.color = Color.blue;
				Gizmos.DrawRay (getGlobalFirstContactPoint(), getMainNormal());
			}
		}
	}
	
	public void switchContact() {
		if (contactColliders [1] != null) {
			Collider2D tmpCollider = contactColliders[0];
			Vector2 tmpPoint = contactPoints[0];
			contactColliders[0] = contactColliders[1];
			contactPoints[0] = contactPoints[1];
			contactColliders[1] = tmpCollider;
			contactPoints[1] = tmpPoint;
		}
	}
		
	public Vector2 getMainLeftTangent() {
		if (contactColliders [0] == null) {
			throw new UnityException("no contact point");
		}
		return - transform.TransformVector(contactRightTangent [0]);
	}
	public Vector2 getMainRightTangent() {
		if (contactColliders [0] == null) {
			throw new UnityException("no contact point");
		}
		return transform.TransformVector(contactRightTangent [0]);
	}
	public Vector2 getMainNormal() {
		if (contactColliders [0] == null) {
			throw new UnityException("no contact point");
		}
		return transform.TransformVector(new Vector2(-contactRightTangent[0].y, contactRightTangent[0].x));
	}
	public Vector2 getLocalFirstContactPoint() {
		if (contactColliders [0] == null) {
			throw new UnityException("no contact point");
		}
		return contactPoints [0];
	}
	public Vector2 getLocalSecondContactPoint() {
		if (contactColliders [1] == null) {
			throw new UnityException("only one contact point");
		}
		return contactPoints[1];
	}
	public Vector2 getGlobalFirstContactPoint() {
		if (contactColliders [0] == null) {
			throw new UnityException("no contact point");
		}
		return transform.TransformPoint(contactPoints [0]);
	}
	public Vector2 getGlobalSecondContactPoint() {
		if (contactColliders [1] == null) {
			throw new UnityException("only one contact point");
		}
		return transform.TransformPoint(contactPoints[1]);
	}
	public Collider2D getFirstContactCollider() {
		if (contactColliders [0] == null) {
			throw new UnityException("no contact point");
		}
		return contactColliders[0];
	}
	public Collider2D getSecondContactCollider() {
		if (contactColliders[1] == null) {
			throw new UnityException("only one contact point");
		}
		return contactColliders[1];
	}
	public bool isInContact() {
		return contactColliders[0] != null;
	}
	public bool isStuck() {
		return contactColliders [1] != null;
	}
}