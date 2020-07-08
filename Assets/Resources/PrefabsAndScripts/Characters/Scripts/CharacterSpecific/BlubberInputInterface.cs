using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*This is used for activating basic Blubber actions, like jump, walk, push, ect.
 * This is used mainly for AI or animators to control a blubber character.
 * It's basically the PlayerInput script, but stripped of the input checking.
 * The variables (like jump, climb, ect.) can be triggered to activate the corresponding actions
 */

public class BlubberInputInterface : MonoBehaviour
{
    public CharacterController2D controller;
    public float horizontalMove = 0f;
    float aimAngleMove = 200f;    //Used in aiming the throw retical (angle)
    float aimForceMove = 15f;    //Used in aiming the throw retical (force)
    bool throwRelease = false;
    public float aimAngleSpeed = 200f; //Speed for aiming the angle of the throwing retical
    public float aimForceSpeed = 50f; //Speed for aiming the angle of the throwing retical

    float aimActionAngleMove = 200f;    //Used in aiming the action retical (angle)
    float aimActionForceMove = 15f;    //Used in aiming the action retical (force)
    public float aimActionAngleSpeed = 200f; //Speed for aiming the angle of the action retical
    public float aimActionForceSpeed = 50f; //Speed for aiming the angle of the action retical

    public bool jump = false;
    public bool crouch = false;
    public bool pickup = false;
    public bool dialog = false;    //Dialog button pressed
    public bool dropDown = false; //The action for dropping through platforms that have the dropDownPlatform script
    bool useItemActionPressed = false; //If we're holding an object with an action, then this flag is triggered when we press the UseItemAction button
    bool useItemActionHeld = false;
    bool useItemActionReleased = false; public float climb = 0;
    public float climbSpeed = 5f;
    public bool holdingAction = false;
    private bool isThrowing = false; //Set to true if we're throwing an object (changing the throw angle and velocity).

    private float jumpCounter = 0; //Used to make sure we're not jumping repeatedly

    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {


    }

    void FixedUpdate()
    {
        if (jumpCounter > 0) jump = false; //Used to make sure we don't get the jump signal repeatedly for several frames.

        if (!controller.isTalking && !controller.isCharacterDead()) //Not in dialog mode, not dead
        {
            if (!isThrowing)
            {
                controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, pickup, climb * Time.fixedDeltaTime, dropDown, dialog);
                if (useItemActionPressed || useItemActionHeld || useItemActionReleased) controller.useItemAction(useItemActionPressed, useItemActionHeld, useItemActionReleased, aimActionForceMove * Time.fixedDeltaTime, aimActionAngleMove * Time.fixedDeltaTime);
            }
            else
            {
                controller.Aim(aimForceMove * Time.fixedDeltaTime, aimAngleMove * Time.fixedDeltaTime, throwRelease, holdingAction);
                controller.Move(horizontalMove * Time.fixedDeltaTime, false, false, false, 0, false, false);
                if (throwRelease) isThrowing = false;
            }
        }


        if (jump)
        {
            jump = false;
            if (jumpCounter == -1) jumpCounter = 0.3f;
        }
        if (jumpCounter > 0) jumpCounter -= Time.fixedDeltaTime;
        if (jumpCounter <= 0) jumpCounter = -1;

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
