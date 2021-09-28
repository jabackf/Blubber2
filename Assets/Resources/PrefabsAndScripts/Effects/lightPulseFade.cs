using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class lightPulseFade : MonoBehaviour
{
    public Light2D light;
    public float rate = 3;
    public float amplitude = 35;

    private float defaultIntensity;
    private float frequency = 0f;

    // Start is called before the first frame update
    void Start()
    {
        defaultIntensity = light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        frequency += rate * Time.deltaTime;
        if (frequency > 360) frequency -= 360;
        float a = Mathf.Sin(frequency) * amplitude * Mathf.Deg2Rad;
        light.intensity = defaultIntensity + a - (a / 2);
    }
}
