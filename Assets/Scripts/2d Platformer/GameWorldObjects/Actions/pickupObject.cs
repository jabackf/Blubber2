using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickupObject : actionInRange
{
    private GameObject holder; //This is the character holding this object
    private FixedJoint2D joint; //This is the joint used to connect the object to the character

    public void pickMeUp(GameObject character)
    {
        this.setRangeActive(false);
        holder = character;
        Debug.Log(holder.name + " picked up " + gameObject.name);
        holder.AddComponent(FixedJoint2D);
        joint = holder.GetComponent<FixedJoint2D>();
        joint.connectedBody = gameObject.rigidbody;
    }
}
