using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Makes an object fall off the screen in specified direction, then destroys it when it's outside of screen boundaries.
//Disables all colliders, so it will fall through stuff. Disables rigidbodies (if it has one) so physics will not effect it.

public class fallOffScreenThenDestroy : MonoBehaviour
{
	bool triggered = false;
	public bool triggerAtStart=false; //If true, we will trigger the card when the script's start function is ran.
	public float triggerDelay=0f; //The amount of time that we wait between calling trigger and starting the fall.
	public Vector2 gravity=new Vector2(0f,-3f); //The force applied when the thing starts falling.
	Vector2 vel = new Vector2(0f,0f);
	public Vector2 screenBuffer = new Vector2(0f,0f); //This buffer is added around the edges of the screen. The object is destroyed when it exceeds screen edge + buffer.
	bool falling=false;
	Global global;
	
    // Start is called before the first frame update
    void Start()
    {
		global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (triggerAtStart) trigger();
    }
	
	public void trigger()
	{
		if (triggered) return;
		triggered=true;
		Invoke("startFalling", triggerDelay);
	}
	
	void startFalling()
	{
		var rb = GetComponent<Rigidbody2D>();
		if (rb) rb.simulated=false;

		foreach(Collider c in GetComponents<Collider> ()) 
			c.enabled = false;
			
		falling=true;
	}

    // Update is called once per frame
    void Update()
    {
        if (triggered && falling)
		{
			vel.x+=gravity.x*Time.deltaTime;
			vel.y+=gravity.y*Time.deltaTime;
			transform.position=new Vector2(transform.position.x+vel.x,transform.position.y+vel.y);
			if (!global.camera_follow_player.insideView (transform, screenBuffer.x, screenBuffer.y))
				Destroy(gameObject);
		}
    }
}
