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
    
    //The variables above are set for general logic, and multiple variables can be true at a time.
    //The state variable is set to give the character one overall state for reference.
    public enum states {idle,walk,jump,doubleJump,crouch,pushing,carryTop,carryFront,throwing,climbing};
    public states state = states.idle;

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

    //A single dress item
    [System.Serializable]
    public class dress
    {
        public static int listSortingOrder = 0;
        public string name;
        public string resourceName; // A resource path to a sprite.
		
        public Vector2 offset;  //This helps in positioning the sprite
        private string layerName = "CharacterDress";
        private SpriteRenderer renderer;
        private int dressSortingOrder = 0;
		public bool inDressDirectory=true; //If true, we will look for the resource in the dress directory specified in the global object. If false, the path must be specified.
        [HideInInspector] public GameObject gameObject;

        public dress(string name, string resourcePath, Transform parent, bool inDressDirectory=true, string layerName = "CharacterDress", int m_dressSortingOrder = -1)
        {
			
			this.inDressDirectory = inDressDirectory;
            Initiate(name, resourcePath, parent, layerName, m_dressSortingOrder);
        }

        public void Initiate(string name, string resourcePath, Transform parent, string layerName = "CharacterDress", int m_dressSortingOrder = -1)
        {
            if (m_dressSortingOrder == -1)
            {
                m_dressSortingOrder = listSortingOrder;
                listSortingOrder++;
            }
			
            this.name = name;
            this.gameObject = new GameObject();
            this.gameObject.name = "dress_" + name;
            this.gameObject.tag = "CharacterDress";
            this.gameObject.transform.parent = parent;
            this.gameObject.transform.localPosition = new Vector3(this.offset.x, this.offset.y, 0f);
            this.gameObject.layer = LayerMask.NameToLayer(layerName);
            this.renderer = this.gameObject.AddComponent<SpriteRenderer>();
            this.renderer.sprite = Resources.Load((this.inDressDirectory ? GameObject.FindWithTag("global").GetComponent<Global>().dirCharacterDress : "") + resourcePath, typeof(Sprite)) as Sprite;
            this.renderer.sortingLayerName = layerName;
            this.renderer.sortingOrder = m_dressSortingOrder;
        }

        public void ShowHide(bool show)
        {
            this.renderer.enabled = show;
        }
    }


    [Space]
    [Header("Dresses")]

    [SerializeField] public List<dress> dressList = new List<dress>();  //A list of all dress items attached to this character

    //A multiDress is a dress item that can have multiple states for multiple sprites. A multiDress will have one state at a time.
    //For example, eyes might be added as a dress, and they can in multiple states (angry, sad, ect.)
    //A state is a name in dressList[dress].name
    public class multiDress
    {
        public string state = "";
        public string[] states;
        public List<dress> dressList;
        public multiDress(ref List<dress> list, string state, string[] states)
        {
            this.state = state;
            this.states = states;
            this.dressList = list;
            setEnabled();
        }
        public void changeState(string state)
        {
            this.state = state;
            setEnabled();
        }
        void setEnabled()
        {
            foreach (dress d in this.dressList)
            {
                foreach (string s in this.states)
                {
                    if (d.name == s)
                    {
                        d.ShowHide( s == this.state ? true : false);
                    }
                }
            }
        }
    }


    // Start is called before the first frame update
    public void Start()
    {
        //Setup any dresses added in the inspector
        foreach (dress d in dressList)
        {
            d.Initiate(d.name, d.resourceName, gameObject.transform);
        }

        SetupCharacter();

    }

    //Overload setup function for unique characters.
    public virtual void SetupCharacter()
    {
    }

    //Overload setup function for unique characters. Called each update
    public virtual void UpdateCharacter()
    {

    }

    public void Update()
    {
        UpdateCharacter();

        //Set speed for movement
        if (speedAnimatorFloat != "")
            animator.SetFloat(speedAnimatorFloat, Mathf.Abs(speed*speedMultiplier));

        if (speed == 0) state = states.idle;
        else state = states.walk;

        if (crouchAnimatorBool != "")
            animator.SetBool(crouchAnimatorBool, crouch);

        if (crouch) state = states.crouch;

        if (jumpAnimatorBool != "")
            animator.SetBool(jumpAnimatorBool, jump);

        if (jump) state = states.jump;

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

        if (doubleJump) state = states.doubleJump;

        if (pushingAnimatorBool != "")
            animator.SetBool(pushingAnimatorBool, pushing);

        if (pushing) state = states.pushing;

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

        if (carryFront) state = states.carryFront;
        if (carryTop) state = states.carryTop;

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

        if (throwing) state = states.throwing;

        if (climbingAnimatorFloat != "")
            animator.SetFloat(climbingAnimatorFloat, Mathf.Abs(climb *climbSpeedMultiplier) );
        if (climbingAnimatorBool != "")
            animator.SetBool(climbingAnimatorBool, isClimbing);

        if (isClimbing) state = states.climbing;
    }

    public void dressShowHide(string name, bool show)
    {
        dress d = dressList.Find(dress => dress.name == name);
        if (d != null) d.ShowHide(show);
    }

    public void FlipX(bool flip)
    {
        //The character controller will automatically flip sprites of child objects if necessary.
        //We do need to flip the offsets though.
        foreach(dress d in dressList)
        {
            d.gameObject.transform.localPosition = new Vector3(flip ? d.offset.x * -1 : d.offset.x, d.offset.y, 0f);
        }
    }
    public void FlipY(bool flip)
    {
        foreach (dress d in dressList)
        {
            d.gameObject.transform.localPosition = new Vector3(d.offset.x, flip ? d.offset.y * -1 : d.offset.y, 0f);
        }
    }

}
