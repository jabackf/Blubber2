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

    private Vector2 worldCameraBottomLeft, worldCameraTopRight;

    private Vector3 cameraPreviousPosition;


    // Start is called before the first frame update
    void Start()
    {
        cameraPreviousPosition = gameObject.transform.position;
        Initialize();
    }

    bool Initialize()
    {
        GameObject player = GameObject.FindWithTag(followTag);
        camera = gameObject.GetComponent<Camera>();
        boundary = GameObject.FindWithTag(sceneBoundaryTag).GetComponent<sceneBoundary>() as sceneBoundary;
        if (!player) return false;
        playerT = player.GetComponent<Transform>() as Transform;
        charCont = GameObject.FindWithTag(followTag).GetComponent<CharacterController2D>() as CharacterController2D;

        //Snap the camera into place at the start
        if (playerT)
        {
            camera.transform.position = new Vector3(playerT.position.x, playerT.position.y, -10);
        }

        updateEdgeCoordinates();

        return true;
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

            playerPosition = new Vector3(playerT.position.x, playerT.position.y, playerT.position.z);

            bool facingRight = true;
            if (charCont != null) facingRight = charCont.isFacingRight();

            playerPosition = new Vector3(playerPosition.x + (facingRight ? offset : -offset), playerPosition.y, -10);

            camera.transform.position = Vector3.Lerp(camera.transform.position, playerPosition, offsetSmoothing * Time.deltaTime);
            updateEdgeCoordinates();

            //Correct for out of scene boundary
            if (boundary != null)
            {
                boundaryCorrection = new Vector3(0f, 0f, 0f);
                if (worldCameraTopRight.x > boundary.getRightX()) boundaryCorrection.x = (worldCameraTopRight.x - boundary.getRightX());
                if (worldCameraBottomLeft.x < boundary.getLeftX() ) boundaryCorrection.x = -(boundary.getLeftX()  - worldCameraBottomLeft.x);
                if (worldCameraTopRight.y > boundary.getTopY()) boundaryCorrection.y = (worldCameraTopRight.y- boundary.getTopY());
                if (worldCameraBottomLeft.y < boundary.getBottomY()) boundaryCorrection.y = -(boundary.getBottomY() - worldCameraBottomLeft.y);

                camera.transform.position -= boundaryCorrection;

                //camera.transform.position = Vector3.Lerp(camera.transform.position, camera.transform.position -= boundaryCorrection, 0.1f);
                updateEdgeCoordinates();
            }
        }
    }
}
