using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breakOnImpact : MonoBehaviour
{
    public LayerMask mask;
    public float maxMagnitude=10f;
    public bool destroyOnBreak = true;
    public GameObject particles;
    public Color particleColor=Color.white;

    void OnCollisionEnter2D(Collision2D other)
    {
        if ((mask & 1 << other.gameObject.layer) == 1 << other.gameObject.layer) //Test it against our collision layer mask
        {
            if (other.relativeVelocity.magnitude > maxMagnitude)
            {
                Break();
            }
        }
    }

    void Break()
    {
        GameObject p=null;
        if (particles) p = Instantiate(particles, gameObject.transform.position, Quaternion.identity);
        if (p)
        {
            var main = p.GetComponent<ParticleSystem>().main;
            main.startColor = particleColor;
        }

        if (destroyOnBreak)
            Destroy(gameObject);
    }
}
