using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Objects with this script will be registered as persistent on creation

public class PersistentObject : MonoBehaviour
{

    public bool thisMapOnly = false;    //Map strings have the format mapName_xpos_ypos. If thisMapOnly is set to true, the object's persistence ends when the mapName portion (excluding xpos_ypos) is changed. 
                                        //If false, the object will be persistent for the entire game. MapName in this case, refers to the map where the object was created.

    private Global global;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(gameObject);
        global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;
        global.registerPersistentObject(gameObject, global.map.currentMap, thisMapOnly);
        
    }

    void OnDestroy()
    {
        global.unregisterPersistentObject(gameObject);
    }
}
