using UnityEngine;

public class VerletRopeNode : MonoBehaviour
{
    public bool drawPreviousVector = true;
    LineRenderer line;
    public Vector3 PreviousPosition;

    void Start()
    {
        if (drawPreviousVector)
        {
            line=gameObject.AddComponent<LineRenderer>();
            line.startWidth = line.endWidth = 0.05f;
            line.SetPosition(0,transform.position);
            line.SetPosition(1,PreviousPosition);
            line.positionCount = 2;
        }
    }

    //This is for debug purposes. It should be called by the rope object
    public void updateLine()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, PreviousPosition);
    }

    
}
