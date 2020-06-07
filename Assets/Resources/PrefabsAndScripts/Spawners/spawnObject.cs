using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnObject : MonoBehaviour
{
    public float spawnTimeWait = 10f; //The amount of time to wait before spawning. Use -1 for no timer.
    private float spawnTimer = 0;

    [SerializeField] public List<GameObject> objectList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (spawnTimeWait != -1) spawnTimer = spawnTimeWait;
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


    public void Spawn(int index = -1)
    {
        if (objectList.Count != 0)
        {
            if (index == -1) index = UnityEngine.Random.Range(0, objectList.Count);
            Instantiate(objectList[index], gameObject.transform.position, gameObject.transform.rotation);
        }
    }

}
