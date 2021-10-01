using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This script gives us a system for configuring character to react to generic things. Say something nasty happens and this character recieves a Gross() message. This script lets us connect that Gross message with various kinds of responses.

public class React : MonoBehaviour
{
    [System.Serializable]
    public class reaction
    {
        public enum isTalkingResponses
        {
            react, dontReact, interupt
        }

        //If we turn to face the initator, then we need to know what to do after the reaction is done. Should we turn back?
        public enum facingReturnBehaviors
        {
            priorToReact, //Get the facing direction immediately before the reaction and return to this direction. Note that if you initate a reaction, turn the character, then initiate another reaction while the character is still turned then the character towards initator direction will be the new previous direction. If you have a character that is generally intended to face a certain direction, it's better to use the next option
            useStartFacing, //Return to the facing direction that the character was initially in at the start of the scene. This tends to be better for NPCs that generally tend to face in a given direction, perhaps because they are in auto dialog with another npc.
            none //Do nothing. Just keep on facing in the direction that the initiator was in at the start of the reaction.
        }


        public isTalkingResponses isTalkingResponse = isTalkingResponses.interupt;

        public string name;
        public bool faceInitiator = true; //Requires a CharacterController2D on this object. It also requires an initiator be passed with the reaction. This makes the character turn to face the initiator when the reaction is triggered. Character turns back when the reaction is complete.
        public facingReturnBehaviors facingReturnBehavior = facingReturnBehaviors.useStartFacing;
        [HideInInspector] public bool previousIsFacingRight; //The facing direction that the character was pointing before faceInitiator was triggered. Used to reverse the faceInitiator action.

        public List<AudioClip> playSound = new List<AudioClip>();
        public float randomizePitchMin = 1f, randomizePitchMax = 1f;

        //Some data that can optionally be sent with the reaction message. For example, PShot sends Vector3 indicating position that the bullet hit.
        [HideInInspector] public Vector3 data_position;
        [HideInInspector] public GameObject data_gameObject;
        [HideInInspector] public string data_string;

        public float sayTime = 6f;
        public float delayedMessagesTime = 7f;
        public float initiatorMessagesTime = 1f;
        public float initiatorSayDelay = 1f;

        //An initiator is an object that ultimately caused us to recieve a command. If a player throws a turd and the turd hits us in the face and sends us a Gross() command then the turd can also send us the initiator (the thrower). We can then send a Mean() message to the initiator.
        [HideInInspector] public GameObject initiator; //Stores the most recently detected initiator. This gets cleared after initiatorMessages are sent.

        public List<string> sayStrings = new List<string>();
		public string animation;	//Name of an animation to play
        public List<UnityEvent> Callbacks = new List<UnityEvent>();
        public List<string> sendMessages = new List<string>();
        public List<string> delayedMessages = new List<string>(); //Like send messages, but these are delayed by delayedMessagesTime amount. E.g., you could send Angry at the start then Normal as a delay
        public List<string> initiatorMessages = new List<string>(); //Send messages to the initiator of the object (if this object has a pickupObject.recentlyThrownBy). This can be set to a delay specified by initiatorMessagesTime
        public List<string> initiatorSay = new List<string>(); //Send messages to the initiator of the object (if this object has a pickupObject.recentlyThrownBy). This can be set to a delay specified by initiatorMessagesTime


        //Store all of the defaults that were initially specified when the character was configured in the editor.
        //This is so we can change these values in certain contexts (using override preExecute()) then revert back to the default state if needed
        private float def_sayTime;
        private float def_delayedMessagesTime;
        private float def_initiatorMessagesTime;
        private float def_initiatorMessagesTwoTime;
		private string def_animation; 
        private List<string> def_sayStrings;
        private List<UnityEvent> def_Callbacks;
        private List<string> def_sendMessages;
        private List<string> def_delayedMessages;
        private List<string> def_initiatorMessages;
        private List<string> def_initiatorSay;
        private float def_initiatorSayDelay;
        private List<AudioClip> def_playSound = new List<AudioClip>();
        private float def_randomizePitchMin = 1f, def_randomizePitchMax = 1f;

