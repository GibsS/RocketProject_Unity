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
		isGrounded = motor.edges [0] != null && Vector2.Dot (motor.normal (0), Vector2.up) > 0
			|| motor.edges [1] != null && Vector2.Dot (motor.normal (1), Vector2.up) > 0;

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
			movement = motor.movement (movement);
			if(motor.edges[0] != null)
				speed -= Vector2.Dot(speed, motor.normal(0)) * motor.normal(0);
			if(motor.edges[1] != null) 
				speed -= Vector2.Dot(speed, motor.normal(1)) * motor.normal(1);
		}

		/*if (Input.GetKeyDown (KeyCode.Q)) {
			motor.movement(-Vector2.right/3);
		}
		if (Input.GetKeyDown (KeyCode.D)) {
			motor.movement (-Vector2.left/3);
		}
		if (Input.GetKeyDown (KeyCode.Z)) {
			motor.movement(Vector2.up/3);
		}
		if (Input.GetKeyDown (KeyCode.S)) {
			motor.movement (-Vector2.up/3);
		}*/
	}

	void OnDrawGizmos() {
		if(motor != null)
			Gizmos.DrawSphere (transform.position, motor.radius);
	}
}

