using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Animator animator;
    public CharacterController2D controller;
    float horizontalMove = 0f;
    public float runSpeed = 40f;
    bool jump = false;
    bool crouch = false;

    public string crouchAnimatorBool = "IsCrouching"; //The Animator's boolean flag to set for crouching animations. Leave empty for no animation change.
    public string jumpAnimatorBool = "IsJumping"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string pushingAnimatorBool = "IsPushing"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string speedAnimatorFloat = "Speed"; //The Animator's float to set for walking animations. Leave empty for no animation change.

    public string pushingDressSprite=""; // A resource path to a sprite. If set, this sprite gets loaded and added as a child to the player while pushing

    private GameObject pushingDressSpriteObj = null;
    private int dressSortingOrder = 0;

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
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        if (speedAnimatorFloat!="")
            animator.SetFloat(speedAnimatorFloat, Mathf.Abs(horizontalMove));
        if (Input.GetButtonDown("Jump"))
        {
            if (jump != true)
            {
                jump = true;
                if (jumpAnimatorBool!="")
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

            if (crouchAnimatorBool!="")
                animator.SetBool(crouchAnimatorBool, crouch);

            if (pushingAnimatorBool != "")
                animator.SetBool(pushingAnimatorBool, controller.isPushingSomething());
        }
        if (pushingDressSpriteObj != null)
        {
            dressShowHide(pushingDressSpriteObj,controller.isPushingSomething());
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
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false;
    }

    public void OnLanding()
    {
        if (jumpAnimatorBool != "")
            animator.SetBool(jumpAnimatorBool, false);
    }
    public void OnCrouch()
    {
        Debug.Log("CROUCH!");
    }
}
