using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class React : MonoBehaviour
{
    [System.Serializable]
    public class reaction
    {
        public enum isTalkingResponses
        {
            react, dontReact, interupt
        }

        public isTalkingResponses isTalkingResponse = isTalkingResponses.interupt;

        public string name;
        public List<string> sayStrings;
        public List<UnityEvent> Callbacks;
        public List<string> sendMessages;

        public float sayTime = 3f;

        public reaction(string name)
        {
            this.name = name;
        }
    }

    public reaction rGross = new reaction("Gross"); //Triggered when something gross happens to this character
    public void Gross() { execute(rGross); }

    public reaction rMean = new reaction("Mean"); //Triggered when another character is angry with you
    public void Mean() { execute(rMean); }

    public reaction rOuch = new reaction("Ouch"); //Triggered when something hurts you
    public void Ouch() { execute(rOuch); }

    CharacterController2D cont;

    public void Start()
    {
        cont = GetComponent<CharacterController2D>();
    }

    public void execute(reaction r)
    {

        if (cont)
        {
            if (cont.getIsPaused() || cont.isCharacterDead()) return;
            if (cont.getIsTalking())
            {
                if (r.isTalkingResponse == reaction.isTalkingResponses.dontReact) return;
                if (r.isTalkingResponse == reaction.isTalkingResponses.interupt)
                {
                    gameObject.SendMessage("interupt", r.sayTime + 1, SendMessageOptions.DontRequireReceiver); 
                }
            }
        }

        preExecute(r);

        if (cont)
            cont.Say(r.sayStrings.ToArray(), r.sayTime) ;
        foreach (var c in r.Callbacks) c.Invoke();
        foreach (var m in r.sendMessages) gameObject.SendMessage(m, SendMessageOptions.DontRequireReceiver);
    }

    public virtual void preExecute(reaction r)
    {
        //Specific characters can inherit from this class and use this function to modify reactions before they are executed.
        //For example, a player might have ReactPlayer :: React with override preExecute() { if dress==santaHat and r.name=="Mean") r.sayStrings = "Merry Christmas, you filthy animal!"; }
    }
}
