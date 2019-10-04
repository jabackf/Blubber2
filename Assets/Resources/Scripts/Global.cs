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

        //Used for changing location
        public bool warpX=false, warpY=false; //If true, the next time this object's map is loaded the object will switch to the other side of the screen
        public string warpTag =""; //If not empty, the object will warp to this point the next time the map is loaded.

        public persistentObject(GameObject go, string map, bool thisMapOnly, bool warpX=false, bool warpY=false, string warpTag="")
        {
            Debug.Log("Adding " + go.name);
            this.gameObject = go;
            this.map = map;
            this.thisMapOnly = thisMapOnly;
            this.warpX = warpX;
            this.warpY = warpY;
            this.warpTag = warpTag;

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

    //This function prepares the global object for a scene change by doing things like handling object persistence. Called by the map system immediately after a new map is loaded.
    public void sceneChange(string newMap)
    {
        string newMapName = map.getMapName(newMap);

        for (int i = persistentObjects.Count-1; i>=0; i--)
        {
            persistentObject o = persistentObjects[i];

            if (o.map == newMap)
            {
                o.gameObject.SetActive(true);
                if (o.warpTag != "")
                {
                    //Find object with this tag and move there
                    //o.gameObject.transform.position = o.warpPoint.position;
                }
                else
                {
                    Vector3 v = new Vector3(0f, 0f,0f);
                    if (o.warpX)
                    {
                        if (o.gameObject.transform.position.x >= Screen.width) v -= new Vector3(Screen.width, 0f, 0f);
                        else if (o.gameObject.transform.position.x <= 0) v += new Vector3(Screen.width, 0f, 0f);

                    }
                    if (o.warpY)
                    {
                        if (o.gameObject.transform.position.y >= Screen.height) v -= new Vector3(0f, Screen.height, 0f);
                        else if (o.gameObject.transform.position.y <= 0) v += new Vector3(0f, Screen.height, 0f);
                    }
                    o.gameObject.transform.position += v;
                }
                o.warpX = false;
                o.warpY = false;
                o.warpTag = "";
            }
            else
                o.gameObject.SetActive(false);

            if (o.thisMapOnly && (map.getMapName(o.map) != newMapName)) //We're on a different map now, and this object's persistence ends with a change in map
            {
                Destroy(o.gameObject);
                persistentObjects.RemoveAt(i);
            }
        }
    }

    //This function registers a new persistent object
    public void registerPersistentObject(GameObject go, string mapName, bool thisMapOnly, bool warpX=false, bool warpY=false, string warpTag="")
    {
        if (!persistentObjects.Exists(x => x.gameObject == go))
            persistentObjects.Add(new persistentObject(go, mapName, thisMapOnly));
    }

    public void unregisterPersistentObject(GameObject go)
    {
        persistentObjects.RemoveAll(x=>x.gameObject == go);
    }

}
