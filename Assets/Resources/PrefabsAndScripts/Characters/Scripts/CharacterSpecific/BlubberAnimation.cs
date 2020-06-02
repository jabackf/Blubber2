using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlubberAnimation : CharacterAnimation
{

    /*List of character emotions
     * 
     *+ Normal
     *+ Love
     *+ Surprise (exclamation mark)
     * Cry
     * Sad
     * Concern (single teardrop, anime style)
     *+ Squint
     * Excited (fast jumping up and down, closed eyes)
     *+ Angry
     * cuteEyes
     *+ bugEyes
     *+ smallEyes
     * eyesClosed
     * 
     * Other:
     * 
     * facePlayer
     * 
     */


    [Space]
    [Header("Blubber Emotions")]

    private Global global;

    multiDress eyes;

    bool blink = false;
    float blinkTimerMin = 4f;
    float blinkTimerMax = 8f;
    float blinkTimer = 0f;
    float blinkCloseTime = 0.25f;
    string currentEyes = "Normal";
    string currentEmotion = "Normal";

    //Character particles are used for various effects, like the hearts that come from the character's love emotion
    private GameObject particles;

    public GameObject particlesLovePrefab;


    //Emote icons are icons that can appear near the character to display emotions such as an exclamation point or tear drop
    private GameObject emoteIcon;

    public GameObject eiExclamationPoint;



    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        base.Start();
    }

    public override void SetupCharacter()
    {
        blinkTimer = UnityEngine.Random.Range(blinkTimerMin, blinkTimerMax);

        dressList.Add(new dress("eyesNormal", global.dirBlubberSprites + "Eyes", gameObject.transform, false));
        dressList.Add(new dress("eyesAngry", global.dirBlubberSprites + "eyesAngry", gameObject.transform, false));
        dressList.Add(new dress("eyesBlink", global.dirBlubberSprites + "eyesBlink", gameObject.transform, false));
        dressList.Add(new dress("eyesClimb", global.dirBlubberSprites + "eyesClimb", gameObject.transform, false));
        dressList.Add(new dress("eyesHalf", global.dirBlubberSprites + "eyesHalf", gameObject.transform, false));
        dressList.Add(new dress("eyesLove", global.dirBlubberSprites + "eyesLove", gameObject.transform, false));
        dressList.Add(new dress("eyesSmall", global.dirBlubberSprites + "eyesSmall", gameObject.transform, false));
        dressList.Add(new dress("eyesBug", global.dirBlubberSprites + "eyesBug", gameObject.transform, false));

        eyes = new multiDress(ref dressList, "eyesNormal", new string[] { "eyesNormal","eyesAngry","eyesBlink","eyesClimb", "eyesHalf", "eyesLove", "eyesSmall", "eyesBug" });

    }
    public override void UpdateCharacter()
    {
        switch (state)
        {
            case states.pushing:
                eyes.changeState("eyesAngry");
                break;
            case states.climbing:
                eyes.changeState("eyesClimb");
                break;

            default:
                eyes.changeState("eyes" + currentEyes);
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

    //Sets the particles to specified prefab. Pass null for no particles
    public void setParticles(GameObject prefab)
    {
        if (particles != null) Destroy(particles);
        if (prefab!=null)
        {
            particles = (GameObject)Instantiate(prefab);
            particles.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.8f, gameObject.transform.position.z);
        }
    }

    //Sets the emote icon to specified prefab. Pass null for no emote icon
    public void setEmoteIcon(GameObject prefab)
    {
        if (emoteIcon != null) Destroy(emoteIcon);
        if (prefab!=null)
        {
            emoteIcon = (GameObject)Instantiate(prefab);
            emoteIcon.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
            emoteIcon.transform.parent = gameObject.transform;
        }
    }

    //START EMOTION IMPLEMENTATION

    public void Angry()
    {
        eyes.changeState("eyesAngry");
        currentEyes = "Angry";
        currentEmotion = "Angry";
        setParticles(null);
        setEmoteIcon(null);
    }
    public void Normal()
    {
        eyes.changeState("eyesNormal");
        currentEyes = "Normal";
        currentEmotion = "Normal";
        setParticles(null);
        setEmoteIcon(null);
    }
    public void Love()
    {
        eyes.changeState("eyesLove");
        currentEyes = "Love";
        currentEmotion = "Love";
        setParticles(particlesLovePrefab);
        setEmoteIcon(null);
    }
    public void Squint()
    {
        eyes.changeState("eyesHalf");
        currentEyes = "Half";
        currentEmotion = "Squint";
        setEmoteIcon(null);
    }
    public void Surprise()
    {
        eyes.changeState("eyesBug");
        currentEyes = "Bug";
        currentEmotion = "Surprise";
        setEmoteIcon(eiExclamationPoint);
    }
    public void smallEyes()
    {
        eyes.changeState("eyesSmall");
        currentEyes = "Small";
    }
    public void bugEyes()
    {
        eyes.changeState("eyesBug");
        currentEyes = "Bug";
    }
}
