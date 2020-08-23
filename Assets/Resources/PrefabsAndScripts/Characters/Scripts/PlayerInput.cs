using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Handles keyboard input for a standard 2d platformer character.
 */

[RequireComponent(typeof(CharacterController2D))]

public class PlayerInput : MonoBehaviour
{
    public CharacterController2D controller;
    private Global global;
    public bool requirePlayerTag = true; //If true, input will only function if this gameObject has the "Player" tag
    float horizontalMove = 0f;
    float aimAngleMove = 200f;    //Used in aiming the throw retical (angle)
    float aimForceMove = 15f;    //Used in aiming the throw retical (force)
    bool throwRelease = false;
    public float runSpeed = 35f;
    public float aimAngleSpeed = 200f; //Speed for aiming the angle of the throwing retical
    public float aimForceSpeed = 50f; //Speed for aiming the angle of the throwing retical
    public bool mouseAim = true; //Set to true to allow the mouse to aim the throw or action reticals

    float aimActionAngleMove = 200f;    //Used in aiming the action retical (angle)
    float aimActionForceMove = 15f;    //Used in aiming the action retical (force)
    public float aimActionAngleSpeed = 200f; //Speed for aiming the angle of the action retical
    public float aimActionForceSpeed = 50f; //Speed for aiming the angle of the action retical

    bool jump = false;
    bool crouch = false;
    bool pickup = false;
    bool dialog = false;    //Dialog button pressed
    bool dropDown = false; //The action for dropping through platforms that have the dropDownPlatform script
    bool useItemActionPressed = false; //If we're holding an object with an action, then this flag is triggered when we press the UseItemAction button
    bool useItemActionHeld = false;
    bool useItemActionReleased = false;
    float climb = 0;
    public float climbSpeed = 5f;
    bool holdingAction = false;
    private bool isThrowing = false; //Set to true if we're throwing an object (changing the throw angle and velocity).
    

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (mouseAim) controller.setMouseAim(mouseAim);
    }

    void Update()
    {


        if (requirePlayerTag == false || gameObject.tag == "Player")
        {
            if (controller.pickupEnabled())
            {
                if (Input.GetButtonDown("Pickup"))
                {
                    if (!controller.holdingSomething())
                        pickup = true;
                }
                if (Input.GetButtonDown("Throw"))
                {
                    if (controller.holdingSomething())
                        isThrowing = true;
                }
            }

            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

            if (!isThrowing) //Not in aiming mode
            {
                if (Input.GetButtonDown("UseItemAction"))
                {
                    useItemActionPressed = true;
                }
                if (Input.GetButton("UseItemAction"))
                {
                    useItemActionHeld = true;

                    //Since we're not in throwing mode and we have the action button pressed, these controls are used for action aim instead
                    if (!Input.GetButton("throwAimHorizontal"))
                    {   //Changing the angle
                        aimActionAngleMove = Input.GetAxisRaw("throwAimVertical") * aimActionAngleSpeed;
                        aimActionForceMove = 0;
                    }
                    else
                    {   //Changing the force
                        aimActionForceMove = Input.GetAxisRaw("throwAimVertical") * aimActionForceSpeed;
                        aimActionAngleMove = 0;
                    }
                }
                if (Input.GetButtonUp("UseItemAction"))
                {
                    useItemActionReleased = true;
                }

                if (Input.GetButtonDown("Jump"))
                {
                    jump = true;

                    //If we're using the actionAim system, then don't jump. This bit of code assumes jump is the same as action aim controls.
                    if (controller.holdingSomething() && controller.getHolding() != null)
                    {
                        if (Input.GetButton("UseItemAction") && controller.getHolding().hasActionAim)
                            jump = false;
                    }

                }

                if (Input.GetButtonDown("Dialog"))
                {
                    dialog = true;
                }

                if (Input.GetButtonDown("dropDown"))
                {
                    dropDown = true;
                }

                if (controller.getCanClimb())
                {
                    climb = Input.GetAxisRaw("Climb") * climbSpeed;
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

                }

            }//end !isThrowing
            else
            {//We're throwing something
                if (!Input.GetButton("Throw"))
                {
                    throwRelease = true;
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
            }
        }//END if (requirePlayerTag==false || gameObject.tag=="Player")
    }

    public void setMouseAim(bool set)
    {
        mouseAim = set;
        controller.setMouseAim(set);
    }

    void FixedUpdate()
    {

        if (!controller.pause && !controller.isCharacterDead() && !global.isSceneChanging()) //Not in dialog mode, not dead, scene isn't changing
        {
            if (!isThrowing)
            {
                controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, pickup, climb * Time.fixedDeltaTime, dropDown, dialog);
                if (useItemActionPressed || useItemActionHeld || useItemActionReleased) controller.useItemAction(useItemActionPressed,useItemActionHeld,useItemActionReleased, aimActionForceMove * Time.fixedDeltaTime, aimActionAngleMove * Time.fixedDeltaTime);
            }
            else
            {
                controller.Aim(aimForceMove * Time.fixedDeltaTime, aimAngleMove * Time.fixedDeltaTime, throwRelease, holdingAction);
                controller.Move(horizontalMove * Time.fixedDeltaTime, false, false, false, 0, false, false);
                if (throwRelease) isThrowing = false;
            }
        }
        jump = false;
        pickup = false;
        holdingAction = false;
        throwRelease = false;
        dropDown = false;
        dialog = false;
        useItemActionPressed = false;
        useItemActionHeld = false;
        useItemActionReleased = false;
    }

    public void OnLanding()
    {

    }
    public void OnCrouch()
    {

    }
}
