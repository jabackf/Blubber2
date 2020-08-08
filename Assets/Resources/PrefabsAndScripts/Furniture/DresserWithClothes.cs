using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DresserWithClothes : OpenClose
{

    public Dialog selectDialog;
    public Transform dialogTop, dialogBottom;
    public string noneString = "Cancel";
    public string nakedString = "Get naked!";

    public bool canChangeColor = true;
    public string changeColorString = "Change color";
    public GameObject colorPicker;

    private sceneSettings sceneSettingsGO;

    public DressObject[] dressList;

    private GameObject characterGo; //Stores the most recent character to have activated the dress selection dialog

    void Awake()
    {
        selectDialog.injectAnswerBranch(0, -1, noneString);
        if (canChangeColor) selectDialog.injectAnswerBranch(0, -1, changeColorString);
        selectDialog.injectAnswerBranch(0, -1, nakedString);
        foreach (DressObject dress in dressList)
        {
            selectDialog.injectAnswerBranch(0, -1, dress.name);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        sceneSettingsGO = GameObject.FindWithTag("SceneSettings").GetComponent<sceneSettings>() as sceneSettings;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public void Open(string name, GameObject characterGo)
    {
        base.Open(name, characterGo);

        this.characterGo = characterGo;
        selectDialog.Initiate(name, characterGo, dialogTop, dialogBottom);
        characterGo.SendMessage("Back", SendMessageOptions.DontRequireReceiver);
    }

    public void Close(string name, GameObject characterGo)
    {

        base.Close(name, characterGo);
    }

    public void Toggle(string name, GameObject characterGo)
    {
        base.Toggle(name, characterGo);

        if (open) Open(name, characterGo);
        else Close(name, characterGo);
    }

    //This is called by our selectDialog using the Dialog.onCompletedCallback. It passes the name of the selected answer.
    public void dressSelected(string answer)
    {
        characterGo.SendMessage("Side", SendMessageOptions.DontRequireReceiver);

        if (canChangeColor && answer == changeColorString)
        {
            //Create a color picker and tell it to send us messages about it's state
            GameObject cpo = Instantiate(colorPicker);
            ColorPicker cp = cpo.GetComponent<ColorPicker>();
            cp.addMessageObject(gameObject);
            BlubberAnimation ba = characterGo.GetComponent<BlubberAnimation>();
            if (ba)
            {
                cp.highlightColor(ba.getColor());
            }
        }
        else
        {
            if (answer == nakedString)
                characterGo.SendMessage("removeNonessentialDresses", SendMessageOptions.DontRequireReceiver);

            DressObject d = Array.Find(dressList, e => e.name == answer);
            if (d)
            {
                characterGo.SendMessage("removeNonessentialDresses", SendMessageOptions.DontRequireReceiver);
                GameObject newDress = Instantiate(d.gameObject, characterGo.transform);
                SpriteRenderer dressRenderer = newDress.GetComponent<SpriteRenderer>() as SpriteRenderer;
                dressRenderer.enabled = true;
                characterGo.SendMessage("addDressObject", newDress, SendMessageOptions.DontRequireReceiver);
                if (sceneSettingsGO != null) sceneSettingsGO.objectCreated(newDress);
            }
            base.Close("", characterGo);
        }

    }

    //This message is sent from the color picker. It is called when we highlight a color with the cursor in the picker.
    public void OnColorHighlighted(Color c)
    {
        characterGo.SendMessage("changeColor", c, SendMessageOptions.DontRequireReceiver);
    }
    //Also sent from the color picker. Called when we choose a color and the picker closes
    public void OnColorSelected(Color c)
    {
        base.Close("", characterGo);
    }
}
