using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Global singleton - Contains all global game management information

public class Global : MonoBehaviour
{

    public static Global Instance { get; private set; }

    public string startScene = "testing_100_100";
    public MapSystem map;

    [System.Serializable]
    public class persistentObject
    {
        public GameObject gameObject;
        public string map = ""; //The map this persistent object is on.
        public bool thisMapOnly = false;    //Map strings have the format mapName_xpos_ypos. If thisMapOnly is set to true, the object's persistence ends when the mapName portion (excluding xpos_ypos) is changed. If false, the object will be persistent for the entire game.

        public persistentObject(GameObject go, string map, bool thisMapOnly)
        {

            this.gameObject = go;
            this.map = map;
            this.thisMapOnly = thisMapOnly;

            DontDestroyOnLoad(go);
        }
    }

    [SerializeField] public List<persistentObject> persistentObjects = new List<persistentObject>();

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

    //This function prepares the global object for a scene change by doing things like handling object persistence. Called by the map system when a new scene is loaded.
    public void sceneChange(string newMap)
    {
        string newMapName = map.getMapName(newMap);

        for (int i = persistentObjects.Count-1; i>=0; i--)
        {
            persistentObject o = persistentObjects[i];

            o.gameObject.SetActive(o.map == newMap ? true : false);

            if (o.thisMapOnly && (map.getMapName(o.map) != newMapName)) //We're on a different map now, and this object's persistence ends with a change in map
            {
                Destroy(o.gameObject);
                persistentObjects.RemoveAt(i);
            }
        }
    }

    //This function registers a new persistent object
    public void registerPersistentObject(GameObject go, string mapName, bool thisMapOnly)
    {
        persistentObjects.Add(new persistentObject(go, mapName, thisMapOnly));
    }

    public void unregisterPersistentObject(GameObject go)
    {
        persistentObjects.RemoveAll(x=>x.gameObject == go);
    }
}
