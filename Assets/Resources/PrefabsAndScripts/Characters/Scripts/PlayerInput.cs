﻿using System.Collections;
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

    public float runSpeed = 35f;
    public float aimAngleSpeed = 200f; //Speed for aiming the angle of the throwing retical
    public float aimForceSpeed = 50f; //Speed for aiming the angle of the throwing retical
    public bool mouseAim = true; //Set to true to allow the mouse to aim the throw or action reticals

    public float aimActionAngleSpeed = 200f; //Speed for aiming the angle of the action retical
    public float aimActionForceSpeed = 50f; //Speed for aiming the angle of the action retical


    public float climbSpeed = 5f;
	
	[HideInInspector] public CharacterController2D_Input bid;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        //if (mouseAim) controller.setMouseAim(mouseAim);
		
		if (controller!=null) Init(controller);
    }
	
	//Calls cc2d_input.init again. 
	public void Init(CharacterController2D cont)
	{
		if (bid==null) bid = new CharacterController2D_Input();
		controller=cont;
		bid.Init(cont);
	}

    void Update()
    {
		//Get input
		if (requirePlayerTag == false || gameObject.tag == "Player")
		{
			if (Input.GetButtonDown("Jump")) bid.jump = true;

			if (Input.GetButtonDown("Pickup")) bid.pickup = true;

			if (Input.GetButtonDown("Throw")) bid.isThrowing = true;
			if (Input.GetButtonUp("Throw")) bid.throwRelease = true;
			if (Input.GetButton("Throw"))
			{
				if (!Input.GetButton("throwAimHorizontal"))
                {   //Changing the angle
                    bid.aimAngleMove = Input.GetAxisRaw("throwAimVertical") * aimAngleSpeed;
                    bid.aimForceMove = 0;
                }
                else
                {   //Changing the force
                    bid.aimForceMove = Input.GetAxisRaw("throwAimVertical") * aimForceSpeed;
                    bid.aimAngleMove = 0;
                }
			}

            bid.horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
			
			if (Input.GetButtonDown("Dialog")) bid.dialog = true;

            if (Input.GetButtonDown("dropDown")) bid.dropDown = true;
			
			bid.climb = Input.GetAxisRaw("Climb") * climbSpeed;

			if (Input.GetButtonDown("holdingAction")) bid.holdingAction = true;
			
			
			if (Input.GetButtonDown("UseItemAction")) bid.useItemActionPressed = true;
			if (Input.GetButtonUp("UseItemAction")) bid.useItemActionReleased = true;
			
			//bid.aimActionAngleMove = 0;
			//bid.aimActionForceMove = 0;
			
			if (Input.GetButton("UseItemAction"))
			{
				bid.useItemActionHeld = true;
				//Since we're not in throwing mode and we have the action button pressed, these controls are used for action aim instead
				if (!Input.GetButton("throwAimHorizontal") )
				{   //Changing the angle
					bid.aimActionAngleMove = Input.GetAxisRaw("throwAimVertical") * aimActionAngleSpeed;
					bid.aimActionForceMove = 0;
				}
				else
				{   //Changing the force
					bid.aimActionForceMove = Input.GetAxisRaw("throwAimVertical") * aimActionForceSpeed;
					bid.aimActionAngleMove = 0;
				}
			}
			
			if (Input.GetKeyDown(KeyCode.Alpha0)) bid.inventorySlot = 0;
			if (Input.GetKeyDown(KeyCode.Alpha1)) bid.inventorySlot = 1;
			if (Input.GetKeyDown(KeyCode.Alpha2)) bid.inventorySlot = 2;
			if (Input.GetKeyDown(KeyCode.Alpha3)) bid.inventorySlot = 3;
			if (Input.GetKeyDown(KeyCode.Alpha4)) bid.inventorySlot = 4;
			if (Input.GetKeyDown(KeyCode.Alpha5)) bid.inventorySlot = 5;
			if (Input.GetKeyDown(KeyCode.Alpha6)) bid.inventorySlot = 6;
			if (Input.GetKeyDown(KeyCode.Alpha7)) bid.inventorySlot = 7;
			if (Input.GetKeyDown(KeyCode.Alpha8)) bid.inventorySlot = 8;
			if (Input.GetKeyDown(KeyCode.Alpha9)) bid.inventorySlot = 9;
		}
	
		bid.UpdateInputLogic();
		/*
        if (requirePlayerTag == false || gameObject.tag == "Player")
        {
            if (controller.pickupEnabled())
            {
                if (Input.GetButtonDown("Pickup"))
                {
                    if (!controller.holdingSomething())
                        bid.pickup = true;
                }
                if (Input.GetButtonDown("Throw"))
                {
                    if (controller.holdingSomething())
                        bid.isThrowing = true;
                }
            }

            bid.horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

            if (!bid.isThrowing) //Not in aiming mode
            {
                if (Input.GetButtonDown("UseItemAction"))
                {
                    bid.useItemActionPressed = true;
                }
                if (Input.GetButton("UseItemAction"))
                {
                    bid.useItemActionHeld = true;

                    //Since we're not in throwing mode and we have the action button pressed, these controls are used for action aim instead
                    if (!Input.GetButton("throwAimHorizontal") )
                    {   //Changing the angle
                        bid.aimActionAngleMove = Input.GetAxisRaw("throwAimVertical") * aimActionAngleSpeed;
                        bid.aimActionForceMove = 0;
                    }
                    else
                    {   //Changing the force
                        bid.aimActionForceMove = Input.GetAxisRaw("throwAimVertical") * aimActionForceSpeed;
                        bid.aimActionAngleMove = 0;
                    }
                }
                if (Input.GetButtonUp("UseItemAction"))
                {
                    bid.useItemActionReleased = true;
                }

                if (Input.GetButtonDown("Jump"))
                {
                    bid.jump = true;

                    //If we're using the actionAim system, then don't jump. This bit of code assumes jump is the same as action aim controls.
                    if (controller.holdingSomething() && controller.getHolding() != null)
                    {
                        if ( Input.GetButton("UseItemAction") && controller.getHolding().hasActionAim && !controller.currentlyMouseAiming )
                            bid.jump = false;
                    }

                }

                if (Input.GetButtonDown("Dialog"))
                {
                    bid.dialog = true;
                }

                if (Input.GetButtonDown("dropDown"))
                {
                    bid.dropDown = true;
                }

                if (controller.getCanClimb())
                {
                    bid.climb = Input.GetAxisRaw("Climb") * climbSpeed;
                }


                if (controller.crouchEnabled())
                {
                    if (Input.GetButtonDown("Crouch"))
                    {
                        bid.crouch = true;
                    }
                    else if (Input.GetButtonUp("Crouch"))
                    {
                        bid.crouch = false;
                    }

                }

            }//end !isThrowing
            else
            {//We're throwing something

                //If we're aiming the pickup retical with the mouse then we can still jump
                if (Input.GetButtonDown("Jump"))
                {
                    if (controller.currentlyMouseAiming)
                        bid.jump = true;
                }

                if (!Input.GetButton("Throw"))
                {
                    bid.throwRelease = true;
                }

                if (!Input.GetButton("throwAimHorizontal"))
                {   //Changing the angle
                    bid.aimAngleMove = Input.GetAxisRaw("throwAimVertical") * aimAngleSpeed;
                    bid.aimForceMove = 0;
                }
                else
                {   //Changing the force
                    bid.aimForceMove = Input.GetAxisRaw("throwAimVertical") * aimForceSpeed;
                    bid.aimAngleMove = 0;
                }

                if (!Input.GetButtonDown("holdingAction"))
                {
                    bid.holdingAction = true;
                }
            }
        }//END if (requirePlayerTag==false || gameObject.tag=="Player")
			*/
    }

    public void setMouseAim(bool set)
    {
        bid.mouseAim = set;
        bid.controller.setMouseAim(set);
    }

    void FixedUpdate()
    {
		/*
        if (!controller.pause && !controller.isCharacterDead() && !global.isSceneChanging()) //Not in dialog mode, not dead, scene isn't changing
        {
            if (!bid.isThrowing)
            {
                controller.Move(bid.horizontalMove * Time.fixedDeltaTime, bid.crouch, bid.jump, bid.pickup, bid.climb * Time.fixedDeltaTime, bid.dropDown, bid.dialog);
                if (bid.useItemActionPressed || bid.useItemActionHeld || bid.useItemActionReleased) controller.useItemAction(bid.useItemActionPressed,bid.useItemActionHeld,bid.useItemActionReleased, bid.aimActionForceMove * Time.fixedDeltaTime, bid.aimActionAngleMove * Time.fixedDeltaTime);
            }
            else
            {
                controller.Aim(bid.aimForceMove * Time.fixedDeltaTime, bid.aimAngleMove * Time.fixedDeltaTime, bid.throwRelease, bid.holdingAction);
                controller.Move(bid.horizontalMove * Time.fixedDeltaTime, false, bid.jump, false, 0, false, false);
                if (bid.throwRelease) bid.isThrowing = false;
            }
        }
        bid.jump = false;
        bid.pickup = false;
        bid.holdingAction = false;
        bid.throwRelease = false;
        bid.dropDown = false;
        bid.dialog = false;
        bid.useItemActionPressed = false;
        bid.useItemActionHeld = false;
        bid.useItemActionReleased = false;
		*/
		
		bid.UpdateCharacterController();
    }

    public void OnLanding()
    {

    }
    public void OnCrouch()
    {

    }
	
}
