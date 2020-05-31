using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class pickupObject : actionInRange
{
    private GameObject holder; //This is the character holding this object
    private FixedJoint2D joint; //This is the joint used to connect the object to the character
    private Transform carryTrans;  //The transform used for carrying objects
    public Vector2 offset = new Vector2(0, 0.4f);  //The offset for carrying objects on top
    private Vector2 initialOffset;
    public bool disableCollider = true; //if true, the collider will be changed to a trigger when the object is carried.
    public float breakForce = 500;  //The amount of force needed to break the joint between the holding character this object
    public float breakTorque = 500;
    public float throwForce = 200; //This number is multiplied by length of the throw arrow.
    public float carryMass = 0.5f;  //This is the mass of the object while it's being carried
    public bool flipOnX = true;     //Flips with the character holding it if set to true
    public bool flipOnY = true;
    public bool freezeRotationOnPickup = true; //If true, the object's ability to rotate on the Z axis will freeze when picked up. When dropped, it's ability to rotate will be reset to whatever it was previously
    public bool resetRotationOnPickup = true; //If true, the object's Z rotation will be set to whatever it was at initialization when picked up
    private float initialMass;  //Stores the intial mass so we can change the mass back when we release it.
    private Rigidbody2D rb;
    public lineArrow throwArc;   //The reference to the lineArrow script
    private GameObject throwArcObj;    //The reference to the object containing the lineArrow script
    bool flippedX=false;  //Tracks if the holding character is currently flipped horizontally
    float releaseTimer = 0; //Timer. When set, the object will be released after it hits zero. This fixes a bug with attempting to pickup an object when something, like a ceiling, is in the way
    float releaseWaitTime = 0.015f; //The default time that the timer above will be set to.
    private bool undroppable = false; //Used to prevent the item from being dropped. Doesn't prevent an item from being thrown. This should be set using makeUndroppable!

    private Vector3 refVelocity = new Vector3(0,0,0);
    private bool initialRotationFreeze;
    private float initialZRotation;

    public enum carryType { Top, Front};
    public carryType mCarryType = carryType.Top; //Rather we carry this item on top or in front

    [Space]
    [Header("Item Use Action")]
    public bool hasAction = false;
    public string actionSendMessage = "";
    public GameObject actionMessageReceiver;

    [SerializeField]
    public UnityEvent ActionCallback;  //Called when the object gets a keypress signal from the use object input

    [SerializeField]
    public UnityEvent OnPickupCallback;  //Called when the object is picked up

    [SerializeField]
    public UnityEvent OnReleaseCallback;  //Called when the object is dropped or thrown

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
        initialOffset = offset;
        initialRotationFreeze = rb.freezeRotation;
        initialZRotation = gameObject.transform.eulerAngles.z;
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

    public void useItemAction()
    {
        if (!hasAction) return;
        if (ActionCallback!=null) ActionCallback.Invoke();
        if (actionMessageReceiver != null && actionSendMessage != "")
        {
            actionMessageReceiver.SendMessage(actionSendMessage);
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

        if (resetRotationOnPickup)
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y,initialZRotation);
        if (freezeRotationOnPickup)
            rb.freezeRotation = true;

        if (disableCollider) //We're not using a collider for this item
        {
            gameObject.GetComponent<Collider2D>().isTrigger = true;

        }
        else //We are using it. Better make sure there's nothing in the way of us picking it up, like a ceiling above the character
        {
            RaycastHit2D[] hit = new RaycastHit2D[10];
            Vector2 v = ((Vector2)carryTrans.position + offset) - (Vector2)gameObject.transform.position;
            var dis = v.magnitude; //Distance
            var dir = v / dis;  //Direction
            int count = rb.Cast(dir, hit, dis);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {

                    //Check to see if something is in the way, and drop it if so.
                    if (hit[i].collider.attachedRigidbody == null && !hit[i].collider.isTrigger)
                    {
                        releaseTimer = releaseWaitTime;
                    }
                }
            }
        }

        if (OnPickupCallback != null) OnPickupCallback.Invoke();
    }

    //Makes the joint unbreakable (meaning you can't accidentally drop it. It can still be thrown)
    public void makeUndroppable()
    {
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        undroppable = true;
    }
    //Undoes the previous function by reverting the break force/torque back to the most recently specified values
    public void makeDroppable()
    {
        joint.breakForce = this.breakForce;
        joint.breakTorque = this.breakTorque;
        undroppable = false;
    }

    //This function will suspend the physics movements of the object
    public void disablePhysics()
    {
        rb.isKinematic = true;  // Deactivated
    }
    public void enablePhysics()
    {
        rb.isKinematic = false;  // Activated
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

    public void releaseFromHolder()
    {
        if (freezeRotationOnPickup) rb.freezeRotation = initialRotationFreeze;
        Destroy(joint);
        this.setRangeActive(true);
        throwArc.hide();

        holder.SendMessage("pickupReleased");
        holder = null;
        rb.mass = initialMass;
        if (disableCollider)
            gameObject.GetComponent<Collider2D>().isTrigger = false;

        if (OnReleaseCallback != null) OnReleaseCallback.Invoke();
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
            offset.x = initialOffset.x * (flipX ? -1 : 1);
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
            offset.y = initialOffset.y * (flipY ? -1 : 1);
            //offset.y= -offset.y;
            gameObject.GetComponent<SpriteRenderer>().flipY = flipY;
        }
    }
}
