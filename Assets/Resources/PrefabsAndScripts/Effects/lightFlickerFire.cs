using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class lightFlickerFire : MonoBehaviour
{
    public Light2D light;
    public float maxFlickerInterval = 0.12f;
    public float minFlickerInterval = 0.05f;
    public float maxIntensityChange = 0.3f;

    private float defaultIntensity, timer=0f;

    // Start is called before the first frame update
    void Start()
    {
        defaultIntensity = light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer<=0f)
        {
            light.intensity = defaultIntensity + Random.Range(-maxIntensityChange, maxIntensityChange);
            timer = Random.Range(minFlickerInterval, maxFlickerInterval);
        }
    }
}
