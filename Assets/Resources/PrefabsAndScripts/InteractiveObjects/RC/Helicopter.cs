using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    private Animator anim;
    private bool active = false;
    private GameObject pilot;
    private string previousTag = "";
    private float horizontalMove=0f, verticalMove = 0f;
    public float hspeed = 10f, vspeed = 10f;
    public float maxHspeed = 15f, maxVspeed = 15f;
    public float movementSmoothing = 0.5f;
    public float activeGravity = 0f; //This is what gravity is set to when we are active.
    private float initialGravity;
    private Rigidbody2D rb;
    private Vector3 m_Velocity = Vector3.zero; //Reference velocity for smoothdamp

    public bool facingRight = true;
    private SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        anim.SetBool("Active", active);
        initialGravity = rb.gravityScale;
        Flip(facingRight);
    }

    // Update is called once per frame
    void Update()
    {

        if (active)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * hspeed;
            verticalMove = Input.GetAxisRaw("Vertical") * vspeed;
            if (Input.GetButtonDown("UseItemAction")) Deactivate(null);
        }
    }

    void FixedUpdate()
    {
        if (active)
        {
            Vector3 targetVelocity = new Vector2(Mathf.Clamp(horizontalMove,-maxHspeed,maxHspeed), Mathf.Clamp(verticalMove,-maxVspeed,maxVspeed));
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, movementSmoothing);

            rb.rotation=Mathf.Lerp(rb.rotation, -Mathf.Clamp(horizontalMove * 3f, -45, 45), 0.2f);

            if (horizontalMove > 4) Flip(false);
            if (horizontalMove < -4) Flip(true);
        }
    }

    //Activates the helicopter. Character is the character gameobject that picked up the remote and activated it.
    public void Activate(GameObject character)
    {
        pilot = character;
        active = true;
        previousTag = gameObject.tag;
        gameObject.tag = "Player";
        pilot.tag = "inactivePlayer";
        anim.SetBool("Active", active);
        Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
        rb.gravityScale = activeGravity;
    }

    public void Deactivate(GameObject character)
    {
        active = false;
        if (previousTag == "") gameObject.tag="RC";
        else gameObject.tag = previousTag;
        if (pilot) pilot.tag = "Player";
        anim.SetBool("Active", active);
        Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
        rb.gravityScale = initialGravity;
    }

    public void Deactivate()
    {
        Deactivate(null);
    }

     void OnDestroy()
    {
        if (active) Deactivate(null);
    }

    void Flip(bool face_right)
    {
        if (facingRight != face_right)
        {
            facingRight = face_right;
            renderer.flipX = facingRight;
        }
    }
}
