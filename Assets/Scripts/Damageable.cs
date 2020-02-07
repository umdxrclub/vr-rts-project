using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Groups together Units and Buildings
public class Damageable : MonoBehaviour
{
	[HideInInspector]
    public PlayerScript owner;
    
    public float maxHealth;
    protected float health;
}
