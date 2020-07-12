using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyOnOutOfBounds : MonoBehaviour
{
    private sceneBoundary bound;
    public string sceneBoundaryTag = "sceneBoundary";
    public Vector2 buffer = new Vector2(-1f, -1f);

    // Start is called before the first frame update
    void Start()
    {
        getBoundObject();
    }

    void getBoundObject()
    {
        GameObject bgo = GameObject.FindWithTag(sceneBoundaryTag);
        if (bgo) bound = bgo.GetComponent<sceneBoundary>() as sceneBoundary;
    }

    // Update is called once per frame
    void Update()
    {
        if (bound == null)
        {
            getBoundObject();
        }
        else
        {
            if (bound.outOfBounds(gameObject.transform, buffer.x, buffer.y)) Destroy(gameObject);
        }
    }
}
