using UnityEngine;
using System.Collections;

public class TightRocketController : MonoBehaviour {
	
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
	float leftSpeed;
	float rightSpeed;
	Direction direction;

	public enum Direction {
		left, right, free
	}
	
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

	/* nudge fails sometimes
	 * falls from the ceiling when running on the wall
	 * add gravity in wall
	 * add drag on wall
	 * add rocket 
	 * add ceiling fall off */

	void movement() {
		// Gathering information :
		int contactCount = motor.getContactCount ();
		Vector2 leftTangent = Vector2.zero;
		Vector2 rightTangent = Vector2.zero;
		Vector2 leftNormal = Vector2.zero;
		Vector2 rightNormal = Vector2.zero;
		Vector2 oldSpeed = speed;
		float oldSpeedNorm = 0;
		switch (direction) {
		case Direction.free : oldSpeedNorm = Vector2.Distance (speed, Vector2.zero);break;
		case Direction.left : oldSpeedNorm = leftSpeed;break;
		case Direction.right : oldSpeedNorm = rightSpeed;break;
		}
		
		if (contactCount >= 1) {
			leftTangent = motor.getLeftTangent ().normalized;
			rightTangent = motor.getRightTangent ().normalized;
			leftNormal = motor.getLeftNormal ().normalized;
			rightNormal = motor.getRightNormal ().normalized;
		}
		bool isLeftMovement = direction == Direction.left; 
		
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
			&& (Vector2.Dot(rightTangent, Vector2.up) < 0 || !isLeftMovement);
		acceptRightWalk |= contactCount >= 1 && rightCeiling && stick && oldSpeedNorm > 0.1f;
		
		bool acceptLeftWalk = contactCount >= 1 && leftGround;
		acceptLeftWalk |= contactCount >= 1 && leftWall && stick 
			&& (Vector2.Dot(leftTangent, Vector2.up) < 0 || isLeftMovement);
		acceptLeftWalk |= contactCount >= 1 && leftCeiling && stick && oldSpeedNorm > 0.1f;
		
		// nudge condition :
		bool acceptLeftNudge = contactCount == 0;
		acceptLeftNudge |= contactCount == 1 && !stick && (leftWall || leftCeiling) && !leftGround && !rightGround 
			&& Vector2.Dot (leftNormal, -Vector2.right) > 0;
		acceptLeftNudge |= contactCount == 2 && !stick && (leftWall || leftCeiling) && !leftGround && !rightGround
			&& Vector2.Dot (leftNormal, -Vector2.right) > 0 
				&& Vector2.Dot (rightNormal, -Vector2.right) > 0;
		
		bool acceptRightNudge = contactCount == 0;
		acceptRightNudge |= contactCount == 1 && !stick && (leftWall || leftCeiling) && !leftGround && !rightGround 
			&& Vector2.Dot (leftNormal, Vector2.right) > 0;
		acceptRightNudge |= contactCount == 2 && !stick && (leftWall || leftCeiling) && !leftGround && !rightGround 
			&& Vector2.Dot (leftNormal, Vector2.right) > 0 
				&& Vector2.Dot (rightNormal, Vector2.right) > 0;
		
		bool acceptIdling = contactCount >= 1 && (leftGround || rightGround) && !goLeft && !goRight;
		
		// gravity
		bool applyGrav = contactCount == 0;
		bool applyWallGrav = contactCount >= 1 && (leftWall || leftCeiling) && !leftGround && !rightGround;

		// jump
		if (contactCount >= 1 && Time.time - lastJump > maxJumpStop) {
			jumping = false;
		}
		if (jumping && Input.GetKeyUp (KeyCode.Space) && Time.time - lastJump < maxJumpStop && speed.y > 0) {
			speed.y /= (1 + speed.y/jumpSpeed);
			jumping = false;
		}

		if (contactCount == 0) {
			direction = Direction.free;
			speed += gravity * Time.deltaTime;
			if (ld) {
				speed += - nudge * Vector2.right * Time.deltaTime;
			} 
			if (rd) {
				speed += nudge * Vector2.right * Time.deltaTime;
			}
		} else {
			if(jump) {
				if(leftGround || rightGround) {
					speed = jumpSpeed * Vector2.up;
					jumping = true;
					lastJump = Time.time;
				} else if(leftWall || leftCeiling) {
					speed = jumpSpeed * leftNormal;
				} 
				switch(direction) {
				case Direction.free : break;
				case Direction.left : speed += leftTangent * leftSpeed; break;
				case Direction.right : speed += rightTangent * rightSpeed; break;
				}

				direction = Direction.free;
			} else if(Vector2.Angle(leftNormal,-Vector2.up) < 80 
			          && (acceptRightWalk && goRight && isLeftMovement
			          	  || acceptLeftWalk && goLeft && !isLeftMovement
			          	  || oldSpeedNorm < ceilingDropOffSpeed 
			    		  || !stick)) {
				speed = -Vector2.up * 0.5f;

				switch(direction) {
				case Direction.free : break;
				case Direction.left : speed += leftTangent * leftSpeed; break;
				case Direction.right : speed += rightTangent * rightSpeed; break;
				}
				
				direction = Direction.free;
			} else {
				if(acceptLeftNudge && ld) {
					speed = - (nudge * Time.deltaTime + 0.5f) * Vector2.right ;
					switch(direction) {
					case Direction.free : break;
					case Direction.left : speed += leftTangent * leftSpeed; break;
					case Direction.right : speed += rightTangent * rightSpeed; break;
					}
					direction = Direction.free;
				} else if(acceptRightNudge && rd) {
					speed = (nudge * Time.deltaTime + 0.5f) * Vector2.right;
					switch(direction) {
					case Direction.free : break;
					case Direction.left : speed += leftTangent * leftSpeed; break;
					case Direction.right : speed += rightTangent * rightSpeed; break;
					}
					direction = Direction.free;
				} else {
					if(acceptRightWalk && goRight) {
						if(direction == Direction.left) {
							if(leftSpeed > 0.01f) {
								float delta = switchAcceleration * Time.deltaTime;
								
								if(leftSpeed - delta > 0) {
									leftSpeed -= delta;
								} else {
									leftSpeed = 0;
									rightSpeed = 0;
									direction = Direction.right;
								}
							} else {
								direction = Direction.right;
								rightSpeed = walkAcceleration * Time.deltaTime;
							}
						} else if(rightSpeed < maxWalkSpeed) {
							direction = Direction.right;
							rightSpeed += walkAcceleration * Time.deltaTime;
						}
					} else if(acceptLeftWalk && goLeft) {
						if(direction == Direction.right) {
							if(rightSpeed > 0.01f) {
								float delta = switchAcceleration * Time.deltaTime;
								
								if(rightSpeed - delta > 0) {
									rightSpeed -= delta;
								} else {
									rightSpeed = 0;
									leftSpeed = 0;
									direction = Direction.left;
								}
							} else {
								direction = Direction.left;
								leftSpeed = walkAcceleration * Time.deltaTime;
							}
						} else if(leftSpeed < maxWalkSpeed) {
							direction = Direction.left;
							leftSpeed += walkAcceleration * Time.deltaTime;
						}
					} else if(acceptIdling) {
						if(direction == Direction.right) {
							float delta = idleAcceleration * Time.deltaTime;

							if(rightSpeed - delta > 0) {
								rightSpeed -= delta;
							} else {
								rightSpeed = 0;
							}	
						} else {
							float delta = idleAcceleration * Time.deltaTime;
							
							if(leftSpeed - delta > 0) {
								leftSpeed -= delta;
							} else {
								leftSpeed = 0;
							}
						}
					}

					if(applyWallGrav) {
						if(direction == Direction.right) {
							if(Vector2.Dot(Vector2.up, rightTangent) > 0) {
								rightSpeed += Vector2.Dot(gravity, rightTangent) * wallGravMultiplier * Time.deltaTime;
								if(rightSpeed < 0) {
									leftSpeed = - rightSpeed;
									direction = Direction.left;
								}
							} else {
								rightSpeed += Vector2.Dot(gravity, rightTangent) * wallGravMultiplier * Time.deltaTime;
							}
						} else {
							if(Vector2.Dot(Vector2.up, leftTangent) > 0) {
								leftSpeed += Vector2.Dot(gravity, leftTangent) * wallGravMultiplier * Time.deltaTime;
								if(leftSpeed < 0) {
									rightSpeed = - leftSpeed;
									direction = Direction.right;
								}
							} else {
								leftSpeed += Vector2.Dot(gravity, leftTangent) * wallGravMultiplier * Time.deltaTime;
							}
						}
					}
					if(!(acceptRightWalk && goRight) && !(acceptLeftWalk && goLeft) && leftWall) {
						if(direction == Direction.right) {
							if(Vector2.Dot (-Vector2.up, rightTangent) > 0) {
								rightSpeed -= wallDrag * oldSpeedNorm * Time.deltaTime;
								if(rightSpeed < 0) {
									rightSpeed = 0;
								}
							}
						} else {
							if(Vector2.Dot (-Vector2.up, leftTangent) > 0) {
								leftSpeed -= wallDrag * oldSpeedNorm * Time.deltaTime;
								if(leftSpeed < 0) {
									leftSpeed = 0;
								}
							}
						}
					}
				}
			}
		}

		if (Input.GetMouseButton (0) && fuel > 0) {
			Vector2 rocketDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
			float distance = Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position);
			float strength = minRocketForce[rocketLevel] + (maxRocketForce[rocketLevel] - minRocketForce[rocketLevel]) * distance/rocketRadius;
			
			float force = Mathf.Min(strength, maxRocketForce[rocketLevel]) * Time.deltaTime;
			force = Mathf.Min (fuel/fuelPerForce, force);
			float projectedSpeed = Vector2.Dot(oldSpeed, rocketDirection.normalized);
			if(projectedSpeed < terminalRocketSpeed) {
				switch(direction) {
				case Direction.free : 
					speed += rocketDirection.normalized * force;
					break;
				case Direction.right :
					if(Vector2.Dot (rightTangent, rocketDirection) > 0 && Vector2.Dot (rightNormal, rocketDirection) < 0.1f) {
						rightSpeed += force;
					} else if(Vector2.Dot (leftTangent, rocketDirection) > 0 && Vector2.Dot (leftNormal, rocketDirection) < 0.1f) {
						rightSpeed -= force;
						if(rightSpeed < 0) {
							leftSpeed = - rightSpeed;
							direction = Direction.left;
						}
					} else {
						speed = rightSpeed * rightTangent + rocketDirection.normalized * force;
						direction = Direction.free;
					}
					break;
				case Direction.left :
					if(Vector2.Dot (rightTangent, rocketDirection) > 0 && Vector2.Dot (rightNormal, rocketDirection) < 0.1f) {
						leftSpeed -= force;
						if(leftSpeed < 0) {
							rightSpeed = - leftSpeed;
							direction = Direction.right;
						}
					} else if(Vector2.Dot (leftTangent, rocketDirection) > 0 && Vector2.Dot (leftNormal, rocketDirection) < 0.1f) {
						leftSpeed += force;
					} else {
						speed = leftSpeed * leftTangent + rocketDirection.normalized * force;
						direction = Direction.free;
					}
					break;
				}
				speed += rocketDirection.normalized * force ;
			} 
			fuel -= force * fuelPerForce;
		}

