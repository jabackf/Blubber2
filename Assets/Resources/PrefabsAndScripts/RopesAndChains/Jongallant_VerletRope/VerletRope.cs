using System.Collections.Generic;
using System;
using UnityEngine;

//https://github.com/jongallant/Unity-Verlet-Rope

public class VerletRope : MonoBehaviour
{
    LineRenderer LineRenderer;
    Vector3[] LinePositions;

    public GameObject nodePrefab;

    private List<VerletRopeNode> RopeNodes = new List<VerletRopeNode>();
    public float NodeDistance = 0.2f;
    private int TotalNodes = 50;
    private float RopeWidth = 0.1f;
    private float anchorDistance = 0;

    public float bounce = 0.7f, friction = 0.99f;

    private Vector3 velocity;

    Camera Camera;

    int LayerMask = 1;
    public ContactFilter2D ContactFilter;    
    RaycastHit2D[] RaycastHitBuffer = new RaycastHit2D[10];
    Collider2D[] ColliderHitBuffer = new Collider2D[10];

    public Vector3 Gravity = new Vector2(0f, -5f);

    public Transform nodeStartAnchor, nodeEndAnchor;

    DistanceJoint2D myDJ;

    void Awake()
    {
        Camera = Camera.main;

        LineRenderer = this.GetComponent<LineRenderer>();

        // Generate some rope nodes based on properties
        Vector3 startPosition = Vector2.zero;
        Vector3 endPosition = Vector2.zero - new Vector2(0,NodeDistance*TotalNodes);
        Vector3 dir = Vector3.zero;
        float angle = 270;
        if (nodeStartAnchor) startPosition = nodeStartAnchor.position;
        if (nodeEndAnchor) endPosition = nodeEndAnchor.position;
        if (nodeStartAnchor && nodeEndAnchor)
        {
            anchorDistance = Vector3.Distance(nodeStartAnchor.transform.position, nodeEndAnchor.transform.position);
            TotalNodes = (int)Math.Ceiling(anchorDistance / NodeDistance);
            dir = nodeEndAnchor.transform.position - nodeStartAnchor.transform.position;
            angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);// + 90;
        }

        for (int i = 0; i < TotalNodes; i++)
        {
            Vector3 pos = Vector3.MoveTowards(startPosition, endPosition, NodeDistance * i);

            VerletRopeNode node = GameObject.Instantiate(nodePrefab, pos, Quaternion.identity).GetComponent<VerletRopeNode>();
            node.transform.position = startPosition;
            node.PreviousPosition = startPosition;
            RopeNodes.Add(node);

            startPosition = pos;
        }

        // for line renderer data
        LinePositions = new Vector3[TotalNodes];

