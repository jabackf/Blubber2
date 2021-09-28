using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class bouncyBlock : MonoBehaviour
{
    public BoxCollider2D bounceCollider;

    //The amount of vertical velocity to impart to other RB objects that collide. 
    //An alternative is to set this to 0 and use a bouncy material on the bounce collider instead. 
    //This would give you a more natural (but also more varied and less predictable) bounce.
    //The velocity is not imparted to the bouncy block itself.
    //This means that if the bounce block is not anchored to the ground and it is able to move freely, 
    //then not using bouncy material means it won't bounce around when flipped upside down.
    //You could optionally try experimenting with both bouncy material and bounceforce together to achieve more consistent results
    public float bounceForce = 15f;

    public UnityEvent bounceCallback;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (bounceCallback != null)
        {
            foreach (ContactPoint2D contact in col.contacts)
            {
                if (contact.otherCollider == bounceCollider)
                {
                    bounceCallback.Invoke();

                    if (bounceForce!=0)
                    {
                        if (col.rigidbody)
                        {
                            col.rigidbody.AddForce(Vector3.up * bounceForce,ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }
    }
}
