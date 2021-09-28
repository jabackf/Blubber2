using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

/**
 * Creates a segmented conveyor belt. Belt is highly customizable. Can have rotors, segment variation, different kinds of movement, etc.
 * Belt paths are defined with pathCreator. Open or closed paths can be used.
 * 
 * TYPES OF MOVEMENT
 * 
 * Speed: This is a normal smooth, continuous conveyor belt.
 * 
 * timedJump: Segments jump instantaneusly by moveDistance using MovePosition. Jumps occur on a timer (segmentUpdateTime).
 * 
 * lerp: This setting lerps the segments by the specified distance. The timer is set to segmentUpdateTime when all segments have finished lerping.
 * This can be used to create a belt that iterates in steps. Meaning it moves ahead a bit, stops, then moves ahead some more.
 * movementDistance can be used if you want to lerp a distance greater than one segment. For example, if waypointsPerSegment is set to one
 * and movementDistances is set to two, we will lerp a distance of three full segments one each cycle.
 * With this setting, speed adjusts the amount of time it takes to complete the lerp.
 * 
 * SmoothDamp: Similar to lerp, but uses SmoothDamp instead.
 * 
 * lerpOnCommand / smoothDampOnCommand is just a special case use of lerp/damp where the timer is taken out of the equation. Instead you have to manually
 * lerp the belt by calling advanceLeft() or advanceRight(). For example, this can be used to create buttons that the player can use to control the conveyor
 * 
 * Stationary does nothing. The belt doesn't move. Setting to this mode stops the best immediately. If you are in the middle of a lerp/damp, then the values are not reset and the lerp/damp will be completed
 * when the belt is put back into lerp/damp mode. To wait for the lerp to complete THEN switch to stationary, call the stop() method.
 * 
 * NTH SEGMENTS
 * 
 * Nth Segments can be used to swap out segments for other objects OR child other objects to every nth segment. For example, every 5th segment on the belt can be swapped out for a special segment
 * containing a perpendicular platform. If you orient such a belt vertically then you now have an elevator with platforms that go up
 * on one side, then loop around and go down on the other.
 */

public class ConveyorSegmented : MonoBehaviour
{
    [System.Serializable]
    public enum segmentRotationTypes
    {
        alwaysFaceOutwards, //Segment's transform.up always face outwards, away from the rotors.
        alwaysInvert, //Segment's transform.up always faces inwards, towards from the rotors.
        invertOnLeft, //Segment's transform.up face inwards on left direction, outwards on right direction
        invertOnRight //Segment's transform.up face outwards on left direction, inwards on right direction
    }
    public enum movementTypes
    {
        speed,
        timedJump,
        lerp,
        lerpOnCommand,
        smoothdamp,
        smoothdampOnCommand,
        stationary
    }
	public enum movementFunctions
    {
        MovePosition,
		Position,
		Velocity
    }
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
        public bool replaceExistingSegment = true; //If set to true, the conveyor's segment for this position will be destroyed and replaced with this nth segment.
                                                   //If set to false, the nth segment will be instantiated as a child of the conveyor's segment.
        public bool active = true; //Toggles the nth segment on or off.
        public bool divide = false; //This changes the way n works. If false, we will attach the unique segment every N segments. 
                                    //If true, we will divide the total number of segments by N and attach our unique segment at those 
                                    //locations. For instance, if you only want four unique segments distributed evenly across the conveyor 
                                    //then set N to four and divide to true. Divide is generally what you want to use if you want unique segments
                                    //distributed evenly, though even distribution depends in part on what you are dividing by and rather there is an
                                    //even or odd number of segments
        public int n = 10;
        public int offset = 0; //This will be added to n to offset it.

