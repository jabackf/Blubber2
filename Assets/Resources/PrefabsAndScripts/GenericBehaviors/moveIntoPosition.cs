using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This script moves an object into a specified position. It is very customizable. It can optionally turn off physics and colliders while it is moving,
//remove itself upon reaching destination, play an audio clip on reaching destination, trigger callbacks, etc.
//If the script is not destroyed on arrival, then it will continue to move the object back into position every time it gets out of snapDistance. 
//It will continue triggering the onEmbark features every time it gets out of snapDistance, and it will continue triggering onArrival features every time it arrives at it's destination.

public class moveIntoPosition : MonoBehaviour
{
	public bool active = true;
	public Vector3 targetPosition = Vector3.zero;
	public float speed = 0.08f;
	public float snapDistance=0.01f;
	
	[Space]
    [Header("Do On Embark")]
	public bool turnOffPhysicsOnEmbark=true;
	public bool turnOffCollidersOnEmbark=true; //Turns off any colliders that are turn on.
	public List<UnityEvent> embarkCallbacks = new List<UnityEvent>();
	
	[Space]
    [Header("Do On Arrival")]
	public float arrivalDelayTimer=0f; //If greater than zero, then a timer will be used. When the object reaches it's destination the sound will be played, arrivalCallbacks will be invoked and setVelocityToZero will be used. Then the timer will be set. Once the timer is complete, turnOnPhysics and colliders will activate, along with selfDestructScript.
	public bool turnOnPhysicsOnArrival=true;
	public bool turnOnCollidersOnArrival=true; //Turns on any colliders that were turned off on embark. This does not touch colliders which were already off before the script started doing it's thing.
	public bool setVelocityToZero = true; //If true and the object has a rigidbody, velocity will be set to zero upon ariving.
	public AudioClip arrivalSound;
	public bool selfDestructScript = true; //If true, the script will remove itself from the gameObject.
	public List<UnityEvent> arrivalCallbacks = new List<UnityEvent>();
	
	Global global;
	bool snapped=false;
	List<Collider2D> colliders = new List<Collider2D>();
	
	public void Start()
	{
		global = GameObject.FindWithTag("global").GetComponent<Global>();
		
		embark();
	}
	public void Update()
	{
		if (!active) return;
		
		if (Vector2.Distance(transform.position,targetPosition)>snapDistance)
		{
			if (snapped==true)
			{
				snapped=false;
				embark();
			}
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.08f);
		}
		else
		{
			if (snapped==false)
			{
				snapped=true;
				transform.position=targetPosition;
				if (arrivalSound) global.audio.Play(arrivalSound);
				foreach (var c in arrivalCallbacks) c.Invoke();
				if (setVelocityToZero)
				{
					Rigidbody2D rb = GetComponent<Rigidbody2D>();
					if (rb) rb.velocity = Vector3.zero;
				}
				Invoke("finishArrival",arrivalDelayTimer);
			}
		}
	}
	
	private void finishArrival()
	{
		if (turnOnPhysicsOnArrival)
		{
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			if (rb) rb.simulated=true;
		}
		
		if (turnOnCollidersOnArrival)
		{
			foreach (var c in colliders) c.enabled=true;
		}
		if (selfDestructScript) Destroy(this);
	}
	
	private void embark()
	{
		if (!active) return;
		if (turnOffPhysicsOnEmbark)
		{
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			if (rb) rb.simulated=false;
		}
		
		if (turnOffCollidersOnEmbark)
		{
			colliders.Clear();
			foreach(Collider2D c in GetComponents<Collider2D> ()) 
			{
				if (c.enabled)
				{
					c.enabled = false;
					colliders.Add(c);
				}
			}
		}
		
		foreach (var c in embarkCallbacks) c.Invoke();
	}
}
