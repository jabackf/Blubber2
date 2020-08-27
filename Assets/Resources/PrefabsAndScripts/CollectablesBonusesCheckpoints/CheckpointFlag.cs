using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Requires the triggering object to have a "Player" tag and a CharacterController2D.

public class CheckpointFlag : MonoBehaviour
{
    private Animator anim;
    private bool triggered = false; //Set to true when the player hits the flag.
    public bool triggerWithInactivePlayer = true; //If set to true, the flag can either be triggered with a "Player" tag or an "inactivePlayer" tag
    public Transform spawnPoint; //The point to spawn the character at. If none is specified, the gameObject's position is used.

    public AudioClip sndPlayOnTriggered;

    Global global;

    public GameObject confetti;

    // Start is called before the first frame update
    void Start()
    {

        global = GameObject.FindWithTag("global").GetComponent<Global>();
        anim = GetComponent<Animator>();
        if (!spawnPoint) spawnPoint = gameObject.transform;
    }

   void OnTriggerEnter2D(Collider2D other)
    {

        if ( (other.gameObject.tag == "Player" || (other.gameObject.tag == "inactivePlayer" && triggerWithInactivePlayer)) && !triggered)
        {
            CharacterController2D cont = other.gameObject.GetComponent<CharacterController2D>();
            if (cont!=null)
            {
                triggered = true;
                anim.SetBool("Active", true);
                cont.registerCheckpoint(spawnPoint.position);
                if (sndPlayOnTriggered) global.audio.Play(sndPlayOnTriggered);
                if (confetti) Instantiate(confetti, transform);
            }
        }
    }
}
