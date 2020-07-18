using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawnOnDestroy : MonoBehaviour
{
    public GameObject spawnParticles;
    private GameObject spawnPrefab;
    public float time = 1f;

    public bool spawnAtStartTransform = true; //If true, the position, rotation, and scale at the start of the object will be reapplied to the spawn. Otherwise, the respawnTransform will be used
    public Transform respawnTransform;
    private Vector3 spawnPosition = new Vector3(0, 0, 0);
    //private Vector3 spawnScale = new Vector3(1f, 1f, 1f);
    private Quaternion spawnRotation;

    private float timer = 0;
    private Global global;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.activeSelf)
        {
            spawnPrefab = Instantiate(gameObject);
            spawnPrefab.SetActive(false);
        }

        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (spawnAtStartTransform)
        {
            spawnPosition = gameObject.transform.position;
            //spawnScale = gameObject.transform.lossyScale;
            spawnRotation = gameObject.transform.rotation;
        }
        else
        {
            spawnPosition = respawnTransform.position;
           //spawnScale = respawnTransform.lossyScale;
            spawnRotation = respawnTransform.rotation;
        }
    }

    public void onEnabled()
    {
        if (!spawnPrefab)
        {
            spawnPrefab = Instantiate(gameObject);
            spawnPrefab.SetActive(false);
        }
    }

    public void OnDestroy()
    {
        GameObject spawnerGo = Instantiate(new GameObject(), spawnPosition, spawnRotation);
        //spawnerGo.transform.localScale = spawnScale;

        spawnObject spawner = spawnerGo.AddComponent(typeof(spawnObject)) as spawnObject;
        spawner.addRespawnObject(spawnPrefab);
        spawner.setTimer(time);
        spawner.setParticles(spawnParticles);
        spawner.setDestroyOnSpawn(true);

        Destroy(gameObject);
    }
}
