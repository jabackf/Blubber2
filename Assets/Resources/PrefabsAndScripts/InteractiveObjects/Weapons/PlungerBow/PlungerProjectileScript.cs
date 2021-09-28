using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlungerProjectileScript : MonoBehaviour
{
	public Transform plungerHeadCenter;	//This is a transform that marks the center of the plunger's head. It also acts as the spawn transform for the anchor. 
	
	[Range(1, 360)]
	public int maxAngleStickiness = 35; //This is the maxmimum angle allowed between the the plunger and the surface angle. If we exceed this angle then the plunger will not stick.
	
	public float minimumImpulse = 5f; //This is how hard the plunger must hit the other object before it can stick.
	
	public bool rendererFlipX=true, rendererFlipY=true; //If set to true, the plunger will look for a sprite renderer on the surface that it is stuck to. If it finds a renderer, then the plunger's positioning and rotation will adjust when the renderers sprite is flipped.
	
	public bool unstickOtherPlungers=true; //If set to true, then this plunger will unstick other plungers if it collides into one. It will not stick to them.
	
	public bool manuallyUnstick=false;
	
	public AudioClip sndStick, sndUnstick;
    Global global;
	
	public ContactFilter2D filter;
	
	bool isStuck = false;
	GameObject stuckTo;
	Vector3 stuckLocalPosition; //This holds the initial position that we get stuck to. Relative to parent.
	Quaternion stuckLocalRotation;	//Initial rotation when we first get stuck.
	bool initialFlipX=false; //If the object we are stuck to has a sprite renderer, this stores the initial state of the flipX property.
	bool initialFlipY=false;
	SpriteRenderer otherRenderer; //Stores the sprite renderer of the thing we are stuck to, if it has one.
	private Vector2 lastStuckNormal;
	
	private GameObject container; //This container holds our plunger when we are stuck to crap. We use a container because it helps prevent issues with things like inhereting parent's scale and stuff.

	
    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
    }
	
	void Update()
    {
        
		if (isStuck)
		{
			SpriteRenderer otherRenderer = stuckTo.GetComponent<SpriteRenderer>();
			if (otherRenderer)
			{
				transform.localPosition=stuckLocalPosition;
				transform.localRotation=stuckLocalRotation;
				
				if (rendererFlipX)
				{
					if (otherRenderer.flipX!=initialFlipX)
					{
						transform.localPosition=new Vector3(stuckLocalPosition.x*-1f,stuckLocalPosition.y,stuckLocalPosition.z);
						transform.localRotation = new Quaternion(stuckLocalRotation.x*-1f,stuckLocalRotation.y,stuckLocalRotation.z,stuckLocalRotation.w*-1f);
						GetComponent<SpriteRenderer>().flipX=true;
					}
					else
					{
						GetComponent<SpriteRenderer>().flipX=false;
					}
				}
				
				if (rendererFlipY)
				{
					if (otherRenderer.flipY!=initialFlipY)
					{
						transform.localPosition=new Vector3(stuckLocalPosition.x,stuckLocalPosition.y*-1f,stuckLocalPosition.z);
						transform.localRotation = new Quaternion(stuckLocalRotation.x,stuckLocalRotation.y*-1f,stuckLocalRotation.z,stuckLocalRotation.w*-1f);
						GetComponent<SpriteRenderer>().flipY=true;
					}
					else
					{
						GetComponent<SpriteRenderer>().flipY=false;
					}
				}
			}
			
			//We don't want to stick to anything that has the Player tag. It's just too glitchy.
			if (stuckTo.tag=="Player")
			{
				unstick();
			}
		}
    }
	

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (!isStuck)
		{
			float distance=-1f;
			ContactPoint2D cp=other.contacts[0];	//This is the contact point that we will be working with.
			bool cpSelected=false;
			
			foreach (var contact in other.contacts)
			{
				//First we remove points that exceed angle allowance
				if (Vector3.Angle(contact.normal*-1f, transform.right)<=maxAngleStickiness)
				{
					//Then we choose the point that's closest to the center.
					if (distance==-1 || distance>Vector3.Distance(contact.point,plungerHeadCenter.position) )
					{
						if (contact.normalImpulse>=minimumImpulse)
						{
							cp = contact;
							cpSelected=true;
						}
					}
				}
			}
			
			if (cpSelected)
			{	
				stick(cp);
			}
		}
	}
	
	
	void stick(ContactPoint2D cp)
	{
		if (isStuck) return;
		
		if(unstickOtherPlungers)
		{
			PlungerProjectileScript opps = cp.collider.gameObject.GetComponent<PlungerProjectileScript>();
			if (opps)
			{
				opps.unstick();
				unstick();
				return;
			}
		}

		isStuck = true;
		
		if (sndStick) global.audio.Play(sndStick);
		
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		
		stuckTo=cp.collider.gameObject;
		
		container = new GameObject();
		container.name="PlungerContainer";
		container.transform.position = plungerHeadCenter.position;
		container.transform.parent = stuckTo.transform;
		gameObject.transform.parent = container.transform;
		
		lastStuckNormal=cp.normal;
		
		//gameObject.transform.right = cp.normal*-1f;
		float angle = Vector2.Angle(Vector2.right, cp.normal*-1f);
		transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, angle);
		//rb.freezeRotation=true;
		
		rb.bodyType = RigidbodyType2D.Kinematic;
		
		rb.velocity = Vector3.zero;
		rb.angularVelocity = 0f;
		
		//Jump to the nearest contact point. 
		Vector2 pos = cp.collider.ClosestPoint(plungerHeadCenter.position);
		Vector2 local = plungerHeadCenter.position;
		local.x-=transform.position.x;
		local.y-=transform.position.y;
		rb.MovePosition(new Vector3(pos.x-local.x, pos.y-local.y,transform.position.x));
		
		stuckLocalPosition = gameObject.transform.localPosition;
		stuckLocalRotation = gameObject.transform.localRotation;
		
		otherRenderer = stuckTo.GetComponent<SpriteRenderer>();
		if (otherRenderer)
		{
			initialFlipX = otherRenderer.flipX;
			initialFlipY = otherRenderer.flipY;
		}
		
		Debug.DrawRay(cp.point, cp.normal, Color.red, 2f);
		Debug.DrawRay(plungerHeadCenter.position, transform.right, Color.yellow, 2f);
		Debug.DrawRay(cp.point, cp.normal*-1f, Color.green, 2f);
		
		
		
	}
	
	public void Explode()
    {
        unstick();
    }
	
	//Call this function to release the plunger from whatever it is stuck to.
	public void unstick()
	{
		if (!isStuck) return;
		isStuck=false;
		
		if (sndUnstick) global.audio.Play(sndUnstick);
		
		//Note: If we just picked the plunger up then it's possible that the parent may have already been changed by the pickupObject script. If this is the case, we don't want to set it to null. So let's make sure we're still stuck to the stuckto object first.
		if (transform.parent==container.transform)
		{
			transform.parent=null;
		}
		
		Destroy(container);
		
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		rb.freezeRotation=false;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = 0f;
		rb.bodyType = RigidbodyType2D.Dynamic;
		
		Debug.Log(transform.parent);		
		GetComponent<SpriteRenderer>().flipY=false;
		GetComponent<SpriteRenderer>().flipX=false;
	}
	
	void OnValidate()
	{
		if (manuallyUnstick)
		{
			manuallyUnstick=false;
			unstick();
		}
	}
}
