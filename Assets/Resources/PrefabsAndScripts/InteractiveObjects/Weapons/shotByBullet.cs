using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This can be used to add a generic response to "Shot()" or "PShot(Vector3 position)" messages
//Lets you specify an HP (-1 for infinite) which goes down by one each show. Also gives you the ability to spawn particles at shot position (only on PShot).

public class shotByBullet : MonoBehaviour
{
    public float HP = -1f; //-1 for infinite health points
    public GameObject particles;
    public Color particleColor = Color.white;

    public void PShot(Vector3 position)
    {
        GameObject p = null;
        if (particles) p=Instantiate(particles, position, Quaternion.identity);
        if (p)
        {
            var main = p.GetComponent<ParticleSystem>().main;
            main.startColor = particleColor;
        }
        Shot();
    }

    public void Shot()
    {
        if (HP>0)
        {
            HP -= 1f;
            if (HP <= 0) Destroy(gameObject);
        }
    }
}
