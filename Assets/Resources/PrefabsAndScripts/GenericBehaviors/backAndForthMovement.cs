using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Moves an object back and forth between the starting point and the specified destination transform.
//Once point is reached, the object stops and waits the specified amount of time.

public class backAndForthMovement : MonoBehaviour
{
    public Transform start;     //If not specified, the object's initial position will be used
    public Transform destination;
    public float stopTime = 1.5f;
    public bool stopped = true;
    public bool startAtDestination = false;
    public float moveSpeed = 3.7f;
    private Vector3 initialPosition;
    private bool goingHome = false;
    private float timer = 0;
    private Vector3 target;
    public bool flipSpriteX = false;
    public bool flipSpriteY = false;
    public bool startFlipped = false;


    // Start is called before the first frame update
    void Start()
    {
        if (!start)
        {
            GameObject goStart = new GameObject("start");
            goStart.transform.position = gameObject.transform.position;
            start = goStart.transform;
        }

        //Let's unparent these guys, because if we use something like scale to flip the sprite then we don't want the start and end points to change.
        destination.transform.parent = null;
        start.transform.parent = null;

        timer = stopTime;
        if (startAtDestination)
        {
            gameObject.transform.position = destination.position;
            goingHome = true;
        }
        target = (goingHome ? initialPosition : destination.position);
        if (startFlipped) gameObject.transform.localScale *= new Vector2(flipSpriteX ? -1 : 1, flipSpriteY ? -1 : 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (stopped)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                stopped = false;
                
            }
        }
        else
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, target, moveSpeed*Time.deltaTime);

            if ( Math.Abs(gameObject.transform.position.x-target.x)<0.01 && Math.Abs(gameObject.transform.position.y - target.y) < 0.01)
            {
                gameObject.transform.position = target;
                stopped = true;
                goingHome = !goingHome;
                timer = stopTime;
                target = (goingHome ? start.position : destination.position);
                gameObject.transform.localScale *= new Vector2(flipSpriteX ? -1 : 1, flipSpriteY ? -1 : 1);
            }
        }
    }


}
