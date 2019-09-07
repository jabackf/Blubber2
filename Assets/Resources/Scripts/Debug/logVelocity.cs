using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script prints the rigidbody2d.velocity vector in the console log.

public class logVelocity : MonoBehaviour
{

    // Update is called once per frame
    void FixedUpdate()
    {
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        Debug.Log("X: "+rb.velocity.x + ", Y: " + rb.velocity.y);
    }
}
