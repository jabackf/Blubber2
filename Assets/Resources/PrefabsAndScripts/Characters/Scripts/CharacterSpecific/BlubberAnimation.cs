using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class BlubberAnimation : CharacterAnimation
{

    /*List of character emotions
     * 
     *+ Normal
     *+ Love
     *+ Surprise (exclamation mark)
     *+ Cry
     *+ Squint
     *+ Angry
     *+ cuteEyes
     *+ bugEyes
     *+ smallEyes
     *+ closedEyes
     *+ blackEye
     * 
     * Other:
     * 
     *+ Front  (face forward) (in CharacterAnimation)
     *+ Back  (face background) (in CharacterAnimation)
     *+ Side  (face left or right as defined by the flip settings in CharacterController2D) (in CharacterAnimation)
     *+ FaceLeft (in CharacterContoller2D)
     *+ FaceRight (in CharacterController2D)
     *+ facePlayer (faces left or right towards object with Player tag) (in CharacterAnimation)
     *+ faceAwayPlayer (opposite of facePlayer) (in CharacterAnimation)
     *+ jumpingOn  (repeatedly jumps)
     *+ jumpingOff
     *+ jumpingToggle
     *+ CircleOn (spin in a circle by calling front,side,back,side...) (in CharacterAnimation)
     *+ CircleOff  (in CharacterAnimation)
     *+ circleToggle  (in CharacterAnimation)
     */


    [Space]
    [Header("Blubber Emotions")]

    private Global global;

    multiDress eyes;

    BlubberInputInterface bii;

    bool jumping = false; //Set to true and the character will jump repeatedly, provided it has a BlubberInputInterface

    public bool normalOnRespawn = true; //If true, we will send a Normal() message to the character upon respawn.

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
    public GameObject particlesTearsPrefab;


    //Emote icons are icons that can appear near the character to display emotions such as an exclamation point or tear drop
    private GameObject emoteIcon;

    public GameObject eiExclamationPoint;

    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        bii = gameObject.GetComponent<BlubberInputInterface>() as BlubberInputInterface;
        base.Start();

    }

    //This function look at various character settings (dresses, color, ect) and looks for specific combinations. It returns a string that represents the first combination found
    //For example, it you're wearing a Santa costume then it it returns "Santa"
    public string getCharacterConfigurationString()
    {
        foreach (var d in dressList)
        {
            if (d.name.CaseInsensitiveContains("chef")) return "Chef";
            if (d.name.CaseInsensitiveContains("santa")) return "Santa";
            if (d.name.CaseInsensitiveContains("link hat")) return "Link";
        }
        return "None";
    }

    public override void SetupCharacter()
    {
        blinkTimer = UnityEngine.Random.Range(blinkTimerMin, blinkTimerMax);

        string eyeDir = global.dirBlubberSprites + "Eyes/";
        dressList.Add(new dress("eyesNormal", eyeDir + "EyesNormal", gameObject.transform, false));
        dressList.Add(new dress("eyesAngry", eyeDir + "eyesAngry", gameObject.transform, false));
        dressList.Add(new dress("eyesBlink", eyeDir + "eyesBlink", gameObject.transform, false));
        dressList.Add(new dress("eyesSquint", eyeDir + "eyesSquint", gameObject.transform, false));
        dressList.Add(new dress("eyesLove", eyeDir + "eyesLove", gameObject.transform, false));
        dressList.Add(new dress("eyesSmall", eyeDir + "eyesSmall", gameObject.transform, false));
        dressList.Add(new dress("eyesBug", eyeDir + "eyesBug", gameObject.transform, false));
        dressList.Add(new dress("eyesCute", eyeDir + "eyesCute", gameObject.transform, false));
        dressList.Add(new dress("eyesBlack", eyeDir + "eyesBlack", gameObject.transform, false));

        eyes = new multiDress(ref dressList, "eyesNormal", new string[] { "eyesNormal","eyesAngry","eyesBlink", "eyesSquint", "eyesLove", "eyesSmall", "eyesBug", "eyesCute", "eyesBlack" });

    }
    public override void UpdateCharacter()
    {
        if (!isDead)
        {
            switch (state)
            {
                case states.pushing:
                    eyes.changeState("eyesAngry");
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

            if (bii != null && jumping)
                bii.jump = true;
        }
    }



    //Change the color of the blubber character
    public void changeColor(Color c)
    {
        renderer.color = c;
    }
    public Color getColor()
    {
        return renderer.color;
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

    public override void onRespawn()
    {
        if (normalOnRespawn)
        {
            Normal();
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
        eyes.changeState("eyesSquint");
        currentEyes = "Squint";
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
    public void closedEyes()
    {
        eyes.changeState("eyesBlink");
        currentEyes = "Blink";
    }
    public void bugEyes()
    {
        eyes.changeState("eyesBug");
        currentEyes = "Bug";
    }
    public void cuteEyes()
    {
        eyes.changeState("eyesCute");
        currentEyes = "Cute";
    }
    public void Cry()
    {
        eyes.changeState("eyesBlink");
        currentEyes = "Blink";
        currentEmotion = "Cry";
        setParticles(particlesTearsPrefab);
        setEmoteIcon(null);
    }
    public void blackEye()
    {
        eyes.changeState("eyesBlack");
        currentEyes = "Black";
        currentEmotion = "Pain";
        setParticles(null);
        setEmoteIcon(null);
    }

    public void jumpingOn()
    {
        jumping = true;
    }
    public void jumpingOff()
    {
        jumping = false;
    }
    public void jumpingToggle()
    {
        jumping = !jumping;
    }

}
