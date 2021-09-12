using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script simply tracks objects that are inside of the attached trigger for easy interaction with outside objects.
//You can request a list of all GameObjects currently inside of the trigger, or you can ask it if a specific object is inside the trigger.

public class getTriggerObjects : MonoBehaviour
{
    public LayerMask mask;
    public List<GameObject> objects = new List<GameObject>();
    
    public List<GameObject> getObjects()
    {
        return objects;
    }

    public bool isInsideTrigger(GameObject go)
    {
        return objects.Contains(go);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            objects.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        objects.Remove(other.gameObject);
    }
}
