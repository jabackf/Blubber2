using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Changes the audio clip currently playing in global.audio.MusicSource when an object with requireTags triggers it.
//This is different from a music zone because this isn't playing an additional source, it is actually changing the background music.
//REQUIRES this object to have changeMusicTriggers tag

public class changeMusicOnTrigger : MonoBehaviour
{
    //These are the tag(s) that are required by the triggering object. Leave empty to accept any object
    public List<string> requireTags = new List<string>() { "Player" };

    public bool pullFromSceneSettings = false; //If true, we will pull the track(s) and settings from the sceneSettings object. Basically, checking this is saying revert to the original scene music configuration.

    public List<AudioClip> music = new List<AudioClip>(); //Leave empty to continue playing whatever is currently playing
    public bool randomizePlaylist = false; //If false, then we play the list all the way through looping to the first track when complete. If true then the tracks will be played randomly (but tracks will not be repeated back-to-back)
    public bool noSceneMusic = false; //If no AudioClip is selected for music, then whatever music is currently playing will continue to play. In order to have a scene that is completely silent, you must check this option.
    public bool restartIfPlaying = false; //If the music is already playing the setting this to true will restart it on scene load
    public bool loopAudio = true; //If true then the music will loop indefinitely. If multilple tracks are selected then the playlist will loop indefinitely


    Global global;

    public bool triggered = false;

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();

        if (pullFromSceneSettings)
        {
            triggered = true;

            GameObject sgo = GameObject.FindWithTag("SceneSettings");
            sceneSettings ss = sgo.GetComponent<sceneSettings>();
            music = ss.music;
            randomizePlaylist = ss.randomizePlaylist;
            noSceneMusic = ss.noSceneMusic;
            restartIfPlaying = ss.restartIfPlaying;
            loopAudio = ss.loopAudio;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
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
            Trigger();
        }
    }

    void Trigger()
    {

        if (triggered) return;

        //Set all other changeMusicTriggers to false.
        GameObject[] triggers = GameObject.FindGameObjectsWithTag("changeMusicTrigger");

        foreach(var t in triggers)
        {
            t.GetComponent<changeMusicOnTrigger>().triggered = false;
        }

        triggered = true;

        AudioSource ms = global.audio.getMusicSource();
        if (music.Count == 1) global.audio.PlayMusic(music[0], loopAudio, restartIfPlaying);
        if (music.Count > 1) global.audio.PlayMusic(music, loopAudio, restartIfPlaying, randomizePlaylist);

        if (noSceneMusic) global.audio.StopMusic();
    }
}
