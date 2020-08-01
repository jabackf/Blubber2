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
    bool flippedX = false;  //Tracks if the holding character is currently flipped horizontally
    float releaseTimer = 0; //Timer. When set, the object will be released after it hits zero. This fixes a bug with attempting to pickup an object when something, like a ceiling, is in the way
    float releaseWaitTime = 0.015f; //The default time that the timer above will be set to.
    private bool undroppable = false; //Used to prevent the item from being dropped. Doesn't prevent an item from being thrown. This should be set using makeUndroppable!

    private SpriteRenderer renderer;

    private Vector3 refVelocity = new Vector3(0, 0, 0);
    private bool initialRotationFreeze;
    private float initialZRotation;

    private Transform parentPrevious = null; //Stores the previous parent for exactPlayerPosition. This is because exactPlayerPosition actually parents the object to the holder instead of creating a joint and following

    //Top = carry with the character's top transform. Front = carry with the player's front transform. exactPlayerPosition = parent the object to the player and follow his position exactly, don't use a joint, don't smoothDamp, kinematic physics while held.
    public enum carryType { Top, Front, exactPlayerPosition };
    public carryType mCarryType = carryType.Top; //Rather we carry this item on top or in front

    public enum releasePositions { None, Front, Top, Bottom }; //The character-relative positions that the this object can jump to on release. For example, if Front is selected, then on release the object will jump the character's Front position.
    public releasePositions releasePosition = releasePositions.None;

    [Space]
    [Header("Sprites")]
    public bool hasHeldSprite = false;  //If true, the script will use a different sprite for when the object is held (heldSprite) vs when it is unheld (unheldSprite). Note that facing direction sprites will over-ride heldSprite, and heldSprite does not need to be specified if you're using facingDirection sprites as well.
    public Sprite heldSprite, unheldSprite;
    public bool hasFacingDirections = false;
    public Sprite sideSprite, frontSprite, backSprite;

    [Space]
    [Header("Item Use Action")]
    public bool hasAction = false;
    public bool hasActionAim = false;     //If set to true, an aiming reticle will be displayed when the useAction button is held
    public bool rotateWithAim = true;   //If set to true and hasActionAim is true, then the gameObject will be rotated with the aim reticle
    private GameObject actionAimObj;
    public lineArrow actionAimArc;
    public string actionPressedSendMessage = "";
    public GameObject actionPressedMessageReceiver;
    public string actionHeldSendMessage = "";
    public GameObject actionHeldMessageReceiver;
    public string actionReleasedSendMessage = "";
    public GameObject actionReleasedMessageReceiver;

    [SerializeField]
    public UnityEvent ActionPressedCallback;  //Called when the object gets a keypress signal from the use object input
    public UnityEvent ActionHeldCallback;
    public UnityEvent ActionReleasedCallback;

    [SerializeField]
    public UnityEvent OnPickupCallback;  //Called when the object is picked up

    [SerializeField]
    public UnityEvent OnReleaseCallback;  //Called when the object is dropped or thrown


    //This is set to some number greater than one upon picking an item up. It then counts down and hits zero a couple frames after picking it up.
    //This was added as a hacky fix for a glitch I was having with the action aim arrows not pointing the proper direction after first picking them up.
    //It's a bad fix, but I don't care! It works!
    private float justPickedUp = 0f;

    void Start()
    {
        base.Start();
        rb = gameObject.GetComponent<Rigidbody2D>() as Rigidbody2D;
        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        initialMass = rb.mass;
        createThrowArrow();

        if (hasAction && hasActionAim)
        {
            createActionArrow();
        }

        initialOffset = offset;
        initialRotationFreeze = rb.freezeRotation;
        initialZRotation = gameObject.transform.eulerAngles.z;
    }

    //Since the throw and action arrows are not child objects, these next three functions check if they exist and create them. We'll call checkForArrow anytime we know that we need one of the arrows, in case we've changed scenes and lost our arrow objects
    private void createThrowArrow()
    {
        throwArcObj = new GameObject(gameObject.name + "_throwArc");
        throwArcObj.transform.localPosition = new Vector3(0, 0, 0);
        throwArc = throwArcObj.AddComponent<lineArrow>() as lineArrow;
        throwArc.follow(gameObject.transform);
        throwArc.hide();
    }
    private void createActionArrow()
    {
        actionAimObj = new GameObject(gameObject.name + "_actionAimObj");
        actionAimObj.transform.localPosition = new Vector3(0, 0, 0);
        actionAimArc = actionAimObj.AddComponent<lineArrow>() as lineArrow;
        actionAimArc.follow(gameObject.transform);
        actionAimArc.reticleMode = true;
        actionAimArc.setAngle(0);
        actionAimArc.hide();
    }

    private void checkForArrows()
    {
        if (throwArcObj == null) createThrowArrow();
        if (actionAimObj == null && hasAction && hasActionAim) createActionArrow();
    }

    //This function hides our throw and action aim arrows
    public void hideArrows()
    {
        checkForArrows();
        throwArc.hide();
        if (hasAction && hasActionAim) actionAimArc.hide();
    }

    //Use the throwing retical. Called by the holding object.
    //Arguments = Horizontal movement, vertical movement, release and throw, use the object's action
    public void Aim(float h, float v, bool release, bool action)
    {
        checkForArrows();

        if (hasAction && hasActionAim)
        {
            if (actionAimArc != null) actionAimArc.hide();
        }

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
            throwArc.setLength(throwArc.length + h);
        }
    }

    //This function controls the actionAim arrow
    public void actionAim(float h, float v)
    {
        checkForArrows();

        if (!hasAction || !hasActionAim) return;

        //Throw arc will take priority. If the user tries to throw while holding the action button down, we cancel the action aiming
        if (throwArc.isShowing())
        {
            actionAimArc.hide();
            return;
        }
        else
        {
            if (!actionAimArc.isShowing())
            {
                actionAimArc.show();
                //actionAimArc.setAngle(flippedX ? actionAimArc.angle : actionAimArc.angle );
                actionAimArc.setMinMax(flippedX ? 0 : 90, flippedX ? 90 : 180);
            }

            actionAimArc.setAngle(actionAimArc.angle += (flippedX ? v : -v));
            actionAimArc.setLength(actionAimArc.length + h);

            if (rotateWithAim) gameObject.transform.eulerAngles = new Vector3(0f, 0f, flippedX ? actionAimArc.angle : actionAimArc.angle + 180);
        }
    }

    //First three variables indicate the state of the action button (pressed, held, released). Last two variables are for aiming the useAction reticle (if there is one)
    public void useItemAction(bool pressed, bool held, bool released, float horizontal = 0f, float vertical = 0f)
    {
        if (!hasAction) return;

        if (pressed)
        {
            if (ActionPressedCallback != null) ActionPressedCallback.Invoke();
            if (actionPressedMessageReceiver != null && actionPressedSendMessage != "")
            {
                actionPressedMessageReceiver.SendMessage(actionPressedSendMessage);
            }


        }
        else if (released)
        {
            if (ActionReleasedCallback != null) ActionReleasedCallback.Invoke();
            if (actionReleasedMessageReceiver != null && actionReleasedSendMessage != "")
            {
                actionReleasedMessageReceiver.SendMessage(actionReleasedSendMessage);
            }

            if (hasActionAim) actionAimArc.hide();
        }
        else if (held)
        {
            if (ActionHeldCallback != null) ActionHeldCallback.Invoke();
            if (actionHeldMessageReceiver != null && actionHeldSendMessage != "")
            {
                actionHeldMessageReceiver.SendMessage(actionHeldSendMessage);
            }

            if (hasActionAim) actionAim(horizontal, vertical);
        }
    }


    public void pickMeUp(GameObject character, Transform top, Transform front)
    {
        justPickedUp = 2;

        this.setRangeActive(false);
        rb.mass = carryMass;
        holder = character;

        if (hasHeldSprite && heldSprite != null) renderer.sprite = heldSprite;

        if (hasFacingDirections) setFacingDirection(holder.GetComponent<CharacterController2D>().getFacingDirection());

        if (mCarryType == carryType.Top)
            carryTrans = top;
        if (mCarryType == carryType.Front)
            carryTrans = front;
        if (mCarryType == carryType.exactPlayerPosition)
        {
            parentPrevious = gameObject.transform.parent;
            gameObject.transform.parent = character.transform;
            gameObject.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            carryTrans = null;
            joint = null;
            disablePhysics();
        }
        else
        {
            joint = gameObject.AddComponent<FixedJoint2D>() as FixedJoint2D;
            joint.connectedBody = holder.GetComponent<Rigidbody2D>() as Rigidbody2D;
            joint.anchor = new Vector2(carryTrans.position.x + offset.x, carryTrans.position.y + offset.y);
            joint.breakForce = this.breakForce;
            joint.breakTorque = this.breakTorque;
        }


        if (resetRotationOnPickup)
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, initialZRotation);
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

        checkForArrows();
        if (hasActionAim && hasAction)
        {
            actionAimArc.setAngle(flippedX ? 180 : 0);
        }

        if (OnPickupCallback != null) OnPickupCallback.Invoke();

    }

    //Makes the joint unbreakable (meaning you can't accidentally drop it. It can still be thrown)
    public void makeUndroppable()
    {
        if (joint)
        {
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            undroppable = true;
        }
    }
    //Undoes the previous function by reverting the break force/torque back to the most recently specified values
    public void makeDroppable()
    {
        if (joint)
        {
            joint.breakForce = this.breakForce;
            joint.breakTorque = this.breakTorque;
            undroppable = false;
        }
    }

    //Called if a holder takes the item to another scene. Called after the new scene finishes loading.
    public void changedScenes()
    {
        hideArrows();
        if (hasAction && hasActionAim)
        {
            actionAimArc.setAngle(flippedX ? 0 : 180);
        }
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
        if (justPickedUp > 0) justPickedUp -= 1;

        if (holder != null)
        {
            if (mCarryType == carryType.exactPlayerPosition)
            {
                gameObject.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            }
            else
                gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, carryTrans.position + new Vector3(offset.x, offset.y, 0), ref refVelocity, 0.1f);

            if (releaseTimer > 0)
            {
                releaseTimer -= Time.fixedDeltaTime;
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
        if (mCarryType == carryType.exactPlayerPosition)
        {
            enablePhysics();
            gameObject.transform.parent = parentPrevious;
        }

        if (hasHeldSprite && unheldSprite != null) renderer.sprite = unheldSprite;

        checkForArrows();

        CharacterController2D cc2d = holder.GetComponent<CharacterController2D>();
        if (cc2d)
        {
            switch (releasePosition)
            {
                case releasePositions.Front:
                    transform.position = cc2d.getFrontPosition();
                    break;
                case releasePositions.Top:
                    transform.position = cc2d.getTopPosition();
                    break;
                case releasePositions.Bottom:
                    transform.position = cc2d.getBottomPosition();
                    break;
            }
        }

        if (freezeRotationOnPickup) rb.freezeRotation = initialRotationFreeze;
        Destroy(joint);
        this.setRangeActive(true);
        throwArc.hide();
        if (hasActionAim && hasAction)
        {
            actionAimArc.setAngle(0);
            actionAimArc.hide();
        }

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
        checkForArrows();
        if (!flippedX)
        {
            throwArc.setMinMax(90, 180);
            if (hasActionAim) actionAimArc.setMinMax(90, 180);
        }
        else
        {
            throwArc.setMinMax(0, 90);
            if (hasActionAim) actionAimArc.setMinMax(0, 90);
        }
        throwArc.setAngle( (180-throwArc.angle) );

        if (hasAction && hasActionAim)
        {
             if (justPickedUp==0) actionAimArc.setAngle(180 - actionAimArc.angle);
             else actionAimArc.setAngle(flippedX ? 0 : 180);
        }

        if (rotateWithAim && hasActionAim && hasAction) gameObject.transform.eulerAngles = new Vector3(0f, 0f, flippedX ? actionAimArc.angle : actionAimArc.angle + 180);
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

    //Sets the facing direction of the object. 0=side, 1=front, 2=back. Called by the character controller that is currently holding it
    public void setFacingDirection(int dir)
    {
        if (hasFacingDirections)
        {
            if (renderer == null) renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
            if (dir == 0 && sideSprite != null) renderer.sprite = sideSprite;
            if (dir == 1 && frontSprite != null) renderer.sprite = frontSprite;
            if (dir == 2 && backSprite != null) renderer.sprite = backSprite;
        }
    }

    public GameObject getHolder()
    {
        return holder;

    }

    void OnDestroy()
    {
        if (holder != null) releaseFromHolder();
    }
}
