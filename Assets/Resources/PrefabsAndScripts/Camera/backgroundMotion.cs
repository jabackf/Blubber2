using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class backgroundMotion : MonoBehaviour
{
    public float xMultiplier=0.3f, yMultiplier=0.3f;
    public bool requireTagForMotion = true; //If true, the object with requireTag must exist or motion won't happen.
    public string requireTag = "Player";
    public Camera m_MainCamera;
    private GameObject requireGO; //This is the gameobject 

    private Vector3 camCur, camInitial, bgInitial;

    // Start is called before the first frame update
    void Awake()
    {
        bgInitial = gameObject.transform.position;
        SceneManager.sceneLoaded += bgSceneLoaded;
    }

    void bgSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Initialize();

        SceneManager.sceneLoaded -= bgSceneLoaded;
    }

    void Initialize()
    {
        if (m_MainCamera == null) m_MainCamera = Camera.main;
        camInitial = camCur = m_MainCamera.gameObject.transform.position;

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
        if (!m_MainCamera) Initialize();

        bool canMove = true;
        if (requireGO == null)
        {
            canMove = checkForRequiredObject();
        }

        if (canMove && m_MainCamera!=null)
        {
            camCur = m_MainCamera.gameObject.transform.position;
            gameObject.transform.position = bgInitial + new Vector3((camCur.x - camInitial.x) * xMultiplier, (camCur.y - camInitial.y) * yMultiplier, 0);
        }
    }

}