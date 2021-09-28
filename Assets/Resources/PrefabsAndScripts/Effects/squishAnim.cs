using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

//This script plays a simple repeating squishing up and down animation

public class squishAnim : MonoBehaviour
{
    public float timeDown = 0.2f; //The time it takes to squish from top->bottom (if the spring is not set)
    public float timeUp = 0.2f; //The time it takes to squish from bottom->top (if the spring is not set)
    public bool springUp = true; //If false, the animation smoothly squishes up and down. If true, it smoothly squishes down then springs back up.
    public bool springDown = false; //Opposite of above
    public float squish = 1; //Stores the current amount of squish. You can change this to alter where the squishing starts (1=normal)
    public bool goingUp = false; //Whether we are currently squishing up or down
    public float targetTop = 1f; //The amount at full not-squish
    public float targetBottom = 0.3f;  //The amount at full squish
    public bool randomOffset = true;
    public bool squishXAxis = false;
    public bool squishYAxis = true;
    public bool squishZAxis = false;
	
	public float yCap = 0; //If the object's y velocity exceeds this value, then the squishing animation will stop. Set to zero if you want to bypass this option and squish regardless of y velocity.

    private float velocity = 0;

    private RectTransform rt;
    private Transform t;

    // Start is called before the first frame update
    void Start()
    {
        if (randomOffset)
        {
            squish = UnityEngine.Random.Range(targetBottom, targetTop);
            goingUp = UnityEngine.Random.Range(0f, 1f) <= 0.5f ? true : false;
        }
    }
	

    // Update is called once per frame
    void Update()
    {
		if (yCap!=0)
		{
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			if (rb)
			{
				if (Mathf.Abs(rb.velocity.y)>yCap)
				{
					squish = targetTop;
					gameObject.transform.localScale = new Vector3(squishXAxis ? squish : gameObject.transform.localScale.x, squishYAxis ? squish : gameObject.transform.localScale.y, squishZAxis ? squish : gameObject.transform.localScale.z);
					return;
				}
			}
		}
		
        if (!goingUp)
        {
            
            if (!springDown)
            {
                squish = Mathf.SmoothDamp(squish, targetBottom, ref velocity, timeDown);
            }
            else
            {
                squish = targetBottom;
            }

            if ( Math.Abs(squish - targetBottom) < 0.02 ) goingUp = true;
        }
        else
        {
            if (!springUp)
            {
                squish = Mathf.SmoothDamp(squish, targetTop, ref velocity, timeUp);
            }
            else
            {
                squish = targetTop;
            }
            if (Math.Abs(squish - targetTop) < 0.02) goingUp = false;
        }

        gameObject.transform.localScale = new Vector3(squishXAxis ? squish : gameObject.transform.localScale.x, squishYAxis ? squish : gameObject.transform.localScale.y, squishZAxis ? squish : gameObject.transform.localScale.z);


    }
}
