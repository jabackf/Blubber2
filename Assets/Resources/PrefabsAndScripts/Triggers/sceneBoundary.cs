﻿using System.Collections;
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
