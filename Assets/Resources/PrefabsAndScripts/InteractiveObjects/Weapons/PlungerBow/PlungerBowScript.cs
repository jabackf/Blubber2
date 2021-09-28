using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlungerBowScript : MonoBehaviour
{
	
	private Animator anim;
	private pickupObject po;
	
    // Start is called before the first frame update
    void Start()
    {
        anim=GetComponent<Animator>();
		po=GetComponent<pickupObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	private void spawnProjectile()
	{
		po.spawnProjectile();

	}
	
	public void Fire()
	{
		anim.SetTrigger("Shoot");
		Invoke("spawnProjectile",0.28f);
	}
}
