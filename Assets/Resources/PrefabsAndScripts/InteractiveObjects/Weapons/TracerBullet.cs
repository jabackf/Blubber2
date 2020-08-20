using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the basis the more realistic style of instaneous bullet. It draws a line from the source until the bullet hits something or until it has surpassed the maximum distance
//Anything the bullet hits will recieve a "PShot(Vector3 position)" message where position is the coordinates where the bullet stopped.
//The source of the shot will be this object's x,y at Start(). The angle of fire will be this object's rotation at Start().

[RequireComponent(typeof(LineRenderer))]
public class TracerBullet : MonoBehaviour
{
    LineRenderer lineRenderer;
    public float maxDistance = 5f;
    public float fadeOutSpeed = 0.8f; //How fast the shot line fades out.
    public float sendMessageTimer = 0.03f; //How long to wait after hitting the object before we send the "Shot" message
    public float maxRandomizeAngle = 2f; //This value will control how much randomization with apply to the angle of the projectile
    GameObject hit; //Stores the object we hit, if any
    public float startAlpha = 1; //The opacity that the line starts with
    public float forceOnShotObject = 6f;

    public ContactFilter2D contactFilter;

    Color start, end;

    Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        start = lineRenderer.startColor;
        end = lineRenderer.endColor;

        transform.eulerAngles += new Vector3(0f,0f,Random.Range(-maxRandomizeAngle, maxRandomizeAngle));

        destination = transform.position + (transform.right * maxDistance);

        List<RaycastHit2D> hitInfo = new List<RaycastHit2D>();

        int count = Physics2D.Linecast(transform.position, destination, contactFilter, hitInfo);

        if (count>0)
        {
            //Find the closest one
            RaycastHit2D closest = hitInfo[0];
            foreach (var h in hitInfo)
            {
                if (Vector3.Distance(transform.position, h.point) < Vector3.Distance(transform.position, destination))
                {
                    destination = h.point;
                    closest = h;
                }
            }
            
            hit = closest.collider.gameObject;
            Invoke("sendTheMessage", sendMessageTimer);
        }

        if (forceOnShotObject!=0 && hit)
        {
            Rigidbody2D r = hit.GetComponent<Rigidbody2D>();
            if (r)
            {
                r.AddForceAtPosition(transform.right * forceOnShotObject, destination, ForceMode2D.Impulse);
            }
        }

        lineRenderer.SetPosition(1, destination);
    }

    void sendTheMessage()
    {
        if (!hit) return;
        hit.SendMessage("PShot", destination, SendMessageOptions.DontRequireReceiver);
    }

    // Update is called once per frame
    void Update()
    {
        //Found out the line
        startAlpha -= Time.deltaTime * fadeOutSpeed;
        start.a = startAlpha;
        end.a = startAlpha;
        lineRenderer.SetColors(start, end);

        if (startAlpha <= 0) Destroy(gameObject);
    }

}
