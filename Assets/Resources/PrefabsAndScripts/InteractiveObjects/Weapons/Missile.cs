using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    private bool playerControlled=false;
    private GameObject pilot;
    private Rigidbody2D rb;
    public float speed = 1f;
    public float rotateSpeed = 5f;

    private float horizontalMove, verticalMove;

    public bool mouseControl = true;
    private float mouseAngle=-1f;

    public GameObject explosion;

    Global global;
    public AudioClip sndOnPlayerControl;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        global = GameObject.FindWithTag("global").GetComponent<Global>();
    }

    // Update is called once per frame
    void Update()
    {

        if (playerControlled)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal");
            verticalMove = Input.GetAxisRaw("Vertical") ;
            if (Input.GetButtonDown("Throw") || (Input.GetButtonDown("UseItemAction")&&!mouseControl) ) Destroy(gameObject);
            if (Input.GetButton("UseItemAction") && mouseControl)
            {
                Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseAngle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * 180 / Mathf.PI;
                if (mouseAngle < 0) mouseAngle += 360;
            }
            else
            {
                mouseAngle = -1f;
            }
        }
    }

    void FixedUpdate()
    {
        if (horizontalMove < 0 || verticalMove > 0) transform.eulerAngles += new Vector3(0f, 0f, rotateSpeed * Time.fixedDeltaTime);
        if (horizontalMove > 0 || verticalMove < 0) transform.eulerAngles -= new Vector3(0f, 0f, rotateSpeed * Time.fixedDeltaTime);
        if (mouseControl && mouseAngle!=-1f)
        {
            transform.eulerAngles = new Vector3(0f,0f,Mathf.MoveTowardsAngle(transform.eulerAngles.z,mouseAngle, rotateSpeed * Time.fixedDeltaTime));
        }
        //rb.MovePosition(transform.position + (transform.right * Time.deltaTime * speed) );
        rb.velocity =  (transform.right * speed);
    }

    //Activates player control. This function is called by any weapon with playerControlledProjectile and spawnProjectile set
    public void initiatePlayerControl(GameObject character)
    {
        if (!global) global = GameObject.FindWithTag("global").GetComponent<Global>();
        if (sndOnPlayerControl) global.audio.Play(sndOnPlayerControl);
        pilot = character;
        playerControlled = true;
        gameObject.tag = "Player";
        pilot.tag = "inactivePlayer";
        Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
        pilot.SendMessage("onControlTaken", SendMessageOptions.DontRequireReceiver);
    }


    void OnDestroy()
    {
        if (playerControlled && pilot)
        {
            playerControlled = false;
            pilot.tag = "Player";
            Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
            pilot.SendMessage("onControlResumed", SendMessageOptions.DontRequireReceiver);
        }

        if (explosion) Instantiate(explosion, transform.position, Quaternion.identity);
    }

}
