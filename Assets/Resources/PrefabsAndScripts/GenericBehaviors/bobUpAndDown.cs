using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class bobUpAndDown : MonoBehaviour
{
    // User Inputs
    public bool on = true;
    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    // Position Storage Variables
    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    // Use this for initialization
    void Start()
    {
        // Store the starting position & rotation of the object
        //posOffset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Float up/down with a Sin()
        //tempPos = transform.position;// posOffset;
        if (on) transform.position += new Vector3(0,Mathf.Sin(Mathf.PI * frequency *Time.time ) * amplitude *5f * Time.deltaTime, 0);
    }
}
