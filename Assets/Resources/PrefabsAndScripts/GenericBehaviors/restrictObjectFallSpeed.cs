using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attach this to objects that have the ability to restrict another object's fall speed.
//For example, apply it to the chicken and configure it to restrict the player's fall speed while the chicken is held.

public class restrictObjectFallSpeed : MonoBehaviour
{
    [System.Serializable]
    public enum appliesToTypes
    {
        restrictGo, //Apply to the object specified in restrictGo variable (either assigned in editor or assigned through enable(restrictObject))
        objectWithPlayerTag, //Find the object with the player tag and assign it to that
        Holder  //If this object has a pickupObject script, get that and assign it to the holder object
    }

    public appliesToTypes aplliesTo = appliesToTypes.restrictGo;

    public GameObject restrictGo = null; //The object to restrict
    private Rigidbody2D rb;
    bool restrict = false; //Turns the restriction on and off
    public float minFallVelocity = -3f;
    private pickupObject po;

    public void Start()
    {
        if (restrictGo) getRigidbody();
        po = gameObject.GetComponent<pickupObject>();
    }

    public void Update()
    {
        if (restrict && restrictGo && rb)
        {
            if (rb.velocity.y < minFallVelocity) rb.velocity = new Vector2(rb.velocity.x, minFallVelocity);
        }
    }

    public void getRigidbody()
    {
        rb = restrictGo.GetComponent<Rigidbody2D>();
    }

    public void enable(GameObject restrictObject)
    {
        restrictGo = restrictObject;
        enable();
    }
    public void enable()
    {
        if (aplliesTo == appliesToTypes.objectWithPlayerTag) restrictGo = GameObject.FindWithTag("Player");
        if (aplliesTo == appliesToTypes.Holder)
        {
            if (po)
            {
                restrictGo = po.getHolder();
            }
        }

        if (restrictGo)
        {
            if (restrictGo) getRigidbody();
            restrict = true;
        }
    }
    public void disable()
    {
        restrict = false;
    }
}
