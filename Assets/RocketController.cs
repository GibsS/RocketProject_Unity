using UnityEngine;
using System.Collections;

public class RocketController : MonoBehaviour {
	
	// Nudging
	public float nudge;

	// Walking
	public float walkGroundAcceleration;
	public float walkWallAcceleration;
	public float walkCeilingAcceleration;
	public float slowAcceleration;

	// Jumping
	public float jumpSpeed;

	// Angle
	public float groundAngle;

	// External force
	public Vector2 gravity;
	public float airDrag;
	public float groundDrag;
	
	CharacterMotor2D motor;
	
	Vector2 speed;
	
	void Start () {
		motor = GetComponent<CharacterMotor2D> ();
	}
	
	void Update () {
		/*bool isFree = motor.contactCount == 0;

		bool isGrounded = (motor.contactCount >= 1 && Vector2.Angle (Vector2.up, motor.contactInfos[0].getNormal()) < groundAngle)
			|| (motor.contactCount >= 2 && Vector2.Angle (Vector2.up, motor.contactInfos[1].getNormal()) < groundAngle);

		bool isOnWall = ((motor.contactCount >= 1 && Vector2.Angle (Vector2.up, motor.contactInfos[0].getNormal()) <= 90)
			|| (motor.contactCount >= 2 && Vector2.Angle (Vector2.up, motor.contactInfos[1].getNormal()) <= 90)) && !isGrounded;

		bool isOnCeiling = !isFree && !isOnWall && !isGrounded;

		if (isFree) {
			Vector2 drag = airDrag * speed;

			if(Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.D)) {

			} else if(!Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.D)) {

			}

			speed -= drag * Time.deltaTime;
			speed += gravity * Time.deltaTime;
		} else if (isGrounded) {
			if(Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.D)) {
				
			} else if(!Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.D)) {
				
			}
			
			if(Input.GetKeyDown(KeyCode.Space)) {
				
			}
		} else if (isOnWall) {
			bool left = Input.GetKey(KeyCode.Q);
			bool right = Input.GetKey(KeyCode.D);
			bool up = Input.GetKey(KeyCode.Z);
			bool down = Input.GetKey(KeyCode.S);

		} else if (isOnCeiling) {

		}
		
		if (motor.contactCount >= 1) {
			Vector2 normal = motor.contactInfos [0].getNormal ();
			if (Vector2.Dot (normal, speed) < 0)
				speed -= Vector2.Dot (speed, normal) * normal;
		}
		
		if (motor.contactCount == 2) {
			Vector2 normal = motor.contactInfos [1].getNormal ();
			if (Vector2.Dot (normal, speed) < 1)
				speed -= Vector2.Dot (speed, normal) * normal;
		}
		
		Vector2 movement = speed * Time.deltaTime;
		while (Vector2.Distance(movement, Vector2.zero) > 0.001f) {
			movement = motor.move (movement);
			if(motor.contactCount >= 1)
				speed -= Vector2.Dot(speed, motor.contactInfos[0].getNormal()) * motor.contactInfos[0].getNormal();
		}*/
	}
}
