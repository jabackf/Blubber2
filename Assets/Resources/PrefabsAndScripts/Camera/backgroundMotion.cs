using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundMotion : MonoBehaviour
{
    //Am
    public float xMultiplier=0.3f, yMultiplier=0.3f;
    public Camera m_MainCamera;

    private Vector3 camPrev, camCur;
    // Start is called before the first frame update
    void Start()
    {
        if (m_MainCamera==null)m_MainCamera = Camera.main;
        camCur = camPrev = m_MainCamera.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        camCur = m_MainCamera.gameObject.transform.position;
        gameObject.transform.position += new Vector3( (camCur.x-camPrev.x)*xMultiplier, (camCur.y - camPrev.y) * yMultiplier, 0);
    }

    void LateUpdate()
    {
        camPrev = m_MainCamera.gameObject.transform.position;
    }
}
