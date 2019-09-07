using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class restrictVelocity : MonoBehaviour
{

    [SerializeField] private float maxVelocity = 25;                              // The maximum velocity that the character is limited to. -1 = none.

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (maxVelocity != -1)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, maxVelocity);
        }
    }
}