		if (direction == Direction.free && contactCount >= 1) {
			if(Vector2.Dot (speed, leftNormal) <= 0.005f && Vector2.Dot (speed, leftTangent) > 0) {
				direction = Direction.left;
				leftSpeed = Vector2.Dot (speed, leftTangent);
			} else if(Vector2.Dot (speed, rightNormal) <= 0.005f && Vector2.Dot (speed, rightTangent) > 0) {
				direction = Direction.right;
				rightSpeed = Vector2.Dot (speed, rightTangent);
			}
		}

		Vector2 freeMovement = speed * Time.deltaTime;
		float leftMovement = leftSpeed * Time.deltaTime;
		float rightMovement = rightSpeed * Time.deltaTime;
		Vector2 tmp = Vector2.zero;

		int a = 0;
		while(((contactCount == 0 && Vector2.Distance(speed, Vector2.zero) > 0.001f)
		       || (contactCount >= 1 && ((direction == Direction.left && leftMovement > 0.001f)
		                          	 || (direction == Direction.right && rightMovement > 0.001f)
		                          	 || (direction == Direction.free && Vector2.Distance(speed, Vector2.zero) > 0.001f)))) && a < 10) {

			Vector2 oldLeftTangent = leftTangent;
			Vector2 oldRightTangent = rightTangent;
			
			Vector2 oldLeftNormal = leftNormal;
			Vector2 oldRightNormal = rightNormal;

			rightGround = contactCount >= 1 && Vector2.Angle(rightNormal, Vector2.up) < groundAngle;
			leftGround = contactCount >= 1 && Vector2.Angle(leftNormal, Vector2.up) < groundAngle;

			if(contactCount == 0) {
				tmp = motor.move(freeMovement);
			}
			if(contactCount >= 1) {
				if(direction == Direction.left) {
					if(motor.isLeftContactEdge ()) {
						tmp = motor.move (leftMovement * leftTangent);
					} else {
						if(contactCount < 2 || leftMovement > 0.001f)
							// when there are 2 contact, we want to avoid moving very little from that corner
							tmp = motor.leftLineMove(leftMovement);
					}
				} else if(direction == Direction.right) {
					if(motor.isRightContactEdge ()) {
						tmp = motor.move (rightMovement * rightTangent);
					} else {
						if(contactCount < 2 || rightMovement > 0.001f)
							tmp = motor.rightLineMove(rightMovement);
					}
				} else {
					tmp = motor.move (freeMovement);
				}
			}

			contactCount = motor.getContactCount();
			if(contactCount == 0) {
				switch(direction) {
				case Direction.free : break;
				case Direction.left : speed = leftSpeed * leftTangent; break;
				case Direction.right : speed = rightSpeed * rightTangent; break;
				}
				freeMovement = Vector2.zero;
				direction = Direction.free;
			} else {
				leftTangent = motor.getLeftTangent();
				rightTangent = motor.getRightTangent();
				
				leftNormal = motor.getLeftNormal();
				rightNormal = motor.getRightNormal();

				if(tmp != Vector2.zero) {
					if(Vector2.Angle (tmp, motor.getLeftTangent()) < Vector2.Angle (tmp, motor.getRightTangent())) {
						if(contactCount < 2 
						   || (Vector2.Angle(leftTangent, rightTangent) > 110 
						    	&& (stick 
						    		|| Vector2.Angle (leftNormal,Vector2.up) < groundAngle 
						    		|| !rightGround))
						   || Vector2.Angle(leftNormal, Vector2.up) < groundAngle) {
							switch(direction) {
							case Direction.free : leftSpeed = Vector2.Dot (speed, leftTangent);break;
							case Direction.left : leftSpeed = Vector2.Dot (oldLeftTangent, leftTangent) * leftSpeed; break;
							case Direction.right : leftSpeed = Vector2.Dot (rightSpeed * oldRightTangent, leftTangent); break;
							}
							leftMovement = Vector2.Dot (motor.getLeftTangent(), tmp);
							direction = Direction.left;
						} else {
							leftSpeed = 0;
							leftMovement = 0;
							direction = Direction.left;
						}
					} else {
						if(contactCount < 2 
						   || (Vector2.Angle(leftTangent, rightTangent) > 110 
						    	&& (stick || Vector2.Angle (rightNormal,Vector2.up) < groundAngle
						    			  || !leftGround))
						   || Vector2.Angle(rightNormal, Vector2.up) < groundAngle) {
							switch(direction) {
							case Direction.free : rightSpeed = Vector2.Dot (speed, rightTangent); break;
							case Direction.left : rightSpeed = Vector2.Dot (leftSpeed * oldLeftTangent, leftTangent) * rightSpeed; break;
							case Direction.right : rightSpeed = Vector2.Dot (oldRightTangent, rightTangent) * rightSpeed; break;
							}
							rightMovement = Vector2.Dot (motor.getRightTangent(), tmp);
							direction = Direction.right;
						} else {
							rightSpeed = 0;
							rightMovement = 0;
							direction = Direction.right;
						}
					} 
				} else {
					switch(direction) {
					case Direction.free :
						if(Vector2.Angle(speed, motor.getLeftTangent ()) < Vector2.Angle(speed, motor.getRightTangent())) {
							direction = Direction.left;
							leftSpeed = Vector2.Dot (speed, motor.getLeftTangent ());
						} else {
							direction = Direction.right;
							rightSpeed = Vector2.Dot (speed, motor.getRightTangent ());
						}
						break;
					case Direction.left : leftMovement = 0; break;
					case Direction.right : rightMovement = 0; break;
					}
				}
			}
			a++;
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
