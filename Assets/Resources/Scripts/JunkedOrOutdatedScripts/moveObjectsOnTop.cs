using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

//Add this script to moving platforms, conveyors, ect. to make objects on top move with it.
//Note: This script gets dynamically added/removed from objects that land on top of it to make them into moving platforms as well.
//Platforms of type moveWithPlatform move objects by making them child objects, then removing them when they exit collision


public class moveObjectsOnTop : MonoBehaviour
{
    public enum platformType { disabled, moveWithPlatform, conveyor};
    public platformType myType = platformType.moveWithPlatform;

    private bool addedDynamically = false;  //Used to determine whether the script instance was added dynamically or not
    private bool firstFrameOfScript = true;  //If this script is added dynamically, it may do so after collisionenter events and some important code may not run. We use this boolean to do some extra checks for this.

    private struct parentEntry
    {
        public GameObject gameObject;
        public Transform previous;
    }

    private List<parentEntry> parents = new List<parentEntry>();

    void LateUpdate()
    {
        firstFrameOfScript = false;
    }

    void FixedUpdate()
    {

    }

    public void checkNewCollision(Collision2D other)
    {
        if (myType != platformType.disabled)
        {
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log(other.gameObject.name + " new col check1 - velocity: " + rb.velocity.y);
                if (!rb.isKinematic && rb.simulated && rb.IsAwake() && other.gameObject.transform.position.y > gameObject.transform.position.y)// && rb.velocity.y <= 0)
                {
                    Debug.Log(other.gameObject.name + " new col check2, does it need script?");
                    moveObjectsOnTop script = other.gameObject.GetComponent<moveObjectsOnTop>();
                    if (script == null)
                    {
                        Debug.Log(other.gameObject.name + " is getting the script from " + gameObject.name);
                        script = other.gameObject.AddComponent(typeof(moveObjectsOnTop)) as moveObjectsOnTop;
                        script.setAddedDynamically(true);
                        script.myType = platformType.moveWithPlatform;
                        if (myType == platformType.moveWithPlatform)
                        {
                            Debug.Log(other.gameObject.name + " is getting childed to " + gameObject.name);
                            parentEntry e = new parentEntry();
                            e.previous = other.gameObject.transform.parent;
                            e.gameObject = other.gameObject;
                            parents.Add(e);

                            other.gameObject.transform.parent = gameObject.transform;
                        }
                    }
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        checkNewCollision(other);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        foreach (parentEntry e in parents)
        {
            if (e.gameObject == other.gameObject)
            {
                e.gameObject.transform.parent = e.previous;
                Debug.Log(e.gameObject.name + " is getting un-childed from" + gameObject.name);
            }
        }
        parents.RemoveAll(parentEntry => parentEntry.gameObject == other.gameObject);

        if (other.gameObject.transform.position.y > gameObject.transform.position.y)
        {
            moveObjectsOnTop script = other.gameObject.GetComponent<moveObjectsOnTop>();
            if (script != null)
            {
                if (script.addedDynamically)
                {
                    script.resetParents();
                    Destroy(script);
                    Debug.Log(other.gameObject.name + " is losing its script from " + gameObject.name);
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (firstFrameOfScript)
        {
            Debug.Log(other.gameObject.name + " is getting a first frame collision check from "+gameObject.name);
            checkNewCollision(other);
        }
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (!rb.isKinematic && rb.simulated && rb.IsAwake())
            {
                if (other.gameObject.transform.position.y > gameObject.transform.position.y && rb.velocity.y >= 0) //Only move objects that are on top
                {
                    if (myType == platformType.conveyor)
                    {

                    }
                }
            }
        }

    }
   
    public void setAddedDynamically(bool value)
    {
        addedDynamically = value;
    }

    public void resetParents()
    {
        Debug.Log(gameObject.name + " is having all parents reset");
        foreach (parentEntry e in parents)
        {
            e.gameObject.transform.parent = e.previous;
            moveObjectsOnTop script = e.gameObject.GetComponent<moveObjectsOnTop>();
            if (script != null)
            {
                if (script.addedDynamically)
                {
                    script.resetParents();
                }
            }
        }

        parents.Clear();
    }
}