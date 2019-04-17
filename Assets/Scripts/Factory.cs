using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub
// Date: April 2019

public class Factory : MonoBehaviour {

	public Transform canvas;
	public PlayerScript owner;
	public GameObject[] unitPrefabs;
	public GameObject menuPrefab;

	private float unitSpacing = 0.5f;
	private GameObject menu;

	public void createUnit(int index) {

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
		GameObject newUnit = Instantiate(unitPrefabs[index], newPos, Quaternion.identity);
		owner.addOwnedUnit(newUnit.GetComponent<Unit>());

	}

	public void openMenu() {

		// If the menu exists, don't need to open a new menu
		if (menu != null) {
			return;
		}

		// Create the new menu
		menu = Instantiate(menuPrefab);
		menu.transform.SetParent(canvas);
		menu.GetComponent<RectTransform>().offsetMin = menuPrefab.GetComponent<RectTransform>().offsetMin;
		menu.GetComponent<RectTransform>().offsetMax = menuPrefab.GetComponent<RectTransform>().offsetMax;
		menu.SetActive(true);

		// Bind the X button to close the menu
		menu.transform.GetChild(0).GetChild(1).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {
			if (gameObject != null) {
				closeMenu();
			}
		});

		// Add buttons for the units
		GameObject genericButton = menu.transform.GetChild(1).gameObject;
		for (int i = 0; i < unitPrefabs.Length; i++) {

			GameObject newButton = Instantiate(genericButton);
			newButton.transform.SetParent(menu.transform);
			newButton.GetComponent<RectTransform>().offsetMin =
					genericButton.GetComponent<RectTransform>().offsetMin + new Vector2(0, -36*i);
			newButton.GetComponent<RectTransform>().offsetMax =
					genericButton.GetComponent<RectTransform>().offsetMax + new Vector2(0, -36*i);

			newButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = unitPrefabs[i].name;
			newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {
				createUnit(int.Parse(newButton.name));
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
