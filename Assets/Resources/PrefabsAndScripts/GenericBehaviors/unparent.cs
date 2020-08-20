using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script sets transform.parent to null with optional timer

public class unparent : MonoBehaviour
{
    public float timer = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("setParentToNull", timer);
    }

    public void setParentToNull()
    {
        transform.parent = null;
    }

}
