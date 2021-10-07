using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used by CPUInput (used for CPU characters) and PlayerInput (used for player control). This stores input data that is used to operate the character controller.
//This script does not get directly applied. An instance of this script is created and used by CPUInput or PlayerInput to interface with the character controller.

public class CharacterController2D_Input
{
	//Each input has a corresponding incap (input capture) ID. Incap is the system used by the CPUInput for recording input and storing it in a file to be replayed.
	
    public float horizontalMove = 0f;  //IncapID = 0
    public float aimAngleMove = 0f;    //Used in aiming the throw retical (angle) IncapID = 1
    public float aimForceMove = 0f;    //Used in aiming the throw retical (force) IncapID = 2
    public bool throwRelease = false; //IncapID = 3
    public bool mouseAim = true; //Set to true to allow the mouse to aim the throw or action reticals IncapID = 4

    public float aimActionAngleMove = 0f;    //Used in aiming the action retical (angle) IncapID = 5
    public float aimActionForceMove = 0f;    //Used in aiming the action retical (force) IncapID = 6

    public bool jump = false; //IncapID = 7
    public bool crouch = false; //IncapID = 8
    public bool pickup = false; //IncapID = 9
    public bool dialog = false;    //Dialog button pressed IncapID = 10
    public bool dropDown = false; //The action for dropping through platforms that have the dropDownPlatform script IncapID = 11
    public bool useItemActionPressed = false; //If we're holding an object with an action, then this flag is triggered when we press the UseItemAction button IncapID = 12
    public bool useItemActionHeld = false; //IncapID = 13
    public bool useItemActionReleased = false; //IncapID = 14
    public float climb = 0; //IncapID = 15
    public bool holdingAction = false; //IncapID = 16
    public bool isThrowing = false; //Set to true if we're throwing an object (changing the throw angle and velocity). IncapID = 17
	
	public int incapCount = 18; //The number of unique inputs with incap IDs.
	
	public List<float> initialValues = new List<float>();	//This stores the initial state of the input controller. This is used when the input controller is reset.
	
	public CharacterController2D controller;
	private Global global;
	
	//This should be called after setting the initial variables, before calling other functions.
	public void Init(CharacterController2D controller)
	{
		global = GameObject.FindWithTag("global").GetComponent<Global>();
		this.controller=controller;
		if (this.mouseAim) this.controller.setMouseAim(this.mouseAim);
		
		this.initialValues.Clear();
		
		for (var i=0; i<this.incapCount; i++)
		{
			this.initialValues.Add(this.incapGetValue(i));
		}
	}
	
	//Resets the inputs to the values the were at the time Init() was called.
	public void reset()
	{
		for (var i=0; i<incapCount; i++)
		{
			this.incapSetValue(i,this.initialValues[i]);
		}
		
		//We had better send a button released message to useItem because if we were holding a button down when this was called the action could get stuck.
		controller.useItemAction(false,false,true);
		controller.resetActionAim();
	}
	
	//This should be called in Update
	public void UpdateInputLogic()
	{
		if (controller.pickupEnabled())
		{
			if (pickup==true && controller.holdingSomething()) pickup=false;
			if (isThrowing==true && !controller.holdingSomething()) this.isThrowing=false;
		}
		else
		{
			this.pickup=false;
			this.isThrowing=false;
		}
		
		if (!controller.getCanClimb())
		{
			climb=0f;
		}
		
		if (!controller.crouchEnabled())
		{
			this.crouch=false;
		}
		
		if (this.isThrowing)
		{
			this.useItemActionPressed = false;
			this.useItemActionHeld = false;
			this.useItemActionReleased = false;
			
			if (this.jump==true  && !controller.currentlyMouseAiming) jump=false;
			
		}
		else //Not throwing
		{
			if (this.useItemActionPressed) this.useItemActionHeld=true;
			if (this.useItemActionReleased) this.useItemActionHeld=false;
			
			if (this.jump==true)
			{
				//If we're using the actionAim system, then don't jump. This bit of code assumes jump is the same as action aim controls.
				if (controller.holdingSomething() && controller.getHolding() != null)
				{
					if ( this.useItemActionHeld && controller.getHolding().hasActionAim && !controller.currentlyMouseAiming )
						this.jump = false;
				}

			}

		} //end !isThrowing
	}
	
	//This should get called in FixedUpdate
	public void UpdateCharacterController()
	{
		if (!controller.pause && !controller.isCharacterDead() && !global.isSceneChanging()) //Not in dialog mode, not dead, scene isn't changing
        {
            if (!this.isThrowing)
            {
                controller.Move(this.horizontalMove * Time.fixedDeltaTime, this.crouch, this.jump, this.pickup, this.climb * Time.fixedDeltaTime, this.dropDown, this.dialog);
                if (this.useItemActionPressed || this.useItemActionHeld || this.useItemActionReleased) controller.useItemAction(this.useItemActionPressed,this.useItemActionHeld,this.useItemActionReleased, this.aimActionForceMove * Time.fixedDeltaTime, this.aimActionAngleMove * Time.fixedDeltaTime);
            }
            else
            {
                controller.Aim(this.aimForceMove * Time.fixedDeltaTime, this.aimAngleMove * Time.fixedDeltaTime, this.throwRelease, this.holdingAction);
                controller.Move(this.horizontalMove * Time.fixedDeltaTime, false, this.jump, false, 0, false, false);
                if (this.throwRelease) this.isThrowing = false;
            }
        }
        this.jump = false;
        this.pickup = false;
        this.holdingAction = false;
        this.throwRelease = false;
        this.dropDown = false;
        this.dialog = false;
        this.useItemActionPressed = false;
        this.useItemActionReleased = false;
	}

