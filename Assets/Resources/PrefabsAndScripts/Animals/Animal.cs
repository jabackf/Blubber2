using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public enum states
    {
        idle, walk, graze, speak, flap, followFood
    }

    public states state = states.idle;
    public states[] availableStates = { states.idle, states.walk, states.graze, states.speak, states.followFood };
    public bool active = true; //If false, the cow will sit in default state
    public float foodEatDistance = 1f; //The distance from front left/right transform that food needs to be within to be eaten
    public states eatFoodState = states.graze;

    public bool hasFallingState = false;
    public float fallingVelocityThreshold = -0.5f; //Once vertical velocity goes below this value, falling state is triggered.
    public states fallingState = states.flap;
    bool falling = false;

    states previousState;
    private Animator anim;
    private Rigidbody2D rb;
    private float timer = 0f;
    public float changeTimeMin = 1.3f;
    public float changeTimeMax = 3.4f;
    public int dir = 1; //Facing and walking direction. 1 for left, 0 for right.
    public float walkVelocity = 4f, walkSmoothing = 0.1f;
    private SpriteRenderer renderer;
    public Transform rangeLeft, rangeRight; //If not null, these are used to keep the animal from walking outside of these x axis boundaries
    private float boundXLeft=-1, boundXRight=-1; //Used internally with the rangeLeft and Right
    private GameObject food;
    private float eatTimer = 0;

    public Transform leftTransform, rightTransform; //These represent the immediate left and right ends of the animal. Used to determine when an object is immediately in front or behind of the animal (for example, food)

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();

        if (leftTransform == null) leftTransform = transform;
        if (rightTransform == null) rightTransform = transform;

        if (rangeLeft && rangeRight)
        {
            boundXLeft = rangeLeft.position.x;
            boundXRight = rangeRight.position.x;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasFallingState && rb.velocity.y<fallingVelocityThreshold)
        {
            setState(fallingState);
            falling = true;
            return;
        }
        else
        {
            if (falling)
            {
                falling = false;
                setState(states.idle);
            }
        }

        if (state!=states.followFood && eatTimer<=0) getStateFromAnimator();

        spriteFacingDirection();

        if (active && state == states.followFood && eatTimer<=0)
        {
            if (food.transform.position.x > transform.position.x) dir = 0;
            if (food.transform.position.x < transform.position.x) dir = 1;

            float speed = 0f;
            bool eatFood = false;
            if (dir == 0) //right
            {
                speed = walkVelocity;
                if (Vector3.Distance(rightTransform.position, food.transform.position) < foodEatDistance) eatFood = true;
            }
            if (dir == 1) //left
            {
                speed = -walkVelocity;
                if (Vector3.Distance(leftTransform.position, food.transform.position) < foodEatDistance) eatFood = true;
            }

            if (!eatFood)
            {
                applyWalkingMotion(speed);
            }
            else
            {
                eatTimer = 1f;
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                anim.SetBool("Walking", false);
                
            }
        }



        if (active && eatTimer>0)
        {
            setState(eatFoodState);
            eatTimer -= Time.deltaTime;
            if (eatTimer <= 0)
            {
                Destroy(food);
                resetTimer();
            }

            return;
        }

        if (active && state == states.flap)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                state = states.idle;
                anim.SetBool("Flapping", false);
                resetTimer();
            }
        }
        
        if (active && state == states.walk)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;

                if (boundXLeft != -1 && boundXRight != -1)
                {
                    if (transform.position.x > boundXRight) dir = 1;
                    if (transform.position.x < boundXLeft) dir = 0;
                }

                float speed = 0f;
                if (dir == 0) speed = walkVelocity;
                if (dir == 1) speed = -walkVelocity;
                applyWalkingMotion(speed);
            }
            else
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                state = states.idle;
                anim.SetBool("Walking", false);
                resetTimer();
            }
        }

        if (state==states.idle && active)
        {
            if (timer > 0) timer -= Time.deltaTime;
            else pickRandomState();
        }
    }

    void applyWalkingMotion(float speed)
    {
        anim.SetBool("Walking", true);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")) //Make sure we're actually playing the walk animation
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(speed, rb.velocity.y, 0), walkSmoothing);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (eatTimer > 0) return;
        if (other.gameObject.tag == "Food" && active && state!=states.followFood)
        {
            if (hasState(states.followFood))
            {
                food = other.gameObject;
                setState(states.followFood);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (eatTimer > 0) return;
        if (active && state==states.followFood && other.gameObject == food)
        {
            food = null;
            state = states.idle;
            resetTimer();
            anim.SetBool("Walking", false);
        }
    }


    //Searches available states for the specified state
    bool hasState(states find)
    {
        foreach (states s in availableStates)
            if (s == find) return true;
        return false;
    }

    void getStateFromAnimator()
    {
        state = states.idle;
        if (anim.GetBool("Walking")) state = states.walk;
        if (anim.GetBool("Grazing")) state = states.graze;
        if (anim.GetBool("Speaking")) state = states.speak;
        if (anim.GetBool("Flapping")) state = states.flap;
    }

    void pickRandomState()
    {
        //Food is more important than doing random junk!
        if (state == states.followFood || eatTimer>0) return;

        previousState = state;
        System.Random r = new System.Random();
        bool done = false;
        while (!done)
        {
            
            int num = r.Next(0, availableStates.Length);
            state = availableStates[num];

            //Make sure we selected one that we want
            done = true;
            if (state == states.followFood ) done = false;
        }

        setState(state);
        resetTimer();
    }

    void setState(states s)
    {
        anim.SetBool("Walking", false);
        anim.SetBool("Grazing", false);
        anim.SetBool("Speaking", false);
        anim.SetBool("Flapping", false);

        System.Random r = new System.Random();
        if (s == states.walk)
        {
            state = states.walk;
            dir = r.Next(0, 2);
            anim.SetBool("Walking", true);
        }
        if (s == states.graze)
        {
            state = states.graze;
            anim.SetBool("Grazing", true);
        }
        if (s == states.speak)
        {
            anim.SetBool("Speaking", true);
            state = states.speak;
        }
        if (s == states.flap)
        {
            anim.SetBool("Flapping", true);
            state = states.flap;
        }
        if (s == states.followFood)
        {
            state = states.followFood;
        }
    }

    void resetTimer()
    {
        timer = UnityEngine.Random.Range(changeTimeMin, changeTimeMax);
    }

    void spriteFacingDirection()
    {
        if (dir == 0) renderer.flipX = true;
        else renderer.flipX = false;
    }
}
