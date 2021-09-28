using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterHat : Helicopter
{
	
	public void Activate(GameObject character)
    {
        rb=character.GetComponent<Rigidbody2D>();
		if (!rb) return;
		base.Activate(character);
	}
	
	public void Activate()
    {
		if (active) return;
		GameObject go=GameObject.FindWithTag("Player");
		if (go) Activate(go);
	}
	
	public void Deactivate(GameObject character)
	{
		pilot.SendMessage( (facingRight ? "FaceRight" : "FaceLeft"), SendMessageOptions.DontRequireReceiver);
		base.Deactivate(character);
	}
	
	public void Deactivate()
	{
		Deactivate(null);
	}
}
