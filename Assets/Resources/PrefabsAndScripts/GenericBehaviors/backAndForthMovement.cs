using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Moves an object back and forth between the starting point and the specified destination transform.
//Once point is reached, the object stops and waits the specified amount of time.

public class backAndForthMovement : MonoBehaviour
{
    public Transform destination;
    public float stopTime = 1.5f;
    public bool stopped = true;
    public bool startAtDestination = false;
    public float moveSpeed = 3.7f;
    private Vector3 initialPosition;
    private bool goingHome = false;
    private float timer = 0;
    private Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = gameObject.transform.position;
        timer = stopTime;
        if (startAtDestination)
        {
            gameObject.transform.position = destination.position;
            goingHome = true;
        }
        target = (goingHome ? initialPosition : destination.position);
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

            if (gameObject.transform.position==target)
            {
                gameObject.transform.position = target;
                stopped = true;
                goingHome = !goingHome;
                timer = stopTime;
                target = (goingHome ? initialPosition : destination.position);
            }
        }
    }
}
