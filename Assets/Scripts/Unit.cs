using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Damageable {

	public float speed = 1f;
	public float damage = 10f;
	public float fireInterval = 1f;

	public GameObject[] coloredParts;
	public Material red, blue;

	[HideInInspector]
	public bool onAuto = false;
	[HideInInspector]
	public GameObject selectionCircle;

	protected CharacterController cc;
	protected bool moving = false;
	protected Vector3 targetPos = Vector3.zero;
	protected float targetAngle = 0f;
	protected LineRenderer line;
	
    void Start() {

		cc = GetComponent<CharacterController>();

		// Create the selection circle
		selectionCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		selectionCircle.transform.SetParent(transform);
		selectionCircle.transform.localPosition = Vector3.zero;
		selectionCircle.transform.localScale = new Vector3(2.5f * cc.radius, 0.1f, 2.5f * cc.radius);
		Destroy(selectionCircle.GetComponent<CapsuleCollider>());
		selectionCircle.GetComponent<MeshRenderer>().enabled = false;

		// Set health to max
		health = maxHealth;

		// Set the colored parts to the owner's color
		Material newColor = (owner == null ? red : blue);
		selectionCircle.GetComponent<Renderer>().material = newColor;
		foreach(GameObject coloredPart in coloredParts) {
			coloredPart.GetComponent<Renderer>().material = newColor;
		}

		if (damage > 0) {
			InvokeRepeating("fireAtClosestEnemyInRange", 1, fireInterval);
		}
    }

	void FixedUpdate() {

		if (onAuto) {

			// Find the closest enemy
			Damageable[] allDamageables = (Damageable[])FindObjectsOfType(typeof(Damageable));
			Damageable closestEnemy = null;
			float closestDistance = 0;
			foreach (Damageable damageable in allDamageables) {
				if (damageable.owner != owner) {
					float distance = Vector3.SqrMagnitude(damageable.transform.position - transform.position);
					if (closestEnemy == null || distance < closestDistance) {
						closestEnemy = damageable;
						closestDistance = distance;
					}
				}
			}

			// Move to the closest enemy
			if (closestEnemy != null) {
				moveTo(closestEnemy.transform.position);
			}
		}
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

	// Function called externally to tell the unit where to go
	public void moveTo(Vector3 position) {

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
		moving = true;
	}

	// Function to fire at the closest enemy if in range
	public void fireAtClosestEnemyInRange() {
		
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
		if (owner != null) {
			owner.removeOwnedUnit(this);
		}
		Destroy(line);
	}
}
