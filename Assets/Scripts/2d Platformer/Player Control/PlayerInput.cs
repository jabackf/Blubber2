using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Handles keyboard input for a standard 2d platformer character.
 */

[RequireComponent(typeof(CharacterController2D))]

public class PlayerInput : MonoBehaviour
{

    public CharacterController2D controller;
    float horizontalMove = 0f;
    float aimAngleMove = 200f;    //Used in aiming the throw retical (angle)
    float aimForceMove = 15f;    //Used in aiming the throw retical (force)
    bool throwRelease = false;
    public float runSpeed = 35f;
    public float aimAngleSpeed = 200f; //Speed for aiming the angle of the throwing retical
    public float aimForceSpeed = 50f; //Speed for aiming the angle of the throwing retical
    bool jump = false;
    bool crouch = false;
    bool pickup = false;
    bool dropDown = false; //The action for dropping through platforms that have the dropDownPlatform script
    float climb = 0;
    public float climbSpeed = 5f;
    bool holdingAction = false;
    private bool isThrowing = false; //Set to true if we're throwing an object (changing the throw angle and velocity).

    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
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

            if (Input.GetButtonDown("Jump"))
            {
                if (jump != true)
                    jump = true;
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

    }

    void FixedUpdate()
    {
        if (!isThrowing)
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, pickup, climb * Time.fixedDeltaTime, dropDown);
        else
        {
            controller.Aim(aimForceMove * Time.fixedDeltaTime, aimAngleMove * Time.fixedDeltaTime, throwRelease, holdingAction);
            controller.Move(horizontalMove * Time.fixedDeltaTime, false, false, false, 0,false);
            if (throwRelease) isThrowing = false;
        }
        jump = false;
        pickup = false;
        holdingAction = false;
        throwRelease = false;
        dropDown = false;
    }

    public void OnLanding()
    {

    }
    public void OnCrouch()
    {

    }
}
