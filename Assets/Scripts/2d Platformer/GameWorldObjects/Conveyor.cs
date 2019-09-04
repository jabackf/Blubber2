using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed = -2f;
    public bool printDebugInfo = false;
    public bool pauseFor5 = false;
    public bool printTopList = false; 

    private Animator animator=null;
    private string animSpeedMultiplier = "SpeedMultiplier";
    private bool paused = false;
    private float pauseTime = 4f;
    private float speedPrevious;

    public class objEntry
    {
        public GameObject gameObject;
        public Rigidbody2D rb;
        public CharacterController2D characterController;

        public objEntry(GameObject go, Rigidbody2D rb = null)
        {
            this.gameObject = go;
            if (rb == null) this.rb = this.gameObject.GetComponent<Rigidbody2D>();
            else this.rb = rb;
            this.characterController = this.gameObject.GetComponent<CharacterController2D>();
            if (this.characterController != null) this.characterController.setIsOnConveyor(true);
        }
        ~objEntry()
        {
            if (this.characterController != null) this.characterController.setIsOnConveyor(false);
        }
    }

    List<objEntry> objects = new List<objEntry>();

    void Start()
    {
        animator = GetComponent<Animator>() as Animator;
        animator.SetFloat(animSpeedMultiplier, speed);
    }

    public void changeSpeed(float newSpeed)
    {
        speed = newSpeed;
        animator.SetFloat(animSpeedMultiplier, speed);
    }

    //The following method will pause the conveyor for the specified amount of time
    void pause (float time=5f)
    {
        if (printDebugInfo) Debug.Log("Conveyor Paused!");
        if (!paused) speedPrevious = speed;
        changeSpeed(0);
        paused = true;
        pauseTime = time;
    }

    void unPause()
    {
        if (printDebugInfo) Debug.Log("Conveyor UnPaused!");
        paused = false;
        pauseFor5 = false;
        changeSpeed(speedPrevious);
    }

    void OnValidate()
    {
        changeSpeed(speed);

        if (pauseFor5 == true)
        {
            pause();
        }

        if (printTopList)
        {
            Debug.Log(gameObject.name + " TopCount: " + objects.Count());
            foreach (objEntry obj in objects)
            {
                Debug.Log(gameObject.name+" TopItem: "+obj.gameObject.name);
            }
            printTopList = false;
        }
    }

    void FixedUpdate()
    {
        foreach(objEntry obj in objects)
        {
            //obj.rb.AddForce(new Vector2(speed, 0));
            //if (printDebugInfo) Debug.Log(obj.gameObject.name + " < Applied Motion: POS:" + (new Vector2(obj.rb.position.x, obj.rb.position.y)) + "Spd: "+ new Vector2(speed, 0) + "Vel: "+ obj.rb.velocity);
            //if (obj.rb.velocity.y<3)
            obj.rb.MovePosition( new Vector2(obj.rb.position.x+speed* Time.fixedDeltaTime, obj.rb.position.y) + (obj.rb.velocity*Time.fixedDeltaTime) );
            //obj.rb.velocity = new Vector2( (obj.rb.velocity.x+speed) * Time.fixedDeltaTime, obj.rb.velocity.y);

            /*if (obj.characterController != null)
                obj.characterController.Move(speed * Time.fixedDeltaTime, false, false);
            else
                obj.rb.MovePosition(new Vector2(obj.gameObject.transform.position.x + speed * Time.fixedDeltaTime, obj.gameObject.transform.position.y));*/

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

    void OnCollisionEnter2D(Collision2D other)
    {
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (other.gameObject.transform.position.y > gameObject.transform.position.y)
            {
                if (printDebugInfo) Debug.Log(gameObject.name+" > "+other.gameObject.name + " added");
                objects.Add(new objEntry(other.gameObject, rb));
                rb.velocity = new Vector2(0,0);
            }
        }
    }
    void OnCollisionExit2D(Collision2D other)
    {
        if (printDebugInfo) Debug.Log(gameObject.name + " > "+other.gameObject.name + " collision exit");
        objects.RemoveAll(o => o.gameObject == other.gameObject);
    }
}
