using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Paul Armstrong, XRClub
// Date: March 2019

public class CommandMarkScript : MonoBehaviour {

	public float decayTime = 0.2f;
	private Vector3 originalScale;
	private float endTime;
	private float factor;

	void Start () {
		originalScale = transform.localScale;
		endTime = Time.time + decayTime;
	}

    void Update() {
		factor = (endTime - Time.time) / decayTime;
        transform.localScale = new Vector3(originalScale.x*factor, originalScale.y, originalScale.z*factor);
		if (factor < 0) {
			Destroy(gameObject);
		}
    }
}
