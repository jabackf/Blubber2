using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlubberAnimation : CharacterAnimation
{
    multiDress eyes;

    bool blink = false;
    float blinkTimerMin = 4f;
    float blinkTimerMax = 8f;
    float blinkTimer = 0f;
    float blinkCloseTime = 0.25f;
    string emotion = "Normal";

    private GameObject particles;
    private string particlesEmotion = "";

    public string particlesLoveResource = "Prefabs/Effects/psHeartParticles.prefab";

    void Start()
    {
        base.Start();
    }

    public override void SetupCharacter()
    {
        blinkTimer = UnityEngine.Random.Range(blinkTimerMin, blinkTimerMax);

        dressList.Add(new dress("eyesNormal", "Sprites/Blubber/Eyes", gameObject.transform));
        dressList.Add(new dress("eyesAngry", "Sprites/Blubber/eyesAngry", gameObject.transform));
        dressList.Add(new dress("eyesBlink", "Sprites/Blubber/eyesBlink", gameObject.transform));
        dressList.Add(new dress("eyesClimb", "Sprites/Blubber/eyesClimb", gameObject.transform));
        dressList.Add(new dress("eyesHalf", "Sprites/Blubber/eyesHalf", gameObject.transform));
        dressList.Add(new dress("eyesLove", "Sprites/Blubber/eyesLove", gameObject.transform));

        eyes = new multiDress(ref dressList, "eyesNormal", new string[] { "eyesNormal","eyesAngry","eyesBlink","eyesClimb", "eyesHalf", "eyesLove" });

    }
    public override void UpdateCharacter()
    {
        switch(state)
        {
            case states.pushing:
                eyes.changeState("eyesAngry");
                break;
            case states.climbing:
                eyes.changeState("eyesClimb");
                break;

            default:
                eyes.changeState("eyes"+emotion);
                break;
        }

        //Blinking
        if (blinkTimer <= 0)
        {
            blink = !blink;
            blinkTimer = blink ? blinkCloseTime : UnityEngine.Random.Range(blinkTimerMin, blinkTimerMax);
        }
        else
        {
            blinkTimer -= Time.deltaTime;
        }

        if (blink && state != states.climbing)
        {
            eyes.changeState("eyesBlink");
        }
    }

    public void Angry()
    {
        eyes.changeState("eyesAngry");
        emotion = "Angry";
        setParticles("none");
    }
    public void Normal()
    {
        eyes.changeState("eyesNormal");
        emotion = "Normal";
        setParticles("none");
    }
    public void Love()
    {
        eyes.changeState("eyesHalf");
        emotion = "Love";
        setParticles(particlesLoveResource);
    }

    public void setParticles(string particleResource)
    {
        if (particleResource=="none")
        {
            if (particles != null) Destroy(particles);
            particlesEmotion = "";
        }
        else if (particlesEmotion != emotion)
        {
            if (particles != null) Destroy(particles);
            particlesEmotion = emotion;
            particles = (GameObject)Instantiate(Resources.Load(particleResource));
            //particles.transform.parent = gameObject.transform;
            particles.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.8f, gameObject.transform.position.z);
            //particles.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
