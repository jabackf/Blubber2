using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundMotion : MonoBehaviour
{
    public float xMultiplier=0.3f, yMultiplier=0.3f;
    public bool requireTagForMotion = true; //If true, the object with requireTag must exist or motion won't happen.
    public string requireTag = "Player";
    public Camera m_MainCamera;
    private GameObject requireGO; //This is the gameobject 

    private Vector3 camCur, camInitial, bgInitial;

    // Start is called before the first frame update
    void Start()
    {
        if (m_MainCamera==null)m_MainCamera = Camera.main;
        camInitial = camCur = m_MainCamera.gameObject.transform.position;
        bgInitial = gameObject.transform.position;
        checkForRequiredObject();
    }

    bool checkForRequiredObject()
    {
        requireGO = GameObject.FindWithTag(requireTag);
        if (requireGO==null) return false;
        else return true;
    }

    // Update is called once per frame
    void Update()
    {
        bool canMove = true;
        if (requireGO == null)
        {
            canMove = checkForRequiredObject();
        }

        if (canMove)
        {
            camCur = m_MainCamera.gameObject.transform.position;
            gameObject.transform.position = bgInitial + new Vector3((camCur.x - camInitial.x) * xMultiplier, (camCur.y - camInitial.y) * yMultiplier, 0);
        }
        
    }

}

/*
 * Original script. Had a bug where the background would go out of place if the camera jumped
 * 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundMotion : MonoBehaviour
{
    public float xMultiplier = 0.3f, yMultiplier = 0.3f;
    public bool requireTagForMotion = true; //If true, the object with requireTag must exist or motion won't happen.
    public string requireTag = "Player";
    public Camera m_MainCamera;
    private cameraFollowPlayer cameraFollow;
    private GameObject requireGO; //This is the gameobject 

    private Vector3 camPrev, camCur;
    // Start is called before the first frame update
    void Start()
    {
        if (m_MainCamera == null) m_MainCamera = Camera.main;
        cameraFollow = m_MainCamera.GetComponent<cameraFollowPlayer>() as cameraFollowPlayer;
        camCur = camPrev = m_MainCamera.gameObject.transform.position;
        checkForRequiredObject();
    }

    bool checkForRequiredObject()
    {
        requireGO = GameObject.FindWithTag(requireTag);
        if (requireGO == null) return false;
        else return true;
    }

    // Update is called once per frame
    void Update()
    {
        bool canMove = true;
        if (requireGO == null)
        {
            canMove = checkForRequiredObject();
        }

        if (canMove)
        {
            camCur = m_MainCamera.gameObject.transform.position;
            if (cameraFollow != null) camPrev = cameraFollow.getPreviousPosition();
            gameObject.transform.position += new Vector3((camCur.x - camPrev.x) * xMultiplier, (camCur.y - camPrev.y) * yMultiplier, 0);
        }

    }

    void LateUpdate()
    {
        //If the camera doesn't have a cameraFollowPlayer script, get the previous position ourselves. Note that this doesn't work if we're using cameraFollowPlayer because the cameraFollowPlayer script uses lateUpdate to move the camera.
        if (cameraFollow == null) camPrev = m_MainCamera.gameObject.transform.position;
    }
}
*/