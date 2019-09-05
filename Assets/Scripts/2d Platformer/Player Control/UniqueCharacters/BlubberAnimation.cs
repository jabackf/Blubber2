﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlubberAnimation : CharacterAnimation
{
    multiDress eyes;

    bool blink = false;
    float blinkTimerMin = 2f;
    float blinkTimerMax = 6f;
    float blinkTimer = 0f;
    float blinkCloseTime = 0.3f;


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
        
        eyes = new multiDress(ref dressList, "eyesNormal", new string[] { "eyesNormal","eyesAngry","eyesBlink","eyesClimb" });
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
                eyes.changeState("eyesNormal");
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
}
