using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//This script is automatically added and configured by sceneSettings. It is applied to any sprite-based object that gets a dropshadow.

public class updateDropShadow : MonoBehaviour
{
    public GameObject obj;  //The object that is casting the shadow
    public SpriteRenderer objSpriteRenderer;
    public Vector2 offset;
    Color color; //Used so we can store our initial color and match the object's alpha

    private SpriteRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>() as SpriteRenderer;
        renderer.sprite = objSpriteRenderer.sprite;
        renderer.drawMode = objSpriteRenderer.drawMode;
        renderer.tileMode = objSpriteRenderer.tileMode;
        color = renderer.color;
        color.a = objSpriteRenderer.color.a;
        renderer.color = color;
    }


    void LateUpdate()
    {
        if (!obj)
        {
            Destroy(gameObject);
        }
        else
        {
            if (!renderer) renderer = GetComponent<SpriteRenderer>() as SpriteRenderer;

            renderer.enabled = objSpriteRenderer.enabled;
            renderer.flipX = objSpriteRenderer.flipX;
            renderer.flipY = objSpriteRenderer.flipY;
            renderer.sprite = objSpriteRenderer.sprite;
            renderer.size = objSpriteRenderer.size;
            color.a = objSpriteRenderer.color.a;
            renderer.color = color;

            //update the position and rotation of the sprite's shadow with moving sprite

            //gameObject.transform.localPosition = obj.transform.localPosition + (Vector3)offset;
            //gameObject.transform.localRotation = obj.transform.localRotation;

            gameObject.transform.localScale = obj.transform.lossyScale;
            gameObject.transform.position = obj.transform.position + (Vector3)offset;
            gameObject.transform.rotation = obj.transform.rotation;
        }
    }


}
