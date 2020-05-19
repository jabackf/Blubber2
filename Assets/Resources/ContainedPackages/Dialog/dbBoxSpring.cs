﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class dbBoxSpring : MonoBehaviour
{
    public float fullRadius = 0.3f; //The full springyness (offset from original scale value)
    public float friction = 0.4f; //How quickly we lose springyness
    public float speed = 1200f;  //How fast we bounce back and fourth
    public float startAngle = 0; //Where in the spring cycle we start

    public bool squishXAxis = false;
    public bool squishYAxis = true;
    public bool squishZAxis = false;

    public bool sprung = false;
    public float angle = 0;
    public float radius = 0;

    private float originalX=1, originalY=1, originalZ=1;

    private RectTransform rectTrans;

    void Start()
    {
        rectTrans = gameObject.GetComponent<RectTransform>() as RectTransform;
        originalX = gameObject.transform.localScale.x;
        originalY = gameObject.transform.localScale.y;
    }


    // Update is called once per frame
    void Update()
    {
        if (sprung)
        {
            angle += speed * Time.deltaTime;
            if (angle > 360) angle = 0;
            if (angle < 0) angle = 360;
            radius -= friction * Time.deltaTime;
            if (radius <= 0.01)
            {
                radius = 0;
                angle = startAngle;
                sprung = false;
                gameObject.transform.localScale = new Vector3(originalX, originalY, 1);
                //rectTrans.sizeDelta = new Vector2(originalX, originalY);
            }
            float sx = originalX + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            float sy = originalY + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float sz = originalZ + Mathf.Tan(angle * Mathf.Deg2Rad) * radius;
            gameObject.transform.localScale = new Vector3(squishXAxis ? sx : gameObject.transform.localScale.x, squishYAxis ? sy : gameObject.transform.localScale.y, 1);
        }
    }

    public void spring()
    {
        if (sprung)
        {
            gameObject.transform.localScale = new Vector3(originalX, originalY, 1);
        }
        sprung = true;
        angle = startAngle;
        radius = fullRadius;
        originalX = gameObject.transform.localScale.x;
        originalY = gameObject.transform.localScale.y;
    }


    private void OnValidate()
    {
        if (sprung == true)
        {
            spring();
        }
    }
}