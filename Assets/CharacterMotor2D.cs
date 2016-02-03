using UnityEngine;
using System.Collections;

public class CharacterMotor2D : MonoBehaviour {

	ContactSolver contactSolver;

	public float radius;

	public EdgeCollider2D[] edges;
	public int[] edgePoints;
	public Vector2[] contactPoints;

	void Start () {
		contactSolver = new ContactSolver ();

		edges = new EdgeCollider2D[2];
		edgePoints = new int[2];
		contactPoints = new Vector2[2];
	}

	public Vector2 globalFirstEdgePoint(int i) {
		return edges[i].transform.TransformPoint(edges [i].points [edgePoints [i]] + edges[i].offset);
	}
	public Vector2 globalSecondEdgePoint(int i) {
		return edges[i].transform.TransformPoint(edges [i].points [edgePoints [i]+1] + edges[i].offset);
	}
	public Vector2 normal(int i) {
		return (transform.position - transform.TransformPoint(contactPoints[i])).normalized;
	}
	public Vector2 localContactPoint(int i) {
		return contactPoints [i];
	}
	public Vector2 globalContactPoint(int i) {
		return transform.TransformPoint (contactPoints [i]);
	}

	public Vector2 movement(Vector2 direction) {
		Vector2 remainder = Vector2.zero;
		if (edges [0] == null) {
			remainder = freeMove(direction);
		} else if (edges [1] == null) {
			Vector2 n = normal (0);
			if(Vector2.Dot(direction, normal (0)) > 0) {
				remainder =  freeMove(direction);
			} else {
				if(Vector2.Distance(globalContactPoint(0), globalFirstEdgePoint(0)) < 0.001f 
				   || Vector2.Distance(globalContactPoint(0), globalSecondEdgePoint(0)) < 0.001f) {
					Vector2 tangent = direction - Vector2.Dot(direction, n)*n;

					remainder = freeMove(tangent * Vector2.Dot(tangent, direction));
				} else {
					Vector2 tangent = (globalSecondEdgePoint(0) - globalFirstEdgePoint(0)).normalized;

					remainder =  tangent * lineMove(0, Vector2.Dot(tangent, direction));
				}
			}
		} else {
			Vector2 normal1 = -normal (0);
			Vector2 normal2 = -normal (1);

			if(Vector2.Angle(normal1, direction) > 90 && Vector2.Angle(normal2, direction) > 90) {
				remainder = freeMove(direction);
			} else if(Vector2.Angle(normal1, direction) <= 90 && Vector2.Dot (normal2, direction) <= 0) {
				if(Vector2.Distance(globalContactPoint(0), globalFirstEdgePoint(0)) < 0.001f 
				   || Vector2.Distance(globalContactPoint(0), globalSecondEdgePoint(0)) < 0.001f) {
					Vector2 tangent = direction - Vector2.Dot(direction, normal1)*normal1;
					
					remainder = freeMove(tangent * Vector2.Dot(tangent, direction));
				} else {
					Vector2 tangent = (globalSecondEdgePoint(0) - globalFirstEdgePoint(0)).normalized;
					
					remainder =  tangent * lineMove(0, Vector2.Dot(tangent, direction));
				}
			} else if(Vector2.Angle(normal2, direction) <= 90 && Vector2.Dot (normal1, direction) <= 0) {
				if(Vector2.Distance(globalContactPoint(1), globalFirstEdgePoint(1)) < 0.001f 
				   || Vector2.Distance(globalContactPoint(1), globalSecondEdgePoint(1)) < 0.001f) {
					Vector2 tangent = direction - Vector2.Dot(direction, normal2)*normal2;
					
					remainder = freeMove(tangent * Vector2.Dot(tangent, direction));
				} else {
					Vector2 tangent = (globalSecondEdgePoint(1) - globalFirstEdgePoint(1)).normalized;
					
					remainder =  tangent * lineMove(1, Vector2.Dot(tangent, direction));
				}
			}
		}

		return remainder;
	}

	private Vector2 freeMove(Vector2 movement) {
		MoveInformation mi = findCollision(movement);

		edges [0] = null;
		edges [1] = null;

		if (mi.hasContact) {
			transform.position = mi.start + mi.ratio * (mi.end - mi.start);

			edges [0] = mi.edge;
			edgePoints [0] = mi.edgePoint;
			contactPoints [0] = transform.InverseTransformPoint(mi.contactPoint);

			return (1 - mi.ratio)*(mi.end - mi.start);
		} else {
			transform.position += (Vector3)movement;
			return Vector2.zero;
		}
	}

