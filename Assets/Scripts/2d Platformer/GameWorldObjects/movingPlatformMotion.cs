using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movingPlatformMotion : MonoBehaviour
{

    public float moveSpeed = 3f;
    public Vector2 moveDirection = new Vector2(1,0);
    public float stopTime = 4f; //The amount of time to stop when hitting a trigger
    private GameObject lastTrigger = null;
    private bool isStopped = false;
    private float timer = 0f;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
       rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isStopped)
        {
            //transform.Translate(moveDirection.x * moveSpeed * Time.fixedDeltaTime, moveDirection.y * moveSpeed * Time.fixedDeltaTime, 0f);
            //rb.MovePosition( (new Vector2(gameObject.transform.position.x, gameObject.transform.position.y)) + (new Vector2(moveDirection.x * moveSpeed * Time.fixedDeltaTime, moveDirection.y * moveSpeed * Time.fixedDeltaTime)));
            rb.velocity = (new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed));

            /*var pos = rb.position;
            float xPosition = gameObject.transform.position.x+ moveDirection.x;
            float yPosition = gameObject.transform.position.y + moveDirection.y;
            pos.x = Mathf.Lerp(pos.x, xPosition, moveSpeed * Time.fixedDeltaTime);
            pos.y = Mathf.Lerp(pos.y, yPosition, moveSpeed * Time.fixedDeltaTime);
            //rb.MovePosition(pos);
            //rb.position = pos;*/
        }
        else
        {
            rb.velocity = new Vector3(0f,0f,0f);
            timer -= Time.deltaTime;
            if (timer <= 0) isStopped = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "triggerDirectional")
        {
            if (lastTrigger != collision.gameObject) //We hit a new trigger
            {
                lastTrigger = collision.gameObject;
                timer = stopTime;
                isStopped = true;
                float fRotation = (lastTrigger.transform.eulerAngles.z-90 ) * Mathf.Deg2Rad;
                float fX = -Mathf.Sin(fRotation);
                float fY = Mathf.Cos(fRotation);
                moveDirection = new Vector2(fX, fY);
            }
        }
    }
}
