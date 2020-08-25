using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    // Audio players components.
    public AudioSource EffectsSource;
    public AudioSource MusicSource;

    // Random pitch adjustment range.
    public float LowPitchRange = .95f;
    public float HighPitchRange = 1.05f;

    // Play a single clip through the sound effects source.
    public void Play(AudioClip clip)
    {
        EffectsSource.clip = clip;
        EffectsSource.Play();
    }

    // Play a single clip through the music source.
    public void PlayMusicOnce(AudioClip clip)
    {
        MusicSource.clip = clip;
        MusicSource.Play();
        MusicSource.loop = false;
    }

    //Loops music indefinitely
    public void PlayMusicLoop(AudioClip clip)
    {
        MusicSource.clip = clip;
        MusicSource.Play();
        MusicSource.loop = true;
    }

    // Play a random clip from an array, and randomize the pitch slightly.
    public void RandomSoundEffect(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(LowPitchRange, HighPitchRange);

        EffectsSource.pitch = randomPitch;
        EffectsSource.clip = clips[randomIndex];
        EffectsSource.Play();
    }

}