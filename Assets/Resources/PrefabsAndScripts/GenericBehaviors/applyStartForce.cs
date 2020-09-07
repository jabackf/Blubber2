using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Applies the specified force / torque at the start()

[RequireComponent(typeof(Rigidbody2D))]
public class applyStartForce : MonoBehaviour
{
    [System.Serializable]
    public enum forceTransformMultipliers
    {
        None,
        TransformUp,
        TransformDown,
        TransformLeft,
        TransformRight

    }
    public forceTransformMultipliers forceTransformMultipler = forceTransformMultipliers.None;

    public ForceMode2D mode = ForceMode2D.Impulse;
    public Vector3 force=new Vector3(1f,1f,0f);
    public Vector3 forceRandomize = new Vector3(0.1f, 0.1f, 0f); //Use it to apply some randomization to each axis of the force. Higher value is more random. 0 is no randomization.
    public float torqueMax=1f, torqueMin = -1f;

    // Start is called before the first frame update
    void Start()
    {
        applyForce();
    }

    public void applyForce()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (forceTransformMultipler == forceTransformMultipliers.TransformUp)
            force = Vector3.Scale(force, transform.up);
        if (forceTransformMultipler == forceTransformMultipliers.TransformDown)
            force = Vector3.Scale(force, -transform.up);
        if (forceTransformMultipler == forceTransformMultipliers.TransformLeft)
            force = Vector3.Scale(force, -transform.right);
        if (forceTransformMultipler == forceTransformMultipliers.TransformRight)
            force = Vector3.Scale(force, transform.right);

        force.x += Random.Range(-forceRandomize.x, forceRandomize.x);
        force.y += Random.Range(-forceRandomize.y, forceRandomize.y);
        force.z += Random.Range(-forceRandomize.z, forceRandomize.z);

        rb.AddForce(force, mode);

        rb.AddTorque(Random.Range(torqueMin, torqueMax), mode);
    }
}
