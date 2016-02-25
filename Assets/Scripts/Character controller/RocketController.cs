using UnityEngine;
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
	public float wallDrag;
	public float wallGravMultiplier;

	// Rocket
	public float[] minRocketForce;
	public float[] maxRocketForce;

	public float minRocketSpeed;
	public float maxRocketSpeed;

	public float rocketRadius;

	public float maxFuel;
	public float fuelPerForce;
	public float terminalRocketSpeed;

	CharacterMotor2D motor;
	
	public int rocketLevel;
	public float fuel;
	
	Vector2 speed;

	bool jumping;
	float lastJump;

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
			Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
			float distance = Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position);
			float strength = minRocketSpeed + (maxRocketSpeed - minRocketSpeed) * distance/rocketRadius;
			rocket.GetComponent<Rigidbody2D>().velocity = direction.normalized * Mathf.Min(strength, maxRocketSpeed);
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
			leftTangent = motor.getLeftTangent ().normalized;
			rightTangent = motor.getRightTangent ().normalized;
			leftNormal = motor.getLeftNormal ().normalized;
			rightNormal = motor.getRightNormal ().normalized;
		}
		bool isLeftMovement = Vector2.Angle (speed, leftTangent) < Vector2.Angle (speed, rightTangent);

		bool leftGround = Vector2.Angle (Vector2.up, leftNormal) < groundAngle;
		bool rightGround = Vector2.Angle (Vector2.up, rightNormal) < groundAngle;
		bool leftWall = Vector2.Angle (Vector2.up, leftNormal) < 91 && !leftGround;
		bool rightWall = Vector2.Angle (Vector2.up, rightNormal) < 91 && !rightGround;
		bool leftCeiling = !leftGround && !leftWall;
		bool rightCeiling = !rightGround && !rightWall;

		// Movement choices : 
		bool ld = Input.GetKey (KeyCode.Q) && !Input.GetKey (KeyCode.D);
		bool rd = Input.GetKey (KeyCode.D) && !Input.GetKey (KeyCode.Q);
		bool ud = Input.GetKey (KeyCode.Z) && !Input.GetKey (KeyCode.S);
		bool bd = Input.GetKey (KeyCode.S) && !Input.GetKey (KeyCode.Z);
		bool stick = Input.GetKey (KeyCode.LeftShift);
		bool jump = Input.GetKeyDown (KeyCode.Space);

		// walk order :
		bool goRight = (Vector2.Angle (Vector2.up, rightNormal) < 80 && rd);
		goRight |= (Vector2.Angle (-Vector2.up, rightNormal) < 80 && ld);
		goRight |= (Vector2.Angle (Vector2.right, rightNormal) < 80 && bd);
		goRight |= (Vector2.Angle (-Vector2.right, rightNormal) < 80 && ud);
		
		bool goLeft = (Vector2.Angle (Vector2.up, leftNormal) < 80 && ld);
		goLeft |= (Vector2.Angle (-Vector2.up, leftNormal) < 80 && rd);
		goLeft |= (Vector2.Angle (Vector2.right, leftNormal) < 80 && ud);
		goLeft |= (Vector2.Angle (-Vector2.right, leftNormal) < 80 && bd);

		// nudge order :
		bool nudgeLeft = ld && !rd;
		bool nudgeRight = rd && !ld;

		// walk condition :
		bool acceptRightWalk = contactCount >= 1 && rightGround;
		acceptRightWalk |= contactCount == 1 && rightWall && stick
							&& (Vector2.Dot(rightTangent, Vector2.up) < 0 || !isLeftMovement)
							&& oldSpeedNorm > 0.1f;
		acceptRightWalk |= contactCount >= 1 && rightCeiling && stick && oldSpeedNorm > 0.1f;
		
		bool acceptLeftWalk = contactCount >= 1 && leftGround;
		acceptLeftWalk |= contactCount >= 1 && leftWall && stick 
							&& (Vector2.Dot(leftTangent, Vector2.up) < 0 || isLeftMovement)
							&& oldSpeedNorm > 0.1f;
		acceptLeftWalk |= contactCount >= 1 && leftCeiling && stick && oldSpeedNorm > 0.1f;

		// nudge condition :
		bool acceptLeftNudge = contactCount == 0;
		acceptLeftNudge |= contactCount == 1 && !stick && leftWall && !leftGround && !rightGround 
											 && Vector2.Dot (leftNormal, -Vector2.right) > 0;
		acceptLeftNudge |= contactCount == 2 && !stick && leftWall && !leftGround && !rightGround
											 && Vector2.Dot (leftNormal, -Vector2.right) > 0 
											 && Vector2.Dot (rightNormal, -Vector2.right) > 0;

		bool acceptRightNudge = contactCount == 0;
		acceptRightNudge |= contactCount == 1 && !stick && leftWall && !leftGround && !rightGround 
											  && Vector2.Dot (leftNormal, Vector2.right) > 0;
		acceptRightNudge |= contactCount == 2 && !stick && leftWall && !leftGround && !rightGround 
											  && Vector2.Dot (leftNormal, Vector2.right) > 0 
											  && Vector2.Dot (rightNormal, Vector2.right) > 0;

		bool acceptIdling = contactCount >= 1 && (leftGround || rightGround) && !goLeft && !goRight;

		// gravity
		bool applyGrav = contactCount == 0;
		bool applyWallGrav = contactCount >= 1 && (leftWall || leftCeiling) && !leftGround;

		// idle
		if(acceptIdling) {
			Debug.Log ("idling");
			Vector2 delta = Vector2.zero;
			if(isLeftMovement) {
				Debug.Log ("idling right");
				delta = idleAcceleration * rightTangent * Time.deltaTime;
			} else {
				Debug.Log("idling left");
				delta = idleAcceleration * leftTangent * Time.deltaTime;
			} 
			
			if(Vector2.Dot (delta + speed, oldSpeed) <= 0) {
				speed = Vector2.zero;
			} else {
				speed += delta;
			}
		}

		if (applyGrav) {
			Debug.Log ("gravity");
			speed += gravity * Time.deltaTime;
		} 

		if (applyWallGrav) {
			Debug.Log ("wall gravity");
			if(Vector2.Dot (leftTangent, -Vector2.up) > 0) {
				speed += Vector2.Dot(gravity * wallGravMultiplier, leftTangent) * leftTangent * Time.deltaTime;
			} else if(Vector2.Dot (rightTangent, -Vector2.up) > 0) {
				speed += Vector2.Dot(gravity * wallGravMultiplier, rightTangent) * rightTangent * Time.deltaTime;
			}
		}

		if (applyWallGrav && !goLeft && !goRight) {
			Debug.Log ("wall drag");
			speed += - wallDrag * speed * Time.deltaTime;
		}

		// nudge
		if (nudgeLeft && acceptLeftNudge) {
			Debug.Log ("nudge left");
			speed += - (nudge * Time.deltaTime) * Vector2.right;
		}
		if (nudgeRight && acceptRightNudge) {
			Debug.Log ("nudge right");
			speed += (nudge * Time.deltaTime) * Vector2.right;
		}

		// walk right
		if (goRight && acceptRightWalk) {
			Debug.Log ("go right");
			if(isLeftMovement) {
				Vector2 delta = switchAcceleration * rightTangent * Time.deltaTime;
				
				if(Vector2.Dot (oldSpeed + delta, speed) < 0) {
					speed -= Vector2.Dot (speed, rightTangent) * rightTangent;
				} else {
					speed += delta;
				}
			} else if(oldSpeedNorm < maxWalkSpeed) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
		}

		// walk left
		if (goLeft && acceptLeftWalk) {
			Debug.Log ("go left");
			if(!isLeftMovement) {
				Vector2 delta = switchAcceleration * leftTangent * Time.deltaTime;

				if(Vector2.Dot (oldSpeed + delta, speed) < 0) {
					speed -= Vector2.Dot (speed, leftTangent) * leftTangent;
				} else if(oldSpeedNorm < maxWalkSpeed) {
					speed += delta;
				}
			} else if(oldSpeedNorm < maxWalkSpeed){
				speed += walkAcceleration * leftTangent.normalized * Time.deltaTime;
			}
		}

		// jump
		if (contactCount >= 1) {
			jumping = false;
		}
		if (jumping && Input.GetKeyUp (KeyCode.Space) && Time.time - lastJump < maxJumpStop && speed.y > jumpSpeed / 2) {
			speed -= Vector2.up * jumpSpeed/2;
			jumping = false;
		}

		if (jump) {
			if(contactCount >= 1) {
				if(leftGround || rightGround) {
					lastJump = Time.time;
					jumping = true;
					speed += jumpSpeed * Vector2.up;
				} else if(leftWall && !(goRight && acceptRightWalk || goLeft && acceptLeftWalk)) {
				    if(Vector2.Dot(leftNormal, Vector2.right) > 0) {
						speed = jumpSpeed * (Vector2.up*2 + Vector2.right).normalized;
					} else {
						speed = jumpSpeed * (Vector2.up*2 - Vector2.right).normalized;
					}
				} else {
					speed += jumpSpeed * (Vector2.Angle (leftNormal, Vector2.up) < Vector2.Angle (rightNormal, Vector2.up) ? 
				                      	leftNormal : rightNormal);
				}
			} 
		} 

		// rocket influence
		if (Input.GetMouseButton (0) && fuel > 0) {
			Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
			float distance = Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position);
			float strength = minRocketForce[rocketLevel] + (maxRocketForce[rocketLevel] - minRocketForce[rocketLevel]) * distance/rocketRadius;

			float force = Mathf.Min(strength, maxRocketForce[rocketLevel]) * Time.deltaTime;
			force = Mathf.Min (fuel/fuelPerForce, force);
			float projectedSpeed = Vector2.Dot(oldSpeed, direction.normalized);
			if(projectedSpeed < terminalRocketSpeed) {
				speed += direction.normalized * force ;
			} 
			fuel -= force * fuelPerForce;
		}

		//Debug.Log ("1 speed : " + speed.x + " " + speed.y);

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

		if (nudgeLeft && acceptLeftNudge && (leftWall || leftCeiling)) {
			motor.move (-0.1f * Time.deltaTime * Vector2.right);
		}
		if (nudgeRight && acceptRightNudge && (leftWall || leftCeiling)) {
			motor.move (0.1f * Time.deltaTime * Vector2.right);
		}
		if (leftCeiling && (!stick || (isLeftMovement && goRight) 
		                    	   || (!isLeftMovement && goLeft) 
		    					   || oldSpeedNorm < ceilingDropOffSpeed)) {
			motor.move (-0.1f * Time.deltaTime * Vector2.up);
		}

		Vector2 movement = speed * Time.deltaTime;
		//Debug.Log ("2 speed : " + speed.x + " " + speed.y + " " + (Vector2.Distance(movement, Vector2.zero) > 0.001f));

		int a = 0;
		while (Vector2.Distance(movement, Vector2.zero) > 0.001f && a < 10) {
			a++;
			movement = motor.move (movement);

			if (motor.getContactCount () >= 1)
				speed -= Vector2.Dot (speed, motor.getLeftNormal ()) * motor.getLeftNormal ();
			if (motor.getContactCount () == 2) { 
				if (Vector2.Angle (motor.getLeftTangent (), motor.getRightTangent ()) < 95) {
					speed = Vector2.zero;
				} else {
					speed -= Vector2.Dot (speed, motor.getRightNormal ()) * motor.getRightNormal ();
				}
			}
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
		if (rocketLevel >= minRocketForce.Length) {
			rocketLevel = minRocketForce.Length -1;
		}
	}
	public bool isMaxLevel() {
		return rocketLevel == minRocketForce.Length - 1;
	}
}
