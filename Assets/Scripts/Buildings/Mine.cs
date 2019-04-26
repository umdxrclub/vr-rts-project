using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: XRClub RTS Team
// Date: April 2019

public class Mine : Building {

    void FixedUpdate() {

		if (owner != null) {
			// Add money to the owner
			owner.money++;
		}
		
	}
}
