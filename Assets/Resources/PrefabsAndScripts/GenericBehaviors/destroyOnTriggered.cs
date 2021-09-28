using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyOnTriggered : MonoBehaviour
{
	[System.Serializable]
    public enum destroyBehaviors
    {
		self,
		other,
		both
	}
	
	public destroyBehaviors destroyBehavior = destroyBehaviors.self;
    public List<string> requireTags = new List<string>() { "Player" };
    public GameObject particles; //Option particles to create
    public Color particleColor = Color.white;
    public List<AudioClip> audio = new List<AudioClip>();

    Global global;

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
            GameObject p = null;
            if (particles) p = Instantiate(particles, gameObject.transform.position, Quaternion.identity);
            if (p)
            {
                var main = p.GetComponent<ParticleSystem>().main;
                main.startColor = particleColor;
            }
            if (!global) global = GameObject.FindWithTag("global").GetComponent<Global>();
            if (audio.Count > 0) global.audio.Play(audio, 0.9f, 1.2f);
			
			if (destroyBehavior==destroyBehaviors.other || destroyBehavior==destroyBehaviors.both)
				Destroy(other.gameObject);
			
			if (destroyBehavior==destroyBehaviors.self || destroyBehavior==destroyBehaviors.both)
				Destroy(gameObject);
        }
    }
}
