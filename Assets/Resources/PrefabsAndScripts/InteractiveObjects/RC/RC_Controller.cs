using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RC_Controller : MonoBehaviour
{
    public string controllerTag = "RC"; //The controller will search for the first object with this tag and send control to it.
    public GameObject device; //This is the device that the RC controller operates. If null or destroyed, we will search for an object with the controllerTag and control that when we try to activate.
    pickupObject po;

    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        po = gameObject.GetComponent<pickupObject>() as pickupObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Note: Most RC vehicles will be setup so the character activates it, then the vehicle itself detects the action keypress to deactivate. This is because the vehicle steals control from the player and the action key no longer works. The exception might be if the remote control is destroyed or the player drops the remote due to death
    public void ToggleActivate()
    {
        if (!device) device = GameObject.FindWithTag(controllerTag);
        else
        {
            if (!device.scene.IsValid())
                device = GameObject.FindWithTag(controllerTag);
        }
        if (device)
        {
            if (device.scene.IsValid())
            {
                GameObject holder = po.getHolder();
                if (holder == null) //You cannot pass null via SendMessage, so we'll just pass an empty GameObject. If we're calling the function without a holder then we probably aren't trying to use the holder anyway.
                {
                    holder = new GameObject();
                }
                if (!active)
                {
                    device.SendMessage("Activate", holder, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    device.SendMessage("Deactivate", holder, SendMessageOptions.DontRequireReceiver);
                }
                active = !active;
            }
        }
    }

    public void Deactivate()
    {
        if (active) ToggleActivate();
    }
    public void Activate()
    {
        if (!active) ToggleActivate();
    }

    void OnDestroy()
    {
        Deactivate();
    }
}
