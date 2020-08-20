using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Instantiates the specified object OnDestroy. Can be set to instantiate the object multiple times, randomize the position, or add start force to objects.
//Does not instantiate the object if OnDestroy is being called because of scene change or application close

public class instantiateOnDestroy : MonoBehaviour
{
    public GameObject go;
    public int spawnCount = 1; //The number of these things to instantiate
    public Vector2 positionOffset = new Vector2(0f, 0f);
    public Vector2 randomizePosition = new Vector2(0f, 0f);

    [Space]
    [Header("Apply Start Force")]

    public bool useStartForce = true;

    public applyStartForce.forceTransformMultipliers forceTransformMultiplier = applyStartForce.forceTransformMultipliers.None;

    public ForceMode2D mode = ForceMode2D.Impulse;
    public Vector3 force = new Vector3(1f, 1f, 0f);
    public Vector3 forceRandomize = new Vector3(0.1f, 0.1f, 0f); //Use it to apply some randomization to each axis of the force. Higher value is more random. 0 is no randomization.
    public float torqueMax = 1f, torqueMin = -1f;
    bool applicationClosing = false;
    Global global;

    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
    }

    void OnApplicationQuit()
    {
        applicationClosing = true;
    }

    public void OnDestroy()
    {
        //Don't instantiate this crap if the scene is changing or we are quitting the application
        if (!global) return; //There is no global object. I think in some cases on application quit the global object gets destroyed before we get here, thus making this check necessary
        if (global.isSceneChanging() || applicationClosing) return;

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject g = Instantiate(go, transform.position + (Vector3)positionOffset
                + new Vector3(Random.Range(-randomizePosition.x, randomizePosition.x), Random.Range(-randomizePosition.y, randomizePosition.y)),
                Quaternion.identity);

            if (useStartForce)
            {
                applyStartForce asf = g.AddComponent(typeof(applyStartForce)) as applyStartForce;
                asf.forceTransformMultipler = forceTransformMultiplier;
                asf.mode = mode;
                asf.force = force;
                asf.forceRandomize = forceRandomize;
                asf.torqueMax = torqueMax;
                asf.torqueMin = torqueMin;
            }
        }
    }
}