        public reaction(string name)
        {
            this.name = name;

            def_sayTime = sayTime;
            def_delayedMessagesTime = delayedMessagesTime;
            def_initiatorMessagesTime = initiatorMessagesTime;
            def_sayStrings = sayStrings;
			def_animation = animation;
            def_Callbacks = Callbacks;
            def_sendMessages = sendMessages;
            def_delayedMessages = delayedMessages;
            def_initiatorMessages = initiatorMessages;
            def_initiatorSay = initiatorSay;
            def_initiatorSayDelay = initiatorSayDelay;
            def_playSound = playSound;
            def_randomizePitchMin = randomizePitchMin;
            def_randomizePitchMax = randomizePitchMax;
        }

        //Resets all value to default configuration specified in the editor
        public void resetToDefault()
        {
            sayTime = def_sayTime;
            delayedMessagesTime = def_delayedMessagesTime;
            initiatorMessagesTime = def_initiatorMessagesTime;
            sayStrings = def_sayStrings;
			animation = def_animation;
            Callbacks = def_Callbacks;
            sendMessages = def_sendMessages;
            delayedMessages = def_delayedMessages;
            initiatorMessages = def_initiatorMessages;
            initiatorSay = def_initiatorSay;
            initiatorSayDelay = def_initiatorSayDelay;
            playSound = def_playSound;
            randomizePitchMin = def_randomizePitchMin;
            randomizePitchMax = def_randomizePitchMax;
        }

        //This function looks at the settings and determines the maximum amount of time it will take to send the final reaction message or clear the final sayString.
        //It's not a perfect way to work this out, but in general we can assume that if we wait the amount of time returned then the reaction will have mostly played out.
        public float getFullReactionTime()
        {
            float maxTime = 0;
            if (sayStrings.Count > 0)
            {
                if (sayTime > maxTime) maxTime = sayTime;
            }
            if (delayedMessages.Count > 0)
            {
                if (delayedMessagesTime > maxTime) maxTime = delayedMessagesTime;
            }
            if (initiatorMessages.Count > 0)
            {
                if (initiatorMessagesTime > maxTime) maxTime = initiatorMessagesTime;
            }
            if (initiatorSay.Count > 0)
            {
                if (initiatorSayDelay > maxTime) maxTime = initiatorSayDelay;
            }
            return maxTime;
        }
    }

    //An optional specific dialog that we can interupt for a reaction. Basically, if reaction.isTalkingResponse is set to interupt, the default behavior when a reaction occurs while the character is talking is to send an Interupt() message to shut him or her up.
    //Sometimes a character is engaged in auto dialog with another character, and both characters won't have the dialog component. If you send a reaction to the character that doesn't have the dialog component then it won't interupt the dialog.
    //This interuptDialog variable can be assigned to forward the interupt message to a different character's dialog component.
    public Dialog interuptDialog;

    //This is where you add new reactions
    public reaction rGross = new reaction("Gross"); //Triggered when something gross happens to this character
    public void IGross(GameObject initiator) { rGross.initiator = initiator; execute(rGross); }
    public void Gross() { rGross.initiator = null;  execute(rGross); }

    public reaction rMean = new reaction("Mean"); //Triggered when another character is angry with you
    public void IMean(GameObject initiator) { rMean.initiator = initiator; execute(rMean); }
    public void Mean() { rMean.initiator = null; execute(rMean); }

    public reaction rOuch = new reaction("Ouch"); //Usually triggered when something solid hits you pretty hard
    public void IOuch(GameObject initiator) { rOuch.initiator = initiator; execute(rOuch); }
    public void Ouch() { rOuch.initiator = null; execute(rOuch); }

    public reaction rExplode = new reaction("Explode"); //Triggered when this character is within explosion range
    public void IExplode(GameObject initiator) { rExplode.initiator = initiator; execute(rExplode); }
    public void Explode() { rExplode.initiator = null; execute(rExplode); }

    public reaction rShot = new reaction("Shot"); //Triggered when a character is shot by ammunition
    public void IShot(GameObject initiator) { rShot.initiator = initiator; execute(rShot); }
    public void PShot(Vector3 position) { rShot.data_position = position; execute(rShot); } //Carries the position that the shot took place.
    public void Shot() { rShot.initiator = null; execute(rShot); }
	
	public reaction rCustomA = new reaction("CustomA"); //A generic thing. Use for whatever fits your fancy.
    public void ICustomA(GameObject initiator) { rCustomA.initiator = initiator; execute(rCustomA); }
    public void CustomA() { rCustomA.initiator = null;  execute(rCustomA); }
	
