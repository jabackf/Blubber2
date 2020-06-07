using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Spawns a player object if it doesn't exist
public class PlayerSpawn : MonoBehaviour
{

    public GameObject playerPrefab;
    public string playerTag = "Player";

    public float respawnTime =  2f; //The amount of time if the setRespawnTime is ran

    public GameObject spawnParticles;

    private SpriteRenderer renderer;
    private float timer = -1;

    void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        renderer.enabled = false;

        Spawn();

    }

    void Update()
    {
        if (timer!=-1)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                Spawn();
                timer = -1;
            }
        }
    }

    public void Spawn()
    {
        if (GameObject.FindWithTag(playerTag) == null)
        {
            Instantiate(playerPrefab, gameObject.transform.position, gameObject.transform.rotation);
            if (spawnParticles) Instantiate(spawnParticles, gameObject.transform.position, Quaternion.identity);
        }
    }

    public void respawn(float time=-1)
    {
        if (time == -1) time = respawnTime;
        timer = time;

    }
}
