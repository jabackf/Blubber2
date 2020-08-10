using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable] public class completeCallback : UnityEvent<string> { } //See list definition of onCompletedCallback

/*Special keywords:
 * "[answer]" added to message text will replace the text in quotes with the last provided answer
 * "[title]" is replaced with the message box title (or character name)
 * "default" specified as an answer in the answer branch will act as a default if none of the answers are provided
 */

public class Dialog : MonoBehaviour
{
    [Space]
    [Header("Basic Settings")]

    public GameObject dialogBoxPrefab;
    [HideInInspector] public bool active = false;

    private string topperString = ""; //This string will be tacked onto the beginning of the next initiated message, then cleared. Used, for example, by AnswerBranch.addToNextMessage
    private string closerString = ""; //Like above, but added to the end of the message instead of the beginning.

    /*Types:
     * Straightshot plays whole dialog through to finish, using player input to initiate and progress through dialog boxes
     * Group plays through the currently selected message group, using player input to initiate and progress through dialog boxes
     * Random picks a message group randomly and plays through it, using player input to initiate and close the dialog box
     * AutoRandom plays random message group entries automatically with timers
     * AutoStraightshot plays the whole conversation through from beginning to end automatically with timers.
     * AutoStraightLoop does the same as above, but loops back to the beginning every time it ends
     * AutoGroup plays the selected group automatically with timers.
     * AutoGroupLoop works like above, but loops to the beginning of the group when it reaches the end
     * 
     * NOTES: The auto types aren't intended to work with menus, answer branches, etc. Those features are built for boxes that require input
    */

    public enum offScreenBehaviors { pause, restart, continueOffScreen, continueOnEdge };     //AUTO ONLY - If the conversation is playing automatically, then these are the actions to take when the speaking objects are full offscreen
                                                                                    //pause = pause stream until it re-enters screen, restart = stop stream and restart when it re-enters stream, continueOffScreen = keep playing conversation and allow it to go off screen (basically just set dbox.stayOnScreen to false), continueOnEdge = show stream on the edge of screen (basically doing nothing in code and leaving default settings)

    public enum type { Straightshot, Group, Random, AutoRandom, AutoStraightshot, AutoStraightshotLoop, AutoGroup, AutoGroupLoop };
    public bool outOfView = false;
    public type myType = type.Straightshot;
    public offScreenBehaviors offScreenBehavior = offScreenBehaviors.pause; //See offScreenBehaviors enum
    public Transform conversationCenterPoint; //This is the "center point" if the conversation, used primarily for determining if the character are on screen. If null, the center point will be assumed to be the gameObject.transform that this script is attached to
    public Vector2 outsideCameraBuffer = new Vector2(-2f, -2f); //This is the buffer that is applied to view edges when detecting if conversation is off screen. For instance, a coversation will be off the left side of the screen when conversationCenterPoint.x < screenLeftEdge+outsideCameraBuffer.x. Negative values mean the object needs to be further outside of the camera view.
    public bool dontRepeatRandoms = false; //If true, then any random type will avoid showing the same message twice in a row.
    public bool faceInitiator = true; //Requires characterController2D! If true, then character this dialog is attached to will face the initiator at start of any conversation with initiator. Character then turns back to original position at the end of conversation stream.
    private bool originallyFacingRight = false; //Stores direction the character was originally facing before turning to initiator
    private CharacterController2D characterController = null;

    private int currentGroup = 0;   //This stores the current group we're playing (only applies to types that use groups)
    public int startGroup = 0;      //This can be used in the inspector to set the currentGroup in the Start() function
    public void setCurrentGroup(int g) { currentGroup = g; }
    public int getCurrentGroup(int g) { return currentGroup; }

    private cameraFollowPlayer view;

    [SerializeField]
    public completeCallback[] onCompleteCallbacks;  //A list of callbacks sent when the dialog completes. Sends (string) as a parameter, with the string being the last provided answer

