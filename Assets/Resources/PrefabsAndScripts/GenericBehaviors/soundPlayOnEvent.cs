using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundPlayOnEvent : MonoBehaviour
{
    public AudioClip onStart, onDestroy, onEnable, onDisable, onTriggerEnter, onTriggerExit, onCollisionEnter, onCollisionExit;
    public LayerMask mask;
    Global global;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (onStart) global.audio.Play(onStart);
    }

    void Destroy()
    {
        if (onStart) global.audio.Play(onDestroy);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onTriggerEnter) global.audio.Play(onTriggerEnter);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onTriggerExit) global.audio.Play(onTriggerExit);
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onCollisionEnter) global.audio.Play(onCollisionEnter);
        }
    }
    void OnCollisionExit2D(Collision2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onCollisionExit) global.audio.Play(onCollisionExit);
        }
    }

    void OnDisable()
    {
        if (onDisable) global.audio.Play(onDisable);
    }
    void OnEnable()
    {
        if (onEnable) global.audio.Play(onEnable);
    }
}
