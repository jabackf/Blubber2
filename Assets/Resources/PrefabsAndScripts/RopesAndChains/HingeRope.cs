using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HingeRope : MonoBehaviour
{
    //Hinge connection pattern: AnchorA <-Node1  <-Node2  <-Node3  ...  <-NodeN->  AnchorB

    public GameObject AnchorA, AnchorB, Node;


    //Different types of collision detection. A capsule collider should already be added to the node, and is used to determine the node's dimensions
    //Sometimes capsule colliders can phase through objects if the rope stretches to much. I've added edge colliders to make this less likely.
    //If selected, the capsule collider will be destroyed and list of EdgeCollider2Ds will be used instead. These edges will be updated every frame. None is no collision detection.
    public enum collisionTypes { Capsule, Edge, None};
    public collisionTypes collisionType = collisionTypes.Edge;
    private List<EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>(); //If we're using edge colliders, this holds a list of them
    public bool ignoreAnchorCollisions = true; //If true, the rope will not collide with the anchors
    public bool ignoreAnchorChildCollisions = true; //If true and ignoreAnchorCollisions is true, ignoreCollisions will be applied to both nodes/anchors and nodes/anchorChildren
    public bool enableLineRenderer = true;
    public LineRenderer line;
    public bool drawNodeSprites = true; //Turn off to disable node sprites

    public float angleLimitMin=-150, angleLimitMax = 150;

    private HingeJoint2D hjA, hjB; //The hingejoints that connect Node1->AnchorA and NodeN->AnchorB
    private DistanceJoint2D myDJ;

    private int nodeCount = 0; //Does not include anchorA and B, so full count is nodeCount+2

    private List<GameObject> nodes = new List<GameObject>(); //A list of all nodes. 0=AnchorA, last element=AnchorB

    private float nodeLength; //The length of one node, as determined by the height of the node's capsule collider

    // Start is called before the first frame update
    void Start()
    {

        //Get the length of one node. Nodes sprites are assumed to be oriented vertically, so the length of a node is the height of the node's capsule collider.
        nodeLength = Node.GetComponent<CapsuleCollider2D>().size.y;

        float anchorDistance = Vector3.Distance(AnchorA.transform.position, AnchorB.transform.position);

        nodeCount = (int)Math.Ceiling(anchorDistance / nodeLength);

        JointAngleLimits2D limits; //Will be used for setting stuff up
        float step = anchorDistance / nodeCount;
        float ropeLength = nodeCount * nodeLength; //This is the actual full length of the rope, which will be slightly longer than AnchorDistance because we used Math.Ceiling to create NodeCount
        Vector3 dir = AnchorB.transform.position - AnchorA.transform.position;
        float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg)+90;
        Quaternion q = Quaternion.Euler(0f, 0f, angle);
        nodes.Add(AnchorA);

        for (int i = 1; i <= nodeCount; i++)
        {
            Vector3 pos = Vector3.MoveTowards(AnchorA.transform.position, AnchorB.transform.position, step * i);
            GameObject n = Instantiate(Node, pos, q);
            n.SetActive(true);
            HingeJoint2D hj = n.GetComponent<HingeJoint2D>();

            if (i == 1) //The first node needs to connect to AnchorA
            {
                hjA = hj;
                hjA.connectedBody = nodes[i - 1].transform.parent.gameObject.GetComponent<Rigidbody2D>();
                hjA.connectedAnchor = AnchorA.transform.position;
                hjA.useLimits = true;
                limits = hjA.limits;
                limits.min = angleLimitMin;
                limits.max = angleLimitMax;
                hjA.limits = limits;
            }
            else
            {
                hj.connectedBody = nodes[i - 1].GetComponent<Rigidbody2D>();
                hj.connectedAnchor = nodes[i - 1].transform.position;
                hj.useLimits = true;
                limits = hj.limits;
                limits.min = angleLimitMin;
                limits.max = angleLimitMax;
                hj.limits = limits;
            }

            if (!drawNodeSprites) n.GetComponent<SpriteRenderer>().enabled = false;

            if (collisionType==collisionTypes.Edge || collisionType==collisionTypes.None)
            {
                Destroy(n.GetComponent<CapsuleCollider2D>());
                if (collisionType == collisionTypes.Edge)
                {
                    EdgeCollider2D e = n.AddComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
                    e.points[0] = nodes[i - 1].transform.position;
                    e.points[1] = n.transform.position;
                    edgeColliders.Add(e);
                }
            }

            nodes.Add(n);
        }

        //Add extra hinge joint from nodeN to AnchorB
        hjB = CopyComponent<HingeJoint2D>(nodes[1].GetComponent<HingeJoint2D>(),nodes[nodes.Count - 1]);
        hjB.connectedBody = AnchorB.transform.parent.gameObject.GetComponent<Rigidbody2D>();
        hjB.connectedAnchor = AnchorB.transform.position;
        hjB.useLimits = true;
        limits = hjB.limits;
        limits.min = angleLimitMin;
        limits.max = angleLimitMax;
        hjB.limits = limits;
        nodes.Add(AnchorB);

        //If we don't do this, then nasty things will happen. Like hinges drifting away from their anchor points. I have no idea why (or what this option actually does, really), but this seems to fix it.
        hjA.autoConfigureConnectedAnchor = hjB.autoConfigureConnectedAnchor = false;

        //Add the final edge collider if needed. Note, the final edge is added to the last node instead of the box. Instead of node to previous node like the rest of them, this is node to anchorB.
        if (collisionType == collisionTypes.Edge)
        {
            EdgeCollider2D e = nodes[nodes.Count - 2].AddComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
            e.points[0] = nodes[nodes.Count - 2].transform.position;
            e.points[1] = AnchorB.transform.position;
            edgeColliders.Add(e);
        }

        
        //Add a distance joint to limit the maximum amount of distance
        myDJ = AnchorA.transform.parent.gameObject.AddComponent<DistanceJoint2D>();
        myDJ.connectedBody = AnchorB.transform.parent.GetComponent<Rigidbody2D>();
        //myDJ.anchor = nodeStartAnchor.position;
        //myDJ.connectedAnchor = nodeEndAnchor.position;
        myDJ.maxDistanceOnly = true;
        myDJ.breakForce = Mathf.Infinity;
        myDJ.breakTorque = Mathf.Infinity;
        myDJ.enableCollision = false;
        myDJ.autoConfigureDistance = false;
        myDJ.autoConfigureConnectedAnchor = false;
        myDJ.distance =  ropeLength * 1.5f; // Vector3.Distance(nodeStartAnchor.position, nodeEndAnchor.position) * 2;
        

        //Handle the ignoreAnchorCollision stuff
        if (ignoreAnchorCollisions && collisionType != collisionTypes.None)
        {
            foreach (var n in nodes)
            {
                ignoreCollide(n, AnchorA.transform.parent.gameObject, ignoreAnchorChildCollisions);
                ignoreCollide(n, AnchorB.transform.parent.gameObject, ignoreAnchorChildCollisions);
            }
        }

        //It's line renderer setup time!
        if (enableLineRenderer && !line) line = gameObject.GetComponent<LineRenderer>();
        if (enableLineRenderer && line)
        {
            line.positionCount = nodeCount+1;
            updateLine();
        }
    }

    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    void updateLine()
    {
        for (int i = 0; i <= nodeCount; i++)
        {
            line.SetPosition(i, new Vector3(nodes[i].transform.position.x, nodes[i].transform.position.y));
        }
    }

    void ignoreCollide(GameObject A, GameObject B, bool ignoreChildrenToo = false)
    {
        foreach (var ca in A.GetComponents<Collider2D>())
        {
            foreach (var cb in B.GetComponents<Collider2D>())
            {
                Physics2D.IgnoreCollision(ca, cb);
            }
        }

        if (ignoreChildrenToo)
        {
            foreach (var ca in A.GetComponentsInChildren<Collider2D>())
            {
                foreach (var cb in B.GetComponentsInChildren<Collider2D>())
                {
                    Physics2D.IgnoreCollision(ca, cb);
                }
            }
        }
    }

    void Update()
    {
        //Update all edge colliders
        if (collisionType==collisionTypes.Edge)
        {
            for (int i = 1; i <= nodeCount; i++) //Cycle through all nodes. First node (anchorA) does not have edge collider, so corresponding edges are edgeCollider[nodeIndex-1]
            {
                edgeColliders[i - 1].points[0] = nodes[i - 1].transform.position;
                edgeColliders[i - 1].points[1] = nodes[i].transform.position;
            }
        }

        if (enableLineRenderer && line) updateLine();
    }
}
