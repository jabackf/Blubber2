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

    public AudioManager audio;
    public AudioSource audioEffectsSource, audioMusicSource;

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
    }

    private void Start()
    {
        OnValidate(); //Implicitly call to setup game scene
        map.goTo(startScene, MapSystem.transitions.none, false);
    }

    public bool isSceneChanging()
    {
        return sceneChanging;
    }

    //Called when a field in the editor is changed
    private void OnValidate()
    {

    }



}
