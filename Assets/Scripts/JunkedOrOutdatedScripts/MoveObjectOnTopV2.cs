using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

//Add this script to moving platforms, conveyors, ect. to make objects on top move with it.
//Note: This script gets dynamically added/removed from objects that land on top of it to make them into moving platforms as well.
//Platforms of type moveWithPlatform move objects by making them child objects, then removing them when they exit collision


public class moveObjectsOnTopV2 : MonoBehaviour
{
    public enum platformType { disabled, moveWithPlatform, conveyor };
    public platformType myType = platformType.moveWithPlatform;
    public float breakForce = 10f;
    public float breakTorque= 10f;

    private bool addedDynamically = false;  //Used to determine whether the script instance was added dynamically or not
    private bool firstFrameOfScript = true;  //If this script is added dynamically, it may do so after collisionenter events and some important code may not run. We use this boolean to do some extra checks for this.
    private FixedJoint2D joint = null;

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
                    moveObjectsOnTopV2 script = other.gameObject.GetComponent<moveObjectsOnTopV2>();
                    if (script == null)
                    {
                        Debug.Log(other.gameObject.name + " is getting the script from " + gameObject.name);
                        script = other.gameObject.AddComponent(typeof(moveObjectsOnTopV2)) as moveObjectsOnTopV2;
                        script.setAddedDynamically(true);
                        script.myType = platformType.moveWithPlatform;
                        if (myType == platformType.moveWithPlatform)
                        {
                            script.joint = other.gameObject.AddComponent(typeof(FixedJoint2D)) as FixedJoint2D;
                            script.joint.connectedBody = gameObject.GetComponent<Rigidbody2D>();
                            script.joint.breakForce = breakForce;
                            script.joint.breakTorque = breakTorque;
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
        if (other.gameObject.transform.position.y > gameObject.transform.position.y)
        {
            moveObjectsOnTopV2 script = other.gameObject.GetComponent<moveObjectsOnTopV2>();
            if (script != null)
            {
                if (script.addedDynamically)
                {
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
            Debug.Log(other.gameObject.name + " is getting a first frame collision check from " + gameObject.name);
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

}