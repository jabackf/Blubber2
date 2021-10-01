using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used by BlubberInputInterface (used for CPU characters) and PlayerInput (used for player control). This stores input data that is used to operate the character controller.

public class BlubberInputData
{
	//Each input has a corresponding incap (input capture) ID. Incap is the system used by the BlubberInputInterface for recording input and storing it in a file to be replayed.
	
    public float horizontalMove = 0f;  //IncapID = 0
    public float aimAngleMove = 200f;    //Used in aiming the throw retical (angle) IncapID = 1
    public float aimForceMove = 15f;    //Used in aiming the throw retical (force) IncapID = 2
    public bool throwRelease = false; //IncapID = 3
    public bool mouseAim = true; //Set to true to allow the mouse to aim the throw or action reticals IncapID = 4

    public float aimActionAngleMove = 200f;    //Used in aiming the action retical (angle) IncapID = 5
    public float aimActionForceMove = 15f;    //Used in aiming the action retical (force) IncapID = 6

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
	
}
