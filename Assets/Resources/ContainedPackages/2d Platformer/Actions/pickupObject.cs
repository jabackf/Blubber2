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
    public float throwForce = 200; //This number is multiplied by length of the throw arrow.
    public float carryMass = 0.5f;  //This is the mass of the object while it's being carried
    public bool flipOnX = true;     //Flips with the character holding it if set to true
    public bool flipOnY = true;
    private float initialMass;  //Stores the intial mass so we can change the mass back when we release it.
    private Rigidbody2D rb;
    public lineArrow throwArc;   //The reference to the lineArrow script
    private GameObject throwArcObj;    //The reference to the object containing the lineArrow script
    bool flippedX=false;  //Tracks if the holding character is currently flipped horizontally
    float releaseTimer = 0; //Timer. When set, the object will be released after it hits zero. This fixes a bug with attempting to pickup an object when something, like a ceiling, is in the way
    float releaseWaitTime = 0.015f; //The default time that the timer above will be set to.

    private Vector3 refVelocity = new Vector3(0,0,0);

    public enum carryType { Top, Front};
    public carryType mCarryType = carryType.Top; //Rather we carry this item on top or in front

    void Start()
    {
        base.Start();
        rb = gameObject.GetComponent<Rigidbody2D>() as Rigidbody2D;
        initialMass = rb.mass;

        throwArcObj = new GameObject(gameObject.name + "_throwArc");
        throwArcObj.transform.parent = gameObject.transform;
        throwArcObj.transform.localPosition = new Vector3(0, 0, 0);
        throwArc = throwArcObj.AddComponent<lineArrow>() as lineArrow;
        throwArc.isChild = true;
        throwArc.hide();
        
    }

    //Use the throwing retical. Called by the holding object.
    //Arguments = Horizontal movement, vertical movement, release and throw, use the object's action
    public void Aim(float h, float v, bool release, bool action)
    {
        if (!throwArc.isShowing())
        {
            throwArc.show();
            throwArc.setAngle(flippedX ? 50 : 130);
            throwArc.setMinMax(flippedX ? 0 : 90, flippedX ? 90 : 180);
        }
        if (release)
        {
            throwItem();
        }
        else
        {

            throwArc.setAngle(throwArc.angle += (flippedX ? v : -v));
            throwArc.setLength(throwArc.length+h);
        }
    }


    public void pickMeUp(GameObject character, Transform top, Transform front)
    {
        this.setRangeActive(false);
        rb.mass = carryMass;
        holder = character;
        if (mCarryType == carryType.Top)
            carryTrans = top;
        if (mCarryType == carryType.Front)
            carryTrans = front;
        joint = gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
        joint.connectedBody = holder.GetComponent<Rigidbody2D>() as Rigidbody2D;
        joint.anchor = new Vector2(carryTrans.position.x+offset.x, carryTrans.position.y+offset.y);
        joint.breakForce = this.breakForce;
        joint.breakTorque = this.breakTorque;

        if (disableCollider) //We're not using a collider for this item
            gameObject.GetComponent<Collider2D>().isTrigger = true; 
        else //We are using it. Better make sure there's nothing in the way of us picking it up, like a ceiling above the character
        {
            RaycastHit2D[] hit = new RaycastHit2D[10];
            Vector2 v = ((Vector2)carryTrans.position + offset) - (Vector2)gameObject.transform.position;
            var dis = v.magnitude; //Distance
            var dir = v / dis;  //Direction
            int count = rb.Cast(dir, hit, dis);
            if (count > 0)
            {
                for(int i=0; i<count; i++)
                {

                    //Check to see if something is in the way, and drop it if so.
                    if (hit[i].collider.attachedRigidbody==null && !hit[i].collider.isTrigger)
                    {
                        releaseTimer = releaseWaitTime;
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (holder != null)
        {
            //var pos = rb.position;
            //pos += new Vector2(carryTrans.position.x, carryTrans.position.y) + new Vector2(offset.x,offset.y);
            //pos = Vector3.SmoothDamp(gameObject.transform.position, carryTrans.position + new Vector3(offset.x, offset.y, 0), ref refVelocity, 0.1f);
            gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, carryTrans.position+new Vector3(offset.x,offset.y,0), ref refVelocity, 0.1f);
            //rb.MovePosition(pos);
                
            //throwArc.follow(gameObject.transform);

            if (releaseTimer>0)
            {
                releaseTimer-=Time.fixedDeltaTime;
                if (releaseTimer <= 0)
                {
                    releaseFromHolder();
                    releaseTimer = 0;
                }
            }
        }
    }

    void OnJointBreak2D(Joint2D broken)
    {
        if (broken == joint)
        {
            releaseFromHolder();
        }
    }


    public void throwItem()
    {
        releaseFromHolder();
        float radAngle = (-throwArc.angle + 90) * Mathf.Deg2Rad;
        rb.AddForce(new Vector2(Mathf.Sin(radAngle), Mathf.Cos(radAngle))*throwForce*throwArc.length);
    }

    void releaseFromHolder()
    {
        Destroy(joint);
        this.setRangeActive(true);
        throwArc.hide();
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

        flippedX = !flipX ;

        //We still want to change the throw arc to the direction we're facing, even if we're not flipping the sprite.
        if (!flippedX) throwArc.setMinMax(90, 180);
        else throwArc.setMinMax(0, 90);
        throwArc.setAngle( (180-throwArc.angle) );
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
