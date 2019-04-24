using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub
// Date: April 2019

public class Factory : MonoBehaviour {

	public Transform canvas;
	public PlayerScript owner;
	public GameObject[] objectPrefabs;
    public int[] prices;
	public GameObject menuPrefab;

	private float unitSpacing = 0.5f;
	private GameObject menu;
	private GameObject placingFactory;

	public void createObject(int index) {

        if (prices[index]>owner.money)
        {
            return;
        } else
        {
            owner.money -= prices[index];
        }

		if (objectPrefabs[index].GetComponent<Unit>() != null) {

			// Find a safe spawn location
			Unit[] allUnits = (Unit[])Resources.FindObjectsOfTypeAll(typeof(Unit));
			Vector3 newPos = transform.position + Vector3.forward;
			bool isSafePos = false;
			while (!isSafePos) {
				newPos += Vector3.forward * unitSpacing;
				isSafePos = true;
				foreach (Unit otherUnit in allUnits) {
					if (Vector3.Distance(newPos, otherUnit.transform.position) < unitSpacing) {
						isSafePos = false;
					}
				}
			}

			// Instantiate a new Unit at the safe spawn location
			GameObject newUnit = Instantiate(objectPrefabs[index], newPos, Quaternion.identity);
			owner.addOwnedUnit(newUnit.GetComponent<Unit>());

            if (objectPrefabs[index].GetComponent<Factory>() != null)
            {
                newUnit.GetComponent<Factory>().owner = this.owner;
                newUnit.GetComponent<Factory>().canvas = this.canvas;
                newUnit.GetComponent<Factory>().menuPrefab = this.menuPrefab;
            }

        } else if (objectPrefabs[index].GetComponent<Factory>() != null) {
			
			// Start placing the factory
			placingFactory = Instantiate(objectPrefabs[index]);
			placingFactory.name = objectPrefabs[index].name;
			foreach (Collider collider in placingFactory.GetComponentsInChildren(typeof(Collider))) {
				collider.enabled = false;
			}
		}
	}

	public void Update () {
		if (placingFactory != null) {

			// Move the factory to the mouse selection location
			Ray lookRay = owner.camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(lookRay, out hit, 1000f, LayerMask.GetMask("Terrain"))) {
				placingFactory.transform.position = hit.point;
			}

			// Let the factory go
			if ((menu == null || Input.GetMouseButtonDown(0))&& Vector3.Distance(placingFactory.transform.position, transform.position) <= 3) {
				placingFactory.GetComponent<Factory>().owner = this.owner;
				placingFactory.GetComponent<Factory>().canvas = this.canvas;
				foreach (Collider collider in placingFactory.GetComponentsInChildren(typeof(Collider))) {
					collider.enabled = true;
				}
				placingFactory = null;
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
		menu = null;
	}
}