    [Space]
    [Header("Interuptions")]
    //The following variables handle interupting. Interupting is designed to be used with auto conversations.
    public Dialog interuptDialog = null;  //If this interuptDialog is playing out, it will be paused while this dialog is played. This variable holds the dialog that recieves the "interupt" and "resume" messages
                                          //NOTE: An interupted dialog DOES NOT send callbacks when it ends. It DOES send start and end messages though.
    public bool restartAfterInterupt = false; //If true, the interupted dialog will be restarted from the beginning when resumed. If false, it will pickup where it left off.
    private bool activeBeforeInterupt = false; //Used to store rather the dialog was even playing when we interupted it.
    public float waitTimeBeforeResume = 1.5f;   //Amount of time to wait before resuming
    private bool resuming = false; //A flag to tell us if we are resuming an existing conversation or not. Used to repeat the current message.
    public string resumeTopperString = "";   //If the conversation is interupted and resumed, this string will be added to message that we resume on. Example: "What was I saying again? Oh ya! " + last displayed message
    public float resumeMessageAddTime = 2f; //This time is added to the onScreen timer. This is to account for the topper string.

    [Space]
    [Header("AutoTimers")]

    //The following timers are used for initiating / closing auto types
    public float defaultOnScreenTime = 8f;  //This is the value that each dialog boxes on/off screen time will default to if no other value is provided
    public float defaultOffScreenTime = 1.4f;
    private bool onScreen = false; //used by auto types to know if dbox is on screen, or if we're waiting before showing the next one
    public float autoOnTimeMultiplier = 1; //This is multiplied to all onScreen times. A value greater than one, for example, will make all dialog entries show for longer
    public float autoOffTimeMultiplier = 1; //This is multiplied to all offScreen times. A value less than one, for example, will make dialogs stay off screen for a shorter period of time
    private bool isAutoType = false;
    private float onScreenTimer;
    private float offScreenTimer;
    private float addToTimer = 0f;
	private string nextBoxOptions = "";

    //This struct couples potential answers to dialog questions with entries indexes to jump to if that answer is provided.
    [System.Serializable]
    public struct AnswerBranch
    {
        public string answer;
        public int index; //The index to jump to if this.answer matches the user selected answer
        public bool ignoreCase; //If true, it will ignore the case of the answer. Meaning, "stinkypants" will match "StInKyPaNtS"
        public string addToNextMessage; //If not empty, this will be added to the beginning of the next message. For example, "You didn't enter anything! " can be added to the beginning of a message asking for input

        public AnswerBranch(string answer, int jumpto, bool ignoreCase=false, string addToNext="")
        {
            this.answer = answer;
            this.index = jumpto;
            this.ignoreCase = ignoreCase;
            this.addToNextMessage = addToNext;
        }
    }

    [System.Serializable]
    public class entry
    {
        public string Message = "";
        public string Title = "";
        public bool saidByInitiator = false; //If set to true, the initiator's info will be used for this box
        public string sendMessageStart = ""; //If not empty, then this message is sent to the gameObject (or initiator, if gameObject is null) at the start of this box. You can send multiple messages by separating messages with pipe (e.g. Angry|CircleOn)
        public string sendMessageEnd = ""; //Same as above, but it sent at the end of the message
        public GameObject gameObject;
        public string startAnimation; //Animation to play when the box first opens. Applies to the speaker or the specified gameObject if not null
        public string endAnimation;  //Animation to play when the box closes
        public Transform locationTop; //If both transforms are null and saidByInitiator is false, the dialog will appear in the center of the screen
        public Transform locationBottom;
        public int group = 0; //This can be used to cluster together messages in groups. For instance, if the conversation is RandomSingle, then a random group will be chosen and all messages in that group will be displayed in order
        public int jumpTo = -1;
        public float timeOnScreen = -1; //For auto types only. The amount of time the box will be displayed on screen. 0 or less = the default time specified by defaultOnScreenTime
        public float timeOffScreen = -1; //The amount of time AFTER this box closes before the next box displays
        public bool getTextInput = false;
        public bool repeatIfEmpty = true; //Repeats this dialog if the answer is empty
        public string repeatAddText = ""; //If repeatIfEmpty is true, and an empty answer is given, this string is tacked to the front of the next message. For example "You didn't enter anything! Try again. "
        public InputField.CharacterValidation inputType = InputField.CharacterValidation.Alphanumeric;
        public string imageResource = "";
        public List<string> answers = new List<string>();
        public List<AnswerBranch> answerBranch = new List<AnswerBranch>();  //This is a list of potential answers coupled with entries indexes to jump to if that answer is provided.
        public UnityEvent callback; //An optional callback for when the current dialog ends.
    }

