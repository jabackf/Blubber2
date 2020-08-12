using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Invokes the specified callback when an object collides with the trigger for longer than the specified time
//Once a callback has been sent, the object must leave the trigger and re-enter before the timer is reset and the callback is sent again.
//You can restrict the objects that are able to trigger the timer with requireTags as well as set a maximum number of times the callback can be invoked with maxInvokeCount
//You can also optionally send messages or say commands to specified objects along with (or instead of) callbacks
//There is also a list of callbacks, messages, and saystrings that can be send upon leaving the trigger AFTER counting down and invoking

public class onTriggerStayTimer : MonoBehaviour
{
    //This is a list of tags that the triggering object needs to require. Leave blank to use any object.
    public List<string> requireTags = new List<string>() { "Player" };
    public int maxInvokeCount = -1; //The maximum number of times we are allowed to count to zero then invoke the callback. Set to -1 for infinite.
    public float time = 3f; //The amount of time we need to stay in the trigger to invoke the callback
    private float timer = 0f; //This is the timer that is actually used for counting.
    public GameObject sendMessageObject = null; //Leave null to use this gameObject
    public GameObject sayObject = null; //Leave null to use this gameObject (requires CharacterController2D)
    public float sayTime = 5f;

    public List<UnityEvent> Callbacks = new List<UnityEvent>();
    public List<string> sendMessages = new List<string>();
    public List<string> sayStrings = new List<string>();

    public List<UnityEvent> TriggerLeaveCallbacks = new List<UnityEvent>();
    public List<string> TriggerLeaveSendMessages = new List<string>();
    public List<string> TriggerLeaveSayStrings = new List<string>();

    private bool invoked = false; //This is set to true AFTER the countdown has hit zero and the invoke is sent, but BEFORE the triggering object has left the trigger. All other times it is false.
    private GameObject triggeringObject = null;
    private int invokeCount = 0; //This stores the number of times we count to zero then invoke the callback.

    void Start()
    {
        if (sendMessageObject == null) sendMessageObject = gameObject;
        if (sayObject == null) sayObject = gameObject;
    }

    void Update()
    {
        if (triggeringObject!=null && timer>0)
        {
            timer -= Time.deltaTime;
            if (timer<=0)
            {
                invoked = true;
                invokeCount += 1;
                if (Callbacks.Count > 0) { foreach (var c in Callbacks) c.Invoke(); }
                if (sendMessages.Count > 0) { foreach (var m in sendMessages) sendMessageObject.SendMessage(m, SendMessageOptions.DontRequireReceiver); }
                if (sayStrings.Count>0)
                {
                    CharacterController2D cont = sayObject.GetComponent<CharacterController2D>();
                    if (cont)
                    {
                        foreach (var s in sayStrings) cont.Say(sayStrings.ToArray(), sayTime);
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggeringObject != null) return; //We're already triggered

        if (maxInvokeCount == -1 || invokeCount < maxInvokeCount)
        {
            bool goodToGo = true;
            if (requireTags.Count > 0)
            {
                goodToGo = false;
                foreach (var t in requireTags)
                {
                    if (t == other.gameObject.tag) goodToGo = true;
                }
            }
            if (goodToGo)
            {
                triggeringObject = other.gameObject;
                timer = time;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (triggeringObject == other.gameObject)
        {
            triggeringObject = null;
            if (invoked)
            {
                if (TriggerLeaveCallbacks.Count > 0) { foreach (var c in TriggerLeaveCallbacks) c.Invoke(); }
                if (TriggerLeaveSendMessages.Count > 0) { foreach (var m in TriggerLeaveSendMessages) sendMessageObject.SendMessage(m, SendMessageOptions.DontRequireReceiver); }
                if (TriggerLeaveSayStrings.Count > 0)
                {
                    CharacterController2D cont = sayObject.GetComponent<CharacterController2D>();
                    if (cont)
                    {
                        foreach (var s in TriggerLeaveSayStrings) cont.Say(TriggerLeaveSayStrings.ToArray(), sayTime);
                    }
                }
            }
            invoked = false;
        }
    }

    //Returns the number of times we've counted to zero and invoked the callback(s)
    public int getInvokeCount()
    {
        return invokeCount;
    }

    //This function resets the invoke counter
    public void resetInvokeCount()
    {
        invokeCount = 0;
    }
}

