using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class actionIcon : MonoBehaviour
{
    private SpriteRenderer renderer;
    private bool visible;
    private float fade = 0;

    // Start is called before the first frame update
    void Start()
    {
        renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        setVisible(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (visible && fade < 0.6f)
            fade += Time.deltaTime * 5;
        if (!visible && fade >0)
            fade -= Time.deltaTime * 5;

        renderer.color = new Color(1f, 1f, 1f, fade);

    }

    public void setVisible(bool vis)
    {
        visible = vis;
    }
}