    [Space]
    [Header("Dialog chain")]

    [SerializeField] public List<entry> entries;

    private int index = 0;
    private GameObject initiator;
    private string initiatorName;
    private Transform initiatorTop;
    private Transform initiatorBottom;

    private string lastAnswer = "";
    private GameObject dialogBoxObj;
    private DialogBox dialogBox;

    public int getIndex() { return index; }
    public void setIndex(int i) { this.index = i; }
    public string getAnswer() { return lastAnswer; }
    public void setAnswer(string answer) { this.lastAnswer = answer; }
    public string getNextText() { return entries[getNextIndex()].Message; }
    public void setNextText(string message) { entries[getNextIndex()].Message = message; }
    public void addToNextMessageBeginning(string text) { this.topperString = text; }
    public void addToNextMessageEnd(string text) { this.closerString = text; }
    public void addTime(float time) { addToTimer = time; }

    // Start is called before the first frame update
    void Start()
    {
        view = Camera.main.GetComponent<cameraFollowPlayer>() as cameraFollowPlayer;
        if (conversationCenterPoint == null) conversationCenterPoint = gameObject.transform;

        characterController = gameObject.GetComponent<CharacterController2D>() as CharacterController2D;
        currentGroup = startGroup;
        if (myType == type.AutoGroup || myType == type.AutoGroupLoop || myType == type.AutoRandom || myType == type.AutoStraightshot || myType == type.AutoStraightshotLoop)
        {
            isAutoType = true;
            Initiate();
        }
    }

