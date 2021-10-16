using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class pickupObject : actionInRange
{
    public string name = "";

    Global global;

    private GameObject holder; //This is the character holding this object
    private FixedJoint2D joint; //This is the joint used to connect the object to the character
    private Transform carryTrans;  //The transform used for carrying objects
    public Vector2 offset = new Vector2(0, 0.4f);  //The offset for carrying objects on top
	public bool canPutInInventory = true; //Set to true for the ability to place the item in your inventory.
    private Vector2 initialOffset;
    public bool disableCollider = true; //if true, the collider will be changed to a trigger when the object is carried.
	public bool resetScaleOnRelease = true; //If true, the transform scale will be set back to what it initially was before it was picked up after release.
	private Vector3 initialScale;
    public float breakForce = 500;  //The amount of force needed to break the joint between the holding character this object
    public float breakTorque = 500;
    public float throwForce = 200; //This number is multiplied by length of the throw arrow.
    public float throwTorque = 0f; //Torque applied on throwing. This value is negated when facing left, so torque is applied in the character's facing direction.
    public float carryMass = 0.5f;  //This is the mass of the object while it's being carried
    public bool flipOnX = true;     //Flips with the character holding it if set to true
    public bool flipOnY = true;
	private bool initialFlipX = false; //This will hold the initial state of the sprite renderer flipX property
    public bool swapXFlip = false; //Set to true if your sprite by default faces left instead of right
    public bool flipWithScale = false; //If set to true, the scale will be flipped instead of the sprite renderer. flipOnX and/or Y still must be true. NOTE that you cannot use renderer flipx at all with this option checked. FlipY was not tested with this option, but it *might* work.
    public bool flipCharacterWithMouseAim = true; //If set to true and the character controller is aiming with the mouse, then the character will flip to face the mouse direction when throw or action button is held down. Flipping is handled in the character controller! This merely tells the character controller how it should behave with this object.
    public bool freezeRotationOnPickup = true; //If true, the object's ability to rotate on the Z axis will freeze when picked up. When dropped, it's ability to rotate will be reset to whatever it was previously
    public bool resetRotationOnPickup = true; //If true, the object's Z rotation will be set to whatever it was at initialization when picked up
    public float pushOutOnRotate=0f;	//This will scale the object out as it is rotated closer to vertical. For example, if set to some magnitude then the object will move outward (away from the character) as it is rotated upwards. Can be used to adjust rotation.
	public Vector2 aimOriginOffset = new Vector2(0f,0f);	//This is added to the position of the throw and aim reticles.
	private float initialMass;  //Stores the intial mass so we can change the mass back when we release it.
    private Rigidbody2D rb;
    public lineArrow throwArc;   //The reference to the lineArrow script
    private GameObject throwArcObj;    //The reference to the object containing the lineArrow script
    bool flippedX = false;  //Tracks if the holding character is currently flipped horizontally
    float releaseTimer = 0; //Timer. When set, the object will be released after it hits zero. This fixes a bug with attempting to pickup an object when something, like a ceiling, is in the way
    float releaseWaitTime = 0.015f; //The default time that the timer above will be set to.
    private bool undroppable = false; //Used to prevent the item from being dropped. Doesn't prevent an item from being thrown. This should be set using makeUndroppable!
    private GameObject recentlyThrownBy; //This gets set to the most recent holder for a few seconds after being thrown. It then gets set to null. Can be useful to tell which character threw it.
    private float recentlyThrownByTimer = 3f; //The amount of time before the recentlyThrownBy object gets cleared to null

    public float minAimNotFlipped = -20f;
    public float maxAimNotFlipped = 90f;
    public float minAimFlipped = 90f;
    public float maxAimFlipped = 200f;

    private sceneSettings sceneSettingsGO;
    private SpriteRenderer renderer;

    private Vector3 refVelocity = new Vector3(0, 0, 0);
    private bool initialRotationFreeze;
    private float initialZRotation;

    private Transform parentPrevious = null; //Stores the previous parent for exactPlayerPosition. This is because exactPlayerPosition actually parents the object to the holder instead of creating a joint and following

    public bool sendFaceMessage = false; //If true, a FaceLeft() or FaceRight() (depending on the holder facing) message will be sent to THIS gameobject on pickup

	public bool hideCharacterDress=false;	//If true, the character dresses will be set to hidden when picked up. They will then be returned to their prior state on drop off.

    //Top = carry with the character's top transform. Front = carry with the player's front transform. exactPlayerPosition = parent the object to the player and follow his position exactly, don't use a joint, don't smoothDamp, kinematic physics while held.
    public enum carryType { Top, Front, exactPlayerPosition };
    public carryType mCarryType = carryType.Top; //Rather we carry this item on top or in front

    public enum releasePositions { None, Front, Top, Bottom }; //The character-relative positions that the this object can jump to on release. For example, if Front is selected, then on release the object will jump the character's Front position.
    public releasePositions releasePosition = releasePositions.None;

    public bool changeRigidBodyType = false; //If set to true, then rigidBody type will be set to the following variable (bodyTypeWhilecarried) when picked up. It will revert to whatever it's initial bodyType was when released.
    public RigidbodyType2D bodyTypeWhileCarried = RigidbodyType2D.Dynamic; //Note that this changeRigidBodyType feature can be glitchy. When throwing an object, changing it's body type mid throw can change the way the forces act on it and create unnatural movement. I originally added this feature to change the body type of the chicken but I didn't end up using it for this reason.
    private RigidbodyType2D bodyTypeAtStart;

    [Space]
    [Header("Sprites")]
    public bool hasHeldSprite = false;  //If true, the script will use a different sprite for when the object is held (heldSprite) vs when it is unheld (unheldSprite). Note that facing direction sprites will over-ride heldSprite, and heldSprite does not need to be specified if you're using facingDirection sprites as well.
    public Sprite heldSprite, unheldSprite;
    public bool hasFacingDirections = false;
    public Sprite sideSprite, frontSprite, backSprite;
	public string sideSortingLayer="", frontSortingLayer="", backSortingLayer="";
	

    [Space]
    [Header("Sounds")]
    public AudioClip sndOnPickup;
    public AudioClip sndLoopOnPickup; //This sound will loop until the object is released
    public AudioClip sndOnThrow;
    public AudioClip sndOnActionPress;
    public AudioClip sndOnActionRelease;
    public AudioClip sndLoopOnActionHeld; //Loops for the entire time the action button is held down
    public AudioClip sndSpawnProjectile;

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

    //This is used internally. It is set to true when we send an action keypress to this script, and false when we send an action key release.
    //One of the main reasons it is useful to track this is to tell when this particular module has recieved BOTH the press and release signals from inside the release behavior code.
    //For example, if you pass player control to a projectile on key release, then use the same keypress to send control back to the player, the next time you depress the key this module will get a key release without first having recieved a key press.
    //If actionKeyPressed is false in the key release part of the code, then we know we did not recieve a key press signal and we can use that information as needed, for example, the prevent spawning another projectile immediately.
    private bool actionKeyPressed = false;

    [SerializeField]
    public UnityEvent ActionPressedCallback;  //Called when the object gets a keypress signal from the use object input
    public UnityEvent ActionHeldCallback;
    public UnityEvent ActionReleasedCallback;

    [SerializeField]
    public UnityEvent OnPickupCallback;  //Called when the object is picked up

    [SerializeField]
    public UnityEvent OnReleaseCallback;  //Called when the object is dropped or thrown

    [Space]
    [Header("Create Projectiles")]
    public GameObject spawnProjectilePrefab = null; //The projectile to spawn. If null, nothing will be spawned.
    public GameObject spawnProjectileParticles = null;
    public float spawnProjectileRate = 0.5f; //The rate the projectile will be spawned if spawnProjectileBehavior is set to spawnWhileHeld
    public float spawnProjectileDistance = 0.8f; //The distance from this gameObject to create the projectile (along the angle of rotation)
    public Vector2 spawnProjectileOffset = new Vector2(0f, 0f); //An optional positioning offset that can be applied to the spawn position
	public Transform projectileSpawnTransform; //If supplied, distance and aim angle will not be used for positioning and the projectile will instead be spawned at this transform point.
	public float spawnProjectileForce = 0f;	//Optionally impart impulse force in aim direction on the projectile at spawn
    public bool playerControlledProjectile = false; //Set to true if the player controls the projectile (like a guided missile). This sends a "initiatePlayerControl(GameObject character)" message to the projectile after it's created, so the projectile should implement this function to take control of input. 
    public bool setProjectileRotation = true; //If true, and if we have checked hasActionAim, then the projectile's transform.rotation will be set to the angle of our aim
    public bool setParticleRotation = false; //If true, the particles are rotated and flipped to match the weapon
    public enum spawnProjectileBehaviors
    {
        spawnOnPress, //Spawns on action key press
        spawnOnRelease, //Spawns on action key release
        spawnWhileHeld, //Repeatedly spawns at spawnProjectileRate as long as the key is held down. Note: You cannot do playerControlledProjectile if you select this behavior.
		none			//This doesn't spawn the projectile at all. Instead, another script will have to invoke spawnProjectile() to create the projectile.
	}
    public spawnProjectileBehaviors spawnProjectileBehavior;

    public float projectileCamshakeIntensity = 0f, projectileCamshakeDuration = 0f; //You can use these to shake the camera when firing a projectile. Both need to be set to a value greater than zero to work.

    //This is set to some number greater than one upon picking an item up. It then counts down and hits zero a couple frames after picking it up.
    //This was added as a hacky fix for a glitch I was having with the action aim arrows not pointing the proper direction after first picking them up.
    //It's a bad fix, but I don't care! It works!
    private float justPickedUp = 0f;

    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();

        base.Start();
        rb = gameObject.GetComponent<Rigidbody2D>() as Rigidbody2D;
        if (rb) bodyTypeAtStart = rb.bodyType;
        sceneSettingsGO = GameObject.FindWithTag("SceneSettings").GetComponent<sceneSettings>() as sceneSettings;
        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
		initialFlipX=renderer.flipX;
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
	
	void Update()
	{
		base.Update();
		//The character controller attempts to automatically flip sprites that are children of the character. If we are flipping with scale, then we don't want none of the crap.
		if (flipWithScale)
			renderer.flipX=initialFlipX;
	}

    //Since the throw and action arrows are not child objects, these next three functions check if they exist and create them. We'll call checkForArrow anytime we know that we need one of the arrows, in case we've changed scenes and lost our arrow objects
    private void createThrowArrow()
    {
        throwArcObj = new GameObject(gameObject.name + "_throwArc");
        throwArcObj.transform.localPosition = new Vector3(0,0,0);
        throwArc = throwArcObj.AddComponent<lineArrow>() as lineArrow;
        throwArc.follow(gameObject.transform);
		throwArc.offset = aimOriginOffset;
        throwArc.hide();
    }
    private void createActionArrow()
    {
        actionAimObj = new GameObject(gameObject.name + "_actionAimObj");
        actionAimObj.transform.localPosition = new Vector3(0,0,0);
        actionAimArc = actionAimObj.AddComponent<lineArrow>() as lineArrow;
        actionAimArc.follow(gameObject.transform);
		actionAimArc.offset = aimOriginOffset;
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
            throwArc.setAngle(flippedX ? 130 : 50);
            //throwArc.setMinMax(flippedX ? 0 : 90, flippedX ? 90 : 180);
            throwArc.setMinMax(flippedX ? minAimFlipped : minAimNotFlipped, flippedX ? maxAimFlipped : maxAimNotFlipped);
        }
        if (release)
        {
            throwItem();
        }
        else
        {

            throwArc.setAngle(throwArc.angle += (flippedX ? -v : v));
            throwArc.setLength(throwArc.length + h);
        }
	
    }

    //These can be used to set aim arc directly to look at the point (world coord) and the function was primarily implemented for mouse aiming. They are intended to be called right before calling Aim() and actionAim() to adjust the angle with the mouse. Called in characterControllers
    public void setThrowAimAngleTowardsPoint(Vector3 point)
    {
        throwArc.setMinMax(flippedX ? minAimFlipped : minAimNotFlipped, flippedX ? maxAimFlipped : maxAimNotFlipped);
        float angle = Mathf.Atan2(point.y - transform.position.y+offset.y, point.x - transform.position.x + offset.x) * 180 / Mathf.PI;
        if (angle < -90) angle += 360;

        throwArc.setAngle(angle);
    }
    public void setActionAimAngleTowardsPoint(Vector3 point)
    {
        if (!hasAction || !hasActionAim) return;
        actionAimArc.setMinMax(flippedX ? minAimFlipped : minAimNotFlipped, flippedX ? maxAimFlipped : maxAimNotFlipped);
        float angle = Mathf.Atan2(point.y - transform.position.y + offset.y, point.x - transform.position.x + offset.x) * 180 / Mathf.PI;
        if (angle < -90) angle +=360;

        actionAimArc.setAngle(angle);
    }
	
	public void resetActionAim()
	{
		if (hasActionAim && hasAction)
        {
            actionAimArc.setAngle(flippedX ? 180 : 0);
			if (rotateWithAim) gameObject.transform.eulerAngles = new Vector3(0f, 0f, flippedX ? actionAimArc.angle + 180 : actionAimArc.angle);
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
                actionAimArc.setMinMax(flippedX ? minAimFlipped : minAimNotFlipped, flippedX ? maxAimFlipped : maxAimNotFlipped);
            }

            actionAimArc.setAngle(actionAimArc.angle += (flippedX ? -v : v));
            actionAimArc.setLength(actionAimArc.length + h);

            if (rotateWithAim) gameObject.transform.eulerAngles = new Vector3(0f, 0f, flippedX ? actionAimArc.angle + 180 : actionAimArc.angle);
        }
    }

    //This function spawns a projectile based on all of the projectile settings. Typically called from useItemAction
    public void spawnProjectile()
    {
        if (!spawnProjectilePrefab) return;
        Vector3 spawnPos;
		
		if (!projectileSpawnTransform)
		{
			if (hasActionAim) spawnPos = gameObject.transform.position + actionAimArc.getPointAlongAngle(spawnProjectileDistance) + (Vector3)offset + (Vector3)spawnProjectileOffset;
			else
			{
				spawnPos = gameObject.transform.position + (Vector3)offset + (Vector3)spawnProjectileOffset;
				spawnPos.x += spawnProjectileDistance * (flippedX ? -1 : 1);
			}
		}
		else
		{
			spawnPos = projectileSpawnTransform.position + (Vector3)spawnProjectileOffset;
		}

        GameObject projectile = Instantiate(spawnProjectilePrefab, spawnPos, Quaternion.identity);
        if (sceneSettingsGO != null) sceneSettingsGO.objectCreated(projectile);
        GameObject particles=null;
        if (spawnProjectileParticles) particles = Instantiate(spawnProjectileParticles, spawnPos, Quaternion.identity);
        if (setProjectileRotation && hasActionAim)
            projectile.transform.eulerAngles = new Vector3(0f, 0f, actionAimArc.angle);
        if (setParticleRotation && hasActionAim && particles)
            particles.transform.eulerAngles = new Vector3(0f, 0f, actionAimArc.angle);

        if (playerControlledProjectile && spawnProjectileBehavior != spawnProjectileBehaviors.spawnWhileHeld && holder!=null)
        {
            projectile.SendMessage("initiatePlayerControl", holder, SendMessageOptions.DontRequireReceiver);
        }

        if (projectileCamshakeDuration!=0 && projectileCamshakeIntensity!=0)
        {
            cameraFollowPlayer cfp = Camera.main.GetComponent<cameraFollowPlayer>();
            if (cfp) cfp.TriggerShakeExt(projectileCamshakeIntensity, projectileCamshakeDuration);
        }

		if (spawnProjectileForce!=0)
		{
			projectile.GetComponent<Rigidbody2D>().AddForce(spawnProjectileForce*projectile.transform.right,ForceMode2D.Impulse);
		}

        if (sndSpawnProjectile) global.audio.Play(sndSpawnProjectile);
    }

    //First three variables indicate the state of the action button (pressed, held, released). Last two variables are for aiming the useAction reticle (if there is one)
    public void useItemAction(bool pressed, bool held, bool released, float horizontal = 0f, float vertical = 0f)
    {
        if (!hasAction) return;

        if (pressed)
        {
            actionKeyPressed = true;
            if (ActionPressedCallback != null) ActionPressedCallback.Invoke();
            if (actionPressedMessageReceiver != null && actionPressedSendMessage != "")
            {
                actionPressedMessageReceiver.SendMessage(actionPressedSendMessage);
            }

            if (spawnProjectilePrefab && spawnProjectileBehavior == spawnProjectileBehaviors.spawnOnPress)
                spawnProjectile();

            if (spawnProjectilePrefab && spawnProjectileBehavior == spawnProjectileBehaviors.spawnWhileHeld)
            {
                InvokeRepeating("spawnProjectile", 0.1f, spawnProjectileRate);
            }

            if (sndOnActionPress) global.audio.Play(sndOnActionPress);
            if (sndLoopOnActionHeld) global.audio.PlayFXLoop(sndLoopOnActionHeld);
        }
        else if (released)
        {
            
            if (ActionReleasedCallback != null) ActionReleasedCallback.Invoke();
            if (actionReleasedMessageReceiver != null && actionReleasedSendMessage != "")
            {
                actionReleasedMessageReceiver.SendMessage(actionReleasedSendMessage);
            }

            if (hasActionAim) actionAimArc.hide();

            if (actionKeyPressed) //Make sure this module recieved a keyPress command. If we didn't, then that means the key was pressed while control was passed to some other object (like a projectile) and we don't want to respond to this key release by making another projectile.
            {
                if (spawnProjectilePrefab && spawnProjectileBehavior == spawnProjectileBehaviors.spawnOnRelease)
                    spawnProjectile();
            }

            actionKeyPressed = false;

            if (spawnProjectilePrefab && spawnProjectileBehavior == spawnProjectileBehaviors.spawnWhileHeld)
                CancelInvoke("spawnProjectile");

            if (sndOnActionRelease) global.audio.Play(sndOnActionRelease);
            if (sndLoopOnActionHeld) global.audio.StopFXLoop(sndLoopOnActionHeld);
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
		
		initialScale = transform.localScale;

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

        if (sendFaceMessage)
        {
            if (holder.GetComponent<CharacterController2D>().isFacingRight()) gameObject.SendMessage("FaceRight", SendMessageOptions.DontRequireReceiver);
            else gameObject.SendMessage("FaceLeft", SendMessageOptions.DontRequireReceiver);
        }
		
		if (hideCharacterDress)
		{
			holder.SendMessage("hideNonessentialDresses", SendMessageOptions.DontRequireReceiver);
		}

        if (resetRotationOnPickup)
            gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, initialZRotation);
        if (freezeRotationOnPickup)
            rb.freezeRotation = true;

        if (changeRigidBodyType)
        {
            rb.bodyType = bodyTypeWhileCarried;
        }


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

        if (sndOnPickup) global.audio.Play(sndOnPickup);
        if (sndLoopOnPickup) global.audio.PlayFXLoop(sndLoopOnPickup);

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

			if (pushOutOnRotate!=0)
			{
				float magnitude = gameObject.transform.rotation.z*pushOutOnRotate;
				gameObject.transform.localPosition = (flippedX ? new Vector3(-1f,1f,0f) : new Vector3(1f,1f,0f)) * magnitude;
			}

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
        recentlyThrownBy = holder;
        Invoke("ClearRecentlyThrownBy", recentlyThrownByTimer);

        if (sndOnThrow) global.audio.Play(sndOnThrow);

        releaseFromHolder();

        float radAngle = (-throwArc.angle + 90) * Mathf.Deg2Rad;
        rb.AddForce(new Vector2(Mathf.Sin(radAngle), Mathf.Cos(radAngle)) * throwForce * throwArc.length);

        if (throwTorque!=0)
        {
            rb.AddTorque(flippedX ? throwTorque : -throwTorque, ForceMode2D.Impulse);
        }
    }

    //Clears the recentlyThrownBy object
    private void ClearRecentlyThrownBy()
    {
        recentlyThrownBy = null;
    }
    public GameObject getRecentlyThrownBy()
    {
        
        return recentlyThrownBy;
    }

    public void releaseFromHolder()
    {
		if (hideCharacterDress)
		{
			holder.SendMessage("showNonessentialDresses", SendMessageOptions.DontRequireReceiver);
		}

        if (mCarryType == carryType.exactPlayerPosition)
        {
            enablePhysics();
            gameObject.transform.parent = parentPrevious;
        }

        if (hasHeldSprite && unheldSprite != null) renderer.sprite = unheldSprite;

        CancelInvoke("spawnProjectile");

        checkForArrows();

        if (changeRigidBodyType)
        {
            //We don't want to immediately revert back to our original bodytype, because if we were thrown we want to apply the forces to the bodytype we had while carried.
            Invoke("revertBodyType", 1f);
        }


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
		
		if (resetScaleOnRelease) transform.localScale = initialScale;

        if (sndLoopOnPickup) global.audio.StopFXLoop(sndLoopOnPickup);

        if (OnReleaseCallback != null) OnReleaseCallback.Invoke();
    }

    private void revertBodyType()
    {
        rb.bodyType = bodyTypeAtStart;
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
            
			if (!flipWithScale)
			{
				if (!swapXFlip) gameObject.GetComponent<SpriteRenderer>().flipX = flipX;
				else gameObject.GetComponent<SpriteRenderer>().flipX = !flipX;
			}
			else
			{
				float neg = -Mathf.Abs(transform.localScale.x);
				float pos = Mathf.Abs(transform.localScale.x);
				if (swapXFlip) 
				{
					neg = -neg;
					pos = -pos;
				}
				transform.localScale = new Vector3((flipX ? neg : pos), transform.localScale.y, transform.localScale.z);
			}
        }

        flippedX = flipX;

        //We still want to change the throw arc to the direction we're facing, even if we're not flipping the sprite.
        checkForArrows();
        if (!flippedX)
        {
            //throwArc.setMinMax(90, 180);
            //if (hasActionAim) actionAimArc.setMinMax(90, 180);
            throwArc.setMinMax(minAimNotFlipped, maxAimNotFlipped);
            if (hasActionAim) actionAimArc.setMinMax(minAimNotFlipped, maxAimNotFlipped);
        }
        else
        {
            //throwArc.setMinMax(0, 90);
            //if (hasActionAim) actionAimArc.setMinMax(0, 90);
            throwArc.setMinMax(minAimFlipped, maxAimFlipped);
            if (hasActionAim) actionAimArc.setMinMax(minAimFlipped, maxAimFlipped);
        }
        throwArc.setAngle( (180-throwArc.angle) );

        if (hasAction && hasActionAim)
        {
             if (justPickedUp==0) actionAimArc.setAngle(180 - actionAimArc.angle);
             else actionAimArc.setAngle(flippedX ? 180 : 0);
        }

        //if (rotateWithAim && hasActionAim && hasAction) gameObject.transform.eulerAngles = new Vector3(0f, 0f, flippedX ? actionAimArc.angle : actionAimArc.angle + 180);
        if (rotateWithAim && hasActionAim && hasAction) gameObject.transform.eulerAngles = new Vector3(0f, 0f, flippedX ? actionAimArc.angle + 180 : actionAimArc.angle );


    }
    public void flipSpriteY(bool flipY)
    {
        if (flipOnY)
        {
            offset.y = initialOffset.y * (flipY ? -1 : 1);
            //offset.y= -offset.y;
            
			if (!flipWithScale)
				gameObject.GetComponent<SpriteRenderer>().flipY = flipY;
			else
			{
				float neg = -Mathf.Abs(transform.localScale.y);
				float pos = Mathf.Abs(transform.localScale.y);
				transform.localScale = new Vector3(transform.localScale.x, (flipY ? neg : pos), transform.localScale.z);
			}
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
			
            if (dir == 0 && sideSortingLayer!="") renderer.sortingLayerName = sideSortingLayer;
            if (dir == 1 && frontSortingLayer != "") renderer.sortingLayerName = frontSortingLayer;
            if (dir == 2 && backSortingLayer != "") renderer.sortingLayerName = backSortingLayer;
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
