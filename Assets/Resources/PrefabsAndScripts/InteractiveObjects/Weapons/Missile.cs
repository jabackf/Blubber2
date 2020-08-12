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

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        if (playerControlled)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal");
            verticalMove = Input.GetAxisRaw("Vertical") ;
            if (Input.GetButtonDown("UseItemAction")) Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (horizontalMove < 0 || verticalMove > 0) transform.eulerAngles += new Vector3(0f, 0f, rotateSpeed * Time.deltaTime);
        if (horizontalMove > 0 || verticalMove < 0) transform.eulerAngles -= new Vector3(0f, 0f, rotateSpeed * Time.deltaTime);
        rb.MovePosition(transform.position + (transform.right * Time.deltaTime * speed) );
    }

    //Activates player control. This function is called by any weapon with playerControlledProjectile and spawnProjectile set
    public void initiatePlayerControl(GameObject character)
    {

        pilot = character;
        playerControlled = true;
        gameObject.tag = "Player";
        pilot.tag = "inactivePlayer";
        Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
    }


    void OnDestroy()
    {
        if (playerControlled && pilot)
        {
            playerControlled = false;
            pilot.tag = "Player";
            Camera.main.SendMessage("findPlayer", SendMessageOptions.DontRequireReceiver);
        }
    }

}
