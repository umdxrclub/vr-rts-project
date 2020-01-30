using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub
// Date: April 2019

public class Factory : Building {

	public GameObject[] objectPrefabs;
    public int[] prices;
	public GameObject menuPrefab;

	private float unitSpacing = 0.5f;
	private float buildingSpacing = 1f;
	private float buildingRange = 3f;
	private GameObject menu;
	private GameObject placingBuilding;
	private int placingPrice;

	public void createObject(int index) {
		
		if (objectPrefabs[index].GetComponent<Unit>() != null && owner.money >= prices[index]) {

			owner.money -= prices[index];
			Vector3 newPos = transform.position + Vector3.forward;
			bool isSafePos = false;

			// Keep moving the position until there is a safe position
			while (!isSafePos) {
				newPos += Vector3.forward * unitSpacing;
				isSafePos = true;
				Collider[] nearbyObjects = Physics.OverlapSphere(newPos, unitSpacing);
				foreach (Collider coll in nearbyObjects) {
					if (getComponentInOrParent<Factory>(coll.transform) != null || getComponentInOrParent<Unit>(coll.transform) != null) {
						isSafePos = false;
					}
				}
			}

			// Instantiate a new Unit at the safe spawn location
			GameObject newUnit = Instantiate(objectPrefabs[index], newPos, Quaternion.identity);
			newUnit.name = objectPrefabs[index].name;
			newUnit.GetComponent<Unit>().owner = this.owner;
			owner.addOwnedUnit(newUnit.GetComponent<Unit>());
			

			// If the new unit is also a factory, provide references to things
            if (objectPrefabs[index].GetComponent<Factory>() != null) {
                newUnit.GetComponent<Factory>().owner = this.owner;
                newUnit.GetComponent<Factory>().canvas = this.canvas;
            }

		// If a mine is being placed down, check to see if one can be placed down
		} else if (objectPrefabs[index].GetComponent<Mine>() != null) {

			Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, buildingRange);
			foreach (Collider collider in nearbyObjects) {
				if (collider.tag.Equals("resource") && owner.money >= prices[index]) {

					// If there is a valid resource and the factory owner has enough money, put a mine
					owner.money -= prices[index];
					GameObject newMine = Instantiate(objectPrefabs[index], collider.transform.position, Quaternion.identity);
					newMine.GetComponent<Building>().owner = this.owner;
					newMine.GetComponent<Building>().canvas = this.canvas;
					Destroy(collider.gameObject);

					// Also destroy the miner (this object)
					closeMenu();
					Destroy(gameObject);

					return;
				}
			}

		// Otherwise a normal Building is being placed down
        } else if (objectPrefabs[index].GetComponent<Building>() != null && placingBuilding == null) {
			
			// Start placing the building
			placingBuilding = Instantiate(objectPrefabs[index]);
			placingBuilding.name = objectPrefabs[index].name;
			foreach (Collider collider in placingBuilding.GetComponentsInChildren(typeof(Collider))) {
				collider.enabled = false;
			}
			placingPrice = prices[index];
		}
	}

	void Update() {

		// If C is pressed, close the menu
		if (Input.GetKeyDown(KeyCode.C)) {
			closeMenu();
		}

		if (placingBuilding != null) {

			// Move the building to the mouse selection location clamped to the building range
			Ray lookRay = owner.cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(lookRay, out hit, 1000f, LayerMask.GetMask("Terrain"))) {
				placingBuilding.transform.position =
						transform.position + Vector3.ClampMagnitude(hit.point - transform.position, buildingRange);
			}

			// Check to see if it should be placed down
			if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()
					&& owner.money >= placingPrice) {

				// Make sure there are not any buildings or units too close
				bool isSafePos = true;
				Collider[] nearbyObjects = Physics.OverlapSphere(placingBuilding.transform.position, buildingSpacing);
				foreach (Collider coll in nearbyObjects) {
					if (getComponentInOrParent<Building>(coll.transform)!= null || getComponentInOrParent<Unit>(coll.transform)!= null) {
						isSafePos = false;
					}
				}

				// If it is safe and valid, place it down
				if (isSafePos) {
					owner.money -= placingPrice;
					placingBuilding.GetComponent<Building>().owner = this.owner;
					placingBuilding.GetComponent<Building>().canvas = this.canvas;
					foreach (Collider coll in placingBuilding.GetComponentsInChildren(typeof(Collider))) {
						coll.enabled = true;
					}
					placingBuilding = null;
				}
			}
		}
	}

	public void openMenu() {

		// If there is no owner, don't do anything
		if (owner == null) {
			return;
		}

		// If another factory menu exists, close it
		Factory[] allFactories = (Factory[])Canvas.FindObjectsOfType(typeof(Factory));
		foreach (Factory factory in allFactories) {
			if (factory.owner == this.owner && factory.menu != null) {
				factory.closeMenu();
			}
		}

		// Create the new menu
		menu = Instantiate(menuPrefab);
		menu.transform.SetParent(canvas);
		menu.GetComponent<RectTransform>().offsetMin = menuPrefab.GetComponent<RectTransform>().offsetMin;
		menu.GetComponent<RectTransform>().offsetMax = menuPrefab.GetComponent<RectTransform>().offsetMax;
		menu.SetActive(true);
		menu.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = gameObject.name + " Menu";

		// Bind the X button to close the menu
		menu.transform.GetChild(0).GetChild(1).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {
			if (gameObject != null) {
				closeMenu();
			}
		});

		// Add buttons for the units
		GameObject genericButton = menu.transform.GetChild(1).gameObject;
		for (int i = 0; i < objectPrefabs.Length; i++) {

			GameObject newButton = Instantiate(genericButton);
			newButton.transform.SetParent(menu.transform);
			newButton.GetComponent<RectTransform>().offsetMin =
					genericButton.GetComponent<RectTransform>().offsetMin + new Vector2(0, -36*i);
			newButton.GetComponent<RectTransform>().offsetMax =
					genericButton.GetComponent<RectTransform>().offsetMax + new Vector2(0, -36*i);

			newButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = objectPrefabs[i].name;
			newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {
				createObject(int.Parse(newButton.name));
			});
			newButton.name = i.ToString();

		}
		Destroy(genericButton);
	}

	public void closeMenu() {
		Destroy(menu);
		Destroy(placingBuilding);
		menu = null;
		placingBuilding = null;
	}

	// Helper function for finding the specified component in curr or a parent transform
	private T getComponentInOrParent<T>(Transform curr) {
		while (curr != null && curr.GetComponent<T>() == null) {
			curr = curr.parent;
		}
		return curr == null ? default(T) : curr.GetComponent<T>();
	}
}
