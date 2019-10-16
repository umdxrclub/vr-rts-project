using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub
// Date: March 2019

public class Unit : MonoBehaviour {

	protected float speed;
	protected float maxHealth;
	
	public PlayerScript owner;
	public GameObject selectionCircle;
	protected CharacterController cc;
	protected bool moving;
	protected Vector3 targetPos;
	protected float targetAngle;
	protected LineRenderer line;
	protected float health;
	
    void Start() {

		// Default stats for units
		speed = 1f;
		maxHealth = 100f;

		setUpUnit();

		// Start the shooting cycle
		InvokeRepeating("fireAtClosestEnemy", 0f, 1f);
    }

    void Update() {


        if (moving) {

			// Perform the smooth movement and rotation
			Vector2 movementVector = new Vector2(targetPos.x - transform.position.x, targetPos.z - transform.position.z);
			cc.Move((Vector3.ClampMagnitude(new Vector3(movementVector.x, 0, movementVector.y), 1) + Vector3.down)
					* speed *  Time.deltaTime);
			transform.localEulerAngles =
					new Vector3(0, Mathf.LerpAngle(transform.localEulerAngles.y, targetAngle, Mathf.Min(10*Time.deltaTime, 0.2f)), 0);

			// Maintain the path line
			line.SetPositions(new Vector3[] { transform.position + 0.05f*Vector3.up , targetPos + 0.05f*Vector3.up });

			// If close enough, end the movement
			if (movementVector.SqrMagnitude() < 0.2f) {
				moving = false;
				Destroy(line.gameObject);
			}
		} else if (!cc.isGrounded) {
			cc.Move(Vector3.down * speed *  Time.deltaTime);
		}
    }

	// Called on start by every unit
	protected void setUpUnit() {
		cc = GetComponent<CharacterController>();
		selectionCircle = transform.Find("SelectionCircle").gameObject;
		moving = false;
		targetPos = Vector3.zero;
		targetAngle = 0f;
		health = maxHealth;

		//InvokeRepeating("fireAtClosestEnemy", 1, 1);
	}

	// Function called externally to tell the unit where to go
	public void moveTo(Vector3 position) {

		moving = true;
		targetPos = position;
		targetAngle = Mathf.Rad2Deg*Mathf.Atan2(targetPos.x-transform.position.x, targetPos.z-transform.position.z);
		if (line != null && line.gameObject != null) {
			Destroy(line.gameObject);
		}
		GameObject lineObj = new GameObject();
		lineObj.AddComponent<LineRenderer>();
		lineObj.GetComponent<Renderer>().material = selectionCircle.GetComponent<Renderer>().material;
		line = lineObj.GetComponent<LineRenderer>();
		line.generateLightingData = true;
		line.startWidth = 0.01f;
		line.endWidth = 0.01f;
	}

	// Function to fire at the closest enemy if in range
	public void fireAtClosestEnemy() {
		
		Unit[] enemies = (Unit[])GameObject.FindObjectsOfType(typeof(Unit));
		
		if (enemies.Length > 0) {
			Unit nearestEnemy = enemies[0];
			float bestDistSqr = (transform.position - enemies[0].transform.position).sqrMagnitude;

			// Find the nearest enemy
			foreach (Unit enemy in enemies) {
				float thisDistSqr = (transform.position - enemy.transform.position).sqrMagnitude;
				if (thisDistSqr < bestDistSqr) {
					nearestEnemy = enemy;
					bestDistSqr = thisDistSqr;
				}
			}

			// Fire at the enemy
			nearestEnemy.health -= 10f;
		}
	}

	void OnDestroy() {
		owner.removeOwnedUnit(this);
	}
}
