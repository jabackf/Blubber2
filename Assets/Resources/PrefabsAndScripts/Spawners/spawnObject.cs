using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnObject : MonoBehaviour
{
    public float spawnTimeWait = 10f; //The amount of time to wait before spawning. Use -1 for no timer.

    public bool destroyOnSpawn = false; //If true, the gameObject this spawn script is attached to will self destruct when the spawn is created.

    public GameObject spawnParticles;

    private float spawnTimer = 0;

    Global global;

    [SerializeField] public List<GameObject> objectList = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (spawnTimeWait != -1) spawnTimer = spawnTimeWait;
        Debug.Log("Spawner spawned!");
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnTimeWait != -1)
        {
            if (spawnTimer > 0)
            {
                spawnTimer -= Time.deltaTime;
            }
            else
            {
                Spawn();
                spawnTimer = spawnTimeWait;
            }
        }
    }

    public void addRespawnObject(GameObject go)
    {
        objectList.Add(go);
    }

    public void setParticles(GameObject p)
    {
        spawnParticles = p;
    }

    public void setTimer(float time)
    {
        spawnTimer = spawnTimeWait = time;
    }

    public void setDestroyOnSpawn(bool dos)
    {
        destroyOnSpawn = dos;
    }

    public void Spawn(int index = -1)
    {
        if (objectList.Count != 0)
        {
            if (index == -1) index = UnityEngine.Random.Range(0, objectList.Count);
            GameObject go = Instantiate(objectList[index], gameObject.transform.position, gameObject.transform.rotation);
            global.map.settings.objectCreated(go);
            if (spawnParticles!=null) Instantiate(spawnParticles, gameObject.transform.position, gameObject.transform.rotation);
            if (destroyOnSpawn) Destroy(gameObject);
        }
    }

}
