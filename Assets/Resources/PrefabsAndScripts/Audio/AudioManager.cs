using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    // Audio players components.
    public AudioSource EffectsSource;
    public AudioSource MusicSource;

    //You can manually add sources from other scripts using addSource(). This can be used for things like music zones and cross fading tracks.
    private List<AudioSource> additionalSources = new List<AudioSource>();

    private int effectBufferCount = 10; //The number of sources we use for effects. Basically, we can't have more than thing many effects playing at once.
    private List<AudioSource> effectsBuffer = new List<AudioSource>(); //This is the buffer of audio clips. Basically, EffectsSource is duplicated and used to fill this buffer.

    cameraFollowPlayer cameraFollow;

    //Playlist settings
    List<AudioClip> playlist = new List<AudioClip>();
    public bool usingPlaylist = false;
    bool loopPlaylist = true;
    bool randomizePlaylist = false;
    int currentTrack = 0;
    bool playlistComplete = false;

    //Used for stopFxPitchDrop where we can stop looping effects by winding them down. Call StopFXLoopPitchDrop to stop an effects loop using this feature
    //NOTE: Pitch can also go up if endPitch is higher than current pitch
    class fxLoopPitchDrop
    {
        public AudioClip clip;
        public int bufferIndex;
        public float dropSpeed;
        public float endPitch;
        public bool pitchGoesUp = false;
		public bool stopSoundOnDone = true; //When true, the sound will be stopped when we reach the done state. If false, the sound will just continue playing at it's new destination pitch.
        public bool done = false; //Set to true when we have finished dropping the pitch. Used when stopping a sound from this list to remove it from the list.
    }
    List<fxLoopPitchDrop> fxLoopPitchDropList = new List<fxLoopPitchDrop>();

    GameObject globalObject;

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
        globalObject = caller;
        for (int i = 1; i < effectBufferCount; i++)
        {
            effectsBuffer.Add(CopyComponent<AudioSource>(EffectsSource, caller));
        }
    }

    //Adds a new audio source and returns it.
    public AudioSource AddSource()
    {
        AudioSource newSource = CopyComponent<AudioSource>(MusicSource, globalObject);
        additionalSources.Add(newSource);
        return newSource;
    }

    public AudioSource getMusicSource() { return MusicSource; }

    //Called by global right before a scene change
    public void onSceneChanging()
    {
        foreach (var s in additionalSources) GameObject.Destroy(s);
        additionalSources.Clear();
        MusicSource.volume = 1f;
        MusicSource.UnPause();
    }

    // Play a single clip through the sound effects source.
    public void Play(AudioClip clip, float randomizePitchMin = 1, float randomizePitchMax = 1)
    {
        float randomPitch = Random.Range(randomizePitchMin, randomizePitchMax);

        int sourceIndex = getEffectsBufferIndex();
        effectsBuffer[sourceIndex].clip = clip;
        effectsBuffer[sourceIndex].pitch = randomPitch;
        effectsBuffer[sourceIndex].Play();
    }
	
	//Overriden function will take a list of audio and play randomly from it.
	public void Play(List<AudioClip> clips, float randomizePitchMin = 1, float randomizePitchMax = 1)
    {
        int randomIndex = Random.Range(0, clips.Count);
        Play(clips[randomIndex],randomizePitchMin, randomizePitchMax);
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
        markStopppedSoundsFromPitchDropListAsDone();
		removeDoneSoundsFromPitchDropList();
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
        markStopppedSoundsFromPitchDropListAsDone();
    }

    //Stops the fx loop, but first it gradually brings the pitch down to create a motor wind-down effect. Speed is how fast the pitch falls and must always be positive, endingPitch is the amount the destination we must reach before the sound stops.
    //NOTE: Pitch can also go up if endPitch is higher than current pitch. This means pitch will be incremented by speed rather than decremented by it
    public void StopFXLoopPitchDrop(AudioClip clip, float endingPitch = 0.5f, float speed = 0.7f)
    {
        if (speed <= 0)
        {
            Debug.Log("You passed a speed that was <= 0 to AudioManager.StopFXLoopPitchDrop. You can't do that, you jerk! We need a positive speed or else the pitch change would never reach its destination! The pitch change can go in either direciton, but you set that with endingPitch and NOT with speed. The next log will contain the offending AudioClip, then we will abort this function call without doing anything else!");
            Debug.Log(clip);
            return;
        }
        for (int i = 0; i < effectBufferCount; i++)
        {
            if (effectsBuffer[i].clip == clip) //Found it. Lets make sure we've not already added this thing to the pitch drop list 
            {

				fxLoopPitchDrop p = new fxLoopPitchDrop();
				bool onList=false;
                if (fxLoopPitchDropList.Count > 0)
                {
                    foreach (var l in fxLoopPitchDropList)
                    {
                        if (l.bufferIndex == i) 
						{
							p=l;
							onList=true;
						}
                    }
                }
				
				p.dropSpeed = speed;
				p.endPitch = endingPitch;
				p.stopSoundOnDone=true;
				if (effectsBuffer[i].pitch < endingPitch) p.pitchGoesUp = true;
				else p.pitchGoesUp=false;
				
				if (!onList) 
				{
					p.bufferIndex = i;
					p.clip = clip;
					fxLoopPitchDropList.Add(p);
				}
            }
        }
    }
	
	//Plays a looping effect, starting it out with a gliding pitch.
	//Most commonly this would be used for rising pitch, but can be used with descending pitch if the startingPitch is higher than endingPitch.
	//Speed must be positive in either case though.
	public void PlayFXLoopPitchRise(AudioClip clip, float startingPitch = 0.5f, float endingPitch=1f, float speed = 0.7f)
    {
		if (speed <= 0)
        {
            Debug.Log("You passed a speed that was <= 0 to AudioManager.PlayFXLoopPitchRise. You can't do that, you jerk! We need a positive speed or else the pitch change would never reach its destination! The pitch change can go in either direciton, but you set that with endingPitch and NOT with speed. The next log will contain the offending AudioClip, then we will abort this function call without doing anything else!");
            Debug.Log(clip);
            return;
        }
		
		StopFXLoop(clip); //Stop it in case it's already playing.
		
        int sourceIndex = getEffectsBufferIndex();
        effectsBuffer[sourceIndex].clip = clip;
        effectsBuffer[sourceIndex].loop = true;
        effectsBuffer[sourceIndex].pitch = startingPitch;
        effectsBuffer[sourceIndex].Play();
		
		fxLoopPitchDrop p = new fxLoopPitchDrop();
		p.bufferIndex = sourceIndex;
		p.clip = clip;
		p.dropSpeed = speed;
		p.endPitch = endingPitch;
		p.stopSoundOnDone=false;
		if (effectsBuffer[sourceIndex].pitch < endingPitch) p.pitchGoesUp = true;
		fxLoopPitchDropList.Add(p);
	}
	
	//Glides the pitch up or down by amount pitchChange (positive or negative). The sound will continue to play at the end.
	public void fxPitchGlide(AudioClip clip, float pitchChange, float speed = 0.7f)
    {
        if (speed <= 0)
        {
            Debug.Log("You passed a speed that was <= 0 to AudioManager.fxPitchGlide. You can't do that, you jerk! We need a positive speed or else the pitch change would never reach its destination! The pitch change can go in either direciton, but you set that with endingPitch and NOT with speed. The next log will contain the offending AudioClip, then we will abort this function call without doing anything else!");
            Debug.Log(clip);
            return;
        }
        for (int i = 0; i < effectBufferCount; i++)
        {
            if (effectsBuffer[i].clip == clip) //Found it. Lets make sure we've not already added this thing to the pitch drop list. If we have, we'll just change the one that's on there.
            {

				fxLoopPitchDrop p = new fxLoopPitchDrop();
				bool onList=false;
                if (fxLoopPitchDropList.Count > 0)
                {
                    foreach (var l in fxLoopPitchDropList)
                    {
                        if (l.bufferIndex == i) 
						{
							p=l;
							onList=true;
						}
                    }
                }
				
				p.dropSpeed = speed;
				p.endPitch = effectsBuffer[i].pitch+pitchChange;
				p.stopSoundOnDone=false;
				if (effectsBuffer[i].pitch < p.endPitch) p.pitchGoesUp = true;
				else p.pitchGoesUp=false;
				
				if (!onList) 
				{
					p.bufferIndex = i;
					p.clip = clip;
					fxLoopPitchDropList.Add(p);
				}
            }
        }
    }

    //This is used internally. It checks fxLoopPitchDropList for any sounds that have been manually stopped and marks them as done.
    void markStopppedSoundsFromPitchDropListAsDone()
    {
        if (fxLoopPitchDropList.Count == 0) return;
        foreach (var p in fxLoopPitchDropList)
        {
            if (!effectsBuffer[p.bufferIndex].isPlaying) p.done = true;
        }
    }
    void removeDoneSoundsFromPitchDropList()
    {
        if (fxLoopPitchDropList.Count == 0) return;
        fxLoopPitchDropList.RemoveAll(p => p.done == true);
    }

    //Plays a sound effect if the specified position is within the camera's view. If cameraFollowPlayer does not exist then it will play the sound every time. 
    //Buffer is added to the boundaries of the camera so pos can be a little outside of the view (negative buffer) or needs to be a little inside of the view (positive buffer)
    //By default the pos can be within one unit ouside of the view and the sound will still play.
    public void PlayIfOnScreen(AudioClip clip, Vector2 pos, float buffer=-1f, float randomizePitchMin = 1, float randomizePitchMax = 1)
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

        if (play) Play(clip, randomizePitchMin, randomizePitchMax);

    }
    public void PlayIfOnScreen(List<AudioClip> clips, Vector2 pos, float buffer = -1f, float randomizePitchMin = 1, float randomizePitchMax = 1)
    {
        int randomIndex = Random.Range(0, clips.Count);
        PlayIfOnScreen(clips[randomIndex], pos, buffer, randomizePitchMin, randomizePitchMax);
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
    public void Update(float deltaTime)
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

        //Process any gliding pitch changes that are currently happening
        if (fxLoopPitchDropList.Count > 0)
        {
            foreach (var p in fxLoopPitchDropList)
            {
                effectsBuffer[p.bufferIndex].pitch -= (p.pitchGoesUp ? -p.dropSpeed : p.dropSpeed)*deltaTime;
                bool done = false;
                if (p.pitchGoesUp && effectsBuffer[p.bufferIndex].pitch > p.endPitch) done = true;
                if (!p.pitchGoesUp && effectsBuffer[p.bufferIndex].pitch < p.endPitch) done = true;
                if (done)
                {
					p.done=true;
					if (p.stopSoundOnDone)
						StopFXLoop(p.clip);
                }
            }
            removeDoneSoundsFromPitchDropList();
        }
    }
}