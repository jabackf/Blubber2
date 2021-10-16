using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Provide a list of objects, and this ai script will attempt to keep them all picked up and placed back in their start positions.
//A little bit like Untitled Goose Game. If another character is holding the object, we will follow them around, give them an angry look, 
//and optionally say things to them.

public class AI_KeepObjectsPickedUp : MonoBehaviour
{
	public bool active=true;	//Active must be true for us to check the objects. 
	private bool previous_active=false;
	public bool disableCpuTriggersOnActive = true; //If true, then cpuinput triggers will be disabled while this script is active. It will also enable triggers on deactivate.
	public float stateChangeDelay=0.5f; //When the state changes, we can have a delay before the character goes into action. This delay is applied to all state changes. So for example, if the player picks up an item there will be a brief delay before the character responds.
	private float stateChangeTimer=0f;
	
	public float acceptableDeviationFromHome = 0.5f; //This is how far the object can be moved away from it's home position before the character tries to pick it up.
	
	enum states 
	{
		idle,
		goingToObject,
		takingObjectHome,
		followingCharacter
	}
	private states state=states.idle;
	private states previousState=states.idle;
	
	public CPUInput cpu;
	
	[System.Serializable]
	public class pickupObj
	{
		public GameObject obj;
		[HideInInspector]public pickupObject po;
		[HideInInspector]public Vector3 homePosition;
	}
	
	public List<pickupObj> objects = new List<pickupObj>();
	private pickupObj currentObj;
	
	private UnityEvent pickupEvent = new UnityEvent();
	private UnityEvent placeEvent = new UnityEvent();
	
	public string playIncapOnIdle = ""; //This can be the name of an incap, or the special keywords "[random]" or "[all]" (cycle through all incaps in order, looping to the start at the end).
	private int incapIndex=-1; //Used when playIncapOnIdle is set to [all].
	
	[Space]
    [Header("When another character picks up item...")]
	public string sendMessageOnFollow=""; //This message will be sent to this gameObject. For example, you might send Angry because the player picked up your crap.
	public string sendMessageOnFollowEnd="";//Sends a message when the followingCharacter state ends.
	public float sayTimeOnFollow = 3f; //Time tha the say dialog is on screen.
	public List<string> sayOnFollowCharacter = new List<string>(); //Randomly say this stuff while we follow a character that has picked up our item!
	
	
    // Start is called before the first frame update
    void Start()
    {
        cpu.onFollowPickupCallbacks.Add(pickupEvent);
		cpu.onPlaceItemCallbacks.Add(placeEvent);
		
		foreach(var o in objects)
		{
			if (o.obj)
			{
				o.homePosition = o.obj.transform.position;
				o.po = o.obj.GetComponent<pickupObject>();
			}
		}
		
		if (active) activate();
    }

