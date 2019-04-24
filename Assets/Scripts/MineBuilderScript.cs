using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBuilderScript : Unit {
	
	void Start() {

		// Override the default Unit stats
		speed = 1.25f;

		setUpUnit();
	}
}