	private float lineMove(int contactId, float movement) {
		Vector2 direction = (globalSecondEdgePoint(contactId) - globalFirstEdgePoint(contactId)).normalized;
		MoveInformation mi = findCollision(movement * direction);

		if (contactId == 0) {
			edges [1] = null;
		} else {
			edges [0] = edges[1];
			edgePoints[0] = edgePoints[1];
			contactPoints[0] = contactPoints[1];
			
			edges [1] = null;
		}

		float reminderLength;
		float movementLength = mi.hasContact?Vector2.Distance (mi.ratio * (mi.end - mi.start), Vector2.zero):Mathf.Abs(movement);
		if (movement > 0) {
			reminderLength = Vector2.Distance (globalSecondEdgePoint (0), globalContactPoint (0));
		} else {
			reminderLength = Vector2.Distance (globalFirstEdgePoint (0), globalContactPoint (0));
		}

		if (movementLength > reminderLength) {
			edges [0] = null;
		}

		if (mi.hasContact) {
			transform.position = mi.start + mi.ratio * (mi.end - mi.start);

			if (edges [0] == null) {
				edges [0] = mi.edge;
				edgePoints [0] = mi.edgePoint;
				contactPoints [0] = transform.InverseTransformPoint(mi.contactPoint);
			} else {
				edges [1] = mi.edge;
				edgePoints [1] = mi.edgePoint;
				contactPoints [1] = transform.InverseTransformPoint(mi.contactPoint);
			}

			return movement - movementLength;
		} else {
			transform.position += (Vector3)direction * movement;
			return 0;
		}
	}

	private MoveInformation findCollision(Vector2 movement) {
		Vector2 start = transform.position;
		Vector2 finish = (Vector2)transform.position + movement;
		
		Collider2D[] coll = Physics2D.OverlapAreaAll (start  - new Vector2 (radius + 1, radius + 1),
		                                              finish + new Vector2 (radius + 1, radius + 1));
		
		EdgeCollider2D collided = null;
		int edgeNo = -1;
		Vector2 contactPoint = Vector2.zero;
		
		float t = 1;
		for (int i = 0; i < coll.Length; i++) {
			EdgeCollider2D edge = (EdgeCollider2D)coll [i];
			
			for (int j = 0; j < edge.points.Length - 1; j++) {
				if((edge != edges[0] || j != edgePoints[0]) && (edge != edges[1] || j != edgePoints[1])) {
					MovementResult mr = contactSolver.getFirstContact (start, 
					                                                   finish, 
					                                                   edge.transform.TransformPoint(edge.points[j] + edge.offset), 
					                                                   edge.transform.TransformPoint(edge.points[j+1] + edge.offset), 
					                                                   radius);
					if (mr.hasContact && mr.ratio < t 
					    && (edges[0] == null || Vector2.Distance(mr.contactPoint, globalContactPoint(0)) > 0.1f)
					    && (edges[1] == null || Vector2.Distance(mr.contactPoint, globalContactPoint(1)) > 0.1f)) {
						t = mr.ratio;
						collided = edge;
						edgeNo = j;
						contactPoint = mr.contactPoint;
					}				
				}
			}
		}
		if (t < 1) 
			return new MoveInformation (collided, edgeNo, contactPoint, start, finish, t);
		else 
			return new MoveInformation();
	}

	void OnDrawGizmos() {
		if (edges != null && edges.Length > 0) {
			if(edges[0] != null) {
				Gizmos.DrawSphere(globalFirstEdgePoint(0), 0.1f);
				Gizmos.DrawSphere(globalSecondEdgePoint(0), 0.1f);
				Gizmos.DrawSphere(globalContactPoint(0), 0.1f);
				Gizmos.DrawRay(globalContactPoint(0), normal(0));
			}

			if(edges[1] != null) {
				Gizmos.DrawSphere(globalFirstEdgePoint(1), 0.1f);
				Gizmos.DrawSphere(globalSecondEdgePoint(1), 0.1f);
				Gizmos.DrawSphere(globalContactPoint(1), 0.1f);
				Gizmos.DrawRay(globalContactPoint(1), normal(1));
			}
		}
	}
}

public class MoveInformation {
	public EdgeCollider2D edge;
	public int edgePoint;
	public Vector2 contactPoint;
	public Vector2 start;
	public Vector2 end;
	public float ratio;
	public bool hasContact;

	public MoveInformation(EdgeCollider2D edge, int edgePoint, Vector2 contactPoint, Vector2 start, Vector2 end, float ratio) {
		this.edge = edge;
		this.edgePoint = edgePoint;
		this.contactPoint = contactPoint; // !!! espace global
		this.start = start;
		this.end = end;
		this.ratio = ratio;
		this.hasContact = true;
	}
	public MoveInformation() {
		this.hasContact = false;
	}
}