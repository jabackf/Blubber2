using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    public Animator anim;
    protected bool active = false;
    protected GameObject pilot;
    protected string previousTag = "";
    protected float horizontalMove=0f, verticalMove = 0f;
    public float hspeed = 10f, vspeed = 10f;
    public float maxHspeed = 15f, maxVspeed = 15f;
    public float movementSmoothing = 0.5f;
    public float activeGravity = 0f; //This is what gravity is set to when we are active.
	
	protected float minimumOffTime = 0.2f; //If we deactivate the copter, then a timer is set to this value. The timer must run out before we can activate again. This effectively prevents scenarios where there copter can be immediately turned back on after deactivating, such as when control is returned to the player and the action key is still held.
    protected float offTimer=0f; //This is the timer used with minimumOffTime
	
	protected float initialGravity;
    protected Rigidbody2D rb;
    protected Vector3 m_Velocity = Vector3.zero; //Reference velocity for smoothdamp

    public bool facingRight = true;
	public bool deactivateOnActionPress=true; //If true, this script watches for the action key when the helicopter is active so it can deactivate. Sometimes you might have another source that deactivates the thing. In that case, you'll set this to true.
    protected SpriteRenderer renderer;
	
	float initialRotation; //This stores the rigidbody's initial rotation.
	public bool resetRotationOnDeactivate=false; //If true, the object's rotation will reset to it's prior value when deactivated.
	protected bool lerpRotationHome = false;

    public AudioClip sndActiveLoop;
    Global global;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (!anim) anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        anim.SetBool("Active", active);
        initialGravity = rb.gravityScale;
        Flip(facingRight);
    }

    // Update is called once per frame
    void Update()
    {
		if (offTimer>0)
		{
			offTimer-=Time.deltaTime;
			if (offTimer<0) offTimer=0;
		}
        if (active)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * hspeed;
            verticalMove = Input.GetAxisRaw("Vertical") * vspeed;
            if (Input.GetButtonDown("UseItemAction") && deactivateOnActionPress) Deactivate(null);
        }
    }

    protected void FixedUpdate()
    {
        if (active)
        {
            Vector3 targetVelocity = new Vector2(Mathf.Clamp(horizontalMove,-maxHspeed,maxHspeed), Mathf.Clamp(verticalMove,-maxVspeed,maxVspeed));
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, movementSmoothing);

            rb.rotation=Mathf.Lerp(rb.rotation, -Mathf.Clamp(horizontalMove * 3f, -45, 45), 0.2f);

            if (horizontalMove > 4) Flip(false);
            if (horizontalMove < -4) Flip(true);
        }
		else
		{
			if (lerpRotationHome && resetRotationOnDeactivate)
			{
				rb.rotation=Mathf.Lerp(rb.rotation, initialRotation, 0.2f);
				if (Mathf.Abs(rb.rotation - initialRotation)<0.1f)
				{
					rb.rotation=initialRotation;
					lerpRotationHome=false;
				}
			}
		}
    }

    //Activates the helicopter. Character is the character gameobject that picked up the remote and activated it.
    public void Activate(GameObject character)
    {
		if (offTimer>0) return;
        pilot = character;
        active = true;
        previousTag = gameObject.tag;
        gameObject.tag = "Player";
        pilot.tag = "inactivePlayer";
        anim.SetBool("Active", active);
        character.SendMessage("onControlTaken", SendMessageOptions.DontRequireReceiver);
        Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
        rb.gravityScale = activeGravity;
		initialRotation = rb.rotation;
        if (sndActiveLoop)
        {
            global.audio.StopFXLoop(sndActiveLoop); //If the motor is winding down from calling pitch drop we want to make sure that's fully stopped first
            global.audio.PlayFXLoop(sndActiveLoop);
        }
    }

	public void Activate()
	{
		if (active) return;
		GameObject go=GameObject.FindWithTag("Player");
		if (go) Activate(go);
	}

    public void Deactivate(GameObject character)
    {
        active = false;
        if (previousTag == "") gameObject.tag="RC";
        else gameObject.tag = previousTag;
        if (pilot)
        {
            pilot.tag = "Player";
            pilot.SendMessage("onControlResumed", SendMessageOptions.DontRequireReceiver);
        }
        anim.SetBool("Active", active);
        Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
        
        rb.gravityScale = initialGravity;
		if (resetRotationOnDeactivate) lerpRotationHome=true;
        if (sndActiveLoop) global.audio.StopFXLoopPitchDrop(sndActiveLoop);
		
		offTimer=minimumOffTime;
    }

    public void Deactivate()
    {
        Deactivate(null);
    }
	
	public void ToggleActivate(GameObject character)
    {
		Debug.Log(active);
        if (active) Deactivate(character);
		else Activate(character);
    }

	public void ToggleActivate()
	{
		if (active) return;
		GameObject go=GameObject.FindWithTag("Player");
		if (go) ToggleActivate(go);
	}
     void OnDestroy()
    {
        if (active) Deactivate(null);
    }

    protected void Flip(bool face_right)
    {
        if (facingRight != face_right)
        {
            facingRight = face_right;
            renderer.flipX = facingRight;
        }
    }
}
