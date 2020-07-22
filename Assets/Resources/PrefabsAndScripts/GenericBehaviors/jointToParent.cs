using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Creates a fixed joint and affixes the object to the parent object. Parent requires Rigidbody2D!

public class jointToParent : MonoBehaviour
{
    FixedJoint2D joint;
    GameObject parent;

    public float breakForce = -1; //-1 for infinity
    public float breakTorque = -1; //-1 for infinity

    // Start is called before the first frame update
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = parent.GetComponent<Rigidbody2D>();
            joint.breakForce = (breakForce == -1 ? Mathf.Infinity : breakForce);
            joint.breakTorque = (breakTorque == -1 ? Mathf.Infinity : breakTorque);
        }
    }

}
