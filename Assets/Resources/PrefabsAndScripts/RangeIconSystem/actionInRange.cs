using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Objects that inherit from this class will show an action icon when the character in range calls the setInRange function
//It disables the icon when character distance exceeds distanceRangeDisable

public class actionInRange : MonoBehaviour
{
    public bool rangeActive = true;

    public GameObject ActionIconPrefab; //A prefab object for the action icon
    public string rangeColliderTag = "RangeCollider";  //This is the tag that any range colliders should have
    public float iconXOffset = -0.2f;
    public float iconYOffset = 0.5f;

    //The requireParentTags are a list of tags required of the rangeCollider's parent in order to trigger this range object. Leave this list empty to not check tags at all.
    //This is primarily used to restrict the characters that can and cannot interact with range objects.
    private List<string> requireParentTags = new List<string>() { }; 
	
	//While the above is a list of items required for the character to interact with the range object, this is required only for the icon to be visible.
	//If the character is not on this list, then the icon will not be visible but InRange will still be triggered and functional.
	//Leave empty to ignore tags entirely.
	private List<string> requireParentTagsVisibleIcon = new List<string>() { "Player" }; 

    private GameObject ActionIcon;
    private actionIcon iScript;

    private GameObject characterObject; //The characterObject that is currently in range, or null if nothing is in range
    private bool range = false; //Set to true when an object is in range

    private bool isChild = false; //If the action icon is a child to the object or not.

    private Global global;
    bool justEnabled = true;

    // Start is called before the first frame update
    protected void Start()
    {
        global = GameObject.FindWithTag("global").GetComponent<Global>() as Global;

        ActionIcon = (GameObject)Instantiate(ActionIconPrefab);

        if (isChild)
        {
            ActionIcon.transform.parent = gameObject.transform;
            //ActionIcon.transform.localPosition = new Vector3(gameObject.transform.position.x + iconXOffset, gameObject.transform.position.y + iconYOffset, 0f);
        }
        else
        {
            ActionIcon.transform.parent = gameObject.transform.parent;
            //ActionIcon.transform.localPosition = new Vector3(iconXOffset, iconYOffset, 0f);
        }

        //ActionIcon.transform.parent = gameObject.transform.parent;
        //ActionIcon.transform.localPosition = new Vector3(iconXOffset, iconYOffset, 0f);
        //ActionIcon.transform.localPosition = new Vector3(gameObject.transform.position.x + iconXOffset, gameObject.transform.position.y + iconYOffset, 0f);
        iScript = ActionIcon.GetComponent(typeof(actionIcon)) as actionIcon;
    }

    // Update is called once per frame
    protected void Update()
    {

        if (ActionIcon != null)
        {
            if (isChild)
                ActionIcon.transform.localPosition = new Vector3(iconXOffset, iconYOffset, 0f);
            else
                ActionIcon.transform.localPosition = new Vector3(gameObject.transform.position.x + iconXOffset, gameObject.transform.position.y + iconYOffset, 0f);
        }
    }
    
    public void LateUpdate()
    {
        justEnabled = false;
    }


    //These functions make the action icon a child of the gameObject. This is mainly used to pack the icon with the gameobject for transfer to new scene.
    //When the action icon IS a child, it adopt's the parent's scale and rotation
    public void makeChild()
    {
        isChild = true;
        ActionIcon.transform.parent = gameObject.transform;
    }
    public void unChild()
    {
        isChild = false;
        ActionIcon.transform.parent = gameObject.transform.parent;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (justEnabled) return;
        if (other.gameObject.tag == "RangeCollider" && rangeActive)
        {
            bool goodToGo = true;
            if (requireParentTags.Count>0)
            {
                goodToGo = false;
                foreach(var t in requireParentTags)
                {
                    if (t == other.transform.parent.gameObject.tag) goodToGo = true;
                }
            }
            if (goodToGo)
            {
				bool visible = true;
				if (requireParentTagsVisibleIcon.Count>0)
				{
					visible = false;
					foreach(var t in requireParentTagsVisibleIcon)
					{
						if (t == other.transform.parent.gameObject.tag) visible = true;
					}
				}
                CharacterController2D cont = other.gameObject.transform.parent.GetComponent<CharacterController2D>() as CharacterController2D;
                if (cont != null)
                {
                    if (!cont.isCharacterDead())
                        setInRange(true, other.gameObject.transform.parent.gameObject,visible);
                }
                else
                    setInRange(true, other.gameObject.transform.parent.gameObject,visible);
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (justEnabled) return;
        if (other.gameObject.tag == "RangeCollider" && rangeActive)
        {
            setInRange(false, null);
        }
    }

    public void OnEnable()
    {
        //There was a glitch that occured when items go from inactive to active while colliding with the player. This was happening on opening the fridge and spawning an item.
        //Still not sure why it was happening, but I found that ignoring all triggers during the first frame of becoming active fixed it. That's what this variable does.
        justEnabled = true;
    }

    public void setRangeActive(bool rangeActive)
    {
        this.rangeActive = rangeActive;
        if (!rangeActive)
        {
            setInRange(false, null);
        }
    }
    public bool israngeActive()
    {
        return rangeActive;
    }
    public void setInRange(bool inRange, GameObject go, bool visible=true)
    {
        if (!iScript) iScript = ActionIcon.GetComponent<actionIcon>();
        if (iScript && visible) iScript.setVisible(inRange);
        range = inRange;

        if (characterObject != null)
        {
            CharacterController2D cont = characterObject.GetComponent<CharacterController2D>() as CharacterController2D;
            if (cont.getActionObjectInRange() == gameObject)
            {
                cont.setActionObjectInRange(null);
            }
        }

        characterObject = go;

        if (characterObject != null)
        {
            var cont = characterObject.GetComponent<CharacterController2D>();
            if (cont) cont.setActionObjectInRange(gameObject);
        }
    }

    void OnDestroy()
    {
        //Destroy(ActionIcon);
    }
}