	public reaction rCustomB = new reaction("CustomB"); //Another generic thing
    public void ICustomB(GameObject initiator) { rCustomB.initiator = initiator; execute(rCustomB); }
    public void CustomB() { rCustomB.initiator = null;  execute(rCustomB); }


    CharacterController2D cont;
    [HideInInspector] public BlubberAnimation blubberAnim;
    Global global;

    public void Start()
    {
        cont = GetComponent<CharacterController2D>();
        blubberAnim = GetComponent<BlubberAnimation>();
        global = GameObject.FindWithTag("global").GetComponent<Global>();

    }

    public void execute(reaction r)
    {


        if (cont)
        {
            if (cont.getIsPaused() || cont.isCharacterDead()) return;
            if (r.isTalkingResponse == reaction.isTalkingResponses.dontReact) return;
            if (r.isTalkingResponse == reaction.isTalkingResponses.interupt)
            {
                if (!interuptDialog)
                    gameObject.SendMessage("interupt", r.sayTime + 1, SendMessageOptions.DontRequireReceiver);
                else
                    interuptDialog.interupt(r.sayTime + 1);
            }
        }

        preExecute(r);

        if (cont && r.sayStrings.Count>0)
            cont.Say(r.sayStrings.ToArray(), r.sayTime);

        foreach (var c in r.Callbacks) c.Invoke();
        foreach (var m in r.sendMessages) gameObject.SendMessage(m, SendMessageOptions.DontRequireReceiver);

        if (r.delayedMessages.Count>=0)
        {
            StartCoroutine(executeDelayedMessages(r));
        }

        if (r.initiator != null)
        {
            if (r.faceInitiator && cont)
            {
                r.previousIsFacingRight = cont.isFacingRight();
                if (r.initiator.transform.position.x > gameObject.transform.position.x) cont.FaceRight();
                else cont.FaceLeft();
                StartCoroutine(returnFacingDirection(r));
            }

            if (r.initiatorMessages.Count >= 0)
                StartCoroutine(executeInitiatorMessages(r));
            if (r.initiatorSay.Count >= 0)
                StartCoroutine(executeInitiatorSay(r));
        }

        if (r.playSound.Count > 0) global.audio.RandomSoundEffect(r.playSound.ToArray(), r.randomizePitchMin, r.randomizePitchMax);
		
		if (r.animation!="")
		{
			gameObject.GetComponent<Animator>().Play(r.animation);
		}

    }

    //If r.faceInitiator is used then the character will turn to face the initiator. This function will be triggered at the end of the reaction to turn the character back to his original facing direction.
    IEnumerator returnFacingDirection(reaction r)
    {
        yield return new WaitForSeconds(r.getFullReactionTime()+1);

        if (r.facingReturnBehavior == reaction.facingReturnBehaviors.priorToReact)
        {
            if (r.previousIsFacingRight) cont.FaceRight();
            else cont.FaceLeft();
        }
        if (r.facingReturnBehavior == reaction.facingReturnBehaviors.useStartFacing)
        {
            cont.faceInitialDirection();
        }
    }

    IEnumerator executeDelayedMessages(reaction r)
    {
        yield return new WaitForSeconds(r.delayedMessagesTime);
        foreach (var m in r.delayedMessages) gameObject.SendMessage(m, SendMessageOptions.DontRequireReceiver);
    }

    IEnumerator executeInitiatorMessages(reaction r)
    {
        yield return new WaitForSeconds(r.initiatorMessagesTime);
        if (r.initiator != null)
        { 
            foreach (var m in r.initiatorMessages)
            {
                r.initiator.SendMessage(m, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    IEnumerator executeInitiatorSay(reaction r)
    {
        yield return new WaitForSeconds(r.initiatorSayDelay);

        if (r.initiator != null)
        {
            CharacterController2D icont = r.initiator.GetComponent<CharacterController2D>();
            if (icont)
                icont.Say(r.initiatorSay.ToArray(), r.sayTime);
        }
    }

    public virtual void preExecute(reaction r)
    {
        //Specific characters can inherit from this class and use this function to modify reactions before they are executed.
        //For example, a player might have ReactPlayer :: React with override preExecute() { if dress==santaHat and r.name=="Mean") r.sayStrings = "Merry Christmas, you filthy animal!"; }
    }
}
