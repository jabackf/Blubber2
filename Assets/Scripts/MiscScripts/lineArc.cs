using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Draws a throwing arc. If velocity, angle or resolution are called, then call CalculateArc to recalculate the line curve.

public class lineArc : MonoBehaviour
{
    public LineRenderer lr;

    public float velocity = 10;
    public float angle=45;
    public int resolution = 15;

    public Vector2 offset=new Vector2(0,0);

    public string sortingLayerName = "InGameGUI";

    public float g;

    private float radianAngle;

    void Awake()
    {
        lr = gameObject.AddComponent<LineRenderer>() as LineRenderer;
        
        g = Mathf.Abs(Physics2D.gravity.y);
        lr.sortingLayerName = sortingLayerName;
    }

    void OnValidate()
    {
        CalculateArc();
    }

    // Start is called before the first frame update
    void Start()
    {
        CalculateArc();
    }

    public void CalculateArc()
    {
        lr.SetVertexCount(resolution + 1);
        lr.SetPositions(CalculateArcArray());
    }

    Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];
        radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(t, maxDistance);
        }

        return arcArray;
    }

    Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = (t * maxDistance)+offset.x;
        float y = (x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle))))+offset.y;
        return new Vector3(x, y);
    }
}
