using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//An item that can be opened and closed and items can be retrieved.
//Items can spawn on opening or be selected. Items can also be dresses that apply to characters.

public class cabinetWithItems : OpenClose
{
    [System.Serializable]
    public enum types
    {
        dresserWithClothes,
        spawnItemRandom,
        spawnDumpContents,
        spawnItemSelect
    }

    public types type = types.dresserWithClothes;
    public GameObject container; //This is the empty GameObject that contains the inventory that is inside of the cabinet. The inventory is all of the children of this Gameobject.
    public bool inventoryIsInfinite = true; //If true, inventory items will spawn infinitely. If false, items will be removed from the container as they are spawned.
    public Transform spawnTransform; //The transform that has the position where the inventory item will be spawned. If none is specified, then we use the container's transform.

    public applyStartForce inventoryStartForce;

    public Dialog selectDialog;
    public Transform dialogTop, dialogBottom;
    public string noneString = "Cancel";
    public string nakedString = "Get naked!";
    public string cabinetIsEmptyString = "It's empty!";

    public bool canChangeColor = true;
    public string changeColorString = "Change color";
    public GameObject colorPicker;

    private sceneSettings sceneSettingsGO;

    private List<DressObject> dressList = new List<DressObject>();
    private List<GameObject> inventory = new List<GameObject>();
    private List<string> inventoryNames = new List<string>();

    private GameObject characterGo; //Stores the most recent character to have activated the dress selection dialog

    void Awake()
    {

        foreach (Transform child in container.transform)
        {
            DressObject d = child.GetComponent<DressObject>();
            if (d) dressList.Add(d);
            else
            {
                inventory.Add(child.gameObject);
                pickupObject po = child.gameObject.GetComponent<pickupObject>();

                //Try to pull the inventory item's name from a pickupObject script. If none is found, use the GameObject's name.
                bool useGOName = true;
                if (po)
                {
                    if (po.name != "")
                    {
                        inventoryNames.Add(po.name);
                        useGOName = false;
                    }
                }
                if (useGOName) inventoryNames.Add(child.gameObject.name);

                child.gameObject.SetActive(false);
            }
        }

        RebuildSelectMenu();

    }

    public void RebuildSelectMenu()
    {
        selectDialog.emptyAnswerBranch(0);
        if (type == types.dresserWithClothes || type == types.spawnItemSelect)
        {
            selectDialog.injectAnswerBranch(0, -1, noneString);

            if (type == types.dresserWithClothes)
            {
                if (canChangeColor) selectDialog.injectAnswerBranch(0, -1, changeColorString);
                selectDialog.injectAnswerBranch(0, -1, nakedString);

                foreach (DressObject dress in dressList)
                {
                    selectDialog.injectAnswerBranch(0, -1, dress.name);
                }
            }
            else if (type == types.spawnItemSelect)
            {
                foreach (string s in inventoryNames)
                {
                    selectDialog.injectAnswerBranch(0, -1, s);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        sceneSettingsGO = GameObject.FindWithTag("SceneSettings").GetComponent<sceneSettings>() as sceneSettings;
        if (!spawnTransform) spawnTransform = container.transform;
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

        if (type == types.dresserWithClothes)
        {
            selectDialog.Initiate(name, characterGo, dialogTop, dialogBottom);
            characterGo.SendMessage("Back", SendMessageOptions.DontRequireReceiver);
        }

        if (inventory.Count <= 0 && (type == types.spawnItemSelect || type == types.spawnItemRandom || type == types.spawnDumpContents))
        {
            var cont = characterGo.GetComponent<CharacterController2D>();
            characterGo.SendMessage("Side", SendMessageOptions.DontRequireReceiver);
            if (cont) cont.Say(cabinetIsEmptyString, 2f);
            StartCoroutine(TimedClose(characterGo, 1.5f));
        }

        if (type == types.spawnItemSelect && inventory.Count > 0)
        {
            selectDialog.Initiate(name, characterGo, dialogTop, dialogBottom);
            characterGo.SendMessage("Back", SendMessageOptions.DontRequireReceiver);
        }

        if (type == types.spawnItemRandom && inventory.Count > 0)
        {
            instantiateInventoryItem(UnityEngine.Random.Range(0, inventory.Count));
            characterGo.SendMessage("Side", SendMessageOptions.DontRequireReceiver);
            StartCoroutine(TimedClose(characterGo, 1.5f));
        }
        if (type == types.spawnDumpContents && inventory.Count > 0)
        {
            instantiateInventoryItem(-1);
            characterGo.SendMessage("Side", SendMessageOptions.DontRequireReceiver);
            StartCoroutine(TimedClose(characterGo, 1.5f));
        }

    }

    //Waits for the specified time then closes
    public IEnumerator TimedClose(GameObject characterGo, float time)
    {
        yield return new WaitForSeconds(time);
        base.Close("", characterGo);
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
    public void itemSelected(string answer)
    {
        characterGo.SendMessage("Side", SendMessageOptions.DontRequireReceiver);

        if (type == types.dresserWithClothes)
        {
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

                DressObject d = dressList.Find(e => e.name == answer);
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

        if (type == types.spawnItemSelect)
        {
            int index = inventoryNames.IndexOf(answer);
            if (index != -1)
            {
                instantiateInventoryItem(index);
            }
            base.Close("", characterGo);
        }
    }


    //Spawns the item at inventory[index]. Passing -1 tells it to create all of the items.
    void instantiateInventoryItem(int index)
    {
        if (inventoryIsInfinite)
        {
            if (index != -1)
            {
                GameObject newItem = Instantiate(inventory[index], spawnTransform.position, Quaternion.identity);
                newItem.SetActive(true);
                if (sceneSettingsGO != null) sceneSettingsGO.objectCreated(newItem);
                if (inventoryStartForce)
                {
                    var n=CopyComponent<applyStartForce>(inventoryStartForce, newItem);
                    n.enabled = true;
                }
            }
            else
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    GameObject newItem = Instantiate(inventory[index], spawnTransform.position, Quaternion.identity);
                    newItem.SetActive(true);
                    if (sceneSettingsGO != null) sceneSettingsGO.objectCreated(newItem);
                    if (inventoryStartForce)
                    {
                        var n = CopyComponent<applyStartForce>(inventoryStartForce, newItem);
                        n.enabled = true;
                    }
                }
            }

        }
        else
        {
            if (index != -1)
            {
                inventory[index].transform.parent = null;
                inventory[index].SetActive(true);
                if (inventoryStartForce)
                {
                    var n = CopyComponent<applyStartForce>(inventoryStartForce, inventory[index]);
                    n.enabled = true;
                    n.applyForce();
                }
                inventory.RemoveAt(index);
                inventoryNames.RemoveAt(index);
            }
            else
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    inventory[i].transform.parent = null;
                    inventory[i].SetActive(true);
                    if (inventoryStartForce)
                    {
                        var n = CopyComponent<applyStartForce>(inventoryStartForce, inventory[i]);
                        n.enabled = true;
                        n.applyForce();
                    }
                }
                inventoryNames.Clear();
                inventory.Clear();
            }
            RebuildSelectMenu();
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

    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }
}
