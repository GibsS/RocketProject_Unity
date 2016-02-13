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
		isGrounded = (motor.getContactCount() >= 1 && Vector2.Angle (Vector2.up, motor.getLeftNormal()) < 88)
			|| (motor.getContactCount() >= 2 && Vector2.Angle (Vector2.up, motor.getRightNormal()) < 88);
		
		speed += gravity * Time.deltaTime;

		if (isGrounded) {
			if(Input.GetKey(KeyCode.Q) && Vector2.Angle (Vector2.up, motor.getLeftNormal()) < 88) {
				speed = walkSpeed * motor.getLeftTangent();
			} else if(Input.GetKey(KeyCode.D) && Vector2.Angle (Vector2.up, motor.getRightNormal()) < 88) {
				speed = walkSpeed * motor.getRightTangent();
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

		if (motor.getContactCount() >= 1) {
			Vector2 normal = motor.getLeftNormal();
			if (Vector2.Dot (normal, speed) < 0)
				speed -= Vector2.Dot (speed, normal) * normal;
		}

		if (motor.getContactCount() == 2) {
			Vector2 normal = motor.getRightNormal();
			if (Vector2.Dot (normal, speed) < 0)
				speed -= Vector2.Dot (speed, normal) * normal;
		}

		Vector2 movement = speed * Time.deltaTime;
		while (Vector2.Distance(movement, Vector2.zero) > 0.001f) {
			movement = motor.move (movement);
			if(motor.getContactCount() >= 1)
				speed -= Vector2.Dot(speed, motor.getLeftNormal()) * motor.getLeftNormal();
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

}

