using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSystem
{
    public Global global;

    [System.Serializable]
    public enum transitions
    {
        DEFAULT, //Default specifies no change to the currently used transition
        none,
        fade
    }
    public transitions transition = transitions.none;

    public string currentMap = "";

    public const char splitter = '_';

    public bool repositionCharacter = false; //If set to false, the character's position will not be touched when the next scene is loaded.
    private Vector2 CharacterJumpPos = new Vector2(0, 0);
    private string warpTag = "";

    private string getNewMapString(string map)
    {
        string[] m = currentMap.Split(splitter);
        string transitionString = "";

        switch (map)
        {
            case "left":
                transitionString = m[0] + splitter + (Int32.Parse(m[1]) - 1).ToString() + splitter + m[2];
                break;

            case "right":
                transitionString = m[0] + splitter + (Int32.Parse(m[1]) + 1).ToString() + splitter + m[2];
                break;

            case "up":
                transitionString = m[0] + splitter + m[1] + splitter + (Int32.Parse(m[2]) + 1).ToString();
                break;

            case "down":
                transitionString = m[0] + splitter + m[1] + splitter + (Int32.Parse(m[2]) - 1).ToString();
                break;

            case "current":
                transitionString = currentMap;
                break;

            default:
                transitionString = map;
                break;
        }

        return transitionString;
    }

    //If a warptag is specified, the player will jump to the object with this tag on next scene load. The warpTag will then be cleared.
    public void setWarpTag(string warpTag)
    {
        this.warpTag = warpTag;
    }

    //This function sets the new position for the character after the scene loads and if a warp tag isn't used
    public void setPlayerPosition(Vector2 newPosition)
    {
        CharacterJumpPos = newPosition;
    }

    //map can either be a map name (in the format of mapname_xpos_ypos)
    //or one of the special keywords: left, right, up, down, current
    public void goTo(string map, transitions trans = transitions.DEFAULT)
    {
        if (trans!=transitions.DEFAULT) transition = trans;
        string transitionString = getNewMapString(map);
        startTransition(transitionString);
    }

    private void startTransition(string map)
    {
        GameObject player = GameObject.FindWithTag("Player");
        //if (player != null) player.SetActive(false);
        switch (transition)
        {
            case transitions.none:
                sceneLoad(map);
                break;
        }
    }

    //This is the function that actually loads the new scene
    private void sceneLoad(string map)
    {
        Debug.Log("sceneLoad was called");

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) player.GetComponent<CharacterController2D>().sceneChangeStart(map);

        currentMap = map;

        SceneManager.LoadScene(map, LoadSceneMode.Single);

        if (repositionCharacter)
        {
            if (player != null)
            {
                if (warpTag != "")
                {
                    GameObject warpTo = GameObject.FindWithTag(warpTag);
                    player.transform.position = warpTo.transform.position;
                }
                else
                {
                    player.transform.position = CharacterJumpPos;
                }
                //player.SetActive(true);
            }
        }
        this.warpTag = "";

        if (player != null) player.GetComponent<CharacterController2D>().sceneChangeComplete();
    }

    public void setRepositionCharacter(bool reposition)
    {
        this.repositionCharacter = reposition;
    }

    //Returns the name of the map (mapName portion of mapName_xpos_ypos). If empty string is passed, returns name of current map.
    public string getMapName(string map= "")
    {
        string[] m = map.Split(splitter);
        return m[0];
    }
}
