using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sideDoorScript : MonoBehaviour
{
    public Sprite sprClosed, sprOpenLeft, sprOpenRight;
    public bool openedByCharacters, openedByAnimals;
    public AudioClip sndOpen, sndClose;
    Global global;

    bool open;

    GameObject opener=null;

    SpriteRenderer renderer;
    List<Collider2D> colliders = new List<Collider2D>();

    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        renderer = gameObject.GetComponent<SpriteRenderer>();
        Collider2D[] colList = transform.GetComponentsInChildren<Collider2D>();
        foreach(var c in colList)
        {
            if (!c.isTrigger) colliders.Add(c);
        }
        open = false;
        renderer.sprite = sprClosed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (open) return;
        Animal animal = other.gameObject.GetComponent<Animal>();
        CharacterController2D charcont = other.gameObject.GetComponent<CharacterController2D>();
        if ( (openedByCharacters&&charcont) || (openedByAnimals && animal))
        {
            open = true;
            if (other.gameObject.transform.position.x < transform.position.x) renderer.sprite = sprOpenRight;
            else renderer.sprite = sprOpenLeft;
            opener = other.gameObject;
            setColliderEnabled(false);
            if (sndOpen) global.audio.PlayIfOnScreen(sndOpen,(Vector2)transform.position);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!open) return;
        if (other.gameObject == opener)
        {
            open = false;
            renderer.sprite = sprClosed;
            setColliderEnabled(true);
            if (sndClose) global.audio.PlayIfOnScreen(sndClose, (Vector2)transform.position);
        }
    }

    void setColliderEnabled(bool enable)
    {
        foreach (var c in colliders) c.enabled = enable;
    }
}
