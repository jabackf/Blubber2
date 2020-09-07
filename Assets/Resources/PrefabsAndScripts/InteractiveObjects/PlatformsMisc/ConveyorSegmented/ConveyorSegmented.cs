using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorSegmented : MonoBehaviour
{
    [System.Serializable]
    public enum directions
    {
        left,
        right
    }

    //An Nth segment basically says "instead of using the specified segment prefab for every single segment, I want to use this prefab for every Nth segment."
    //For example, you can do a vertical conveyor belt with a perpendicular platform attached to every 10th segment to create a mechanical elevator like you might see in a factory.
    [System.Serializable]
    public class NthSegment
    {
        public GameObject Segment; //The segment prefab
        public bool divide = false; //This changes the way n works. If false, we will attach the unique segment every N segments. 
                                    //If true, we will divide the total number of segments by N and attach our unique segment at those 
                                    //locations. For instance, if you only want four unique segments distributed evenly across the conveyor 
                                    //then set N to four and divide to true. Divide is generally what you want to use if you want unique segments
                                    //distributed evenly, though even distribution depends in part on what you are dividing by and rather there is an
                                    //even or odd number of segments
        public int n=10;
        public int offset=0; //This will be added to n to offset it.
    }
    public List<NthSegment> NthSegments = new List<NthSegment>();

    public GameObject RotorA, RotorB, Segment;

    //If you want conveyors that go faster than this, you'll probably have to make some adjustments to prevent glitching.
    [Range(0f, 170f)]
    public float speed = 60;
    public directions direction = directions.right; 

    private float rotorRadius = 0.35f; //The radius from the center of the rotor to the outside of it
    private float radiusOffsetWhenAttached = 0.09f; //When a segments gets attached to a rotor we might want to the radius from which it is attached to shrink a bit. ie, pull it in closer to the rotor while it's attached. This can give us more control over how objects fall off the conveyor and can prevent objects from bunching up from segments rotating and doing and objects hitting the angle of the collider. This number is subtracted from rotorRadius. So if rotorRadius is 0.2 and this value is 0.05, rotorRadius will be 0.15 while the segment is attached to the rotor.
    int endCount = 4; //The number of segments that start out wrapped around each rotor. IE, the segments that are currently rotating around. Adjust this if you change the segment width or notice gaps in the conveyor belt.

    //Different types of collision detection. Collider will use whatever collider is attached to the segment. This option basically does nothing. Edge will add an edge collider between every segment to form a solid line between all segments. Attached colliders will not be disabled, and can be used in conjunction with edge colliders if desired.
    public enum collisionTypes { Collider, Edge };
    public collisionTypes collisionType = collisionTypes.Edge;
    private List<EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>(); //If we're using edge colliders, this holds a list of them
    public bool enableLineRenderer = true; //We can draw a line to represent the conveyor
    public LineRenderer line;
    public bool drawSegmentSprites = true; //Turn off to disable Segment sprites

    private int SegmentCount = 0; //Number of conveyor segments

    private List<GameObject> Segments = new List<GameObject>(); //A list of all Segments.
    private List<Rigidbody2D> SegmentRbs = new List<Rigidbody2D>(); //We'll use this to cache the rigidbodies of each segment

    private float SegmentLength; //The length of one Segment, as determined by the width of the Segment's capsule collider
    private float step; //This is used to store the step distance between segments. Used for positioning
    private float angle; //Angle between RotorA and B

    private Vector3 rotorATriggerPoint, rotorBTriggerPoint; //This is used to determine when a segment has reached the end of the line and needs to attach to a rotor
    float endTriggerDistanceThreshold = 0.15f; //How close a segment needs to be to the end trigger to attach to the rotor. Smaller values will maintain more uniformity, but could introduce errors where the segment overshoots and passes the rotor entirely, traveling indefinitely off screen. This is more likely to happen at higher speeds.
    int deltaAngleThreshold = 5; //How close we have to be to our target angle before getting off or swapping rotors. If the conveyor is going really fast and segments are getting stuck to the rotors, try raising this number.

    public bool printDebugInfo = false;
    public int debugSegmentNumber = -1; //Enter a segment number here to print it's debug info while it moves. -1 prints no info.

    void Awake()
    {
        //The code assumes that the Rotor with the larger X value is RotorB. If we set the conveyor up wrong and put RotorB to the left of RotorA, then we'll just fix it by swapping the rotors.
        if (RotorB.transform.position.x < RotorA.transform.position.x)
        {
            var r = RotorB;
            RotorB = RotorA;
            RotorA = r;
        }

        //RotorA will stay in lock step with RotorB, with 180 degrees difference. We'll go ahead and add the rotation difference before attaching anything to it.
        RotorA.transform.Rotate(0f, 0f, -180f);

        //Get the length of one Segment. Segments sprites are assumed to be oriented horizontally by default, so the length of a Segment is the size.x of the Segment's sprite.
        SegmentLength = Segment.GetComponent<SpriteRenderer>().size.x;

        float anchorDistance = Vector3.Distance(RotorA.transform.position, RotorB.transform.position);

        SegmentCount = (int)Math.Ceiling(anchorDistance / SegmentLength);

        step = anchorDistance / SegmentCount;
        float conveyorLength = SegmentCount * SegmentLength; //This is the actual full length of the rope, which will be slightly longer than AnchorDistance because we used Math.Ceiling to create SegmentCount
        Vector3 dir = RotorB.transform.position - RotorA.transform.position;
        angle = Angle(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg); //The angle of the whole conveyor belt, or the angle from RotorA to RotorB
        Quaternion q = Quaternion.Euler(0f, 0f, angle); 
        float angleStep = 90 / endCount; //The amount of change in angle for each segment that is attached to a rotor

        GameObject n; //Use this for referencing current segment

        //First we'll add segments that are wrapped around rotorA
        for (int i = endCount; i >0; i--)
        {
            n = Instantiate(Segment, RotorA.transform.position, q);
            n.transform.position += ((rotorRadius- radiusOffsetWhenAttached) * n.transform.up);
            var temp = new GameObject("temp_rotator");
            temp.transform.position = RotorA.transform.position;
            n.transform.parent = temp.transform;
            temp.transform.Rotate(0f, 0f, (angleStep * i) , Space.World);
            n.transform.parent = RotorA.transform;
            Destroy(temp);
            n.SetActive(true);
            if (!drawSegmentSprites) n.GetComponent<SpriteRenderer>().enabled = false;
            else n.GetComponent<SpriteRenderer>().enabled = true;
            Segments.Add(n);
            SegmentRbs.Add(n.GetComponent<Rigidbody2D>());
            n.name = gameObject.name+"_Seg" + Segments.Count;
        }


        //Then we'll add the middle segments
        for (int i = 1; i <= SegmentCount; i++)
        {
            Vector3 pos = Vector3.MoveTowards(RotorA.transform.position, RotorB.transform.position, step * (i-1) );
            n = Instantiate(Segment, pos, q);
            n.transform.position += (rotorRadius * n.transform.up);
            n.SetActive(true);

            if (!drawSegmentSprites) n.GetComponent<SpriteRenderer>().enabled = false;
            else n.GetComponent<SpriteRenderer>().enabled = true;
            Segments.Add(n);
            SegmentRbs.Add(n.GetComponent<Rigidbody2D>());
            n.name = gameObject.name + "_Seg" + Segments.Count;

            //While we're here, we'll get the "trigger points" for each rotor using the first segment that we know is not attached to a rotor. These are the points near the rotors that we need to get close enough to in order to jump off the conveyor line and onto the rotor. They are used when segments reach the end of the line, and can be either rotor depending on the direction.
            if (i == 1)
            {
                rotorATriggerPoint = RotorA.transform.position + (rotorRadius * n.transform.up);
                rotorBTriggerPoint = RotorB.transform.position + (rotorRadius * n.transform.up);
            }
        }

        //Add segments at the end attached to rotorB
        for (int i = endCount; i >=0; i--)
        {
            n = Instantiate(Segment, RotorB.transform.position, q);
            n.transform.position += ((rotorRadius - radiusOffsetWhenAttached) * n.transform.up);
            var temp = new GameObject("temp_rotator");
            temp.transform.position = RotorB.transform.position;
            n.transform.parent = temp.transform;
            temp.transform.Rotate(0f, 0f, (angleStep * i) - 90, Space.World);
            n.transform.parent = RotorB.transform;
            Destroy(temp);
            n.SetActive(true);
            if (!drawSegmentSprites) n.GetComponent<SpriteRenderer>().enabled = false;
            else n.GetComponent<SpriteRenderer>().enabled = true;
            Segments.Add(n);
            SegmentRbs.Add(n.GetComponent<Rigidbody2D>());


            n.name = gameObject.name + "_Seg" + Segments.Count;
        }

        //This needs updated to include rotor segments
        SegmentCount = Segments.Count;

        //It's line renderer setup time!
        if (enableLineRenderer && !line) line = gameObject.GetComponent<LineRenderer>();
        if (enableLineRenderer && line)
        {
            line.positionCount = SegmentCount;
            line.loop = true;
            updateLine();
        }

        //Now let's do the edge colliders, if needed
        if (collisionType == collisionTypes.Edge)
        {
            //If we are using edge colliders, we'll start by adding one between segment0 and the segmentN
            EdgeCollider2D e = Segments[0].AddComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
            e.points[0] = Segments[0].transform.position;
            e.points[1] = Segments[SegmentCount-1].transform.position;
            edgeColliders.Add(e);

            //Then we'll loop through all segments
            for (int i = 1; i < SegmentCount; i++)
            {
                n = Segments[i];
                e = n.AddComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
                e.points[0] = Segments[i - 1].transform.position;
                e.points[1] = n.transform.position;
                edgeColliders.Add(e);
            }
        }

        //Now we will go through and swap some segments for Nth segments
        if (NthSegments.Count>0)
        {
            foreach(var e in NthSegments)
            {
                int nth = e.n;
                if (e.divide && nth!=0) nth = SegmentCount / nth;
                for (int i = e.offset; i<SegmentCount; i+=nth)
                {
                    var newseg = Instantiate(e.Segment);
                    newseg.name = Segments[i].name;
                    newseg.transform.position = Segments[i].transform.position;
                    newseg.transform.rotation = Segments[i].transform.rotation;
                    newseg.transform.parent = Segments[i].transform.parent;
                    Destroy(Segments[i]);
                    Segments[i] = newseg;
                    Destroy(SegmentRbs[i]);
                    SegmentRbs[i] = newseg.GetComponent<Rigidbody2D>();
                    Segments[i].SetActive(true);
                }
            }
        }
    }

    //Call this to change the speed. Speed must be positive.
    public void changeSpeed(float s)
    {
        if (s>=0)
            speed = s;
    }

    //This bit of code copies a component from one gameObject to another
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

    //This updates the line positions
    void updateLine()
    {
        for (int i = 0; i < SegmentCount; i++)
        {
            line.SetPosition(i, new Vector3(Segments[i].transform.position.x, Segments[i].transform.position.y));
        }
    }

    //This clamps the input value to 0 - 360
    float Angle(float eulerAngles)
    {
        float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
        if (result < 0)
        {
            result += 360f;
        }
        return result;
    }

    //This returns the difference between two angles
    float deltaAngle(float a, float b)
    {
        a=Angle(a);
        b = Angle(b);
        if (a > b) return Angle(a - b);
        if (b > a) return Angle(b - a);
        return Angle(a - b);
    }

    void FixedUpdate()
    {
        int i = 0; //Increments to track index of foreach loop

        //Move the segments
        foreach (var s in SegmentRbs)
        {
            var parent = s.gameObject.transform.parent;
            if (parent == RotorA.transform || parent == RotorB.transform)
            {
                float sAngle = s.gameObject.transform.eulerAngles.z;
                GameObject rBegin, rEnd;
                Vector3 endTrigger;
                if (direction == directions.right)
                {
                    rBegin = RotorA;
                    rEnd = RotorB;
                }
                else
                {
                    rBegin = RotorB;
                    rEnd = RotorA;
                }

                if (i == debugSegmentNumber) Debug.Log(s.gameObject.name + " parented to "+parent+". Currently rotating. Position: " + s.gameObject.transform.position + ", Velocity: " + s.velocity + ", eulerAngles.z: " + s.gameObject.transform.eulerAngles.z + ", angle: "+angle+", sAngle: "+sAngle+", rBegin: "+rBegin.name+", rEnd: "+rEnd.name);

                if (parent == rEnd.transform) //Segment is turning downwards at the end, getting ready to loop around to the other side.
                {
                    if (deltaAngle(sAngle, Angle((direction==directions.right ? 270 + angle : 90+angle) )) <= deltaAngleThreshold)
                    {
                        if (i == debugSegmentNumber) Debug.Log(s.gameObject.name + " passed angle threshold at the end. About to loop to the beginning. Position: " + s.gameObject.transform.position + ", eulerAngles.z: " + s.gameObject.transform.eulerAngles.z + ", angle: "+angle+", sAngle: "+sAngle);
                        var temp = new GameObject("convRotator");
                        temp.transform.position = rEnd.transform.position;
                        s.gameObject.transform.parent = temp.transform;
                        temp.transform.Rotate(0f, 0f, 180f, Space.Self);
                        temp.transform.position = rBegin.transform.position;
                        s.gameObject.transform.parent = rBegin.transform;
                        Destroy(temp);
                        if (i == debugSegmentNumber) Debug.Log(s.gameObject.name + " looping to beginning complete. Now starting to rotate around beginning rotor. Position: " + s.gameObject.transform.position + ", eulerAngles.z: " + s.gameObject.transform.eulerAngles.z + ", angle: " + angle + ", sAngle: " + sAngle);
                    }
                }
                else if (parent == rBegin.transform)//Segment is coming up from the bottom, starting it's journey
                {

                    if (deltaAngle(sAngle, angle) <= deltaAngleThreshold)
                    {
                        if (i == debugSegmentNumber) Debug.Log(s.gameObject.name + " passed angle threshold at the beginning. Parent is about to be set to null and the segment will start moving towards the end. Position: " + s.gameObject.transform.position + ", eulerAngles.z: " + s.gameObject.transform.eulerAngles.z + ", angle: " + angle + ", sAngle: " + sAngle);
                        s.gameObject.transform.parent = null;
                        s.gameObject.transform.eulerAngles = new Vector3(0f, 0f, angle);
                        s.gameObject.transform.position = (direction == directions.right ? rotorATriggerPoint : rotorBTriggerPoint);

                    }
                }
            }
            else
            {
                s.velocity = s.transform.right * (direction == directions.right ? speed : -speed) * Time.fixedDeltaTime;
                Vector3 endTrigger = (direction == directions.right ? rotorBTriggerPoint : rotorATriggerPoint);

                if (i == debugSegmentNumber) Debug.Log(s.gameObject.name + " unparented. Applying velocity. Direction: " + direction + ", Position: " + s.gameObject.transform.position + ", Velocity: " + s.velocity+", eulerAngles.z: " + s.gameObject.transform.eulerAngles.z+", endTrigger pos: "+endTrigger+ ", Distance to endTrigger: "+Vector3.Distance(s.gameObject.transform.position, endTrigger));

                //The thing can go in either direction, so we'll test to find out what direction we are going and which rotor we will jump to.
                if (Vector3.Distance(s.gameObject.transform.position,endTrigger)<=endTriggerDistanceThreshold)
                {
                    if (i == debugSegmentNumber) Debug.Log(s.gameObject.name + " passed end trigger threshold (" + Vector3.Distance(s.position, endTrigger) + " <= " + endTriggerDistanceThreshold + "). Parenting to " + (direction == directions.right ? RotorB.name : RotorA.name));
                    s.gameObject.transform.position = (s.gameObject.transform.position - ((radiusOffsetWhenAttached) * s.transform.up) ); //Make the segment a little closer to the rotor when attached. See radiusOffsetWhenAttached definition for more info.
                    s.gameObject.transform.parent = (direction == directions.right ? RotorB.transform : RotorA.transform);
                    s.velocity = new Vector3(0f, 0f, 0f);
                }
            }
            i++;
        } //End move segments

        //Spin the rotors. Do this after checking the segments because if we don't, we can introduce glitches where segments stick to the rotors on the first update.
        RotorB.transform.Rotate(0f, 0f, (direction == directions.right ? -speed : speed) * Time.fixedDeltaTime * 3, Space.Self);
        RotorA.transform.eulerAngles = RotorB.transform.eulerAngles - new Vector3(0f, 0f, 180);
    }

    void Update()
    { 
        //Update all edge colliders
        if (collisionType == collisionTypes.Edge)
        {
            edgeColliders[0].points[0] = Segments[0].transform.position;
            edgeColliders[0].points[1] = Segments[SegmentCount-1].transform.position;

            for (int i = 1; i < SegmentCount; i++) //Cycle through all Segments. 
            {
                edgeColliders[i - 1].points[0] = Segments[i - 1].transform.position;
                edgeColliders[i - 1].points[1] = Segments[i].transform.position;
            }
        }

        if (enableLineRenderer && line) updateLine();
    }

    void OnValidate()
    {
        if (printDebugInfo)
        {
            PrintDebugInfo();
        }
    }

    void PrintDebugInfo()
    {
        printDebugInfo = false;
        Debug.Log("Segment Count: " + SegmentCount);
        Debug.Log("RotorA Angle: " + RotorA.transform.eulerAngles.z + ", RotorB Angle: " + RotorB.transform.eulerAngles.z);
        Debug.Log("RotorA TriggerPos: " + rotorATriggerPoint);
        Debug.Log("RotorB TriggerPos: " + rotorBTriggerPoint);
        Debug.Log("Belt Angle: " + angle);

        foreach(var s in Segments)
        {
            Debug.Log(s.name + " Parent: " + s.transform.parent + 
                ", Angle: " + s.transform.eulerAngles.z +
                ", Position: " + s.transform.position);
        }
    }
}
