﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//The scene settings class contains various settings for every scene.
//The map system keeps a reference to the current scene settings, and can be accessed through global.map.settings

public class sceneSettings : MonoBehaviour
{
    [Header("Message")]
    public string bottomMessage = "";

    //Settings for the drop shadow system
    //NOTE: When using the drop shadow system, this object must be notified of new objects that are created. Otherwise these new objects will not have a drop shadow.
    //You can notify sceneSettings by calling objectCreated(GameObject) and passing the newly created object.
    //Current Drop Shadow Glitches: Player eyes shadow not showing in the right place, so the character dress was just added to ignoretags
    [Space]
    [Header("Drop Shadow")]

    public bool dsEnabled = true;
    public LayerMask dsLayers;
    //A list of tags for the drop shadow system to ignore. Objects with these tags will not get a drop shadow.
    public string[] dsIgnoreTags = { "IgnoreDropShadow","ParticleEffects", "ActionIcon", "CharacterDress" }; 
    public Vector2 dsOffset = new Vector2(-0.1f, -0.1f); //Possibly adjust this in realtime by selecting a light source?
    public Material dsMaterial;
    public string dsSortingLayerName = "DropShadow";

    // Start is called before the first frame update
    void Start()
    {
        //Start attaching drop shadows to stuff
        if (dsEnabled)
        { 

            object[] obj = GameObject.FindSceneObjectsOfType(typeof(GameObject));
            foreach (object o in obj)
            {
                GameObject g = (GameObject)o;
                addDropShadow(g);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //You can use this function to notify the scene management system that you just created an object that it should know about.
    //If you don't let the scene manager know, then some scene settings (like drop shadow) may not be applied to it.
    public void objectCreated(GameObject obj)
    {
        //Add a drop shadow if needed
        if (dsEnabled)
        {
            addDropShadow(obj);
        }
    }

    //This adds a drop shadow (if object should have one) to the gameobject. 
    public void addDropShadow(GameObject g)
    {
        if (HasLayer(dsLayers, g.layer))
        {
            bool ignore = false;
            foreach(string tag in dsIgnoreTags)
            {
                if (tag == g.tag) ignore = true;
            }
            if (!ignore)
            {
                SpriteRenderer spriteRenderer = g.GetComponent<SpriteRenderer>();
                if (!spriteRenderer)
                {
                    addDropShadowTilemap(g);
                }
                else
                {
                    //create a drop shadow
                    addDropShadowSprite(g, spriteRenderer);
                }
            }
        }
    }

    //This function applies a drop shadow to an object that has a sprite renderer
    //The following two functions are for specific types (sprite or tilemap). Use the above function if the type of renderer is unknown
    public void addDropShadowSprite(GameObject g, SpriteRenderer spriteRenderer)
    {
        GameObject shadowGameobject = new GameObject("Shadow_" + g.name);
        SpriteRenderer shadowSpriteRenderer = shadowGameobject.AddComponent<SpriteRenderer>();
        shadowSpriteRenderer.sprite = spriteRenderer.sprite;
        shadowSpriteRenderer.material = dsMaterial;
        shadowSpriteRenderer.sortingLayerName = dsSortingLayerName;
        shadowSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

        updateDropShadow uds = shadowGameobject.AddComponent<updateDropShadow>();
        uds.obj = g;
        uds.objSpriteRenderer = spriteRenderer;
        uds.offset = dsOffset;
    }
    public void addDropShadowTilemap(GameObject g)
    {
        TilemapRenderer gMapRenderer = g.GetComponent<TilemapRenderer>();
        if (!gMapRenderer) return;

        GameObject shadowGameobject = Instantiate(g);
        shadowGameobject.name = "Shadow_" + g.name;
        TilemapRenderer shadowMapRenderer = shadowGameobject.GetComponent<TilemapRenderer>();
        Destroy(shadowGameobject.GetComponent<TilemapCollider2D>()); //Don't want the collider

        //A tilemap must be a child of a grid component. However, we want the grid to be offset.
        GameObject grid = new GameObject("Shadow_Grid");
        CopyComponent(g.transform.parent.gameObject.GetComponent<Grid>(), grid);
        shadowGameobject.transform.parent = grid.transform;
        grid.transform.position = g.transform.parent.gameObject.transform.position;
        grid.transform.position += new Vector3(dsOffset.x, dsOffset.y, 0);


        shadowMapRenderer.material = dsMaterial;
        shadowMapRenderer.sortingLayerName = dsSortingLayerName;
        shadowMapRenderer.sortingOrder = gMapRenderer.sortingOrder - 1;
    }


    //This function checks the specified LayerMask to see if it contains the specified layer
    private static bool HasLayer(LayerMask layerMask, int layer)
    {
        if (layerMask == (layerMask | (1 << layer)))
        {
            return true;
        }

        return false;
    }

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
}