using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Spawns a player object if it doesn't exist
public class PlayerSpawn : MonoBehaviour
{

    public GameObject playerPrefab;
    public string playerTag = "Player";

    private SpriteRenderer renderer;

    void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        renderer.enabled = false;

        if (GameObject.FindWithTag(playerTag) == null)
        {
            Instantiate(playerPrefab, gameObject.transform.position, gameObject.transform.rotation);
        }
    }
}
