using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog : MonoBehaviour
{
    public GameObject dialogBoxPrefab;


    [System.Serializable]
    public class entry
    {
        public bool saidByInitiator = false; //If set to true, the initiator's info will be used for this box
        public GameObject gameObject; 
        public Transform locationTop; //If both transforms are null and saidByInitiator is false, the dialog will appear in the center of the screen
        public Transform locationBottom;
        public int jumpTo = -1;
        public string Message = "";
        public string Title = "";
        public bool getTextInput = false;
        public bool inputNumersOnly = false;
        public string imageResource = "";
        public List<string> answers = new List<string>();
        public List<int> answerBranch = new List<int>();
    }

    [SerializeField] public List<entry> entries;

    public bool active = false;
    private int index = 0;
    private GameObject initiator;
    private string initiatorName;
    private Transform initiatorTop;
    private Transform initiatorBottom;

    private string lastAnswer = "";
    private GameObject dialogBoxObj;
    private DialogBox dialogBox;

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

    public void Next(string answer = "")
    {
        if (answer != "") lastAnswer = answer;
        KillBox();
        if (index < entries.Count-1)
        {
            if(entries[index].jumpTo ==-1 && entries[index].answerBranch.Count==0) index++;
            else
            {
                if (entries[index].jumpTo != -1) index = entries[index].jumpTo;
                if (entries[index].answerBranch.Count > 0)
                {
                    index = entries[index].answerBranch[entries[index].answers.FindIndex(x => x == answer)];
                }
            }
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
        dialogBox.textInputNumbersOnly = entries[index].inputNumersOnly;
    }

    public void KillBox()
    {
        dialogBox.Kill();
        Destroy(dialogBox);
    }
}