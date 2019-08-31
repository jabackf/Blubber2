using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickupObject : actionInRange
{
    private GameObject holder; //This is the character holding this object

    public void pickMeUp(GameObject character)
    {
        this.setRangeActive(false);
        holder = character;
        Debug.Log(holder.name + " picked up " + gameObject.name);
    }
}
