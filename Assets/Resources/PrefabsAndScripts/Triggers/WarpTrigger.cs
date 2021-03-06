﻿using System.Collections;
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

    public bool takeCarriedObject = true; //If set to true, the player will take any objects it is carrying with it. If set to false, it will release the objects.

    [Space]
    [Header("Character Repositioning")]
    public MapSystem.repositionTypes repositionType = MapSystem.repositionTypes.wrap;

    //These following variables control the player's positioning as he enters the next scene
    public string warpTag = ""; //This is a tag for a gameObject in the next scene. The character will warp to this tag if one is specified

    public Vector2 wrapOffset = new Vector2(1, 1); //Added or subtracted from the wrap position. (added if wrapping right to left, subtracted if wrapping left to right)
    public Vector2 screenEdgeBuffer = new Vector2(2f, 2f); //In order to wrap, the player must be outside of screenWidth-screenEdgeBuffer or 0+screenEdgeBuffer

    public bool jumpToRelativeX = false; //If not using wrap functions or warptag, you can use the jumpTo values to specify either an absolute position in the new room or a position relative to the previous position.
    public bool jumpToRelativeY = false;  //Uncheck for absolute
    public Vector2 jumpTo = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;

    }

    public void setCallOnTriggerEnter(bool callOnTrigger)
    {
        callOnTriggerEnter = callOnTrigger;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
       
        if (callOnTriggerEnter && !triggered)
        {
            if (global==null ) global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;

            string otherTag = other.gameObject.tag;

            if (otherTag == "Player")
            {
                collidingPlayer = other.gameObject;
                Warp();
               
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (callOnTriggerEnter && !triggered)
        {
            if (global == null) global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;

            string otherTag = other.gameObject.tag;

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
        if (triggered) return;
        triggered = true;


        //Work out player's new position
        if (repositionType==MapSystem.repositionTypes.warpTag)
        {
            global.map.setWarpTag(warpTag);
            global.map.setRepositionType(MapSystem.repositionTypes.warpTag);
        }
        if (repositionType == MapSystem.repositionTypes.wrap)
        {

            global.map.setRepositionType(MapSystem.repositionTypes.wrap);
            global.map.setWrapOffset(wrapOffset);
            global.map.setWrapEdgeBuffer(screenEdgeBuffer);

        }
        if (repositionType == MapSystem.repositionTypes.characterJump)
        {
            Vector2 pos = jumpTo;
            if (jumpToRelativeX) pos.x += collidingPlayer.gameObject.transform.position.x;
            if (jumpToRelativeY) pos.y += collidingPlayer.gameObject.transform.position.y;
            global.map.setPlayerPosition(pos);
            global.map.setRepositionType(MapSystem.repositionTypes.characterJump);
        }

        if (!takeCarriedObject)
        {
            collidingPlayer.GetComponent<CharacterController2D>().dropObject();
        }

        //Now call the function to intiate the scene change
        if (!global.map.goTo(goTo, transType)) triggered = false;
		
		

    }

    
}
