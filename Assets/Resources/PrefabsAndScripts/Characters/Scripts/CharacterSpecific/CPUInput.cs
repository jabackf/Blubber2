using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Events;
using UnityEditor;

/*This is used for activating basic Blubber actions, like jump, walk, push, ect.
 * This is used mainly for AI or animators to control a blubber character.
 * It's basically the PlayerInput script, but stripped of the input checking.
 * The variables (like jump, climb, ect.) can be triggered to activate the corresponding actions
 */

public class CPUInput : MonoBehaviour
{
	[System.Serializable]
	public enum states
	{
		idle,
		playingIncap,
		recordInProgress,
		trackingIncap, //We're playing an incap, but we're pausing for a second to get back on track between keyframes.
		goingHome,
		incapGoingHome //We're at the start or end of an animation and have to go home before continuing.
	}
	public states state=states.idle;
	public CharacterController2D controller;
	public Transform homeTransform; //This is the transform used for the homeMarker. If null, we will just use the character's start position.
	[HideInInspector] public CharacterController2D_Input bid;
	
	//An incap animation is just a list of incapKeyframes. A keyframe is created for every instance when a CharacterController2D_Input field changes.
	[System.Serializable]
	public class incapKeyframe
	{
		public float time;	//The amount of time that has elapses between the start of the input cature and this particular keyframe
		public int id; //The incap id that corresponds to the CharacterController2D_Input field that has changed.
		public float val; //The new value that the field will change to. For bools, this is 1 for true 0 for false.
		public float x; //The character's world location as recorded on this keyframe. Motion generally comes from input values, but we can use this to determine if we're off track.
		public float y;
		
		public incapKeyframe(float time, int id, float val, float x, float y)
		{
			this.time=time;
			this.id=id;
			this.val = val;
			this.x=x;
			this.y=y;
		}
	}
	
	//These are behaviors we can do when we arrive home during a "goingHome" state. This excludes incapGoingHome
	[System.Serializable]
	public enum arriveHomeBehaviors
	{
		none,
		idle,
		keepGoingHome, //This behavior will cause the character to continue tracking the homeMarker. Character will stand still, but if the homeMarker moves it will try to move with it.
		restartIncap, //If currentIncap is set to something we restart it. Otherwise we go into idle.
		playFirstIncap //Plays whatever incap animation is first in the list of incap animations.
	}
		
	
	//These are actions we can take if still stuck gets triggered.
	[System.Serializable]
	public enum stillStuckBehaviors
	{
		none,
		idle, //Just go into idle state. Can optionally say a message by setting "stillStuckIdleMessage"
		jumpHome, //Jump to the home position.
		die, //Triggers the character's death. Character will then obey the rules of respawnBehavior
		goHome, //Switch to goHome mode and attempt to walk there in the hopes of not getting stuck again and ending up in an endless cycle.
		goHomeJumpHome //Attempt one time to go home. If StillStuck gets triggered a second time, we'll do jump home.
	}
	
	//These are actions we can take if we get stuck on a ladder
	[System.Serializable]
	public enum climbStuckBehaviors
	{
		none,
		idle, //Just go into idle state. Can optionally say a message by setting "stillStuckIdleMessage"
		jumpHome, //Jump to the home position.
		die, //Triggers the character's death. Character will then obey the rules of respawnBehavior
		goHome //Switch to goHome mode and attempt to walk there in the hopes of not getting stuck again and ending up in an endless cycle.
	}
	
	[System.Serializable]
	public enum respawnBehaviors
	{
		idle, //Just go into idle state.
		goHome, //Calls goHome()
		restartIncap, //If currentIncap is set to something we restart it. Otherwise we go into idle.
		playFirstIncap //Plays whatever incap animation is first in the list of incap animations.
	}
		
	[System.Serializable]
	public enum incapEndBehavior
	{
		//What should we do at the start or end of the incap animation?
		goHome, //Move towards home.
		jumpHome, //Instantaneously jump home
		none //Just stay where you're at
	}
	
	[System.Serializable]
	public enum incapTrackingBehavior
	{
		//Each keyframe stores a location that we *should* be at. Slight variations, as well as shit getting in our way, could screw everything up!
		//What should we do to stay on track?
		snap, //On every keyframe we automatically jump the character to location he is supposed to be in. It's accurate, but it can be choppy.
		nothing, //Don't do crap. Just go on to the next keyframe from our current location.
		move //Pause the animation and attempt to move to the next location by going left or right.
	}
	
	[System.Serializable]
	public enum keyframeActionTriggers //How will we trigger a particular keyframe action?
	{
		keyframe, //When the keyframe index equals this value
		endOfAnimation, //This is different from lastKeyframe. lastKeyframe will be triggered as soon as we finish the action of secondToLastKeyframe. This is because our current keyframe is always the one that is next in the queue (We could be waiting for the time to play the frame's action, tracking first, etc). endOfAnimation is triggered after lastKeyframe is complete.
		lastKeyframe, //When we hit the final keyframe of the animation
		secondToLastKeyframe, //Last keyframe minus 1. This is particularly useful if you use addTimeToEnd. The added padding keyframe becomes lastKeyframe, and secondToLastKeyframe becomes the actual final keyframe in the incap file.
		percent, //When this percent of the animation has completed (between 0 and 100, with 100 or over being the last frame)
		time //When a timer hits this value. This time excludes time that was accrued while state was equal to something other that playingIncap (for example, trackingIncap)
	}
	
	[System.Serializable]
	public enum facingDirectionOptions
	{
		noChange,
		faceLeft,
		faceRight,
		facePlayer
	}

	[System.Serializable]
	public enum freezingOptions
	{
		none,
		freezePlayer,
		unfreezePlayer
	}
	[System.Serializable]
	public enum focusOptions
	{
		none,
		takeFocus,
		returnFocus
	}
	
