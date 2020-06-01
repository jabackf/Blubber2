/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Objects with this script will be registered as persistent on creation

public class PersistentObject : MonoBehaviour
{

    public bool thisMapOnly = false;    //Map strings have the format mapName_xpos_ypos. If thisMapOnly is set to true, the object's persistence ends when the mapName portion (excluding xpos_ypos) is changed. 
                                        //If false, the object will be persistent for the entire game. MapName in this case, refers to the map where the object was created.

    public bool duplicateOnSceneReload = false;  //This option changes how IDs are assigned (position hash on false, random hash on true). Basically, if you take an object out of the scene it was created in and return to that scene later, this option will effect the behavior.
                                                   //If set to true, the object will be created again leaving both the persistent and new object. If false, the object recreated on the new scene load will be destroyed and the persistent object which left the scene and returned will be perserved.
                                                   //In general you want to set this to false for objects that were placed manually into the scene, and true for objects that are generated dynamically and repeatedly (such as a pipe that is continually spawing new items)

    private Global global;
    [HideInInspector]public float id; //The position hash serves as a unique ID that distinguishes the object from other registered persistent objects. It is generated based on x,y, and time since scene load.
    private bool unregistered = false; //Set to true if the object tried to register as persistent but it couldn't because it already exists in the global persistent register

    private List<GameObject> relatedObjects;    //These are related persistent objects. These are marked as persistent and kept with the object across maps. This is for objects that BELONG to this object, but aren't children of the object. They're persistent by bent of belonging to a persistent object. Related objects use lose their persistence when the object it is related to gets destroyed.

    [HideInInspector] public bool isRelated = false; //This is set to true if this object was specified as a relatedObject by another persisten object 

    // Start is called before the first frame update
    void Start()
    {
        if (!duplicateOnSceneReload)
            id = (1000 * transform.position.x) + transform.position.y;
        else
            id = UnityEngine.Random.Range(-1000.000f, 10000.000f);
        Debug.Log(gameObject.name + " - " + id);
        global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;
        if (!global.registerPersistentObject(gameObject, global.map.currentMap, thisMapOnly, id))
        {
            unregistered = true;
            Destroy(gameObject);
        }
    }

    //This function gets called whenever this object gets sent to another map. It sends related objects along with the object.
    public void poSentToMap(string map, bool wrapX, bool wrapY, string warpTag)
    {
        Debug.Log(gameObject.name + " poSentToMap Called");
        if (relatedObjects != null)
        {
            foreach (GameObject go in relatedObjects)
            {
                Debug.Log("poSentToMap from " + gameObject.name + " - cycling through related object: " + go.name);
                PersistentObject po = go.GetComponent<PersistentObject>() as PersistentObject;
                if (po != null)
                {
                    global.map.sendObject(go, map, po.thisMapOnly, po.id, wrapX, wrapY, warpTag);
                }
                else Debug.Log("PO script was not found!" + go.name);
            }
        }
    }

    //Registers a related gameobject as persistent using the same settings as this gameobject. For example, this might be called for an object being held by a character
    //If this object is destroyed or sent to another map, all related object will go too.
    public void registerRelated(PersistentObject po)
    {
        //if (unregistered && !po.isRelated) return;
        //Debug.Log(gameObject.name + " received a registerRelated request from "+go.name);
        Debug.Log("Adding1 "+po.gameObject.name);
        relatedObjects.Add(po.gameObject);
        po.relatedObjects.Add(gameObject);
        Debug.Log("Added");
        /*PersistentObject po = go.GetComponent<PersistentObject>() as PersistentObject;
        if (po == null)
        {
            go.AddComponent<PersistentObject>();
            po = go.GetComponent<PersistentObject>() as PersistentObject;
        }*/
    //po.thisMapOnly = thisMapOnly;
    //po.id = id;
   // po.isRelated = true;
    //Debug.Log("Register Related " + go.name);
/*
    }

    public void unregisterRelated(PersistentObject po)
    {
        //Debug.Log("UnRegistering Related.. " + go.name);
        //po.isRelated = false;
        relatedObjects.Remove(po.gameObject);
        po.relatedObjects.Remove(gameObject);  
        //Debug.Log("UnRegistered Related " + go.name);
    }

    void OnDestroy()
    {
        if (relatedObjects != null)
        {
            foreach (GameObject go in relatedObjects)
            {
                if (go != null)
                {
                    PersistentObject po = go.GetComponent<PersistentObject>() as PersistentObject;
                    if (po != null)
                    {
                        if (po.id == id)
                        {
                            global.unregisterPersistentObject(go, po.id);
                            Destroy(po);
                        }
                    }
                }
            }
        }
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
        if (!unregistered)
            global.unregisterPersistentObject(gameObject,id);
    }
}
*/