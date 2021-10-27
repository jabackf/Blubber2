using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT_Script : MonoBehaviour
{
	public instantiateOnDestroy explosionInstantiationScript;	//This is the instantiateOnDestroy script that creates the explosion. Sometimes we want to destroy the tnt without creating an explosion (like when we hit the trigger), so we disable the script.
	
	private bool destroy=false; //Since destroying the tnt immediately after disabling the explosion doesn't work, we will set this flag and destroy it in the next frame.
	private int count=0;
	
	void Update()
	{
		if (destroy) count++;
		if (count>=3)	Destroy(gameObject);
	}
	
	
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag=="TNT_DestroyTrigger")
        {
			
			if (explosionInstantiationScript!=null) 
			{
				explosionInstantiationScript.active=false;
			}
			Destroy(gameObject);
        }
		
		if (other.gameObject.tag=="Player")
		{
			other.gameObject.SendMessage("die", SendMessageOptions.DontRequireReceiver);
			Destroy(gameObject);
		}
    }
}
