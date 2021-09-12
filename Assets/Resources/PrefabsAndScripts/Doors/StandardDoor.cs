using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WarpTrigger))]
public class StandardDoor : MonoBehaviour
{

    WarpTrigger warp;
    public enum types
    {
        standard, crystal, key, button
    }

    public types type = types.standard;
    OpenClose openclose_script;

    bool open = false;

    GameObject player=null;
    Global global;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        warp = GetComponent<WarpTrigger>();
        warp.setCallOnTriggerEnter(false);

        openclose_script = gameObject.GetComponent(typeof(OpenClose)) as OpenClose;
    }

    //onOpen and onClose is sometimes triggered by an external script (like OpenClose) to tell us when the door has been opened or closed.
    public void onOpen()
    {
        if (open) return;
        open = true;

        if (type == types.crystal && openclose_script) //For doors that the play does not manually open and close, we have to tell the openclose script to do it's thing.
            openclose_script.Open();
    }
    public void onClose()
    {
        if (!open) return;
        open = false;

        if (type == types.crystal && openclose_script) //For doors that the play does not manually open and close, we have to tell the openclose script to do it's thing.
            openclose_script.Close();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            player = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            player = null;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("EnterDoor") && player && open && !global.isSceneChanging())
        {
            //We're in the trigger zone, the door is open, and the player is pressing the enter door key.
            player.SendMessage("Back", SendMessageOptions.DontRequireReceiver);
            warp.Warp();
        }

        if (type == types.crystal)
        {
            if (GameObject.FindGameObjectsWithTag("crystal").Length <= 0)
            {
                onOpen();
            }
        }
    }

    void OnDestroy()
    {
        if (global)
        {
            if (global.isSceneChanging() && player) player.SendMessage("Side", SendMessageOptions.DontRequireReceiver);
        }
    }
}
