using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Spawns a player object if it doesn't exist
public class PlayerSpawn : MonoBehaviour
{

    //NOTE: This object was initially used to respawn the player after death. The respawn action was changed to occur in the characterController. 
    //The respawn code wasn't removed from this script. This object still has the ability to respawn the player, but this isn't where it actually happens in game
    //Instead, this object is used to spawn the player if the player does not exist (i.e. he didn't come from another room via dontdestroyonload)

    public GameObject playerPrefab;
    public string playerTag = "Player";

    public float respawnTime =  2f; //The amount of time if the setRespawnTime is ran

    public GameObject spawnParticles;

    private SpriteRenderer renderer;
    private float timer = -1;

    Global global;

    void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        renderer.enabled = false;
        global = GameObject.FindWithTag("global").GetComponent<Global>();
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
            global.map.settings.objectCreated(Instantiate(playerPrefab, gameObject.transform.position, gameObject.transform.rotation));
            if (spawnParticles) Instantiate(spawnParticles, gameObject.transform.position, Quaternion.identity);
        }
    }

    public void respawn(float time=-1)
    {
        if (time == -1) time = respawnTime;
        timer = time;

    }
}
