using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Groups together Units and Buildings
public class Damageable : MonoBehaviour
{
	[HideInInspector]
    public PlayerScript owner;
    
    public float maxHealth;
    public float health;

    void Start() {

        // Set health to max
		health = maxHealth;
    }

    // Function to deal damage to this Damageable
	public void doDamage(float dmg) {
        health -= dmg;
        if (health <= 0) {

            // TODO make the object explode or something
            Destroy(gameObject);
        }
	}
}
