using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoopScript : MonoBehaviour
{
	Global global;
	
    // Start is called before the first frame update
    void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
    }

    public void onPoopSplat()
	{
		if (global.getSceneName()=="Video Game History Museum")
		{
			var npcs = FindObjectsOfType<CPUInput>();
			foreach (var npc in npcs)
			{
				if (npc.name=="Eugene")
				{
					npc.trigger("PoopYeet");
				}
			}
		}
	}
}
