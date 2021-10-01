using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Events;

/*This is used for activating basic Blubber actions, like jump, walk, push, ect.
 * This is used mainly for AI or animators to control a blubber character.
 * It's basically the PlayerInput script, but stripped of the input checking.
 * The variables (like jump, climb, ect.) can be triggered to activate the corresponding actions
 */

public class BlubberInputInterface : MonoBehaviour
{
	//An incap animation is just a list of incapKeyframes. A keyframe is created for every instance when a BlubberInputData field changes.
	[System.Serializable]
	public class incapKeyframe
	{
		public float time;	//The amount of time that has elapses between the start of the input cature and this particular keyframe
		public int id; //The incap id that corresponds to the BlubberInputData field that has changed.
		public float val; //The new value that the field will change to. For bools, this is 1 for true 0 for false.
		
		public incapKeyframe(float time, int id, float val)
		{
			this.time=time;
			this.id=id;
			this.val = val;
		}
	}
	
	[System.Serializable]
	public enum incapBehavior
	{
		goHome, //Move towards home.
		jumpHome, //Instantaneously jump hom
		maintain //Maintain either starts the animation from whatever our current location is if applied to behaviorAtStart, or it LOOPS the animation if applied to behaviorAtEnd
	}
	
	[System.Serializable]
	public class incapAnimation
	{
		public string name=""; //The name that will be used to reference this animation
		[HideInInspector] public List<incapKeyframe> keyframes;
		[HideInInspector] public bool loaded=false; //Set to true after the keyframes have been loaded.
		public TextAsset incapFile;
		public incapBehavior behaviorAtStart = incapBehavior.goHome; 
		public incapBehavior behaviorAtEnd = incapBehavior.goHome;
		public UnityEvent startCallback; //Called when the animation begins (after going home, right at the incap start)
		public UnityEvent endCallback; //Called when the animation ends (after incap ends but BEFORE going home)
		public UnityEvent endHomeCallback; //Called when the animation ends (after incap ends AND after going home)
		
		
		public void loadKeyframes()
		{
			BinaryFormatter bf = new BinaryFormatter();
			Stream stream = new MemoryStream(this.incapFile.bytes);
			this.keyframes = (List<incapKeyframe>)bf.Deserialize(stream);
			stream.Close();
			this.loaded=true;
		}
	}
	
	[Space]
    [Header("Input Capture")]
	//The following are used for input recording.
	public string saveFileName="InputCapture"; //The name of the file that will be recorded. It will be saved to global.dirInputCapture in resources. A number will be appended if the file exists. These are saved with bytes extension so they can be supplied in the inspector as a TextAsset.
	public bool record=false;
	bool recordInProgress=false;
	GameObject player;
	string myPreviousTag;
	float recordTime=0f;
	private GameObject homeMarker;
	private PlayerInput pi;
	private BlubberInputData previousBid; //Used for recording to detect when an input value has changed.
	private List<incapKeyframe> recordFrames; //This list stores the current keyframes that we are recording when recordInProgress is true.
	
	
	[Space]
    [Header("Input Playback")]
	
	private bool playingIncap=false;
	private incapAnimation currentIncap; //The incap that we are currently playing if playingIncap is true
	float incapPlayTime=0; //The amount of time that has elapsed since the start of the playback.
	int keyframeIndex=0; //This stores the index of the keyframe list that we are on for the currently playing incap. It's like the playhead.
	public List<incapAnimation> incapAnimations; //These are all of the incap animations for this character
	
	[Space]
    [Header("Movement")]
    public CharacterController2D controller;

    public float aimAngleSpeed = 200f; //Speed for aiming the angle of the throwing retical
    public float aimForceSpeed = 50f; //Speed for aiming the angle of the throwing retical
	public float runSpeed = 35f;
    public float aimActionAngleSpeed = 200f; //Speed for aiming the angle of the action retical
    public float aimActionForceSpeed = 50f; //Speed for aiming the angle of the action retical

    public float climbSpeed = 5f;

    private float jumpCounter = 0; //Used to make sure we're not jumping repeatedly
	
	[HideInInspector] public BlubberInputData bid;
	
	Global global;

    // Start is called before the first frame update
    void Start()
    {
		global = GameObject.FindWithTag("global").GetComponent<Global>();
		if (bid==null) bid = new BlubberInputData();
		
		foreach (var a in incapAnimations) a.loadKeyframes();
		
		playIncap("Test");
    }

    void Update()
    {
			if (playingIncap)
			{
				incapPlayTime+=Time.deltaTime;
				bool done=false;
				while (!done)
				{
					if (currentIncap.keyframes[keyframeIndex].time<=incapPlayTime)
					{
						bid.incapSetValue(currentIncap.keyframes[keyframeIndex].id,currentIncap.keyframes[keyframeIndex].val);
						keyframeIndex+=1;
						if (keyframeIndex>=currentIncap.keyframes.Count)
						{
							//Animation finished.
							keyframeIndex=0;
							Debug.Log("AnimationFinished");
							playingIncap=false;
							done=true;
						}
					}
					else
					{
						done=true;
					}
				}
			}
    }

