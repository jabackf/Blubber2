using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundPlayOnEvent : MonoBehaviour
{
    public AudioClip onStart, onDestroy, onEnable, onDisable, onTriggerEnter, onTriggerExit, onCollisionEnter, onCollisionExit, onManualCall;
    public LayerMask mask;
    public float randomizePitchMin = 1f, randomizePitchMax = 1f;
    Global global;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (onStart) global.audio.Play(onStart, randomizePitchMin, randomizePitchMax);
    }

    void Destroy()
    {
        if (onStart) global.audio.Play(onDestroy, randomizePitchMin, randomizePitchMax);
    }

    public void ManualCall()
    {
        if (onManualCall) global.audio.Play(onManualCall, randomizePitchMin, randomizePitchMax);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onTriggerEnter) global.audio.Play(onTriggerEnter, randomizePitchMin, randomizePitchMax);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onTriggerExit) global.audio.Play(onTriggerExit, randomizePitchMin, randomizePitchMax);
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onCollisionEnter) global.audio.Play(onCollisionEnter, randomizePitchMin, randomizePitchMax);
        }
    }
    void OnCollisionExit2D(Collision2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (onCollisionExit) global.audio.Play(onCollisionExit, randomizePitchMin, randomizePitchMax);
        }
    }

    void OnDisable()
    {
        if (onDisable) global.audio.Play(onDisable, randomizePitchMin, randomizePitchMax);
    }
    void OnEnable()
    {
        if (onEnable) global.audio.Play(onEnable, randomizePitchMin, randomizePitchMax);
    }
}
