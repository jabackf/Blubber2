using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This can be used to add a generic response to "Shot()" or "PShot(Vector3 position)" messages
//Lets you specify an HP (-1 for infinite) which goes down by one each show. Also gives you the ability to spawn particles at shot position (only on PShot).

public class shotByBullet : MonoBehaviour
{
    public float HP = 5f;
    public float HPSubtractOnShot = 1f; //How much we subtract from HP when we get shot. Set to zero and health will not drain.
    public GameObject particles;
    public Color particleColor = Color.white;

    public List<AudioClip> sndGetShot = new List<AudioClip>(); //The sound that plays when we get shot but our hp hasn't ran out
    public List<AudioClip> sndKillShot = new List<AudioClip>(); //The sound that plays on the shot that takes our hp to zero
    public float sndKillPitchRandomizeMin=1f, sndKillPitchRandomizeMax = 1f, sndShotPitchRandomizeMin = 1f, sndShotPitchRandomizeMax = 1f;

    Global global;

    public void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
    }

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
        HP -= HPSubtractOnShot;

        if (HP <= 0)
        {
            if (sndKillShot.Count > 0) global.audio.RandomSoundEffect(sndKillShot.ToArray(), sndKillPitchRandomizeMin, sndKillPitchRandomizeMax);
            Destroy(gameObject);
        }
        else
        {
            if (sndGetShot.Count > 0) global.audio.RandomSoundEffect(sndGetShot.ToArray(), sndShotPitchRandomizeMin, sndShotPitchRandomizeMax);
        }
    }
}
