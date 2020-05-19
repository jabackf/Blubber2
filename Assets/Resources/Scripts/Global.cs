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

    /*[System.Serializable]
    public class persistentObject
    {
        public GameObject gameObject;
        public string map = ""; //The map this persistent object is on.
        public bool thisMapSetOnly = false;    //Map strings have the format mapName_xpos_ypos. A map set is any set of maps that share the mapName portion of the string. If thisMapSetOnly is set to true, the object's persistence ends when the mapName portion (excluding xpos_ypos) is changed. If false, the object will be persistent for the entire game.
        public float id;

        //Used for changing location
        public bool wrapX = false, wrapY = false; //If true, the next time this object's map is loaded the object will switch to the other side of the screen
        public string warpTag =""; //If not empty, the object will warp to this point the next time the map is loaded.

        public persistentObject(GameObject go, string map, bool thisMapOnly, float id, bool wrapX = false, bool wrapY = false, string warpTag="")
        {
            Debug.Log("Adding " + go.name);
            this.gameObject = go;
            this.map = map;
            this.thisMapSetOnly = thisMapOnly;
            this.wrapX = wrapX;
            this.wrapY = wrapY;
            this.warpTag = warpTag;
            this.id = id; //A unique identifier for this object, generated based on the object's spawn position and spawn time.

       
            go.transform.parent = null;
            DontDestroyOnLoad(go);

        }
    }

    [SerializeField] public List<persistentObject> persistentObjects = new List<persistentObject>();*/

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

    /*//This function prepares the global object for a scene change by doing things like handling object persistence. Called by the map system immediately after a new map is loaded.
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
                    //Maybe also add the option of multiple tags. IE, "tag1, tag2, tag3" ect. Check for collisions at each tag and take the first open spot
                    //Or, maybe warp points that have rects can spaw objects randomly inside the rect
                }
                else if (o.wrapX || o.wrapY)
                {
                    Vector3 vpos = Camera.main.WorldToScreenPoint(o.gameObject.transform.position);

                    if (o.wrapX)
                    {
                        Debug.Log(vpos.x + ", "+ o.gameObject.transform.position.x+", "+ Camera.main.pixelWidth+", "+Screen.width);
                        if (vpos.x <= 0) vpos.x = Camera.main.pixelWidth;
                        else if (vpos.x >= Camera.main.pixelWidth) vpos.x = 0;
                    }
                    if (o.wrapY)
                    {
                        if (vpos.y <= 0) vpos.y = Camera.main.pixelHeight;
                        else if (vpos.y >= Camera.main.pixelHeight) vpos.y = 0;
                    }
                    o.gameObject.transform.position = Camera.main.ScreenToWorldPoint(vpos);
                }
                o.wrapX = false;
                o.wrapY = false;
                o.warpTag = "";
            }
            else
                o.gameObject.SetActive(false);

            if (o.thisMapSetOnly && (map.getMapName(o.map) != newMapName)) //We're on a different map now, and this object's persistence ends with a change in map
            {
                Destroy(o.gameObject);
                persistentObjects.RemoveAt(i);
            }
        }
    }

    //This function registers a new persistent object. Returns true on success, false if the object was not added because it has already been added.
    public bool registerPersistentObject(GameObject go, string mapName, bool thisMapOnly, float id, bool wrapX=false, bool wrapY=false, string warpTag="")
    {
        bool exists = persistentObjects.Any(x => (x.id == id && x.gameObject.name == go.name));
        Debug.Log("Registering " + go.name + "... GO: "+go+", id: "+id+", Exists: " + exists);
        if (!exists)
        {
            persistentObjects.Add(new persistentObject(go, mapName, thisMapOnly, id, wrapX, wrapY, warpTag));
            return true;
        }
        else
            return false;
    }

    public void unregisterPersistentObject(GameObject go, float id)
    {
        persistentObjects.RemoveAll(x => (x.id == id && x.gameObject.name == go.name));
    }*/

}
