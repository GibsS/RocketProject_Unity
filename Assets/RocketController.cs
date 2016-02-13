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

	// Angle
	public float groundAngle;

	// External force
	public Vector2 gravity;
	public float airDrag;
	public float groundDrag;

	// Rocket
	public float rocketSpeedPerUnit;
	public float[] rocketForcePerUnit;
	public float maxSpeed;
	public float maxForce;

	public float maxFuel;
	public float fuelPerNewton;
	
	CharacterMotor2D motor;
	
	int rocketLevel;
	float fuel;
	
	Vector2 speed;

	void Start () {
		motor = GetComponent<CharacterMotor2D> ();
	}
	
	void Update () {
		movement ();

		shooting ();
	}

	void OnTriggerEnter2D(Collider2D coll) {
		Debug.Log ("test");
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
					* Mathf.Max(rocketSpeedPerUnit*Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position), maxSpeed);
			rocket.transform.position = transform.position;
		}
	}

	void movement() {
		int contactCount = motor.getContactCount ();
		Vector2 leftTangent = Vector2.zero;
		Vector2 rightTangent = Vector2.zero;
		Vector2 leftNormal = Vector2.zero;
		Vector2 rightNormal = Vector2.zero;
		Vector2 oldSpeed = speed;
		float oldVelocity = Vector2.Distance (speed, Vector2.zero);
		if (contactCount >= 1) {
			leftTangent = motor.getLeftTangent ();
			rightTangent = motor.getRightTangent ();
			leftNormal = motor.getLeftNormal ();
			rightNormal = motor.getRightNormal ();
		}
		bool isLeftMovement = Vector2.Angle (speed, leftTangent) < Vector2.Angle (speed, rightTangent);
		
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
		
		bool ld = Input.GetKey (KeyCode.Q) && !Input.GetKey (KeyCode.D);
		bool rd = Input.GetKey (KeyCode.D) && !Input.GetKey (KeyCode.Q);
		bool ud = Input.GetKey (KeyCode.Z) && !Input.GetKey (KeyCode.S);
		bool bd = Input.GetKey (KeyCode.S) && !Input.GetKey (KeyCode.Z);
		bool jump = Input.GetKeyDown (KeyCode.Space);
		
		/*Debug.Log (Vector2.Angle (speed, leftTangent) + " " + Vector2.Angle (speed, rightTangent)
		           + " " + (Vector2.Angle (speed, leftTangent) < Vector2.Angle (speed, rightTangent))
		           + " " + isLeftMovement);
		Debug.DrawRay (transform.position, rightTangent, Color.green);
		Debug.DrawRay (transform.position, leftTangent, Color.red);
		Debug.DrawRay (transform.position, speed.normalized, Color.black);*/
		
		if (positionState == 0) { // free
			Vector2 drag = - speed * airDrag * Time.deltaTime;
			if(ld) {
				speed += - nudge * Vector2.right * Time.deltaTime;
			} else if(rd) {
				speed += nudge * Vector2.right * Time.deltaTime;
			}
			speed += gravity * Time.deltaTime;
			speed += drag;
		} else if (positionState == 1) { // grounded
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld) {
				if(isLeftMovement) {
					speed += walkAcceleration * leftTangent * Time.deltaTime;
				} else {
					speed += switchAcceleration * leftTangent * Time.deltaTime;
				}
			} else if(rd) {
				if(isLeftMovement) {
					speed += switchAcceleration * rightTangent * Time.deltaTime;
				} else {
					speed += walkAcceleration * rightTangent * Time.deltaTime;
				}
			} else if(oldVelocity > zeroSpeed) {
				if(isLeftMovement) {
					speed += idleAcceleration * rightTangent * Time.deltaTime;
				} else {
					speed += idleAcceleration * leftTangent * Time.deltaTime;
				} 
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += drag;
			
			if(!ld && !rd && Vector2.Dot(speed, oldSpeed) < 0) {
				speed = Vector2.zero;
			}
		} else if (positionState == 2) { // between ground and left wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			} else if(rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += drag;
		} else if (positionState == 3) { // between ground and right wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			} else if(rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += drag;
		} else if (positionState == 4) { // left wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			} else if(rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += gravity * Time.deltaTime;
			speed += drag;
		} else if (positionState == 5) { // right wall
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if(ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			} else if( rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
			speed += gravity * Time.deltaTime;
			speed += drag;
		} else if (positionState == 6) { // between left wall and ceiling
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if( ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			} else if( rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * rightNormal;
			}
			speed += drag;
		} else if (positionState == 7) { // between right wall and ceiling
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if( ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			} else if( rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * leftNormal;
			}
			speed += drag;
		} else if (positionState == 8) { // between ground and left ceiling
			if( rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * Vector2.up;
			}
		} else if (positionState == 9) { // 9 : between ground and right ceiling
			if( ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			}
			
			if (jump) {
				speed += jumpSpeed * leftNormal;
			}
		} else if (positionState == 10) { // ceiling
			Vector2 drag = - speed * groundDrag * Time.deltaTime;
			if( ld) {
				speed += walkAcceleration * leftTangent * Time.deltaTime;
			} else if( rd) {
				speed += walkAcceleration * rightTangent * Time.deltaTime;
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

		if (Input.GetMouseButton (0) && fuel > 0) {
			float force = Mathf.Max(rocketForcePerUnit[rocketLevel]*Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position), maxForce) * Time.deltaTime;
			force = Mathf.Min (fuel/fuelPerNewton, force);
			speed += (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized 
				* force ;

			fuel -= force * fuelPerNewton;
		}
		
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