	[System.Serializable]
	public class incapKeyframeAction //An incap keyframe action is a particular action that we can specify for a certain point in the animation.
	{
		public keyframeActionTriggers keyframeActionTrigger = keyframeActionTriggers.keyframe;
		public float actionTriggerValue = 0f;
		public GameObject sendTo; //All messages and say commands will apply to this GameObject. Leave blank if you want it to apply to this character.
		[HideInInspector] public bool done=false;
		
		public freezingOptions freezePlayer = freezingOptions.none;
		public focusOptions takeFocus = focusOptions.none;
		
		//Actions
		public float randomizePitchMin=1f, randomizePitchMax=1f, sayTime = 3f;
		public float frameTimeModifier = 0f; //This will be added to the current frame's time value. Meaning you can use this to adjust the time to wait before starting this keyframe.
		public List<UnityEvent> callbacks = new List<UnityEvent>();
		public List<string> sendMessages = new List<string>(); //Send all of the messages
		public List<string> say = new List<string>(); //Play a random message from this list. If sendTo has a characterController, then sendTo will say it. If not, this character will say it.
		public List<AudioClip> audioClips = new List<AudioClip>(); //Randomly play one of these sounds
		[Range(-1, 9)] public int inventoryToggle=-1; //-1 for none. 0-9 to pull things out and put things in inventory slots.
		
		
		//GameObject is THIS character gameObject. It has nothing to do with the sendTo object.
		public void activate(GameObject gameObject, Global global)
		{
			if (this.done) return;
			this.done=true;
			
			if (!sendTo) sendTo=gameObject;
			
			foreach (var c in callbacks) c.Invoke();
			foreach (var m in sendMessages) sendTo.SendMessage(m, SendMessageOptions.DontRequireReceiver);
			
			CharacterController2D cont = sendTo.GetComponent<CharacterController2D>();
			if (!cont) cont = gameObject.GetComponent<CharacterController2D>();
			cont.Say(say.ToArray(),sayTime);
			
			if (audioClips.Count > 0) global.audio.RandomSoundEffect(audioClips.ToArray(), randomizePitchMin, randomizePitchMax);
			
			if (inventoryToggle!=-1) cont.inventoryToggleEquip(inventoryToggle);
		}
		
		public float getFrameTimeModifier()
		{
			return frameTimeModifier;
		}
	}
	
	[System.Serializable]
	public class incapAnimation
	{
		public string name=""; //The name that will be used to reference this animation
		[HideInInspector] public List<incapKeyframe> keyframes;
		[HideInInspector] public bool loaded=false; //Set to true after the keyframes have been loaded.
		public TextAsset incapFile;
		public bool loop=true;
		public bool freezePlayer=false; //Freezes the player for the duration of the animation, or until a keyframe action unfreezes him.
		public bool takeFocus=false;	//Takes camera focus away from the player and puts it on this character for the duration of the animation, or until a keyframe action returns focus.
		[HideInInspector] public bool complete=false; //This will be set to false each time we begin a new run through of the animation (before going home) and false when we complete it (before going home.)
		public incapTrackingBehavior trackingBehavior = incapTrackingBehavior.snap;
		public incapEndBehavior behaviorAtStart = incapEndBehavior.goHome; 
		public incapEndBehavior behaviorAtEnd = incapEndBehavior.goHome;
		public bool facePlayerTowardsCharacter = false; //If set to true, the player's facing direction will be set to face this character at the start of the animation.
		public facingDirectionOptions facingAtEnd = facingDirectionOptions.noChange; //This is what direction the character will turn to at the end of the animation.
		public bool startFrameZeroImmediately=false; //If set to true we won't wait for keyframes[0].time. We will just immediately start with keyframes[0] at the start of the animation. We WILL still wait for startTimeDelay though.
		public float startTimeDelay=0f;	//This is a delay that can be added at the start after going home, before hitting play on the animation.
		public float addTimeToEnd = 0f; //If this is greater than zero, then an additional keyframe will be added this amount of time after the last. The keyframe will contain no actions and only acts as padding for the end of the animation. If you change this value at runtime then it will have no effect unless you call loadKeyframes again to reload the keyframes.
		
		private bool focusTaken=false;
		private bool playerFrozen=false;
		
		[HideInInspector] public float currentFrameTimeModifier=0; //This value is added to keyframes[current].time. This value is reset to zero after each frame. 
		
		public List<incapKeyframeAction> incapKeyframeActions = new List<incapKeyframeAction>();
		
		public void loadKeyframes()
		{
			BinaryFormatter bf = new BinaryFormatter();
			Stream stream = new MemoryStream(this.incapFile.bytes);
			this.keyframes.Clear();
			this.keyframes = (List<incapKeyframe>)bf.Deserialize(stream);
			stream.Close();
			
			if (this.addTimeToEnd>0)
			{
				int lastFrameIndex = this.keyframes.Count-1;
				this.keyframes.Add(new incapKeyframe(keyframes[lastFrameIndex].time+addTimeToEnd, -1, 0, keyframes[lastFrameIndex].x, keyframes[lastFrameIndex].y ) );
			}
			
			this.loaded=true;
		}
		
		public void printKeyFrameData(CharacterController2D_Input cont)
		{
			int i=0;
			foreach (var k in keyframes)
			{
				Debug.Log("KeyFrame: "+i+", Input: "+cont.incapIDToName(k.id)+", Value: "+k.val+", Time: "+k.time+", Pos: "+k.x+","+k.y);
				i++;
			}
		}
		
		private void freezePlayerMethod(bool freeze, Global global, GameObject gameObject)
		{
			global.pausePlayer(freeze);
			this.playerFrozen=freeze;
		}
		private void takeFocusMethod(bool take, Global global, GameObject gameObject)
		{
			if (take)global.camera_follow_player.takeFocus(gameObject.transform);
			else global.camera_follow_player.returnFocus();
			this.focusTaken=take;
		}
		private void freezePlayerMethod(freezingOptions freeze, Global global, GameObject gameObject)
		{
			if (freeze==freezingOptions.freezePlayer) freezePlayerMethod(true, global, gameObject);
			if (freeze==freezingOptions.unfreezePlayer) freezePlayerMethod(false, global, gameObject);
		}
		private void takeFocusMethod(focusOptions focus, Global global, GameObject gameObject)
		{
			if (focus==focusOptions.takeFocus) takeFocusMethod(true, global, gameObject);
			if (focus==focusOptions.returnFocus) takeFocusMethod(false, global, gameObject);
		}
		
