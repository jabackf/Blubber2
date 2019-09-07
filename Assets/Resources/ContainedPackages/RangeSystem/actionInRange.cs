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

    private GameObject ActionIcon;
    private actionIcon iScript;

    private GameObject characterObject; //The characterObject that is currently in range, or null if nothing is in range
    private bool range = false; //Set to true when an object is in range

    // Start is called before the first frame update
    protected void Start()
    {
        ActionIcon = (GameObject)Instantiate(ActionIconPrefab);
        ActionIcon.transform.localPosition = new Vector3(gameObject.transform.position.x + iconXOffset, gameObject.transform.position.y + iconYOffset, 0f);
        iScript = ActionIcon.GetComponent(typeof(actionIcon)) as actionIcon;
    }

    // Update is called once per frame
    protected void Update()
    {
        ActionIcon.transform.localPosition = new Vector3(gameObject.transform.position.x + iconXOffset, gameObject.transform.position.y + iconYOffset, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "RangeCollider" && rangeActive)
        {
            setInRange(true, other.gameObject.transform.parent.gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "RangeCollider" && rangeActive)
        {
            setInRange(false, null);
        }
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
    public void setInRange(bool inRange, GameObject go)
    {
        iScript.setVisible(inRange);
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
            characterObject.GetComponent<CharacterController2D>().setActionObjectInRange(gameObject);
        }
    }
}
