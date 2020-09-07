using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private Animator animator;
    public float speed = -2f;
    public bool autoScaleColliders = true;
    public bool pauseFor5 = false;
    public LayerMask mask;
    public bool printTopObjects = false;

    public class obj
    {
        public Rigidbody2D rb;
        public float gravity;
        public float mass;
        public obj(Rigidbody2D rb)
        {
            this.rb = rb;
            this.gravity = rb.gravityScale;
            this.mass = rb.mass;
        }
    }
    private List<obj> objects = new List<obj>(); //A list of objects that are on top

    private string animSpeedMultiplier = "SpeedMultiplier";
    private bool paused = false;
    private float pauseTime = 4f;
    private float speedPrevious;

    private BoxCollider2D trigger, collider;
    private SpriteRenderer renderer;

    void Awake()
    {
        animator = GetComponent<Animator>() as Animator;
        changeSpeed(speed);

        renderer = GetComponent<SpriteRenderer>();

        //Get the colliders. It is assumed that conveyors have two box colliders: the platform collider and the trigger which tells us when we are colliding.
        BoxCollider2D[] colList = transform.GetComponentsInChildren<BoxCollider2D>();
        foreach(var c in colList)
        {
            if (c.isTrigger) trigger = c;
            else collider = c;
        }

        if (autoScaleColliders) setColliderWidth();
    }

    public void setColliderWidth()
    {
        var S = renderer.size;
        //trigger.offset = new Vector2(0, 0);
        //collider.offset = new Vector2(0, 0);
        trigger.size = new Vector2(S.x / transform.lossyScale.x, trigger.size.y);
        collider.size = new Vector2(S.x / transform.lossyScale.x, collider.size.y);
    }

    public void changeSpeed(float newSpeed)
    {
        speed = newSpeed;
        animator.SetFloat(animSpeedMultiplier, speed*1.1f);
    }

    //The following method will pause the conveyor for the specified amount of time
    void pause (float time=5f)
    {
        if (!paused) speedPrevious = speed;
        changeSpeed(0);
        paused = true;
        pauseTime = time;
    }

    void unPause()
    {
        paused = false;
        pauseFor5 = false;
        changeSpeed(speedPrevious);
    }

    void OnValidate()
    {
        if (animator) changeSpeed(speed);

        if (pauseFor5 == true)
        {
            pause(5);
        }

        if (printTopObjects)
        {
            printTopObjects = false;
            foreach (var o in objects) Debug.Log(o.rb.gameObject);
        }
    }

    void FixedUpdate()
    {
        /*var rb = gameObject.GetComponent<Rigidbody2D>();
        rb.position -= new Vector2(speed * Time.fixedDeltaTime, 0);
        rb.MovePosition(new Vector2(rb.position.x + (speed * Time.fixedDeltaTime), rb.position.y));
        */

        
        foreach(var o in objects)
        {
            if (o.rb)
            {
                if (o.rb.velocity.y <= 0 && o.rb.gameObject.transform.position.y > transform.position.y)
                {
                    //Move the position of the object while maintaining the velocity applied by other forces. Also, we want to move the object up just slightly. This prevents some bugs in the physics. If we dont do this, stuff with bouncy materials will shake around because they keep "colliding" with the conveyor.

                    //o.rb.MovePosition(new Vector2(o.rb.position.x + speed * Time.fixedDeltaTime, o.rb.position.y + 0.06f) + (o.rb.velocity * Time.fixedDeltaTime));

                    var temp = new GameObject("conveyorMover");
                    temp.transform.position = o.rb.gameObject.transform.position;
                    var prevp = o.rb.gameObject.transform.parent;
                    //var tempRb = temp.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                    temp.transform.position += new Vector3(speed * Time.fixedDeltaTime, 0f, 0f);
                    //tempRb.isKinematic = true;
                    //o.rb.gameObject.transform.parent = temp.transform;
                    //tempRb.MovePosition(new Vector2(tempRb.position.x + speed * Time.fixedDeltaTime, tempRb.position.y + 0.06f));
                    o.rb.gameObject.transform.parent = prevp;
                    Destroy(temp);


                    //o.rb.velocity = new Vector2(0, 0);

                    //o.rb.AddForce(new Vector2(speed * Time.fixedDeltaTime, 0), ForceMode2D.Force);
                    //o.rb.gameObject.transform.localPosition+= new Vector3(speed * Time.fixedDeltaTime, 0, 0);
                    //o.rb.Translate(new Vector2(o.rb.position.x + speed * Time.fixedDeltaTime, rb.position.y + 0.07f) + (o.rb.velocity * Time.fixedDeltaTime));

                    //o.rb.MovePosition(o.rb.position + o.rb.velocity * Time.fixedDeltaTime);
                    //o.rb.MovePosition(new Vector2(o.rb.position.x + speed * Time.fixedDeltaTime, o.rb.position.y));
                    //o.rb.velocity = new Vector2(0, 0);

                    //o.rb.MovePosition(o.rb.position + o.rb.velocity * Time.fixedDeltaTime);
                    //o.rb.velocity = new Vector2( speed * Time.fixedDeltaTime, 0f);
                    //o.rb.AddRelativeForce(new Vector2(1000 * speed * Time.fixedDeltaTime,0), ForceMode2D.Force);

                    //o.rb.position += new Vector2(speed * Time.fixedDeltaTime,0f);
                    //o.rb.gameObject.transform.position += new Vector3(speed * Time.fixedDeltaTime,0f,0f);


                }
                //Occasionally the player can land "inside" of the conveyor, on the bottom side of it. This breaks the conveyor functioning. Let's make absolutely sure that any object that is resting on the conveyor is actually on top of it
                //if (o.rb.gameObject.transform.position.y < transform.position.y && o.rb.velocity.y == 0)
                //o.rb.MovePosition(rb.position + new Vector2(0f,0.1f) );
            }
        }
        
    }

    public void OnCollisionStay2D(Collision2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            //other.gameObject.transform.position += new Vector3(speed * Time.fixedDeltaTime,0f,0f);
            var rb = other.gameObject.GetComponent<Rigidbody2D>();
            if (rb)
            {
                if (rb.velocity.y <= 0 && rb.gameObject.transform.position.y > transform.position.y && !rb.isKinematic)
                {
                    //rb.MovePosition(new Vector2(rb.position.x + speed * Time.fixedDeltaTime, rb.position.y + 0.07f) + (rb.velocity * Time.fixedDeltaTime));
                    //rb.velocity = new Vector2(speed * Time.fixedDeltaTime, 0f);
                    //rb.MovePosition(new Vector2(rb.position.x + speed * Time.fixedDeltaTime, rb.position.y) + (rb.velocity * Time.fixedDeltaTime));

                    //rb.MovePosition(rb.position + rb.velocity * Time.fixedDeltaTime);
                    //rb.velocity = new Vector2( 100* speed * Time.fixedDeltaTime, 0f);

                    //rb.MovePosition(rb.position+rb.velocity * Time.fixedDeltaTime);
                    //rb.MovePosition(new Vector2(rb.position.x + speed * Time.fixedDeltaTime, rb.position.y));
                    //rb.velocity = new Vector2(0, 0);

                    //rb.position += new Vector2(speed * Time.fixedDeltaTime, 0f);
                }
            }
        }
    }

    void Update()
    {
        if (paused)
        {
            pauseTime -= Time.deltaTime;
            if (pauseTime <= 0)
            {
                unPause();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            var rb = other.gameObject.GetComponent<Rigidbody2D>();
            if (rb)
            {
                var o = new obj(rb);
                objects.Add(o);
                var cont = other.gameObject.GetComponent<CharacterController2D>();
                if (cont)
                    cont.setIsOnConveyor(true);
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        var rb = other.gameObject.GetComponent<Rigidbody2D>();
        if (!rb) return;
        int i = objects.FindIndex(o => o.rb == rb);
        if (i>=0)
        {
            objects.RemoveAt(i);
            var cont = other.gameObject.GetComponent<CharacterController2D>();
            if (cont)
                cont.setIsOnConveyor(false);
        }
    }

}