		//This is called from playIncap at the start of the animation (after going home, before startTimeDelay). Requires this character's gameObject and a global reference.
		public void startOfAnimation(GameObject gameObject, Global global)
		{
			if (this.takeFocus)
			{
				takeFocusMethod(true, global, gameObject);
			}
			if (this.freezePlayer) 
			{
				freezePlayerMethod(true, global, gameObject);
			}
			if (facePlayerTowardsCharacter)
			{
				CharacterController2D pcc2d = global.getPlayerController();
				if (pcc2d!=null) pcc2d.FacePosition(gameObject.transform.position);
			}
		}
		
		//This is called when we hit the end of the animation, before going home. Requires this character gameObject and a global reference.
		public void endOfAnimation(GameObject gameObject, Global global)
		{
			foreach (var a in this.incapKeyframeActions)
			{
				if (a.keyframeActionTrigger==keyframeActionTriggers.endOfAnimation) a.activate(gameObject, global);
			}
			
			//Now we reset all of the action done states if we are starting the animation over.
			foreach(var a in this.incapKeyframeActions) a.done=false;
			
			this.currentFrameTimeModifier=0f;
			
			CharacterController2D cont = gameObject.GetComponent<CharacterController2D>();
			if (facingAtEnd  == facingDirectionOptions.faceLeft) cont.FaceLeft();
			if (facingAtEnd  == facingDirectionOptions.faceRight) cont.FaceRight();
			if (facingAtEnd  == facingDirectionOptions.facePlayer) cont.FaceTag("Player");
			
			if (playerFrozen)
			{
				freezePlayerMethod(false, global, gameObject);
			}
			
			if (focusTaken)
			{
				takeFocusMethod(false, global, gameObject);
			}
		}
		
		//Intended to be called for every frame that the animation is playing. currentKeyframe is the keyframe we are currently on. Time is the amount of time that the incap has been playing (exluding trackingIncap mode)
		//GameObject is a reference to this gameObject character. We also need a global reference.
		public void updateKeyframeActions(int currentKeyframe, float time, GameObject gameObject, Global global)
		{
			
			//Now we cycle through the actions, determining which ones we need to trigger.
			foreach (var a in this.incapKeyframeActions)
			{
				switch(a.keyframeActionTrigger) 
				{
				  case keyframeActionTriggers.keyframe:
					if (currentKeyframe>=a.actionTriggerValue && !a.done) 
					{
						a.activate(gameObject, global);
						currentFrameTimeModifier=a.frameTimeModifier;
						takeFocusMethod(a.takeFocus, global, gameObject);
						freezePlayerMethod(a.freezePlayer, global, gameObject);
					}
					break;
				  case keyframeActionTriggers.lastKeyframe:
					if (currentKeyframe>=keyframes.Count-1 && !a.done) 
					{
						a.actionTriggerValue=keyframes.Count-1;
						a.activate(gameObject, global);
						currentFrameTimeModifier=a.frameTimeModifier;
						takeFocusMethod(a.takeFocus, global, gameObject);
						freezePlayerMethod(a.freezePlayer, global, gameObject);
					}
					break;
				  case keyframeActionTriggers.secondToLastKeyframe:
					if (currentKeyframe>=keyframes.Count-2 && !a.done) 
					{
						a.actionTriggerValue=keyframes.Count-2;
						a.activate(gameObject, global);
						currentFrameTimeModifier=a.frameTimeModifier;
						takeFocusMethod(a.takeFocus, global, gameObject);
						freezePlayerMethod(a.freezePlayer, global, gameObject);
					}
					break;
				  case keyframeActionTriggers.percent:
					float atv = a.actionTriggerValue;
					if (atv>100) atv=100;
					if (atv<0) atv=0;
					if ( (currentKeyframe/(keyframes.Count-1)*100) >=atv && !a.done) 
					{
						a.activate(gameObject, global);
						currentFrameTimeModifier=a.frameTimeModifier;
						takeFocusMethod(a.takeFocus, global, gameObject);
						freezePlayerMethod(a.freezePlayer, global, gameObject);
					}
					break;
				  case keyframeActionTriggers.time:
					if (time>=a.actionTriggerValue && !a.done) 
					{
						a.activate(gameObject, global);
						currentFrameTimeModifier=a.frameTimeModifier;
						takeFocusMethod(a.takeFocus, global, gameObject);
						freezePlayerMethod(a.freezePlayer, global, gameObject);
					}
					break;
				}
				
			}
		}
		
		public float getCurrentFrameTimeModifier()
		{
			return currentFrameTimeModifier;
		}
	}
	
	[System.Serializable]
	public class triggerClass //A trigger is a name associated with some kind of action. They are called with the trigger function. For example, calling trigger("dance") might initiate an incap with a dancing animation.
	{
		public string name;
		public GameObject sendTo; //All messages and say commands will apply to this GameObject. Leave blank if you want it to apply to this character.
		[HideInInspector] public bool done=false;
		
		//Actions
		public string playIncapTitle=""; //If not empty, we will search for this incap and play it.
		public float randomizePitchMin=1f, randomizePitchMax=1f, sayTime = 3f;
		public List<UnityEvent> callbacks = new List<UnityEvent>();
		public List<string> sendMessages = new List<string>(); //Send all of the messages
		public List<string> say = new List<string>(); //Play a random message from this list. If sendTo has a characterController, then sendTo will say it. If not, this character will say it.
		public List<AudioClip> audioClips = new List<AudioClip>(); //Randomly play one of these sounds
		[Range(-1, 9)] public int inventoryToggle=-1; //-1 for none. 0-9 to pull things out and put things in inventory slots.
		