    // Update is called once per frame
    void Update()
    {
		if (!active) 
			return;
		
		if (stateChangeTimer>0)
		{
			stateChangeTimer-=Time.deltaTime;
			cpu.followObjectActive = false;
			if (stateChangeTimer<=0) cpu.followObjectActive = true;
			else return;
		}
		
		if (state==states.idle)
		{
			foreach (var o in objects)
			{
				if (o.obj)
				{
					if (Vector2.Distance(o.obj.transform.position,o.homePosition)>acceptableDeviationFromHome)
					{
						Debug.Log(o.obj.transform.position+", "+o.homePosition+", "+Vector2.Distance(o.obj.transform.position,o.homePosition));
						currentObj = o;
						setTargetTransform();
						break;
					}
				}
			}
		}
		else
		{
			if (!currentObj.obj) state=states.idle;
		}
		
		if (state==states.followingCharacter)
		{
			if (currentObj.po && currentObj.obj)
			{
				if (currentObj.po.getHolder()==null)
				{
					setTargetTransform();
				}
			}
		}
		
		if (state==states.goingToObject)
		{
			if (currentObj.po && currentObj.obj)
			{
				if (currentObj.po.getHolder()!=null) //Someone has picked it up
				{
					setTargetTransform();
				}
			}
		}
		
		if (previousState==states.followingCharacter && state!=states.followingCharacter)
		{
			if (sendMessageOnFollowEnd!="") gameObject.SendMessage(sendMessageOnFollowEnd, SendMessageOptions.DontRequireReceiver);
		}
		
		if (state==states.idle)
		{
			if (previousState!=states.idle) incapIndex=-1; //Reset the incap index every time we go back into idle.
			if (playIncapOnIdle!="" && !cpu.currentlyPlayingIncap())
			{
				if (playIncapOnIdle=="[random]")
				{
					int i = (int)Mathf.Floor(Random.Range(0,cpu.incapAnimations.Count));
					cpu.playIncap(cpu.incapAnimations[i].name);
				}
				else if (playIncapOnIdle=="[all]")
				{
					incapIndex+=1;
					if (incapIndex>=cpu.incapAnimations.Count) incapIndex=0;
					cpu.playIncap(cpu.incapAnimations[incapIndex].name);
				}
				else
				{
					cpu.playIncap(playIncapOnIdle);
				}
			}
		}
		
		if (state!=previousState)
			stateChangeTimer=stateChangeDelay;
		
		previousState=state;
    }
	
	//This function sets cpu.target transform. It determines if a character is holding the object or not, then sets the target to either the character or the obj.
	public void setTargetTransform()
	{
		if (currentObj.po && currentObj.obj)
		{
			cpu.stopIncap(true);
			cpu.state=CPUInput.states.idle;
			GameObject holder = currentObj.po.getHolder();
			if (holder)
			{
				state=states.followingCharacter;
				cpu.followObjectTransform=holder.transform;
				cpu.followObjectDistanceMin=1.5f;
				cpu.pickupOnArrival=false;
				if (sendMessageOnFollow!="") gameObject.SendMessage(sendMessageOnFollow, SendMessageOptions.DontRequireReceiver);
				if (sayOnFollowCharacter.Count>0) onFollowSay();
			}
			else
			{
				cpu.followObjectTransform=currentObj.obj.transform;
				cpu.followObjectDistanceMin=0.5f;
				state=states.goingToObject;
				cpu.pickupOnArrival=true;
			}
		}
	}
	
	//This makes the character say stuff while we are following the player around cuz he's holding our crap.
	private void onFollowSay()
	{
		if (state!=states.followingCharacter) return;
		if (sayOnFollowCharacter.Count<=0) return;
		cpu.bid.controller.Say(sayOnFollowCharacter.ToArray(), sayTimeOnFollow);
		Invoke("onFollowSay",sayTimeOnFollow+Random.Range(1.5f,3f));
	}
	
	public void activate()
	{
		active=true;
		cpu.state=CPUInput.states.idle;
		cpu.followObjectActive = true;
		cpu.pickupOnArrival=true;
		cpu.followObjectTransform=null;
		if (disableCpuTriggersOnActive) cpu.useTriggers=false;
		
		pickupEvent.AddListener(onPickup);
		placeEvent.AddListener(onPlace);
		state=states.idle;
		
		stateChangeTimer=0;
	}
	
	
	public void deactivate()
	{
		active=false;
		cpu.state=CPUInput.states.idle;
		cpu.followObjectActive = false;
		if (disableCpuTriggersOnActive) cpu.useTriggers=true;
		pickupEvent.RemoveListener(onPickup);
		placeEvent.RemoveListener(onPlace);
	}
	
	//This functions is called when items are picked up
	public void onPickup()
	{
		if (state==states.goingToObject)
		{
			state=states.takingObjectHome;
			cpu.placeItem(currentObj.homePosition);
		}
	}
	
	//This function is called when items are placed.
	public void onPlace()
	{
		state=states.idle;
		cpu.followObjectTransform=null;
	}

	public void OnValidate()
	{
		if (active && !previous_active)
			activate();
		if (!active && previous_active)
			deactivate();
		previous_active = active;
	}
}
