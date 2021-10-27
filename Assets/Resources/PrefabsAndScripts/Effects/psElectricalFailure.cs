using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Creates a sequence of sparks with flashing lights, then creates smoke trail.

public class psElectricalFailure : MonoBehaviour
{
	public GameObject psSparkPrefab, psSmokeTrailPrefab;
	
	public bool trigger=false; //Set to true to trigger the particles (or call activate())
	
	public float smokeTrailTime = 4f; //The amount of time the smoke trail is on the screen.
	
	public float initialDelay=0.1f; //A delay added before the first spark.
	
	GameObject smokeTrail;
	
	int stage=0;
	
	public AudioClip sndSpark;
    Global global;
	
	void Start()
	{
		global = GameObject.FindWithTag("global").GetComponent<Global>();
	}
	
    void triggerParticles()
	{
		if (stage==0)
		{
			var o = Instantiate(psSparkPrefab);
			o.SetActive(true);
			o.transform.position = transform.position;
			o.transform.parent=gameObject.transform;

			if (sndSpark) global.audio.Play(sndSpark,0.8f,1.2f);
			stage=1;
			Invoke("triggerParticles", 0.5f);
			return;
		}
		if (stage==1)
		{
			var o = Instantiate(psSparkPrefab);
			o.SetActive(true);
			o.transform.position = transform.position;
			o.transform.parent=gameObject.transform;

			if (sndSpark) global.audio.Play(sndSpark,0.8f,1.2f);
			stage=2;
			Invoke("triggerParticles", 0.2f);
			return;
		}
		if (stage==2)
		{
			var o = Instantiate(psSparkPrefab);
			o.SetActive(true);
			o.transform.position = transform.position;
			o.transform.parent=gameObject.transform;
		
			if (sndSpark) global.audio.Play(sndSpark,0.8f,1.2f);
			stage=3;
			Invoke("triggerParticles", 0.1f);
			return;
		}
		if (stage==3)
		{
			if (smokeTrail) Destroy(smokeTrail);
			smokeTrail=Instantiate(psSmokeTrailPrefab);
			smokeTrail.SetActive(true);
			smokeTrail.transform.position = transform.position;
			smokeTrail.transform.parent=gameObject.transform;
			
			stage=4;
			Invoke("triggerParticles", smokeTrailTime);
			return;
		}
		if (stage==4)
		{
			if (smokeTrail) Destroy(smokeTrail);
			return;
		}
	}
	
	public void activate(float delay=-1)
	{
		if (delay!=-1) initialDelay=delay;
		stage=0;
		CancelInvoke("triggerParticles");
		Invoke("triggerParticles", initialDelay);
	}
	
	void OnValidate()
	{
		if (trigger)
		{
			trigger=false;
			activate();
		}
	}
}
