using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub

public class PlayerScript : MonoBehaviour {

	public float lookSensitivity;
	public GameObject canvas;
    public UnityEngine.UI.Text moneyText;
    public GameObject commandMarkPrefab;
	public GameObject dragSelectionPanelPrefab;
	public Camera cam;
    public int money = 1000;
	public int income = 0;

    private Transform head;
	private CharacterController cc;
	private float walkSpeed;
	private float jumpVelocity;
	private List<Unit> ownedUnits;
	private List<Unit> selectedUnits;
	private float mouseDownTime;
	private Vector3 mouseDownPos;
	private bool dragSelecting;
	private GameObject dragSelectionPanel;
	private string[] mouseModeNames;

	void Start () {

		lookSensitivity = 2f;
		head = transform.Find("Head");
		cam = head.GetComponent<Camera>();
		cc = GetComponent<CharacterController>();
		walkSpeed = 5f;
		jumpVelocity = 0f;
		ownedUnits = new List<Unit>();
		selectedUnits = new List<Unit>();
		mouseDownTime = 0f;
		mouseDownPos = Vector3.zero;
		dragSelecting = false;
		dragSelectionPanel = null;

        InvokeRepeating("addIncome", 0f, 1f);
	}

	void addIncome() {
		money += income;
	}

	void Update () {

		// Update the money UI
		moneyText.text = "Money: $" + money + " | Income: $" + income + "/s";

		// Camera rotation
		if (Input.GetMouseButton(1)) {
			transform.eulerAngles += new Vector3(0f, Input.GetAxisRaw("Mouse X")*lookSensitivity, 0f);
			head.eulerAngles += new Vector3(-Input.GetAxisRaw("Mouse Y")*lookSensitivity, 0f, 0f);
		}
		
		// Movement
		Vector3 newVelocity = Vector3.ClampMagnitude(
			Input.GetAxisRaw("Vertical")*transform.forward + Input.GetAxisRaw("Horizontal")*transform.right, 1f
		) * walkSpeed;
		if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded) {
			jumpVelocity = 5f;
		}
		if (!cc.isGrounded) {
			jumpVelocity = jumpVelocity - 9.8f * Time.deltaTime;
		}
		cc.Move(new Vector3(newVelocity.x, jumpVelocity, newVelocity.z) * Time.deltaTime);

		// If C is pressed, clear the selection
		if (Input.GetKeyDown(KeyCode.C)) {
			clearSelectedUnits();
		}

		if (Input.GetMouseButtonDown(0)) {
			mouseDownTime = Time.time;
			mouseDownPos = Input.mousePosition;
		}

		selectionUpdate();
	}

	// Public function for adding a unit to the player's ownedUnits
	public void addOwnedUnit(Unit unit) {
		ownedUnits.Add(unit);
	}

		// Public function for removing a unit from the player's ownedUnits
	public void removeOwnedUnit(Unit unit) {
		selectedUnits.Remove(unit);
		ownedUnits.Remove(unit);
	}

	// Function to process the selection for each update
	private void selectionUpdate() {

		// Process a non-dragging selection click
		if (Input.GetMouseButtonUp(0) && !dragSelecting) {
		    processSelectionClick(cam.ScreenPointToRay(Input.mousePosition));
		}

		// Start the drag selection
		if (Input.GetMouseButton(0) && !dragSelecting && Time.time - mouseDownTime > 0.1f
				&& Vector2.Distance(mouseDownPos, new Vector2(Input.mousePosition.x, Input.mousePosition.y)) > 30) {
			dragSelecting = true;
			dragSelectionPanel = Instantiate(dragSelectionPanelPrefab, Vector3.zero , Quaternion.identity, canvas.transform);
			dragSelectionPanel.SetActive(true);

			clearSelectedUnits();
		}

		// Process the drag selection each frame dragging
		if (dragSelecting) {
			
			float bottom = Mathf.Min(mouseDownPos.y, Input.mousePosition.y);
			float top = Mathf.Max(mouseDownPos.y, Input.mousePosition.y);
			float left = Mathf.Min(mouseDownPos.x, Input.mousePosition.x);
			float right = Mathf.Max(mouseDownPos.x, Input.mousePosition.x);

			dragSelectionPanel.GetComponent<RectTransform>().offsetMin = new Vector2(left, bottom);
			dragSelectionPanel.GetComponent<RectTransform>().offsetMax =
					new Vector2(right - Screen.width, top - Screen.height);

			foreach (Unit unit in ownedUnits) {
				Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position);
				if (left < screenPos.x && screenPos.x < right && bottom < screenPos.y && screenPos.y < top) {
					if (!selectedUnits.Contains(unit)) {
						addSelectedUnit(unit);
					}
				} else {
					if (selectedUnits.Contains(unit)) {
						removeSelectedUnit(unit);
					}
				}
			}
		}

		// End the drag selection
		if (Input.GetMouseButtonUp(0) && dragSelecting) {
			
			dragSelecting = false;
			Destroy(dragSelectionPanel);
		}
	}
	
	// Function to process a non-dragging selection click event
	private void processSelectionClick(Ray pointingRay) {

		RaycastHit hit;
		if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(pointingRay, out hit)) {

			// If it hits a factory, open the factory menu and return
			Factory hitFactory = getComponentInOrParent<Factory>(hit.transform);
			if (hitFactory != null) {
				hitFactory.openMenu();
			}

			Unit hitUnit = getComponentInOrParent<Unit>(hit.transform);
			if (ownedUnits.Contains(hitUnit)) {

				// If left click is pressed, and left control isn't held, reset the selection
				if (!Input.GetKey(KeyCode.LeftControl)) {
					clearSelectedUnits();
				}

				if (!selectedUnits.Contains(hitUnit)) {
					addSelectedUnit(hitUnit);
				} else {
					removeSelectedUnit(hitUnit);
				}
			} else {
				commandMovement();
			}
		}
	}

	// Function to issue a movement command to selected units
	private void commandMovement() {

		// First make sure there are units selected
		if (selectedUnits.Count > 0) {
			Ray lookRay = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(lookRay, out hit)) {

				// Create a mark
				GameObject newMark = Instantiate(commandMarkPrefab);
				newMark.transform.position = hit.point;
				newMark.GetComponent<CommandMarkScript>().decayTime = 0.1f;

				// Tell each unit to move to the position
				foreach (Unit unit in selectedUnits) {
					unit.moveTo(hit.point);
				}
			}
		}
	}

	// Function to add to selected units
	private void addSelectedUnit(Unit unit) {
		unit.selectionCircle.GetComponent<MeshRenderer>().enabled = true;
		selectedUnits.Add(unit);
	}

	// Function to select a unit
	private void removeSelectedUnit(Unit unit) {
		unit.selectionCircle.GetComponent<MeshRenderer>().enabled = false;
		selectedUnits.Remove(unit);
	}

	// Function to clear selected units
	private void clearSelectedUnits() {
		foreach (Unit unit in selectedUnits) {
			unit.selectionCircle.GetComponent<MeshRenderer>().enabled = false;
		}
		selectedUnits.Clear();
	}

	// Helper function for finding the specified component in curr or a parent transform
	private T getComponentInOrParent<T>(Transform curr) {
		while (curr != null && curr.GetComponent<T>() == null) {
			curr = curr.parent;
		}
		return curr == null ? default(T) : curr.GetComponent<T>();
	}

	// Helper function does integer mod with expected behavior for negative m
	private int safeMod(int m, int n) {
		return (m + n) % n;
	}
}
