using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Creates a swirling vortex of particles and adds a suction force to any object that gets in it's way.
//It can also be clogged and stop working.

//Checks for the following customTags on the object that it's applying suction to:
//edibleByVacuum (destroys the object when it reaches the end. This one does not require the object to have a rigidbody.)
//vacuumNoEatSound (when edibleByVacuum tag is present and the object is eaten, this tag will prevent it from playing sndEat)
//movePositionByVacuum (Moves the object when it gets into the vortex, regardless of rather it has a rigidbody or not.)
//addForceByVacuum (Moves the object via rigidbody.addforce, regardless of rather the vacuum is configured to use movePosition by default)
//ignoredByVacuum
//swirledByVacuum (Adds rotation.)
//shrunkByVacuum (Shrinks the object to an edible size while it's being sucked.)
//clogsVacuum
//removedFromHolderByVacuum (The vacuum will remove the item from the holder if it's being held)
 

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(getTriggerObjects))]
public class vacuumCleaner : MonoBehaviour {
	
	public bool flipWithParentRenderer = true; //If set to true, the particle system will flip if the parent's sprite renderer flipX property is true.
	SpriteRenderer parentRenderer;
	bool parentFlipX=false; //Stores the most recent state of parent renderer's flip x (only if flipWithParentRenderer is true)
	public bool isClogged = false;
	public string nameOnClogged = "Clogged Vacuum"; //This will change the parent.pickupObject name when isClogged gets set to true. Leave empty for no change.
	string nameBeforeClogged = ""; //Holds the initial name 
	
	public bool dontUseForceToMove = true; //When an object is stuck in a vortex, we can either move it via transform with MovePosition or rigidbody with add force.
										   //When this option is false, we will always try to move objects with MovePosition (though they still require either a Rigidbody, or a relevant customTag like edibleByVacuum or movePositionByVacuum).
										   //When it is true, we will true we will check for a Rigidbody and move the object with that instead (unless the object has movePositionByVacuum)
	
	public bool ignoreCharacters = true; //If true, objects with a CharacterController2D will not be impacted by the vortex.
	
	float initialScaleX; //Used for flipping.
    public float suctionVel = 5;
    public float shapeX = 10;
    public float shapeRad = 1;
	public float innerVortexRadius=0.6f; //This is the "event horizon" of the vortex. It specifies a radius around suctionPosition. It marks the end of the line. Particles stop when they enter this circle, and edible objects are eaten when they hit it.
	public float objectSwirlSpeed = 50f; //If an object gets into the vortex that has the "swirledByVacuum" customTag, then this is how fast we will rotate the object.
	public float objectSuctionForceMultiplier = 50f; //If an object is caught in the vortex and moved with force then this is the value that is multipled by suctionVel to get the force applied.
	public float objectSuctionMoveSpeed = 50f; //If an object is caught in the vortex without a rigidbody and it is moved with moveSpeed instead, this is the speed at which it will move.
	public float objectShrinkSpeed = 20f;
	public float objectScaleTargetHeight=1f; //When object shrinking is used, we will apply shrink to the object until it reaches this y height.
	public GameObject eatObjectParticles; //Instantiated when the vacuum eats an object.
	public Color eatObjectParticleColor = Color.white;
	public psElectricalFailure electricalFailure; //The particle system script that creates sparks and smoke trail.

    ParticleSystem partSys;
    ParticleSystem.MainModule partSysMain;
    ParticleSystem.Particle[] particles;
    ParticleSystem.ShapeModule shape;
	
	Vector3 suctionPosition; //Stores the current center of the vortex.
	public Transform suctionTransform;
	public bool active=false;
	bool prevActive;
	
	getTriggerObjects trigger;
	
	public AudioClip sndEat;
	public AudioClip sndVacuumHum;
	
    Global global;
	

	// Use this for initialization
	void Start () {
		
		global = GameObject.FindWithTag("global").GetComponent<Global>();
		
        // Get the ParticleSystem
        partSys = GetComponent<ParticleSystem>();
        partSysMain = partSys.main;
        shape = partSys.shape;
		prevActive=active;

        particles = new ParticleSystem.Particle[partSysMain.maxParticles];

		initialScaleX = transform.localScale.x;
		if (!active) turnOff();
		else turnOn();
		
		trigger = GetComponent<getTriggerObjects>();
	}
	
	public void turnOn()
	{
		if (isClogged) return;
		active=true;
		partSys.Play();
		
		if (sndVacuumHum)
		{
		    global.audio.StopFXLoop(sndVacuumHum); //If the motor is winding down from calling pitch drop we want to make sure that's fully stopped first
            global.audio.PlayFXLoopPitchRise(sndVacuumHum);
		}
	}
	public void turnOff()
	{
		if (isClogged) return;
		active=false;
		partSys.Stop();
		
		if (sndVacuumHum)
		    global.audio.StopFXLoopPitchDrop(sndVacuumHum);
	}
	
