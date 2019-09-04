using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Triggers animations for a 2d platformer character, also handles character dressings
 */

[RequireComponent(typeof(CharacterController2D))]

public class CharacterAnimation : MonoBehaviour
{

    public Animator animator;
    public CharacterController2D controller;
    public float climbSpeedMultiplier = 10f; //Speed multiplier for climb animation
    public float speedMultiplier = 1.5f; //Use to adjust the speed of the animation.

    //The following variables are mostly set automatically by the character controller, though a few are unset by this script. They give us info about the character's current state.
    [HideInInspector]  public float speed = 0f; //Controls the speed of walking animations. Set by the character controller
    [HideInInspector]  public bool jump = false; //Stays true for duration of jump, until landing. Set and unset by the character controller.
    [HideInInspector] public bool doubleJump = false; //Only true for a fixed duration. Jump will also be true. Set by character controller, unset by this charAnim script
    [HideInInspector] public bool crouch = false; //True for the full duration of crouch. Set and unset by the character controller.
    [HideInInspector] public bool pushing = false; //True for the full duration of push. Set and unset by the character controller.
    [HideInInspector] public bool pickingUp = false; //Only true for a fixed duration, initiated after the player initiates an item pickup. Set by character controller, unset by this charAnim script
    [HideInInspector] public bool carryTop = false; //Set to true for the full duration of carry action. Set and unset by the character controller
    [HideInInspector] public bool carryFront = false; //Ditto
    [HideInInspector] public bool throwing = false; //Set to true after an object is thrown. Set by the character controller, unset by this charAnim script
    [HideInInspector] public float climb = 0; //Set to true for the duration of the climb by the character controller.
    [HideInInspector] public bool isClimbing = false;
    

    public float doubleJumpDuration = 0.3f;  //The amount of time to play the double jump animation
    private float doubleJumpTimer = 0;

    public float pickingUpDuration = 0.2f;  //The amount of time to play the picking up animation
    private float pickingUpTimer = 0;

    public float throwingDuration = 0.2f;  //The amount of time to play the picking up animation
    private float throwingTimer = 0;

    [Space]
    [Header("Animator Parameters")]
    public string crouchAnimatorBool = "IsCrouching"; //The Animator's boolean flag to set for crouching animations. Leave empty for no animation change.
    public string jumpAnimatorBool = "IsJumping"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string doubleJumpAnimatorBool = "IsDoubleJumping"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string pushingAnimatorBool = "IsPushing"; //The Animator's boolean flag to set for jump animations. Leave empty for no animation change.
    public string speedAnimatorFloat = "Speed"; //The Animator's float to set for walking animations. Leave empty for no animation change.
    public string pickupAnimatorBool = "IsPickingUp"; //Character is picking something up
    public string throwingAnimatorBool = "IsThrowing"; //Releasing the throw button down.
    public string frontCarryAnimatorBool = "IsCarryingFront"; //Releasing the throw button down.
    public string topCarryAnimatorBool = "IsCarryingTop"; //Releasing the throw button down.
    public string climbingAnimatorFloat = "ClimbSpeed"; //Climbing AnimSpeed
    public string climbingAnimatorBool = "IsClimbing"; //Climbing

    [Space]
    [Header("Dresses")]
    public string pushingDressName = "angry"; //The name of a dress to activate when we are pushing. Leave blank for none.

    public class dress
    {
        public static int listSortingOrder = 0;

        public string resourceName; // A resource path to a sprite.
        public string name;
        public string layerName = "CharacterDress";
        public SpriteRenderer renderer;
        public int dressSortingOrder = 0;
        public GameObject gameObject;

        public dress(string name, string resourcePath, Transform parent, string layerName = "CharacterDress", int m_dressSortingOrder = -1)
        {
            if (m_dressSortingOrder == -1)
            {
                m_dressSortingOrder = listSortingOrder;
                listSortingOrder++;
            }
            this.name = name;
            this.gameObject = new GameObject();
            this.gameObject.name = "dress_"+name;
            this.gameObject.transform.parent = parent;
            this.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            this.gameObject.layer = LayerMask.NameToLayer(layerName);
            this.renderer = this.gameObject.AddComponent<SpriteRenderer>();
            this.renderer.sprite = Resources.Load(resourcePath, typeof(Sprite)) as Sprite;
            this.renderer.sortingLayerName = layerName;
            this.renderer.sortingOrder = m_dressSortingOrder;
        }
    }

    public List<dress> dressList = new List<dress>();

    // Start is called before the first frame update
    void Start()
    {
        dressList.Add(new dress("angry", "Art/RippedFromGMXPrototype/sprDressAngry_0", gameObject.transform));
        dressShowHide("angry", false);
    }

    void Update()
    {
        //Set speed for movement
        if (speedAnimatorFloat != "")
            animator.SetFloat(speedAnimatorFloat, Mathf.Abs(speed*speedMultiplier));

        if (crouchAnimatorBool != "")
            animator.SetBool(crouchAnimatorBool, crouch);


        if (jumpAnimatorBool != "")
            animator.SetBool(jumpAnimatorBool, jump);

        if (doubleJumpTimer > 0)
        {
            doubleJumpTimer -= Time.deltaTime;
            if (doubleJumpTimer <= 0)
            {
                doubleJump = false;
                doubleJumpTimer = 0;
            }
        }
        if (doubleJumpTimer == 0 && doubleJump) doubleJumpTimer = doubleJumpDuration;
        if (!doubleJump) doubleJumpTimer = 0;
        if (doubleJumpAnimatorBool != "")
            animator.SetBool(doubleJumpAnimatorBool, doubleJump);

        if (pushingAnimatorBool != "")
            animator.SetBool(pushingAnimatorBool, pushing);
        if (pushingDressName != "")
            dressShowHide(pushingDressName, pushing);

        if (pickingUpTimer > 0)
        {
            pickingUpTimer -= Time.deltaTime;
            if (pickingUpTimer <= 0)
            {
                pickingUp = false;
                pickingUpTimer = 0;
            }
        }
        if (pickingUpTimer == 0 && pickingUp) pickingUpTimer = pickingUpDuration;
        if (!pickingUp) pickingUpTimer = 0;
        if (pickupAnimatorBool != "")
            animator.SetBool(pickupAnimatorBool, pickingUp);

        if (frontCarryAnimatorBool != "")
            animator.SetBool(frontCarryAnimatorBool, carryFront);
        if (topCarryAnimatorBool != "")
            animator.SetBool(topCarryAnimatorBool, carryTop);

        if (throwingTimer > 0)
        {
            throwingTimer -= Time.deltaTime;
            if (throwingTimer <= 0)
            {
                throwing = false;
                throwingTimer = 0;
            }
        }
        if (throwingTimer == 0 && throwing) throwingTimer = throwingDuration;
        if (!throwing) throwingTimer = 0;
        if (throwingAnimatorBool != "")
            animator.SetBool(throwingAnimatorBool, throwing);

        if (climbingAnimatorFloat != "")
            animator.SetFloat(climbingAnimatorFloat, Mathf.Abs(climb *climbSpeedMultiplier) );
        if (climbingAnimatorBool != "")
            animator.SetBool(climbingAnimatorBool, isClimbing);
    }

    public void dressShowHide(string name, bool show)
    {
        dress d = dressList.Find(dress => dress.name == name);
        if (d != null) d.renderer.enabled = show;
    }

}