		//GameObject is THIS character gameObject. It has nothing to do with the sendTo object.
		public void activate(GameObject gameObject, Global global)
		{
			if (this.done) return;
			this.done=true;
			
			if (!sendTo) sendTo=gameObject;
			
			foreach (var c in callbacks) c.Invoke();
			foreach (var m in sendMessages) sendTo.SendMessage(m, SendMessageOptions.DontRequireReceiver);
			
			CharacterController2D cont = sendTo.GetComponent<CharacterController2D>();
			if (!cont) cont = gameObject.GetComponent<CharacterController2D>();
			cont.Say(say.ToArray(),sayTime);
			
			if (audioClips.Count > 0) global.audio.RandomSoundEffect(audioClips.ToArray(), randomizePitchMin, randomizePitchMax);
			
			if (inventoryToggle!=-1) cont.inventoryToggleEquip(inventoryToggle);
			
			if (playIncapTitle!="") gameObject.GetComponent<CPUInput>().playIncap(playIncapTitle);
		}
	}
	
	[Space]
    [Header("Input Capture")]
	//The following are used for input recording.
	public string saveFileName="InputCapture"; //The name of the file that will be recorded. It will be saved to global.dirInputCapture in resources. A number will be appended if the file exists. These are saved with bytes extension so they can be supplied in the inspector as a TextAsset.
	public bool record=false;
	public bool cancelRecording=false; //Click this if you are in the middle of recording and you want to cancel it.
	bool recordInProgress=false;
	GameObject player;
	string myPreviousTag;
	float recordTime=0f;
	private GameObject homeMarker;
	private PlayerInput pi;
	private CharacterController2D_Input previousBid; //Used for recording to detect when an input value has changed.
	private List<incapKeyframe> recordFrames; //This list stores the current keyframes that we are recording when recordInProgress is true.
	
	
	[Space]
    [Header("Input Playback")]
	private incapAnimation currentIncap; //The incap that we are currently playing if playingIncap is true
	float incapPlayTime=0; //The amount of time that has elapsed since the start of the playback.
	int keyframeIndex=0; //This stores the index of the keyframe list that we are on for the currently playing incap. It's like the playhead.
	public List<incapAnimation> incapAnimations; //These are all of the incap animations for this character
	private bool startTimeDelayWait=false;	//Incaps have an optional startTimeDelay. This is set to true after we call playIncap and before the delay has finished.
	
	[Space]
    [Header("Triggers")]
	public List<triggerClass> triggers = new List<triggerClass>();
	
	[Space]
    [Header("Movement")]

    public float aimAngleSpeed = 200f; //Speed for aiming the angle of the throwing retical
    public float aimForceSpeed = 50f; //Speed for aiming the angle of the throwing retical
	public float runSpeed = 35f;
    public float aimActionAngleSpeed = 200f; //Speed for aiming the angle of the action retical
    public float aimActionForceSpeed = 50f; //Speed for aiming the angle of the action retical

    public float climbSpeed = 5f;

    //private float jumpCounter = 0; //Used to make sure we're not jumping repeatedly
	
	public float snapResolution = 0.4f; //Used for snapping the character in place on incap keyframes. If we are set to incapTrackingBehavior.move, then the character will try to move within this distance of the keyframe position
	
	private Vector2 target; //Used for various states, such as states.trackingIncap or states.goingHome. This is a target that we are trying to move to.
	
	[Space]
    [Header("Stuck")]
	//Stuck!
	private bool stuck=false; //We detect when we have been trying to move horizontally for a bit, but we're not going anywhere.
	public float stuckTime = 0.5f; //This is how long we try to move before setting stuck to true. Set to -1 to not use the stuck timer.
	private float stuckTimer=0f;
	private float previousHMove=0;
	private float previousX = 0;
	
	//We can do stuff if we have been stuck for a bit and we can't get unstuck.
	public float stillStuckTime=3f; //This timer starts counting down after stuck is triggered. If it hits zero and stuck is still true, the stillStuck is set to true. Set to -1 to not use the stillStuckTimer.
	private float stillStuckTimer=0f;
	public stillStuckBehaviors stillStuckBehavior = stillStuckBehaviors.idle;
	public string stillStuckMessage="Well, it would seem as though I can't get over there. Guess I'll just sit here."; //We can optionally say this message. Leave blank for no message.
	private int goHomeJumpHomeCounter=0; //Used for the goHomeJumpHome stillStuckBehavior
	
	[Space]
    [Header("on Respawn")]
	//Respawn
	public respawnBehaviors respawnBehavior = respawnBehaviors.idle;
	public string respawnMessage = "Ouch."; //This optional message is said when we respawn.
	
	[Space]
    [Header("Climbing")]
	//Ladders and such
	public float ladderCheckDistance=14f; //If we are trying to go somewhere that is significantly higher or lower than where we are at, then we will do a raycast to look for a ladder. This is how far to the left or right we will look for a ladder. A ray is drawn in the inspector reflecting this value.
	public ContactFilter2D climbContactFilter;
	private GameObject targetLadder; //If this is not null and we are set to goingHome, trackingIncap, or incapGoingHome, or any other mode that attempts to navigate to a target, then we will attempt to climb the ladder to reach our destination if necessary.
	private Collider2D targetLadderCollider;
	private float ladderEdgeX=0; //This holds the x position where the raycast hit RELATIVE to the ladder's x position. This is used to position us on the ladder when we arrive.
	private bool climbStuck=false;
	public float climbStuckTime = 4f; //A timer to determine if we get stuck climbing the ladder. -1 for none.
	private float climbStuckTimer=0f;
	public climbStuckBehaviors climbStuckBehavior = climbStuckBehaviors.die;
	private float climbStuckPreviousY=0;
	private float climbStuckPreviousClimb=0;
	private float ladderCharacterOffset=0; //Used to help us position the character on the ladder.
	
	
	[Space]
    [Header("Misc Behaviors")]
	//Arrive Home
	public arriveHomeBehaviors arriveHomeBehavior = arriveHomeBehaviors.idle;
	
