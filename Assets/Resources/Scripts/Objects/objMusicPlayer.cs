using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objMusicPlayer : MonoBehaviour
{

    public bool on = false;
    public ParticleSystem particles;

    //Have a list for audio clips, either cycle through them auto or stop after each one.
    //Play them randomly or in order. Optionally turn down main music

    // Start is called before the first frame update
    void Start()
    {
        setParticlesEnabled();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggle()
    {
        on = !on;
        setParticlesEnabled();
    }

    public void toggle(bool on)
    {
        this.on = on;
        setParticlesEnabled();
    }

    private void setParticlesEnabled()
    {
        if (particles)
        {
            var em = particles.emission;
            em.enabled = on;
        }
    }
}