	void FixedUpdate()
	{
		if (!active) return;
		
		if (suctionTransform)
			suctionPosition=suctionTransform.position;
		else 
			suctionPosition = transform.position;

		if (isClogged) return;
		
		foreach (var o in trigger.objects)
		{
			
			customTags tags = o.GetComponent<customTags>();
			bool ignore=false, edible=false, swirl=false, shrink=false, clog=false, removeHolder=false, movePosition=false, useForce=false, eatSound=true;
			if (tags)
			{
				ignore=tags.hasTag("ignoredByVacuum");
				edible=tags.hasTag("edibleByVacuum");
				swirl=tags.hasTag("swirledByVacuum");
				shrink=tags.hasTag("shrunkByVacuum");
				clog=tags.hasTag("clogsVacuum");
				removeHolder=tags.hasTag("removedFromHolderByVacuum");
				useForce=tags.hasTag("addForceByVacuum");
				movePosition=tags.hasTag("movePositionByVacuum");
				eatSound = !tags.hasTag("vacuumNoEatSound");
			}
			
			if (ignoreCharacters)
			{
				if (o.GetComponent<CharacterController2D>()) ignore=true;
			}
			
			if (!ignore)
			{
				if (removeHolder)
				{
					pickupObject po = o.GetComponent<pickupObject>();
					if (po) po.releaseFromHolder();
				}
				
				//Handle movement
				var rb = o.GetComponent<Rigidbody2D>();
				bool moved=false;
				if (rb)
				{
					if (!dontUseForceToMove || useForce)
					{
						var vel = (suctionPosition - o.transform.position).normalized * suctionVel;
						rb.AddForce(vel*objectSuctionForceMultiplier*Time.fixedDeltaTime, ForceMode2D.Impulse);
						moved=true;
					}
				}
				if (!moved)
				{
					if (edible || (rb && dontUseForceToMove) || movePosition)
					{
						o.transform.position= Vector3.MoveTowards(o.transform.position, suctionPosition, objectSuctionMoveSpeed*Time.fixedDeltaTime);
						moved=true;
					}
				}
				
				if (edible)
				{
					if (Vector2.Distance(o.transform.position,suctionPosition)<=innerVortexRadius)
						eatObject(o, clog, eatSound);
				}
				
				if (swirl)
					o.transform.Rotate(new Vector3(0f,0f,objectSwirlSpeed*Time.fixedDeltaTime));
				
				if (shrink)
				{
					var r = o.GetComponent<SpriteRenderer>();
					if (r)
					{
						var factor = objectScaleTargetHeight / r.sprite.bounds.size.y;
						if (o.transform.localScale.y >= factor)
						{
							float deltaSize = objectShrinkSpeed*Time.fixedDeltaTime;
							o.transform.localScale = new Vector2(o.transform.localScale.x-deltaSize, o.transform.localScale.y-deltaSize);
						}
					}
				}
			}
		}
	}
	
	//After the vacuum is clogged, it runs for a second then this function is called to kill it.
	void clogKillVacuum()
	{
		active=false;
		
		if (sndVacuumHum)
		    global.audio.StopFXLoopPitchDrop(sndVacuumHum,0f,4f);
		
		if (nameOnClogged!="")
		{
			pickupObject po = transform.parent.GetComponent<pickupObject>();
			if (po)
			{
				nameBeforeClogged = po.name;
				po.changeName(nameOnClogged);
			}
		}
	}
	
	//Changes the vacuum from clogged to unclogged state.
	public void unclogVacuum()
	{
		if (!isClogged) return;
		
		if (nameOnClogged!="")
		{
			pickupObject po = transform.parent.GetComponent<pickupObject>();
			if (po)
			{
				po.changeName(nameBeforeClogged);
			}
		}
		CancelInvoke("clogKillVacuum"); //Just in case this happens to be in the process of being invoked.
		isClogged=false;
	}
	
	//Changes the vacuum state to clogged.
	public void clogVacuum()
	{
		if (isClogged) return;
		
		if (electricalFailure) electricalFailure.activate(2f);
		if (sndVacuumHum) global.audio.fxPitchGlide(sndVacuumHum,1f,1.2f);
		Invoke("clogKillVacuum",1.5f);
		partSys.Stop();
		isClogged = true;
	}
	
	//This is called when the vacuum eats an object. It is assumed when this is called that the object is within innerVortexRadius around suctionPosition.
	//Clog specifies rather this object should clog the vacuum or not.
	public void eatObject(GameObject o, bool clog=false, bool playEatSound=true)
	{
		if (clog)
			clogVacuum();
		
		GameObject p=null;
        if (eatObjectParticles) p = Instantiate(eatObjectParticles, o.transform.position, Quaternion.identity);
        if (p)
        {
            var main = p.GetComponent<ParticleSystem>().main;
            main.startColor = eatObjectParticleColor;
        }
		
		Destroy(o);
		
		if (sndEat && playEatSound) global.audio.Play(sndEat);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		//if (!active) return;
		if (flipWithParentRenderer)
		{
			if (!parentRenderer)
				parentRenderer = transform.parent.gameObject.GetComponent<SpriteRenderer>();
			if (parentRenderer)
				transform.localScale = new Vector3(initialScaleX * (parentRenderer.flipX ? -1f: 1f), transform.localScale.y, transform.localScale.z);
			
			//Sometimes when the thing flips we get particles in strange places. This is the easiest way that I found to fix it. If we flipped, clear the particle system and start with fresh particles.
			if (parentFlipX!= parentRenderer.flipX)
				partSys.Clear();
			
			parentFlipX = parentRenderer.flipX;
		}
		
		shape.position = Vector2.right * shapeX;
        shape.radius = shapeRad;
		
        int numParts = partSys.GetParticles(particles);

        Vector3 pDelta, vel;
        float uD;
        for (int i=0; i<numParts; i++) 
		{

            if (Vector2.Distance(particles[i].position,suctionPosition)<=innerVortexRadius ) {
                particles[i].velocity=Vector3.zero;
				particles[i].remainingLifetime=0.001f;
                continue;
            }


			
			vel = (suctionPosition - particles[i].position).normalized * suctionVel;

            particles[i].velocity = vel;
			
        }

        partSys.SetParticles(particles, numParts);
		
	}
	
	void OnValidate()
	{
		if (active!=prevActive)
		{
			if (active) turnOn();
			if (!active) turnOff();
			
		}
		prevActive=active;
	}
}
