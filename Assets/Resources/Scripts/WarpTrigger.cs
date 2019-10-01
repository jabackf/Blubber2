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

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && callOnTriggerEnter && !triggered)
        {
            Warp();
        }
    }

    public void Warp()
    {
        if (triggered) return;
        triggered = true;
        global.map.goTo(goTo, transType);
    }
}
