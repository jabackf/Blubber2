using System.IO;
using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

//Make sure to place this script in a folder called "Editor"
public class BlubberOneLevelImporter 
{
        //This array matches the gmx names to the corresponding unity assets.
        static string[,] obj = new string[,] {
        { "Brick", "staticBrick" },
        { "WoodBox", "staticWoodBox" },
        { "GreenBrick", "staticGreenBrick" },
        { "YellowPlat", "PlatformYellow" },
        { "Crystal", "standardCrystal" },
        { "SpikeUp", "SpikeUpObj" }, //!

        { "FadePlat", "blueFadePlatObj" },
        { "Door", "standardDoorObj" },//!

        { "Bounce", "staticBounce" }, //!
        { "ConvLeft", "ConveryorLeft" }, //!
        { "ConvRight", "ConveryorRight" }, //!
        { "FlyGuy", "standardFlyGuy" }, //!
        { "Killbot", "standardKillbot" }, //!


        { "PlayerOne", "PlayerSpawnObj" }
        };

    [MenuItem("MyTools/ImportGMX")]
    static void importgmx()
    {

        string path = EditorUtility.OpenFilePanel("", "Assets/Resources/BlubberOne/GMXLevelExport", "txt");
        if (path.Length != 0)
        {

            GameObject parent =new GameObject("GMXImport");

            string fileContents = File.ReadAllText(path);

            string[] data = fileContents.Split(',');

            foreach (var s in data)
            {
                if (s.Length > 0)
                {
                    string[] d = s.Split('_');
                    if (d.Length > 0)
                    {
                        Debug.Log("Looking for: " + d[0] + ", X: " + d[1] + ", Y: " + d[2]);

                        int objIndex = -1;

                        //Find object in our obj array
                        for (int i = 0; i<obj.GetLength(0); i++)
                        {
                            if (obj[i,0] == d[0])
                            {
                                objIndex = i;
                                break;
                            }
                        }

                        //If we found the object name in the obj list
                        if (objIndex>=0)
                        {
                            string assetName = obj[objIndex, 1];
                            string[] results = AssetDatabase.FindAssets(assetName);
                            if (results.Length > 0)
                            {
                                string assetPath = AssetDatabase.GUIDToAssetPath(results[0]);
                                if (assetPath.Length>0)
                                {
                                    Debug.Log("Found: " + assetPath);
                                    Vector3 pos = new Vector3(Convert.ToInt32(d[1]) / 24, -Convert.ToInt32(d[2]) / 24, 0f);
                                    GameObject go = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject, pos, Quaternion.identity);
                                    go.transform.parent = parent.transform;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
