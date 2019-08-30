using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script simply hides a sprite when the object's start method runs
[RequireComponent(typeof(SpriteRenderer))]
public class hideOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
