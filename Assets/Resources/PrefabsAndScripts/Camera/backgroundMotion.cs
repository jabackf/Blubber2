using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundMotion : MonoBehaviour
{
    public float xMultiplier=0.3f, yMultiplier=0.3f;
    public Camera m_MainCamera;
    private cameraFollowPlayer cameraFollow;

    private Vector3 camPrev, camCur;
    // Start is called before the first frame update
    void Start()
    {
        if (m_MainCamera==null)m_MainCamera = Camera.main;
        cameraFollow = m_MainCamera.GetComponent<cameraFollowPlayer>() as cameraFollowPlayer;
        camCur = camPrev = m_MainCamera.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        camCur = m_MainCamera.gameObject.transform.position;
        if (cameraFollow != null) camPrev = cameraFollow.getPreviousPosition();
        gameObject.transform.position += new Vector3( (camCur.x-camPrev.x)*xMultiplier, (camCur.y - camPrev.y) * yMultiplier, 0);
    }

    void LateUpdate()
    {
        //If the camera doesn't have a cameraFollowPlayer script, get the previous position ourselves. Note that this doesn't work if we're using cameraFollowPlayer because the cameraFollowPlayer script uses lateUpdate to move the camera.
        if (cameraFollow==null) camPrev = m_MainCamera.gameObject.transform.position;
    }
}