	[Space]
    [Header("Debug")]
	public bool showHomeBool=false;
	public bool printCurrentIncapFrames=false;
	public bool printInputState=false;
	public bool printKeyframeNumber=false;
	public bool playTestIncap=false; //Plays the specified incap
	public string testIncapName="";
	public bool debugGoHome = false; //When set to true, the character stops what it's doing and goes home.
	public bool debugGoIdle = false; //If you read the previous variable's comment, then I'm sure I don't need to explain to you what this one does.
	public List<string> debugInputs = new List<string>(); //Enter the name of an input (for example, jump or horizontalMove) and the state will be drawn above the character.
	public Color debugInputsColor = Color.red;
	public bool drawTargetGizmo = false;

	[Space]
    [Header("Inspect Incap Playback")]
	public bool keyframeWait = false; //If set to true, we will pause at every incap keyframe and print the current keyframe number and information to the console.
	public bool keyframeNext = false; //If keygframeWait is true, this we will have to activate this variable every time we want to advance to the next keyframe.
	private bool keyframeWaitWasOn=false; //Used to detect when keyframe wait goes from true to false.
	
	Global global;

    // Start is called before the first frame update
    void Start()
    {
		global = GameObject.FindWithTag("global").GetComponent<Global>();
		bid = new CharacterController2D_Input();
		bid.mouseAim=false;
		bid.Init(controller);
		
		if (homeTransform==null) homeTransform=gameObject.transform;
		homeMarker = new GameObject();
		homeMarker.name=gameObject.name+"_homeMarker";
		homeMarker.transform.position=homeTransform.position;
		SpriteRenderer r = homeMarker.AddComponent<SpriteRenderer>();
		SpriteRenderer myr = gameObject.GetComponent<SpriteRenderer>();
		r.sprite = myr.sprite;
		r.sortingLayerID = myr.sortingLayerID;
		r.sortingOrder = myr.sortingOrder-1;
		r.color=myr.color;
		homeMarker.SetActive(showHomeBool);
		
		foreach (var a in incapAnimations) a.loadKeyframes();
		
    }

    void Update()
    {
		if (state==states.goingHome || state==states.incapGoingHome)
		{
			target=homeMarker.transform.position;
		}
		
		
		if (state==states.playingIncap) 
		{
			if (!keyframeWait) incapPlayTime+=Time.deltaTime;
			if (!startTimeDelayWait)currentIncap.updateKeyframeActions(keyframeIndex, incapPlayTime, gameObject, global);
		}

		
		//If we're recording, then a playerInput script should be managing the bid updating.
		if (state!=states.recordInProgress)
			bid.UpdateInputLogic();

    }

