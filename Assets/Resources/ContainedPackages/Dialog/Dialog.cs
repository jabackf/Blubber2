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
     * RandomSingle picks a random message and shows it once, using player input to initiate and close the dialog box
     * Auto plays the whole dialog through to finish, played automatically with timers.
     * AutoRandom plays random message entries automatically with timers
     * AutoStraightshot plays the whole conversation through from beginning to end automatically with timers.
     * AutoStraightLoop does the same as above, but loops back to the beginning every time it ends
     * AutoSingle plays the selected index automatically with timers.
    */
    
    public enum type {Straightshot, RandomSingle, Auto, AutoRandom, AutoStraightshot, AutoStraightshotLoop, AutoSingle };

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
        public int jumpTo = -1;
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

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initiate(string name, GameObject go, Transform top, Transform bottom)
    {
        initiator = go;
        initiatorName = name;
        initiatorTop = top;
        initiatorBottom = bottom;
        active = true;
        index = 0;
        LoadBox();
    }

    public int getNextIndex()
    {
        if (index < entries.Count - 1)
        {
            if (entries[index].jumpTo == -1 && entries[index].answerBranch.Count == 0) return index+1;
            else
            {
                if (entries[index].jumpTo != -1) return entries[index].jumpTo;

                if (entries[index].answerBranch.Count>0)
                {
                    foreach (AnswerBranch a in entries[index].answerBranch)
                    { 
                        if (a.answer == lastAnswer)
                        {
                            return a.index;
                        }
                    }
                }
            }
        }
        return -1;
    }

    public void Next(string answer = "")
    {
        if (answer != "") lastAnswer = answer;

        if (entries[index].callback != null)
        {
            entries[index].callback.Invoke();
        }

        KillBox();
        if (index < entries.Count-1)
        {
            index = getNextIndex();
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
    }

    public void KillBox()
    {
        dialogBox.Kill();
        Destroy(dialogBox);
    }
}