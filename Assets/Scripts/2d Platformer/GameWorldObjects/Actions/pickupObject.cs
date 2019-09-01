using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickupObject : actionInRange
{
    private GameObject holder; //This is the character holding this object
    private FixedJoint2D joint; //This is the joint used to connect the object to the character
    private Transform carryTrans;  //The transform used for carrying objects
    public Vector2 offset = new Vector2(0, 0.4f);  //The offset for carrying objects on top
    public bool disableCollider = true; //if true, the collider will be changed to a trigger when the object is carried.
    public float breakForce = 500;  //The amount of force needed to break the joint between the holding character this object
    public float breakTorque = 500;
    public float carryMass = 0.5f;  //This is the mass of the object while it's being carried
    public bool flipOnX = true;     //Flips with the character holding it if set to true
    public bool flipOnY = true;
    private float initialMass;  //Stores the intial mass so we can change the mass back when we release it.
    private Rigidbody2D rb;

    private Vector3 refVelocity = new Vector3(0,0,0);

    public enum carryType { Top, Front};
    public carryType mCarryType = carryType.Top; //Rather we carry this item on top or in front

    void Start()
    {
        base.Start();
        rb = gameObject.GetComponent<Rigidbody2D>() as Rigidbody2D;
        initialMass = rb.mass;
    }

    public void pickMeUp(GameObject character, Transform top, Transform front)
    {
        this.setRangeActive(false);
        rb.mass = carryMass;
        holder = character;
        Debug.Log(holder.name + " picked up " + gameObject.name);
        if (mCarryType == carryType.Top)
            carryTrans = top;
        if (mCarryType == carryType.Front)
            carryTrans = front;
        //Debug.Log(front +" - "+top);
        //joint = holder.AddComponent<FixedJoint2D>() as FixedJoint2D;
        //joint.connectedBody = gameObject.GetComponent<Rigidbody2D>() as Rigidbody2D;
        joint = gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
        joint.connectedBody = holder.GetComponent<Rigidbody2D>() as Rigidbody2D;
        joint.anchor = new Vector2(carryTrans.position.x+offset.x, carryTrans.position.y+offset.y);
        joint.breakForce = this.breakForce;
        joint.breakTorque = this.breakTorque;

        if (disableCollider)
            gameObject.GetComponent<Collider2D>().isTrigger = true;
        //gameObject.transform.position = new Vector3(top.x, top.y, 0);
    }

    void FixedUpdate()
    {
        if (holder != null)
        {
            gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, carryTrans.position+new Vector3(offset.x,offset.y,0), ref refVelocity, 0.1f);
        }
    }

    void OnJointBreak2D(Joint2D broken)
    {
        if (broken == joint)
        {
            releaseFromHolder();
        }
    }

    void releaseFromHolder()
    {
        this.setRangeActive(true);
        holder.SendMessage("pickupReleased");
        holder = null;
        rb.mass = initialMass;
        if (disableCollider)
            gameObject.GetComponent<Collider2D>().isTrigger = false;
    }

    public void changeTransform(Transform newT)
    {
        carryTrans = newT;
    }
    public void changeFrontTransform(Transform newT)
    {
        if (mCarryType == carryType.Front)
        {
            carryTrans = newT;
        }
    }
    public void flipSpriteX(bool flipX)
    {
        if (flipOnX)
        {
            offset.x = -offset.x;
            gameObject.GetComponent<SpriteRenderer>().flipX = flipX;
        }
    }
    public void flipSpriteY(bool flipY)
    {
        if (flipOnY)
        {
            offset.y= -offset.y;
            gameObject.GetComponent<SpriteRenderer>().flipY = flipY;
        }
    }
}
