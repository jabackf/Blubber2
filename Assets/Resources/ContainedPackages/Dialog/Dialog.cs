using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Dialog : MonoBehaviour
{
    public GameObject dialogBoxPrefab;
    public bool active = false;

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
    
    public enum type {Straightshot, Group, Random, AutoRandom, AutoStraightshot, AutoStraightshotLoop, AutoGroup, AutoGroupLoop };
    public type myType = type.Straightshot;

    private int currentGroup = 0;   //This stores the current group we're playing (only applies to types that use groups)
    public int startGroup = 0;      //This can be used in the inspector to set the currentGroup in the Start() function
    public void setCurrentGroup(int g) { currentGroup = g; }
    public int getCurrentGroup(int g) { return currentGroup; }
    public bool dontRepeatRandoms = false; //If true, then any random type will avoid showing the same message twice in a row.

    //The following timers are used for initiating / closing auto types
    private bool isAutoType = false;
    private float onScreenTimer;
    private float offScreenTimer;
    private bool onScreen = false; //used by auto types to know if dbox is on screen, or if we're waiting before showing the next one
    public float autoOnTimeMultiplier = 1; //This is multiplied to all onScreen times. A value greater than one, for example, will make all dialog entries show for longer
    public float autoOffTimeMultiplier = 1; //This is multiplied to all offScreen times. A value less than one, for example, will make dialogs stay off screen for a shorter period of time

    //This struct couples potential answers to dialog questions with entries indexes to jump to if that answer is provided.
    [System.Serializable]
    public struct AnswerBranch
    {
        public string answer;
        public int index;
    }

    [System.Serializable]
    public class entry
    {
        public UnityEvent callback; //An optional callback for when the current dialog ends.
        public bool saidByInitiator = false; //If set to true, the initiator's info will be used for this box
        public GameObject gameObject; 
        public Transform locationTop; //If both transforms are null and saidByInitiator is false, the dialog will appear in the center of the screen
        public Transform locationBottom;
        public int group = 0; //This can be used to cluster together messages in groups. For instance, if the conversation is RandomSingle, then a random group will be chosen and all messages in that group will be displayed in order
        public int jumpTo = -1;
        public float timeOnScreen = 8f; //For auto types only. The amount of time the box will be displayed on screen
        public float timeOffScreen = 1.4f; //For auto types only. The amount of time AFTER this box closes before the next box displays
        public string Message = "";
        public string Title = "";
        public bool getTextInput = false;
        public InputField.CharacterValidation inputType = InputField.CharacterValidation.Alphanumeric;
        public string imageResource = "";
        public List<string> answers = new List<string>();
        public List<AnswerBranch> answerBranch = new List<AnswerBranch>();  //This is a list of potential answers coupled with entries indexes to jump to if that answer is provided.
    }

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

    // Start is called before the first frame update
    void Start()
    {
        currentGroup = startGroup;
        if (myType == type.AutoGroup || myType == type.AutoGroupLoop || myType == type.AutoRandom || myType == type.AutoStraightshot || myType == type.AutoStraightshotLoop)
        {
            isAutoType = true;
            Initiate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isAutoType)
        {
            if (onScreen)
            {
                if (onScreenTimer > 0) onScreenTimer -= Time.deltaTime;
                else
                {
                    if (dialogBox != null) dialogBox.closeBox();
                    offScreenTimer = entries[index].timeOffScreen*autoOffTimeMultiplier;
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
                    onScreenTimer = entries[index].timeOnScreen*autoOnTimeMultiplier;
                }
            }
        }
    }

    //Initiates a dialog sequence
    public void Initiate(string name="", GameObject go=null, Transform top=null, Transform bottom=null)
    {
        initiator = go;
        initiatorName = name;
        initiatorTop = top;
        initiatorBottom = bottom;
        active = true;

        if (myType == type.AutoStraightshot|| myType == type.Straightshot|| myType == type.AutoStraightshotLoop) //Starting at 0
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

        if (isAutoType)
        {
            onScreen = true;
            onScreenTimer = entries[index].timeOnScreen*autoOnTimeMultiplier;
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

        if (dontRepeatRandoms && groups.Count>1)
        {
            while (groups[i]==currentGroup) i=random.Next(groups.Count);
        }

        return groups[i];
    }

    //Returns the first index in entries that has the specified group number. Returns 0 if group is not found
    private int getGroupStartIndex(int group)
    {
        for(int i = 0; i<entries.Count; i++)
        {
            if (entries[i].group == group) return i;
        }
        return 0;
    }
    //Returns the last index in entries that has the specified group number. Returns 0 if group is not found
    private int getGroupEndIndex(int group)
    {
        for (int i = entries.Count-1; i >= 0; i--)
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
                if (a.answer == lastAnswer)
                {
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
        if (answer != "NoneProvided") lastAnswer = answer;

        if (dialogBox!=null) KillBox();

        int next = getNextIndex();
        if (next != - 1)
        {
            index = next;

            //If we have to jump or branch off into a different group, then we want to change the current group to avoid errors
            currentGroup = entries[index].group;

            LoadBox();
        }
        else
        {
            active = false;
            if (initiator != null)
            {
                initiator.SendMessage("StopTalking");
            }
        }
    }

    //This function actually instantiates the box and passes the settings to it
    public void LoadBox()
    {
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
            dialogBox.followTop = entries[index].locationTop;
            dialogBox.followBottom = entries[index].locationBottom;
            dialogBox.title = entries[index].Title;
        }

        dialogBox.text = entries[index].Message.Replace("[answer]", lastAnswer);
        dialogBox.imgResource = entries[index].imageResource;
        dialogBox.dialogParent = this;
        dialogBox.answers = entries[index].answers;
        dialogBox.getTextInput = entries[index].getTextInput;
        dialogBox.inputType = entries[index].inputType;

        if (isAutoType) dialogBox.isAuto = true;
    }

    //Kill the current dialog box
    public void KillBox()
    {

        if (entries[index].callback != null)
        {
            entries[index].callback.Invoke();
        }
        dialogBox.Kill();
        Destroy(dialogBox);
        dialogBox = null;
    }
}