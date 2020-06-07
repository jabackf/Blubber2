using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class lightIntensityFade : MonoBehaviour
{
    public Light2D light;
    public float fadeSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (light.intensity>0)
        {
            light.intensity -= Time.deltaTime * fadeSpeed;
            if (light.intensity < 0) light.intensity = 0;
        }
    }
}
