using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering.LWRP;

public class pushButton : MonoBehaviour
{
    [Space]
    [Header("Sprite & Behavior")]
    public Sprite buttonDown;
    public Sprite buttonUp;
    public float activeTimeWait = 1f; //Amount of time the button will stay active for. Set to -1 to wait indefinitely
    public float releaseForce = 10f; //Amount of force applied to any objects standing on top of the botton when it deactivates
    private float activeTimer = 0;
    private bool activated = false;
    private SpriteRenderer renderer;
    private bool justDeactivated = false;
    public AudioClip sndOnActivate, sndOnDeactivate;

    Global global;

    [Space]
    [Header("Button Light")]
    public Light2D light;
    public bool lightOn = true;
    public Color lightActiveColor = Color.green;
    public Color lightInactiveColor = Color.red;

    [Space]
    [Header("Callback Events")]
    public UnityEvent OnActivateEvent;
    public UnityEvent OnDeactivateEvent;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>() as SpriteRenderer;
        if (light==null) light = GetComponent<Light2D>() as Light2D;
        if (lightOn)
        {
            UpdateLight();
        }
        else
        {
            light.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (activeTimer > 0)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0) Deactivate();
        }
        
    }

    void UpdateLight()
    {
        if (isActivated())
        {
            light.color = lightActiveColor;
        }
        else
        {
            light.color = lightInactiveColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody!=null && !other.isTrigger && !activated && other.attachedRigidbody.velocity.y<0)
        {
            Activate();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (justDeactivated)
        {
            if (other.attachedRigidbody != null && !other.isTrigger && !activated)
            {
                //other.attachedRigidbody.AddForce(new Vector2(0f,releaseForce),ForceMode2D.Impulse);
                other.attachedRigidbody.velocity += new Vector2(0f, releaseForce);
                justDeactivated = false;
            }
        }
    }

    public void Activate()
    {
        if (!activated)
        {
            activated = true;
            renderer.sprite = buttonDown;
            if (sndOnActivate) global.audio.Play(sndOnActivate);
            if (activeTimeWait != -1) activeTimer = activeTimeWait;
            if (OnActivateEvent != null) OnActivateEvent.Invoke();
        }

        UpdateLight();
    }

    public void Deactivate()
    {
        if (activated)
        {
            activated = false;
            if (sndOnDeactivate) global.audio.Play(sndOnDeactivate);
            justDeactivated = true;
            renderer.sprite = buttonUp;
            if (activeTimeWait != -1) activeTimer = 0;
            if (OnDeactivateEvent != null) OnDeactivateEvent.Invoke();
        }

        UpdateLight();
    }

    public bool isActivated()
    {
        return activated;
    }
}
