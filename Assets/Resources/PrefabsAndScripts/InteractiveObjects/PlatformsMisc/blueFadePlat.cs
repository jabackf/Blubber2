using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blueFadePlat : MonoBehaviour
{

    public bool invisible = false; //Check on or off to start visible / invisible
    public float fadeSpeed = 0.7f;
    public float timeBetweenCycles = 2f;

    private float alpha = 1;
    private float timer = 0f;
    private SpriteRenderer renderer;
    private Color color;
    private bool fadeDirectionIn = false;
         
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        color = renderer.color;
        if (invisible)
        {
            alpha = 0;
            fadeDirectionIn = true;
        }
        else
        {
            alpha = 1;
            fadeDirectionIn = false;
        }

        timer = timeBetweenCycles;

        color.a = alpha;
        renderer.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if (!fadeDirectionIn)
            {
                alpha -= fadeSpeed * Time.deltaTime;
                if (alpha<=0)
                {
                    alpha = 0;
                    fadeDirectionIn = true;
                    timer = timeBetweenCycles;
                }
            }
            else
            {
                alpha += fadeSpeed * Time.deltaTime;
                if (alpha >= 1)
                {
                    alpha = 1;
                    fadeDirectionIn = false;
                    timer = timeBetweenCycles;
                }
            }
            color.a = alpha;
            renderer.color = color;

            if (alpha <= 0)
            {
                if (!invisible) SetAllCollidersStatus(false);
                invisible = true;
            }
            else
            {
                if (invisible) SetAllCollidersStatus(true);
                invisible = false;
            }
        }
    }

    public void SetAllCollidersStatus(bool active)
    {
        foreach (Collider2D c in GetComponents<Collider2D>())
        {
            c.enabled = active;
        }
    }
}
