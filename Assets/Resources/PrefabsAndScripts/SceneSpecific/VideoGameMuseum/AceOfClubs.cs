﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AceOfClubs : MonoBehaviour
{
	bool active=false;	//Set to true while the card is bouncing.
	bool waiting=false; //Set to true after the card has finished it's bounce cycle and been reset to the start, but we're waiting for the dummy cards to be cleared.
	private Vector2 vel;
	float gravity=-0.7f;
	public GameObject museumCurator;
	
	float dummyTime = 0.009f;	//How frequently the card creates a dummy card. The lower the number, the more dummy cards.
	float dummyTimer=0f;
	int dummyCounter=0;
	
	public GameObject dummyCard;
	public Transform floorTransform;	//Used to mark the floor and far left boundaries
	Vector3 boundaries;	//Actually stores the values of the supplied transform at the start.
	Vector3 initialPosition;
	
	SpriteRenderer renderer;
	
	public bool manualTrigger = false; //Set to true to manually trigger the card.
	
    // Start is called before the first frame update
    void Start()
    {
		boundaries = floorTransform.position;
		initialPosition=transform.position;
		renderer=GetComponent<SpriteRenderer>();
    }

	void Update()
	{
		if (active)
		{
			vel.y+=gravity;
			if (transform.position.y<boundaries.y) 
			{
				transform.position=new Vector3(transform.position.x, boundaries.y, transform.position.z);
				vel.y*=-0.8f;
			}
			
			if (dummyTimer<=0)
			{
				dummyCounter+=1;
				GameObject go = GameObject.Instantiate(dummyCard, null);
				go.transform.position = transform.position;
				go.SetActive(true);
				go.GetComponent<SpriteRenderer>().sortingOrder = renderer.sortingOrder+dummyCounter;
				dummyTimer=dummyTime;
				
			}
			dummyTimer-=Time.deltaTime;
			
			if (transform.position.x<boundaries.x)
			{
				dummyTimer=0;
				transform.position = initialPosition;
				vel = new Vector2(0,0);
				active = false;
				waiting=true;
				
			}
			
			transform.position = new Vector3(transform.position.x+vel.x*Time.deltaTime, transform.position.y+vel.y*Time.deltaTime, transform.position.z);

		}
		
		renderer.enabled=!waiting;
	}
	
	public void Activate()
	{
		if (active) return;
		transform.position = initialPosition;
		vel = new Vector2(-5f,14f);
		active = true;
		
		Invoke("notifyCurator",1f);
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
	}
}