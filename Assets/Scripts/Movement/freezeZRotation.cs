using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Freezes the z rotation to zValue

public class freezeZRotation : MonoBehaviour
{
    public float zValue = 0f;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
       gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, zValue) ;
    }
}
