using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogRange : actionInRange
{

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

    //Call to initiated the dialog sequence. Argument0 is the object that started the dialog. 
    //It will be alerted with a StopTalking() message when the dialog has concluded.
    public void Initiate(GameObject go)
    {
        setRangeActive(false);
    }
}
