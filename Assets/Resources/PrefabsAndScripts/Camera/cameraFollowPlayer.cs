using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollowPlayer : MonoBehaviour
{
    public string followTag = "Player"; //The tag of the object to follow
    public string sceneBoundaryTag = "sceneBoundary"; //OPTIONAL The tag of the sceneBoundary object to prevent the camera from moving outside of the scene boundaries
    public float offset = 2;
    private sceneBoundary boundary;
    private Vector3 playerPosition;
    public float offsetSmoothing = 0.5f;
    private Transform playerT;
    private Camera camera;  //The camera, assumed to be attached to the same object as this script
    private CharacterController2D charCont;
    private Vector3 boundaryCorrection = new Vector3(0, 0, 0); //Applied to the camera to prevent it from going off scene boundary

    public bool clamp = true; //If set to true, the camera will clamp to the scene boundary. Requires scene boundary object! If none exists, setting this to true does nothing!


    private Vector2 worldCameraBottomLeft, worldCameraTopRight;
    private Vector3 cameraPreviousPosition, clampMin, clampMax;

    //Camera dimensions in world units! Calculated in the calculateDimensions() function
    private float width, height, halfWidth, halfHeight;

    private bool firstUpdate = true;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        cameraPreviousPosition = gameObject.transform.position;

        //We're not getting the proper clamp positions when we call this in start. I think it's because our pixel perfect camera script is causing us to calulcate the wrong screen dimensions. This is my cheap and easy way of fixing it.
        Invoke("calculateClampPosition", 0.05f);
    }

    bool Initialize()
    {
        GameObject player = GameObject.FindWithTag(followTag);
        camera = gameObject.GetComponent<Camera>();
        boundary = GameObject.FindWithTag(sceneBoundaryTag).GetComponent<sceneBoundary>() as sceneBoundary;
        if (!player) return false;
        playerT = player.GetComponent<Transform>() as Transform;
        charCont = GameObject.FindWithTag(followTag).GetComponent<CharacterController2D>() as CharacterController2D;

        snapIntoPosition();

        //Determine clamping position for camera based on scene boundary
        calculateClampPosition();

        updateEdgeCoordinates();

        return true;
    }

    void calculateDimensions()
    {
        //Calculate some junk that might be useful
        height = camera.orthographicSize * 2.0f;
        width = height * camera.aspect;
        halfHeight = camera.orthographicSize;
        halfWidth = camera.aspect * halfHeight;
    }

    //This function immediately snaps the camera into place. It does not clamp!
    void snapIntoPosition()
    {
        if (playerT)
        {
            camera.transform.position = new Vector3(playerT.position.x, playerT.position.y, -10);
        }
    }

    //This function calculates the clamping position of the camera based on the scene boundaries
    void calculateClampPosition()
    {
        calculateDimensions();

        if (boundary)
        {
           

            clampMin = new Vector3(boundary.getLeftX() + halfWidth,
                                   boundary.getBottomY() + halfHeight, -10f);
            clampMax = new Vector3(boundary.getRightX() - halfWidth,
                       boundary.getTopY() - halfHeight, -10f);

            //Snap the camera into the boundary
            camera.transform.position = new Vector3(Mathf.Clamp(playerPosition.x, clampMin.x, clampMax.x), Mathf.Clamp(playerPosition.y, clampMin.y, clampMax.y), -10f);
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

        bool playerExists = true;
        if (!playerT)
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

            //To clamp or not to clamp!
            if (boundary != null && clamp==true)
            {
                //Actually move the camera
                camera.transform.position = Vector3.Lerp(camera.transform.position, 
                    new Vector3(Mathf.Clamp(playerPosition.x, clampMin.x, clampMax.x), Mathf.Clamp(playerPosition.y, clampMin.y, clampMax.y), -10f),
                    offsetSmoothing * Time.deltaTime);

            }
            else
            {
                //Actually move the camera
                camera.transform.position = Vector3.Lerp(camera.transform.position, playerPosition, offsetSmoothing * Time.deltaTime);
            }

            updateEdgeCoordinates();
        }
    }
}
