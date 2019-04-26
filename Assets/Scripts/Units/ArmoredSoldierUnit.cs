using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoredSoldierUnit : Unit {
	
	void Start() {

		// Override the default Unit stats
		speed = 0.75f;
		maxHealth = 2;

		setUpUnit();
	}
}
