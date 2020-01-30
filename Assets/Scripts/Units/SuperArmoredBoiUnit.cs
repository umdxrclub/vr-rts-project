using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperArmoredBoiUnit : Unit
{
    void Start() {
        
        // Override the default Unit stats
        speed = 0.4f;
        maxHealth = 500;

        setUpUnit();
    }
}
