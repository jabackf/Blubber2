using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script simply destroys the object after a given time

public class destroyTimer : MonoBehaviour
{
    public float timer = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timer);
    }

}
