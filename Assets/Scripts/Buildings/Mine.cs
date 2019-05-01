using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: XRClub RTS Team
// Date: April 2019

public class Mine : Building
{

    int moneyGain = 50;
    float interval = 10f;

    void Start() {
        InvokeRepeating("addMoney", 0f, interval);
    }

    private void addMoney() {

		if (owner != null) {
			// Add money to the owner
			owner.money += moneyGain;
		}
		
	}
}
