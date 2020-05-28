using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//TODO Implement removeFromInstantiateLoadList and removeAllFromInstantiateLoadList
//Maybe we can improve instantiate on load system by distinguishing between objects that load on any next scene, and objects that load on specific next scene

public class MapSystem 
{
    public Global global;


    [System.Serializable]
    public enum transitions
    {
        DEFAULT, //Default specifies no change to the currently used transition
        none,
        fade
    }
    public transitions transition = transitions.none;
	private int transitionStage = 0; //Used to track the stage of different transitions
	private bool transitioning = false;
	private GameObject transitionObject;

    private GameObject player; //Stores the player object
    private string wrapSide = "none"; //Used to track the side we are wrapping from
    private sceneBoundary boundary;

    private Vector2 characterReposition;

    public string currentMap = "";
	
	private string nextMap = ""; //When we begin a transition, this variable stores the name of the next map we are going to

    public const char splitter = '_';

    public enum repositionTypes
    {
        none,       //Do not move the character
        wrap,       //Wrap if we're off the screen 
        warpTag,    //Use the specified warpTag
        characterJump   //Jump to the x,y location specified by CharacterJumpPos
    }

    repositionTypes repositionType = repositionTypes.none;

    private Vector2 CharacterJumpPos = new Vector2(0, 0);
    private string warpTag = "";
    private Vector2 wrapOffset = new Vector2(0f,0f); //This offset is added or subtracted to the characters new position during wrapping. Added if wrapping right to left, subtracted if wrapping left to right, ect
    private Vector2 wrapEdgeBuffer = new Vector2(0f, 0f); //This is a buffer used to shrink the edges of the screen when checking if a character is outside for wrapping. For example, the right side of the screen would be screenRight-wrapEdgeBuffer.x

    public List<GameObject> destroyOnLoadList = new List<GameObject>(); //Stores a list of objects to destroy on scene change. See destroyOnSceneChange();

	public class instantiateOnLoadObject
	{
		public GameObject go;
		public string resource;
		public float x,y;
        public string scene;    //If a scene name is provided, the object will be instantiated on that scene load. If no scene name is provided, the object will instantiate on whatever scene loads next.
        public bool loaded;

		public instantiateOnLoadObject(string resource, string scene="", float x=0, float y=0)
		{
            this.loaded = false;
			this.resource=resource;
            this.scene = scene;
			this.x=x;
			this.y=y;
		}
		
		public GameObject load()
		{
			this.go = UnityEngine.Object.Instantiate(Resources.Load(resource)) as GameObject;
			go.transform.position = new Vector3(this.x,this.y,0f);
            this.loaded = true;
            return this.go;
		}
	}

    public List<instantiateOnLoadObject> instantiateOnLoadList = new List<instantiateOnLoadObject>(); //Stores a list of objects to instantiate after the scene is fully loaded

	public MapSystem()
	{
		SceneManager.sceneLoaded += onSceneLoaded;
	}

    private string getNewMapString(string map)
    {
        string[] m = currentMap.Split(splitter);
        string transitionString = "";

        switch (map)
        {
            case "left":
                transitionString = m[0] + splitter + (Int32.Parse(m[1]) - 1).ToString() + splitter + m[2];
                break;

            case "right":
                transitionString = m[0] + splitter + (Int32.Parse(m[1]) + 1).ToString() + splitter + m[2];
                break;

            case "up":
                transitionString = m[0] + splitter + m[1] + splitter + (Int32.Parse(m[2]) + 1).ToString();
                break;

            case "down":
                transitionString = m[0] + splitter + m[1] + splitter + (Int32.Parse(m[2]) - 1).ToString();
                break;

            case "current":
                transitionString = currentMap;
                break;

            default:
                transitionString = map;
                break;
        }

        return transitionString;
    }

    //If a warptag is specified, the player will jump to the object with this tag on next scene load. The warpTag will then be cleared.
    public void setRepositionType(repositionTypes type)
    {
        this.repositionType = type;
    }

    public void setWrapOffset(Vector2 off)
    {
        this.wrapOffset = off;
    }
    public void setWrapEdgeBuffer(Vector2 buff)
    {
        this.wrapEdgeBuffer = buff;
    }

    //If a warptag is specified, the player will jump to the object with this tag on next scene load. The warpTag will then be cleared.
    public void setWarpTag(string warpTag)
    {
        this.warpTag = warpTag;
    }

    //This function sets the new position for the character after the scene loads and if a warp tag isn't used
    public void setPlayerPosition(Vector2 newPosition)
    {
        CharacterJumpPos = newPosition;
    }

    //map can either be a map name (in the format of mapname_xpos_ypos)
    //or one of the special keywords: left, right, up, down, current
	//Returns true on success, false if scene change was rejected
    public bool goTo(string map, transitions trans = transitions.DEFAULT)
    {
		if (transitioning) return false;
		
		
		if (trans!=transitions.DEFAULT) transition = trans;
        string transitionString = getNewMapString(map);
		nextMap = transitionString;
		
		GameObject p = GameObject.FindWithTag("Player") as GameObject;
		p.GetComponent<CharacterController2D>().sceneChangeStart(map);
		
        startTransition(transitionString);
		
		return true;
		
    }

