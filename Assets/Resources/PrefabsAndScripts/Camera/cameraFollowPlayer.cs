using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollowPlayer : MonoBehaviour
{
    public string followTag = "Player"; //The tag of the object to follow
    public string sceneBoundaryTag = "sceneBoundary"; //OPTIONAL The tag of the sceneBoundary object to prevent the camera from moving outside of the scene boundaries
    public float offset=2;
    private sceneBoundary boundary;
    private Vector3 playerPosition;
    public float offsetSmoothing=0.5f;
    private Transform playerT;
    private Camera camera;  //The camera, assumed to be attached to the same object as this script
    private CharacterController2D charCont;
    private Vector3 boundaryCorrection = new Vector3(0, 0, 0); //Applied to the camera to prevent it from going off scene boundary

    //These transform points define the edges of the camera in world space
    public Transform cameraLeft, cameraRight, cameraTop, cameraBottom;

    // Start is called before the first frame update
    void Start()
    {
        playerT = GameObject.FindWithTag(followTag).GetComponent<Transform>() as Transform;
        charCont = GameObject.FindWithTag(followTag).GetComponent<CharacterController2D>() as CharacterController2D;
        camera = gameObject.GetComponent<Camera>();
        boundary = GameObject.FindWithTag(sceneBoundaryTag).GetComponent<sceneBoundary>() as sceneBoundary;

        //Snap the camera into place at the start
        if (playerT)
        {
            camera.transform.position = new Vector3(playerT.position.x, playerT.position.y,-10);
        }

    }

    //Checks a transform to determine if it is in the camera's view
    public bool insideView (Transform t, float bufferX=0f, float bufferY=0f)
    {
        float x=t.position.x, y=t.position.y;
        if (x >= cameraLeft.position.x + bufferX && x <= cameraRight.position.x - bufferX && y >= cameraBottom.position.y + bufferY && y <= cameraTop.position.y - bufferY)
            return true;
        else
            return false;
    }
    public bool insideView(Vector2 t, float bufferX = 0f, float bufferY = 0f)
    {
        float x = t.x, y = t.y;
        if (x >= cameraLeft.position.x + bufferX && x <= cameraRight.position.x - bufferX && y >= cameraBottom.position.y + bufferY && y <= cameraTop.position.y - bufferY)
            return true;
        else
            return false;
    }

    void FixedUpdate()
    {
        bool playerExists = true;
        if (!playerT)
        {
            GameObject player = GameObject.FindWithTag(followTag);
            if (!player)
                playerExists = false;
            else
                playerT = player.GetComponent<Transform>() as Transform;
        }

        if (playerExists)
        {
            playerPosition = new Vector3(playerT.position.x, playerT.position.y, playerT.position.z);

            bool facingRight = true;
            if (charCont != null) facingRight = charCont.isFacingRight();

            playerPosition = new Vector3(playerPosition.x + (facingRight ? offset : -offset), playerPosition.y, -10);

            //Actually move the camera
            camera.transform.position = Vector3.Lerp(camera.transform.position, playerPosition, offsetSmoothing * Time.deltaTime);

            //Correct for out of scene boundary
            if (boundary != null)
            {
                if (cameraRight.position.x > boundary.getRightX()) boundaryCorrection.x = (cameraRight.position.x - boundary.getRightX());
                if (cameraLeft.position.x < boundary.getLeftX()) boundaryCorrection.x = -(boundary.getLeftX() - cameraLeft.position.x);
                if (cameraTop.position.y > boundary.getTopY()) boundaryCorrection.y = (cameraTop.position.y - boundary.getTopY());
                if (cameraBottom.position.y < boundary.getBottomY()) boundaryCorrection.y = -(boundary.getBottomY() - cameraBottom.position.y);
                if (boundaryCorrection.x != 0) camera.transform.position = new Vector3(camera.transform.position.x - boundaryCorrection.x, camera.transform.position.y, camera.transform.position.z);
                if (boundaryCorrection.y != 0) camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y - boundaryCorrection.y, camera.transform.position.z);

            }
        }

    }
}
