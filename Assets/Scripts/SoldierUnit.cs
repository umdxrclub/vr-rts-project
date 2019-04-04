using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierUnit : Unit {
	
	void Start() {

		// Override the normal speed
		speed = 1.25f;

		setUpUnit();
	}
}
