using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Triggers animations for a 2d platformer character, also handles character dressings
 */

[RequireComponent(typeof(CharacterController2D))]

public class CharacterAnimation : MonoBehaviour
{

    public Animator animator;
    public CharacterController2D controller;

    //The following variables are mostly set automatically by the character controller, though a few are unset by this script. They give us info about the character's current state.
    public float speed = 0f; //Controls the speed of walking animations. Set by the character controller
    public float speedMultiplier = 1.5f; //Use to adjust the speed of the animation.
    public bool jump = false; //Stays true for duration of jump, until landing. Set and unset by the character controller.
    public bool doubleJump = false; //Only true for a fixed duration. Jump will also be true. Set by character controller, unset by this charAnim script
    public bool crouch = false; //True for the full duration of crouch. Set and unset by the character controller.
    public bool pushing = false; //True for the full duration of push. Set and unset by the character controller.
    public bool pickingUp = false; //Only true for a fixed duration, initiated after the player initiates an item pickup. Set by character controller, unset by this charAnim script
    public bool carryTop = false; //Set to true for the full duration of carry action. Set and unset by the character controller
    public bool carryFront = false; //Ditto
    public bool throwing = false; //Set to true after an object is thrown. Set by the character controller, unset by this charAnim script
    public bool climb = false; //Not yet implemented in character controller.

    public float doubleJumpDuration = 0.3f;  //The amount of time to play the double jump animation
    private float doubleJumpTimer = 0;

    public float pickingUpDuration = 0.2f;  //The amount of time to play the picking up animation
    private float pickingUpTimer = 0;

    public float throwingDuration = 0.2f;  //The amount of time to play the picking up animation
    private float throwingTimer = 0;

    public string crouchAnimatorBool = "IsCrouching"; //The Animator's boolean flag to set for crouching animations. Leave empty for no animation change.
    public string jumpAnimatorBool = "IsJumping"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string doubleJumpAnimatorBool = "IsDoubleJumping"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string pushingAnimatorBool = "IsPushing"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string speedAnimatorFloat = "Speed"; //The Animator's float to set for walking animations. Leave empty for no animation change.
    public string pickupAnimatorBool = "IsPickingUp"; //Character is picking something up
    public string throwingAnimatorBool = "IsThrowing"; //Releasing the throw button down.
    public string frontCarryAnimatorBool = "IsCarryingFront"; //Releasing the throw button down.
    public string topCarryAnimatorBool = "IsCarryingTop"; //Releasing the throw button down.
    public string climbingAnimatorBool = "IsClimbing"; //Climbing

