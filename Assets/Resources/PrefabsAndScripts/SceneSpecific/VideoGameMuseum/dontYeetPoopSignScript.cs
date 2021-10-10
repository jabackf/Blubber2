using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dontYeetPoopSignScript : MonoBehaviour
{
	GameObject sign;
	public AudioClip hangSound;
	Global global;
	public void Start()
	{
		global = GameObject.FindWithTag("global").GetComponent<Global>();
	}
	public void Update()
	{
		if (sign!=null)
		{
			sign.transform.position = Vector3.MoveTowards(sign.transform.position, transform.position, 0.08f);
		}
	}
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.name=="dontYeetPoopSign")
		{
			global.audio.Play(hangSound);
			sign = col.gameObject;
			col.gameObject.GetComponent<Rigidbody2D>().simulated = false;
			Destroy(col.gameObject.GetComponent<pickupObject>());
			
		}
    }
}