    bool inView()
    {
        if (!view) return true; //If there is no script to make the camera follow the player, then assume we are in view
        else
        {
            if (view.isActiveAndEnabled)
                return view.insideView(conversationCenterPoint, outsideCameraBuffer.x, outsideCameraBuffer.y);
            else
                return true; //If the component is there but disabled, then we aren't moving the view so we'll just return true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isAutoType)
        {
            if (inView() || offScreenBehavior==offScreenBehaviors.continueOffScreen || offScreenBehavior == offScreenBehaviors.continueOnEdge) //Conversation is within camera view OR we don't care rather it is in view or not
            {
                if (outOfView) //We were outside of camera view, but we just re-entered and this is the first cycle that we've detected it
                {
                    outOfView = false;
                    switch (offScreenBehavior)
                    {
                        case offScreenBehaviors.pause:
                            resume(false);
                            break;
                        case offScreenBehaviors.restart:
                            resume(false, true);
                            break;
                        case offScreenBehaviors.continueOffScreen:

                            break;
                    }
                }

                if (onScreen) //We're not between dialog boxes
                {
                    if (onScreenTimer > 0) onScreenTimer -= Time.deltaTime;
                    else
                    {
                        if (dialogBox != null) dialogBox.closeBox();
                        offScreenTimer = entries[index].timeOffScreen > 0 ? entries[index].timeOffScreen * autoOffTimeMultiplier : defaultOffScreenTime * autoOffTimeMultiplier;
                        onScreen = false;
                    }
                }
                else //Not currently on the screen
                {
                    if (offScreenTimer > 0) offScreenTimer -= Time.deltaTime;
                    else
                    {
                        Next();
                        onScreen = true;
                        onScreenTimer = entries[index].timeOnScreen > 0 ? (entries[index].timeOnScreen + addToTimer) * autoOnTimeMultiplier : (defaultOnScreenTime + addToTimer) * autoOnTimeMultiplier;
                        addToTimer = 0f;
                    }
                }
            }
            else
            {
                
                if (outOfView == false) //We just went out of view and this is the first cycle that we have detected it
                {
                    outOfView = true;

                    switch (offScreenBehavior)
                    {
                        case offScreenBehaviors.pause:
                            interupt();
                        break;
                        case offScreenBehaviors.restart:
                            interupt();
                        break;
                        case offScreenBehaviors.continueOffScreen:

                        break;
                    }
                }
            }
        }
		else
		{
			if (!onScreen)
			{
                if (offScreenTimer > 0) offScreenTimer -= Time.deltaTime;
                else
                {
                    Next(nextBoxOptions);
                    onScreen = true;
					offScreenTimer = entries[index].timeOffScreen;

                }
            }
		}
    }

    //This method can be called to remotely add to the list of answers of the specified dialog index.
    //For example, a dresser might call injectAnswerBranch for each item of clothing. This allows the dresser script to control all of the options presented.
    //You would then use the onCompleteCallback to send the selected answer back to the dresser script
    public void injectAnswerBranch(int injectIndex, int jumptoIndex, string answer, string addToNextMessage="", bool ignoreCase=true)
    {
        entries[injectIndex].answers.Add(answer);
        entries[injectIndex].answerBranch.Add(new AnswerBranch(answer, jumptoIndex, ignoreCase, addToNextMessage));
        
    }

    //Called when someone is interupting our conversation.
    //Intended primarily for when NPCs are having an automatic conversation and the player interupts them 
    //Timer is an optional variable. If it's not -1, then it will automatically invoke resume after the time is up
    public void interupt(float timer=-1)
    {
        if (!active) return;

        activeBeforeInterupt = active;
        active = false;
        onScreen = false;
        KillBox(false);

        if (timer != -1) Invoke("invokeResume", timer);
    }

    public void invokeResume()
    {
        //So you can't use Invoke("resume") because there are optional parameters. For some reason that's a no-no, even though if I'm using Invoke I don't want to pass parameters anyway.
        //Instead I've created this invokeResume function that simple calls resume() without any parameters. Now I can call Invoke("invokeResume") and everything is just wonderful.
        resume();
    }

    //Resumes an interupted dialog
    public void resume(bool attachInteruptString = true, bool restart=false)
    {
        if (activeBeforeInterupt)
        {
            topperString = (attachInteruptString ? resumeTopperString : "");
            addTime(resumeMessageAddTime);

            if (restartAfterInterupt || restart)
                Initiate(initiatorName, initiator, initiatorTop, initiatorBottom);
            else
            {
                active = true;
                onScreen = false;
                onScreenTimer = 0;
                resuming = true;
                offScreenTimer = waitTimeBeforeResume * autoOffTimeMultiplier;

            }
        }
    }

    //Initiates a dialog sequence
    public void Initiate(string name = "", GameObject go = null, Transform top = null, Transform bottom = null)
    {
        if (interuptDialog != null) interuptDialog.interupt();

        initiator = go;
        initiatorName = name;
        initiatorTop = top;
        initiatorBottom = bottom;
        active = true;

        if (characterController != null && initiator != null)
        {
            characterController.ClearSay();
            if (faceInitiator)
            {
                originallyFacingRight = characterController.isFacingRight();
                if (initiator.transform.position.x < gameObject.transform.position.x && originallyFacingRight)
                {
                    characterController.Flip();
                }
                if (initiator.transform.position.x > gameObject.transform.position.x && !originallyFacingRight)
                {
                    characterController.Flip();
                }
            }
        }

        if (initiator != null && !isAutoType)
        {
            initiator.SendMessage("StartTalking", SendMessageOptions.DontRequireReceiver);
        }

        if (myType == type.AutoStraightshot || myType == type.Straightshot || myType == type.AutoStraightshotLoop) //Starting at 0
            index = 0;
        else //The type uses groups
        {
            if (myType == type.AutoRandom || myType == type.Random) //Pick a random group
            {
                currentGroup = getRandomGroup();
            }
            index = getGroupStartIndex(currentGroup);
        }

        LoadBox();

		onScreen = true;
        if (isAutoType)
        {
            onScreenTimer = entries[index].timeOnScreen > 0 ? (entries[index].timeOnScreen+addToTimer) * autoOnTimeMultiplier : (defaultOnScreenTime+addToTimer) * autoOnTimeMultiplier;
        }
    }

    //Returns a list of all unique group id's in the entry list
    public List<int> getGroupList()
    {
        List<int> l = new List<int>();
        foreach (entry e in entries)
        {
            if (!l.Contains(e.group)) l.Add(e.group);
        }
        return l;
    }

    //Returns a group randomly selected from all unique groups in the entries list
    public int getRandomGroup()
    {
        List<int> groups = getGroupList();
        var random = new System.Random();
        int i = random.Next(groups.Count);

        if (dontRepeatRandoms && groups.Count > 1)
        {
            while (groups[i] == currentGroup) i = random.Next(groups.Count);
        }

        return groups[i];
    }

    //Returns the first index in entries that has the specified group number. Returns 0 if group is not found
    private int getGroupStartIndex(int group)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].group == group) return i;
        }
        return 0;
    }
    //Returns the last index in entries that has the specified group number. Returns 0 if group is not found
    private int getGroupEndIndex(int group)
    {
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            if (entries[i].group == group) return i;
        }
        return 0;
    }

    //Takes all settings into account and determines what the next entry index should be.
    //Returns -1 if we've reached the end of the current dialog chain or group, and we don't need to loop.
    public int getNextIndex()
    {
        int end = entries.Count - 1;
        if (myType == type.AutoGroup || myType == type.AutoRandom || myType == type.Random || myType == type.Group || myType == type.AutoGroupLoop) //These types use message groups
        {
            end = getGroupEndIndex(currentGroup);
        }

        //First let's see if there is somewhere we need to jump to
        if (entries[index].jumpTo != -1) return entries[index].jumpTo;

        if (entries[index].answerBranch.Count > 0)
        {
            foreach (AnswerBranch a in entries[index].answerBranch)
            {
                string answer = a.answer;
                string lAnswer = lastAnswer;
                if (a.ignoreCase)
                {
                    answer = answer.ToLower();
                    lAnswer = lAnswer.ToLower();
                }
                if (answer == lAnswer)
                {
                    addToNextMessageBeginning(a.addToNextMessage);
                    return a.index;
                }
            }

            //Didn't find the correct answer. Let's check for a default.
            foreach (AnswerBranch a in entries[index].answerBranch)
            {
                if (a.answer.ToLower() == "default")
                {
                    addToNextMessageBeginning(a.addToNextMessage);
                    return a.index;
                }
            }
        }

        //There are no jumpTo's or answer branches, so we need to increment to the next index instead
        if (index < end)
        {
            return index + 1;
        }
        else
        {
            if (myType == type.AutoStraightshotLoop) return 0;
            if (myType == type.AutoGroupLoop)
            {
                return getGroupStartIndex(currentGroup);
            }
        }

        //Nowhere to go.
        return -1;
    }

    //This function actually selects the next box and runs the LoadBox method
    public void Next(string answer = "NoneProvided")
    {
        if (!active) return;

        bool repeat = false;
        if (answer != "NoneProvided")
        {
            lastAnswer = answer;
            if (entries[index].repeatIfEmpty && String.IsNullOrWhiteSpace(lastAnswer))
            {
                addToNextMessageBeginning(entries[index].repeatAddText);
                repeat = true;
            }
        }

        if (dialogBox != null) KillBox();

        if (resuming) //Our conversation was interupted. Let's resume by repeating the last message.
        {
            repeat = true;
            resuming = false;
        }

        if (!repeat)
        {
            int next = getNextIndex();
            if (next != -1)
            {
                index = next;

                //If we have to jump or branch off into a different group, then we want to change the current group to avoid errors
                currentGroup = entries[index].group;

                LoadBox();
            }
            else
            {
                EndConversation();
            }
        } //End !repeat
        else
        {
            LoadBox(); //Repeat the message with new text appended
        }
    }

    public void EndConversation()
    {
        active = false;
        if (interuptDialog != null) interuptDialog.resume();
        if (initiator != null)
        {
            initiator.SendMessage("StopTalking");
        }

        if (characterController != null && initiator != null && faceInitiator)
        {
            if (!characterController.isFacingRight() && originallyFacingRight)
                characterController.Flip();
            if (characterController.isFacingRight() && !originallyFacingRight)
                characterController.Flip();
        }

        foreach (completeCallback c in onCompleteCallbacks)
        {
            c.Invoke(lastAnswer);
        }
    }

    //This function actually instantiates the box and passes the settings to it
    public void LoadBox()
    {
        if (!active) return;

        dialogBoxObj = Instantiate(dialogBoxPrefab);
        dialogBox = dialogBoxObj.GetComponent<DialogBox>() as DialogBox;
        if (entries[index].saidByInitiator)
        {
            dialogBox.followTop = initiatorTop;
            dialogBox.followBottom = initiatorBottom;
            dialogBox.title = initiatorName;
        }
        else
        {
            //Try to pull character information, unless it was otherwise specified
            if (entries[index].gameObject != null)
            {
                CharacterController2D cont = entries[index].gameObject.GetComponent<CharacterController2D>() as CharacterController2D;
                if (cont != null)
                {
                    if (entries[index].Title == "") entries[index].Title = cont.CharacterName;
                    if (entries[index].locationTop == null)
                    {
                        entries[index].locationTop = cont.getDialogTop();
                        if (entries[index].locationBottom == null)
                            entries[index].locationBottom = cont.getDialogBottom();
                    }
                }
            }

            dialogBox.followTop = entries[index].locationTop;
            dialogBox.followBottom = entries[index].locationBottom;
            dialogBox.title = entries[index].Title;
        }

        dialogBox.text = topperString +entries[index].Message.Replace("[answer]", lastAnswer).Replace("[title]", dialogBox.title)+ closerString;
        topperString = closerString = "";
        dialogBox.imgResource = entries[index].imageResource;
        dialogBox.dialogParent = this;
        dialogBox.answers = entries[index].answers;
        dialogBox.getTextInput = entries[index].getTextInput;
        dialogBox.inputType = entries[index].inputType;

        if (isAutoType)
        {
            dialogBox.isAuto = true;
            if (offScreenBehavior == offScreenBehaviors.continueOffScreen) dialogBox.stayOnScreen = false;
        }

        if (entries[index].sendMessageStart != "")
        {
            string[] msgs = entries[index].sendMessageStart.Split('|');

            if (entries[index].gameObject != null)
            {
                foreach (var msg in msgs)
                {
                    entries[index].gameObject.SendMessage(msg, null, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                foreach (var msg in msgs)
                {
                    if (initiator != null) initiator.SendMessage(msg, null, SendMessageOptions.DontRequireReceiver);

                }
            }
        }
        if (entries[index].startAnimation != "")
        {

            if (entries[index].gameObject != null)
            {
                entries[index].gameObject.GetComponent<Animator>().Play(entries[index].startAnimation);
            }
            else
            {
                if (initiator != null) initiator.GetComponent<Animator>().Play(entries[index].startAnimation);
            }
        }
    }

    //Kill the current dialog box
    public void KillBox(bool invokeCallBacks=true)
    {
        if (dialogBox == null) return;

        if (invokeCallBacks)
        {
            if (entries[index].callback != null)
            {
                entries[index].callback.Invoke();
            }
        }

        if (entries[index].sendMessageEnd != "")
        {
            string[] msgs = entries[index].sendMessageEnd.Split('|');

            if (entries[index].gameObject != null)
            {
                foreach (var msg in msgs)
                {
                    entries[index].gameObject.SendMessage(msg, null, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                foreach (var msg in msgs)
                {
                    if (initiator != null) initiator.SendMessage(msg, null, SendMessageOptions.DontRequireReceiver);

                }
            }
        }

        if (entries[index].endAnimation != "")
        {

            if (entries[index].gameObject != null)
            {
                entries[index].gameObject.GetComponent<Animator>().Play(entries[index].endAnimation);
            }
            else
            {
                if (initiator != null) initiator.GetComponent<Animator>().Play(entries[index].endAnimation);
            }
        }

        dialogBox.Kill();
        Destroy(dialogBox);
        dialogBox = null;
    }
	
	//This will set a string that is passed to the Next function next time it runs. Used to pass answers after offScreenTimer is complete. (only for dialogs that require user input to progress)
	public void setNextBoxOptions(string options)
	{
		nextBoxOptions = options;
	}
	
	public void setOnScreen(bool val)
	{
		onScreen= val;
	}
	
}