using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Apply this script to objects that warp the player from one scene to another
public class WarpTrigger : MonoBehaviour
{
    public MapSystem.transitions transType = MapSystem.transitions.none;
    public string goTo = ""; //Can be map name of the format mapname_xpos_ypos, or special words "up", "down", "left", "right", "current"
    public bool callOnTriggerEnter = true; //Set to true to execute the warp on trigger enter

    private Global global;
    private bool triggered = false; //Set to true when the warp function has been called

    public bool wrapX = false;
    public bool wrapY = false;
    public string warpTag = "";

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
       
        if (callOnTriggerEnter && !triggered)
        {
            if (global==null ) global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;

            string otherTag = other.tag;

            PersistentObject po = other.gameObject.GetComponent<PersistentObject>() as PersistentObject;
            if (po!=null)   //It's persistent, so it can travel to the next room
            {
                if (!po.isRelated) //Make sure the object itself is persistent, and isn't only persistent because it was specified as being related to another persistent object. If it is related, then the object that specified it as related will take it when it goes to the map.
                {
                    Debug.Log(other.gameObject.name + " is about to be passed to sendObject");
                    global.map.sendObject(other.gameObject, goTo, po.thisMapOnly, po.id, wrapX, wrapY, warpTag);
                }
            }

            if (otherTag == "Player")
                Warp();
        }
    }

    //The function that changes the scene
    public void Warp()
    {
        Debug.Log("WARP Called for Player");
        if (triggered) return;
        triggered = true;
        global.map.goTo(goTo, transType);
    }
}