        //Add a distance joint to limit the maximum amount of distance
        myDJ = nodeStartAnchor.gameObject.AddComponent<DistanceJoint2D>();
        myDJ.connectedBody = nodeEndAnchor.GetComponent<Rigidbody2D>();
        //myDJ.anchor = nodeStartAnchor.position;
        //myDJ.connectedAnchor = nodeEndAnchor.position;
        myDJ.maxDistanceOnly = true;
        myDJ.breakForce = Mathf.Infinity;
        myDJ.breakTorque = Mathf.Infinity;
        myDJ.enableCollision = false;
        myDJ.autoConfigureDistance = false;
        myDJ.autoConfigureConnectedAnchor = false;
        myDJ.distance = Vector3.Distance(nodeStartAnchor.position, nodeEndAnchor.position) * 2;
    }


    void Update()
    {

        DrawRope();
    }

    private void FixedUpdate()
    {
        Simulate();
                      
        // Higher iteration results in stiffer ropes and stable simulation
        for (int i = 0; i < 80; i++)
        {
            ApplyConstraint();

            if (i % 2 == 1)
                Collisions();

            foreach (var node in RopeNodes) node.updateLine();
        }
    }

    private void Simulate()
    {
        // step each node in rope
        for (int i = 0; i < TotalNodes; i++)
        {            
            // derive the velocity from previous frame
            velocity = RopeNodes[i].transform.position - RopeNodes[i].PreviousPosition;
            RopeNodes[i].PreviousPosition = RopeNodes[i].transform.position;

            // calculate new position
            Vector3 newPos = RopeNodes[i].transform.position + velocity;
            newPos += Gravity * Time.fixedDeltaTime;
            newPos *= friction;
            Vector3 direction = RopeNodes[i].transform.position - newPos;
             
            /*
            // cast ray towards this position to check for a collision
            int result = -1;
            result = Physics2D.CircleCast(RopeNodes[i].transform.position, RopeNodes[i].transform.localScale.x / 2f, -direction.normalized, ContactFilter, RaycastHitBuffer, direction.magnitude);

            if (result > 0)
            {
                for (int n = 0; n < result; n++)
                {                    
                    if (RaycastHitBuffer[n].collider.gameObject.layer == 9)
                    {
                        Vector2 collidercenter = new Vector2(RaycastHitBuffer[n].collider.transform.position.x, RaycastHitBuffer[n].collider.transform.position.y);
                        Vector2 collisionDirection = RaycastHitBuffer[n].point - collidercenter;
                        // adjusts the position based on a circle collider
                        Vector2 hitPos = collidercenter + collisionDirection.normalized * (RaycastHitBuffer[n].collider.transform.localScale.x / 2f + RopeNodes[i].transform.localScale.x / 2f);
                        newPos = hitPos;
                        break;              //Just assuming a single collision to simplify the model
                    }
                }
            }*/

            RopeNodes[i].transform.position = newPos;
        }
    }

    private void Collisions()
    {
        for (int i = 0; i < TotalNodes - 1; i++)
        {
            //Get velocity, apply bounce
            float vx = velocity.x * bounce;
            float vy = velocity.y * bounce;

            int result = -1;
            result = Physics2D.Linecast(RopeNodes[i].transform.position, RopeNodes[i + 1].transform.position, ContactFilter, RaycastHitBuffer);

            if (result > 0)
            {
                for (int n = 0; n < result; n++)
                {
                    //Get closest point on collider. We'll use the previous position because ClosestPoint just returns the point we passed if it's already in contact with the collider.
                    Vector3 cp = RaycastHitBuffer[n].collider.ClosestPoint(RopeNodes[i].PreviousPosition);
                    //Vector3 cp = RopeNodes[i].PreviousPosition;

                    //We want to move slightly outside of the collider because if we don't the rope feels sticky, like one of those sticky-hand things you get out of a quarter vending machine.
                    Vector3 collisionDirection = RopeNodes[i].PreviousPosition - cp;
                    cp += collisionDirection.normalized * 0.07f;

                    //RopeNodes[i].transform.position = cp;
                    //RopeNodes[i].PreviousPosition = new Vector2(RopeNodes[i].transform.position.x + vx, RopeNodes[i].transform.position.y + vy);

                    //Adjust position and old position so we bounce off in the appropriate direction
                    if (cp.x > RopeNodes[i].transform.position.x || cp.x < RopeNodes[i].transform.position.x)
                    {
                        RopeNodes[i].transform.position = new Vector2(cp.x, RopeNodes[i].transform.position.y);
                        //RopeNodes[i].PreviousPosition.x = RopeNodes[i].transform.position.x + vx;
                    }
                    if (cp.y > RopeNodes[i].transform.position.y || cp.y < RopeNodes[i].transform.position.y)
                    {
                        RopeNodes[i].transform.position = new Vector2(RopeNodes[i].transform.position.x, cp.y);
                        //RopeNodes[i].PreviousPosition.y = RopeNodes[i].transform.position.y + vy;
                    }
                }
                
            }
        }
    }
    
    /*private void AdjustCollisions()
    {
        // Loop rope nodes and check if currently colliding
        for (int i = 0; i < TotalNodes - 1; i++)
        {
            VerletRopeNode node = this.RopeNodes[i];

            int result = -1;
            result = Physics2D.OverlapCircleNonAlloc(node.transform.position, node.transform.localScale.x / 2f, ColliderHitBuffer);

            if (result > 0)
            {
                for (int n = 0; n < result; n++)
                {
                    if (ColliderHitBuffer[n].gameObject.layer != 8)
                    {
                        // Adjust the rope node position to be outside collision
                        Vector3 collidercenter = ColliderHitBuffer[n].transform.position;
                        Vector3 collisionDirection = node.transform.position - collidercenter;

                        Vector3 hitPos = collidercenter + collisionDirection.normalized * ((ColliderHitBuffer[n].transform.localScale.x / 2f) + (node.transform.localScale.x / 2f));
                        node.transform.position = hitPos;
                        break;
                    }
                }
            }
        }    
    }*/

    private void ApplyConstraint()
    {
        // Apply start and end anchors
        if (nodeStartAnchor) RopeNodes[0].transform.position = nodeStartAnchor.position;
        if (nodeEndAnchor) RopeNodes[TotalNodes-1].transform.position = nodeEndAnchor.position;

        for (int i = 0; i < TotalNodes - 1; i++)
        {
            VerletRopeNode node1 = this.RopeNodes[i];
            VerletRopeNode node2 = this.RopeNodes[i + 1];

            // Get the current distance between rope nodes
            float currentDistance = (node1.transform.position - node2.transform.position).magnitude;
            float difference = Mathf.Abs(currentDistance - NodeDistance);
            Vector2 direction = Vector2.zero;
           
            // determine what direction we need to adjust our nodes
            if (currentDistance > NodeDistance)
            {
                direction = (node1.transform.position - node2.transform.position).normalized;
            }
            else if (currentDistance < NodeDistance)
            {
                direction = (node2.transform.position - node1.transform.position).normalized;
            }

            // calculate the movement vector
            Vector3 movement = direction * difference;

            // apply correction
            node1.transform.position -= (movement * 0.5f);
            node2.transform.position += (movement * 0.5f);
        }
    }

    private void DrawRope()
    {
        LineRenderer.startWidth = RopeWidth;
        LineRenderer.endWidth = RopeWidth;

        for (int n = 0; n < TotalNodes; n++)
        {
            LinePositions[n] = new Vector3(RopeNodes[n].transform.position.x, RopeNodes[n].transform.position.y, 0);
        }

        LineRenderer.positionCount = LinePositions.Length;
        LineRenderer.SetPositions(LinePositions);
    }

}
