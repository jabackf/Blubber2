﻿using System.Collections;
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

    //map can either be a map name (in the format of mapname_xpos_ypos)
    //or one of the special keywords: left, right, up, down, current
    public void goTo(string map, transitions trans = transitions.DEFAULT)
    {
        if (trans!=transitions.DEFAULT) transition = trans;
        string[] m = currentMap.Split(splitter);
        string transitionString = "";

        switch (map)
        {
            case "left":
                transitionString = m[0] + splitter + (Int32.Parse(m[1]) - 1).ToString() + splitter  + m[2];
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

        
        startTransition(transitionString);
    }

    private void startTransition(string map)
    {
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
        global.sceneChange(map); //This function prepares the global object for a scene change by doing things like handling object persistence
        currentMap = map;
        SceneManager.LoadScene(map, LoadSceneMode.Single);
    }

    //Returns the name of the map (mapName portion of mapName_xpos_ypos). If empty string is passed, returns name of current map.
    public string getMapName(string map= "")
    {
        string[] m = map.Split(splitter);
        return m[0];
    }
}
