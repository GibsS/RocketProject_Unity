using UnityEngine;
using System.Collections;

public class CharacterMotor2D : MonoBehaviour {

	ContactSolver contactSolver;

	public float radius;

	ContactInfo[] contactInfos;
	public int contactCount;

	int left;
	int right;

	Vector2 leftTangent;
	Vector2 rightTangent;
	
	EdgeCollider2D tester;

	void Start() {
		tester = GetComponent<EdgeCollider2D> ();
		contactSolver = new ContactSolver ();
		contactInfos = new ContactInfo[2];
		contactCount = 0;
	}

	public bool hasContact() {
		return contactCount >= 1;
	}
	public int getContactCount() {
		return contactCount;
	}
	public Vector2 getLeftTangent() {
		return leftTangent;
	}
	public Vector2 getRightTangent() {
		return rightTangent;
	}
	public Vector2 getLeftNormal() {
		if (left == 0) {
			return contactInfos[0].getNormal();
		} else {
			return contactInfos[1].getNormal();
		}
	}
	public Vector2 getRightNormal() {
		if (right == 0) {
			return contactInfos[0].getNormal();
		} else {
			return contactInfos[1].getNormal();
		}
	}
	public EdgeCollider2D getLeftEdgeCollider() {
		if (left == 0) {
			return contactInfos[0].edge;
		} else {
			return contactInfos[1].edge;
		}
	}
	public EdgeCollider2D getRightEdgeCollider() {
		if (right == 0) {
			return contactInfos[0].edge;
		} else {
			return contactInfos[1].edge;
		}
	}
	public Vector2 getLeftFirstPointEdge() {
		if (left == 0) {
			return contactInfos[0].edge.transform
				.TransformPoint(contactInfos[0].edge.points[contactInfos[0].edgePoint] + contactInfos[0].edge.offset);
		} else {
			return contactInfos[1].edge.transform
				.TransformPoint(contactInfos[1].edge.points[contactInfos[1].edgePoint] + contactInfos[1].edge.offset);
		}
	}
	public Vector2 getLeftSecondPointEdge() {
		if (left == 0) {
			return contactInfos[0].edge.transform
				.TransformPoint(contactInfos[0].edge.points[contactInfos[0].edgePoint+1] + contactInfos[0].edge.offset);
		} else {
			return contactInfos[1].edge.transform
				.TransformPoint(contactInfos[1].edge.points[contactInfos[1].edgePoint+1] + contactInfos[1].edge.offset);
		}
	}
	public Vector2 getRightFirstPointEdge() {
		if (right == 0) {
			return contactInfos[0].edge.transform
				.TransformPoint(contactInfos[0].edge.points[contactInfos[0].edgePoint] + contactInfos[0].edge.offset);
		} else {
			return contactInfos[1].edge.transform
				.TransformPoint(contactInfos[1].edge.points[contactInfos[1].edgePoint] + contactInfos[1].edge.offset);
		}
	}
	public Vector2 getRightSecondPointEdge() {
		if (right == 0) {
			return contactInfos[0].edge.transform
				.TransformPoint(contactInfos[0].edge.points[contactInfos[0].edgePoint+1] + contactInfos[0].edge.offset);
		} else {
			return contactInfos[1].edge.transform
				.TransformPoint(contactInfos[1].edge.points[contactInfos[1].edgePoint+1] + contactInfos[1].edge.offset);
		}
	}

