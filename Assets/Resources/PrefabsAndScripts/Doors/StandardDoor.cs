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


    bool open = false;

    GameObject player=null;
    Global global;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        warp = GetComponent<WarpTrigger>();
        warp.setCallOnTriggerEnter(false);
    }

    //onOpen and onClose should be triggered by an external script (like OpenClose) to tell us when the door has been opened or closed.
    public void onOpen()
    {
        open = true;
    }
    public void onClose()
    {
        open = false;
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
    }

    void OnDestroy()
    {
        if (global)
        {
            if (global.isSceneChanging() && player) player.SendMessage("Side", SendMessageOptions.DontRequireReceiver);
        }
    }
}
