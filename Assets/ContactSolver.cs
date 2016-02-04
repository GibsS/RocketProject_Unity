using UnityEngine;
using System.Collections.Generic;

public class ContactSolver {

	public TrajectoryInfo getFirstContact(Vector2 O, Vector2 E, Vector2 A, Vector2 B, float R) {
		float t_A = - Vector2.Dot (O - A, B - A) / Vector2.Dot (E - O, B - A);
		float t_B = - Vector2.Dot (O - B, A - B) / Vector2.Dot (E - O, A - B);

		Vector2 ABnormal = new Vector2 (-(B - A).y, (B - A).x);

		float t_position = 2;
		float t_contact = 0;
		bool isEdge = false;

		List<float> l = null;
		if (t_A < t_B) {
			l = getFirstContactPoint(O, E, A, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] <= t_A && l[i] < t_position) {
				t_position = l[i];
				t_contact = 0;
				isEdge = true;
			}

			l = getFirstContactPoint(O, E, B, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] >= t_B && l[i] < t_position && l[i] <= 1) {
				t_position = l[i];
				t_contact = 1;
				isEdge = true;
			}
		} else {
			l = getFirstContactPoint(O, E, A, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] >= t_A && l[i] < t_position && l[i] <= 1) {
				t_position = l[i];
				t_contact = 0;
				isEdge = true;
			}
			
			l = getFirstContactPoint(O, E, B, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] <= t_B && l[i] < t_position) {
				t_position = l[i];
				t_contact = 1;
				isEdge = true;
			}
		}
		l = getFirstContactLine(O, E, A, B, R);
		for(int i = 0; i < l.Count; i++)
		if(l[i] >= 0 && l[i] >= Mathf.Min(t_A, t_B) && l[i] <= Mathf.Max(t_A, t_B) && l[i] < t_position) {
			t_position = l[i];
			Vector2 AB = (B-A).normalized;
			Vector2 pos = O + l[i]*(E - O);
			t_contact = Vector2.Dot(pos - A, AB)/Vector2.Distance(B, A);
			isEdge = false;
		}

		if (t_position <= 1) {
			Vector2 pos = O + t_position*(E - O);
			if(isEdge) {
				if(t_contact == 0)
					return new TrajectoryInfo(A, B, t_contact, false, isEdge, 
			                          Vector2.Angle(ABnormal, pos-A), R);
				else 
					return new TrajectoryInfo(A, B, t_contact, false, isEdge, 
				                          Vector2.Angle(ABnormal, pos-B), R);
			} else {
				return new TrajectoryInfo(A, B, t_contact, Vector2.Dot (pos-A, ABnormal) > 0, isEdge, 
				                          0, R);
			}
		} else {
			return new TrajectoryInfo();
		}
	}

	public List<float> getFirstContactPoint(Vector2 O, Vector2 E, Vector2 K, float R) {
		float a = (E.x - O.x) * (E.x - O.x) + (E.y - O.y) * (E.y - O.y);
		float b = 2 * ((O.x - K.x) * (E.x - O.x) + (O.y - K.y) * (E.y - O.y));
		float c = (O.x - K.x) * (O.x - K.x) + (O.y - K.y) * (O.y - K.y) - R * R;
		float delta = b * b - 4 * a * c;

		List<float> res = new List<float> ();

		if (delta > 0) {
			res.Add((-b-Mathf.Sqrt(delta))/(2*a));
			res.Add((-b+Mathf.Sqrt(delta))/(2*a));
		} else if (delta == 0) {
			res.Add(-b/(2*a));
		} 

		return res;
	}

	public List<float> getFirstContactLine(Vector2 O, Vector2 E, Vector2 A, Vector2 B, float R) {
		Vector2 AB = (B - A).normalized;
		float Ax = ((O.x - A.x) * AB.x + (O.y - A.y) * AB.y) * AB.x - O.x + A.x;
		float Ay = ((O.x - A.x) * AB.x + (O.y - A.y) * AB.y) * AB.y - O.y + A.y;
		float Bx = ((E.x - O.x) * AB.x + (E.y - O.y) * AB.y) * AB.x - (E.x - O.x);
		float By = ((E.x - O.x) * AB.x + (E.y - O.y) * AB.y) * AB.y - (E.y - O.y);

		float c = Ax * Ax + Ay * Ay - R * R;
		float b = 2 * (Ax * Bx + Ay * By);
		float a = Bx * Bx + By * By;
		
		List<float> res = new List<float> ();
		if (a != 0) {
			float delta = b * b - 4 * a * c;

			if (delta > 0) {
				res.Add ((-b - Mathf.Sqrt (delta)) / (2 * a));
				res.Add ((-b + Mathf.Sqrt (delta)) / (2 * a));
			} else if (delta == 0) {
				res.Add (-b / (2 * a));
			} 
		
			return res;
		} else {
			res.Add(-c/b);
			return res;
		}
	}
}

public class TrajectoryInfo {
	public Vector2 A;
	public Vector2 B;
	public float contactRatio;
	public bool side; 
	
	public bool isEdgeContact;
	public float angle;

	public bool hasContact;

	public float R;
	
	public TrajectoryInfo(Vector2 A, Vector2 B, float contactRatio, bool side, bool isEdgeContact, float angle, float R) {
		this.A = A;
		this.B = B;
		this.contactRatio = contactRatio;
		this.side = side;

		this.isEdgeContact = isEdgeContact;
		this.angle = angle;

		this.hasContact = true;

		this.R = R;
	}
	public TrajectoryInfo() {
		this.hasContact = false;
	}

	public Vector2 getPosition() {
		if (isEdgeContact && contactRatio == 1)
			return B + R*(Vector2)(Quaternion.AngleAxis (-angle, new Vector3 (0, 0, 1)) * new Vector2 (-(B - A).normalized.y, (B - A).normalized.x));
		else if (isEdgeContact) 
			return A + R*(Vector2)(Quaternion.AngleAxis (angle, new Vector3 (0, 0, 1)) * new Vector2 (-(B - A).normalized.y, (B - A).normalized.x));
		else 
			return A + contactRatio * (B - A) + (side?1:-1) * R * new Vector2 (-(B - A).normalized.y, (B - A).normalized.x);
	}

	public static implicit operator bool(TrajectoryInfo d){
		return d != null && d.hasContact;
	}
}