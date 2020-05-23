using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] public string CharacterName = "Blubber";                     //The name of this character, used for dialog and stuff
    [SerializeField] public bool startFlipped = false;                          //If set to true, the character will start out flipped horizontally

    [Header("Movement")]
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
    [SerializeField] private float maxVelocity = -1;                              // The maximum velocity that the character is limited to. -1 = none.
    [SerializeField] private bool m_AirControl = true;                          // Whether or not a player can steer while jumping;
    [SerializeField] private bool canUseDropDownPlatforms = true;              // Whether or not a player can steer while jumping;

    [Space]
    [Header("Jumping")]
    [SerializeField] private float m_JumpVelocity = 10.5f;                       // Amount of velocity added when the player jumps. 
    [SerializeField] private bool canDoubleJump = false;                        // Whether or not the player can jump a second time
    [SerializeField] private bool infiniteJump = false;                         // Jump whenever you want!

    [Space]
    [Header("Crouching")]
    [SerializeField] private bool canCrouch = true;                             // Whether or not the player can crouch
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

    [Space]
    [Header("Pushing")]
    [SerializeField] private bool canPush = true;                               // Whether or not the player can push
    [SerializeField] private float m_PushForce = 90f;                          // if canPush is true, this is the additional force applied when pushing
    [SerializeField] private float m_PushWait = 1f;                             // The amount of time to press against something before going into push mode

    [Space]
    [Header("Item Pickup")]
    [SerializeField] private bool canPickup = true;                             // Whether or not the player can push
    [SerializeField] private bool pickupWithJump = false;                       // If "Jump" and "Pickup" inputs are the same, set this to true to prevent jumping when picking something up
    [SerializeField] private Transform m_pickupTop;                             // Where to attach objects if carrying them above your head
    [SerializeField] private Transform m_pickupL;                             // Where to attach objects if carrying them in front left
    [SerializeField] private Transform m_pickupR;                             // Where to attach objects if carrying them in front right

    [Space]
    [Header("Climbing")]
    [SerializeField] private bool canClimb = true;                             // Whether or not the player can climb
    [SerializeField] private bool canCarryWhileClimbing = true;                // Whether or not the player can climb while holding something
    [SerializeField] private bool climbWithJump = false;                       // If "Jump" and "Climb" inputs are the same, set this to true to prevent jumping when climbing
    [SerializeField] private LayerMask m_WhatIsClimb;                          // A mask determining what is ground to the character

    [Space]
    [Header("Other")]
    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings. Also used to test for ladders.
    [SerializeField] private Transform m_SideCheckL;                            // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_SideCheckR;                            // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_rangeColliderL;                       // A position marking where to check if the player is grounded.
    [SerializeField] private Collider2D m_rangeColliderR;                       // A position marking where to check for ceilings
    [SerializeField] private Transform m_DialogTop;                             // The top position that the dialog box will point to
    [SerializeField] private Transform m_DialogBottom;                          // The bottom position that the dialot box will point to

    public Transform getDialogTop() { return m_DialogTop;  }
    public Transform getDialogBottom() { return m_DialogBottom; }

    private enum flipType { none, spriteRenderer, scale}
    [SerializeField] private flipType spriteFlipMethod = flipType.scale;  // The method used for flipping the sprite when the character turns around. NOTE: Sprite renderer will also flip sprites of child objects       

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded. 
	const float k_CeilingRadius = .1f; // Radius of the overlap circle to determine if the player can stand up. (Adjust is player is autocrouching or standing up when you don't want him to.)
    const float k_SideRadius = .1f; // Radius of the overlap circle to determine if the player can stand up. (Adjust is player is autocrouching or standing up when you don't want him to.)
    private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;
    private int numJumps = 0; //Counts the number of jumps while in air. Resets when player lands. Used, for example, in double jumping.
    private GameObject currentPlatform = null; //One of the current platforms we are standing on. Null if not grounded.
    private GameObject previousPlatform = null; 
    private CharacterAnimation charAnim = null;
    private Vector2 platformPreviousPosition;
    private bool isPushing = false; //Set to true when the character is trying to push something while grounded
    private float pushTimer = -1; //Stores the timer for PushWait. -1 means countdown hasn't started yet.
    private bool timerComplete=false; //Set to true when a timer has completed for pushing
    private bool isHolding = false; //Set to true if we're holding an object
    private pickupObject holding;     //The object we are currently (or were last) holding
    private GameObject actionObjectInRange; //This is set to the most recent gameobject using actionInRange that is colliding with this character's range colliders. Null if nothing in range.
    private bool isOnConveyor = false; //Set by the conveyor script
    private float initialGravityScale; //Used to store our gravity state in case we have to turn gravity off.
    private bool isClimbing = false;
    private dropDownPlatform onDropPlatformScript = null;
    public bool isTalking=false;


    [Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

    private Global global;

    private bool heldObjectChangedScenes = false;   //Set to true when an object is taken to a new scene. Used to mark the object for destruction at next scene load when it's dropped.

    private void Awake()
	{
        //We want the character to be persistent, and we only want one player
        if (gameObject.tag == "Player")
        {
            DontDestroyOnLoad(gameObject);
            if (GameObject.FindGameObjectsWithTag("Player").Length > 1)
            {
                Destroy(gameObject);
                return;
            }
        }

        global = GameObject.FindWithTag("global").GetComponent<Global>();

		m_Rigidbody2D = GetComponent<Rigidbody2D>();
        initialGravityScale = m_Rigidbody2D.gravityScale;

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();

        charAnim = gameObject.GetComponent<CharacterAnimation>() as CharacterAnimation;

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.01f);

        if (startFlipped) Flip();
    }

	private void FixedUpdate()
	{
        if (maxVelocity != -1)
        {
            m_Rigidbody2D.velocity = Vector2.ClampMagnitude(m_Rigidbody2D.velocity, maxVelocity);
        }

        bool wasGrounded = m_Grounded;
		m_Grounded = false;
        previousPlatform = currentPlatform;
        currentPlatform = null;
        onDropPlatformScript = null;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
            //Get the dropDown platform if we're on it
            onDropPlatformScript = colliders[i].gameObject.GetComponent<dropDownPlatform>() as dropDownPlatform;

            if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
                currentPlatform = colliders[i].gameObject;

                if (!wasGrounded && m_Rigidbody2D.velocity.y<0)
                {
                    OnLandEvent.Invoke();
                    if (charAnim != null)
                    {
                        charAnim.jump = false;
                        charAnim.doubleJump = false;
                    }
                    numJumps = 0;
                }
			}
		}


        if (currentPlatform == previousPlatform && currentPlatform!=null) //Still on the same platform. Add any platform motion to the character.
        {
            gameObject.transform.Translate(currentPlatform.transform.position.x - platformPreviousPosition.x, currentPlatform.transform.position.y - platformPreviousPosition.y, 0);
        }


        if (currentPlatform != null)
        {
            platformPreviousPosition = new Vector2(currentPlatform.transform.position.x, currentPlatform.transform.position.y);
        }

    }

    //Use the throwing retical
    //Arguments = Horizontal movement, vertical movement, release and throw, use the object's action
    public void Aim(float h, float v, bool release, bool action)
    {
        if (!isHolding || !canPickup || holding==null) return;

        holding.Aim(h, v, release, action);

        if (charAnim != null) charAnim.throwing = release;
    }


    public void Move(float move, bool crouch, bool jump, bool pickup=false, float climb=0, bool dropDown=false, bool dialog=false)
	{
        bool justPickedUp = false;

        //Handle drop down platforms
        if (dropDown && onDropPlatformScript != null && canUseDropDownPlatforms)
        {
            onDropPlatformScript.DropObject(gameObject);
        }

        // If crouching, check to see if the character can stand up
        if (!crouch && canCrouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
            }
		}

        //Climbing Code
        m_Rigidbody2D.gravityScale = initialGravityScale;
        if (charAnim != null) charAnim.climb = 0f;
        if ( (climb!=0||isClimbing) && canClimb && (!isHolding||canCarryWhileClimbing) ) //We meet some conditions for climbing, so let's check for something to climb
        {
            //check for climbing up and down the climb surface
            Collider2D collider = Physics2D.OverlapCircle(m_GroundCheck.position, k_GroundedRadius, m_WhatIsClimb);
            if (collider==null) isClimbing = false;
            else
            {
                isClimbing = true;
                m_Grounded = true;
                m_Rigidbody2D.gravityScale = 0;
                m_Rigidbody2D.velocity = new Vector3(0,0,0);
                numJumps = 0;
                if (charAnim != null)
                {
                    charAnim.climb = climb;
                    charAnim.jump = false; //Sometimes the jump animation sticks. This fixes that.
                }
                // Move the character by finding the target velocity
                //Vector3 climbVelocity = new Vector2(m_Rigidbody2D.velocity.x, m_ClimbSpeed);
                // And then smoothing it out and applying it to the character
                //m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, climbVelocity, ref m_Velocity, m_MovementSmoothing);
                if (climb!=0)
                    m_Rigidbody2D.MovePosition((Vector2)gameObject.transform.position + new Vector2(0,climb));
            }
        }
        if (charAnim != null) charAnim.isClimbing = isClimbing;

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch && canCrouch && (!isClimbing || !canClimb))
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
                }

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

                if (charAnim != null) charAnim.crouch = true;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
                    if (charAnim != null) charAnim.crouch = false;
                }
			}


            if (charAnim != null) charAnim.pushing = false;
            //Start pushing logic
            if (move==0)
            {
                pushTimer = -1;
                isPushing = false;
                timerComplete = false;
            }
            if (m_Grounded && (move>0 || move<0) && canPush && (!isClimbing || !canClimb) ) //If we're in a situation where we can potentially push
            {
                bool pushingLeft = false;
                bool pushingRight = false;
                Collider2D leftCol = Physics2D.OverlapCircle(m_SideCheckL.position, k_SideRadius, m_WhatIsGround);
                Collider2D rightCol = Physics2D.OverlapCircle(m_SideCheckR.position, k_SideRadius, m_WhatIsGround);
                if (leftCol!=null && move < 0) pushingLeft = true;
                if (rightCol!=null && move > 0) pushingRight = true;

                if (pushingLeft || pushingRight)
                {
                    if (pushTimer == -1 && !timerComplete) pushTimer = m_PushWait;
                    pushTimer -= Time.fixedDeltaTime;
                    if (pushTimer <= 0 && !timerComplete)
                    {
                        timerComplete = true;
                    }
                    if (timerComplete)
                    {
                        if (charAnim != null) charAnim.pushing = true;
                        isPushing = true;
                        if (pushingLeft)
                        {
                            if (leftCol.attachedRigidbody!=null)
                                leftCol.attachedRigidbody.AddForceAtPosition(new Vector2(-m_PushForce, 0f), new Vector2(m_SideCheckL.position.x, m_SideCheckL.position.y));
                        }
                        if (pushingRight)
                        {
                            if (rightCol.attachedRigidbody!=null)
                                rightCol.attachedRigidbody.AddForceAtPosition(new Vector2(m_PushForce, 0f), new Vector2(m_SideCheckR.position.x, m_SideCheckR.position.y));
                        }
                    }
                }
                else
                {
                    pushTimer = -1;
                    isPushing = false;
                    timerComplete = false;
                }
            }

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);

			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            if (charAnim != null) charAnim.speed = Mathf.Abs(move);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}

            //Start pickup item code
            if (canPickup && !isHolding && m_Grounded && actionObjectInRange != null && pickup && (!isClimbing || !canClimb)) //We're in range of something, can pick it up, and trying to pick it up
            {
                holding = actionObjectInRange.GetComponent<pickupObject>() as pickupObject;
                if (holding != null)
                {
                    holding.pickMeUp(gameObject, m_pickupTop, m_FacingRight ? m_pickupR : m_pickupL);
                    isHolding = true;
                    setActionObjectInRange(null);
                    justPickedUp = true;
                    if (charAnim != null) charAnim.pickingUp = true;
                    holding.SendMessage("flipSpriteX", !m_FacingRight); //Update the held object's facing direction
                }
            }

            //Start dialog initiation code
            if (dialog && !isHolding && m_Grounded && actionObjectInRange != null && (!isClimbing || !canClimb)) //We're in range of something that we can talk to, and are pressing the dialog button
            {
                DialogRange d = actionObjectInRange.GetComponent<DialogRange>() as DialogRange;
                if (d != null)
                {
                    m_Rigidbody2D.velocity = new Vector2(0f, 0f);
                    d.Initiate(CharacterName, gameObject, m_DialogTop, m_DialogBottom);
                    isTalking = true;
                    setActionObjectInRange(null);
                }
            }

        } //END if Grounded || Air Control

        charAnim.carryTop = false;
        charAnim.carryFront = false;

        if (isHolding && charAnim!=null)
        {
            if (holding.mCarryType == pickupObject.carryType.Top) charAnim.carryTop = true;
            if (holding.mCarryType == pickupObject.carryType.Front) charAnim.carryFront = true;
        }

        // If the player should jump...
        if (jump && (!justPickedUp||!pickupWithJump||!canPickup) && (!isClimbing||!climbWithJump||!canClimb) )
		{
            if (infiniteJump || (canDoubleJump && numJumps < 2) || m_Grounded)
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                numJumps++;
                if (charAnim != null)
                {
                    charAnim.jump = true;
                    if (numJumps > 1) charAnim.doubleJump = true;
                }
                //m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                m_Rigidbody2D.velocity += (new Vector2(0f, m_JumpVelocity));
            }
		}
    }
    public void StopTalking()
    {
        isTalking = false;
    }
    public void setIsOnConveyor(bool val)
    {
        isOnConveyor = val;
    }
    public bool getIsOnConveyor()
    {
       return isOnConveyor;
    }
    public bool getCanClimb()
    {
        return canClimb;
    }
    public bool crouchEnabled()
    {
        return canCrouch;
    }
    public bool pickupEnabled()
    {
        return canPickup;
    }
    public bool holdingSomething()
    {
        return isHolding;
    }
    public bool pushEnabled()
    {
        return canPush;
    }
    public bool isPushingSomething()
    {
        return isPushing;
    }
    public void setActionObjectInRange(GameObject go)
    {
        this.actionObjectInRange = go;
    }
    public GameObject getActionObjectInRange()
    {
        return actionObjectInRange;
    }
    public void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;
        if (isHolding)
        {
            holding.SendMessage("changeFrontTransform", m_FacingRight ? m_pickupR : m_pickupL);
            holding.SendMessage("flipSpriteX", !m_FacingRight);
        }

        if (charAnim != null) charAnim.FlipX(!m_FacingRight);

        if (spriteFlipMethod == flipType.scale)
        {
            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
        if (spriteFlipMethod == flipType.spriteRenderer)
        {
            SpriteRenderer spr = gameObject.GetComponent<Renderer>() as SpriteRenderer;
            spr.flipX = !m_FacingRight;

            //Flip child objects that have sprites
            Transform[] ts = gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform t in ts)
            {
                SpriteRenderer renderer = t.gameObject.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                if (renderer != null)
                {
                    renderer.flipX = !m_FacingRight;
                }
            }
        }


        if (canPickup && (m_rangeColliderL!=null && m_rangeColliderR != null))
        {
            m_rangeColliderR.enabled = m_FacingRight;
            m_rangeColliderL.enabled = !m_FacingRight;
        }
    }

    public bool isFacingRight()
    {
        return m_FacingRight;
    }

    //Called from pickupObject script when holding item is released (dropped, thrown, added to inventory, etc)
    public void pickupReleased()
    {
        isHolding = false;
        if (heldObjectChangedScenes)
        {
            Debug.Log("CC2d::pickupReleased() " +holding.gameObject.name);
            global.map.destroyOnSceneChange(holding.gameObject);
            heldObjectChangedScenes = false;
        }
    }

    //Drops an object if one is carried
    public void dropObject()
    {
        if (holdingSomething())
        {
            Debug.Log("CC2d::dropObject() (if holdingSomething) " + holding.gameObject.name);
            holding.releaseFromHolder();
        }
    }

    //Called when the character (if it is a player) is about to warp to the next scene. Function is when the player activates a warp trigger, before the character is repositioned and new map is loaded. Called by warpTrigger::Warp
    //map = name of map we are moving to.
    public void sceneChangeStart(string map)
    {
        Debug.Log("CC2d::sceneChangeStart()");
        //If the warp trigger doesn't want us to carry the object across, it should have sent us a dropObject message by now. So we'll assume we can take it with us.
        if (holdingSomething())
        {
            Debug.Log("CC2d::sceneChangeStart() (if holdingSomething) " + holding.gameObject.name);
            holding.transform.parent = gameObject.transform;
            global.map.removeFromDestroyLoadList(holding.gameObject); //If it was previously added to the destroy on scene change list then picked back up, we don't want to destroy it. We want to carry it to the next scene
        }
    }

    //Called immedately after a new scene is loaded and the character is relocated. Called by MapSystem::goto
    public void sceneChangeComplete()
    {
        if (holdingSomething())
        {
            Debug.Log("CC2d::sceneChangeComplete() (if holdingSomething) " + holding.gameObject.name);
            holding.transform.parent = null;
            heldObjectChangedScenes = true;  //Used to mark the object for destruction on next scene load after it is dropped
        }
    }

}