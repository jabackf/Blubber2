using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Global singleton - Contains all global game management information

public class Global : MonoBehaviour
{

    public static Global Instance { get; private set; }

    [Header("Debug Options")]
    [Space]
    public bool showDebugLayer = false; //Toggles the debug layer on/off

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        OnValidate(); //Implicitly call to setup game scene
    }

    //Called when a field in the editor is changed
    private void OnValidate()
    {
        if (showDebugLayer)
            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("Debug");
        else
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Debug"));
    }
}
