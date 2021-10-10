using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cameraFollowPlayer : MonoBehaviour
{
    public bool active = true; //When set to false everything in the script functions except for the actions that move the camera to follow the player

    public string followTag = "Player"; //The tag of the object to follow
	private bool focusTaken=false; //Focus can be taken off of the player and put elsewhere for cutscenes and stuff. Do this by calling takeFocus(Transform T) and returnFocus(). When that has happened, this is set to true.
    private Transform previousTransform; //Used to return focus after focus was taken.
	public string sceneBoundaryTag = "sceneBoundary"; //OPTIONAL The tag of the sceneBoundary object to prevent the camera from moving outside of the scene boundaries
    public float offset = 2;
    private sceneBoundary boundary;
    private Vector3 playerPosition;
    public float offsetSmoothing = 0.5f;
    private Transform playerT;
    private Camera camera;  //The camera, assumed to be attached to the same object as this script
    private CharacterController2D charCont;
    private Vector3 boundaryCorrection = new Vector3(0, 0, 0); //Applied to the camera to prevent it from going off scene boundary

    public Canvas blankScreen; //This can be used to make the entire view go blank. See blankOn() and blankOff()

    public bool clampX = true, clampY = true; //If set to true, the camera will clamp to the scene boundary. Requires scene boundary object! If none exists, setting this to true does nothing!


    private Vector2 worldCameraBottomLeft, worldCameraTopRight;
    private Vector3 cameraPreviousPosition, clampMin, clampMax;

    public bool drawClampBoundaries = false; //Use for debugging. Draws a line around the camera clamp boundaries
    private LineRenderer clampLine = null;

    //Camera dimensions in world units! Calculated in the calculateDimensions() function
    private float width, height, halfWidth, halfHeight;


    [Space]
    [Header("Camera Shake Effect")]

    // Desired duration of the shake effect
    private float shakeDuration = 0f;

    // A measure of magnitude for the shake. 
    private float shakeMagnitude = 0.7f;

    // A measure of how quickly the shake effect should evaporate
    private float dampingSpeed = 1.0f;

    private float defaultShakeMagnitude = 0.5f;
    private float defaultShakeDuration = 0.3f;


    void Start()
    {
        cameraPreviousPosition = gameObject.transform.position;

        if (drawClampBoundaries)
        {
            clampLine = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
            Invoke("setupClampLine", 0.2f);
        }

    }

    void setupClampLine()
    {
        //clampLine.material = new Material(Resources.Load("Materials/Flat"));
        clampLine.widthMultiplier = 0.08f;
        clampLine.positionCount = 4;
        clampLine.loop = true;
        clampLine.startColor = clampLine.endColor = Color.red;
        Vector3[] points = new Vector3[4];
        points[0].x = clampMin.x; points[0].y = clampMax.y;
        points[1].x = clampMax.x; points[1].y = clampMax.y;
        points[2].x = clampMax.x; points[2].y = clampMin.y;
        points[3].x = clampMin.x; points[3].y = clampMin.y;

        /*I used the following lines to test out the viewport of the camera.
         * points[0].x = transform.position.x - halfWidth; points[0].y = transform.position.y + halfHeight;
        points[1].x = transform.position.x + halfWidth; points[1].y = transform.position.y + halfHeight;
        points[2].x = transform.position.x + halfWidth; points[2].y = transform.position.y - halfHeight;
        points[3].x = transform.position.x - halfWidth; points[3].y = transform.position.y - halfHeight;*/

        points[0].z = points[1].z = points[2].z = points[3].z = 0f;
        clampLine.SetPositions(points);
        
    }

    bool Initialize()
    {
        getVariables();

        snapIntoPosition();

        updateEdgeCoordinates();

        //Determine clamping position for camera based on scene boundary
        //For some reason I can't call it immediately. I don't know why. I need to wait a split second or else sometimes screen width and height isn't properly calculated.
        Invoke("calculateClampPosition", 0.01f);

        if (playerT == null)
            return false;
        else
            return true;

    }

    public void findPlayer()
    {
        GameObject player = GameObject.FindWithTag(followTag);
        if (player)
        {
            playerT = player.GetComponent<Transform>() as Transform;
            charCont = player.GetComponent<CharacterController2D>() as CharacterController2D;
        }
        else
        {
            //Player doesn't exist.
            playerT = null;
        }
    }

    public void getVariables()
    {
        camera = gameObject.GetComponent<Camera>() as Camera;
        boundary = GameObject.FindWithTag(sceneBoundaryTag).GetComponent<sceneBoundary>() as sceneBoundary;
        findPlayer();
    }

    public void blankOn()
    {
        Debug.Log("ON");
        if (blankScreen != null) blankScreen.gameObject.SetActive(true);
    }
    public void blankOff()
    {
        Debug.Log("OFF");
        if (blankScreen != null) blankScreen.gameObject.SetActive(false);
    }
    public void blankToggle()
    {
        if (blankScreen != null) blankScreen.gameObject.SetActive(!blankScreen.gameObject.activeSelf);
    }

    void calculateDimensions()
    {
        //Calculate some junk that might be useful

        //var lowerLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        //var upperRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        var lowerLeft = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        var upperRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        height = upperRight.y - lowerLeft.y;
        width = upperRight.x - lowerLeft.x;
        halfHeight = height / 2;
        halfWidth = width / 2;

        /*Test out the calculated screen dimensions by drawing a line from lowerLeft to upperRight
         * GameObject vpline = new GameObject("vpline");
        LineRenderer dbl = vpline.AddComponent(typeof(LineRenderer)) as LineRenderer;
        dbl.widthMultiplier = 0.08f;
        dbl.positionCount = 4;
        dbl.loop = true;
        dbl.startColor = dbl.endColor = Color.green;
        Vector3[] points = new Vector3[4];
        points[0].x = lowerLeft.x; points[0].y = upperRight.y;
        points[1].x = upperRight.x; points[1].y = upperRight.y;
        points[2].x = upperRight.x; points[2].y = lowerLeft.y;
        points[3].x = lowerLeft.x; points[3].y = lowerLeft.y;
        points[0].z = points[1].z = points[2].z = points[3].z = 0f;
        dbl.SetPositions(points);*/
    }

    //This function immediately snaps the camera into place. It does not clamp!
    void snapIntoPosition()
    {
        if (!active) return;
        if (playerT)
        {
            camera.transform.position = new Vector3(playerT.position.x, playerT.position.y, -10);
        }
    }

    //This function calculates the clamping position of the camera based on the scene boundaries
    void calculateClampPosition()
    {
        
        calculateDimensions();

        if (boundary && playerT!=null)
        {

            playerPosition = new Vector3(playerT.position.x, playerT.position.y, -10f);

            clampMin = new Vector3(boundary.getLeftX() + halfWidth,
                                   boundary.getBottomY() + halfHeight, -10f);
            clampMax = new Vector3(boundary.getRightX() - halfWidth,
                                   boundary.getTopY() - halfHeight, -10f);

            //Snap the camera into the boundary
            if (active)
                camera.transform.position = new Vector3( (clampX ? Mathf.Clamp(playerPosition.x, clampMin.x, clampMax.x) : camera.transform.position.x), (clampY ? Mathf.Clamp(playerPosition.y, clampMin.y, clampMax.y) : camera.transform.position.y), -10f);
        }
        
    }

    //This returns the position of the camera immediately before the last update
    public Vector3 getPreviousPosition()
    {
        return cameraPreviousPosition;
    }

    //Checks a transform to determine if it's x and y are in the camera's view
    public bool insideView (Transform t, float bufferX=0f, float bufferY=0f)
    {
        float x=t.position.x, y=t.position.y;
        if (x >= (worldCameraBottomLeft.x + bufferX) && x <= (worldCameraTopRight.x - bufferX) && y >= (worldCameraBottomLeft.y + bufferY) && y <= (worldCameraTopRight.y - bufferY))
            return true;
        else
            return false;
    }
    public bool insideView(Vector2 t, float bufferX = 0f, float bufferY = 0f)
    {
        float x = t.x, y = t.y;
        if (x >= worldCameraBottomLeft.x + bufferX && x <= worldCameraTopRight.x - bufferX && y >= worldCameraBottomLeft.y + bufferY && y <= worldCameraTopRight.y - bufferY)
            return true;
        else
            return false;
    }

    public float getLeftViewX()
    {
        return worldCameraBottomLeft.x;
    }
    public float getRightViewX()
    {
        return worldCameraTopRight.x;
    }
    public float getTopViewY()
    {
        return worldCameraTopRight.y;
    }
    public float getBottomViewY()
    {
        return worldCameraBottomLeft.y;
    }

    void updateEdgeCoordinates()
    {
        //Update the camera edge coordinates
        worldCameraTopRight = (Vector2)Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        worldCameraBottomLeft = (Vector2)Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
    }

    void LateUpdate()
    {
        // This is used to apply a shaking effect additively
        Vector3 shakeOffset = new Vector3(0f,0f,0f);
        if (shakeDuration > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime * dampingSpeed;
        }


        bool playerExists = true;
        if (playerT == null)
        {
            playerExists = Initialize();
        }

        if (playerExists)
        {
            cameraPreviousPosition = camera.transform.position;

            playerPosition = new Vector3(playerT.position.x, playerT.position.y, -10f);


            bool facingRight = true;
            if (charCont != null) facingRight = charCont.isFacingRight();

            playerPosition = new Vector3(playerPosition.x + (facingRight ? offset : -offset), playerPosition.y, -10f);

            float smoothing = offsetSmoothing * Time.deltaTime;

            if (active)
            {

                camera.transform.position += shakeOffset;

                //To clamp or not to clamp!
                if (boundary != null && (clampX == true || clampY == true))
                {
                    //Actually move the camera
                    camera.transform.position = Vector3.Lerp(camera.transform.position,
                        new Vector3((clampX ? Mathf.Clamp(playerPosition.x, clampMin.x, clampMax.x) : playerPosition.x),
                        (clampY ? Mathf.Clamp(playerPosition.y, clampMin.y, clampMax.y) : playerPosition.y),
                        -10f), offsetSmoothing * Time.deltaTime);

                }
                else
                {
                    //Actually move the camera
                    camera.transform.position = Vector3.Lerp(camera.transform.position, playerPosition, offsetSmoothing * Time.deltaTime);
                }
            }

            updateEdgeCoordinates();
        }
       
    }
	
	public void takeFocus(Transform T)
	{
		if (focusTaken) return;
		if (T!=null)
		{
			previousTransform=playerT;
			playerT=T;
			focusTaken=true;
		}
	}
	
	public void returnFocus()
	{
		if (!focusTaken) return;
		focusTaken=false;
		playerT = previousTransform;
	}

    //Use these functions to trigger shakes of various magnitudes and durations
    public void TriggerShake() //This uses default settings
    {

        shakeDuration = defaultShakeDuration;
        shakeMagnitude = defaultShakeMagnitude;
    }
    public void TriggerShakeDuration(float duration)
    {
        shakeDuration = duration;
        shakeMagnitude = defaultShakeMagnitude;
    }
    public void TriggerShakeExt(float magnitude, float duration)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}
