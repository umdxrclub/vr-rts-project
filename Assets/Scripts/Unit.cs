using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub
// Date: March 2019

public class Unit : MonoBehaviour {
	public float speed;
	public MeshRenderer selectionCircle;
	public Material themeColor;
	
	private CharacterController cc;
	private bool moving;
	private Vector3 targetPos;
	private float targetAngle;
	private LineRenderer line;
	
    void Start() {
		speed = 1f;
		cc = GetComponent<CharacterController>();
		moving = false;
		targetPos = Vector3.zero;
		targetAngle = 0f;
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
		}
    }

	public void moveTo(Vector3 position) {
		moving = true;
		targetPos = position;
		targetAngle = Mathf.Rad2Deg*Mathf.Atan2(targetPos.x-transform.position.x, targetPos.z-transform.position.z);
		Destroy(line);
		GameObject lineObj = new GameObject();
		lineObj.AddComponent<LineRenderer>();
		lineObj.GetComponent<Renderer>().material = themeColor;
		line = lineObj.GetComponent<LineRenderer>();
		line.generateLightingData = true;
		line.startWidth = 0.01f;
		line.endWidth = 0.01f;
	}
}
