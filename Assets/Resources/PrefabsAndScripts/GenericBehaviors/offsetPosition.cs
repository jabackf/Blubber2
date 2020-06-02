using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script simply applies an offset vector to the object

public class offsetPosition : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position += offset;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
