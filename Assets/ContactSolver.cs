using UnityEngine;
using System.Collections.Generic;

public class ContactSolver {

	public MovementResult getFirstContact(Vector2 O, Vector2 E, Vector2 A, Vector2 B, float R) {
		float t_A = - Vector2.Dot (O - A, B - A) / Vector2.Dot (E - O, B - A);
		float t_B = - Vector2.Dot (O - B, A - B) / Vector2.Dot (E - O, A - B);

		float t_min = 2;
		Vector2 contactPoint = Vector2.zero;

		List<float> l = null;
		if (t_A < t_B) {
			l = getFirstContactPoint(O, E, A, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] <= t_A && l[i] < t_min) {
				t_min = l[i];
				contactPoint = A;
			}

			l = getFirstContactPoint(O, E, B, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] >= t_B && l[i] < t_min && l[i] <= 1) {
				t_min = l[i];
				contactPoint = B;
			}
		} else {
			l = getFirstContactPoint(O, E, A, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] >= t_A && l[i] < t_min && l[i] <= 1) {
				t_min = l[i];
				contactPoint = A;
			}
			
			l = getFirstContactPoint(O, E, B, R);
			for(int i = 0; i < l.Count; i++)
			if(l[i] >= 0 && l[i] <= t_B && l[i] < t_min) {
				t_min = l[i];
				contactPoint = B;		
			}
		}
		l = getFirstContactLine(O, E, A, B, R);
		for(int i = 0; i < l.Count; i++)
		if(l[i] >= 0 && l[i] >= Mathf.Min(t_A, t_B) && l[i] <= Mathf.Max(t_A, t_B) && l[i] < t_min) {
			t_min = l[i];
			Vector2 AB = (B-A).normalized;
			Vector2 pos = O + l[i]*(E - O);
			contactPoint = Vector2.Dot(pos - A, AB)*AB + A;
		}

		if (t_min <= 1) {
			return new MovementResult(O, E, t_min, contactPoint, true);
		} else {
			return new MovementResult();
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

	public bool pointOnSegment(Vector2 M, Vector2 A, Vector2 B) {
		Vector2 H = Vector2.Dot (M - A, (B - A).normalized) * (B - A).normalized + A;
		return Vector2.Dot (M - A, B - A) > 0 && Vector2.Dot (M - B, A - B) > 0 && Vector2.Distance(H, M) < 0.05f;
	}
}

public class MovementResult {
	public Vector2 start;
	public Vector2 expectedFinish;
	public float ratio;
	public Vector2 contactPoint;
	public bool hasContact;
	
	public MovementResult(Vector2 start, Vector2 expectedFinish, float ratio, Vector2 contactPoint, bool hasContact) {
		this.start = start;
		this.expectedFinish = expectedFinish;
		this.hasContact = hasContact;
		this.contactPoint = contactPoint;
		this.ratio = ratio;
	}
	public MovementResult() {
		this.hasContact = false;
	}

	public Vector2 getContactPosition() {
		return start + ratio * (expectedFinish - start);
	}
}