	//Returns the value (converted to a float) stored in the input with the corresponding incap id.
	//For example, if jump corresponds with 7, then incapGetValue(7) will return either 0 (false) or 1 (true) based on the value stored in jump
	public float incapGetValue(int incapID)
	{
		switch (incapID)
		{
			case 0:
				return this.horizontalMove;
				break;
			case 1:
				return this.aimAngleMove;
				break;
			case 2:
				return this.aimForceMove;
				break;
			case 3:
				return (this.throwRelease ? 1f : 0f);
				break;
			case 4:
				return (this.mouseAim ? 1f : 0f);
				break;
			case 5:
				return this.aimActionAngleMove;
				break;
			case 6:
				return this.aimActionForceMove;
				break;
			case 7:
				return (this.jump ? 1f : 0f);
				break;
			case 8:
				return (this.crouch ? 1f : 0f);
				break;
			case 9:
				return (this.pickup ? 1f : 0f);
				break;
			case 10:
				return (this.dialog ? 1f : 0f);
				break;
			case 11:
				return (this.dropDown ? 1f : 0f);
				break;
			case 12:
				return (this.useItemActionPressed ? 1f : 0f);
				break;
			case 13:
				return (this.useItemActionHeld ? 1f : 0f);
				break;
			case 14:
				return (this.useItemActionReleased ? 1f : 0f);
				break;
			case 15:
				return this.climb;
				break;
			case 16:
				return (this.holdingAction ? 1f : 0f);
				break;
			case 17:
				return (this.isThrowing ? 1f : 0f);
				break;
		}
		return -1;
	}
	
	public void incapSetValue(int incapID, float newValue)
	{
		switch (incapID)
		{
			case 0:
				this.horizontalMove=newValue;
				break;
			case 1:
				this.aimAngleMove=newValue;
				break;
			case 2:
				this.aimForceMove=newValue;
				break;
			case 3:
				this.throwRelease = (newValue==1f ? true : false);
				break;
			case 4:
				this.mouseAim = (newValue==1f ? true : false);
				break;
			case 5:
				this.aimActionAngleMove=newValue;
				break;
			case 6:
				this.aimActionForceMove=newValue;
				break;
			case 7:
				this.jump  = (newValue==1f ? true : false);
				break;
			case 8:
				this.crouch  = (newValue==1f ? true : false);
				break;
			case 9:
				this.pickup  = (newValue==1f ? true : false);
				break;
			case 10:
				this.dialog  = (newValue==1f ? true : false);
				break;
			case 11:
				this.dropDown  = (newValue==1f ? true : false);
				break;
			case 12:
				this.useItemActionPressed  = (newValue==1f ? true : false);
				break;
			case 13:
				this.useItemActionHeld  = (newValue==1f ? true : false);
				break;
			case 14:
				this.useItemActionReleased  = (newValue==1f ? true : false);
				break;
			case 15:
				this.climb=newValue;
				break;
			case 16:
				this.holdingAction  = (newValue==1f ? true : false);
				break;
			case 17:
				this.isThrowing  = (newValue==1f ? true : false);
				break;
		}
	}
	
	//Pass an incap id and will return a string with the name of the variable.
	public string incapIDToName(int incapID)
	{
		switch (incapID)
		{
			case 0:
				return "horizontalMove";
				break;
			case 1:
				return "aimAngleMove";
				break;
			case 2:
				return "aimForceMove";
				break;
			case 3:
				return "throwRelease";
				break;
			case 4:
				return "mouseAim";
				break;
			case 5:
				return "aimActionAngleMove";
				break;
			case 6:
				return "aimActionForceMove";
				break;
			case 7:
				return "jump";
				break;
			case 8:
				return "crouch";
				break;
			case 9:
				return "pickup";
				break;
			case 10:
				return "dialog";
				break;
			case 11:
				return "dropDown";
				break;
			case 12:
				return "useItemActionPressed";
				break;
			case 13:
				return "useItemActionHeld";
				break;
			case 14:
				return "useItemActionReleased";
				break;
			case 15:
				return "climb";
				break;
			case 16:
				return "holdingAction";
				break;
			case 17:
				return "isThrowing";
				break;
		}
		return "none";
	}
	
		//Pass an incap id and will return a string with the name of the variable.
	public int incapNameToID(string name)
	{
		switch (name)
		{
			case "horizontalMove":
				return 0;
				break;
			case "aimAngleMove":
				return 1;
				break;
			case "aimForceMove":
				return 2;
				break;
			case "throwRelease":
				return 3;
				break;
			case "mouseAim":
				return 4;
				break;
			case "aimActionAngleMove":
				return 5;
				break;
			case "aimActionForceMove":
				return 6;
				break;
			case "jump":
				return 7;
				break;
			case "crouch":
				return 8;
				break;
			case "pickup":
				return 9;
				break;
			case "dialog":
				return 10;
				break;
			case "dropDown":
				return 11;
				break;
			case "useItemActionPressed":
				return 12;
				break;
			case "useItemActionHeld":
				return 13;
				break;
			case "useItemActionReleased":
				return 14;
				break;
			case "climb":
				return 15;
				break;
			case "holdingAction":
				return 16;
				break;
			case "isThrowing":
				return 17;
				break;
		}
		return -1;
	}
	
	//For debugging. Prints the name and state of all inputs to the console.
	public void printState()
	{
		Debug.Log("Printing input state...");
		for (int i=0; i<incapCount; i++)
		{
			Debug.Log(incapIDToName(i)+": "+incapGetValue(i));
		}
	}
}

