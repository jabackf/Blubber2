using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

//This class does thing when the attached object collides with an impact that exceeds the specified value.
//Can be used, for example, to make an object break or to send an "Ouch" command when something hits a character too hard.

public class impactThreshold : MonoBehaviour
{
    public LayerMask mask;
    public float maxMagnitude=10f;
    public bool destroyOnBreak = true;
    public GameObject particles; //Option particles to create
    public Color particleColor=Color.white;
    public string sendMessageToOther = ""; //If we hit something and this object breaks, we can send a message to the something that we hit.
    public bool tellOtherAboutThrower = true; //If set to true, we will tell the Other that was hit who threw the object when we send the message (if the information is available)

    private GameObject other=null; //Stores the most recent other gameobject from onCollisionEnter

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

        if (destroyOnBreak)
            Destroy(gameObject);
    }
}
