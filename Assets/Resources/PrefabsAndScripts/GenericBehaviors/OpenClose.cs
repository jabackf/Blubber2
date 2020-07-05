using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a generic class for objects that can be opened/closed, on/off, whatever, with an action key and action icon.

public class OpenClose : MonoBehaviour
{
    public bool open = false;
    public Sprite closedSprite, openSprite;
    private SpriteRenderer renderer;

    public bool closedByPlayer = true;        //If false, the open/close action icon will not be available when the object is open.
                                              //So this can be true if you want something that can be freely opened and closed.
                                              //If you want an object that holds the player while they do something (like select a dress) 
                                              //then closes it automatically afterward, set it to false

    // Start is called before the first frame update
    public void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        updateSprite();
    }

    // Update is called once per frame
    public void Update()
    {
        
    }

    public void updateSprite()
    {
        renderer.sprite = (open ? openSprite : closedSprite);
    }

    public void Open(string name, GameObject characterGo)
    {
        open = true;
        updateSprite();
        if (closedByPlayer) gameObject.SendMessage("setRangeActive", true, SendMessageOptions.DontRequireReceiver);
    }

    public void Close(string name, GameObject characterGo)
    {
        gameObject.SendMessage("setRangeActive", true, SendMessageOptions.DontRequireReceiver);
        open = false;
        updateSprite();
    }

    public void Toggle(string name, GameObject characterGo)
    {
        open = !open;
        if (open) Open(name, characterGo);
        else Close(name, characterGo);
        updateSprite();
    }
}
