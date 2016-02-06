using UnityEngine;
using System.Collections;

public class RocketController : MonoBehaviour {
	
	// Nudging
	public float nudge;

	// Walking
	public float walkGroundAcceleration;
	public float walkWallAcceleration;
	public float walkCeilingAcceleration;
	public float ceilingDropOffSpeed;
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
		int contactCount = motor.getContactCount ();
		Vector2 leftTangent = Vector2.zero;
		Vector2 rightTangent = Vector2.zero;
		Vector2 leftNormal = Vector2.zero;
		Vector2 rightNormal = Vector2.zero;
		bool isLeftMovement = Vector2.Angle (speed, leftTangent) < Vector2.Angle (speed, rightTangent);
		if (contactCount >= 1) {
			leftTangent = motor.getLeftTangent ();
			rightTangent = motor.getRightTangent ();
			leftNormal = motor.getLeftNormal ();
			rightNormal = motor.getRightNormal ();
		}

		bool isFree = contactCount == 0;

		/* 0 : free
		 * 1 : grounded
		 * 2 : between ground and left wall
		 * 3 : between ground and right wall
		 * 4 : left wall
		 * 5 : right wall
		 * 6 : between left wall and ceiling
		 * 7 : between right wall and ceiling
		 * 8 : between ground and left ceiling
		 * 9 : between ground and right ceiling
		 * 10 : ceiling
		 * 11 : two walls
		 * */
		int positionState = -1;  

		if (!isFree) {
			float leftAngle = Vector2.Angle(-Vector2.right, leftTangent);
			float rightAngle = Vector2.Angle(Vector2.right, rightTangent);

			if(leftAngle < groundAngle) {
				if(rightAngle < groundAngle) {
					positionState = 1;
				} else if(rightAngle <= 90) {
					positionState = 3;
				} else {
					positionState = 9;
				}
			} else if(leftAngle <= 90) {
				if(rightAngle < groundAngle) {
					positionState = 2;
				} else if(rightAngle <= 90) {
					if(Vector2.Dot (rightTangent, Vector2.up) > 0) {
						if(Vector2.Dot (leftTangent, Vector2.up) > 0) {
							positionState = 11;
						} else {
							positionState = 5;
						}
					} else {
						if(Vector2.Dot (leftTangent, Vector2.up) > 0) {
							positionState = 4;
						} else {
							// impossible
						}
					}
				} else {
					positionState = 7;
				}
			} else {
				if(rightAngle < groundAngle) {
					positionState = 8;
				} else if(rightAngle <= 90) {
					positionState = 6;
				} else {
					positionState = 10;
				}
			}
		} else {
			positionState = 0;
		}

		bool ld = Input.GetKey (KeyCode.Q);
		bool rd = Input.GetKey (KeyCode.D);
		bool ud = Input.GetKey (KeyCode.Z);
		bool bd = Input.GetKey (KeyCode.S);
		bool jump = Input.GetKey (KeyCode.Space);

		if (positionState == 0) { // free
			Vector2 drag = - speed * airDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += - nudge * Vector2.right * Time.deltaTime;
			} else if(!ld && rd) {
				speed += nudge * Vector2.right * Time.deltaTime;
			}
			speed += gravity * Time.deltaTime;
			speed += drag;
		} else if (positionState == 1) { // grounded
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkGroundAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkGroundAcceleration * rightTangent * Time.deltaTime;
			}

			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += drag;
		} else if (positionState == 2) { // between ground and left wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkWallAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkGroundAcceleration * rightTangent * Time.deltaTime;
			}

			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += drag;
		} else if (positionState == 3) { // between ground and right wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkGroundAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkWallAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += drag;
		} else if (positionState == 4) { // left wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkWallAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkWallAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += gravity * Time.deltaTime;
			speed += drag;
		} else if (positionState == 5) { // right wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkWallAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkWallAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += gravity * Time.deltaTime;
			speed += drag;
		} else if (positionState == 6) { // between left wall and ceiling
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkCeilingAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkWallAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * rightNormal;
			}
			speed += drag;
		} else if (positionState == 7) { // between right wall and ceiling
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkWallAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkCeilingAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * leftNormal;
			}
			speed += drag;
		} else if (positionState == 8) { // between ground and left ceiling
			if(!ld && rd) {
				speed += walkGroundAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
		} else if (positionState == 9) { // 9 : between ground and right ceiling
			if(ld && !rd) {
				speed += walkGroundAcceleration * leftTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * leftNormal;
			}
		} else if (positionState == 10) { // ceiling
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld && !rd) {
				speed += walkCeilingAcceleration * leftTangent * Time.deltaTime;
			} else if(!ld && rd) {
				speed += walkCeilingAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * leftNormal;
			}
			if(Vector2.Distance(speed, Vector2.zero) < ceilingDropOffSpeed) {
				speed += gravity * Time.deltaTime;
			}
			speed += drag;
		} else if (positionState == 11) { // two walls
			// not handled
		}

		if (contactCount >= 1) {
			Vector2 normal = motor.getLeftNormal();
			if (Vector2.Dot (normal, speed) < 0)
				speed -= Vector2.Dot (speed, normal) * normal;
		}
		
		if (contactCount == 2) {
			Vector2 normal = motor.getRightNormal();
			if (Vector2.Dot (normal, speed) < 1)
				speed -= Vector2.Dot (speed, normal) * normal;
		}
		
		Vector2 movement = speed * Time.deltaTime;
		while (Vector2.Distance(movement, Vector2.zero) > 0.001f) {
			movement = motor.move (movement);
			if(motor.getContactCount() >= 1)
				speed -= Vector2.Dot(speed, motor.getLeftNormal()) * motor.getLeftNormal();
		}
	}
}
