using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawnOnDestroy : MonoBehaviour
{
    public GameObject spawnParticles;
    public Vector2 spawnParticlesOffset = new Vector2(0f, 0f);
    private GameObject spawnPrefab;
    public float time = 1f;

    public bool spawnAtStartTransform = true; //If true, the position, rotation, and scale at the start of the object will be reapplied to the spawn. Otherwise, the respawnTransform will be used
    public Transform respawnTransform;
    private Vector3 spawnPosition = new Vector3(0, 0, 0);
    //private Vector3 spawnScale = new Vector3(1f, 1f, 1f);
    private Quaternion spawnRotation;

    private float timer = 0;
    private Global global;
    private bool applicationClosing = false; //Set to true by onApplicationQuit. Used so we aren't trying to respawn objects when we close the application.

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

    void OnApplicationQuit()
    {
        applicationClosing = true;
    }

    public void OnDestroy()
    {
        //Don't respawn if the scene is changing or we are quitting the application
        if (!global) return; //There is no global object. I think in some cases on application quit the global object gets destroyed before we get here, thus making this check necessary
        if (global.isSceneChanging() || applicationClosing) return;

        GameObject spawnerGo = Instantiate(new GameObject(), spawnPosition, spawnRotation);
        //spawnerGo.transform.localScale = spawnScale;

        spawnObject spawner = spawnerGo.AddComponent(typeof(spawnObject)) as spawnObject;
        spawner.addRespawnObject(spawnPrefab);
        spawner.setTimer(time);
        spawner.setParticles(spawnParticles);
        spawner.setParticlesOffset(spawnParticlesOffset);
        spawner.setDestroyOnSpawn(true);

        Destroy(gameObject);
    }
}
