using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script alters transform.localScale to emulate classic stretch and squash animations based on velocity.

[RequireComponent(typeof(Rigidbody2D))]
public class stretchAndSquash : MonoBehaviour
{
	[System.Serializable]
    public enum axes //Did you know axes is the plural of axis????
    {
        x,
		y
	}
	public enum behaviors
    {
        stretchPosAndNeg,
		squashPosAndNeg,
		stretchPosSquashNeg,
		squashPosStretchNeg
	}
	public axes axis = axes.y; //This is the axis that the effect impacts
	public behaviors behavior = behaviors.stretchPosSquashNeg;
	public float intensity = 0.02f;
	public float max = 0.1f;
	
	Rigidbody2D rb;
	
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var v = (axis==axes.x ? rb.velocity.x : rb.velocity.y)*intensity;
		
		if (behavior==behaviors.stretchPosAndNeg) v = Mathf.Abs(v);
		if (behavior==behaviors.squashPosAndNeg) v = -Mathf.Abs(v);
		if (behavior==behaviors.squashPosStretchNeg) v *= -1;
		
		if (v>max) v=max;
		if (v<-max) v=-max;
		
		Vector3 newScale = transform.localScale;
		if (axis==axes.x) 
		{
			newScale.x+=v;
			newScale.y-=v;
		}
		if (axis==axes.y)
		{
			newScale.x-=v;
			newScale.y+=v;
		}
		gameObject.transform.localScale = newScale;
    }
}
