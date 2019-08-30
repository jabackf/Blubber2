using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class bouncyBlock : MonoBehaviour
{
    public BoxCollider2D bounceCollider;
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
                }
            }
        }
    }
}
