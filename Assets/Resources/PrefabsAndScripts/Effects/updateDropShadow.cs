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

    private SpriteRenderer renderer;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>() as SpriteRenderer;
    }

    void LateUpdate()
    {
        if (!obj || !renderer) Destroy(gameObject);
        else
        {
            renderer.enabled = objSpriteRenderer.enabled;
            renderer.flipX = objSpriteRenderer.flipX;
            renderer.flipY = objSpriteRenderer.flipY;
            renderer.sprite = objSpriteRenderer.sprite;

            //update the position and rotation of the sprite's shadow with moving sprite
            gameObject.transform.localPosition = obj.transform.localPosition + (Vector3)offset;
            gameObject.transform.localRotation = obj.transform.localRotation;
            gameObject.transform.localScale = obj.transform.localScale;
        }
    }


}
