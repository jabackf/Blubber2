using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Global singleton - Contains all global game management information

public class Global : MonoBehaviour
{

    public static Global Instance { get; private set; }


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

    }
}
