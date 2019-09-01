using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Animator animator;
    public CharacterController2D controller;
    float horizontalMove = 0f;
    float verticalMove = 0f;    //Used in aiming the throw retical
    bool throwRelease = false;
    public float runSpeed = 40f;
    public float aimSpeed = 40f; //Speed for aiming the throwing retical
    bool jump = false;
    bool crouch = false;
    bool pickup = false;
    bool holdingAction = false;

    public string crouchAnimatorBool = "IsCrouching"; //The Animator's boolean flag to set for crouching animations. Leave empty for no animation change.
    public string jumpAnimatorBool = "IsJumping"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string pushingAnimatorBool = "IsPushing"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string speedAnimatorFloat = "Speed"; //The Animator's float to set for walking animations. Leave empty for no animation change.
    public string pickupAnimatorBool = "isPickingUp"; //Character is picking something up
    public string prepThrowAnimatorBool = ""; //Holding the throw button down.
    public string ThrowingAnimatorBool = ""; //Releasing the throw button down.

    public string pushingDressSprite=""; // A resource path to a sprite. If set, this sprite gets loaded and added as a child to the player while pushing

    private GameObject pushingDressSpriteObj = null;
    private int dressSortingOrder = 0;
    private bool isThrowing = false; //Set to true if we're throwing an object (changing the throw angle and velocity).

    // Start is called before the first frame update
    void Start()
    {
        if (pushingDressSprite!="" && controller.pushEnabled())
        {
            pushingDressSpriteObj = addDress("pushingDress", pushingDressSprite);
        }
    }

    void Update()
    {

        if (controller.pickupEnabled())
        {
            if (Input.GetButtonDown("Pickup") )
            {
                if (!controller.holdingSomething())
                {
                    pickup = true;
                    if (pickupAnimatorBool != "")
                        animator.SetBool(pickupAnimatorBool, true);
                }
            }
            if (Input.GetButtonDown("Throw"))
            {
                if (controller.holdingSomething())
                {
                    isThrowing = true;
                    controller.Move(0, false, false, false); //Stop moving if we're walking
                    //Debug.Log("Throwing mode ACTIVE");
                    if (prepThrowAnimatorBool != "")
                        animator.SetBool(prepThrowAnimatorBool, true);
                }
            }
        }

        if (!isThrowing && controller.pickupEnabled())
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
            if (speedAnimatorFloat != "")
                animator.SetFloat(speedAnimatorFloat, Mathf.Abs(horizontalMove));

            if (Input.GetButtonDown("Jump"))
            {
                if (jump != true)
                {
                    jump = true;
                    if (jumpAnimatorBool != "")
                        animator.SetBool(jumpAnimatorBool, true);
                }
            }


            if (controller.crouchEnabled())
            {
                if (Input.GetButtonDown("Crouch"))
                {
                    crouch = true;
                }
                else if (Input.GetButtonUp("Crouch"))
                {
                    crouch = false;
                }

                if (crouchAnimatorBool != "")
                    animator.SetBool(crouchAnimatorBool, crouch);

                if (pushingAnimatorBool != "")
                    animator.SetBool(pushingAnimatorBool, controller.isPushingSomething());
            }
            if (pushingDressSpriteObj != null)
            {
                dressShowHide(pushingDressSpriteObj, controller.isPushingSomething());
            }
        }//end !isThrowing
        else
        {//We're throwing something
            if (!Input.GetButton("Throw"))
            {
                throwRelease = true;
                //Debug.Log("Throwing mode INACTIVE");
                if (ThrowingAnimatorBool != "")
                    animator.SetBool(ThrowingAnimatorBool, true);
            }

            horizontalMove = Input.GetAxisRaw("throwAimHorizontal") * aimSpeed;
            verticalMove = Input.GetAxisRaw("throwAimHorizontal") * aimSpeed;

            if (!Input.GetButtonDown("holdingAction"))
            {
                holdingAction = true;
            }
        }

    }

    public GameObject addDress(string name, string resourcePath, string layerName="CharacterDress", int m_dressSortingOrder=-1)
    {
        if (m_dressSortingOrder==-1)
        {
            m_dressSortingOrder = dressSortingOrder;
            dressSortingOrder++;
        }
        GameObject dress = new GameObject();
        dress.name = name;
        dress.transform.parent = gameObject.transform;
        dress.transform.localPosition = new Vector3(0f, 0f, 0f);
        dress.layer = LayerMask.NameToLayer(layerName);
        SpriteRenderer renderer = dress.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load(resourcePath, typeof(Sprite)) as Sprite;
        renderer.sortingLayerName = layerName;
        return dress;
    }
    public void dressShowHide(GameObject dress, bool show)
    {
        dress.GetComponent<SpriteRenderer>().enabled = show;
    }

    void FixedUpdate()
    {
        if (!isThrowing)
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, pickup);
        else
        {
            controller.Aim(horizontalMove * Time.fixedDeltaTime, verticalMove * Time.fixedDeltaTime, throwRelease, holdingAction);
            if (throwRelease) isThrowing = false;
        }
        jump = false;
        pickup = false;
        holdingAction = false;
        throwRelease = false;
    }

    public void OnLanding()
    {
        if (jumpAnimatorBool != "")
            animator.SetBool(jumpAnimatorBool, false);
    }
    public void OnCrouch()
    {

    }
}
