using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class TriggerEvent : UnityEvent<string, GameObject> { }

public class RangeTriggerEvent : actionInRange
{
    [SerializeField]
    public TriggerEvent[] triggerEvents;  //Called when the range is activated

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        
    }

    //This is triggered from a characterController2D when the character is in range and presses the action button
    //Character controller specifically looks for RangeTriggerEvent and recognizes it as a generic action that can be performed, then it calls Activate()
    //The passed variables are the character's name and the gameObject for the character
    //By default, we just go through all of the events specified in the editor and trigger them.
    //You could alternatively inherit from this class and rewrite this Activate function to do whatever you want, giving you customizable range actions.
    public void Activate(string name, GameObject go)
    {
        setRangeActive(false);

        foreach (TriggerEvent e in triggerEvents)
        {
            e.Invoke(name,go);
        }
    }
}