        //Stores a list of all nth segment gameobjects
        [HideInInspector] public List<GameObject> objectList = new List<GameObject>();
    }


    [System.Serializable]
    public class Segment
    {
        public GameObject gameObject;
        public Rigidbody2D rb;
        public Vector3 prevPos; //Stores the previous position of the segment
        public int rightSegment, leftSegment;
        public float distance; //The distance along the path that we are currently at
        public Quaternion targetRotation; //This is used to store the rotation for the segment. It is separated from transform.rotation so we can lerp.
        public float smoothRefVelocity = 0; //Used for smoothdamping
    }


    public GameObject SegmentPrefab;
    public PathCreator pathCreator;
    public float segmentDistanceMultiplier = 1f; //This multiplies segmentLength. It can effectively be used to change the distance between segments. 1 = 1 segment length apart.


    public List<NthSegment> NthSegments = new List<NthSegment>();
    public movementTypes movementType = movementTypes.speed;
	public movementFunctions movementFunction = movementFunctions.MovePosition;	//This is the type of function used to move the segments. 
    public int maxSegments = 800; //The maximum number of segments we are allowed to have. Primarily used in case a glitch happens during segment instantiation and the segments try to go on forever, getting us stuck in an infinite loop. 

    public GameObject beltCaps; //If an object is specified (and the belt is open ended), this object will be duplicated and placed at each end of the belt.

    //These are used to control lerpOnCommand. If you're wanting to manually lerp the belt, you can do so from script by calling advanceLeft() or advanceRight(),
    //OR you can set either one of these to true (which is good for testing in the editor). The lerping code will set them back to false after it receives the command to lerp.
    [SerializeField] private bool advanceLeftBool = false;
    [SerializeField] private bool advanceRightBool = false;

    [SerializeField] private bool stopBool = false; //This will call the stop() method


    [Header("Speed and Timing")]
    //If you want conveyors that go faster than this, you'll probably have to make some adjustments to prevent glitching.
    [Range(0f, 100f)]
    public float speed = 50;

    [Range(0f, 4f)]
    public float segmentUpdateTime = 0.02f; //How often the belt segments update. If you're using timed jump, this should be very small (~0.02). If you're using smoothdamp, this should be large enough for the segments to completely move to the next waypoint.
    private float timer = 0f;
    public float smoothDampTimer = 0.5f; //This is the time it takes to complete a smoothdamp.
    public float movementDistance = 4; //This controls This controls how far we lerp or jump. 1 = 1 segment length. Floating point values can be used for fractions of a segment.
    private Vector3 lerpTarget; //This is used to store the next target position (for Segment[0]) for lerping / damping. 
    private float lerpTargetDistance; //Stores the full distance in path units that we are traveling to the end of the current lerp. This is NOT relative to any segment, and travels from 0 to the full movement distance of a single lerp (positive or negative based on direction). Recalculated at the start of each lerp.
    private float lerpSourceDistance = 0f; //Stores the current lerp progress. Starts at zero at the beginning of each lerp.
    private float segmentZeroLerpTargetDistance; //Stores the target distance for each lerp, but this one is relative to segment 0
    public float finishLerpBuffer = 0.02f; //This is how close the source distance needs to be to the target distance before the segments are snapped into the new destination and the lerp cycle is complete.
    public directions direction = directions.right;
    private float lerpVelocity = 0f;
    private bool pauseTimer = false; //Stops the timer from counting down. Pauses movement for timed jump. For lerping it is used internally to pause the timer until the segments have almost finished moving.

    private bool goStationaryAtEndOfLerp = false; //When true, the belt will go into stationary mode at the end of a lerp movement. Used by the stop() method.
    private bool firstLerp = true; //This is set to false the first time we attempt to lerp a segement. Used internally to prevent the segments from moving when the game starts.

    [Header("Segment Rotation")]
    public bool rotateSegments = false; //If false, the only rotation applied will be segmentRotationOffset
	public float rotateSegmentInDirection = 0f; //Adds or subtracts this rotation to each segment depending on direction. This will allow you to, for example, make the segments tilt backwards when the belt moves forward.
    public bool lerpSegmentRotation = true; //If true, then segment rotation will lerp to the belt normal by segmentLerpRotSpeed
    public float segmentLerpRotSpeed = 200f; //How fast we rotate, if lerpSegmentRotation is true
    public float segmentRotationOffset = -90; //This is added to the rotation of the segment. Our rotation will be the belt normal plus this value.
    public segmentRotationTypes segmentRotationType = segmentRotationTypes.alwaysFaceOutwards;


    [Header("Rotors")]
    public GameObject RotorContainer;
    private List<GameObject> Rotors = new List<GameObject>();
    public bool rotorsActive = true; //When false, rotors are deactivated and invisible
    private bool rotorsOn = true; //Rotor rotation is not updated when this is off. Note, this variable is toggled automatically in lerp/damp modes to stop the rotors when the segments aren't moving.
    public float rotorSpinMultiplier = 6f; //Controls how fast the rotors spin. Doesn't actually impact belt speed.



    [Header("Collision, Sprites, Lines")]
    public bool enableLineRenderer = true; //We can draw a line to represent the conveyor
    public LineRenderer line;
    public bool drawSegmentSprites = true; //Turn off to disable Segment sprites
    //Different types of collision detection. Nothing will do nothing (you can use whatever collider is attached to the segment). Edge will add an edge collider between every segment to form a solid line between all segments. Attached colliders will not be disabled, and can be used in conjunction with edge colliders if desired.
    public enum collisionTypes { Nothing, Edge };
    public collisionTypes collisionType = collisionTypes.Nothing;
    private List<EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>(); //If we're using edge colliders, this holds a list of them


    private List<Segment> Segments = new List<Segment>(); //A list of all Segments.

    private float SegmentLength; //The length of one Segment, as determined by the width of the Segment's capsule collider

    [Header("Debug")]

    public bool updateSegments = true; //Set to false to skip all fixed update code that moves segments and turns rotors. 
    bool beltFullyInitialized = false; //This is set to true once the belt with all the segments have been fully setup

    void Start()
    {
        if (RotorContainer)
        {
            foreach (Transform t in RotorContainer.transform)
            {
                Rotors.Add(t.gameObject);
            }
            RotorContainer.SetActive(rotorsActive);
        }


        //Get the length of one Segment. Segments sprites are assumed to be oriented horizontally by default, so the length of a Segment is the size.x of the Segment's sprite.
        SegmentLength = SegmentPrefab.GetComponent<SpriteRenderer>().size.x;

        GameObject n;


        //Create the segments.
        bool done = false;
        float step = 0;

        while (!done)
        {
            float distance = step * SegmentLength * segmentDistanceMultiplier;
            if (distance >= pathCreator.path.length)
            {
                done = true;
                break;
            }


            n = Instantiate(SegmentPrefab, pathCreator.path.GetPointAtDistance(distance), Quaternion.identity);
            var s = new Segment();
            s.gameObject = n;
            s.prevPos = n.transform.position;
            s.gameObject.name = gameObject.name + "_Seg" + Segments.Count;
            s.gameObject.SetActive(true);
            s.rb = n.gameObject.GetComponent<Rigidbody2D>();
            s.distance = distance;
            n.transform.parent = gameObject.transform;
            if (!drawSegmentSprites) n.GetComponent<SpriteRenderer>().enabled = false;
            Segments.Add(s);

            //Get the rotation
            Quaternion q = getSegmentRotation(s);
            s.gameObject.transform.rotation = q;
            s.targetRotation = q;

            if (Segments.Count >= maxSegments) done = true; //This gives us an out in case we glitch out. We don't want an infinite loop.

            step++;
        }

        for (int i = 0; i < Segments.Count; i++)
        {
            var b = Segments[i];
            if (i == Segments.Count - 1) b.rightSegment = 0;
            else b.rightSegment = i + 1;
            if (i == 0) b.leftSegment = Segments.Count - 1;
            else b.leftSegment = i - 1;

        }

        //It's line renderer setup time!
        if (enableLineRenderer && !line) line = gameObject.GetComponent<LineRenderer>();
        if (enableLineRenderer && line)
        {
            line.positionCount = Segments.Count;
            updateLine();
        }

        //Now let's do the edge colliders, if needed
        if (collisionType == collisionTypes.Edge)
        {
            //If we are using edge colliders, we'll start by adding one between segment0 and the segmentN
            EdgeCollider2D e = Segments[0].gameObject.AddComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
            e.points[0] = Segments[0].gameObject.transform.position;
            e.points[1] = Segments[Segments.Count - 1].gameObject.transform.position;
            edgeColliders.Add(e);

            //Then we'll loop through all segments
            for (int i = 1; i < Segments.Count; i++)
            {
                n = Segments[i].gameObject;
                e = n.AddComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
                e.points[0] = Segments[i - 1].gameObject.transform.position;
                e.points[1] = n.transform.position;
                edgeColliders.Add(e);
            }
        }

        //Now we will go through and add the Nth segments
        if (NthSegments.Count > 0)
        {
            foreach (var e in NthSegments)
            {
                int nth = e.n;
                if (nth == 0) continue; //This is no place for zeros!

                if (e.divide)
                {
                    if (Segments.Count % nth == 0)
                        nth = (Segments.Count) / nth;
                    else
                        nth = ((Segments.Count) / nth) + 1;
                }

                for (int i = e.offset; i < Segments.Count - 1; i += nth)
                {
                    var newseg = Instantiate(e.Segment);
                    newseg.transform.position = Segments[i].gameObject.transform.position;
                    newseg.transform.rotation = Segments[i].gameObject.transform.rotation;

                    if (e.replaceExistingSegment)
                    {
                        newseg.name = Segments[i].gameObject.name;
                        newseg.transform.parent = Segments[i].gameObject.transform.parent;
                        Destroy(Segments[i].gameObject);
                        Segments[i].gameObject = newseg;
                        Segments[i].rb = newseg.GetComponent<Rigidbody2D>();
                        Segments[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        newseg.transform.parent = Segments[i].gameObject.transform;
                    }

                    if (!drawSegmentSprites) Segments[i].gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    e.objectList.Add(newseg);
                    newseg.SetActive(e.active);
                }
            }
        }

        //place caps at the ends of the belt
        if (beltCaps && !pathCreator.path.isClosedLoop)
        {
            GameObject endcap = GameObject.Instantiate(beltCaps, transform);
            endcap.transform.position = Segments[Segments.Count - 1].gameObject.transform.position;
            endcap.transform.rotation = Segments[Segments.Count - 1].gameObject.transform.rotation;
            endcap.SetActive(true);

            GameObject startcap = GameObject.Instantiate(beltCaps, transform);
            startcap.transform.position = Segments[0].gameObject.transform.position;
            startcap.transform.rotation = Segments[0].gameObject.transform.rotation;
            startcap.SetActive(true);
        }

        beltFullyInitialized = true;
        timer = segmentUpdateTime;
    }


    //This updates the line positions
    void updateLine()
    {
        for (int i = 0; i < Segments.Count; i++)
        {
            line.SetPosition(i, new Vector3(Segments[i].gameObject.transform.position.x, Segments[i].gameObject.transform.position.y));
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
        a = Angle(a);
        b = Angle(b);
        if (a > b) return Angle(a - b);
        if (b > a) return Angle(b - a);
        return Angle(a - b);
    }


    void FixedUpdate()
    {
        if (!updateSegments || !beltFullyInitialized || speed == 0) return;

        //Spin the rotors.
        if (rotorsOn && movementType!=movementTypes.stationary)
        {
            float motion = Vector3.Distance(Segments[1].gameObject.transform.position, Segments[1].prevPos);
            foreach (var r in Rotors) r.transform.Rotate(0f, 0f, (direction == directions.right ? -motion : motion) * Time.fixedDeltaTime * 1000 * rotorSpinMultiplier, Space.Self);
        }
		
		if (movementFunction==movementFunctions.Velocity)
		{
			foreach(var s in Segments)  s.rb.velocity = new Vector3(0f,0f,0f);
		}

        //Apply segment motion
        if (movementType == movementTypes.speed) movementSpeed();
        if (movementType == movementTypes.timedJump) movementTimedJump();

        if (movementType == movementTypes.lerp
            || movementType == movementTypes.lerpOnCommand
            || movementType == movementTypes.smoothdamp
            || movementType == movementTypes.smoothdampOnCommand) movementLerp();

        //Apply rotation to segments
        if (rotateSegments)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                setSegmentRotation(i);
            }
        }
    }

    //if segment's distance has exceeded the end of the belt (based on direction), then we return a new distance that has been looped. Otherwise we return -1
    //Buffer brings the detected ends of the path inward. It is subtracted from the lengh and added to the start.
    float getSegmentExceededPathLength(int i, float buffer=0f)
    {
        bool exceededPathLength = false;
        if (!pathCreator.path.isClosedLoop)
        {
            if (direction == directions.right && Segments[i].distance > pathCreator.path.length-buffer)
                exceededPathLength = true;
            if (direction == directions.left && Segments[i].distance < buffer)
                exceededPathLength = true;
        }
        if (exceededPathLength) return (direction == directions.right ? Segments[i].distance - pathCreator.path.length : pathCreator.path.length + Segments[i].distance);
        else return -1;

    }

    //Assigns the target rotation for specified segments then applies rotation based on settings. i = index of the segment.
    void setSegmentRotation(int i)
    {
        var s = Segments[i];
        s.targetRotation = getSegmentRotation(s);
		
		s.targetRotation.z += direction==directions.left ? -rotateSegmentInDirection : rotateSegmentInDirection;
		
        if (lerpSegmentRotation)
            s.gameObject.transform.rotation = Quaternion.RotateTowards(s.gameObject.transform.rotation, s.targetRotation, Time.fixedDeltaTime * segmentLerpRotSpeed);
        else
            s.gameObject.transform.rotation = s.targetRotation;
		

    }

    //Returns the quaternion rotation for the specified segment, based on segment distance, rotation type, and rotation offset
    Quaternion getSegmentRotation(Segment s)
    {
        if (!rotateSegments) return Quaternion.Euler(0f, 0f, segmentRotationOffset);

        Vector3 norm = pathCreator.path.GetNormal​AtDistance(s.distance);

        if (segmentRotationType == segmentRotationTypes.alwaysInvert)
        {
            norm *= -1f;
        }
        if (segmentRotationType == segmentRotationTypes.invertOnLeft)
        {
            if (direction == directions.left) norm *= -1f;
        }
        if (segmentRotationType == segmentRotationTypes.invertOnRight)
        {
            if (direction == directions.right) norm *= -1f;
        }

        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, norm);
        rotation *= Quaternion.Euler(0f, 0f, segmentRotationOffset);
        return rotation;
    }

    void movementSpeed()
    {
        for (int i = 0; i < Segments.Count; i++)
        {
            var s = Segments[i];
            s.prevPos = s.gameObject.transform.position;

            var spd = speed * 0.001f;
            s.distance += (direction == directions.right ? spd : -spd);

			move (s.rb, pathCreator.path.GetPointAtDistance(s.distance));

            float distance = getSegmentExceededPathLength(i);
            if (distance != -1) //exceeded path distance
            {
                s.gameObject.transform.position = pathCreator.path.GetPointAtDistance(distance);
                s.distance = distance;
				if (movementFunction==movementFunctions.Velocity) s.rb.velocity = new Vector3(0f,0f,0f);
            }
        }
    }

    private void advanceLerpTarget()
    {
        lerpSourceDistance = 0f;
        lerpTargetDistance = (movementDistance * SegmentLength) * (direction == directions.left ? -1 : 1);
        segmentZeroLerpTargetDistance = Segments[0].distance + lerpTargetDistance;
        lerpTarget = pathCreator.path.GetPointAtDistance(Segments[0].distance + lerpTargetDistance, EndOfPathInstruction.Loop);
    }

    //This function actually handles all smoothdamp and lerp movement types
    private void movementLerp()
    {
        if (speed == 0) return;
        lerpVelocity = speed * Time.fixedDeltaTime * 0.1f;

        if (pauseTimer)
        {
            rotorsOn = true; //Turn this on so the rotors spin.
            
            //First we lerp or smoothdamp segment zero
            float distanceMoved;
            Segment s = Segments[0];
            s.prevPos = s.gameObject.transform.position;
            float prevDistance = s.distance;

            if (movementType == movementTypes.lerp || movementType == movementTypes.lerpOnCommand)
                distanceMoved = Mathf.Lerp(lerpSourceDistance, lerpTargetDistance, lerpVelocity);
            else
                distanceMoved = Mathf.SmoothDamp(lerpSourceDistance, lerpTargetDistance, ref s.smoothRefVelocity, smoothDampTimer, Mathf.Infinity, Time.fixedDeltaTime);

            float difference = distanceMoved - lerpSourceDistance;
            s.distance += difference;
            lerpSourceDistance = distanceMoved;

            Vector3 newpos = pathCreator.path.GetPointAtDistance(s.distance, EndOfPathInstruction.Loop);
			move(s.rb, newpos);

            //Check if we are close to our destination
            if (Math.Abs(lerpTargetDistance - lerpSourceDistance) <= Math.Abs(finishLerpBuffer))
            {
                s.rb.MovePosition(lerpTarget);
                s.distance = segmentZeroLerpTargetDistance;
                pauseTimer = false;
            }

            //Then we move all of the other segments by the same amount that segment 0 moved.
            for (int i = 0; i < Segments.Count; i++)
            {
                s = Segments[i];

                if (i != 0) //We don't need this stuff for segment0 because we already moved it and set prevDistance.
                {
                    s.prevPos = s.gameObject.transform.position;
                    prevDistance = s.distance;
                    s.distance = Segments[0].distance + (i * SegmentLength * segmentDistanceMultiplier);
                    s.distance = s.distance % pathCreator.path.length;
                }

                //Time to actually move the segment.
                //If the loop is open then we have a wierd thing where the segments kind of float between the two ends of the path. So IF we're at the end of the path and it's an open path, move the segment with the transform to avoid floaty crap.
                if (Math.Abs(s.distance - prevDistance) >= pathCreator.path.length*0.6f && !pathCreator.path.isClosedLoop)
                {
					s.gameObject.transform.position = pathCreator.path.GetPointAtDistance(s.distance);
					if (movementFunction==movementFunctions.Velocity) s.rb.velocity = new Vector3(0f,0f,0f);
				}
                else
                {
                    //Advance the segment to it's new destination. We already moved segment 0, so we don't need to move it again.
                    if (i!=0) move(s.rb, pathCreator.path.GetPointAtDistance(s.distance, EndOfPathInstruction.Loop));
                }

            }
        }
        else
        {
            rotorsOn = false;

            //We don't want to count down the timer if we're one of the onCommand movement styles
            if (movementType != movementTypes.lerpOnCommand && movementType != movementTypes.smoothdampOnCommand)
            {
                timer -= Time.fixedDeltaTime;
                if (timer <= 0)
                {
                    timer = segmentUpdateTime;
                    pauseTimer = true;
                    advanceLerpTarget();
                }
            }
            else //We are in one of the onCommand modes...
            {
                if (advanceLeftBool) setDirectionLeft();
                if (advanceRightBool) setDirectionRight();
                if (advanceLeftBool || advanceRightBool)
                {
                    advanceLerpTarget();
                    pauseTimer = true;
                    advanceLeftBool = false;
                    advanceRightBool = false;
                }
            }
        }

    }


    private void movementTimedJump()
    {
        //Decrement the timer. All update code after this point will only run at the timed intervals specified by segmentUpdateTime
        if (!pauseTimer) timer -= Time.fixedDeltaTime;
        if (timer >= 0) return;

        timer = segmentUpdateTime;

        //Cycle through all segments. 
        for (int i = 0; i < Segments.Count; i++)
        {
            var s = Segments[i];

            if (i == 0)
            {
                s.distance += (direction == directions.right ? (SegmentLength * movementDistance) : -(SegmentLength * movementDistance));
            }
            else
            {
                s.distance = Segments[0].distance + (i * SegmentLength * segmentDistanceMultiplier);
                s.distance = s.distance % pathCreator.path.length;
            }

            s.prevPos = s.gameObject.transform.position;
			move(s.rb, pathCreator.path.GetPointAtDistance(s.distance));

            float distance = getSegmentExceededPathLength(i);
            if (distance != -1)
            {
                s.gameObject.transform.position = pathCreator.path.GetPointAtDistance(distance);
                s.distance = distance;
				if (movementFunction==movementFunctions.Velocity) s.rb.velocity = new Vector3(0f,0f,0f);
            }
        }
    }


    void Update()
    {

        //Update all edge colliders
        if (collisionType == collisionTypes.Edge)
        {
            edgeColliders[0].points[0] = Segments[0].gameObject.transform.position;
            edgeColliders[0].points[1] = Segments[Segments.Count - 1].gameObject.transform.position;

            for (int i = 1; i < Segments.Count; i++) //Cycle through all Segments. 
            {
                edgeColliders[i - 1].points[0] = Segments[i - 1].gameObject.transform.position;
                edgeColliders[i - 1].points[1] = Segments[i].gameObject.transform.position;
            }
        }

        if (enableLineRenderer && line) updateLine();

        if (stopBool) stop();


        //We've called the stop() method.
        if (goStationaryAtEndOfLerp && !isLerping())
        {
            setMovementType(6);
            goStationaryAtEndOfLerp = false;
        }

    }

    void OnValidate()
    {

        //Update the set active switch for each nth segment
        foreach (var e in NthSegments)
        {
            foreach (var s in e.objectList)
            {
                s.SetActive(e.active);
            }
        }

        if (RotorContainer)
        {
            RotorContainer.SetActive(rotorsActive);
        }
    }
	
	//This is called to move individual segments. It moves the specified object according to the movementFunction settings.
	private void move(Rigidbody2D rb, Vector3 pos)
	{
		if (movementFunction==movementFunctions.MovePosition) rb.MovePosition(pos);
		if (movementFunction==movementFunctions.Position) rb.gameObject.transform.position = pos;
		if (movementFunction==movementFunctions.Velocity) 
			rb.velocity = (pos-rb.gameObject.transform.position)/ Time.fixedDeltaTime;
	}

    //USER FUNCTIONS for controlling the belt externally.

    //Call this to change the speed. Speed must be positive.
    public void changeSpeed(float s)
    {
        if (s >= 0)
            speed = s;
    }

    //Direction control
    public void setDirectionLeft()
    {
        direction = directions.left;
    }
    public void setDirectionRight()
    {
        direction = directions.right;
    }
    public void changeDirection()
    {
        if (direction == directions.right) direction = directions.left;
        else direction = directions.right;
    }

    //Movement types
    public void setMovementType(int type)
    {
        if (type == 0) movementType = movementTypes.speed;
        if (type == 1) movementType = movementTypes.timedJump;
        if (type == 2) movementType = movementTypes.lerp;
        if (type == 3) movementType = movementTypes.lerpOnCommand;
        if (type == 4) movementType = movementTypes.smoothdamp;
        if (type == 5) movementType = movementTypes.smoothdampOnCommand;
        if (type == 6) movementType = movementTypes.stationary;
    }

    public int getMovementType()
    {
        if (movementType == movementTypes.speed) return 0;
        if (movementType == movementTypes.timedJump) return 1;
        if (movementType == movementTypes.lerp) return 2;
        if (movementType == movementTypes.lerpOnCommand) return 3;
        if (movementType == movementTypes.smoothdamp) return 4;
        if (movementType == movementTypes.smoothdampOnCommand) return 5;
        if (movementType == movementTypes.stationary) return 6;

        return -1;
    }

    //This returns true if the belt is in either a lerp or damp mode, AND the belt is currently in the middle of a lerp/damp motion.
    public bool isLerping()
    {
        if (movementType == movementTypes.lerp
        || movementType == movementTypes.lerpOnCommand
        || movementType == movementTypes.smoothdamp
        || movementType == movementTypes.smoothdampOnCommand)
        {
            if (pauseTimer) return true;
        }
        return false;
    }

    //This stops the belt by putting it into stationary mode. It also waits until a lerp is complete before switching to stationary.
    public void stop()
    {
        stopBool = false;
        if (movementType == movementTypes.lerp
        || movementType == movementTypes.lerpOnCommand
        || movementType == movementTypes.smoothdamp
        || movementType == movementTypes.smoothdampOnCommand)
        {
            goStationaryAtEndOfLerp = true;
        }
        else
        {
            setMovementType(6);
        }
    }

    //Manual lerping
    public void advanceLeft()
    {
        advanceLeftBool = true;
    }
    public void advanceRight()
    {
        advanceRightBool = true;
    }
}
