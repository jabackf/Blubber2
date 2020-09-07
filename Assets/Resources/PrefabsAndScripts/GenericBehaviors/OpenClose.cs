using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This is a generic class for objects that can be opened/closed, on/off, whatever, with an action key and action icon.

public class OpenClose : MonoBehaviour
{
    public bool open = false;
    public Sprite closedSprite, openSprite;
    private SpriteRenderer renderer;

    public AudioClip sndOpen, sndClose;
    Global global;

    public bool triggerCallbackOnStart = true; //If true, then we will trigger either the onOpen or onClose events in the start of this script to tell other components if the thing is default opened or closed
    public UnityEvent[] onOpenEvents, onCloseEvents;

    public bool closedByPlayer = true;        //If false, the open/close action icon will not be available when the object is open.
                                              //So this can be true if you want something that can be freely opened and closed.
                                              //If you want an object that holds the player while they do something (like select a dress) 
                                              //then closes it automatically afterward, set it to false

    // Start is called before the first frame update
    public void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();

        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        updateSprite();

        if (triggerCallbackOnStart)
        {
            if (open)
            {
                foreach (UnityEvent e in onOpenEvents) e.Invoke();
            }
            else
            {
                foreach (UnityEvent e in onCloseEvents) e.Invoke();
            }
        }
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

        if (sndOpen) global.audio.Play(sndOpen);

        foreach (UnityEvent e in onOpenEvents)
        {
            e.Invoke();
        }
    }

    public void Close(string name, GameObject characterGo)
    {
        gameObject.SendMessage("setRangeActive", true, SendMessageOptions.DontRequireReceiver);
        open = false;
        updateSprite();

        if (sndClose) global.audio.Play(sndClose);

        foreach (UnityEvent e in onCloseEvents)
        {
            e.Invoke();
        }
    }

    public void Toggle(string name, GameObject characterGo)
    {
        open = !open;
        if (open) Open(name, characterGo);
        else Close(name, characterGo);
        updateSprite();
    }
}
