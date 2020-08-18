using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class Explosion : MonoBehaviour
{
    public LayerMask mask; //Anything on this mask will receive the explosion messages

    private float timer = 0.3f; //The trigger stops working after this time.

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.SendMessage("TriggerShake", SendMessageOptions.DontRequireReceiver);
    }

    void Update()
    {
        timer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (timer <= 0) return;
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            other.gameObject.SendMessage("Explode", SendMessageOptions.DontRequireReceiver);
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            if (rb)
            {
            
                rb.AddExplosionForce(20f, gameObject.transform.position, gameObject.GetComponent<CircleCollider2D>().radius, 5);
            }
        }
    }
}
