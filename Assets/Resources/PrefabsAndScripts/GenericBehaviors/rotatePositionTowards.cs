using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotatePositionTowards : MonoBehaviour
{
	//Supply a transform or tag, and this object's position will rotate to track it. The center of the cicle is the start position of the object, and it will rotate at the supplide radius.
	//If the tracking object isn't found, the object reverts to it's start position.
	//An example is the eyes of the pacman ghost painting. The eyes rotate in the ghost's socket, tracking the player.
	
	public Transform followTransform; //The transform to follow. If none is supplied, the followTag will be used to set the transform.
	public string followTag="Player";
	
	public float radius=0.1f;
	public bool invertRotation=false; //If set to true, we will rotate in the opposite direction away from the follow transform.
	
	public bool setOriginToStartPos = true;
	public Vector3 origin; //If none is supplied, the origin will be the start position.
	
	bool snappedToOrigin=false; //If the object doesn't exist, we use this to make sure we only snap to the origin once. That way other scripts are free to move the object should they want to.
	
    // Start is called before the first frame update
    void Start()
    {
        if (setOriginToStartPos) origin=gameObject.transform.position;
		if (followTransform==null && followTag!="")
			followTransform = GameObject.FindWithTag(followTag).transform;
    }

    // Update is called once per frame
    void Update()
    {
		if (followTransform==null && followTag!="")
			followTransform = GameObject.FindWithTag(followTag).transform;
		
		if (followTransform==null)
		{
			if (!snappedToOrigin)
			{
				snappedToOrigin=true;
				transform.position = origin;
			}
		}
		else
		{
			snappedToOrigin=false;
			float angle = Mathf.Atan2(followTransform.position.x-origin.x, followTransform.position.y-origin.y);
			if (invertRotation) angle*=-1f;
			Vector2 pos = new Vector2(0f,0f);
			pos.x = origin.x+Mathf.Sin(angle)*radius;
			pos.y = origin.y+Mathf.Cos(angle)*radius;
			gameObject.transform.position = pos;
		}
    }
}
