using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : Damageable {

	public float speed = 1f;
	public float damage = 10f;
	public float fireInterval = 1f;
	public float range = 1f;

	public GameObject[] coloredParts;
	public Material red, blue;

	[HideInInspector]
	public bool onAuto = false;
	[HideInInspector]
	public GameObject selectionCircle;

	// if moving is true, targetPos must exist, targetObject might be null
	protected CharacterController cc;
	protected Damageable closestEnemy = null;
	protected GameObject targetObject = null;
	protected bool moving = false;
	protected Vector3 targetPos = Vector3.zero;
	protected float targetAngle = 0f;
	protected LineRenderer line;
	
    void Start() {

		cc = GetComponent<CharacterController>();

		// Create the selection circle
		selectionCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		selectionCircle.transform.SetParent(transform);
		selectionCircle.transform.position = new Vector3(transform.position.x,
				TerrainScript.instance.heightMap(transform.position.x, transform.position.z) + 0.05f, transform.position.z);
		selectionCircle.transform.localScale = new Vector3(2.5f * cc.radius, 0.1f, 2.5f * cc.radius);
		Destroy(selectionCircle.GetComponent<CapsuleCollider>());
		selectionCircle.GetComponent<MeshRenderer>().enabled = false;

		// Set the colored parts to the owner's color
		Material newColor = (owner == null ? red : blue);
		selectionCircle.GetComponent<Renderer>().material = newColor;
		foreach(GameObject coloredPart in coloredParts) {
			coloredPart.GetComponent<Renderer>().material = newColor;
		}

		if (damage > 0) {
			InvokeRepeating("fireAtClosestEnemyInRange", Random.value * 0.2f, fireInterval);
		}

		// Start watching for closest enemy for when onAuto is true
		InvokeRepeating("updateEnemySearch", Random.value * 0.2f, 1);
    }

    void Update() {

        if (moving) {
			if (targetObject != null) {
				targetPos = targetObject.transform.position;
			}
			
			// Perform the smooth movement and rotation
			Vector2 movementVector = new Vector2(targetPos.x - transform.position.x, targetPos.z - transform.position.z);
			cc.Move((Vector3.ClampMagnitude(new Vector3(movementVector.x, 0, movementVector.y), 1) + Vector3.down)
					* speed *  Time.deltaTime);
			transform.localEulerAngles =
					new Vector3(0, Mathf.LerpAngle(transform.localEulerAngles.y, targetAngle, Mathf.Min(10*Time.deltaTime, 0.2f)), 0);

			if (owner != null) {
				// Maintain the selection circle's position
				selectionCircle.transform.position = new Vector3(transform.position.x,
						TerrainScript.instance.heightMap(transform.position.x, transform.position.z) + 0.05f, transform.position.z);

				// Maintain the path line
				line.SetPositions(new Vector3[] { selectionCircle.transform.position + 0.05f*Vector3.up , targetPos + 0.05f*Vector3.up });
			}

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
		lineObj.GetComponent<Renderer>().enabled = owner != null;
		line = lineObj.GetComponent<LineRenderer>();
		line.generateLightingData = true;
		line.startWidth = 0.01f;
		line.endWidth = 0.01f;

		moving = true;
	}

	// Function to tell the unit to go to a GameObject
	public void moveTo(GameObject target) {

		targetObject = target;
		moveTo(targetObject.transform.position);
	}

	// Function to fire at the closest enemy if in range
	void fireAtClosestEnemyInRange() {
				
		// If there is an enemy and they are in range, fire at the enemy
		if (closestEnemy != null
				&& closestEnemy.owner != owner
				&& Vector3.SqrMagnitude(closestEnemy.transform.position - transform.position) <= range*range) {

			// Create laser to visually represent damage being done
			GameObject laserShotObj = new GameObject();
			LaserShotScript laserShot = laserShotObj.AddComponent<LaserShotScript>();
			laserShot.color = selectionCircle.GetComponent<Renderer>().material;
			laserShot.origin = transform;
			laserShot.destination = closestEnemy.transform;
			laserShot.laserSpeed = 10f;
			laserShot.laserLength = 0.5f;
			laserShot.damage = damage;
			laserShot.damageEvent = new DamageEvent();
			laserShot.damageEvent.AddListener(closestEnemy.doDamage);
		}
	}

	// Check for the most up to date closest enemy to target
	void updateEnemySearch() {

		// Find the closest enemy
		Damageable[] allDamageables = (Damageable[])FindObjectsOfType(typeof(Damageable));
		Damageable localClosestEnemy = null;
		float closestDistance = 0;
		foreach (Damageable damageable in allDamageables) {
			if (damageable.owner != owner) {
				float distance = Vector3.SqrMagnitude(damageable.transform.position - transform.position);
				if (localClosestEnemy == null || distance < closestDistance) {
					localClosestEnemy = damageable;
					closestDistance = distance;
				}
			}
		}

		// If there is a closest enemy, keep track of it
		// If on auto, move to this enemy
		if (localClosestEnemy != null) {
			closestEnemy = localClosestEnemy;
			if (onAuto) {
				moveTo(closestEnemy.gameObject);
			}
		}
	}

	void OnDestroy() {
		if (owner != null) {
			owner.removeOwnedUnit(this);
		}
		Destroy(line);
	}
}
