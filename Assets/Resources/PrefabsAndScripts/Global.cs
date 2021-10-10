using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Global singleton - Contains all global game management information
//Find with global = GameObject.FindWithTag("global").GetComponent<Global>();

public class Global : MonoBehaviour
{

    public static Global Instance { get; private set; }

    public string startScene = "testing_100_100";
    public MapSystem map;
    [HideInInspector] public bool sceneChanging = false; //This is set to true/false by the mapsystem. As soon as a scene change is triggered and a transition starts, this is marked true. It is marked false at the start of the new scene

	//Some useful handles
    public AudioManager audio;
    public AudioSource audioEffectsSource, audioMusicSource;
	
	public cameraFollowPlayer camera_follow_player;
	public sceneSettings scene_settings;

    [Space]
    [Header("Directories")]
	//We want these directory variables to be the same for the whole project, meaning the same for all global objects. HideInspector helps ensure that the script is the only place we are defining these strings.
	[HideInInspector] public string dirResourceRoot = "Assets/Resources/";

    [System.NonSerialized] public string dirAnimations = "Animations/";
    [System.NonSerialized] public string dirBackgrounds = "BackgroundsAndTiles/";
    [System.NonSerialized] public string dirFonts = "Fonts/";
    [System.NonSerialized] public string dirMaterials = "Materials/";
	[System.NonSerialized] public string dirPhysicsMaterials = "PhysicsMaterials/";
	[System.NonSerialized] public string dirPrefabs = "PrefabsAndScripts/";
    [System.NonSerialized] public string dirPalettes = "Palettes/";
    [System.NonSerialized] public string dirSpawners = "PrefabsAndScripts/Spawners/";
    [System.NonSerialized] public string dirRenderer = "Renderer/";
	[System.NonSerialized] public string dirScenes = "Scenes/";
	[System.NonSerialized] public string dirSprites = "Sprites/";
    [System.NonSerialized] public string dirImages = "Sprites/Images/";
    [System.NonSerialized] public string dirIcons = "Sprites/Icons/";
	[System.NonSerialized] public string dirParticleSystems = "PrefabsAndScripts/Effects/";
	[System.NonSerialized] public string dirBlubberSprites = "Sprites/Characters/Blubber/";
	[System.NonSerialized] public string dirCharacterDress = "Sprites/Dress/";
	[System.NonSerialized] public string dirDialogSystem = "PrefabsAndScripts/MenusAndDialogs/Dialog/";
	[System.NonSerialized] public string dirRangeIconSystem = "PrefabsAndScripts/RangeIconSystem/";
	[System.NonSerialized] public string dirInputCapture = "InputCapture/";
	

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

        audio = new AudioManager();
        audio.EffectsSource = audioEffectsSource;
        audio.MusicSource = audioMusicSource;
        audio.Start(gameObject);
		
		scene_settings = GameObject.FindWithTag("SceneSettings").GetComponent<sceneSettings>();
		camera_follow_player = GameObject.FindWithTag("MainCamera").GetComponent<cameraFollowPlayer>();
    }

    private void Start()
    {
        OnValidate(); //Implicitly call to setup game scene
        map.goTo(startScene, MapSystem.transitions.none, false);
    }

    private void Update()
    {
        //Update any of our classes that are not inheriting from and need an update
        audio.Update(Time.deltaTime);
    }

    public bool isSceneChanging()
    {
        return sceneChanging;
    }

    //Called by map system when the scene is about to change
    public void onSceneChanging()
    {
        audio.onSceneChanging();
    }
	
	//Called by map system when the scene has finished changing.
	public void onSceneChanged()
	{
		scene_settings = GameObject.FindWithTag("SceneSettings").GetComponent<sceneSettings>();
		camera_follow_player = GameObject.FindWithTag("MainCamera").GetComponent<cameraFollowPlayer>();
	}
	
	//Returns the name of the map. If getFullName is false, we only return the mapname portion of mapname_xpos_ypos. Otherwise we return the whole string.
	public string getMapName(bool getFullName=false)
	{
		if (getFullName) return map.getCurrentMap();
		else return map.getMapName(map.getCurrentMap());
	}
	
	//The scene name is not the same as the map name. The map name is the actual file (mapnam_x_y). The scene name is stored in the sceneSettings object. It is a more end user friendly name for the scene.
	public string getSceneName()
	{
		if (scene_settings!=null) return scene_settings.getSceneName();
		else return "";
	}
	
	public GameObject getPlayer()
	{
		return GameObject.FindWithTag("Player");
	}
	public CharacterController2D getPlayerController()
	{
		GameObject p = getPlayer();
		if (p) return p.GetComponent<CharacterController2D>();
		return null;
	}
	
	public bool pausePlayer(bool paused=true)
	{
		GameObject p = GameObject.FindWithTag("Player");
		CharacterController2D cc2d;
		
		if (p!=null) cc2d = p.GetComponent<CharacterController2D>();
		else return false;
		
		if (cc2d!=null) cc2d.setPause(paused);
		else return false;
		
		return true;
	}

    //Called when a field in the editor is changed
    private void OnValidate()
    {

    }



}