    void FixedUpdate()
    {
		if (state==states.trackingIncap || state==states.goingHome || state==states.incapGoingHome)
		{
			bool xIsGood=false;
			bool yIsGood=true; //We're going to worry less about the y position than the x position. We might check for a ladder and attempt to make it to y, but if we get to X then good enough.
			
			if (Mathf.Abs(transform.position.x-target.x) <= snapResolution) xIsGood=true;
			
			if (Mathf.Abs(transform.position.y-target.y) > snapResolution && targetLadder==null && bid.controller.getCanClimb())
			{
				if (Mathf.Abs(target.y-transform.position.y)>3f) //If the target is higher or lower than this number of units, then we'll check for a ladder.
				{
					Debug.DrawRay(new Vector2(transform.position.x-ladderCheckDistance,bid.controller.getBottomPosition().y), Vector2.right*ladderCheckDistance*2, Color.green, 2f);
					List<RaycastHit2D> hits = new List<RaycastHit2D>();
					Physics2D.Raycast(new Vector2(transform.position.x-ladderCheckDistance,bid.controller.getBottomPosition().y), Vector2.right, climbContactFilter, hits, ladderCheckDistance*2);
					foreach(var h in hits)
					{
						//The raycast hit a ladder. Let's check to see if it takes us closer to the target's y value.
						Vector3 cp = h.collider.ClosestPoint(target);
						//if (Mathf.Abs(cp.y-target.y)<=1f)
						if ( Mathf.Abs(Mathf.Abs(cp.y-target.y) - Mathf.Abs(target.y-transform.position.y))>2f && Mathf.Abs(cp.y-target.y) < Mathf.Abs(target.y-transform.position.y) )
						{
							//Looks close enough. Let's try using it!
							targetLadder = h.collider.gameObject;
							targetLadderCollider = h.collider;
							ladderEdgeX =  h.collider.ClosestPoint(transform.position).x - targetLadder.transform.position.x;
							
							SpriteRenderer r = GetComponent<SpriteRenderer>();
							ladderCharacterOffset = (r.bounds.extents.x) * (targetLadder.transform.position.x<transform.position.x ? -1f : 1f);
							
							//There is an edge case where everything goes all stupid if we're already standing in front of the ladder, so let's fix that.
							List<RaycastHit2D> ch = new List<RaycastHit2D>();
							GetComponent<Collider2D>().Cast(Vector2.zero, climbContactFilter, ch, 0f,true);
							foreach (var o in ch)
							{
								if (o.collider.gameObject==targetLadder)
								{
									ladderEdgeX=0f; //If we are already colliding with the thing, let's just do the easy thing and climb right up the center of the ladder. Hopefully this works well really fat climb layers (like vines should I ever add those in)
								}
							}
							
							yIsGood=false;
						}
					}
				}
			}
			
			if (xIsGood && yIsGood && targetLadder==null)
			{

				bid.reset();
				transform.position = new Vector3(target.x,transform.position.y ,0f);
				if (state==states.trackingIncap) 
				{
					//We've snapped into place and are in the middle of an animation, so let's switch back to playing and move to the next keyframe.
					state=states.playingIncap;
					advanceIncapKeyframe();
				}
				if (state==states.incapGoingHome) 
					advanceIncapKeyframe();
				if (state==states.goingHome) arrivedHome();
			}
			else
			{
				if (targetLadder!=null)
				{
					bid.dropDown=true; //This stops us from getting stuck on any platforms at the top of the ladder.
					Vector3 targetLadderPos;
					if (ladderEdgeX!=0)
					{
						targetLadderPos = new Vector3(targetLadder.transform.position.x+ladderEdgeX+ladderCharacterOffset,targetLadder.transform.position.y,targetLadder.transform.position.z);
					}
					else
						targetLadderPos = targetLadder.transform.position;
					Debug.DrawRay(targetLadderPos,Vector3.up,Color.red);
					if (Mathf.Abs(targetLadderPos.x-transform.position.x)>snapResolution)
					{
						walkTowards(targetLadderPos);
					}
					else
					{
						walkTowards(targetLadderPos);
						bid.climb = climbSpeed * (target.y<transform.position.y ? -1f : 1f);
						//Check if the ladder took us to the target area OR if we are reaching the end of the ladder.
						float ladderEndY = (target.y<transform.position.y ? targetLadderCollider.bounds.min.y : targetLadderCollider.bounds.max.y);
						if ((transform.position.y>target.y+0.005f && transform.position.y<target.y+0.3f) || Mathf.Abs(ladderEndY-transform.position.y)<0.1f)
						{
							//We seem to be done climbing. Let's try going to the target now!
							bid.reset();
							targetLadder=null;
						}
					}
				}
				else
				{
					if (!xIsGood)
						walkTowards(target);
				}
			}
		}
		else
		{
			if (targetLadder!=null)
			{
				bid.reset();
				targetLadder=null;
				bid.controller.stopClimbing();
			}
		}
		
		if (state==states.recordInProgress)
		{
			recordTime+=Time.deltaTime;
			
			for (int i=0; i<pi.bid.incapCount; i++)
			{
				if (pi.bid.incapGetValue(i) != previousBid.incapGetValue(i))
				{
					recordFrames.Add(new incapKeyframe(recordTime, i, pi.bid.incapGetValue(i), transform.position.x, transform.position.y ) );
					previousBid.incapSetValue(i, pi.bid.incapGetValue(i));
				}
			}
		}
		else
		{
			bid.UpdateCharacterController(); //If we're recording then the playerInput script should be managing the bid updates.
		}
		
		//Detect when stuck
		if (previousHMove==bid.horizontalMove && Mathf.Abs(transform.position.x-previousX)<0.1f && bid.horizontalMove!=0 && stuckTime!=-1 && (!keyframeWait||state!=states.playingIncap) )
		{
			stuckTimer-=Time.fixedDeltaTime;
			if (stuckTimer<=0 && stuck!=true)
			{
				stuck=true;
				stillStuckTimer=stillStuckTime;
			}
		}
		else
		{
			stuck=false;
			stuckTimer=stuckTime;
		}
		if (stuck==true && stillStuckTime!=-1)
		{
			stillStuckTimer-=Time.deltaTime;
			if (stillStuckTimer<=0)
			{
				goIdle();
				Invoke("holyCrapWeAreStuckSomewhereAndCantGetOut",2f);
			}
		}
		previousX = transform.position.x;
		previousHMove=bid.horizontalMove;
		
		//Detect if we get stuck on a ladder
		if (climbStuckPreviousClimb==bid.climb && Mathf.Abs(transform.position.y-climbStuckPreviousY)<0.1f && bid.climb!=0 && targetLadder!=null && (!keyframeWait||state!=states.playingIncap) )
		{
			climbStuckTimer-=Time.fixedDeltaTime;
			if (climbStuckTimer<=0 && climbStuck!=true)
			{
				climbStuck=true;
				targetLadder=null;
				bid.controller.stopClimbing();
				if (climbStuckBehavior==climbStuckBehaviors.idle) goIdle();
				if (climbStuckBehavior==climbStuckBehaviors.goHome) goHome();
				if (climbStuckBehavior==climbStuckBehaviors.die) 
				{
					bid.controller.die();
					bid.reset();
				}
				if (climbStuckBehavior==climbStuckBehaviors.jumpHome) jumpCharacter(homeMarker.transform.position);
			}
		}
		else
		{
			climbStuck=false;
			climbStuckTimer=climbStuckTime;
		}	
		climbStuckPreviousY = transform.position.y;
		climbStuckPreviousClimb=bid.climb;		
    }
	
	//Plays the first located incapAnimations entry specified by name. Returns false if the animation cannot be played.
	public bool playIncap(string name)
	{
		if (state==states.recordInProgress) return false;
		
		foreach(var a in incapAnimations)
		{
			if (a.name==name)//Found it!
			{
				state=states.playingIncap;
				currentIncap = a;
				incapPlayTime=0;
				keyframeIndex=0;
				currentIncap.complete=false;
				bid.reset();
				
				if (currentIncap.behaviorAtStart==incapEndBehavior.goHome && !areWeHome(true)) state=states.incapGoingHome;
				else 
				{
					startTimeDelayWait=true;
					if (currentIncap.behaviorAtStart==incapEndBehavior.jumpHome) jumpCharacter(homeMarker.transform.position);
					currentIncap.startOfAnimation(gameObject, global);
					Invoke("advanceIncapKeyframe",(currentIncap.startFrameZeroImmediately ? currentIncap.startTimeDelay+ currentIncap.getCurrentFrameTimeModifier() : currentIncap.keyframes[keyframeIndex].time+currentIncap.startTimeDelay + currentIncap.getCurrentFrameTimeModifier()));
				}
				return true;
			}
		}
		
		return false;
	}
	
