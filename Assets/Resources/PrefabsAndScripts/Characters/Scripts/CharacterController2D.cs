using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] public string CharacterName = "Blubber";                     //The name of this character, used for dialog and stuff
    [SerializeField] public bool startFlipped = false;                          //If set to true, the character will start out flipped horizontally

    [Header("Movement")]
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
    [SerializeField] private float maxVelocity = -1;                              // The maximum velocity that the character is limited to. -1 = none.
    [SerializeField] private bool m_AirControl = true;                          // Whether or not a player can steer while jumping;
    [SerializeField] private bool canUseDropDownPlatforms = true;              // Whether or not a player can steer while jumping;
    [SerializeField] private bool inWater = false;
    [SerializeField] private float waterMultiplier = 0.7f;                   //This value is multiplied to move speed when the player is in water
    private bool initialIsFacingRight;                                      //This is set to m_isFacingRight at the start of the object and retains this value. Can be retrieved using isInitiallyFacingRight(). Can be useful for knowing what direction an NPC was initially configured to face in the editor 
    private bool mouseAim = false;                                          //Set to true by the input controller to aim throw and action reticals with mouse
    private Vector3 previousMousePosition=new Vector3(0f,0f,0f);               //Used for mouse aiming to track the previous position of the mouse in the frame update
    [HideInInspector] public bool currentlyMouseAiming = false;                              //Used internally for setting character facing direction with mouse aim. This is set to true if we are holding down the throw or action AND we have since moved the mouse, false upon throw or action release

    [Space]
    [Header("Jumping")]
    [SerializeField] private float m_JumpVelocity = 10.5f;                       // Amount of velocity added when the player jumps. 
    [SerializeField] private bool temporaryExtraJump = false;                   //If this gets set to true, the player will get a temporary double jump that will go away either upon landing or using it. For example, used by double jump arrow
    [SerializeField] private bool canDoubleJump = false;                        // Whether or not the player can jump a second time
    [SerializeField] private bool infiniteJump = false;                         // Jump whenever you want!
    [SerializeField] private bool infiniteWaterJump = true;                    //Jump anytime, if in water
    [SerializeField] private float m_WaterJumpMultiplier = 0.6f;                    //Multiplied to jump force if in water
    [SerializeField] private float m_MaxJumpVelocity = 12f;                    //Infinite jump or water jump can be exploited to lead to high speeds. This clamps the speed after jumping.
    [SerializeField] private AudioClip jumpSound;

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
    [SerializeField] private bool canPickup = true;                             // Whether or not the character can pickup items
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
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings. Also used to test for ladders.
    [SerializeField] private Transform m_SideCheckL;                            // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_SideCheckR;                            // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_rangeColliderL;                       // A position marking where to check if the player is grounded.
    [SerializeField] private Collider2D m_rangeColliderR;                       // A position marking where to check for ceilings
    [SerializeField] private Transform m_DialogTop;                             // The top position that the dialog box will point to
    [SerializeField] private Transform m_DialogBottom;                          // The bottom position that the dialot box will point to
    [SerializeField] private AudioClip dialogSound;
	

    public Transform getDialogTop() { return m_DialogTop; }
    public Transform getDialogBottom() { return m_DialogBottom; }

    private DialogBox sayDialogBox; //This stores the current dialog box created with the Say() command

    private enum flipType { none, spriteRenderer, scale }
    [SerializeField] private flipType spriteFlipMethod = flipType.scale;  // The method used for flipping the sprite when the character turns around. NOTE: Sprite renderer will also flip sprites of child objects       

    [SerializeField] private float k_GroundedRadius = .1f; // Radius of the overlap circle to determine if grounded
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
    private bool timerComplete = false; //Set to true when a timer has completed for pushing
    private bool isHolding = false; //Set to true if we're holding an object
    private pickupObject holding;     //The object we are currently (or were last) holding
    private GameObject actionObjectInRange; //This is set to the most recent gameobject using actionInRange that is colliding with this character's range colliders. Null if nothing in range.
    private bool isOnConveyor = false; //Set to true when a platform below us has the "Conveyor" tag
    private float initialGravityScale; //Used to store our gravity state in case we have to turn gravity off.
    private bool isClimbing = false;
    private dropDownPlatform onDropPlatformScript = null;
    public bool isTalking = false;       //Set to true when we are in dialog. 
    public bool pause = false;       //Set to true when we are selecting something from a menu like a color picker, in non-auto dialog, or the pause menu, ect. This freezes all input. 
    private Transform holdingParentPrevious = null; //Used for transferring the a object to the next scene when scene changing
    private bool controlTaken = false;


    [Space]
    [Header("Death")]
    [SerializeField] private bool canDie = true;                                 //If true, objects with the specified tag will kill this character
    [SerializeField] private bool respawn = true;                                //If set to true, the character will respawn after death.
    [SerializeField] private string[] killTags = new string[] { "killPlayer" };   //A list of tags. If the character collides with an object that has this tag and canDie is true, the character will die
    [SerializeField] private GameObject deathParticles;                          //A particle system object that is spawned when the character dies
    [SerializeField] private GameObject spawnParticles;
    [SerializeField] private Transform respawnOverride = null;                     //If a transform is specified, the character will always respawn here. If null, the best respawn point will be determined
    [SerializeField] private float respawnTime = 0.5f;                            //The amount of time after death before the character is lerped back to his respawn point
    private Vector3 respawnPosition;                                             //Used for tracking the respawn position. This will either be the position at scene start, position of checkpoint, or respawnOverride
    private bool isDead = false;                                                 //Set to true when the character is in the dead state
    private float respawnTimer = 0f;                                              //This is the timer that is set for respawning
    private bool kinematicStateBeforeDeath = false;                               //Used to store and reset rb.isKinematic to it's normal state after death and respawn
    private int layerBeforeDeath;                                           //When you die, you are temporarily moved to the disableAllCollision layer. This is used to store the initial layer so you can be moved back at respawn
    [SerializeField] private float respawnLerpSpeed = 3f;                 //Speed used for lerping the character back to the respawn point. Enter -1 for instant jump
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip spawnSound;

    [Space]
    [Header("Debug")]
	
	[SerializeField] private bool d_showCurrentPlatform = false;	//When set to true, the platform that the player is currently standing on will be highlighted. Note this does not work for tiles.
	private Color d_showCurrentPlatformStartColor=Color.white;					//Used internally so we can revert the current platform back to it's normal color when we leave it.
	[SerializeField] private bool d_showPlatformCheck=false;		//When set to true, a circle is drawn showing the position and radius of the circle cast that detects the platform.
	
	
    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    private Global global;

    private bool heldObjectChangedScenes = false;   //Set to true when an object is taken to a new scene. Used to mark the object for destruction at next scene load when it's dropped.

    private UIOverlayScript uiOverlay;

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

            getHoldingUI();
        }

        global = GameObject.FindWithTag("global").GetComponent<Global>();

        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        initialGravityScale = m_Rigidbody2D.gravityScale;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        charAnim = gameObject.GetComponent<CharacterAnimation>() as CharacterAnimation;

        respawnPosition = gameObject.transform.position;

        initialIsFacingRight = (startFlipped ? false : true);

        StartCoroutine(DelayedStart());

    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.01f);

        if (startFlipped) Flip();
    }

    void LateUpdate()
    {
        previousMousePosition = Input.mousePosition;
    }

    private void FixedUpdate()
    {
		
        //Let's freeze if the scene is changing.
        if (global.isSceneChanging())
        {
            m_Rigidbody2D.velocity = new Vector3(0f, 0f, 0f);
            return;
        }

        if (isDead)
        {
            if (respawn)
            {
                if (respawnTimer > 0)
                {
                    respawnTimer -= Time.deltaTime;
                }
                else
                {
                    respawnTimer = 0;

                    if (respawnLerpSpeed == -1) gameObject.transform.position = respawnPosition;
                    else gameObject.transform.position = Vector3.Lerp(transform.position, respawnPosition, Time.deltaTime * respawnLerpSpeed);

                    if (Vector3.Distance(gameObject.transform.position, respawnPosition) < 0.1f)
                        respawnCharacter();
                }
            }
        }
        else
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
			
			isOnConveyor=false;
			
			float shortestDistance=-1f;
			
			//We don't want to keep switching the platform we are on, as that can cause problems. Particularly when it comes to lots of tiny platforms that are close together and moving (like conveyors)
			//We'll use this flag to see if the collider list contains the previous platform. If it does, we'll favor sticking to that one.
			bool containsPreviousPlatform=false;
			
            for (int i = 0; i < colliders.Length; i++)
            {
                //Get the dropDown platform if we're on it
                onDropPlatformScript = colliders[i].gameObject.GetComponent<dropDownPlatform>() as dropDownPlatform;

                GameObject holdingGo = null;
                if (holdingSomething())
                    holdingGo = holding.gameObject;
				
				//Check if it is a conveyor
				if (colliders[i].gameObject.tag=="Conveyor") 
				{
					isOnConveyor=true;
				}


                if (colliders[i].gameObject != gameObject && colliders[i].gameObject != holdingGo && !colliders[i].isTrigger)
                {
                    m_Grounded = true;
                    temporaryExtraJump = false;
					
					if (colliders[i].gameObject==previousPlatform)
					{
						containsPreviousPlatform=true;
					}
					
					//We want to choose the platform that is closest to our groundcheck position to be our current platform.
					if (Mathf.Abs(Vector3.Distance(colliders[i].gameObject.transform.position,m_GroundCheck.position))<shortestDistance || shortestDistance==-1)
						currentPlatform = colliders[i].gameObject;

                    if (!wasGrounded && m_Rigidbody2D.velocity.y < 0)
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
			
			//Favor the platform we were already standing on.
			if (containsPreviousPlatform ) currentPlatform=previousPlatform;



			//The following code helps move the player along any moving objects or platforms. 
            if ( currentPlatform == previousPlatform && currentPlatform != null) //Still on the same platform. Add any platform motion to the character.
            {
                //But first let's make sure the platform didn't move a ridiculously large amount. Generally, if the platform teleports out from under us we probably don't want to go with it. 
                //An example of this is the half segmented conveyor belts. Segments jump from the end rotor to the beginning, and we don't want to take the player.
                if ( Mathf.Abs(currentPlatform.transform.position.x - platformPreviousPosition.x) <= 1f && Mathf.Abs(currentPlatform.transform.position.y - platformPreviousPosition.y) <= 1f)
                {
					gameObject.transform.Translate(currentPlatform.transform.position.x - platformPreviousPosition.x, currentPlatform.transform.position.y - platformPreviousPosition.y, 0);
				}
            }


            if (currentPlatform != null)
            {
                platformPreviousPosition = new Vector2(currentPlatform.transform.position.x, currentPlatform.transform.position.y);
            }
			
			if (d_showCurrentPlatform)
			{
				if(currentPlatform!=previousPlatform)
				{
					if (previousPlatform!=null) 
					{
						if (previousPlatform.GetComponent<SpriteRenderer>()!=null)
							previousPlatform.GetComponent<SpriteRenderer>().color = d_showCurrentPlatformStartColor;
					}
					if (currentPlatform!=null) 
					{
						if (currentPlatform.GetComponent<SpriteRenderer>())
						{
							d_showCurrentPlatformStartColor=currentPlatform.GetComponent<SpriteRenderer>().color;
							currentPlatform.GetComponent<SpriteRenderer>().color = Color.yellow;
						}
					}
				}
			}
        }
    }

    public void useItemAction(bool pressed, bool held, bool released, float horizontal = 0, float vertical = 0)
    {
        if (isDead) return;

        if (!isHolding || !canPickup || holding == null) return;

        //Debug.Log("P:" + pressed + ", H:" + held + ", R:" + released + ", Hor:" + horizontal + ", Vert:" + vertical);
        if (mouseAim)
        {
            if (Input.mousePosition != previousMousePosition)
            {
                if (held) currentlyMouseAiming = true;
                holding.setActionAimAngleTowardsPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            }
            if (currentlyMouseAiming && holding.flipCharacterWithMouseAim)
            {
                if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < transform.position.x) FaceLeft();
                if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x > transform.position.x) FaceRight();
                horizontal = vertical = 0f;
            }
        }

        if (released) currentlyMouseAiming = false;

        holding.useItemAction(pressed, held, released, horizontal, vertical);
    }

    //Use the throwing retical
    //Arguments = Horizontal movement, vertical movement, release and throw, use the object's action
    public void Aim(float h, float v, bool release, bool action)
    {
        if (isDead) return;

        if (!isHolding || !canPickup || holding == null) return;
        
        if (mouseAim)
        {
            if (Input.mousePosition != previousMousePosition)
            {
                currentlyMouseAiming = true;
                holding.setThrowAimAngleTowardsPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            }
            if (currentlyMouseAiming && holding.flipCharacterWithMouseAim)
            {
                if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < transform.position.x) FaceLeft();
                if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x > transform.position.x) FaceRight();
                h = v = 0f;
            }
        }

        if (release) currentlyMouseAiming = false;

        holding.Aim(h, v, release, action);

        if (charAnim != null) charAnim.throwing = release;
    }


    public void Move(float move, bool crouch, bool jump, bool pickup = false, float climb = 0, bool dropDown = false, bool dialog = false)
    {

        if (isDead || controlTaken || pause) return;

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
            Collider2D col = Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround);
            if (col)
            {
                GameObject holdingGo = null;
                if (holdingSomething())
                    holdingGo = holding.gameObject;

                if (!col.isTrigger && col.gameObject!=holdingGo)
                    crouch = true;
            }
        }

        //Climbing Code
        m_Rigidbody2D.gravityScale = initialGravityScale;
        if (charAnim != null) charAnim.climb = 0f;
        if ((climb != 0 || isClimbing) && canClimb && (!isHolding || canCarryWhileClimbing)) //We meet some conditions for climbing, so let's check for something to climb
        {
            //check for climbing up and down the climb surface
            Collider2D collider = Physics2D.OverlapCircle(m_GroundCheck.position, k_GroundedRadius, m_WhatIsClimb);
            if (collider == null) isClimbing = false;
            else
            {
                isClimbing = true;
                m_Grounded = true;
                m_Rigidbody2D.gravityScale = 0;
                m_Rigidbody2D.velocity = new Vector3(0, 0, 0);
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
                if (climb != 0)
                    m_Rigidbody2D.MovePosition((Vector2)gameObject.transform.position + new Vector2(0, climb));
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
            }
            else
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
            if (move == 0)
            {
                pushTimer = -1;
                isPushing = false;
                timerComplete = false;
            }
            if (m_Grounded && (move > 0 || move < 0) && canPush && (!isClimbing || !canClimb)) //If we're in a situation where we can potentially push
            {
                bool pushingLeft = false;
                bool pushingRight = false;

                Collider2D[] leftColAll = Physics2D.OverlapCircleAll(m_SideCheckL.position, k_SideRadius, m_WhatIsGround);
                Collider2D[] rightColAll = Physics2D.OverlapCircleAll(m_SideCheckR.position, k_SideRadius, m_WhatIsGround);
                Collider2D leftCol = null, rightCol = null;

                GameObject holdingGo = null;
                if (holdingSomething())
                    holdingGo = holding.gameObject;

                foreach (var l in leftColAll)
                {
                    if (l != null && l.gameObject != holdingGo && !l.isTrigger && move < 0)
                    {
                        pushingLeft = true;
                        leftCol = l;
                        break;
                    }
                }
                foreach (var r in rightColAll)
                {
                    if (r != null && r.gameObject != holdingGo && !r.isTrigger && move > 0)
                    {
                        rightCol = r;
                        pushingRight = true;
                        break;
                    }
                }

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
                            if (leftCol.attachedRigidbody != null)
                                leftCol.attachedRigidbody.AddForceAtPosition(new Vector2(-m_PushForce, 0f), new Vector2(m_SideCheckL.position.x, m_SideCheckL.position.y));
                        }
                        if (pushingRight)
                        {
                            if (rightCol.attachedRigidbody != null)
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
			
			//if ( (isOnConveyor && move!=0 ) || !isOnConveyor) 
		
				// Move the character by finding the target velocity
				Vector3 targetVelocity = new Vector3(move * 10f, m_Rigidbody2D.velocity.y,3);

				// And then smoothing it out and applying it to the character
				m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
	
	
            if (inWater)
            {
                m_Rigidbody2D.velocity *= new Vector3(waterMultiplier, 1, 1);
            }


            if (charAnim != null) charAnim.speed = Mathf.Abs(move);

            //We're about to flip the character. If we're aiming with the mouse then we don't want to flip the character while walking. Let's check for that first.
            var dontFlip = false;
            if (isHolding)
            {
                if (mouseAim && currentlyMouseAiming && holding.flipCharacterWithMouseAim) 
                    dontFlip = true;
            }

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight && !dontFlip)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight && !dontFlip)
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
                    currentlyMouseAiming = false;

                    holding.pickMeUp(gameObject, m_pickupTop, m_FacingRight ? m_pickupR : m_pickupL);
                    isHolding = true;
                    setHoldingUIText(holding.name);
                    setActionObjectInRange(null);
                    justPickedUp = true;
                    if (charAnim != null) charAnim.pickingUp = true;
                    holding.SendMessage("flipSpriteX", !m_FacingRight); //Update the held object's facing direction
                }
            }

            //Start dialog initiation code
            if (dialog && !isHolding && m_Grounded && actionObjectInRange != null && (!isClimbing || !canClimb)) //We're in range of something that we can talk to, and are pressing the dialog button
            {
                if (sayDialogBox == null)
                {
                    DialogRange d = actionObjectInRange.GetComponent<DialogRange>() as DialogRange;
                    if (d != null)
                    {
                        m_Rigidbody2D.velocity = new Vector2(0f, 0f);
                        d.Initiate(CharacterName, gameObject, m_DialogTop, m_DialogBottom);
                        isTalking = true;
                        pause = true;
                        setActionObjectInRange(null);
                    }
                }
            }

            //Code for other range triggers (RangeTriggerEvent)
            if (pickup && !isHolding && m_Grounded && actionObjectInRange != null && (!isClimbing || !canClimb)) //We're in range of something, so we'll check to see if it has a RangeTriggerEvent. Pickup is used to check if the action key (pickup key) was pressed
            {
                RangeTriggerEvent d = actionObjectInRange.GetComponent<RangeTriggerEvent>() as RangeTriggerEvent;
                if (d != null)
                {
                    m_Rigidbody2D.velocity = new Vector2(0f, 0f);
                    d.Activate(CharacterName, gameObject);
                    setActionObjectInRange(null);
                }
            }

        } //END if Grounded || Air Control

        charAnim.carryTop = false;
        charAnim.carryFront = false;

        if (isHolding && charAnim != null)
        {
            if (holding.mCarryType == pickupObject.carryType.Top) charAnim.carryTop = true;
            if (holding.mCarryType == pickupObject.carryType.Front) charAnim.carryFront = true;
        }

        // If the player should jump...
        if (jump && (!justPickedUp || !pickupWithJump || !canPickup) && (!isClimbing || !climbWithJump || !canClimb))
        {
            if (infiniteJump || (canDoubleJump && numJumps < 2) || m_Grounded || (inWater && infiniteWaterJump) || temporaryExtraJump)
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                numJumps++;

                //With the temporary extra jump things tend to work more consistently when we cancel out any current y velocities and start at zero. This gives us better control over the jump.
                if (temporaryExtraJump ) m_Rigidbody2D.velocity = new Vector3(m_Rigidbody2D.velocity.x, 0f, 0f);

                temporaryExtraJump = false;
                if (charAnim != null)
                {
                    charAnim.jump = true;
                    if (numJumps > 1) charAnim.doubleJump = true;
                }

                //m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                m_Rigidbody2D.velocity += (new Vector2(0f, m_JumpVelocity * (inWater ? m_WaterJumpMultiplier : 1)  ));

                //Sometimes infinite jump or water jump can be exploited to lead to high speeds. This clamps the velocity down.
                if (m_Rigidbody2D.velocity.y > m_MaxJumpVelocity) m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_MaxJumpVelocity);

                if (jumpSound) global.audio.RandomSoundEffect(jumpSound);
            }
        }
        inWater = false;
    }
	
	private void OnDrawGizmos()
	{
		if (d_showPlatformCheck)
		{
			UnityEditor.Handles.color = Color.green;
			UnityEditor.Handles.DrawWireDisc(m_GroundCheck.position , Vector3.forward, k_GroundedRadius);
		}

	}

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (global.isSceneChanging())
            return;

        if (canDie && !isCharacterDead())
        {
            bool deathTag = false;
            foreach (string s in killTags)
            {
                if (s == other.gameObject.tag) deathTag = true;
            }

            if (deathTag) die();
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Water")
        {
            inWater = true;
        }

    }

    //This function will give the player the ability to temporarily double jump until they either use their double jump or land on the ground. Only works while they are still in the air.
    public void addTemporaryJump()
    {
        temporaryExtraJump = true;
    }

    //Called by checkpoint objects. If null is passed as the position, then the character's position is used.
    public void registerCheckpoint(Vector3 position)
    {
        if (position == null) position = gameObject.transform.position;
        if (respawnOverride == null) respawnPosition = position;
    }

    public void die()
    {
        if (isDead) return;

        ClearSay();
        StopTalking();

        dropObject();
        if (deathParticles)
        {
            GameObject ps = Instantiate(deathParticles, gameObject.transform.position, Quaternion.identity);
            var main = ps.GetComponent<ParticleSystem>().main;
            main.startColor = gameObject.GetComponent<SpriteRenderer>().color;
        }

        if (respawn)
        {
            if (respawnOverride) respawnPosition = respawnOverride.position;

            respawnTimer = respawnTime;
        }

        isDead = true;

        if (dieSound) global.audio.Play(dieSound);

        layerBeforeDeath = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("DisableAllCollision");
        kinematicStateBeforeDeath = m_Rigidbody2D.isKinematic;
        m_Rigidbody2D.isKinematic = true;
        m_Rigidbody2D.Sleep();
        m_Rigidbody2D.velocity = new Vector3(0f, 0f, 0f);

        if (charAnim == null) charAnim = gameObject.GetComponent<CharacterAnimation>();
        charAnim.characterDied();
    }

    public void respawnCharacter()
    {
        if (!respawn || !isDead) return;
        gameObject.transform.position = respawnPosition;
        isDead = false;
        m_Rigidbody2D.WakeUp();
        m_Rigidbody2D.isKinematic = kinematicStateBeforeDeath;
        gameObject.layer = layerBeforeDeath;
        if (spawnParticles) makeSpawnParticles();
        if (charAnim == null) charAnim = gameObject.GetComponent<CharacterAnimation>();
        if (spawnSound) global.audio.Play(spawnSound);
        charAnim.characterRespawned();
		CPUInput cpu = GetComponent<CPUInput>();
		if (cpu!=null) cpu.characterRespawned();
    }
	
	public void makeSpawnParticles()
	{
		Instantiate(spawnParticles, gameObject.transform.position, Quaternion.identity);
	}

    public bool isCharacterDead()
    {
        return isDead;
    }

    public void StopTalking()
    {
        isTalking = false;
        pause = false;
    }
    public void StartTalking()
    {
        isTalking = true;
        pause = true;
    }
    public void unpauseCharacter()
    {
        pause = false;
    }
    public void pauseCharacter()
    {
        pause = true;
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

    public pickupObject getHolding()
    {
        return holding;
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

    public void FaceLeft()
    {
        if (m_FacingRight) Flip();
    }
    public void FaceRight()
    {
        if (!m_FacingRight) Flip();
    }
	
	public void stopClimbing()
	{
		isClimbing=false;
	}

    //Returns an int that represents the direction the character is facing. 0=side, 1=front, 2=back. This info is pulled from the characterAnimation script which handles this data. 
    //This does NOT return m_FacingRight. Left and right are different from facing direction (front,back,side), so use isFacingRight() for that.
    public int getFacingDirection()
    {
        return charAnim.getFacingDirection();
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


        if (canPickup && (m_rangeColliderL != null && m_rangeColliderR != null))
        {
            m_rangeColliderR.enabled = m_FacingRight;
            m_rangeColliderL.enabled = !m_FacingRight;
        }
    }

    public bool isFacingRight()
    {
        return m_FacingRight;
    }
    public bool isInitiallyFacingRight()
    {
        return initialIsFacingRight;
    }

    //Call to point the character back in the direction he/she was configured to face at the start of the scene
    public void faceInitialDirection()
    {
        if (isInitiallyFacingRight()) FaceRight();
        else FaceLeft();
    }

    //Called from pickupObject script when holding item is released (dropped, thrown, added to inventory, etc)
    public void pickupReleased()
    {
        setHoldingUIText("");

        currentlyMouseAiming = false;
        isHolding = false;
        if (heldObjectChangedScenes)
        {
            global.map.destroyOnSceneChange(holding.gameObject);
            heldObjectChangedScenes = false;
        }
    }

    //Drops an object if one is carried
    public void dropObject()
    {
        currentlyMouseAiming = false;
        if (holdingSomething())
        {
            holding.releaseFromHolder();
        }
    }

    //Called when the character (if it is a player) is about to warp to the next scene. Function is when the player activates a warp trigger, before the character is repositioned and new map is loaded. Called by MapSystem::goto
    //map = name of map we are moving to.
    public void sceneChangeStart(string map)
    {
        ClearSay();
        currentlyMouseAiming = false;
        //If the warp trigger doesn't want us to carry the object across, it should have sent us a dropObject message by now. So we'll assume we can take it with us.
        if (holdingSomething())
        {
            holdingParentPrevious = holding.transform.parent;
            holding.transform.parent = gameObject.transform;
            holding.SendMessage("makeChild", SendMessageOptions.DontRequireReceiver); //This packs up the action icon(s) into children from transport
            holding.makeUndroppable();
            holding.hideArrows();
            global.map.removeFromDestroyLoadList(holding.gameObject); //If it was previously added to the destroy on scene change list then picked back up, we don't want to destroy it. We want to carry it to the next scene
        }
    }

    //Called immedately after a new scene is loaded and the character is relocated. Called by MapSystem::SceneLoad
    public void sceneChangeComplete()
    {
        if (holdingSomething())
        {

            holding.transform.parent = holdingParentPrevious;
            holding.SendMessage("unChild", SendMessageOptions.DontRequireReceiver); //This unchilds the action icons
            holding.makeDroppable();
            holding.changedScenes();
            heldObjectChangedScenes = true;  //Used to mark the object for destruction on next scene load after it is dropped
            setHoldingUIText(holding.name);
        }
        else
        {
            setHoldingUIText("");
        }

        //If this character just entered a new scene, then we can't use the old respond position for the previous scene
        if (canDie && respawn)
        {
            if (gameObject.scene.buildIndex == -1) //This is basically if (dontdestroyonload is activated)
            {
                respawnPosition = gameObject.transform.position;
            }
        }
    }

    public bool isGrounded()
    {
        return m_Grounded;
    }

    //This should be called by the playerInput module. If you are trying to toggle mouse aim, call setMouseAim from there
    public void setMouseAim(bool set)
    {
        mouseAim = set;
    }

    //Returns the vector for the sidecheck transform that the character is facing towards
    public Vector3 getFrontPosition()
    {
        if (m_FacingRight) return m_SideCheckR.position;
        else return m_SideCheckL.position;
    }
    public Vector3 getTopPosition()
    {
        return m_CeilingCheck.position;
    }
    public Vector3 getBottomPosition()
    {
        return m_GroundCheck.position;
    }

    public bool getIsTalking()
    {
        return isTalking;
    }

    public bool getIsPaused()
    {
        return pause;
    }

    public void Say(string message, float time = 2.5f)
    {
        if (message == "") return;
        ClearSay();
        DialogBox db = gameObject.AddComponent(typeof(DialogBox)) as DialogBox;
        db.title = CharacterName;
        db.text = message;
        db.followTop = getDialogTop();
        db.followBottom = getDialogBottom();
        db.isAuto = true;
        db.stayOnScreen = false;
        db.dialogParent = null;
        db.autoSelfDestructTimer = time;
        db.dialogSound = GetDialogSound();
        sayDialogBox = db;
        isTalking = true;
        Invoke("ClearSay", time);
    }

    //This function clears the say command. It is either called when the Say command has ran it's course and the timer has ran out, or it can be called manually to get rid of a Say dialog
    public void ClearSay()
    {
        if (sayDialogBox == null) return;
        isTalking = false;
        sayDialogBox.closeBox(); //This will initiate the process of killing the dialog box. Note: The db's autoSelfDestructTimer will do this anyway if the say timer has ran out, but we will do it implicity here as well in case we are explicitly bailing from a Say command.
        sayDialogBox = null;
    }

    //Overrides Say by using an array of strings rather than a single string. Picks a random string out of the array to say.
    public void Say(string[] messages, float time = 2.5f)
    {
        if (messages.Length <= 0) return;
        int i = UnityEngine.Random.Range(0, messages.Length - 1);

        Say(messages[i], time);
    }

    //Internal helper. Searches for the UI element that displays the text name of the object we are holding and assigns it to holdingUIText.
    private void getHoldingUI()
    {
        GameObject overlaygo = GameObject.FindWithTag("GameplayUIOverlay");
        if (overlaygo) uiOverlay = overlaygo.GetComponent<UIOverlayScript>();
    }

    private void setHoldingUIText(string text)
    {
        if (gameObject.tag == "Player")
        {
            if (!uiOverlay) getHoldingUI();
            if (uiOverlay)
            {
                uiOverlay.setHoldingText(text);
            }
        }
    }

    public AudioClip GetDialogSound() { return dialogSound; }

    //These are called by objects (like rc vehicles, guided missiles, ect) when control has been taken from this player and given back (resumed).
    public void onControlTaken()
    {
        controlTaken = true;
        m_Rigidbody2D.velocity = new Vector3(0f, m_Rigidbody2D.velocity.y, 0f);
    }
    public void onControlResumed()
    {
        controlTaken = false;
    }
}