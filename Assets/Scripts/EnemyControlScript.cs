using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControlScript : MonoBehaviour
{

    public TerrainScript terrain;
    public GameObject mountainManPrefab;
    public float waveDuration = 3f;

    private float nextWaveTime = 0f;
    private bool started = false;

    // This is will be called when the terrain is done
    public void BeginControlling() {
        started = true;
    }

    void FixedUpdate() {
        if (started && Time.time > nextWaveTime) {
            
            // Spawn a mountain man at each mountain
            foreach (GameObject mountain in terrain.mountains) {

                GameObject newMountainMan = Instantiate(mountainManPrefab, mountain.transform.position + Vector3.up * 0.1f, Quaternion.identity);
                newMountainMan.GetComponent<Unit>().owner = null;
                newMountainMan.GetComponent<Unit>().onAuto = true;
                newMountainMan.name = "Mountain Man";
            }

            // Set the new nextWaveTime
            nextWaveTime = Time.time + waveDuration;
        }
    }
}
