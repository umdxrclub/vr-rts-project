using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageEvent : UnityEvent<float>{};

public class LaserShotScript : MonoBehaviour
{
    public Transform origin, destination;
    public float damage;
    public DamageEvent damageEvent;
    public float laserLength = 0.1f;
    public float laserSpeed = 1f;
    public Material color;

    private float originTime;
    private float originalDistance;
    private float laserDuration;
    private LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        originTime = Time.time;
        originalDistance = Vector3.Magnitude(destination.position - origin.position);
        laserDuration = originalDistance / laserSpeed;

		lr = gameObject.AddComponent<LineRenderer>();
		GetComponent<Renderer>().material = color;
		lr.generateLightingData = true;
		lr.startWidth = 0.05f;
		lr.endWidth = 0.05f;

        Destroy(gameObject, laserDuration);
    }

    // Update is called once per frame
    void Update()
    {
        if (origin != null && destination != null) {
            Vector3 originDestVector = (destination.position - origin.position);
            Vector3 laserFrontOffset = (destination.position - origin.position) * (Time.time - originTime) / laserDuration;
            Vector3 laserBackOffset = Vector3.SqrMagnitude(laserFrontOffset) > laserLength*laserLength ?
                    laserFrontOffset - Vector3.Normalize(originDestVector) * laserLength : Vector3.zero;
            lr.SetPositions(new Vector3[] {
                origin.position + laserBackOffset,
                origin.position + laserFrontOffset
            });
        } else {
            Destroy(gameObject);
        }
    }

    void OnDestroy() {
        if (damageEvent != null) {
            damageEvent.Invoke(damage);
        }
    }
}
