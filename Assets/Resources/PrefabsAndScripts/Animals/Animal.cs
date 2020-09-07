using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    [System.Serializable]
    public class createObject
    {
        public GameObject obj; //The object to create
        public int max = -1; //The maximum number of this object that this animal can create. -1 for infinite.
        public states createState = states.speak; // This is the state we will switch into while creating the object. Basically, what animation would you like to play while we are creating the thing.
        public bool createAtRearTransform = true; //If true then the created object(s) will be spawned at the animal's crapper. If false, it will be spawned by their face.
        public float createTimer = 0.4f; //This is how long we wait before starting the createState animation and actually spawining the object.
    }


    public enum states
    {
        idle, walk, graze, speak, flap, followFood, makeSomething
    }

    public states state = states.idle;
    public states[] availableStates = { states.idle, states.walk, states.graze, states.speak, states.followFood }; //These are the states which will be randomly selected from. Some can be responses to things in the environment, like follow food, and will only be triggered if they appear in this list.
    public bool active = true; //If false, the cow will sit in default state
    private bool distressed = false; //If distressed, we will stay in the state marked as distressed until distressed is false. Can be toggled on and off with setDistressed(bool) and toggleDistressed()
    public states distressedState = states.flap;
    public Transform leftTransform, rightTransform; //These represent the immediate left and right ends of the animal. Used to determine when an object is immediately in front or behind of the animal (for example, food)
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
    private float boundXLeft = -1, boundXRight = -1; //Used internally with the rangeLeft and Right
    private sceneSettings sceneSettingsGO;
    Global global;

    [Space]
    [Header("Sounds")]
    //Note: These sounds will only play if they are within the camera view+camera buffer, or if cameraFollowPlayer does not exist on the camera.
    public List<AudioClip> sndSpeak = new List<AudioClip>();
    public float sndSpeakTimeOffset = 0f;
    public AudioClip sndGraze;
    public AudioClip sndMakeSomething;
    public AudioClip sndFlap;

    [Space]
    [Header("Food")]
    public states eatFoodState = states.graze;
    public float eatFoodRadius = 0.2f; //This is the radius of the circle used to determine if we are close enough to eat the food. Basically, a bigger number means we can eat the food from farther away.
    private GameObject food;
    private float eatTimer = 0;

    //If the collider you are using for food is separate from this game object, then check this to true;
    //You can have two food collider setups. The first is to add a trigger that acts as a food collider to this object and uncheck this option.
    //The second is to add a child object with the food collider and the "animalFoodCollider" script attached and this option checked. This prevents us having to add a trigger to this object which could interfere with things like pickupObject ranges and stuff.
    public bool foodColliderIsSeparate = false; 

    [Space]
    [Header("Falling")]
    public bool hasFallingState = false;
    public float fallingVelocityThreshold = -0.5f; //Once vertical velocity goes below this value, falling state is triggered.
    public states fallingState = states.flap;
    bool falling = false;

    [Space]
    [Header("Creating objects")]
    public List<createObject> createObjects;     //If the makeSomething state is triggered, then an object will be randomly selected from this list of object. For example, this list could contain eggs, poop, and whatever else an animal might create.
    private int createSelection = 0; //This is used internally to select a random object from createObjects and start the animation before actually playing it

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        sceneSettingsGO = GameObject.FindWithTag("SceneSettings").GetComponent<sceneSettings>() as sceneSettings;
        global = GameObject.FindWithTag("global").GetComponent<Global>();

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
        if (!distressed)
        {
            if (hasFallingState && rb.velocity.y < fallingVelocityThreshold)
            {
                if (state != fallingState)
                {
                    setState(fallingState);
                }
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
        }

        if (state!=states.followFood && eatTimer<=0) getStateFromAnimator();

        if (!distressed) spriteFacingDirection(); //We don't want to mess with the facing direction if we are distressed, because distressed is generally called for things like being carried.

        if (active && state == states.followFood && eatTimer<=0)
        {
            if (!food) //Something happened to the food! It don't exist anymore! If it just left the trigger then we should have had the state changed to something other than followFood. I'll bet it got destroyed!
            {
                setState(states.idle);
                resetTimer();
            }
            else
            {
                if (food.transform.position.x > transform.position.x) dir = 0;
                if (food.transform.position.x < transform.position.x) dir = 1;

                float speed = 0f;
                if (dir == 0) //right
                {
                    speed = walkVelocity;
                }
                if (dir == 1) //left
                {
                    speed = -walkVelocity;
                }

                if (!isFoodCloseEnoughToEat())
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
        }



        if (active && eatTimer>0)
        {
            setState(eatFoodState);
            eatTimer -= Time.deltaTime;
            if (eatTimer <= 0)
            {
                //We started the food-eating process. The food doesn't dissappear immediately. We had to wait a second so the animal can stop and bend down. Now that we've waited, let's make sure the food is still close enough.
                if (isFoodCloseEnoughToEat())
                {
                    //Nom nom
                    Destroy(food);
                    resetTimer();
                }
                else
                {
                    //The stupid food is moving! I don't know. We should probably keep chasing the food if it's still nearby. If it has left the trigger though then we'll just have to go back to the idle state and hope our sloppy code does something that looks intelligent.
                    if (food == null) //It's null, so it's left the trigger!
                    {
                        setState(states.idle);
                        resetTimer();
                    }
                    else
                    {
                        setState(states.followFood);
                    }
                }
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

    public void setDistressed(bool on)
    {
        distressed = on;
        if (distressed) setState(distressedState);
        else setState(states.idle);
        active = !distressed;
    }
    public void distressedOff()
    {
        distressed = false;
        setState(states.idle);
        active = !distressed;
    }
    public void distressedTime(float time)
    {
        distressed = true;
        setState(distressedState);
        active = !distressed;
        Invoke("distressedOff", time);
    }

    public void toggleDistressed()
    {
        setDistressed(!distressed);
    }

    bool isFoodCloseEnoughToEat()
    {
        if (food) //food still exists
        {
            Vector3 p = (dir == 0 ? rightTransform.position : leftTransform.position);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(p, eatFoodRadius);
            foreach (var c in colliders) if (c.gameObject == food) return true;
        }

        return false;
    }

    void applyWalkingMotion(float speed)
    {
        anim.SetBool("Walking", true);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")) //Make sure we're actually playing the walk animation
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(speed, rb.velocity.y, 0), walkSmoothing);
    }

    public void triggerEntered(Collider2D other)
    {
        if (eatTimer > 0) return;
        if (other.gameObject.tag == "Food" && active && state != states.followFood)
        {
            if (hasState(states.followFood))
            {
                food = other.gameObject;
                setState(states.followFood);
            }
        }
    }
    public void triggerExited(Collider2D other)
    {
        if (eatTimer > 0) //We're trying to eat it. We don't really want to mess with the states. We do want to set food to null though so we know we can't chase it anymore.
        {
            if (other.gameObject == food) food = null;
            return;
        }
        if (active && state == states.followFood && other.gameObject == food)
        {
            food = null;
            state = states.idle;
            resetTimer();
            anim.SetBool("Walking", false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        triggerEntered(other); //We're making these separate functions so we can call them from "animalFoodCollider" when using a separate food collider
    }

    void OnTriggerExit2D(Collider2D other)
    {
        triggerExited(other);
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
        if (s == states.idle)
        {
            state = states.idle;
        }
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
            if (sndGraze) global.audio.PlayIfOnScreen(sndGraze, (Vector2)transform.position);
        }
        if (s == states.speak)
        {
            anim.SetBool("Speaking", true);
            state = states.speak;
            if (sndSpeak.Count > 0)
            {
                Invoke("playSpeakSound", sndSpeakTimeOffset);
                
            }
        }
        if (s == states.flap)
        {
            anim.SetBool("Flapping", true);
            state = states.flap;
            if (sndFlap) global.audio.PlayIfOnScreen(sndFlap, (Vector2)transform.position);
        }
        if (s == states.followFood)
        {
            state = states.followFood;
        }
        if (s == states.makeSomething)
        {
            state = states.idle;
            if (createObjects.Count == 0) return;

            bool found = false;
            int tries = 5; //We'll try to randomly select an object a few times. If we fail every time then we've exceeded the max limit on all the objects we tried. We'll just stop trying to make a thing in that case and go back to our idle state.
            while (tries > 0)
            {
                createSelection = UnityEngine.Random.Range(0, (createObjects.Count - 1));

                int count = -1;
                if (createObjects[createSelection].max > 0)
                    count = countObjectsByName(createObjects[createSelection].obj.name+"(Clone)");

                if (count < createObjects[createSelection].max)
                {
                    Invoke("createSomething", createObjects[createSelection].createTimer);
                    if (sndMakeSomething) global.audio.PlayIfOnScreen(sndMakeSomething, (Vector2)transform.position);
                    found = true;
                    tries = -1;
                }
                else tries -= 1;

            }
            if (found)
            {
                if (createObjects[createSelection].createState != states.makeSomething) setState(createObjects[createSelection].createState); //We don't want recursion because that would be bad!
            }
        }
    }

    public void playSpeakSound()
    {
        global.audio.PlayIfOnScreen(sndSpeak, (Vector2)transform.position, 0.8f, 1.2f);
    }

    public int countObjectsByName(string name)
    {
        int count = 0;
        foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            //Debug.Log(gameObj.name +" == "+ name);
            if (gameObj.name == name)
            {
                count++;
            }
        }

        return count;
    }

    void createSomething()
    {
        Vector3 p;
        if (createObjects[createSelection].createAtRearTransform) p = (dir == 1 ? rightTransform.position : leftTransform.position);
        else p = (dir == 0 ? rightTransform.position : leftTransform.position);
       
        GameObject go = Instantiate(createObjects[createSelection].obj,p, Quaternion.identity);

        if (sceneSettingsGO != null) sceneSettingsGO.objectCreated(go);
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

    public void FaceLeft()
    {
        renderer.flipX = false;
        dir = 1;
    }
    public void FaceRight()
    {
        renderer.flipX = true;
        dir = 0;
    }
}
