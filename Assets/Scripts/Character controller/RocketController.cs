﻿using UnityEngine;
using System.Collections;

public class RocketController : MonoBehaviour {
	
	// Nudging
	public float nudge;

	// Walking
	public float maxWalkSpeed;
	public float zeroSpeed;
	public float walkAcceleration;
	public float idleAcceleration;
	public float switchAcceleration;
	public float ceilingDropOffSpeed;

	// Jumping
	public float jumpSpeed;
	public float maxJumpStop;

	// Angle
	public float groundAngle;

	// External force
	public Vector2 gravity;
	public float airDrag;
	public float groundDrag;

	// Rocket
	public float rocketSpeedPerUnit;
	public float[] rocketForcePerUnit;
	public float[] minRocketForce;
	public float maxRocketSpeed;
	public float maxRocketForce;

	public float maxFuel;
	public float fuelPerNewton;

	CharacterMotor2D motor;
	
	public int rocketLevel;
	public float fuel;
	
	Vector2 speed;

	//bool jumping;
	//float lastJump;

	void Start () {
		motor = GetComponent<CharacterMotor2D> ();
	}
	
	void Update () {
		movement ();

		shooting ();
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Deadly") {
			kill ();
		}
	}

	void OnGUI() {
		GUI.Label (new Rect (10, 10, 200, 20), "Fuel : " + getFuel ());
		GUI.Label (new Rect (10, 30, 200, 20), "Rocket level : " + getRocketLevel ());
	}

	void shooting() {
		if (rocketLevel >= 1 && Input.GetMouseButtonDown(1)) {
			rocketLevel --;
			GameObject rocket = (GameObject)Instantiate(Resources.Load ("Prefabs/Rocket"));
			rocket.GetComponent<Rigidbody2D>().velocity = 
				(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized 
					* Mathf.Max(rocketSpeedPerUnit*Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position), maxRocketSpeed);
			rocket.transform.position = transform.position;
		}
	}

	void movement() {
		// Gathering information :
		int contactCount = motor.getContactCount ();
		Vector2 leftTangent = Vector2.zero;
		Vector2 rightTangent = Vector2.zero;
		Vector2 leftNormal = Vector2.zero;
		Vector2 rightNormal = Vector2.zero;
		Vector2 oldSpeed = speed;
		float oldSpeedNorm = Vector2.Distance (speed, Vector2.zero);

		if (contactCount >= 1) {
			leftTangent = motor.getLeftTangent ();
			rightTangent = motor.getRightTangent ();
			leftNormal = motor.getLeftNormal ();
			rightNormal = motor.getRightNormal ();
		}
		bool isLeftMovement = Vector2.Angle (speed, leftTangent) < Vector2.Angle (speed, rightTangent);

		bool leftGround = Vector2.Angle (Vector2.up, leftNormal) < groundAngle;
		bool rightGround = Vector2.Angle (Vector2.up, rightNormal) < groundAngle;
		bool leftWall = Vector2.Angle (Vector2.up, leftNormal) < 89 && !leftGround;
		bool rightWall = Vector2.Angle (Vector2.up, rightNormal) < 89 && !rightGround;
		bool leftCeiling = !leftGround && !leftWall;
		bool rightCeiling = !rightGround && !rightWall;

		// Movement choices : 
		bool ld = Input.GetKey (KeyCode.Q) && !Input.GetKey (KeyCode.D);
		bool rd = Input.GetKey (KeyCode.D) && !Input.GetKey (KeyCode.Q);
		bool ud = Input.GetKey (KeyCode.Z) && !Input.GetKey (KeyCode.S);
		bool bd = Input.GetKey (KeyCode.S) && !Input.GetKey (KeyCode.Z);
		bool stick = Input.GetKey (KeyCode.LeftShift);
		bool jump = Input.GetKeyDown (KeyCode.Space);

		// drag
		bool applyAirDrag = contactCount == 0;
		if (applyAirDrag) {
			speed -= oldSpeed * airDrag * Time.deltaTime;
		} else {
			speed -= oldSpeed * groundDrag * Time.deltaTime;
		}

		// gravity
		bool applyGrav = contactCount == 0;
		applyGrav |= contactCount == 1 && (leftWall || (leftCeiling && (oldSpeedNorm < ceilingDropOffSpeed || !stick)));
		applyGrav |= contactCount == 2 && (leftWall || (leftCeiling && (oldSpeedNorm < ceilingDropOffSpeed || !stick)))
									 && (rightWall || (rightCeiling && (oldSpeedNorm < ceilingDropOffSpeed || !stick)));
		if (applyGrav) {
			speed += gravity * Time.deltaTime;
		} else if (stick && leftCeiling) {
			speed += - Vector2.Dot (gravity, -leftTangent) * leftTangent * Time.deltaTime;
		} else if (stick && rightCeiling) {
			speed += - Vector2.Dot (gravity, -rightTangent) * rightTangent * Time.deltaTime;
		}
		// walk 
		bool walkRight = (Vector2.Angle (Vector2.up, rightNormal) < 80 && rd);
		walkRight |= (Vector2.Angle (-Vector2.up, rightNormal) < 80 && ld);
		walkRight |= (Vector2.Angle (Vector2.right, rightNormal) < 80 && bd);
		walkRight |= (Vector2.Angle (-Vector2.right, rightNormal) < 80 && ud);

		bool walkLeft = (Vector2.Angle (Vector2.up, leftNormal) < 80 && ld);
		walkLeft |= (Vector2.Angle (-Vector2.up, leftNormal) < 80 && rd);
		walkLeft |= (Vector2.Angle (Vector2.right, leftNormal) < 80 && ud);
		walkLeft |= (Vector2.Angle (-Vector2.right, leftNormal) < 80 && bd);

		bool applyRightWalk = contactCount >= 1 && rightGround;
		applyRightWalk |= contactCount >= 1 && rightWall && (Vector2.Dot (rightTangent, Vector2.up) < 0 || (!isLeftMovement && oldSpeedNorm > 0.3f));
		applyRightWalk |= contactCount >= 1 && rightCeiling && !isLeftMovement && oldSpeedNorm > 0.3f;


		bool applyLeftWalk = contactCount >= 1 && leftGround;
		applyLeftWalk |= contactCount >= 1 && leftWall && (Vector2.Dot (leftTangent, Vector2.up) < 0 || (isLeftMovement && oldSpeedNorm > 0.3f));
		applyLeftWalk |= contactCount >= 1 && leftCeiling && isLeftMovement && oldSpeedNorm > 0.3f;

		// walk right
		if (applyRightWalk && walkRight && (isLeftMovement || oldSpeedNorm < maxWalkSpeed)) {
			if(isLeftMovement) {
				Vector2 delta = switchAcceleration * rightTangent * Time.deltaTime;
				
				if(Vector2.Dot (oldSpeed + delta, speed) < 0) {
					speed = Vector2.zero;
				} else {
					speed += delta;
				}
			} else {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
		}

		// walk left
		if (applyLeftWalk && walkLeft && (!isLeftMovement || oldSpeedNorm < maxWalkSpeed)) {
			if(!isLeftMovement) {
				Vector2 delta = switchAcceleration * leftTangent * Time.deltaTime;

				if(Vector2.Dot (oldSpeed + delta, speed) < 0) {
					speed = Vector2.zero;
				} else {
					speed += delta;
				}
			} else {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			}
		}

		// idle
		bool applyIdling = contactCount >= 1 && (leftGround || rightGround) && !walkLeft && !walkRight;
		if(applyIdling) {
			Vector2 delta = Vector2.zero;
			if(isLeftMovement) {
				delta = idleAcceleration * rightTangent * Time.deltaTime;
			} else {
				delta = idleAcceleration * leftTangent * Time.deltaTime;
			} 

			if(Vector2.Dot (delta + speed, oldSpeed) <= 0) {
				speed = Vector2.zero;
			} else {
				speed += delta;
			}
		}

		// nudge
		if (contactCount == 0) {
			if (ld && oldSpeed.x > -maxWalkSpeed) {
				speed += - nudge * Vector2.right * Time.deltaTime;
			} else if (rd && oldSpeed.x < maxWalkSpeed) {
				speed += nudge * Vector2.right * Time.deltaTime;
			}
		}

		/*if (contactCount >= 1) {
			jumping = false;
		}

		if (jumping && Input.GetKeyUp (KeyCode.Space) && Time.time - lastJump < maxJumpStop && speed.y > jumpSpeed / 2) {
			speed -= Vector2.up * jumpSpeed/3;
			jumping = false;
		}*/

		// jump
		if (jump && contactCount >= 1) {
			//lastJump = Time.time;
			//jumping = true;
			speed += jumpSpeed * (Vector2.Angle (leftNormal, Vector2.up) < Vector2.Angle (rightNormal, Vector2.up) ? 
			                      	leftNormal : rightNormal);
		} 

		// rocket influence
		if (Input.GetMouseButton (0) && fuel > 0 && oldSpeedNorm < maxWalkSpeed) {
			float force = Mathf.Min(minRocketForce[rocketLevel] +rocketForcePerUnit[rocketLevel]*Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position), maxRocketForce) * Time.deltaTime;
			force = Mathf.Min (fuel/fuelPerNewton, force);
			speed += (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized 
				* force ;

			fuel -= force * fuelPerNewton;
		}

		// stepping movement
		if (contactCount >= 1) {
			Vector2 normal = motor.getLeftNormal();
			if (Vector2.Dot (normal, speed) < 0)
				speed -= Vector2.Dot (speed, normal) * normal;
		}
		
		if (contactCount == 2) {
			Vector2 normal = motor.getRightNormal();
			if (Vector2.Dot (normal, speed) < 0)
				speed -= Vector2.Dot (speed, normal) * normal;
		}
		
		Vector2 movement = speed * Time.deltaTime;
		while (Vector2.Distance(movement, Vector2.zero) > 0.001f) {
			movement = motor.move (movement);

			if(motor.getContactCount() >= 1)
				speed -= Vector2.Dot(speed, motor.getLeftNormal()) * motor.getLeftNormal();
			if(motor.getContactCount() == 2) 
				speed -= Vector2.Dot(speed, motor.getRightNormal()) * motor.getRightNormal();

		}
	}

	public void kill() {
		Application.LoadLevel (Application.loadedLevel);
	}

	public float getFuel() {
		return fuel;
	}
	public float getMaxFuel() {
		return maxFuel;
	}
	public void setFuel(float f) {
		if (f > maxFuel) {
			fuel = f;
		} else {
			fuel = maxFuel;
		}
	}
	public void addFuel(float f) {
		fuel += f;
	}

	public int getRocketLevel() {
		return rocketLevel;
	}
	public void setRocketLevel(int l) {
		rocketLevel = l;
	}
	public void addRocket() {
		rocketLevel ++;
		if (rocketLevel >= rocketForcePerUnit.Length) {
			rocketLevel = rocketForcePerUnit.Length -1;
		}
	}
	public bool isMaxLevel() {
		return rocketLevel == rocketForcePerUnit.Length - 1;
	}
}
