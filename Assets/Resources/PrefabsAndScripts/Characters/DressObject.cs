using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DressObject : MonoBehaviour
{
    public string name = "My Dress";

    public Sprite spriteSide, spriteFront, spriteBack;
    public Vector2 offset = new Vector2(0f,0f);

    private SpriteRenderer renderer;

    public bool essentialDress = false; //If set to false, the dress will be considered non-essential. Meaning it can be switched with other dresses or removed with removeNonessentialDresses. Eyes are an example of an essential dress, whereas a hat is an example of a nonessential dress.

    // Start is called before the first frame update
    void Start()
    {
        renderer = gameObject.GetComponent <SpriteRenderer>() as SpriteRenderer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Sets the facing direction of the object. 0=side, 1=front, 2=back
    public void setFacingDirection(int dir)
    {
        if (renderer == null) renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        if (dir == 0 && spriteSide!=null) renderer.sprite = spriteSide;
        if (dir == 1 && spriteFront != null) renderer.sprite = spriteFront;
        if (dir == 2 && spriteBack != null) renderer.sprite = spriteBack;
    }
}
