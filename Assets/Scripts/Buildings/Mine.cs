using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Building
{

    int mineIncome = 5;

    void Start() {
		if (owner != null) {
			owner.income += mineIncome;
		}
    }

	void OnDestroy() {
		if (owner != null) {
			owner.income -= mineIncome;
		}
	}
}
