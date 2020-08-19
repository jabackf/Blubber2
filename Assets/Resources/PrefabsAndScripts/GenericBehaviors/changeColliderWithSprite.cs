using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script lets you link sprites and colliders together.
//I.E. if the renderer is set to a specified sprite, then the collider for that sprite is activated and all other colliders are disabled.
//Used to have the collider change with the sprite. For example, a standing barrel might have a box collider. A laying barrel might have a circle so it can roll.
//Another example: An object with the frozen variation of it's sprites might use a collider with a more slippery material

//Note: If the spriteRenderer is set to a sprite that is not specified in the spriteColliders list, then ALL the collider2ds listed in the spriteColliders list will be turned 
//off. This script will not touch collider2Ds that are not specified in the spriteColliders list.

[RequireComponent(typeof(SpriteRenderer))]
public class changeColliderWithSprite : MonoBehaviour
{
    [System.Serializable]
    public struct spriteCollider
    {
        public Sprite sprite;
        public Collider2D collider;
    }

    public List<spriteCollider> spriteColliders = new List<spriteCollider>();

    private SpriteRenderer renderer;
    private Sprite previousSprite; //Used to detect when the sprite has changed

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        onSpriteChange();
        previousSprite = renderer.sprite;
    }

    void Update()
    {
        if (renderer.sprite!=previousSprite)
        {
            onSpriteChange();
            previousSprite = renderer.sprite;
        }
    }

    //This is the function that actually turns colliders on and off. This script will manually call this method from Start() and from Update() if we detect a sprite change 
    public void onSpriteChange()
    {
        //First lets go through and turn off all of the specified colliders. It's a good idea to do this seperately so we don't get a mess from things like sprites using multiple colliders
        foreach (var e in spriteColliders) e.collider.enabled = false;

        foreach (var e in spriteColliders) //Now we seek out the match and activate it.
        {
            if (renderer.sprite == e.sprite) //Found a match
            {
                e.collider.enabled = true;
            }
        }
    }

}
