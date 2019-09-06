using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class pushButton : MonoBehaviour
{
    public Sprite buttonDown;
    public Sprite buttonUp;
    public float activeTimeWait = 1f; //Amount of time the button will stay active for. Set to -1 to wait indefinitely
    public float releaseForce = 10f; //Amount of force applied to any objects standing on top of the botton when it deactivates
    private float activeTimer = 0;
    private bool activated = false;
    private SpriteRenderer renderer;
    private bool justDeactivated = false;

    public UnityEvent OnActivateEvent;
    public UnityEvent OnDeactivateEvent;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>() as SpriteRenderer;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeTimer > 0)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0) Deactivate();
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
            if (activeTimeWait != -1) activeTimer = activeTimeWait;
            if (OnActivateEvent != null) OnActivateEvent.Invoke();
        }
    }

    public void Deactivate()
    {
        if (activated)
        {
            activated = false;
            justDeactivated = true;
            renderer.sprite = buttonUp;
            if (activeTimeWait != -1) activeTimer = 0;
            if (OnDeactivateEvent != null) OnDeactivateEvent.Invoke();
        }
    }

    public bool isActivated()
    {
        return activated;
    }
}
