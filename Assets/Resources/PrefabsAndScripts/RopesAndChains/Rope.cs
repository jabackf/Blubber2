using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    //Hinge connection pattern: AnchorA <-Node1  <-Node2  <-Node3  ...  <-NodeN->  AnchorB

    public GameObject AnchorA, AnchorB, Node;

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

        float step = anchorDistance / nodeCount;
        Vector3 dir = AnchorB.transform.position - AnchorA.transform.position;
        float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg)+90;
        Quaternion q = Quaternion.Euler(0f, 0f, angle);
        //Destroy(AnchorA.GetComponent<HingeJoint2D>()); //We don't need the hinge joint on the first anchor.
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
            }
            else
            {
                hj.connectedBody = nodes[i - 1].GetComponent<Rigidbody2D>();
                hj.connectedAnchor = nodes[nodes.Count - 1].transform.position;
            }

            nodes.Add(n);
        }

        //Add extra hinge joint from nodeN to AnchorB
        hjB = nodes[nodes.Count - 1].AddComponent<HingeJoint2D>();
        hjB.connectedBody = AnchorB.transform.parent.gameObject.GetComponent<Rigidbody2D>();
        hjB.connectedAnchor = AnchorB.transform.position;
        nodes.Add(AnchorB);

        //If we don't do this, then nasty things will happen. Like hinges drifting away from their anchor points. I have no idea why (or what this option actually does, really), but this seems to fix it.
        hjA.autoConfigureConnectedAnchor = hjB.autoConfigureConnectedAnchor = false;

        myDJ = AnchorA.transform.parent.gameObject.AddComponent<DistanceJoint2D>();
        myDJ.autoConfigureDistance = false;
        myDJ.autoConfigureConnectedAnchor = false;
        myDJ.distance = anchorDistance;
        myDJ.maxDistanceOnly = true;
        myDJ.breakForce = Mathf.Infinity;
        myDJ.breakTorque = Mathf.Infinity;
        myDJ.enableCollision = true;
        myDJ.connectedBody = AnchorB.transform.parent.GetComponent<Rigidbody2D>();
    }
}