    private void startTransition(string map)
    {
        GameObject player = GameObject.FindWithTag("Player");
        //if (player != null) player.SetActive(false);
		transitioning=true;
		
        switch (transition)
        {
            case transitions.none:
                sceneLoad(map);
				transitioning=false;
                break;
			case transitions.fade:
				transitionObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Effects/sceneFadeOut")) as GameObject;
				transitionStage = 0;
			break;
        }
    }
	
	//Called by the fade object when a fade in / out is complete
	public void fadeComplete()
	{
		if (transitionStage==1)
		{
			transitioning=false;
		}
		else if (transitionStage==0)
		{
			sceneLoad(nextMap);
			instantiateOnSceneLoaded("Prefabs/Effects/sceneFadeIn");
			transitionStage = 1;
		}
	}

    //This function adds to a list of objects that will be manually destroyed next time the scene is changed.
    //One thing this is used for is to carry object across scenes. DontDestroyOnLoad is applied to the held object
    //meaning if it's not manually destroyed on scene changes then it will persist in every scene. The object list is
    //cleared after every scene load
    public void destroyOnSceneChange(GameObject obj)
    {
        destroyOnLoadList.Add(obj);
    }

    //This is the function that destroys the objects on destroyOnLoadList then clears the list. Typically called when scene changes
    public void executeDestroyOnSceneChange()
    {
        if (destroyOnLoadList.Count>0)
        {
            foreach (GameObject obj in destroyOnLoadList)
            {
                if (obj) UnityEngine.Object.Destroy(obj);
            }
            destroyOnLoadList.Clear();
        }
    }

    //Removes an object from  the destroyLoadList (if it is on there) without destroying it.
    public void removeFromDestroyLoadList(GameObject obj)
    {
        destroyOnLoadList.Remove(obj);
    }
	
	
    //This function adds the specified object to a list of objects that will be instantiated on the load of the next scene
    public void instantiateOnSceneLoaded(string resource, string sceneName="", float x=0, float y=0)
    {
        instantiateOnLoadList.Add(new instantiateOnLoadObject(resource, sceneName, x, y));
    }

    //This callback is triggered when a scene has finished loading
    public void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        boundary = GameObject.FindWithTag("sceneBoundary").GetComponent<sceneBoundary>() as sceneBoundary;

        if (instantiateOnLoadList.Count>0)
        {
            foreach (instantiateOnLoadObject obj in instantiateOnLoadList)
            {
                if (obj.scene == "" || scene.name == obj.scene)
                {
                    obj.load();
                }
            }
            instantiateOnLoadList.RemoveAll(obj => obj.loaded==true); //Remove all of the objects that were loaded
        }

        if (player != null)
        {
            repositionCharacterOnLoad();
            player.GetComponent<CharacterController2D>().sceneChangeComplete();
        }
    }

    //Removes any objects matching the specified resource from instantiateOnLoadList. Does not destroy instantiated objects!
	public void removeAllFromInstantiateLoadList(string resource)
    {
        instantiateOnLoadList.RemoveAll(obj => obj.resource==resource);
    }
    public void clearInstantiateLoadList()
    {
        instantiateOnLoadList.Clear();
    }

    //This is the function that actually loads the new scene
    private void sceneLoad(string map)
    {
        player = GameObject.FindWithTag("Player");

        executeDestroyOnSceneChange();

        currentMap = map;
        
        boundary = GameObject.FindWithTag("sceneBoundary").GetComponent<sceneBoundary>() as sceneBoundary;

        characterReposition.x = player.transform.position.x;
        characterReposition.y = player.transform.position.y;
        wrapSide = "none";

        //If we're wrapping, let's check if the player is off screen before we load the new scene
        if (repositionType==repositionTypes.wrap)
        {
            wrapSide = boundary.boundaryCheck(player, wrapEdgeBuffer.x, wrapEdgeBuffer.y);
        }

        ///LOAD THE NEW SCENE
        SceneManager.LoadScene(map, LoadSceneMode.Single);
    }

    //Repositions the character according to current class settings. 
    public void repositionCharacterOnLoad()
    {
        if (repositionType == repositionTypes.wrap)
        {
            if (wrapSide == "right") characterReposition.x = boundary.getLeftX() + wrapOffset.x;
            if (wrapSide == "left") characterReposition.x = boundary.getRightX() - wrapOffset.x;
            if (wrapSide == "top") characterReposition.y = boundary.getBottomY() + wrapOffset.y;
            if (wrapSide == "bottom") characterReposition.y = boundary.getTopY() - wrapOffset.y;
        }
        if (repositionType == repositionTypes.characterJump)
        {
            characterReposition = CharacterJumpPos;
        }
        if (repositionType == repositionTypes.warpTag)
        {
            GameObject warpTo = GameObject.FindWithTag(warpTag);
            characterReposition.x = warpTo.transform.position.x;
            characterReposition.y = warpTo.transform.position.y;
        }

        player.transform.position = new Vector3(characterReposition.x, characterReposition.y, 0);
    }

    //Returns the name of the map (mapName portion of mapName_xpos_ypos). If empty string is passed, returns name of current map.
    public string getMapName(string map= "")
    {
        string[] m = map.Split(splitter);
        return m[0];
    }
}
