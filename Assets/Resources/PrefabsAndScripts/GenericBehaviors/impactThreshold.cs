﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.Events;

//This class does thing when the attached object collides with an impact that exceeds the specified value.
//Can be used, for example, to make an object break or to send an "Ouch" command when something hits a character too hard.
//Notes: This won't be triggered with everything unless your rigidbody is set to dynamic. Some collisions won't be detected if you're set to kinematic.

public class impactThreshold : MonoBehaviour
{
    public LayerMask mask;
    public float maxMagnitude=10f;
    public bool destroyOnBreak = true;
    public GameObject particles; //Option particles to create
    public Color particleColor=Color.white;
    public bool breakOnExplode = true; //If we recieve an Explode() message, then break the object
    public bool breakOnShot = true;
    public string sendMessageToOther = ""; //If we hit something and this object breaks, we can send a message to the something that we hit.
    public bool tellOtherAboutThrower = true; //If set to true, we will tell the Other that was hit who threw the object when we send the message (if the information is available)
    public List<AudioClip> playOnExceed = new List<AudioClip>();
	public List<UnityEvent> callbacks = new List<UnityEvent>();

    private GameObject other=null; //Stores the most recent other gameobject from onCollisionEnter

    Global global;

    void OnCollisionEnter2D(Collision2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (other.relativeVelocity.magnitude > maxMagnitude)
            {
                this.other = other.gameObject;
                thresholdExceeded();
            }
        }
    }

    public void Explode(){ if (breakOnExplode) thresholdExceeded();}
    public void IExplode(GameObject go) { if (breakOnExplode) thresholdExceeded(); }
    public void Shot() { if (breakOnShot) thresholdExceeded(); }
    public void IShot(GameObject go) { if (breakOnShot) thresholdExceeded(); }
    public void PShot(Vector3 Pos) { if (breakOnShot) thresholdExceeded(); }

    void thresholdExceeded()
    {
        GameObject p=null;
        if (particles) p = Instantiate(particles, gameObject.transform.position, Quaternion.identity);
        if (p)
        {
            var main = p.GetComponent<ParticleSystem>().main;
            main.startColor = particleColor;
        }

        pickupObject po = GetComponent<pickupObject>();
        GameObject thrower = null;
        if (po)
        {
            thrower = po.getRecentlyThrownBy();
        }

        if (other != null && sendMessageToOther != "")
        {
            if (tellOtherAboutThrower && thrower!=null)
                other.SendMessage("I"+sendMessageToOther, thrower, SendMessageOptions.DontRequireReceiver);
            else
                other.SendMessage(sendMessageToOther, SendMessageOptions.DontRequireReceiver);
        }

		foreach (var c in callbacks) c.Invoke();

        if (!global) global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (playOnExceed.Count>0) global.audio.PlayIfOnScreen(playOnExceed, (Vector2)transform.position, 0.8f, 1.2f);

        if (destroyOnBreak)
            Destroy(gameObject);
    }
}