	//This is called every time the next keyframe is needed via invoke. It updates the CC2D_Input object and stuff
	public void advanceIncapKeyframe()
	{
		if (state!=states.playingIncap)
		{
			if (state!=states.incapGoingHome) return;
			else
			{
				//We've either just finished going home at the start or end of this animation. Let's make sure we're home.
				if (areWeHome(true))
				{
					if (currentIncap.complete==false) //We're just starting the animation.
						playIncap(currentIncap.name);
					else //We've finished an animation then gone home.
					{
						if (currentIncap.loop) 
							playIncap(currentIncap.name);
						else 
							goIdle();
					}
				}
				else
					return; //We're not done going home yet.
			}
		}

		startTimeDelayWait=false;
		float time = currentIncap.keyframes[keyframeIndex].time;

		if (currentIncap.trackingBehavior == incapTrackingBehavior.move && Mathf.Abs(transform.position.x-currentIncap.keyframes[keyframeIndex].x)>snapResolution) 
		{
			state=states.trackingIncap;
			target.x=currentIncap.keyframes[keyframeIndex].x;
			target.y=currentIncap.keyframes[keyframeIndex].y;
		}
		else
		{
			//If we're either set to move and close enough, or set to snap, then snap the character
			if (currentIncap.trackingBehavior == incapTrackingBehavior.move || currentIncap.trackingBehavior == incapTrackingBehavior.snap) 
				transform.position = new Vector3(currentIncap.keyframes[keyframeIndex].x,transform.position.y,0);
			
			if (keyframeWait && !keyframeNext) 
			{	
				if (!bid.isPaused())
				{					
					Debug.Log("Paused ["+currentIncap.name+"]. Awaiting keyframeNext to continue.");
					Debug.Log("KEYFRAME: "+keyframeIndex +" OF "+(currentIncap.keyframes.Count-1));
					Debug.Log("Input: "+bid.incapIDToName(currentIncap.keyframes[keyframeIndex].id)+", Value: "+currentIncap.keyframes[keyframeIndex].val+", Pos: "+currentIncap.keyframes[keyframeIndex].x+", "+currentIncap.keyframes[keyframeIndex].y+" PlayTime: "+incapPlayTime);
					bid.pause(true);
				}
				return;
			}
			else 
			{
				if (keyframeWait)
				{
					bid.pause(false);
					keyframeNext = false;
				}
			}
			
			bid.incapSetValue(currentIncap.keyframes[keyframeIndex].id,currentIncap.keyframes[keyframeIndex].val);
			if (printKeyframeNumber)Debug.Log("KEYFRAME: "+keyframeIndex +" OF "+(currentIncap.keyframes.Count-1));
			keyframeIndex+=1;
			
			if (keyframeIndex>=currentIncap.keyframes.Count)
			{
				//Animation finished.
				currentIncap.endOfAnimation(gameObject, global);
				keyframeIndex=0;
				currentIncap.complete=true;
				incapPlayTime=0f;
				if (currentIncap.behaviorAtEnd == incapEndBehavior.goHome) goHome();
				else
				{
					if (currentIncap.behaviorAtEnd == incapEndBehavior.jumpHome) jumpCharacter(homeMarker.transform.position);
					if (currentIncap.loop) 
						playIncap(currentIncap.name);
					else 
						goIdle();
				}
				
				return;
			}
		}

		
		//Debug.Log("TIME "+currentIncap.keyframes[keyframeIndex].time+" - "+time+" = "+(currentIncap.keyframes[keyframeIndex].time-time));
		
		//We're going to give this an additional call here, because if we don't then getCurrentFrameTimeModifier() won't be updated.
		if (!startTimeDelayWait)currentIncap.updateKeyframeActions(keyframeIndex, incapPlayTime, gameObject, global);
		Invoke("advanceIncapKeyframe",currentIncap.keyframes[keyframeIndex].time-time + currentIncap.getCurrentFrameTimeModifier());
		currentIncap.currentFrameTimeModifier=0;
	}

    public void OnLanding()
    {

    }
    public void OnCrouch()
    {

    }
	
