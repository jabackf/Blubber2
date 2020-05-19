using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Apply this script to objects that warp the player from one scene to another
public class WarpTrigger : MonoBehaviour
{
    public MapSystem.transitions transType = MapSystem.transitions.none;
    public string goTo = ""; //Can be map name of the format mapname_xpos_ypos, or special words "up", "down", "left", "right", "current"
    public bool callOnTriggerEnter = true; //Set to true to execute the warp on trigger enter

    private Global global;
    private bool triggered = false; //Set to true when the warp function has been called

    private GameObject collidingPlayer; //The player gameObject that triggered this warp trigger

    //These following variables control the player's positioning as he enters the next scene
    public string warpTag = ""; //This is a tag for a gameObject in the next scene. The character will warp to this tag if one is specified

    public bool wrapX = false;  //If no warptag is specified, these are used for wrapping the character to the other side of the screen when changing scenes
    public bool wrapY = false;
    public Vector2 wrapOffset = new Vector2(0, 0);

    public bool jumpToRelativeX = false; //If not using wrap functions or warptag, you can use the jumpTo values to specify either an absolute position in the new room or a position relative to the previous position.
    public bool jumpToRelativeY = false;  //Uncheck for absolute
    public Vector2 jumpTo = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
       
        if (callOnTriggerEnter && !triggered)
        {
            if (global==null ) global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;

            string otherTag = other.tag;

            if (otherTag == "Player")
            {
                collidingPlayer = other.gameObject;
                Warp();
                
            }
        }
    }

    //The function that changes the scene
    public void Warp()
    {
        Debug.Log("WARP Called for Player");
        if (triggered) return;
        triggered = true;

        //Work out player's new position
        if (warpTag != "") global.map.setWarpTag(warpTag);
        else if (wrapX || wrapY)
        {
            Vector3 vpos = Camera.main.WorldToScreenPoint(collidingPlayer.gameObject.transform.position);

            if (wrapX)
            {
                if (vpos.x <= 0) vpos.x = Camera.main.pixelWidth+wrapOffset.x;
                else if (vpos.x >= Camera.main.pixelWidth) vpos.x = wrapOffset.x;
            }
            if (wrapY)
            {
                if (vpos.y <= 0) vpos.y = Camera.main.pixelHeight+wrapOffset.y;
                else if (vpos.y >= Camera.main.pixelHeight) vpos.y = wrapOffset.y;
            }
            global.map.setPlayerPosition(Camera.main.ScreenToWorldPoint(vpos));
        }
        else
        {
            Vector2 pos = jumpTo;
            if (jumpToRelativeX) pos.x += collidingPlayer.gameObject.transform.position.x;
            if (jumpToRelativeY) pos.y += collidingPlayer.gameObject.transform.position.y;
            global.map.setPlayerPosition(pos);
        }

        //Now call the function to intiate the scene change
        global.map.goTo(goTo, transType);

    }

    
}
