using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sceneBoundary : MonoBehaviour
{

    public Transform top, bottom, left, right;
    public class collision
    {
        public GameObject go;
        public string side;  //left, right, top, or bottom

        public collision(GameObject go, string side)
        {
            this.go = go;
            this.side = side;
        }
    }

    private List<collision> currentCollisions = new List<collision>();

    public bool drawBoundaries = false; //Use for debugging. Draws a line around the boundaries
    private LineRenderer boundLine = null;

    void Start()
    {
        if (drawBoundaries)
        {
            boundLine = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
            Invoke("setupBoundLine", 0.1f);
        }
    }

    //Sets up the line renderer that draws the boundary lines for debugging purposes. Turn on drawBoundaries to use
    void setupBoundLine()
    {
        boundLine.widthMultiplier = 0.08f;
        boundLine.positionCount = 4;
        boundLine.loop = true;
        boundLine.startColor = boundLine.endColor = Color.red;
        Vector3[] points = new Vector3[4];
        points[0].x = getLeftX(); points[0].y = getTopY();
        points[1].x = getRightX(); points[1].y = getTopY();
        points[2].x = getRightX(); points[2].y = getBottomY();
        points[3].x = getLeftX(); points[3].y = getBottomY();
        points[0].z = points[1].z = points[2].z = points[3].z = 0f;
        boundLine.SetPositions(points);
    }

    //Checks if the specified object is outside one of the room boundaries. Returns "top" "left" "right" "bottom" or "none"
    //Optional buffer can be added to the interior of the scene boundaries to shrink it
    public string boundaryCheck(GameObject obj, float bufferX=0f, float bufferY=0f)
    {
        if (!obj) return "none";
        float xpos = obj.transform.position.x;
        float ypos = obj.transform.position.y;
        if (xpos > getRightX()-bufferX) return "right";
        if (xpos < getLeftX()+bufferX) return "left";
        if (ypos > getTopY()-bufferY) return "top";
        if (ypos < getBottomY()+bufferY) return "bottom";
        return "none";
    }
    public string boundaryCheckTransform(Transform obj, float bufferX = 0f, float bufferY = 0f)
    {
        if (!obj) return "none";
        float xpos = obj.position.x;
        float ypos = obj.position.y;
        if (xpos > getRightX() - bufferX) return "right";
        if (xpos < getLeftX() + bufferX) return "left";
        if (ypos > getTopY() - bufferY) return "top";
        if (ypos < getBottomY() + bufferY) return "bottom";
        return "none";
    }

    public bool outOfBounds(Transform obj, float bufferX = 0f, float bufferY = 0f)
    {
        if (!obj) return false;
        float xpos = obj.position.x;
        float ypos = obj.position.y;
        if (xpos > getRightX() - bufferX) return true;
        else if (xpos < getLeftX() + bufferX) return true;
        else if (ypos > getTopY() - bufferY) return true;
        else if (ypos < getBottomY() + bufferY) return true;
        return false;
    }

    //Returns the leftmost side of the screen
    public float getLeftX()
    {
        return left.position.x;
    }
    public float getRightX()
    {
        return right.position.x;
    }
    public float getTopY()
    {
        return top.position.y;
    }
    public float getBottomY()
    {
        return bottom.position.y;
    }
}
