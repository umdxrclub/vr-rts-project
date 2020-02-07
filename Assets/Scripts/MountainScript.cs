using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainScript : MonoBehaviour
{
    public GameObject mountainManPrefab;
    public GameObject terrain;
    // Start is called before the first frame update
    void Start()
    {
        // Start spawning mountain men
        InvokeRepeating("spawnMountainMan", 5, 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void spawnMountainMan() {
        GameObject newMountainMan = Instantiate(mountainManPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity);
        newMountainMan.GetComponent<Unit>().owner = null;
        newMountainMan.name = "Mountain Man";

        StartCoroutine(doMoveTo(newMountainMan));
    }

    private IEnumerator doMoveTo(GameObject newMountainMan) {
        yield return new WaitForSeconds(1);
        float angle = Random.value * 2f * Mathf.PI;
        float x = Mathf.Sin(angle), z = Mathf.Cos(angle);
        Vector3 targetPos = new Vector3(x, terrain.GetComponent<TerrainScript>().heightMap(x, z), z);
        newMountainMan.GetComponent<Unit>().moveTo(targetPos);
    }
}
