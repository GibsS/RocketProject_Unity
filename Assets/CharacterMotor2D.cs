using UnityEngine;
using System.Collections;

public class CharacterMotor2D : MonoBehaviour {

	ContactSolver contactSolver;

	public float radius;

	public ContactInfo[] contactInfos;
	public int contactCount;
	
	EdgeCollider2D tester;

	void Start() {
		tester = GetComponent<EdgeCollider2D> ();
		contactSolver = new ContactSolver ();
		contactInfos = new ContactInfo[2];
		contactCount = 0;
	}

	public Vector2 move(Vector2 movement) {
		if (contactCount == 0) {
			return free (movement);
		} else if (contactCount == 1) {
			if(Vector2.Dot(movement, contactInfos[0].getNormal()) > 0) {
				return free (movement);
			} else {
				if(contactInfos[0].isEdgeContact) {
					Vector2 normal = contactInfos[0].getNormal();
					return free (movement - Vector2.Dot (movement, normal)*normal);
				} else {
					return line (0, Vector2.Dot(movement, contactInfos[0].getMainTangent()));
				}
			}
		} else {
			Vector2 normal1 = contactInfos[0].getNormal();
			Vector2 normal2 = contactInfos[1].getNormal();

			Vector2 tangent1 = contactInfos[0].getMainTangent();
			Vector2 tangent2 = contactInfos[1].getMainTangent();

			if(Vector2.Dot(normal1, tangent2) < 0) {
				tangent2 = -tangent2;
			}
			if(Vector2.Dot(normal2, tangent1) < 0) {
				tangent1 = -tangent1;
			}

			bool area1 = Vector2.Dot (movement, normal1) > 0;
			bool area2 = Vector2.Dot (movement, normal2) > 0;
			if(area1 && area2) {
				return free (movement);
			} else if(area1 && Vector2.Dot (movement, tangent2) > 0) {
				return line (1, Vector2.Dot(movement, contactInfos[1].getMainTangent()));
			} else if(area2 && Vector2.Dot (movement, tangent1) > 0) {
				return line (0, Vector2.Dot(movement, contactInfos[0].getMainTangent()));
			} else {
				return Vector2.zero;
			}
		}
	}

	private Vector2 free(Vector2 movement) {
		ContactInfo ci = findContact (movement);
		
		contactCount = 0;
		
		if (ci) {
			contactInfos[0] = ci;
			contactCount = 1;

			Vector2 newPosition = ci.getPosition();

			Vector2 remainder = (Vector2)transform.position + movement - newPosition;
			transform.position = ci.getPosition();
			return remainder;
		} else {
			transform.position += (Vector3)movement;
			return Vector2.zero;
		}
	}
	private Vector2 line(int contactId, float movement) {
		Vector2 movementVec = movement * (contactInfos [contactId].getSecondEdgePoint () - contactInfos [contactId].getFirstEdgePoint ()).normalized;
		Vector2 expectedPosition = (Vector2)transform.position + movementVec;

		ContactInfo ci = findContact (movementVec);

		// Calculate new position :
		Vector2 newPosition;
		if (ci) {
			newPosition = ci.getPosition();
		} else {
			newPosition = expectedPosition;
		}

		// Deleting extra contact
		contactCount = 1;
		if (contactId == 1) {
			contactInfos[0] = contactInfos[1];
		}

		// Updating current contact
		float remainderMovement;
		if (movement > 0) {
			remainderMovement = Vector2.Distance (contactInfos [0].getContactPoint(), 
			                                      contactInfos [0].getSecondEdgePoint ());
		} else {
			remainderMovement = Vector2.Distance (contactInfos [0].getContactPoint(), 
			                                      contactInfos [0].getFirstEdgePoint ());
		}

		if (remainderMovement < Mathf.Abs (movement)) {
			contactCount = 0;
		} else {
			Vector2 A = contactInfos[0].getFirstEdgePoint();
			Vector2 B = contactInfos[0].getSecondEdgePoint();
			Vector2 AB = (B - A).normalized;
			float ABlength = Vector2.Distance(B, A);
			contactInfos[0].contactRatio = Vector2.Dot(newPosition - A, AB)/ABlength;
		}

		// Adding new contact
		if (ci) {
			if(contactCount == 0) {
				contactInfos[0] = ci;
				contactCount = 1;
			} else {
				contactInfos[1] = ci;
				contactCount = 2;
			}
		}

		transform.position = newPosition;
		return expectedPosition - newPosition;
	}

