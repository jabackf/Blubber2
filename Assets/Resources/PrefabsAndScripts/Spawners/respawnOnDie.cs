using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//When the Die() function is called, the object gets destroyed then respawned

public class respawnOnDie : MonoBehaviour
{
    public GameObject spawnParticles;
    public float time = 1f;

    public bool spawnAtStartTransform = true; //If true, the position, rotation, and scale at the start of the object will be reapplied to the spawn. Otherwise, the respawnTransform will be used
    public Transform respawnTransform;
    private Vector3 spawnPosition = new Vector3(0, 0, 0);
    private Vector3 spawnScale = new Vector3(1f, 1f, 1f); 
    private Quaternion spawnRotation; 

    private GameObject spawn;
    private float timer = 0;
    private bool respawning = false;
    private Global global;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Created: " + gameObject);
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (spawnAtStartTransform)
        {
            spawnPosition = gameObject.transform.position;
            spawnScale = gameObject.transform.lossyScale;
            spawnRotation = gameObject.transform.rotation;
        }
        else
        {
            spawnPosition = respawnTransform.position;
            spawnScale = respawnTransform.lossyScale;
            spawnRotation = respawnTransform.rotation;
        }
    }

    public void Die()
    {
        spawn = Instantiate(gameObject, spawnPosition, spawnRotation);
        spawn.transform.localScale = spawnScale;

        respawnOnDie spawner = spawn.GetComponent<respawnOnDie>();
        spawner.newSpawn();

        Destroy(gameObject);
    }

    public void newSpawn()
    {
        respawning = true;
        timer = time;
    }

    void Update()
    {
        if (respawning)
        {
            if (timer > 0) timer -= Time.deltaTime;
            else
            {
                respawning = false;
                Instantiate(spawnParticles, spawnPosition, spawnRotation);
            }
        }
    }
}
