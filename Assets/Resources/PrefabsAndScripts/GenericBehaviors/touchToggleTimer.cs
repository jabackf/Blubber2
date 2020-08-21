using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This class makes the trigger object turn-on-able by touching it, then activates a timer to revert it back to it's original state. 
//An example is the double jump arrow. The player collides with it and it goes into the deactivated state for a period of time before reactivating.

public class touchToggleTimer : MonoBehaviour
{
    public bool triggered = false; //Set to true when the player (or whatever is specified) collides with it. Can be checked at start to start off triggered
    public float triggerTime = 4f; //The amount of time after being triggered and before reverting back to normal
    public SpriteRenderer renderer; //This is the renderer that gets the triggered/nontriggered sprites. Leave empty to use this object's renderer.
    private List<string> requireTags = new List<string>() { "Player" }; //These are the tag(s) required of the colliding object in order to trigger it
    public Sprite triggeredSprite, nonTriggeredSprite;
    private float timer=0;
    public GameObject triggerParticles; //If not null, this is instantiated when triggered

    private GameObject triggeringObject; //This tracks the last object that triggered us
    public List<string> sendMessages = new List<string>(); //Messages get sent to the triggering object (probably the Player)
    public UnityEvent[] triggerEvents;  //Called when triggered

    // Start is called before the first frame update
    void Start()
    {
        if (!renderer) renderer = GetComponent<SpriteRenderer>();
        if (triggered) trigger();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
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
            trigger();
        }
    }

    public void trigger()
    {
        if (triggered) return; 

        timer = triggerTime;
        triggered = true;
        renderer.sprite = triggeredSprite;

        if (triggerParticles) Instantiate(triggerParticles, transform.position, Quaternion.identity);

        foreach (UnityEvent e in triggerEvents)
        {
            e.Invoke();
        }

        if (triggeringObject)
        {
            foreach (var m in sendMessages) triggeringObject.SendMessage(m, SendMessageOptions.DontRequireReceiver);
        }
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                renderer.sprite = nonTriggeredSprite;
                triggered = false;
            }
        }
    }
}