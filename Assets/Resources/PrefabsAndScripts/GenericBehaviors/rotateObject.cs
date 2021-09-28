using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateObject : MonoBehaviour
{
	public float speed=50f;

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0f,0f,speed*Time.deltaTime);
    }
}