	public Vector2 move(Vector2 movement) {
		
		//Debug.Log (movement);
		if (contactCount == 0) {
			return free (movement);
		} else if (contactCount == 1) {
			if(Vector2.Angle(movement, contactInfos[0].getNormal()) < 88) {
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

			Vector2 tangent1 = new Vector2(-normal1.y, normal1.x);
			Vector2 tangent2 = new Vector2(-normal2.y, normal2.x);

			if(Vector2.Dot(normal1, tangent2) < 0) {
				tangent2 = -tangent2;
			}
			if(Vector2.Dot(normal2, tangent1) < 0) {
				tangent1 = -tangent1;
			}

			bool area1 = Vector2.Angle (movement, normal1) < 88;
			bool area2 = Vector2.Angle (movement, normal2) < 88;
			if(area1 && area2) {
				return free (movement);
			} else if(Vector2.Dot (movement, normal1) > 0 && Vector2.Dot (movement, tangent2) > 0) {
				if(Mathf.Abs(Vector2.Dot(movement, contactInfos[1].getMainTangent())) > 0.0001f) {
					if(!contactInfos[1].isEdgeContact)
						return line (1, Vector2.Dot(movement, contactInfos[1].getMainTangent()));
					else 
						return free (movement - Vector2.Dot (movement, normal2)*normal2);
				} else {
					return Vector2.zero;
				}
			} else if(Vector2.Dot (movement, normal2) > 0 && Vector2.Dot (movement, tangent1) > 0) {
				if(Mathf.Abs(Vector2.Dot(movement, contactInfos[0].getMainTangent())) > 0.0001f) {
					if(!contactInfos[0].isEdgeContact)
						return line (0, Vector2.Dot(movement, contactInfos[0].getMainTangent()));
					else
						return free (movement - Vector2.Dot (movement, normal1)*normal1);
				} else {
					return Vector2.zero;
				}
			} else {
				return Vector2.zero;
			}
		}
	}

	// Not tested..
	public Vector2 lineMove(int contactId, float movement) {
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

		if (contactId == 0 && contactCount >= 1) {
			if(contactCount == 2) {
				if(Vector2.Dot(normal2, movement*tangent1) < 0) {
					return Vector2.zero;
				} else {
					return line(0, movement);
				}
			} else {
				return line (0, movement);
			}
		} else if (contactId == 1 && contactCount == 2) {
			if(Vector2.Dot(normal1, movement*tangent2) < 0) {
				return Vector2.zero;
			} else {
				return line(1, movement);
			}
		} else {
			return Vector2.zero;
		}
	}
	// Not tested..
	public Vector2 leftLineMove(float movement) {
		if (contactCount == 1) {
			Vector2 normal = contactInfos[0].getNormal();
			Vector2 left = new Vector2(-normal.y, normal.x);
			Vector2 tangent = contactInfos[0].getMainTangent();
			if(Vector2.Dot (left, tangent) > 0) {
				return line (0, movement);
			} else {
				return line (0, -movement);
			}
		} else if (contactCount == 2) {
			Vector2 normal1 = contactInfos[0].getNormal();
			Vector2 normal2 = contactInfos[1].getNormal();
			
			Vector2 tangent1 = contactInfos[0].getMainTangent();
			Vector2 tangent2 = contactInfos[1].getMainTangent();

			Vector2 outwardTangent1 = tangent1;
			if(Vector2.Dot(normal2, tangent1) < 0) {
				outwardTangent1 = -tangent1;
			}

			if(Vector2.Dot (outwardTangent1, normal2) > 0) {
				Vector2 left = new Vector2(-normal1.y, normal1.x);
				if(Vector2.Dot (left, tangent1) > 0) {
					return line (0, movement);
				} else {
					return line (0, -movement);
				}
			} else {
				Vector2 left = new Vector2(-normal2.y, normal2.x);
				if(Vector2.Dot (left, tangent2) > 0) {
					return line (1, movement);
				} else {
					return line (1, -movement);
				}
			}
		} else {
			return Vector2.zero;
		}
	}
	// Not tested..
	public Vector2 rightLineMove(float movement) {
		if (contactCount == 1) {
			Vector2 normal = contactInfos [0].getNormal ();
			Vector2 right = new Vector2 (normal.y, -normal.x);
			Vector2 tangent = contactInfos [0].getMainTangent ();
			if (Vector2.Dot (right, tangent) > 0) {
				return line (0, movement);
			} else {
				return line (0, -movement);
			}
		} else if (contactCount == 2) {
			Vector2 normal1 = contactInfos [0].getNormal ();
			Vector2 normal2 = contactInfos [1].getNormal ();
			
			Vector2 tangent1 = contactInfos [0].getMainTangent ();
			Vector2 tangent2 = contactInfos [1].getMainTangent ();
			
			Vector2 outwardTangent1 = tangent1;
			if (Vector2.Dot (normal2, tangent1) < 0) {
				outwardTangent1 = -tangent1;
			}
			
			if (Vector2.Dot (outwardTangent1, normal2) > 0) {
				Vector2 right = new Vector2 (normal1.y, -normal1.x);
				if (Vector2.Dot (right, tangent1) > 0) {
					return line (0, movement);
				} else {
					return line (0, -movement);
				}
			} else {
				Vector2 right = new Vector2 (normal2.y, -normal2.x);
				if (Vector2.Dot (right, tangent2) > 0) {
					return line (1, movement);
				} else {
					return line (1, -movement);
				}
			}
		} else {
			return Vector2.zero;
		}
	}

	private Vector2 free(Vector2 movement) {
		ContactInfo ci = findContact (movement);
		
		contactCount = 0;
		
		if (ci) {
			contactInfos[0] = ci;
			contactCount = 1;
			updateContact();

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
		updateContact();
		return expectedPosition - newPosition;
	}

	private void updateContact() {
		if (contactCount == 1) {
			left = right = 0;
			Vector2 normal = contactInfos[0].getNormal();
			leftTangent = new Vector2(-normal.y, normal.x);
			rightTangent = new Vector2(normal.y, -normal.x);
		} else if (contactCount == 2) {
			Vector2 normal1 = contactInfos[0].getNormal();
			Vector2 normal2 = contactInfos[1].getNormal();

			Vector2 left1 = new Vector2(-normal1.y, normal1.x);
			Vector2 right1 = new Vector2(normal1.y, -normal1.x);

			Vector2 left2 = new Vector2(-normal2.y, normal2.x);
			Vector2 right2 = new Vector2(normal2.y, -normal2.x);

			if(Vector2.Dot (normal2, left1) > 0) {
				leftTangent = left1;
				rightTangent = right2;
				left = 0;
				right = 1;
			} else {
				leftTangent = left2;
				rightTangent = right1;
				left = 1;
				right = 0;
			}
		}
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
			if(colliders[i].GetType() == typeof(EdgeCollider2D)) {
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