using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script stores a list of sprites which will be randomly chosen from when pick() is called.
//Can also be set to trigger pick() at start.

[RequireComponent(typeof(SpriteRenderer))]
public class selectRandomSprite : MonoBehaviour
{
    public bool chooseAtStart = true;
    public List<Sprite> sprites = new List<Sprite>();

    private SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        if (chooseAtStart) pick();
    }

    // This is the function at actually picks the sprite and assigns it
    public void pick()
    {
        renderer.sprite = sprites[Random.Range(0, sprites.Count - 1)];
    }
}
