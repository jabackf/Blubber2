using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Objects that inherit from this class will show an action icon when the character in range calls the setInRange function
//It disables the icon when character distance exceeds distanceRangeDisable

public class actionInRange : MonoBehaviour
{
    public string AIPrefabResource = "Prefabs/ActionIcons/ActionIcon_Carry"; //A prefab object for the action icon
    public float iconXOffset = -0.2f;
    public float iconYOffset = 0.5f;
    public float distanceRangeDisable = 1.4f; //The amount of distance the character must be when inRange is disabled

    private GameObject ActionIcon;
    private actionIcon iScript;

    private GameObject characterObject; //The characterObject that is currently (or was last) in range
    private bool range = false; //Set to true when an object is in range

    // Start is called before the first frame update
    void Start()
    {
        ActionIcon = (GameObject)Instantiate(Resources.Load(AIPrefabResource));
        ActionIcon.transform.localPosition = new Vector3(gameObject.transform.position.x + iconXOffset, gameObject.transform.position.y + iconYOffset, 0f);
        iScript = ActionIcon.GetComponent(typeof(actionIcon)) as actionIcon;
    }

    // Update is called once per frame
    void Update()
    {
        ActionIcon.transform.localPosition = new Vector3(gameObject.transform.position.x + iconXOffset, gameObject.transform.position.y + iconYOffset, 0f);
    }

    void LateUpdate()
    {
        if (range == true)
        {
            if (Vector2.Distance(characterObject.transform.position, gameObject.transform.position) > distanceRangeDisable)
            {
                setInRange(false, null);
            }
        }
    }

    public void setInRange(bool inRange, GameObject go)
    {
        iScript.setVisible(inRange);
        range = inRange;
        characterObject = go;
    }
}
