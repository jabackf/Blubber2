using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSystem 
{
    [System.Serializable]
    public enum transitions
    {
        DEFAULT, //Default specifies no change to the currently used transition
        none,
        fade
    }
    public transitions transition = transitions.none;

    public string currentMap = "";

    //map can either be a map name (in the format of mapname_xpos_ypos)
    //or one of the special keywords: left, right, up, down, current
    public void goTo(string map, transitions trans = transitions.DEFAULT)
    {
        if (trans!=transitions.DEFAULT) transition = trans;
        string[] m = currentMap.Split('_');
        string transitionString = "";

        switch (map)
        {
            case "left":
                transitionString = m[0] + "_" + (Int32.Parse(m[1]) - 1).ToString() + "_" + m[2];
                break;

            case "right":
                transitionString = m[0] + "_" + (Int32.Parse(m[1]) + 1).ToString() + "_" + m[2];
                break;

            case "up":
                transitionString = m[0] + "_" + m[1] + "_" + (Int32.Parse(m[2]) + 1).ToString();
                break;

            case "down":
                transitionString = m[0] + "_" + m[1] + "_" + (Int32.Parse(m[2]) - 1).ToString();
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
                currentMap = map;
                SceneManager.LoadScene(map, LoadSceneMode.Single);
                break;
        }
    }
}
