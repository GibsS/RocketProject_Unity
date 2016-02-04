using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour {

	public float nudge;
	public float walkSpeed;
	public float jumpSpeed;
	public Vector2 gravity;

	CharacterMotor2D motor;

	Vector2 speed;

	bool isGrounded;

	void Start () {
		motor = GetComponent<CharacterMotor2D> ();
	}

	void Update () {
		isGrounded = (motor.contactCount >= 1 && Vector2.Dot (Vector2.up, motor.contactInfos[0].getNormal()) > 0)
					 || (motor.contactCount >= 2 && Vector2.Dot (Vector2.up, motor.contactInfos[1].getNormal()) > 0);
		
		speed += gravity * Time.deltaTime;

		if (isGrounded) {
			if(Input.GetKey(KeyCode.Q)) {
				speed.x = -walkSpeed;
			} else if(Input.GetKey(KeyCode.D)) {
				speed.x = walkSpeed;
			} else {
				speed = Vector2.zero;
			}
			if(Input.GetKeyDown(KeyCode.Space)) {
				speed.y += jumpSpeed;
			}
		} else {
			if(Input.GetKey(KeyCode.Q)) {
				speed += - nudge * Time.deltaTime * Vector2.right;
			}
			if(Input.GetKey(KeyCode.D)) {
				speed += nudge * Time.deltaTime * Vector2.right;
			}
		}

		Vector2 movement = speed * Time.deltaTime;
		while (Vector2.Distance(movement, Vector2.zero) > 0.001f) {
			movement = motor.move (movement);
			if(motor.contactCount >= 1)
				speed -= Vector2.Dot(speed, motor.contactInfos[0].getNormal()) * motor.contactInfos[0].getNormal();
		}

		/*if (Input.GetKeyDown (KeyCode.Q)) {
			motor.move(-Vector2.right/3);
		}
		if (Input.GetKeyDown (KeyCode.D)) {
			motor.move (-Vector2.left/3);
		}
		if (Input.GetKeyDown (KeyCode.Z)) {
			motor.move(Vector2.up/3);
		}
		if (Input.GetKeyDown (KeyCode.S)) {
			motor.move (-Vector2.up / 3);
		}*/
	}

	void OnDrawGizmos() {
		//if(motor != null)
		//	Gizmos.DrawSphere (transform.position, motor.radius);
	}
}