	private ContactInfo findContact(Vector2 movement) {
		Vector2 start = transform.position;
		Vector2 end = (Vector2)transform.position + movement;

		Collider2D[] colliders = Physics2D.OverlapAreaAll (new Vector2 (Mathf.Min (start.x, end.x) - 1, Mathf.Min (start.y, end.y) - 1),
		                                               new Vector2 (Mathf.Max (start.x, end.x) + 1, Mathf.Max (start.y, end.y) + 1));

		float minDistance = Mathf.Infinity;
		TrajectoryInfo minTi = null;
		EdgeCollider2D minEdge = null;
		int minPoint = -1;

		for (int i = 0; i < colliders.Length; i++) {
			EdgeCollider2D edge = (EdgeCollider2D)colliders[i];

			for(int j = 0; j < edge.points.Length-1; j++) {
				bool canCollide = (contactCount < 1 || contactInfos[0].acceptableEdge(edge, j))
								&& (contactCount < 2 || contactInfos[1].acceptableEdge(edge, j))
								&& tester != edge;

				if(canCollide) {
					TrajectoryInfo ti = contactSolver.getFirstContact(start,
					                                                  end,
					                                                  edge.transform.TransformPoint(edge.points[j]+edge.offset),
					                                                  edge.transform.TransformPoint(edge.points[j+1]+edge.offset),
					                                                  radius);

					if(ti) {
						float distance = Vector2.Distance(start, ti.getPosition());
						if (distance < minDistance) {
							minTi = ti;
							minDistance = distance;
							minEdge = edge;
							minPoint = j;
						}
					}
				}
			}
		}

		if (minTi) {
			return new ContactInfo(this, 
			                       minTi.contactRatio,
			                       minEdge,
			                       minPoint,
			                       minTi.side,
			                       minTi.isEdgeContact,
			                       minTi.angle);
		} else {
			return null;
		}
	}

	void OnDrawGizmos() {
		if (contactInfos != null && contactInfos.Length > 0) {
			if(contactCount == 1) {
				Gizmos.DrawSphere (contactInfos[0].getContactPoint(), 0.1f);
				Gizmos.DrawSphere (contactInfos[0].getFirstEdgePoint(), 0.1f);
				Gizmos.DrawSphere (contactInfos[0].getSecondEdgePoint(), 0.1f);
			} else if(contactCount == 2) {
				Gizmos.DrawSphere (contactInfos[1].getContactPoint(), 0.1f);
				Gizmos.DrawSphere (contactInfos[1].getFirstEdgePoint(), 0.1f);
				Gizmos.DrawSphere (contactInfos[1].getSecondEdgePoint(), 0.1f);
			}
		}

		/*if (contactInfos != null && tester.enabled) {
			Vector2 start = tester.transform.TransformPoint (tester.points [0] + tester.offset);
			Vector2 end = tester.transform.TransformPoint (tester.points [1] + tester.offset);

			ContactInfo ci = findContact (end - start);

			if (ci) {
				Gizmos.DrawSphere (ci.getPosition (), radius);
				Gizmos.DrawSphere (ci.getContactPoint (), 0.1f);
			}
		}*/
	}
}

public class ContactInfo {
	public CharacterMotor2D motor;

	public float contactRatio;
	public EdgeCollider2D edge;
	public int edgePoint;
	public bool side;

	public bool isEdgeContact;
	public float angle;

	public ContactInfo(CharacterMotor2D motor, float contactRatio, EdgeCollider2D edge, int edgePoint, bool side, bool isEdgeContact, float angle) {
		this.motor = motor;

		this.contactRatio = contactRatio;
		this.edge = edge;
		this.edgePoint = edgePoint;
		this.side = side;

		this.isEdgeContact = isEdgeContact;
		this.angle = angle;
	}

	public Vector2 getNormal() {
		return (getPosition () - getContactPoint ()).normalized;
	}
	public Vector2 getMainTangent() {
		return (getSecondEdgePoint() - getFirstEdgePoint()).normalized;
	}

	public Vector2 getFirstEdgePoint() {
		return edge.transform.TransformPoint (edge.points [edgePoint] + edge.offset);
	}
	public Vector2 getSecondEdgePoint() {
		return edge.transform.TransformPoint (edge.points [edgePoint+1] + edge.offset);
	}

	public Vector2 getPosition() {
		Vector2 A = getFirstEdgePoint ();
		Vector2 B = getSecondEdgePoint ();
		float R = motor.radius;

		if (isEdgeContact && contactRatio == 1)
			return B + R*(Vector2)(Quaternion.AngleAxis (-angle, new Vector3 (0, 0, 1)) * new Vector2 (-(B - A).normalized.y, (B - A).normalized.x));
		else if (isEdgeContact) 
			return A + R*(Vector2)(Quaternion.AngleAxis (angle, new Vector3 (0, 0, 1)) * new Vector2 (-(B - A).normalized.y, (B - A).normalized.x));
		else 
			return A + contactRatio * (B - A) + (side?1:-1) * R * new Vector2 (-(B - A).normalized.y, (B - A).normalized.x);
	}
	public Vector2 getContactPoint() {
		Vector2 A = getFirstEdgePoint ();
		Vector2 B = getSecondEdgePoint ();
		return A + contactRatio * (B - A);
	}

	public bool acceptableEdge(EdgeCollider2D e, int n) {
		return edge != e || edgePoint != n;
	}

	public static implicit operator bool(ContactInfo d){
		return d != null;
	}
}