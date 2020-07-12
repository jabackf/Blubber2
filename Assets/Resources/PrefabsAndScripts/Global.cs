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
	
    [Space]
    [Header("Directories")]
	//We want these directory variables to be the same for the whole project, meaning the same for all global objects. HideInspector helps ensure that the script is the only place we are defining these strings.
	[HideInInspector] public string dirResourceRoot = "Assets/Resources/";

	[HideInInspector] public string dirAnimations = "Animations/";
	[HideInInspector] public string dirBackgrounds = "BackgroundsAndTiles/";
	[HideInInspector] public string dirFonts = "Fonts/";
	[HideInInspector] public string dirMaterials = "Materials/";
	[HideInInspector] public string dirPhysicsMaterials = "PhysicsMaterials/";
	[HideInInspector] public string dirPrefabs = "PrefabsAndScripts/";
    [HideInInspector] public string dirSpawners = "PrefabsAndScripts/Spawners/";
    [HideInInspector] public string dirRenderer = "Renderer/";
	[HideInInspector] public string dirScenes = "Scenes/";
	[HideInInspector] public string dirSprites = "Sprites/";
    [HideInInspector] public string dirImages = "Sprites/Images/";
    [HideInInspector] public string dirIcons = "Sprites/Icons/";
	[HideInInspector] public string dirParticleSystems = "PrefabsAndScripts/Effects/";
	[HideInInspector] public string dirBlubberSprites = "Sprites/Characters/Blubber/";
	[HideInInspector] public string dirCharacterDress = "Sprites/Dress/";
	[HideInInspector] public string dirDialogSystem = "PrefabsAndScripts/Dialog/";
	[HideInInspector] public string dirRangeIconSystem = "PrefabsAndScripts/RangeIconSystem/";
	

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
        map.goTo(startScene, MapSystem.transitions.none, false);
    }

    //Called when a field in the editor is changed
    private void OnValidate()
    {

    }

}
