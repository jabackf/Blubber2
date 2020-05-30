using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script makes the gameObject pace back and forth between two triggers

public class aiBackAndForth : MonoBehaviour
{
    public float moveForce=10;
    public float maxVelocity = 8;
    public float triggerHitRecoilForce = 0.5f;
    public float waitTime;
    public bool facingRight = true;
    public bool turnAtSceneEdge = true;
    public bool waiting = false;  //Rather we are stopped and waiting. If true, the object will start in the waiting state
    public bool flipSprite = true;  //Flip the sprite with localScale
    public bool turnIfBlocked = true; //If something gets in it's path and stops it for a bit, then turn around
    public bool attemptToStandIfTipped = true; //If set to true, the object will attempt to stand up if it's tipped
    public float standTorque = 4f, standForce = 3; //If the object should attempt to stand, these are the forces it will attempt to use for jumping up.
    public string triggerTag = "trigger";

    private sceneBoundary boundary;
    private float timeLeft = 0;
    private float stoppedTimer = -1; //If we run into something and stop, then this timer will see how long we're stopped. If it's too long, something is in the way and we should try turning around.
    private Rigidbody2D rb;
    private GameObject lastTrigger;
    private Vector3 positionPrevious;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        boundary = GameObject.FindWithTag("sceneBoundary").GetComponent<sceneBoundary>() as sceneBoundary;
        positionPrevious = gameObject.transform.position;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (waiting || col.gameObject==lastTrigger) return;
        if (col.tag == triggerTag)
        {
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(new Vector2((facingRight ? -triggerHitRecoilForce : triggerHitRecoilForce), 0), ForceMode2D.Impulse);
            waiting = true;
            timeLeft = waitTime;
            lastTrigger = col.gameObject;
        }
    }

    void FixedUpdate()
    {
        if (waiting)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                waiting = false;
                facingRight = !facingRight;
            }
        }
        else
        {
            if (!isTipped())
            {
                //If we get stuck, wait a second then try turning around
                if (turnIfBlocked)
                {
                    if (delta(positionPrevious.x, gameObject.transform.position.x) < 0.005f && stoppedTimer == -1)
                    {
                        stoppedTimer = waitTime * 1.5f;
                    }
                    if (stoppedTimer > 0)
                    {
                        stoppedTimer -= Time.deltaTime;
                        if (stoppedTimer <= 0)
                        {
                            rb.AddForce(new Vector2((facingRight ? -triggerHitRecoilForce : triggerHitRecoilForce), 0), ForceMode2D.Impulse);
                            facingRight = !facingRight;
                            lastTrigger = null;
                            stoppedTimer = -1;
                        }
                        if (delta(positionPrevious.x, gameObject.transform.position.x) > 0.005f) //We've started moving again
                        {
                            stoppedTimer = -1;
                        }
                    }
                }

                //Here is where we add the force
                if (!(rb.velocity.x < -maxVelocity) && !(rb.velocity.x > maxVelocity))
                    rb.AddForce(new Vector2((facingRight ? moveForce : -moveForce), 0));

                //Handle outside of scene bounds
                if (turnAtSceneEdge && boundary!=null)
                {
                    if (gameObject.transform.position.x>boundary.getRightX() || gameObject.transform.position.x < boundary.getLeftX() )
                    {
                        rb.velocity = new Vector2(0, 0);
                        rb.AddForce(new Vector2((facingRight ? -triggerHitRecoilForce : triggerHitRecoilForce), 0), ForceMode2D.Impulse);
                        waiting = true;
                        timeLeft = waitTime;
                        lastTrigger = null ;
                    }

                }
            }
            else
            {
                if (attemptToStandIfTipped)
                {
                    if (stoppedTimer == -1) stoppedTimer = waitTime*2;
                    stoppedTimer -= Time.deltaTime;
                    if (stoppedTimer <= 0)
                    {
                        rb.AddForce(new Vector2(0, standForce), ForceMode2D.Impulse);
                        rb.AddTorque( (gameObject.transform.eulerAngles.z>200 ? standTorque : -standTorque) , ForceMode2D.Impulse);
                        stoppedTimer = -1;
                    }
                }
            }
        }

        if (flipSprite)
        {
             transform.localScale = new Vector3( (facingRight ? 1 : -1), transform.localScale.y, transform.localScale.z);
        }

        positionPrevious = gameObject.transform.position;
        
    }

    public bool isTipped()
    {

        if (delta(Math.Abs(gameObject.transform.eulerAngles.z), 90) < 30 || delta(Math.Abs(gameObject.transform.eulerAngles.z), 270) < 30) return true;
        else return false;
    }

    public float delta(float a, float b)
    {
        return Math.Abs(a - b);
    }
}
