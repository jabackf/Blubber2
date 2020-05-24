using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Global singleton - Contains all global game management information

public class Global : MonoBehaviour
{

    public static Global Instance { get; private set; }

    public string startScene = "testing_100_100";
    public MapSystem map;

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

        map = new MapSystem();
        map.global = this;
        map.currentMap = startScene;
        map.setRepositionType(MapSystem.repositionTypes.none);
    }

    private void Start()
    {
        OnValidate(); //Implicitly call to setup game scene
        map.goTo(startScene, MapSystem.transitions.none);
    }

    //Called when a field in the editor is changed
    private void OnValidate()
    {

    }

}
