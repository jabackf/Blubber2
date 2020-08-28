using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Partitions part of the scene to play alternative music. Can be activated with triggers and/or cross faded based on distance.
//Can be used with music set to none so you can have silent zones

public class MusicZone : MonoBehaviour
{
    [System.Serializable]
    public enum configurations
    {
        trigger, //Activated / deactivated using an attached trigger

        //Cross faded volume is based on the distance from the object to this gameObject.transform. Use outerDistance and innerDistance to set this up. 
        //Can only work with one object, and the first object found in requireTags is used.
        //In this configuration, any trigger enters from the distanceObject will immediately activate the music and switch volume levels to full activation.
        //This is so we can construct audio zones that may have distance crossfades when coming from one direction, but remain playing when you are on the other side.
        distance
    }

    public configurations configuration = configurations.trigger;

    public AudioClip music;
    private bool activated = false;

    //These are the tag(s) that are required by the triggering object. Leave empty to accept any object
    public List<string> requireTags = new List<string>() { "Player" };
    private GameObject distanceObject; //The object tracked when using the distance configuration. This is the first object found with requireTags[0], so likely this is always going to be the player.

    public float crossFadeSpeed = 1f;

    public float sceneMusicDestination = 0f; //How much to turn the scene music down while we are in the zone. 0 to silence completely

    //Rather to restart the music from the beginning when we go from 0 to >0 in volume.
    public bool restartSceneMusic = false, restartZoneMusic = false;

    private float sceneMusicVol = 1f, zoneVol = 0f;
    AudioSource zoneSource, sceneMusicSource;

    //When using distance configuration, outerDistance is when the music zone is initially activated. Inner distance is when we reach full activation volumes.
    [Space]
    [Header("Distance Configuration Settings")]
    public float outerDistance = 6f;
    public float innerDistance = 4f;

    private bool distanceInsideTrigger = false; //If we are set to distance configuration, this variable gets set to true when our distance object is inside of this gameObject's trigger. See configurations.distance for an explanation of what triggers do in distance mode.

    public Transform setOuterDistance, setInnerDistance; //Optional. Overrides manual setting of outter and inner distance. The distance from these transforms is used to specify an outter distance and an inner distance. 


    Global global;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        zoneSource = global.audio.AddSource();
        zoneSource.clip = music;
        sceneMusicSource = global.audio.getMusicSource();

        if (setOuterDistance)
            outerDistance = Vector3.Distance(transform.position, setOuterDistance.position);
        if (setInnerDistance)
            innerDistance = Vector3.Distance(transform.position, setInnerDistance.position);

        if (activated) Activate();
    }

    bool getDistanceObject()
    {
        if (!distanceObject) distanceObject = GameObject.FindWithTag(requireTags[0]);
        if (distanceObject) return true;
        else return false;
    }

    // Update is called once per frame
    void Update()
    {
        float previousSceneMusicVol = sceneMusicVol;
        float previousZoneVol = zoneVol;

        if (configuration == configurations.trigger)
        {
            if (activated)
            {
                if (sceneMusicVol > sceneMusicDestination) sceneMusicVol -= crossFadeSpeed * Time.deltaTime;
                if (zoneVol < 1) zoneVol += crossFadeSpeed * Time.deltaTime;
            }
            else
            {
                if (sceneMusicVol < 1) sceneMusicVol += crossFadeSpeed * Time.deltaTime;
                if (zoneVol > 0) zoneVol -= crossFadeSpeed * Time.deltaTime;
            }
        }

        if (configuration == configurations.distance)
        {
            if (getDistanceObject())
            {
                if (!distanceInsideTrigger)//The object isn't inside of a trigger
                {
                    float pdistance = Vector3.Distance(gameObject.transform.position, distanceObject.transform.position);
                    if (pdistance < outerDistance)
                    {
                        Activate();
                        float difference = outerDistance - innerDistance;
                        float playerDifference = pdistance - innerDistance; //The closer this number is to 0, the closer we want to be to full activated volume. The closer this number is to difference, the closer we need to be to full deactivated volumes

                        zoneVol = 1 - (1 / difference * (playerDifference));
                        sceneMusicVol = sceneMusicDestination + ((1 - zoneVol) * (1 - sceneMusicDestination));

                        if (zoneVol < 0.02f) zoneVol = 0;
                        if (sceneMusicVol < 0.02f) sceneMusicVol = 0;
                        if (zoneVol > 0.95f) zoneVol = 1;
                        if (sceneMusicVol > 0.95f) sceneMusicVol = 1;
                    }
                    else
                    {
                        Deactivate();
                        zoneVol = 0f;
                        sceneMusicVol = 1f;
                    }
                }
                else //The distance object is inside of one of our triggers
                {
                    Activate();
                    zoneVol = 1f;
                    sceneMusicVol = sceneMusicDestination;
                }
            }
        }

        sceneMusicVol = Mathf.Clamp(sceneMusicVol, sceneMusicDestination, 1f);
        zoneVol = Mathf.Clamp(zoneVol, 0f, 1f);

        if (previousZoneVol == 0 && zoneVol > 0 && restartZoneMusic)
        {
            zoneSource.Stop();
            zoneSource.Play();
        }
        if (previousSceneMusicVol == sceneMusicDestination && sceneMusicVol > sceneMusicDestination && restartSceneMusic)
        {
            sceneMusicSource.Stop();
            sceneMusicSource.Play();
        }

        sceneMusicSource.volume = sceneMusicVol;
        zoneSource.volume = zoneVol;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (configuration == configurations.trigger)
        {
            bool goodToGo = true;
            if (requireTags.Count > 0)
            {
                goodToGo = false;
                foreach (var t in requireTags)
                {
                    if (t == other.gameObject.tag) goodToGo = true;
                }
            }
            if (goodToGo)
            {
                Activate();
            }
        }

        if (configuration == configurations.distance)
        {
            if (other.gameObject == distanceObject)
            {
                distanceInsideTrigger = true;
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (configuration == configurations.trigger)
        {
            bool goodToGo = true;
            if (requireTags.Count > 0)
            {
                goodToGo = false;
                foreach (var t in requireTags)
                {
                    if (t == other.gameObject.tag) goodToGo = true;
                }
            }
            if (goodToGo)
            {
                Deactivate();
            }
        }
        if (configuration == configurations.distance)
        {
            if (other.gameObject == distanceObject)
            {
                distanceInsideTrigger = false;
            }
        }
    }

    public void Activate()
    {
        activated = true;
        if (!zoneSource.isPlaying) zoneSource.Play();
    }
    public void Deactivate()
    {
        activated = false;
    }
}