	public void OnValidate()
	{
		if (keyframeWait)
		{
			keyframeWaitWasOn=true;
			if (keyframeNext && state==states.playingIncap) 
				advanceIncapKeyframe();
			keyframeNext=false;
		}
		else
		{
			if (keyframeWaitWasOn)
			{
				bid.pause(false);
				if (state==states.playingIncap) 
					advanceIncapKeyframe();
				keyframeWaitWasOn=false;
			}
		}
		showHome(showHomeBool);
		if (printInputState)
		{
			bid.printState();
			printInputState=false;
		}
		if (printCurrentIncapFrames)
		{
			printCurrentIncapFrames=false;
			currentIncap.printKeyFrameData(bid);
		}
		if(playTestIncap)
		{
			playTestIncap=false;
			playIncap(testIncapName);
		}
		if(debugGoHome)
		{
			debugGoHome=false;
			goHome();
		}
		if(debugGoIdle)
		{
			debugGoIdle=false;
			goIdle();
		}
		if (record && state!=states.recordInProgress)
		{
			bid.reset();
			state=states.recordInProgress;
			recordTime=0f;
			myPreviousTag=gameObject.tag;
			player=GameObject.FindWithTag("Player");
			gameObject.tag = "Player";
			player.tag = "inactivePlayer";
			player.SendMessage("onControlTaken", SendMessageOptions.DontRequireReceiver);
			Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
			pi = gameObject.AddComponent<PlayerInput>();
			pi.aimAngleSpeed = aimAngleSpeed;
			pi.aimForceSpeed = aimForceSpeed;
			pi.runSpeed = runSpeed;
			pi.aimActionAngleSpeed = aimActionAngleSpeed;
			pi.aimActionForceSpeed = aimActionForceSpeed;
			pi.climbSpeed = climbSpeed;
			pi.mouseAim=false;
			pi.Init(bid.controller);
			
			showHome(true);
			
			previousBid = new CharacterController2D_Input();
			
			recordFrames = new List<incapKeyframe>();
		}
		
		if (!record && state==states.recordInProgress)
		{
			state=states.idle;
			gameObject.tag = myPreviousTag;
			player.tag = "Player";
			player.SendMessage("onControlResumed", SendMessageOptions.DontRequireReceiver);
			Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
			Destroy(GetComponent<PlayerInput>());
			showHome(false);
			
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
		
		if (state==states.recordInProgress && cancelRecording)
		{
			cancelRecording=false;
			record=false;
			jumpCharacter(homeMarker.transform.position);
			state=states.idle;
			gameObject.tag = myPreviousTag;
			player.tag = "Player";
			player.SendMessage("onControlResumed", SendMessageOptions.DontRequireReceiver);
			Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
			Destroy(GetComponent<PlayerInput>());
			showHome(false);	
			recordFrames.Clear();
		}
	}
	
	//Call this function to go into idle mode.
	public void goIdle()
	{
		bid.reset();
		state=states.idle;
	}
	
	//Shows a marker where the home location is. Home by default is the transform that our character started in. It's the place that the typically character tries to return to when idle.
	public void showHome(bool show)
	{
		if (homeMarker!=null)
			homeMarker.SetActive(show);
	}
	
	//This moves the location of home.
	public void moveHomeLocation(Vector3 position)
	{
		homeMarker.transform.position=position;
	}
	
	//This tells our character to try to go home by moving there.
	public void goHome()
	{
		bid.reset();
		target = homeMarker.transform.position;
		state=states.goingHome;
	}
	
	//Returns true if we are within snapping distance of home. The optional snap variable will go ahead and snap the character home if we are in snapping distance.
	public bool areWeHome(bool snap=false)
	{
		if (Vector2.Distance(transform.position,homeMarker.transform.position) <= snapResolution)
		{
			if (snap) transform.position=homeMarker.transform.position;
			if (state==states.goingHome) arrivedHome();
			return true;
		}
		return false;
	}
	
	//This should be called anytime we arrive home and state is equal to goingHome.
	public void arrivedHome()
	{
		if (state!=states.goingHome) return;
		if (arriveHomeBehavior==arriveHomeBehaviors.idle) goIdle();
		if (arriveHomeBehavior==arriveHomeBehaviors.playFirstIncap) playIncap(incapAnimations[0].name);
		if (arriveHomeBehavior==arriveHomeBehaviors.restartIncap) 
		{
			if (currentIncap!=null) playIncap(currentIncap.name);
			else goIdle();
		}
	}
	
	//Jumps the character to a new location spontaneously, creating pretty little sparkly warpy effecty thingies.
	public void jumpCharacter(Vector3 newpos)
	{
		bool makeParticles=true;
		if (Vector3.Distance(newpos,transform.position)<=snapResolution) makeParticles=false;
		if (makeParticles) bid.controller.makeSpawnParticles();
		transform.position=newpos;
		if (makeParticles) bid.controller.makeSpawnParticles();
	}
	
	//This function can be called when we are stuck somewhere for awhile. It does something based on the settings of stillStuckBehavior
	public void holyCrapWeAreStuckSomewhereAndCantGetOut()
	{
		stuck=false; //We done with this being stuck nonsense.
		if (stillStuckBehavior==stillStuckBehaviors.goHomeJumpHome)
		{
			goHomeJumpHomeCounter+=1;
			if (goHomeJumpHomeCounter==1) goHome();
			if (goHomeJumpHomeCounter>1)
			{
				goHomeJumpHomeCounter=0;
				bid.reset();
				jumpCharacter(homeMarker.transform.position);
			}
				
		}
		else 
			goHomeJumpHomeCounter=0;
		if (stillStuckBehavior==stillStuckBehaviors.idle)
			goIdle();
		if (stillStuckBehavior==stillStuckBehaviors.goHome)
			goHome();
		if (stillStuckBehavior==stillStuckBehaviors.jumpHome)
		{
			bid.reset();
			jumpCharacter(homeMarker.transform.position);
		}
		if (stillStuckBehavior==stillStuckBehaviors.die)
			bid.controller.die();
		bid.controller.Say(stillStuckMessage,4f);
	}
	
	//This is called by the character controller upon respawning.
	public void characterRespawned()
	{
		bid.reset();
		if (respawnBehavior==respawnBehaviors.idle) goIdle();
		if (respawnBehavior==respawnBehaviors.goHome) 
		{
			goHome();
		}
		if (respawnBehavior==respawnBehaviors.restartIncap) 
		{
			if (currentIncap!=null) playIncap(currentIncap.name);
			else goIdle();
		}
		if (respawnBehavior==respawnBehaviors.playFirstIncap) playIncap(incapAnimations[0].name);
		bid.controller.Say(respawnMessage,4f);
	}
	
	//Causes the character to walk horizontally towards the supplied position. If jumpToAvoid is true, then we will try to jump when we get stuck.
	public void walkTowards(Vector2 pos, bool  jumpToAvoid=true)
	{
		if (pos.x<transform.position.x) bid.horizontalMove = -runSpeed;
		if (pos.x>transform.position.x) bid.horizontalMove = runSpeed;
		if (pos.y>transform.position.y && Mathf.Abs(pos.y-transform.position.y)>1f) bid.jump = true;

		if (stuck && jumpToAvoid) bid.jump = true;
	}
	
	public void OnDrawGizmos() 
	{
		if (bid==null) return;
		if (drawTargetGizmo && target!=null)
		{	
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, 0.2f);
			
			Gizmos.color = Color.green;
			Gizmos.DrawIcon(target, "TIcon.png", false);
			Gizmos.DrawWireSphere(target, 0.5f);

		}
		if (debugInputs.Count>0)
		{
			int c = 2;
			GUIStyle style = new GUIStyle(); 
			style.normal.textColor = debugInputsColor;
			style.fontStyle = FontStyle.Bold;
			foreach(var i in debugInputs)
			{
				int id = bid.incapNameToID(i);
				Handles.Label(transform.position + Vector3.up * c*0.8f, i+": "+bid.incapGetValue(id), style);
				c+=1;
			}
		}
	}
	
	public void trigger(string triggerName)
	{
		foreach (var t in triggers)
		{
			if (t.name==triggerName) t.activate(gameObject, global);
		}
	}
	
	public string getCharacterName()
	{
		return bid.controller.name;
	}
}
