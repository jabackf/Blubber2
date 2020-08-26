using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    // Audio players components.
    public AudioSource EffectsSource;
    public AudioSource MusicSource;

    private int effectBufferCount = 10; //The number of sources we use for effects. Basically, we can't have more than thing many effects playing at once.
    private List<AudioSource> effectsBuffer = new List<AudioSource>(); //This is the buffer of audio clips. Basically, EffectsSource is duplicated and used to fill this buffer.

    cameraFollowPlayer cameraFollow;

    //Playlist settings
    List<AudioClip> playlist = new List<AudioClip>();
    bool usingPlaylist = false;
    bool loopPlaylist = true;
    bool randomizePlaylist = false;
    int currentTrack = 0;
    bool playlistComplete = false;

    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    public void Start(GameObject caller)
    {
        effectsBuffer.Add(EffectsSource);
        for (int i = 1; i<effectBufferCount; i++)
        {
            effectsBuffer.Add(CopyComponent<AudioSource>(EffectsSource, caller));
        }
    }

    // Play a single clip through the sound effects source.
    public void Play(AudioClip clip)
    {
        int sourceIndex = getEffectsBufferIndex();
        effectsBuffer[sourceIndex].clip = clip;
        effectsBuffer[sourceIndex].pitch = 1;
        effectsBuffer[sourceIndex].Play();
    }
    //Plays an effect in loop indefinitely. Must be stopped with either StopFXLoop or StopFXLoopAll
    public void PlayFXLoop(AudioClip clip)
    {
        int sourceIndex = getEffectsBufferIndex();
        effectsBuffer[sourceIndex].clip = clip;
        effectsBuffer[sourceIndex].loop = true;
        effectsBuffer[sourceIndex].pitch = 1;
        effectsBuffer[sourceIndex].Play();
    }
    //Searches for the specific effect and stops it. Used to stop a looping sound effect.
    public void StopFXLoop(AudioClip clip)
    {
        for (int i = 0; i < effectBufferCount; i++)
        {
            if (effectsBuffer[i].clip == clip)
            {
                effectsBuffer[i].loop = false;
                effectsBuffer[i].Stop();
            }
        }
    }
    //Stops any sound effects that are looping.
    public void StopFXLoopAll()
    {
        for (int i = 0; i < effectBufferCount; i++)
        {
            if (effectsBuffer[i].loop)
            {
                effectsBuffer[i].loop = false;
                effectsBuffer[i].Stop();
            }
        }
    }

    //Plays a sound effect if the specified position is within the camera's view. If cameraFollowPlayer does not exist then it will play the sound every time. 
    //Buffer is added to the boundaries of the camera so pos can be a little outside of the view (negative buffer) or needs to be a little inside of the view (positive buffer)
    //By default the pos can be within one unit ouside of the view and the sound will still play.
    public void PlayIfOnScreen(AudioClip clip, Vector2 pos, float buffer=-1f)
    {
        if (!cameraFollow)
        {
            cameraFollow = Camera.main.GetComponent<cameraFollowPlayer>();
        }

        bool play = true;

        if (cameraFollow)
        {
            if (!cameraFollow.insideView(pos, buffer, buffer)) play = false;
        }

        if (play) Play(clip);

    }

    public int getEffectsBufferIndex()
    {
        int sourceIndex = 0;
        for (int i = 0; i < effectBufferCount; i++)
        {
            if (!effectsBuffer[i].isPlaying)
            {
                sourceIndex = i;
                i = effectBufferCount;
            }
        }
        return sourceIndex;
    }

    //Plays music
    public void PlayMusic(AudioClip clip, bool loop=true, bool restartIfPlaying = false)
    {
        usingPlaylist = false;
        MusicSource.loop = loop;
        if (MusicSource.isPlaying && MusicSource.clip == clip) //We're already playing the clip
        {
            if (restartIfPlaying)
            {
                MusicSource.Play();
            }
        }
        else
        {
            MusicSource.clip = clip;
            MusicSource.Play();
        }
    }

    //Plays through a playlist of music.
    //Restart if playing = If the track we are trying to play is on the playlist, should we restart the entire playlist (true)? Or should we start the playlist on that track (false)?
    //Loop = If the playlist isn't randomized, then should we loop to the start track once we reach the end?
    //randomizePlaylist = Should we play all tracks in order, from start to finish (false)? Or should be randomly play songs on the playlist, making sure we do no repeat the same song twice (true)?
    public void PlayMusic(List<AudioClip> newlist, bool loop = true, bool restartIfPlaying = false, bool randomizePlaylist = false)
    {
        if (newlist.Count == 0) return;
        if (newlist.Count == 1)
        {
            usingPlaylist = false;
            PlayMusic(newlist[0], loop, restartIfPlaying);
            return;
        }
        usingPlaylist = true;
        playlistComplete = false;
        bool playTrack = true;
        if (!restartIfPlaying) //If this is false then we need to see if the current audioclip exists on the new playlist and is currently playing
        {
            if (newlist.Contains(MusicSource.clip) && MusicSource.isPlaying)
            {
                playTrack = false;
                currentTrack = newlist.IndexOf(MusicSource.clip);
            }
        }

        playlist = newlist;
        loopPlaylist = loop;
        this.randomizePlaylist = randomizePlaylist;

        if (playTrack)
        {
            RestartPlaylist();
            PlayMusic(newlist[currentTrack], false, true);
            usingPlaylist = true;
        }
    }

    //Restarts the current playlist at 0, or random if the playback is randomized
    public void RestartPlaylist()
    {
        currentTrack = -1;
        AdvanceToNextTrack();
    }

    public void AdvanceToNextTrack()
    {
        if (randomizePlaylist)
        {
            int p = currentTrack;
            int tries = 10;
            while (tries > 0)
            {
                tries -= 1;
                currentTrack = UnityEngine.Random.Range(0, playlist.Count);
                if (currentTrack != p) tries = 0;
            }
        }
        else
        {
            currentTrack += 1;
            if (currentTrack > playlist.Count)
            {
                currentTrack = 0;
                playlistComplete = true;
            }
        }
    }

    public void StopMusic()
    {
        MusicSource.Stop();
    }

    // Play a random clip from an array, and randomize the pitch slightly. 
    public void RandomSoundEffect(AudioClip[] clips, float minPitch = 0.85f, float maxPitch = 1.15f)
    {
        int sourceIndex = getEffectsBufferIndex();

        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(minPitch, maxPitch);

        effectsBuffer[sourceIndex].pitch = randomPitch;
        effectsBuffer[sourceIndex].clip = clips[randomIndex];
        effectsBuffer[sourceIndex].Play();
    }

    public void RandomSoundEffect(AudioClip clip, float minPitch = 0.85f, float maxPitch = 1.15f)
    {
        int sourceIndex = getEffectsBufferIndex();
        float randomPitch = Random.Range(minPitch, maxPitch);
        effectsBuffer[sourceIndex].pitch = randomPitch;
        effectsBuffer[sourceIndex].clip = clip;
        effectsBuffer[sourceIndex].Play();
    }

    //Called from the Global class which keeps the reference to this instance
    public void Update()
    {
        if (usingPlaylist)
        {
            if (!MusicSource.isPlaying)
            {
                if ( !playlistComplete || loopPlaylist  )
                {
                    AdvanceToNextTrack();
                    if (!playlistComplete || loopPlaylist)
                    {
                        playlistComplete = false;
                        PlayMusic(playlist[currentTrack], false, true);
                        usingPlaylist = true;
                    }
                }
            }
        }
    }
}