    void FixedUpdate()
    {
		if (!recordInProgress)
		{
			
			if (jumpCounter > 0) bid.jump = false; //Used to make sure we don't get the jump signal repeatedly for several frames.

			if (!controller.pause && !controller.isCharacterDead()) //Not in pause mode, not dead
			{
				if (!bid.isThrowing)
				{
					controller.Move(bid.horizontalMove * Time.fixedDeltaTime, bid.crouch, bid.jump, bid.pickup, bid.climb * Time.fixedDeltaTime, bid.dropDown, bid.dialog);
					if (bid.useItemActionPressed || bid.useItemActionHeld || bid.useItemActionReleased) controller.useItemAction(bid.useItemActionPressed, bid.useItemActionHeld, bid.useItemActionReleased, bid.aimActionForceMove * Time.fixedDeltaTime, bid.aimActionAngleMove * Time.fixedDeltaTime);
				}
				else
				{
					controller.Aim(bid.aimForceMove * Time.fixedDeltaTime, bid.aimAngleMove * Time.fixedDeltaTime, bid.throwRelease, bid.holdingAction);
					controller.Move(bid.horizontalMove * Time.fixedDeltaTime, false, false, false, 0, false, false);
					if (bid.throwRelease) bid.isThrowing = false;
				}
			}


			if (bid.jump)
			{
				bid.jump = false;
				if (jumpCounter == -1) jumpCounter = 0.3f;
			}
			if (jumpCounter > 0) jumpCounter -= Time.fixedDeltaTime;
			if (jumpCounter <= 0) jumpCounter = -1;

			bid.pickup = false;
			bid.holdingAction = false;
			bid.throwRelease = false;
			bid.dropDown = false;
			bid.dialog = false;
			bid.useItemActionPressed = false;
			bid.useItemActionHeld = false;
			bid.useItemActionReleased = false;
		}
		else //We are recording incap data
		{
			recordTime+=Time.deltaTime;
			
			for (int i=0; i<pi.bid.incapCount; i++)
			{
				if (pi.bid.incapGetValue(i) != previousBid.incapGetValue(i))
				{
					recordFrames.Add(new incapKeyframe(recordTime, i, pi.bid.incapGetValue(i) ) );
					previousBid.incapSetValue(i, pi.bid.incapGetValue(i));
				}
			}
		}
    }

    public void OnLanding()
    {

    }
    public void OnCrouch()
    {

    }
	
	public void OnValidate()
	{
		if (record && !recordInProgress)
		{
			recordInProgress=true;
			recordTime=0f;
			myPreviousTag=gameObject.tag;
			player=GameObject.FindWithTag("Player");
			gameObject.tag = "Player";
			player.tag = "inactivePlayer";
			player.SendMessage("onControlTaken", SendMessageOptions.DontRequireReceiver);
			Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
			pi = gameObject.AddComponent<PlayerInput>();
			pi.controller = GetComponent<CharacterController2D>();
			pi.aimAngleSpeed = aimAngleSpeed;
			pi.aimForceSpeed = aimForceSpeed;
			pi.runSpeed = runSpeed;
			pi.aimActionAngleSpeed = aimActionAngleSpeed;
			pi.aimActionForceSpeed = aimActionForceSpeed;
			pi.climbSpeed = climbSpeed;
			
			homeMarker = new GameObject(gameObject.name+"_homeMarker");
			homeMarker.transform.position=gameObject.transform.position;
			SpriteRenderer r = homeMarker.AddComponent<SpriteRenderer>();
			SpriteRenderer myr = gameObject.GetComponent<SpriteRenderer>();
			r.sprite = myr.sprite;
			r.sortingLayerID = myr.sortingLayerID;
			r.sortingOrder = myr.sortingOrder-1;
			r.color=myr.color;
			
			previousBid = new BlubberInputData();
			
			recordFrames = new List<incapKeyframe>();
		}
		
		if (!record && recordInProgress)
		{
			recordInProgress=false;
			gameObject.tag = myPreviousTag;
			player.tag = "Player";
			player.SendMessage("onControlResumed", SendMessageOptions.DontRequireReceiver);
			Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
			Destroy(GetComponent<PlayerInput>());
			Destroy(homeMarker);
			
			//Write the file.
			int append = 0;
			string appendString="";
			
			string dir = global.dirResourceRoot+global.dirInputCapture;
			
			while (File.Exists(dir+saveFileName+appendString+".bytes"))
			{
				append+=1;
				appendString = append.ToString();
				if (append>100) break;
			}
			
			FileStream fs = new FileStream(dir+saveFileName+appendString+".bytes", FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(fs, recordFrames);
			fs.Close();
						
			recordFrames.Clear();
		}
	}
	
	//Plays an incapAnimations entry specified by name. Returns false if the animation cannot be played.
	public bool playIncap(string name)
	{
		if (recordInProgress) return false;
		
		foreach(var a in incapAnimations)
		{
			if (a.name==name)//Found it!
			{
				playingIncap = true;
				currentIncap = a;
				incapPlayTime=0;
				keyframeIndex=0;
			}
		}
		
		return true;
	}
}