    public string pushingDressSprite = ""; // A resource path to a sprite. If set, this sprite gets loaded and added as a child to the player while pushing
    private GameObject pushingDressSpriteObj = null;
    private int dressSortingOrder = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (pushingDressSprite != "" && controller.pushEnabled())
            pushingDressSpriteObj = addDress("pushingDress", pushingDressSprite);
    }

    void Update()
    {
        //Set speed for movement
        if (speedAnimatorFloat != "")
            animator.SetFloat(speedAnimatorFloat, Mathf.Abs(speed*speedMultiplier));

        if (crouchAnimatorBool != "")
            animator.SetBool(crouchAnimatorBool, crouch);


        if (jumpAnimatorBool != "")
            animator.SetBool(jumpAnimatorBool, jump);

        if (doubleJumpTimer > 0)
        {
            doubleJumpTimer -= Time.deltaTime;
            if (doubleJumpTimer <= 0)
            {
                doubleJump = false;
                doubleJumpTimer = 0;
            }
        }
        if (doubleJumpTimer == 0 && doubleJump) doubleJumpTimer = doubleJumpDuration;
        if (!doubleJump) doubleJumpTimer = 0;
        if (doubleJumpAnimatorBool != "")
            animator.SetBool(doubleJumpAnimatorBool, doubleJump);

        if (pushingAnimatorBool != "")
            animator.SetBool(pushingAnimatorBool, pushing);

        if (pickingUpTimer > 0)
        {
            pickingUpTimer -= Time.deltaTime;
            if (pickingUpTimer <= 0)
            {
                pickingUp = false;
                pickingUpTimer = 0;
            }
        }
        if (pickingUpTimer == 0 && pickingUp) pickingUpTimer = pickingUpDuration;
        if (!pickingUp) pickingUpTimer = 0;
        if (pickupAnimatorBool != "")
            animator.SetBool(pickupAnimatorBool, pickingUp);

        if (frontCarryAnimatorBool != "")
            animator.SetBool(frontCarryAnimatorBool, carryFront);
        if (topCarryAnimatorBool != "")
            animator.SetBool(topCarryAnimatorBool, carryTop);

        if (throwingTimer > 0)
        {
            throwingTimer -= Time.deltaTime;
            if (throwingTimer <= 0)
            {
                throwing = false;
                throwingTimer = 0;
            }
        }
        if (throwingTimer == 0 && throwing) throwingTimer = throwingDuration;
        if (!throwing) throwingTimer = 0;
        if (throwingAnimatorBool != "")
            animator.SetBool(throwingAnimatorBool, throwing);
    }

    public void animateOnMove(float move, bool crouch, bool jump, bool pickup = false)
    {/*

        if (controller.pickupEnabled())
        {
            if (Input.GetButtonDown("Pickup"))
            {
                if (!controller.holdingSomething())
                {
                    pickup = true;
                    if (pickupAnimatorBool != "")
                        animator.SetBool(pickupAnimatorBool, true);
                }
            }
            if (Input.GetButtonDown("Throw"))
            {
                if (controller.holdingSomething())
                {
                    isThrowing = true;
                    //controller.Move(0, false, false, false); //Stop moving if we're walking
                    //Debug.Log("Throwing mode ACTIVE");
                    if (prepThrowAnimatorBool != "")
                        animator.SetBool(prepThrowAnimatorBool, true);
                }
            }
        }

        if (!isThrowing && controller.pickupEnabled())
        {

            if (Input.GetButtonDown("Jump"))
            {
                if (jump != true)
                {
                    jump = true;
                    if (jumpAnimatorBool != "")
                        animator.SetBool(jumpAnimatorBool, true);
                }
            }


            if (controller.crouchEnabled())
            {
                if (Input.GetButtonDown("Crouch"))
                {
                    crouch = true;
                }
                else if (Input.GetButtonUp("Crouch"))
                {
                    crouch = false;
                }

                if (crouchAnimatorBool != "")
                    animator.SetBool(crouchAnimatorBool, crouch);

                if (pushingAnimatorBool != "")
                    animator.SetBool(pushingAnimatorBool, controller.isPushingSomething());
            }
            if (pushingDressSpriteObj != null)
            {
                dressShowHide(pushingDressSpriteObj, controller.isPushingSomething());
            }
        }//end !isThrowing
        else
        {//We're throwing something
            if (!Input.GetButton("Throw"))
            {
                throwRelease = true;
                //Debug.Log("Throwing mode INACTIVE");
                if (ThrowingAnimatorBool != "")
                    animator.SetBool(ThrowingAnimatorBool, true);
            }

            if (!Input.GetButton("throwAimHorizontal"))
            {   //Changing the angle
                aimAngleMove = Input.GetAxisRaw("throwAimVertical") * aimAngleSpeed;
                aimForceMove = 0;
            }
            else
            {   //Changing the force
                aimForceMove = Input.GetAxisRaw("throwAimVertical") * aimForceSpeed;
                aimAngleMove = 0;
            }

            if (!Input.GetButtonDown("holdingAction"))
            {
                holdingAction = true;
            }
        }*/

    }

    public GameObject addDress(string name, string resourcePath, string layerName = "CharacterDress", int m_dressSortingOrder = -1)
    {
        if (m_dressSortingOrder == -1)
        {
            m_dressSortingOrder = dressSortingOrder;
            dressSortingOrder++;
        }
        GameObject dress = new GameObject();
        dress.name = name;
        dress.transform.parent = gameObject.transform;
        dress.transform.localPosition = new Vector3(0f, 0f, 0f);
        dress.layer = LayerMask.NameToLayer(layerName);
        SpriteRenderer renderer = dress.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load(resourcePath, typeof(Sprite)) as Sprite;
        renderer.sortingLayerName = layerName;
        return dress;
    }
    public void dressShowHide(GameObject dress, bool show)
    {
        dress.GetComponent<SpriteRenderer>().enabled = show;
    }

    void FixedUpdate()
    {/*
        if (!isThrowing)
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, pickup);
        else
        {
            controller.Aim(aimForceMove * Time.fixedDeltaTime, aimAngleMove * Time.fixedDeltaTime, throwRelease, holdingAction);
            controller.Move(horizontalMove * Time.fixedDeltaTime, false, false, false);
            if (throwRelease) isThrowing = false;
        }
        jump = false;
        pickup = false;
        holdingAction = false;
        throwRelease = false;*/
    }

}
