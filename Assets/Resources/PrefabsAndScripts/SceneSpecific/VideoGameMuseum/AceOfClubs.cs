using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AceOfClubs : MonoBehaviour
{
	bool active=false;	//Set to true while the card is bouncing.
	bool waiting=false; //Set to true after the card has finished it's bounce cycle and been reset to the start, but we're waiting for the dummy cards to be cleared.
	public float advanceCardTime=0.09f; //This is how often the card's position is advanced. A dummy card is created with every advancement.
	private Vector2 vel;
	public float gravity=-2f;
	public float bounceMultiplier=-0.7f;
	public Vector2 triggerVelocity = new Vector2(-5f,14f); //The velocity applied when the card is initially triggered.
	public GameObject museumCurator;
	public GameObject respawnParticles; //When the card respawns it can create these particles.
	public AudioClip activateSound;
	Global global;
	
	public string layerWhileInactive="Default";
	public string layerWhileActive="ObjectInFronofCharacter";
	
	List<GameObject> dummyCardList = new List<GameObject>();
	public GameObject dummyCard;
	public Transform floorTransform;	//Used to mark the floor and far left boundaries
	Vector3 boundaries;	//Actually stores the values of the supplied transform at the start.
	Vector3 initialPosition;
	
	SpriteRenderer renderer;
	
	public bool manualTrigger = false; //Set to true to manually trigger the card.
	public bool manualClear = false;
	
	
	springAnim spring; //Used to make the main card spring when it respawns.
	
    // Start is called before the first frame update
    void Start()
    {
		boundaries = floorTransform.position;
		initialPosition=transform.position;
		renderer=GetComponent<SpriteRenderer>();
		spring = GetComponent<springAnim>();
		global = GameObject.FindWithTag("global").GetComponent<Global>();
    }

	void advanceCard()
	{
		if (active)
		{
			vel.y+=gravity;
			
			transform.position = new Vector3(transform.position.x+vel.x, transform.position.y+vel.y, transform.position.z);
			
			if (transform.position.y<boundaries.y) 
			{
				transform.position=new Vector3(transform.position.x, boundaries.y, transform.position.z);
				vel.y*=bounceMultiplier;
			}
			
			if (transform.position.x<boundaries.x)
			{
				CancelInvoke("advanceCard");
				transform.position = initialPosition;
				vel = new Vector2(0,0);
				active = false;
				waiting=true;
			}
			else
			{
				GameObject go = GameObject.Instantiate(dummyCard, null);
				go.transform.position = transform.position;
				go.SetActive(true);
				dummyCardList.Add(go);
				go.GetComponent<SpriteRenderer>().sortingOrder = renderer.sortingOrder+dummyCardList.Count;
			}
		}
	}
	
	void Update()
	{
		renderer.enabled=!waiting;
		if (!waiting)
		{
			if (active) renderer.sortingLayerName = layerWhileActive;
			else renderer.sortingLayerName = layerWhileInactive;
		}
	}
	
	public void Activate()
	{
		if (active || waiting || dummyCardList.Count>0) return;
		transform.position = initialPosition;
		vel = triggerVelocity;
		active = true;
		global.audio.Play(activateSound);
		Invoke("notifyCurator",1f);
		InvokeRepeating("advanceCard",advanceCardTime,advanceCardTime);
	}
	
	//Call to start the process of resetting the cards.
	public void resetCards()
	{
		if (active || !waiting) return;
		int c=0;
		foreach (var d in dummyCardList)
		{
			if (d!=null)
			{
				var fall = d.AddComponent<fallOffScreenThenDestroy>();
				fall.triggerDelay=c*0.018f;
				fall.gravity= new Vector2(0f,-2f);
				fall.screenBuffer= new Vector2(-2f,-2f);
				fall.trigger();
				c++;
			}
		}
		dummyCardList.Clear();
		Invoke("resetCardsStageTwo",1.5f);
	}
	
	//Called by resetCards to complete the process of resetting.
	void resetCardsStageTwo()
	{
		waiting = false;
		active=false;
		transform.position = initialPosition;
		if (spring) spring.spring();
		if (respawnParticles) Instantiate(respawnParticles,transform);
	}

	void notifyCurator()
	{
		museumCurator.SendMessage("CustomA", SendMessageOptions.DontRequireReceiver);
	}
	
	void OnValidate()
	{
		if (manualTrigger)
		{
			manualTrigger=false;
			Activate();
		}
		
		if (manualClear)
		{
			manualClear=false;
			resetCards();
		}
	}
}
