using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoredSoldierUnit : Unit {
	
	void Start() {

		// Override the normal speed
		speed = 0.75f;

		setUpUnit();
	}
}
